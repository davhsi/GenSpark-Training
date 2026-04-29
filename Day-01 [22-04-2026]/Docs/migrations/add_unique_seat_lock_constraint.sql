-- Add unique constraint to prevent double locking of same seat on same date
-- This is the critical safety net that prevents race conditions

CREATE UNIQUE INDEX unique_active_seat_lock
ON seat_lock(seat_id, journey_date)
WHERE is_active = true;

-- Add comment for documentation
COMMENT ON INDEX unique_active_seat_lock IS 'Prevents multiple active locks on same seat for same journey date';
