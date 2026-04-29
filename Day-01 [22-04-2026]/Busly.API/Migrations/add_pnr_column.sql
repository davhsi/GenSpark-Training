-- Migration: Add stored pnr column to booking table
-- Run this against your PostgreSQL database before deploying the updated API.

-- 1. Add the column (nullable initially so existing rows don't fail)
ALTER TABLE booking ADD COLUMN IF NOT EXISTS pnr VARCHAR(8);

-- 2. Back-fill existing rows: derive PNR from the first 8 hex chars of the UUID
UPDATE booking
SET pnr = UPPER(REPLACE(id::text, '-', '')::varchar(8))
WHERE pnr IS NULL;

-- 3. Make the column NOT NULL now that all rows are populated
ALTER TABLE booking ALTER COLUMN pnr SET NOT NULL;

-- 4. Add unique index for O(1) PNR lookups
CREATE UNIQUE INDEX IF NOT EXISTS idx_booking_pnr ON booking (pnr);
