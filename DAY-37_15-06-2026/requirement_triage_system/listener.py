"""
listener.py — IMAP Email Poller for Requirement Triage

Connects to the configured inbox, searches for unread emails with "requirement"
in the subject, and extracts attached .txt or .md files.
"""

import email
import imaplib
import logging
from dataclasses import dataclass
from email.message import Message
from typing import Iterator, Optional

from config import AppConfig

logger = logging.getLogger(__name__)


@dataclass
class RequirementEmail:
    """Represents a successfully parsed requirement email."""
    sender: str
    subject: str
    content: str


def _extract_attachment_text(msg: Message) -> Optional[str]:
    """
    Search the email parts for a .txt or .md attachment and return its decoded text.
    If no valid attachment is found, returns None.
    """
    for part in msg.walk():
        if part.get_content_maintype() == "multipart":
            continue

        filename = part.get_filename()
        if not filename:
            continue

        filename_lower = filename.lower()
        if filename_lower.endswith(".txt") or filename_lower.endswith(".md"):
            logger.info("Found attachment: %s", filename)
            payload = part.get_payload(decode=True)
            if payload:
                try:
                    return payload.decode("utf-8")
                except UnicodeDecodeError:
                    return payload.decode("latin-1")

    return None


def fetch_unread_requirements(config: AppConfig) -> Iterator[RequirementEmail]:
    """
    Connect to IMAP, find unread emails matching 'requirement', extract their
    attachments, and yield RequirementEmail objects.

    By using RFC822 for fetching, the IMAP server automatically marks the emails
    as \Seen, so they won't be processed again on the next loop.
    """
    logger.debug("Connecting to IMAP server: %s", config.imap_server)
    try:
        mail = imaplib.IMAP4_SSL(config.imap_server)
        mail.login(config.sender_email, config.gmail_app_password)
        mail.select("inbox")
    except Exception as exc:
        logger.error("Failed to connect or login to IMAP server: %s", exc)
        return

    # Search for unread emails with "requirement" in the subject
    status, messages = mail.search(None, '(UNSEEN SUBJECT "requirement")')
    if status != "OK":
        logger.error("IMAP search failed with status: %s", status)
        mail.logout()
        return

    message_ids = messages[0].split()
    if not message_ids:
        logger.debug("No new requirement emails found.")
        mail.logout()
        return

    logger.info("Found %d new requirement email(s) to process.", len(message_ids))

    for msg_id in message_ids:
        status, msg_data = mail.fetch(msg_id, "(RFC822)")
        if status != "OK":
            logger.warning("Failed to fetch message ID %s", msg_id)
            continue

        # Extract the raw email bytes
        raw_email = msg_data[0][1]
        msg = email.message_from_bytes(raw_email)

        sender = msg.get("From", "Unknown Sender")
        subject = msg.get("Subject", "No Subject")
        
        logger.info("Processing email from: %s | Subject: %s", sender, subject)

        content = _extract_attachment_text(msg)
        if not content:
            logger.warning(
                "Skipping email from %s: No .txt or .md attachment found.", sender
            )
            continue

        yield RequirementEmail(sender=sender, subject=subject, content=content)

    mail.close()
    mail.logout()
