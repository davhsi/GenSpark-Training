-- ============================================================
-- Library Management System - Stored Procedures
-- These handle transactional write operations with DML side effects.
-- Call with: CALL proc_borrow_book(...) / CALL proc_return_book(...)
-- Run this script in psql or pgAdmin against the LibraryManagement DB
-- ============================================================


-- ============================================================
-- PROCEDURE 1: proc_borrow_book
-- Handles the full borrow transaction atomically:
--   1. Validates member is active
--   2. Checks unpaid fines < 500
--   3. Checks borrowing limit not exceeded
--   4. Validates book copy is Available
--   5. Checks member doesn't already have this book
--   6. INSERTs Borrowing record
--   7. UPDATEs BookCopy status to 'Borrowed'
-- Returns result message via OUT parameter.
-- ============================================================
CREATE OR REPLACE PROCEDURE proc_borrow_book(
    p_member_id  INT,
    p_copy_id    INT,
    OUT p_result TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_member        RECORD;
    v_copy          RECORD;
    v_active_count  INT;
    v_unpaid_fine   NUMERIC;
    v_due_date      TIMESTAMPTZ;
BEGIN
    -- 1. Get member with membership type details
    SELECT m."Id", m."IsActive", mt."MaxBorrowings", mt."MaxBorrowDays"
    INTO v_member
    FROM "Members" m
    JOIN "MembershipTypes" mt ON m."MembershipTypeId" = mt."Id"
    WHERE m."Id" = p_member_id;

    IF NOT FOUND THEN
        p_result := 'Member not found.';
        RETURN;
    END IF;

    IF NOT v_member."IsActive" THEN
        p_result := 'Member account is inactive.';
        RETURN;
    END IF;

    -- 2. Check unpaid fines
    SELECT calculate_member_fine(p_member_id) INTO v_unpaid_fine;
    IF v_unpaid_fine > 500 THEN
        p_result := FORMAT('Borrowing blocked. Unpaid fines: Rs.%s', v_unpaid_fine);
        RETURN;
    END IF;

    -- 3. Check borrowing limit
    SELECT COUNT(*) INTO v_active_count
    FROM "Borrowings"
    WHERE "MemberId" = p_member_id AND "Status" = 'Borrowed';

    IF v_active_count >= v_member."MaxBorrowings" THEN
        p_result := 'Borrowing limit reached for your membership type.';
        RETURN;
    END IF;

    -- 4. Get book copy and validate status
    SELECT bc."Id", bc."Status", bc."BookId"
    INTO v_copy
    FROM "BookCopies" bc
    WHERE bc."Id" = p_copy_id;

    IF NOT FOUND THEN
        p_result := 'Book copy not found.';
        RETURN;
    END IF;

    IF v_copy."Status" != 'Available' THEN
        p_result := 'Book copy is not available.';
        RETURN;
    END IF;

    -- 5. Check member does not already have this book borrowed
    IF EXISTS (
        SELECT 1
        FROM "Borrowings" b
        JOIN "BookCopies" bc ON b."BookCopyId" = bc."Id"
        WHERE b."MemberId" = p_member_id
          AND bc."BookId" = v_copy."BookId"
          AND b."Status" = 'Borrowed'
    ) THEN
        p_result := 'You already have an active borrowing for this book.';
        RETURN;
    END IF;

    -- 6. Calculate due date
    v_due_date := (NOW() AT TIME ZONE 'UTC') + (v_member."MaxBorrowDays" || ' days')::INTERVAL;

    -- 7. Insert borrowing record
    INSERT INTO "Borrowings" ("MemberId", "BookCopyId", "BorrowDate", "DueDate", "Status")
    VALUES (p_member_id, p_copy_id, NOW() AT TIME ZONE 'UTC', v_due_date, 'Borrowed');

    -- 8. Mark book copy as borrowed
    UPDATE "BookCopies"
    SET "Status" = 'Borrowed'
    WHERE "Id" = p_copy_id;

    p_result := 'Book borrowed successfully.';

EXCEPTION WHEN OTHERS THEN
    ROLLBACK;
    p_result := FORMAT('Error: %s', SQLERRM);
END;
$$;

-- CALL proc_borrow_book(1, 1, NULL);


-- ============================================================
-- PROCEDURE 2: proc_return_book
-- Handles the full return transaction atomically:
--   1. Validates the borrowing exists and is active
--   2. UPDATEs Borrowing to 'Returned'
--   3. Checks copy condition — if Damaged, marks copy 'Unavailable'
--      and adds a flat Rs.500 damage fine on top of any overdue fine.
--      If not damaged, marks copy 'Available'.
--   4. Calculates overdue days and INSERTs a Fine if overdue
--   5. If both overdue AND damaged, issues a single combined fine.
-- Returns result message via OUT parameter.
-- ============================================================
CREATE OR REPLACE PROCEDURE proc_return_book(
    p_borrowing_id INT,
    p_is_damaged   BOOLEAN,   -- caller passes TRUE if the copy is being returned damaged
    OUT p_result   TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_borrowing      RECORD;
    v_delayed_days   INT;
    v_overdue_fine   NUMERIC := 0;
    v_damage_fine    NUMERIC := 0;
    v_total_fine     NUMERIC := 0;
    v_copy_status    TEXT;
BEGIN
    -- 1. Get the borrowing record
    SELECT b."Id", b."MemberId", b."BookCopyId", b."DueDate"
    INTO v_borrowing
    FROM "Borrowings" b
    WHERE b."Id" = p_borrowing_id AND b."Status" = 'Borrowed';

    IF NOT FOUND THEN
        p_result := 'Invalid or already returned borrowing.';
        RETURN;
    END IF;

    -- 2. Mark borrowing as returned
    UPDATE "Borrowings"
    SET "Status" = 'Returned',
        "ReturnDate" = NOW() AT TIME ZONE 'UTC'
    WHERE "Id" = p_borrowing_id;

    -- 3. Handle copy condition
    IF p_is_damaged THEN
        -- Damaged copy goes out of circulation
        UPDATE "BookCopies"
        SET "Status" = 'Unavailable', "Condition" = 'Damaged'
        WHERE "Id" = v_borrowing."BookCopyId";
        v_damage_fine := 500; -- flat Rs.500 damage penalty
    ELSE
        UPDATE "BookCopies"
        SET "Status" = 'Available'
        WHERE "Id" = v_borrowing."BookCopyId";
    END IF;

    -- 4. Calculate overdue fine
    v_delayed_days := GREATEST(0,
        EXTRACT(DAY FROM ((NOW() AT TIME ZONE 'UTC') - v_borrowing."DueDate"))::INT
    );
    IF v_delayed_days > 0 THEN
        v_overdue_fine := v_delayed_days * 10; -- Rs. 10 per day
    END IF;

    -- 5. Issue combined fine if any penalty applies
    v_total_fine := v_overdue_fine + v_damage_fine;

    IF v_total_fine > 0 THEN
        INSERT INTO "Fines" ("MemberId", "BorrowingId", "Amount", "IssuedDate", "IsPaid")
        VALUES (v_borrowing."MemberId", p_borrowing_id, v_total_fine, NOW() AT TIME ZONE 'UTC', false);

        IF p_is_damaged AND v_delayed_days > 0 THEN
            p_result := FORMAT(
                'Book returned damaged and overdue. Fine of Rs.%s issued (Rs.%s overdue + Rs.500 damage).',
                v_total_fine, v_overdue_fine
            );
        ELSIF p_is_damaged THEN
            p_result := FORMAT('Book returned damaged. Damage fine of Rs.500 issued.');
        ELSE
            p_result := FORMAT('Book returned. Fine of Rs.%s issued for %s overdue day(s).', v_overdue_fine, v_delayed_days);
        END IF;
        RETURN;
    END IF;

    p_result := 'Book returned successfully. No fines.';

EXCEPTION WHEN OTHERS THEN
    ROLLBACK;
    p_result := FORMAT('Error: %s', SQLERRM);
END;
$$;

-- CALL proc_return_book(1, false, NULL);
-- CALL proc_return_book(1, true,  NULL);  -- damaged return
