"""
analyzer.py — Document ingestion, token guardrails, and Groq API orchestration.

Responsibilities:
  1. Read the client requirement document from disk (.txt / .md / .pdf).
  2. Estimate token usage and abort before sending a request that would
     blow the 8,000 TPM free-tier cap.
  3. Call the GroqCloud API with a hardcoded system prompt (maximising
     automatic prompt caching) and structured JSON output enforcement.
  4. Wrap the API call in a tenacity retry loop that handles 429 responses
     with exponential backoff.
  5. Parse, validate, and return the structured analysis dictionary.
"""

import json
import logging
from pathlib import Path
from typing import Any, Optional

import groq
from tenacity import (
    before_sleep_log,
    retry,
    retry_if_exception_type,
    stop_after_attempt,
    wait_exponential,
)

logger = logging.getLogger(__name__)


# ──────────────────────────────────────────────────────────────────────────────
# Constants
# ──────────────────────────────────────────────────────────────────────────────

MODEL_ID = "openai/gpt-oss-120b"

# Free-tier hard caps for this model on GroqCloud.
GROQ_FREE_TIER_TPM = 8_000        # Tokens Per Minute ceiling
GROQ_FREE_TIER_RPM = 30           # Requests Per Minute ceiling

# Maximum characters to accept from the input document before truncation.
# Budget breakdown per request:
#   ~600 tokens  — SYSTEM_PROMPT (static; cached on successive runs)
#   ~5,000 tokens — user document content (this limit)
#   ~2,400 tokens — reserved for the model's JSON response
#   ─────────────────────────────────────────────────────
#   ~8,000 tokens — stays within TPM cap for a single request
#
# Estimation: 1 token ≈ 4 characters (safe approximation for English prose).
MAX_DOCUMENT_TOKENS = 5_000
MAX_DOCUMENT_CHARS = MAX_DOCUMENT_TOKENS * 4   # = 20,000 characters


# ──────────────────────────────────────────────────────────────────────────────
# System prompt (HARDCODED — never interpolate user data into this string)
#
# Keeping the system prompt static across all invocations allows GroqCloud's
# automatic KV-cache to hit on every request after the first, dramatically
# reducing the effective input-token cost per minute and making it possible
# to stay within the 8,000 TPM cap even for moderate document sizes.
# ──────────────────────────────────────────────────────────────────────────────

SYSTEM_PROMPT = """You are a Principal Business Analyst and Solutions Architect with 15 years of enterprise experience. Your task is to analyse a client requirement document and extract all structured information it contains.

CRITICAL: You MUST respond with ONLY a single valid JSON object. Do not include markdown fences, prose explanations, or any text outside the JSON structure. Your entire response must be parseable by json.loads().

The JSON object must conform exactly to this schema — every key is required, even if its value is an empty array:

{
  "functional_requirements": [
    {
      "id": "FR-001",
      "title": "Short, action-oriented title (max 10 words)",
      "description": "Full description of what the system must do",
      "priority": "High | Medium | Low",
      "acceptance_criteria": "Specific, testable condition that proves this requirement is satisfied"
    }
  ],
  "non_functional_requirements": [
    {
      "id": "NFR-001",
      "category": "Performance | Security | Scalability | Reliability | Usability | Maintainability | Compliance",
      "description": "Full description of the quality constraint",
      "metric": "Quantitative target where applicable, e.g. '< 200ms p99 latency' or 'AES-256 encryption at rest'"
    }
  ],
  "risks": [
    {
      "id": "RISK-001",
      "description": "Clear statement of what could go wrong",
      "impact": "High | Medium | Low",
      "likelihood": "High | Medium | Low",
      "mitigation": "Concrete strategy to reduce or eliminate this risk"
    }
  ],
  "assumptions": [
    {
      "id": "ASMP-001",
      "description": "A belief taken as true for analysis purposes that has not been explicitly stated or confirmed by the client"
    }
  ],
  "questions_for_client": [
    {
      "id": "Q-001",
      "question": "Precise, answerable question the client must resolve before development begins",
      "context": "Why this ambiguity blocks design or estimation decisions"
    }
  ]
}

Extraction rules:
- Extract EVERY identifiable requirement; do not summarise or consolidate unless two items are truly identical.
- Infer risks from technical complexity, missing detail, ambiguous scope, and integration dependencies.
- Derive assumptions from anything the document implies but does not state explicitly.
- Questions must be specific and actionable — not generic like "Can you clarify requirements?"
- Never hallucinate requirements that cannot be inferred from the document content."""


