"""
notifier.py — HTML email construction and SMTP delivery module.

Responsibilities:
  1. Transform the structured analysis dictionary into a full, professional
     HTML email with inline CSS (maximising email-client compatibility).
  2. Provide a plain-text fallback for clients that do not render HTML.
  3. Connect to Gmail via SMTP on port 587, negotiate STARTTLS, authenticate
     with the Google App Password, and deliver the message.
"""

import logging
import smtplib
import ssl
from datetime import datetime, timezone
from email.mime.application import MIMEApplication
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText
from pathlib import Path
from typing import Any

from config import AppConfig

logger = logging.getLogger(__name__)


# ──────────────────────────────────────────────────────────────────────────────
# Colour palette (centralised for easy theming)
# ──────────────────────────────────────────────────────────────────────────────

_COLOUR = {
    "header_bg":    "#1a237e",
    "fr":           "#27ae60",
    "nfr":          "#8e44ad",
    "risk":         "#c0392b",
    "assumption":   "#2980b9",
    "question":     "#e67e22",
    "border":       "#ecf0f1",
    "row_alt":      "#f8f9fa",
    "text_body":    "#333333",
    "text_muted":   "#7f8c8d",
    "text_heading": "#2c3e50",
}


# ──────────────────────────────────────────────────────────────────────────────
# Micro-component renderers (return raw HTML strings, no side effects)
# ──────────────────────────────────────────────────────────────────────────────

def _badge(label: str, fg: str, bg: str, border: str = "") -> str:
    """Render a small pill/badge with inline CSS."""
    border_css = f"border:1px solid {border or fg};" if (border or fg) else ""
    return (
        f'<span style="display:inline-block;padding:2px 9px;border-radius:12px;'
        f'font-size:11px;font-weight:700;color:{fg};background-color:{bg};'
        f'{border_css}font-family:Arial,sans-serif;">'
        f'{label.upper()}'
        f'</span>'
    )


def _priority_badge(priority: str) -> str:
    """Return a colour-coded badge for High / Medium / Low priority labels."""
    profiles = {
        "high":   ("#c0392b", "#fdf2f2"),
        "medium": ("#d68910", "#fef9e7"),
        "low":    ("#1a5276", "#eaf4fc"),
    }
    fg, bg = profiles.get(priority.strip().lower(), ("#555555", "#f5f5f5"))
    return _badge(priority, fg, bg)


def _category_badge(category: str) -> str:
    """Return a purple category chip for NFR category labels."""
    return _badge(category, "#6c3483", "#f5eef8")


def _section_header_row(title: str, count: int, accent: str) -> str:
    """
    Return a <tr> for the section title bar inside the card table.

    Includes the section title on the left and an item-count chip on the right.
    """
    chip = (
        f'<span style="display:inline-block;padding:2px 10px;border-radius:12px;'
        f'font-size:12px;font-weight:700;color:#fff;background-color:{accent};'
        f'font-family:Arial,sans-serif;">'
        f'{count} item{"s" if count != 1 else ""}'
        f'</span>'
    )
    return (
        f'<tr><td style="padding:28px 32px 8px 32px;">'
        f'<table width="100%" cellpadding="0" cellspacing="0" border="0"><tr>'
        f'<td style="font-size:16px;font-weight:700;color:{accent};'
        f'font-family:Arial,sans-serif;border-bottom:2px solid {accent};'
        f'padding-bottom:8px;">{title}</td>'
        f'<td align="right" style="border-bottom:2px solid {accent};padding-bottom:8px;">'
        f'{chip}</td>'
        f'</tr></table>'
        f'</td></tr>'
    )


def _empty_row(message: str = "None identified.") -> str:
    """Return a <tr> placeholder when a section has no items."""
    return (
        f'<tr><td style="padding:6px 32px 24px 32px;">'
        f'<p style="margin:0;font-family:Arial,sans-serif;font-size:13px;'
        f'color:{_COLOUR["text_muted"]};font-style:italic;">{message}</p>'
        f'</td></tr>'
    )


def _th(label: str) -> str:
    """Render a table header cell with consistent styling."""
    return (
        f'<th style="padding:10px 14px;text-align:left;font-family:Arial,sans-serif;'
        f'font-size:11px;color:{_COLOUR["text_muted"]};font-weight:700;'
        f'text-transform:uppercase;letter-spacing:0.5px;">{label}</th>'
    )


def _th_center(label: str) -> str:
    """Render a centred table header cell."""
    return (
        f'<th style="padding:10px 14px;text-align:center;font-family:Arial,sans-serif;'
        f'font-size:11px;color:{_COLOUR["text_muted"]};font-weight:700;'
        f'text-transform:uppercase;letter-spacing:0.5px;">{label}</th>'
    )


