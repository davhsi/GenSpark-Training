-- ============================================================
-- Library Management System - Database Constraints
-- Run this script ONCE against the LibraryManagement DB.
-- These add uniqueness enforcement at the database level for
-- email and phone, which the application also validates but
-- the DB is the final safety net.
-- ============================================================

-- Unique constraint on Member email (case-insensitive via lower index)
CREATE UNIQUE INDEX IF NOT EXISTS uix_members_email
    ON "Members" (LOWER("Email"))
    WHERE "Email" IS NOT NULL;

-- Unique constraint on Member phone
CREATE UNIQUE INDEX IF NOT EXISTS uix_members_phone
    ON "Members" ("Phone")
    WHERE "Phone" IS NOT NULL AND "Phone" != '';

-- Unique constraint on BookCopy accession number
-- (should already be unique per library practice, enforce it)
CREATE UNIQUE INDEX IF NOT EXISTS uix_bookcopies_accession
    ON "BookCopies" ("AccessionNumber");

-- Optional: unique ISBN on Books (only when provided)
CREATE UNIQUE INDEX IF NOT EXISTS uix_books_isbn
    ON "Books" ("ISBN")
    WHERE "ISBN" IS NOT NULL AND "ISBN" != '';

-- ============================================================
-- Verification queries
-- ============================================================
-- SELECT indexname, indexdef FROM pg_indexes WHERE tablename = 'Members';
-- SELECT indexname, indexdef FROM pg_indexes WHERE tablename = 'BookCopies';
-- SELECT indexname, indexdef FROM pg_indexes WHERE tablename = 'Books';