# ──────────────────────────────────────────────────────────────────────────────
# File ingestion
# ──────────────────────────────────────────────────────────────────────────────

def _read_text_file(file_path: Path) -> str:
    """Read a plain-text or Markdown file with UTF-8 encoding."""
    try:
        text = file_path.read_text(encoding="utf-8")
    except UnicodeDecodeError:
        # Fall back to latin-1 for documents exported from legacy tools.
        logger.warning(
            "UTF-8 decode failed for '%s'; retrying with latin-1.", file_path.name
        )
        text = file_path.read_text(encoding="latin-1")

    if not text.strip():
        raise ValueError(f"Text file is empty: {file_path}")
    return text


def _read_pdf_file(file_path: Path) -> str:
    """
    Extract plain text from a PDF using pdfplumber.

    pdfplumber handles multi-column layouts and embedded fonts better than
    PyPDF2.  It requires the `pdfplumber` package (listed in requirements.txt).
    """
    try:
        import pdfplumber  # Local import — optional dependency guard
    except ImportError as exc:
        raise ImportError(
            "pdfplumber is required to read PDF files. "
            "Install it with: pip install pdfplumber"
        ) from exc

    try:
        with pdfplumber.open(file_path) as pdf:
            pages: list[str] = []
            for page_num, page in enumerate(pdf.pages, start=1):
                page_text = page.extract_text()
                if page_text:
                    pages.append(page_text)
                else:
                    logger.debug("Page %d of '%s' yielded no text.", page_num, file_path.name)
    except Exception as exc:
        raise IOError(f"Failed to open or parse PDF '{file_path}': {exc}") from exc

    combined = "\n\n".join(pages).strip()
    if not combined:
        raise ValueError(
            f"PDF '{file_path.name}' contains no extractable text. "
            "The file may be scanned/image-based and requires OCR preprocessing."
        )
    return combined


def read_requirement_file(file_path: Path) -> str:
    """
    Read the requirement document from *file_path* and return its raw text.

    Supported formats:
      .txt  — UTF-8 or latin-1 plain text
      .md   — Markdown (treated as plain text; headers become context)
      .pdf  — PDF with embedded selectable text

    Raises:
        FileNotFoundError: If the path does not exist on disk.
        ValueError:        If the file is empty or the format is unsupported.
        IOError:           If a PDF cannot be opened or parsed.
    """
    if not file_path.exists():
        raise FileNotFoundError(f"Requirement file not found: {file_path}")
    if not file_path.is_file():
        raise ValueError(f"Path is a directory, not a file: {file_path}")

    suffix = file_path.suffix.lower()

    if suffix in {".txt", ".md"}:
        return _read_text_file(file_path)
    elif suffix == ".pdf":
        return _read_pdf_file(file_path)
    else:
        raise ValueError(
            f"Unsupported file format: '{suffix}'. "
            "Accepted formats are: .txt, .md, .pdf"
        )


# ──────────────────────────────────────────────────────────────────────────────
# Token estimation and guardrail
# ──────────────────────────────────────────────────────────────────────────────

