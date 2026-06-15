"""
main.py — IMAP Polling Agent for the Requirement Triage System.

Execution flow:
  1. Load and validate all environment secrets via config.py.
  2. Enter a continuous polling loop (checks every 60s).
  3. Fetch unread requirement emails via listener.py.
  4. Send the extracted text to GroqCloud via analyzer.py.
  5. Render the analysis as an HTML email and deliver it via notifier.py.
  6. Loop indefinitely until interrupted.
"""

import logging
import sys
import time

# Local modules
from config import load_config
from analyzer import analyze_requirements_text
from notifier import send_analysis_email
from listener import fetch_unread_requirements

# ──────────────────────────────────────────────────────────────────────────────
# Logging setup
# ──────────────────────────────────────────────────────────────────────────────

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s  [%(levelname)-8s]  %(name)s — %(message)s",
    datefmt="%Y-%m-%d %H:%M:%S",
    stream=sys.stdout,
)

logger = logging.getLogger(__name__)


# ──────────────────────────────────────────────────────────────────────────────
# Main orchestrator
# ──────────────────────────────────────────────────────────────────────────────

def main() -> int:
    """
    Continuous IMAP polling orchestrator.
    """
    logger.info("━" * 60)
    logger.info("  Requirement Triage System — IMAP Poller Start")
    logger.info("━" * 60)

    # ── Stage 1: Load configuration ───────────────────────────────────────────
    try:
        config = load_config()
    except EnvironmentError as exc:
        logger.error("Configuration error (missing variables):\n%s", exc)
        return 1
    except ValueError as exc:
        logger.error("Configuration error (invalid value):\n%s", exc)
        return 1

    logger.info(
        "      Config OK — polling: %s | receiver: %s | IMAP: %s | SMTP: %s:%d",
        config.sender_email,
        config.receiver_email,
        config.imap_server,
        config.smtp_server,
        config.smtp_port,
    )

    # ── Stage 2: Continuous Polling Loop ─────────────────────────────────────
    logger.info("[2/2] Starting polling loop. Press Ctrl+C to stop.")
    
    try:
        while True:
            try:
                for req in fetch_unread_requirements(config):
                    logger.info("━" * 40)
                    logger.info("Processing new email: %s", req.subject)
                    
                    # Analyze requirements
                    try:
                        analysis = analyze_requirements_text(
                            raw_text=req.content,
                            api_key=config.groq_api_key,
                        )
                    except Exception as exc:
                        logger.error("Analysis failed for '%s': %s", req.subject, exc, exc_info=True)
                        continue

                    # Send email report
                    try:
                        send_analysis_email(
                            config=config,
                            analysis=analysis,
                            source_name=req.subject,
                        )
                    except Exception as exc:
                        logger.error("Email delivery failed for '%s': %s", req.subject, exc, exc_info=True)
                        continue
                        
                    logger.info("Successfully processed '%s'", req.subject)
                    logger.info("━" * 40)
                    
            except Exception as exc:
                logger.error("Unexpected error during polling cycle: %s", exc, exc_info=True)
                
            # Sleep until next cycle
            time.sleep(60)
            
    except KeyboardInterrupt:
        logger.info("\nReceived KeyboardInterrupt. Shutting down cleanly...")
        return 0


if __name__ == "__main__":
    sys.exit(main())