# ──────────────────────────────────────────────────────────────────────────────
# Section-level renderers — each returns one or two <tr> elements
# ──────────────────────────────────────────────────────────────────────────────

def _render_functional_requirements(items: list[dict[str, Any]]) -> str:
    """Return <tr>(s) containing the Functional Requirements table."""
    if not items:
        return _empty_row("No functional requirements identified.")

    data_rows = ""
    for fr in items:
        fr_id = fr.get("id", "—")
        title = fr.get("title", "—")
        description = fr.get("description", "—")
        priority = fr.get("priority", "—")
        acceptance = fr.get("acceptance_criteria", "—")

        data_rows += (
            f'<tr>'
            # ID column
            f'<td style="padding:12px 14px;border-bottom:1px solid {_COLOUR["border"]};'
            f'font-family:Arial,sans-serif;font-size:12px;font-weight:700;'
            f'color:{_COLOUR["text_heading"]};vertical-align:top;width:68px;">{fr_id}</td>'
            # Requirement detail column
            f'<td style="padding:12px 14px;border-bottom:1px solid {_COLOUR["border"]};'
            f'font-family:Arial,sans-serif;font-size:13px;color:{_COLOUR["text_body"]};'
            f'vertical-align:top;">'
            f'<strong>{title}</strong><br>'
            f'<span style="color:{_COLOUR["text_muted"]};font-size:12px;line-height:1.5;">'
            f'{description}</span><br><br>'
            f'<span style="font-size:11px;color:{_COLOUR["text_muted"]};">'
            f'<strong>Acceptance:</strong> {acceptance}</span>'
            f'</td>'
            # Priority column
            f'<td style="padding:12px 14px;border-bottom:1px solid {_COLOUR["border"]};'
            f'vertical-align:top;text-align:center;width:90px;">'
            f'{_priority_badge(priority)}</td>'
            f'</tr>'
        )

    inner_table = (
        f'<table width="100%" cellpadding="0" cellspacing="0" border="0" '
        f'style="border-collapse:collapse;border:1px solid {_COLOUR["border"]};">'
        f'<tr style="background-color:{_COLOUR["row_alt"]};">'
        f'{_th("ID")}{_th("Requirement")}{_th_center("Priority")}'
        f'</tr>'
        f'{data_rows}'
        f'</table>'
    )

    return f'<tr><td style="padding:0 32px 24px 32px;">{inner_table}</td></tr>'


def _render_non_functional_requirements(items: list[dict[str, Any]]) -> str:
    """Return <tr>(s) containing the Non-Functional Requirements table."""
    if not items:
        return _empty_row("No non-functional requirements identified.")

    data_rows = ""
    for nfr in items:
        nfr_id = nfr.get("id", "—")
        category = nfr.get("category", "—")
        description = nfr.get("description", "—")
        metric = nfr.get("metric", "")

        metric_html = (
            f'<br><span style="font-size:11px;color:{_COLOUR["text_muted"]};">'
            f'<strong>Metric:</strong> {metric}</span>'
        ) if metric else ""

        data_rows += (
            f'<tr>'
            f'<td style="padding:10px 14px;border-bottom:1px solid {_COLOUR["border"]};'
            f'font-family:Arial,sans-serif;font-size:12px;font-weight:700;'
            f'color:{_COLOUR["text_heading"]};vertical-align:top;width:68px;">{nfr_id}</td>'
            f'<td style="padding:10px 14px;border-bottom:1px solid {_COLOUR["border"]};'
            f'vertical-align:top;width:130px;">{_category_badge(category)}</td>'
            f'<td style="padding:10px 14px;border-bottom:1px solid {_COLOUR["border"]};'
            f'font-family:Arial,sans-serif;font-size:12px;color:{_COLOUR["text_body"]};'
            f'vertical-align:top;">{description}{metric_html}</td>'
            f'</tr>'
        )

    inner_table = (
        f'<table width="100%" cellpadding="0" cellspacing="0" border="0" '
        f'style="border-collapse:collapse;border:1px solid {_COLOUR["border"]};">'
        f'<tr style="background-color:{_COLOUR["row_alt"]};">'
        f'{_th("ID")}{_th("Category")}{_th("Details")}'
        f'</tr>'
        f'{data_rows}'
        f'</table>'
    )

    return f'<tr><td style="padding:0 32px 24px 32px;">{inner_table}</td></tr>'


