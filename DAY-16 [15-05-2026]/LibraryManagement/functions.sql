-- ============================================================
-- Library Management System - Functions
-- These are pure read/compute operations with no side effects.
-- Call with: SELECT * FROM fn_...() or SELECT fn_...()
-- Run this script in psql or pgAdmin against the LibraryManagement DB
-- ============================================================


-- ============================================================
-- Function: calculate_member_fine
-- Computes total unpaid fine amount for a member.
-- Used internally by proc_borrow_book.
-- ============================================================
CREATE OR REPLACE FUNCTION calculate_member_fine(p_member_id INT)
RETURNS NUMERIC
LANGUAGE plpgsql
AS $$
DECLARE
    v_total NUMERIC;
BEGIN
    SELECT COALESCE(SUM(f."Amount"), 0)
    INTO v_total
    FROM "Fines" f
    WHERE f."MemberId" = p_member_id AND f."IsPaid" = false;

    RETURN v_total;
END;
$$;

-- SELECT calculate_member_fine(1);


-- ============================================================
-- Function: get_member_borrowing_summary
-- Returns a summary of active borrowings, returned books,
-- overdue count, and total unpaid fine for a given member.
-- ============================================================
CREATE OR REPLACE FUNCTION get_member_borrowing_summary(p_member_id INT)
RETURNS TABLE (
    active_borrowings   BIGINT,
    returned_books      BIGINT,
    overdue_books       BIGINT,
    total_unpaid_fine   NUMERIC
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        COUNT(*) FILTER (WHERE b."Status" = 'Borrowed')                                          AS active_borrowings,
        COUNT(*) FILTER (WHERE b."Status" = 'Returned')                                          AS returned_books,
        COUNT(*) FILTER (WHERE b."Status" = 'Borrowed' AND b."DueDate" < NOW() AT TIME ZONE 'UTC') AS overdue_books,
        COALESCE(calculate_member_fine(p_member_id), 0)                                          AS total_unpaid_fine
    FROM "Borrowings" b
    WHERE b."MemberId" = p_member_id;
END;
$$;

-- SELECT * FROM get_member_borrowing_summary(1);


-- ============================================================
-- REPORT FUNCTIONS
-- Pure read queries with multi-table joins.
-- All return tabular results; no DML.
-- ============================================================


-- ============================================================
-- fn_get_member_borrowing_history
-- Returns full borrowing history for a member, newest first.
-- Joins: Borrowings → BookCopies → Books → Members
-- ============================================================
CREATE OR REPLACE FUNCTION fn_get_member_borrowing_history(p_member_id INT)
RETURNS TABLE (
    "Id"         INT,
    "MemberId"   INT,
    "BookCopyId" INT,
    "BorrowDate" TIMESTAMPTZ,
    "DueDate"    TIMESTAMPTZ,
    "ReturnDate" TIMESTAMPTZ,
    "Status"     TEXT
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        b."Id",
        b."MemberId",
        b."BookCopyId",
        b."BorrowDate",
        b."DueDate",
        b."ReturnDate",
        b."Status"
    FROM "Borrowings" b
    JOIN "BookCopies" bc ON bc."Id" = b."BookCopyId"
    JOIN "Books" bk      ON bk."Id" = bc."BookId"
    JOIN "Members" m     ON m."Id"  = b."MemberId"
    WHERE b."MemberId" = p_member_id
    ORDER BY b."BorrowDate" DESC;
END;
$$;

-- SELECT * FROM fn_get_member_borrowing_history(1);


-- ============================================================
-- fn_get_currently_borrowed_books
-- Returns all active borrowings (Status = 'Borrowed'),
-- ordered by due date ascending.
-- Joins: Borrowings → BookCopies → Books → Members
-- ============================================================
CREATE OR REPLACE FUNCTION fn_get_currently_borrowed_books()
RETURNS TABLE (
    "Id"         INT,
    "MemberId"   INT,
    "BookCopyId" INT,
    "BorrowDate" TIMESTAMPTZ,
    "DueDate"    TIMESTAMPTZ,
    "ReturnDate" TIMESTAMPTZ,
    "Status"     TEXT
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        b."Id",
        b."MemberId",
        b."BookCopyId",
        b."BorrowDate",
        b."DueDate",
        b."ReturnDate",
        b."Status"
    FROM "Borrowings" b
    JOIN "BookCopies" bc ON bc."Id" = b."BookCopyId"
    JOIN "Books" bk      ON bk."Id" = bc."BookId"
    JOIN "Members" m     ON m."Id"  = b."MemberId"
    WHERE b."Status" = 'Borrowed'
    ORDER BY b."DueDate" ASC;
END;
$$;

-- SELECT * FROM fn_get_currently_borrowed_books();


-- ============================================================
-- fn_get_overdue_books
-- Returns all active borrowings where DueDate has passed.
-- Joins: Borrowings → BookCopies → Books → Members
-- ============================================================
CREATE OR REPLACE FUNCTION fn_get_overdue_books()
RETURNS TABLE (
    "Id"         INT,
    "MemberId"   INT,
    "BookCopyId" INT,
    "BorrowDate" TIMESTAMPTZ,
    "DueDate"    TIMESTAMPTZ,
    "ReturnDate" TIMESTAMPTZ,
    "Status"     TEXT
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        b."Id",
        b."MemberId",
        b."BookCopyId",
        b."BorrowDate",
        b."DueDate",
        b."ReturnDate",
        b."Status"
    FROM "Borrowings" b
    JOIN "BookCopies" bc ON bc."Id" = b."BookCopyId"
    JOIN "Books" bk      ON bk."Id" = bc."BookId"
    JOIN "Members" m     ON m."Id"  = b."MemberId"
    WHERE b."Status" = 'Borrowed'
      AND b."DueDate" < (NOW() AT TIME ZONE 'UTC')
    ORDER BY b."DueDate" ASC;
END;
$$;

-- SELECT * FROM fn_get_overdue_books();


-- ============================================================
-- fn_get_members_with_pending_fines
-- Returns all members who have at least one unpaid fine.
-- Joins: Members → MembershipTypes → Fines
-- ============================================================
CREATE OR REPLACE FUNCTION fn_get_members_with_pending_fines()
RETURNS TABLE (
    "Id"               INT,
    "FirstName"        TEXT,
    "LastName"         TEXT,
    "Phone"            TEXT,
    "Email"            TEXT,
    "Password"         TEXT,
    "MembershipTypeId" INT,
    "IsActive"         BOOLEAN,
    "IsAdmin"          BOOLEAN,
    "JoinDate"         TIMESTAMPTZ
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT DISTINCT
        m."Id",
        m."FirstName",
        m."LastName",
        m."Phone",
        m."Email",
        m."Password",
        m."MembershipTypeId",
        m."IsActive",
        m."IsAdmin",
        m."JoinDate"
    FROM "Members" m
    JOIN "MembershipTypes" mt ON mt."Id" = m."MembershipTypeId"
    JOIN "Fines" f            ON f."MemberId" = m."Id"
    WHERE f."IsPaid" = false
    ORDER BY m."Id";
END;
$$;

-- SELECT * FROM fn_get_members_with_pending_fines();


-- ============================================================
-- fn_get_most_borrowed_books
-- Returns books ranked by total borrow count, descending.
-- Joins: Books → BookCategories → BookCopies → Borrowings
-- ============================================================
CREATE OR REPLACE FUNCTION fn_get_most_borrowed_books()
RETURNS TABLE (
    "Id"              INT,
    "Title"           TEXT,
    "Author"          TEXT,
    "ISBN"            TEXT,
    "BookCategoryId"  INT,
    "PublicationYear" INT,
    "BorrowCount"     BIGINT
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        bk."Id",
        bk."Title",
        bk."Author",
        bk."ISBN",
        bk."BookCategoryId",
        bk."PublicationYear",
        COUNT(br."Id") AS "BorrowCount"
    FROM "Books" bk
    JOIN "BookCategories" cat ON cat."Id" = bk."BookCategoryId"
    JOIN "BookCopies" bc      ON bc."BookId" = bk."Id"
    JOIN "Borrowings" br      ON br."BookCopyId" = bc."Id"
    GROUP BY bk."Id", bk."Title", bk."Author", bk."ISBN", bk."BookCategoryId", bk."PublicationYear"
    HAVING COUNT(br."Id") > 0
    ORDER BY "BorrowCount" DESC;
END;
$$;

-- SELECT * FROM fn_get_most_borrowed_books();


-- ============================================================
-- fn_get_available_books_by_category
-- Returns flat (CategoryId, BookId) pairs where the book has
-- at least one Available copy. Grouped into tuples in C#.
-- Joins: BookCategories → Books → BookCopies
-- ============================================================
CREATE OR REPLACE FUNCTION fn_get_available_books_by_category()
RETURNS TABLE (
    "CategoryId"      INT,
    "CategoryName"    TEXT,
    "BookId"          INT,
    "Title"           TEXT,
    "Author"          TEXT,
    "ISBN"            TEXT,
    "BookCategoryId"  INT,
    "PublicationYear" INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT DISTINCT
        cat."Id"              AS "CategoryId",
        cat."Name"            AS "CategoryName",
        bk."Id"               AS "BookId",
        bk."Title",
        bk."Author",
        bk."ISBN",
        bk."BookCategoryId",
        bk."PublicationYear"
    FROM "BookCategories" cat
    JOIN "Books" bk      ON bk."BookCategoryId" = cat."Id"
    JOIN "BookCopies" bc ON bc."BookId" = bk."Id"
    WHERE bc."Status" = 'Available'
    ORDER BY cat."Name", bk."Title";
END;
$$;

-- SELECT * FROM fn_get_available_books_by_category();
