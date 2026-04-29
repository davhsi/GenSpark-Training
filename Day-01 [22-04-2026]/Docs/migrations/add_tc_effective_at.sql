-- Migration: Add effective_at column to tc_version table
-- This migration adds the missing effective_at column to support T&C effective dates

ALTER TABLE tc_version 
ADD COLUMN effective_at TIMESTAMP;

-- Add comment to document the column purpose
COMMENT ON COLUMN tc_version.effective_at IS 'When this T&C version becomes effective (null means immediately upon publishing)';