def _render_risks(items: list[dict[str, Any]]) -> str:
    """Return <tr>(s) containing the Risk Register table."""
    if not items:
        return _empty_row("No risks identified.")

    data_rows = ""
    for risk in items:
        risk_id = risk.get("id", "—")
        description = risk.get("description", "—")
        impact = risk.get("impact", "—")
        likelihood = risk.get("likelihood", "—")
        mitigation = risk.get("mitigation", "—")

        data_rows += (
            f'<tr>'
            f'<td style="padding:10px 14px;border-bottom:1px solid {_COLOUR["border"]};'
            f'font-family:Arial,sans-serif;font-size:12px;font-weight:700;'
            f'color:{_COLOUR["text_heading"]};vertical-align:top;width:68px;">{risk_id}</td>'
            f'<td style="padding:10px 14px;border-bottom:1px solid {_COLOUR["border"]};'
            f'font-family:Arial,sans-serif;font-size:12px;color:{_COLOUR["text_body"]};'
            f'vertical-align:top;">'
            f'{description}<br>'
            f'<span style="font-size:11px;color:{_COLOUR["text_muted"]};">'
            f'<strong>Mitigation:</strong> {mitigation}</span></td>'
            f'<td style="padding:10px 14px;border-bottom:1px solid {_COLOUR["border"]};'
            f'vertical-align:top;text-align:center;width:90px;">{_priority_badge(impact)}</td>'
            f'<td style="padding:10px 14px;border-bottom:1px solid {_COLOUR["border"]};'
            f'vertical-align:top;text-align:center;width:90px;">{_priority_badge(likelihood)}</td>'
            f'</tr>'
        )

    inner_table = (
        f'<table width="100%" cellpadding="0" cellspacing="0" border="0" '
        f'style="border-collapse:collapse;border:1px solid {_COLOUR["border"]};">'
        f'<tr style="background-color:{_COLOUR["row_alt"]};">'
        f'{_th("ID")}{_th("Risk")}{_th_center("Impact")}{_th_center("Likelihood")}'
        f'</tr>'
        f'{data_rows}'
        f'</table>'
    )

    return f'<tr><td style="padding:0 32px 24px 32px;">{inner_table}</td></tr>'


def _render_assumptions(items: list[dict[str, Any]]) -> str:
    """Return a <tr> containing a bulleted list of assumptions."""
    if not items:
        return _empty_row("No assumptions recorded.")

    list_items = "".join(
        f'<li style="margin-bottom:8px;font-family:Arial,sans-serif;font-size:13px;'
        f'color:{_COLOUR["text_body"]};line-height:1.5;">'
        f'<strong style="color:{_COLOUR["text_heading"]};">{a.get("id", "—")}:</strong> '
        f'{a.get("description", "—")}</li>'
        for a in items
    )

    return (
        f'<tr><td style="padding:0 32px 24px 32px;">'
        f'<ul style="margin:0;padding-left:22px;">{list_items}</ul>'
        f'</td></tr>'
    )


def _render_questions(items: list[dict[str, Any]]) -> str:
    """Return a <tr> containing orange-accented question cards."""
    if not items:
        return _empty_row("No open questions for the client.")

    cards = ""
    for q in items:
        q_id = q.get("id", "—")
        question = q.get("question", "—")
        context = q.get("context", "")

        context_html = (
            f'<p style="margin:6px 0 0 0;font-family:Arial,sans-serif;font-size:11px;'
            f'color:{_COLOUR["text_muted"]};font-style:italic;">'
            f'Context: {context}</p>'
        ) if context else ""

        cards += (
            f'<div style="background:#fdf2e9;border-left:4px solid {_COLOUR["question"]};'
            f'padding:12px 16px;margin-bottom:10px;border-radius:0 4px 4px 0;">'
            f'<p style="margin:0 0 4px 0;font-family:Arial,sans-serif;font-size:11px;'
            f'font-weight:700;color:{_COLOUR["question"]};">{q_id}</p>'
            f'<p style="margin:0;font-family:Arial,sans-serif;font-size:13px;'
            f'color:{_COLOUR["text_heading"]};font-weight:600;">{question}</p>'
            f'{context_html}'
            f'</div>'
        )

    return (
        f'<tr><td style="padding:0 32px 24px 32px;">{cards}</td></tr>'
    )


def build_markdown_report(analysis: dict[str, Any], source_name: str) -> str:
    """
    Build a markdown version of the report to be attached as a file.
    """
    ts = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M:%S UTC")
    lines = [
        f"# Client Requirement Analysis Report",
        "",
        f"**Source:** {source_name}",
        f"**Generated:** {ts}",
        "",
    ]

    def section(title: str, items: list[dict]) -> None:
        lines.append(f"## {title}")
        if not items:
            lines.append("*(none)*\n")
            return
        for item in items:
            lines.append("- " + " | ".join(f"**{k}**: {v}" for k, v in item.items()))
        lines.append("")

    section("Functional Requirements", analysis.get("functional_requirements", []))
    section("Non-Functional Requirements", analysis.get("non_functional_requirements", []))
    section("Risks", analysis.get("risks", []))
    section("Assumptions", analysis.get("assumptions", []))
    section("Questions for Client", analysis.get("questions_for_client", []))

    return "\n".join(lines)


