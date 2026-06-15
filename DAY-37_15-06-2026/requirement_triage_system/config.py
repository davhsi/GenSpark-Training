"""
config.py — Environment variable loader and validator.

This module is the sole place in the application that touches os.environ.
All other modules receive an AppConfig instance so that secrets never
bleed across module boundaries as bare strings.
"""

import os
import re
from dataclasses import dataclass

from dotenv import load_dotenv


# ──────────────────────────────────────────────────────────────────────────────
# Configuration container
# ──────────────────────────────────────────────────────────────────────────────

@dataclass(frozen=True)
class AppConfig:
    """
    Immutable snapshot of every runtime secret and setting.

    frozen=True means attribute assignment raises FrozenInstanceError at
    runtime, preventing accidental mutation of secrets after loading.
    """
    groq_api_key: str
    sender_email: str
    gmail_app_password: str
    receiver_email: str
    imap_server: str
    smtp_server: str
    smtp_port: int


# ──────────────────────────────────────────────────────────────────────────────
# Internal validation helpers
# ──────────────────────────────────────────────────────────────────────────────

# Surface-level email pattern — catches obvious typos, not a full RFC 5322 parser.
_EMAIL_RE = re.compile(r"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$")


def _require_email(value: str, field_name: str) -> str:
    """
    Verify that *value* looks like an email address.

    Raises:
        ValueError: If the value does not match the basic email pattern.
    """
    value = value.strip()
    if not _EMAIL_RE.match(value):
        raise ValueError(
            f"'{field_name}' does not look like a valid email address: {value!r}\n"
            "Expected format: user@domain.tld"
        )
    return value


def _require_nonempty(value: str, field_name: str, missing: list[str]) -> str:
    """
    Add *field_name* to the *missing* accumulator if *value* is blank.

    Returns the stripped value so callers can always assign it (even if empty).
    This pattern allows us to collect ALL missing fields in a single pass
    before raising, rather than aborting on the first missing variable.
    """
    stripped = value.strip()
    if not stripped:
        missing.append(field_name)
    return stripped


# ──────────────────────────────────────────────────────────────────────────────
# Public factory function
# ──────────────────────────────────────────────────────────────────────────────

def load_config() -> AppConfig:
    """
    Load, validate, and return an immutable AppConfig.

    Behaviour:
    - Calls load_dotenv(), which reads .env into os.environ without
      overwriting variables already present in the shell environment.
      This makes the module safe to use in both local dev and CI/CD.
    - Collects every missing/invalid variable before raising, so operators
      see the complete list of problems in one error message rather than
      having to fix-and-retry for each field.

    Raises:
        EnvironmentError: One or more required variables are absent.
        ValueError:       A variable is present but fails format validation.
    """
    # load_dotenv is idempotent; safe to call multiple times.
    load_dotenv()

    # Accumulate names of absent variables; raise once at the end.
    missing: list[str] = []

    # ── GROQ_API_KEY ──────────────────────────────────────────────────────────
    groq_api_key = _require_nonempty(
        os.getenv("GROQ_API_KEY", ""), "GROQ_API_KEY", missing
    )

    # ── SENDER_EMAIL ──────────────────────────────────────────────────────────
    sender_email_raw = _require_nonempty(
        os.getenv("SENDER_EMAIL", ""), "SENDER_EMAIL", missing
    )
    # Only validate format when the value is actually present.
    sender_email = (
        _require_email(sender_email_raw, "SENDER_EMAIL")
        if sender_email_raw
        else ""
    )

    # ── GMAIL_APP_PASSWORD ───────────────────────────────────────────────────
    # Google displays App Passwords as "xxxx xxxx xxxx xxxx" (with spaces).
    # We strip spaces before the 16-character length check so both display
    # formats are accepted.
    raw_app_password = _require_nonempty(
        os.getenv("GMAIL_APP_PASSWORD", ""), "GMAIL_APP_PASSWORD", missing
    )
    if raw_app_password:
        normalised_password = raw_app_password.replace(" ", "")
        if len(normalised_password) != 16:
            raise ValueError(
                f"GMAIL_APP_PASSWORD must be exactly 16 characters after stripping spaces "
                f"(found {len(normalised_password)} characters).\n"
                "Generate a new App Password at: https://myaccount.google.com/apppasswords\n"
                "Tip: Make sure 2-Step Verification is enabled on your Google account first."
            )
        gmail_app_password = normalised_password
    else:
        gmail_app_password = ""

    # ── RECEIVER_EMAIL ────────────────────────────────────────────────────────
    receiver_email_raw = _require_nonempty(
        os.getenv("RECEIVER_EMAIL", ""), "RECEIVER_EMAIL", missing
    )
    receiver_email = (
        _require_email(receiver_email_raw, "RECEIVER_EMAIL")
        if receiver_email_raw
        else ""
    )

    # ── EMAIL SERVERS ─────────────────────────────────────────────────────────
    # Default to Gmail's endpoints; override in .env only if switching providers.
    imap_server = os.getenv("IMAP_SERVER", "imap.gmail.com").strip() or "imap.gmail.com"
    smtp_server = os.getenv("SMTP_SERVER", "smtp.gmail.com").strip() or "smtp.gmail.com"

    # ── SMTP_PORT ─────────────────────────────────────────────────────────────
    smtp_port_raw = os.getenv("SMTP_PORT", "587").strip()
    try:
        smtp_port = int(smtp_port_raw)
    except ValueError:
        raise ValueError(
            f"SMTP_PORT must be a valid integer (e.g. 587), got: {smtp_port_raw!r}"
        )
    if not (1 <= smtp_port <= 65535):
        raise ValueError(
            f"SMTP_PORT must be between 1 and 65535, got: {smtp_port}"
        )

    # ── Raise a single, actionable error if any required vars are absent ──────
    if missing:
        bullet_list = "\n".join(f"  • {var}" for var in missing)
        raise EnvironmentError(
            "The following required environment variables are not set:\n"
            f"{bullet_list}\n\n"
            "Fix: copy .env.example to .env and fill in the missing values."
        )

    return AppConfig(
        groq_api_key=groq_api_key,
        sender_email=sender_email,
        gmail_app_password=gmail_app_password,
        receiver_email=receiver_email,
        imap_server=imap_server,
        smtp_server=smtp_server,
        smtp_port=smtp_port,
    )
