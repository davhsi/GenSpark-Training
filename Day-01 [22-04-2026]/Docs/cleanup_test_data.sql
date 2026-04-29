-- ======================
-- CLEANUP TEST DATA SCRIPT
-- ======================
-- This script removes all test data while preserving admin credentials
-- Run this script to wipe out test bus operators, customers, and buses
-- Only keeps admin data and platform config

-- Start transaction for safety
BEGIN;

-- Display what will be deleted (for verification)
DO $$
DECLARE
    customer_count INTEGER;
    operator_count INTEGER;
    bus_count INTEGER;
    route_count INTEGER;
    booking_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO customer_count FROM customer;
    SELECT COUNT(*) INTO operator_count FROM bus_operator;
    SELECT COUNT(*) INTO bus_count FROM bus;
    SELECT COUNT(*) INTO route_count FROM route;
    SELECT COUNT(*) INTO booking_count FROM booking;
    
    RAISE NOTICE 'About to delete: % customers, % operators, % buses, % routes, % bookings', 
                 customer_count, operator_count, bus_count, route_count, booking_count;
END $$;

-- ======================
-- DELETE IN DEPENDENCY ORDER (bottom-up)
-- ======================

-- 1. Delete audit logs (except admin actions)
DELETE FROM audit_log WHERE actor_role != 'admin';

-- 2. Delete notifications (customer and operator notifications)
DELETE FROM notification WHERE customer_id IS NOT NULL OR operator_id IS NOT NULL;

-- 3. Delete seat locks
DELETE FROM seat_lock;

-- 4. Delete booked seats
DELETE FROM booked_seat;

-- 5. Delete payments
DELETE FROM payment;

-- 6. Delete cancellations
DELETE FROM cancellation;

-- 7. Delete coupons
DELETE FROM coupon;

-- 8. Delete bookings
DELETE FROM booking;

-- 9. Delete bus operating days
DELETE FROM bus_operating_days;

-- 10. Delete bus stops
DELETE FROM bus_stop;

-- 11. Delete seats (will cascade from layouts)
DELETE FROM seat;

-- 12. Delete buses
DELETE FROM bus;

-- 13. Delete bus layouts
DELETE FROM bus_layout;

-- 14. Delete routes (keep only if you want to preserve them)
-- Uncomment the line below if you want to keep routes
-- DELETE FROM route WHERE created_by_admin IS NULL;
DELETE FROM route; -- This removes all routes including admin-created ones

-- 15. Delete bus operators
DELETE FROM bus_operator;

-- 16. Delete customers
DELETE FROM customer;

-- 17. Delete T&C versions (except active ones)
DELETE FROM tc_version WHERE is_active = FALSE;

-- ======================
-- PRESERVED DATA (NOT DELETED)
-- ======================
-- - admin table (all admin records preserved)
-- - platform_config table (all config preserved)
-- - Active T&C versions (preserved)

-- ======================
-- RESET SEQUENCES (if any)
-- ======================
-- Note: UUID columns don't use sequences, so no reset needed

-- Commit the transaction
COMMIT;

-- ======================
-- VERIFICATION
-- ======================
DO $$
DECLARE
    remaining_customers INTEGER;
    remaining_operators INTEGER;
    remaining_buses INTEGER;
    remaining_bookings INTEGER;
    admin_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO remaining_customers FROM customer;
    SELECT COUNT(*) INTO remaining_operators FROM bus_operator;
    SELECT COUNT(*) INTO remaining_buses FROM bus;
    SELECT COUNT(*) INTO remaining_bookings FROM booking;
    SELECT COUNT(*) INTO admin_count FROM admin;
    
    RAISE NOTICE 'Cleanup completed. Remaining records:';
    RAISE NOTICE '- Admins: % (should be > 0)', admin_count;
    RAISE NOTICE '- Customers: % (should be 0)', remaining_customers;
    RAISE NOTICE '- Operators: % (should be 0)', remaining_operators;
    RAISE NOTICE '- Buses: % (should be 0)', remaining_buses;
    RAISE NOTICE '- Bookings: % (should be 0)', remaining_bookings;
END $$;