def estimate_token_count(text: str) -> int:
    """
    Estimate the number of tokens in *text* without calling the tokenizer.

    Rule of thumb: 1 token ≈ 4 characters for English prose.  This is a
    conservative approximation used only for the pre-flight guardrail — it
    does not need to be exact, it just needs to prevent a clear overage.
    """
    return max(len(text) // 4, len(text.split()))


def enforce_token_budget(document_text: str) -> str:
    """
    Truncate *document_text* if it exceeds MAX_DOCUMENT_CHARS.

    A truncation notice is appended so the model is aware the document
    was cut and can note that in its questions_for_client array.

    Returns the (possibly truncated) text safe to send as the user message.
    """
    if len(document_text) <= MAX_DOCUMENT_CHARS:
        return document_text

    logger.warning(
        "Document exceeds the safe character budget (%d chars). "
        "Truncating to %d chars to stay within the 8,000 TPM free-tier cap.",
        len(document_text),
        MAX_DOCUMENT_CHARS,
    )

    truncated = document_text[:MAX_DOCUMENT_CHARS]
    notice = (
        "\n\n[SYSTEM NOTE: The document was truncated at this point because it exceeded "
        "the token budget. Requirements appearing after this point were not analysed. "
        "Flag this in questions_for_client so the client knows a full analysis requires "
        "splitting the document into smaller sections.]"
    )
    return truncated + notice


# ──────────────────────────────────────────────────────────────────────────────
# Groq API call with tenacity retry
# ──────────────────────────────────────────────────────────────────────────────

@retry(
    # Only retry on HTTP 429 (rate limit). Let authentication errors and
    # other Groq exceptions propagate immediately so the caller can diagnose them.
    retry=retry_if_exception_type(groq.RateLimitError),

    # Exponential backoff: 30s → 60s → 120s (capped).
    # The free-tier TPM window resets every 60 seconds, so starting at 30s
    # gives a 50% chance of recovering on the first retry, and the 60s wait
    # virtually guarantees the next attempt falls in a fresh window.
    wait=wait_exponential(multiplier=2, min=30, max=120),

    # Give up after 5 total attempts (1 initial + 4 retries).
    stop=stop_after_attempt(5),

    # Log each sleep so operators can see retry cadence without diving into
    # debug traces.
    before_sleep=before_sleep_log(logger, logging.WARNING),

    # Re-raise the final RateLimitError if all retries are exhausted, rather
    # than wrapping it in a tenacity RetryError.
    reraise=True,
)
def _call_groq_with_retry(client: groq.Groq, user_content: str) -> dict[str, Any]:
    """
    Make a single chat-completion request to the Groq API.

    This function is decorated with @retry so it is automatically retried on
    429 responses.  It is intentionally thin — all business logic lives in
    analyze_requirements().

    Args:
        client:       An initialised groq.Groq client bound to the caller's API key.
        user_content: The full document text to analyse (already token-bounded).

    Returns:
        A Python dict parsed from the model's JSON response.

    Raises:
        groq.RateLimitError:    If all retry attempts are exhausted.
        groq.APIError:          For non-429 API-level errors (auth, server, etc.)
        json.JSONDecodeError:   If the model's response cannot be parsed as JSON.
        ValueError:             If the model returned an empty response body.
    """
    logger.info(
        "Calling Groq API (model=%s, ~%d estimated input tokens).",
        MODEL_ID,
        estimate_token_count(SYSTEM_PROMPT + user_content),
    )

    response = client.chat.completions.create(
        model=MODEL_ID,
        messages=[
            {
                "role": "system",
                # The system prompt is hardcoded above to maximise prompt
                # caching.  Groq caches static prefixes automatically.
                "content": SYSTEM_PROMPT,
            },
            {
                "role": "user",
                "content": (
                    "Please analyse the following client requirement document and return "
                    "the structured JSON analysis as instructed.\n\n"
                    "--- DOCUMENT START ---\n"
                    f"{user_content}\n"
                    "--- DOCUMENT END ---"
                ),
            },
        ],
        # Force JSON output mode — the model MUST return a valid JSON object.
        # This maps directly to the OpenAI-compatible structured output flag
        # that GroqCloud supports.
        response_format={"type": "json_object"},
        # Low temperature for deterministic, structured extraction.
        temperature=0.1,
        # Reserve up to 2,048 tokens for the response.
        max_tokens=2048,
    )

    raw_content: Optional[str] = response.choices[0].message.content
    if not raw_content or not raw_content.strip():
        raise ValueError(
            "Groq API returned an empty response body. "
            "This may indicate a model-side error. Try again in a moment."
        )

    logger.debug("Raw API response (first 300 chars): %.300s", raw_content)

    # Parse the JSON response.  json_object mode should guarantee this succeeds,
    # but we wrap it explicitly for a clear error message on unexpected failures.
    try:
        parsed: dict[str, Any] = json.loads(raw_content)
    except json.JSONDecodeError as exc:
        raise json.JSONDecodeError(
            f"Model response was not valid JSON (offset {exc.pos}): {exc.msg}\n"
            f"Raw content: {raw_content[:500]}",
            exc.doc,
            exc.pos,
        ) from exc

    return parsed


# ──────────────────────────────────────────────────────────────────────────────
# Response schema validation
# ──────────────────────────────────────────────────────────────────────────────

# The five top-level keys the model must return.
_REQUIRED_SCHEMA_KEYS = {
    "functional_requirements",
    "non_functional_requirements",
    "risks",
    "assumptions",
    "questions_for_client",
}


def _validate_and_normalise(raw: dict[str, Any]) -> dict[str, Any]:
    """
    Ensure the parsed response conforms to the expected schema.

    Any missing top-level key is defaulted to an empty list and a warning is
    emitted, allowing the pipeline to continue rather than crash on a partial
    response.  Individual item fields are intentionally not validated here
    because the HTML renderer already uses .get() with safe defaults.
    """
    for key in _REQUIRED_SCHEMA_KEYS:
        if key not in raw:
            logger.warning(
                "Schema key '%s' absent from model response; defaulting to [].", key
            )
            raw[key] = []
        elif not isinstance(raw[key], list):
            logger.warning(
                "Schema key '%s' is not a list (got %s); defaulting to [].",
                key,
                type(raw[key]).__name__,
            )
            raw[key] = []

    return raw


# ──────────────────────────────────────────────────────────────────────────────
# Public entry point
# ──────────────────────────────────────────────────────────────────────────────

def analyze_requirements(file_path: Path, api_key: str) -> dict[str, Any]:
    """
    Full pipeline: read file → check tokens → call Groq → return analysis dict.

    Args:
        file_path: Absolute or relative path to the requirement document.
        api_key:   GroqCloud API key (from AppConfig; never hardcoded here).

    Returns:
        A dict with five keys:
          - functional_requirements
          - non_functional_requirements
          - risks
          - assumptions
          - questions_for_client

    Raises:
        FileNotFoundError:  If file_path does not exist.
        ValueError:         If the file is empty, unsupported, or too large after truncation.
        groq.APIError:      For unrecoverable Groq API errors (auth failure, server error).
        groq.RateLimitError: If all retry attempts are exhausted (passes through from tenacity).
        json.JSONDecodeError: If the model response cannot be parsed.
    """
    # ── Step 1: Read the document ─────────────────────────────────────────────
    logger.info("Reading requirement document: %s", file_path)
    try:
        raw_text = read_requirement_file(Path(file_path))
    except (FileNotFoundError, ValueError, IOError) as exc:
        logger.error("File ingestion failed: %s", exc)
        raise

    return analyze_requirements_text(raw_text, api_key)


def analyze_requirements_text(raw_text: str, api_key: str) -> dict[str, Any]:
    """
    Analyze raw text requirements.
    """
    logger.info(
        "Document loaded: %d characters (~%d estimated tokens).",
        len(raw_text),
        estimate_token_count(raw_text),
    )

    # ── Step 2: Apply token budget guardrail ──────────────────────────────────
    user_content = enforce_token_budget(raw_text)

    # ── Step 3: Initialise the Groq client ───────────────────────────────────
    # The client is constructed here (not at module level) so that different
    # api_key values can be used across tests or multi-tenant invocations.
    try:
        client = groq.Groq(api_key=api_key)
    except Exception as exc:
        raise RuntimeError(
            f"Failed to initialise the Groq client: {exc}. "
            "Verify that the GROQ_API_KEY value is correct."
        ) from exc

    # ── Step 4: Call the API (with automatic retry on 429) ───────────────────
    try:
        raw_analysis = _call_groq_with_retry(client, user_content)
    except groq.AuthenticationError as exc:
        raise RuntimeError(
            "Groq API authentication failed. Check that GROQ_API_KEY is correct "
            "and has not been revoked at https://console.groq.com/keys"
        ) from exc
    except groq.RateLimitError as exc:
        raise RuntimeError(
            "Groq API rate limit exceeded after all retry attempts. "
            f"Free-tier cap: {GROQ_FREE_TIER_TPM} TPM / {GROQ_FREE_TIER_RPM} RPM. "
            "Wait 60 seconds and try again, or upgrade your GroqCloud plan."
        ) from exc
    except groq.APIError as exc:
        raise RuntimeError(
            f"Groq API returned an unrecoverable error (status {exc.status_code}): {exc}"
        ) from exc

    # ── Step 5: Validate and return ───────────────────────────────────────────
    analysis = _validate_and_normalise(raw_analysis)

    logger.info(
        "Analysis complete — FRs: %d | NFRs: %d | Risks: %d | "
        "Assumptions: %d | Questions: %d",
        len(analysis["functional_requirements"]),
        len(analysis["non_functional_requirements"]),
        len(analysis["risks"]),
        len(analysis["assumptions"]),
        len(analysis["questions_for_client"]),
    )

    return analysis