# ──────────────────────────────────────────────────────────────────────────────
# SMTP delivery
# ──────────────────────────────────────────────────────────────────────────────

def send_analysis_email(
    config: AppConfig,
    analysis: dict[str, Any],
    source_name: str,
) -> None:
    """
    Build the MIME message and deliver it to config.receiver_email via Gmail SMTP.
      4. Re-send EHLO post-TLS (required by RFC 3207).
      5. Authenticate with Gmail App Password.
      6. Transmit the MIME message.
      7. SMTP context manager automatically issues QUIT on exit.

    The message is a multipart/alternative with:
      - Part 1: text/plain  (accessibility + non-HTML client fallback)
      - Part 2: text/html   (rich rendered report — preferred by modern clients)

    Raises:
        RuntimeError: For authentication failures, network errors, or SMTP
                      rejections (wraps the underlying exception for context).
    """
    # ── Build the MIME message ────────────────────────────────────────────────
    date_str = datetime.now(timezone.utc).strftime("%Y-%m-%d")
    subject = f"[Triage Report] {source_name} — {date_str}"

    # Use mixed multipart for file attachments
    msg = MIMEMultipart("mixed")
    msg["Subject"] = subject
    msg["From"]    = config.sender_email
    msg["To"]      = config.receiver_email
    msg["X-Mailer"] = "RequirementTriageSystem/1.0"

    # Add the simple plain text body
    body_text = (
        f"Hello,\n\n"
        f"Please find the generated requirement triage report attached for '{source_name}'.\n\n"
        f"Best regards,\n"
        f"Requirement Triage System"
    )
    msg.attach(MIMEText(body_text, "plain", "utf-8"))

    # Attach markdown file as application/octet-stream to preserve the .md extension
    md_text = build_markdown_report(analysis, source_name)
    md_part = MIMEApplication(md_text.encode("utf-8"), Name="report.md")
    md_part.add_header("Content-Disposition", 'attachment; filename="report.md"')
    msg.attach(md_part)

    logger.info(
        "Connecting to SMTP server %s:%d …", config.smtp_server, config.smtp_port
    )

    # ── Connect, upgrade to TLS, authenticate, send ───────────────────────────
    try:
        # timeout=30 prevents the thread from blocking indefinitely if the
        # server is unreachable or unresponsive.
        with smtplib.SMTP(config.smtp_server, config.smtp_port, timeout=30) as smtp:

            smtp.ehlo()

            # STARTTLS negotiates TLS on the existing connection (port 587).
            # ssl.create_default_context() enforces certificate verification
            # against the system CA bundle, preventing MITM attacks.
            smtp.starttls(context=ssl.create_default_context())

            # Re-issue EHLO after TLS upgrade so the server updates its
            # advertised capabilities for the encrypted session.
            smtp.ehlo()

            smtp.login(config.sender_email, config.gmail_app_password)

            smtp.sendmail(
                from_addr=config.sender_email,
                to_addrs=[config.receiver_email],
                msg=msg.as_string(),
            )

    except smtplib.SMTPAuthenticationError as exc:
        raise RuntimeError(
            "Gmail SMTP authentication failed.\n"
            "  • Verify SENDER_EMAIL is correct.\n"
            "  • Verify GMAIL_APP_PASSWORD is the 16-character App Password "
            "(not your regular Gmail password).\n"
            "  • Ensure 2-Step Verification is enabled at myaccount.google.com.\n"
            f"  • Underlying error: {exc}"
        ) from exc

    except smtplib.SMTPRecipientsRefused as exc:
        raise RuntimeError(
            f"SMTP server refused delivery to recipient(s): {exc.recipients}\n"
            f"Verify RECEIVER_EMAIL='{config.receiver_email}' is correct."
        ) from exc

    except smtplib.SMTPServerDisconnected as exc:
        raise RuntimeError(
            "SMTP server closed the connection unexpectedly. "
            "This can happen if the server rate-limits new connections. "
            f"Retry in a few minutes. Underlying error: {exc}"
        ) from exc

    except smtplib.SMTPException as exc:
        raise RuntimeError(
            f"An SMTP protocol error occurred during email transmission: {exc}"
        ) from exc

    except TimeoutError as exc:
        raise RuntimeError(
            f"Connection to {config.smtp_server}:{config.smtp_port} timed out after 30 s. "
            "Check your network connectivity and SMTP_SERVER / SMTP_PORT values."
        ) from exc

    except OSError as exc:
        raise RuntimeError(
            f"Network-level error connecting to {config.smtp_server}:{config.smtp_port}: {exc}"
        ) from exc

    logger.info(
        "Email delivered successfully to '%s' (subject: %s).",
        config.receiver_email,
        subject,
    )
