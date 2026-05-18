-- =============================================================
--  LIBRARY MANAGEMENT SYSTEM — TEST SEED DATA
--  Run this script any time you need a fresh test state.
--
--  All member passwords are: Test@123
--  SHA-256("Test@123") = 8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e
--
--  Test accounts:
--    alice@example.com  — Basic,   active,  unpaid fine Rs.40
--    bob@example.com    — Student, active,  2 overdue borrowings
--    carol@example.com  — Premium, active,  clean record
--    david@example.com  — Student, INACTIVE (login blocked)
--    eve@example.com    — Basic,   active,  BORROWING BLOCKED (Rs.550 fine > Rs.500)
--    admin@example.com  — Admin
--
--  Demo scenarios:
--    Return Bob's overdue books  → fn_return_book generates fines automatically
--    Try borrowing as Eve        → blocked (unpaid fine > Rs.500)
--    Try logging in as David     → blocked (inactive account)
--    Admin Reports               → all 6 reports have meaningful data
--    Admin Mark Copy Damaged     → try ACC-0003 (copy ID 3)
--    Admin Search Member         → search '9876' or 'alice'
-- =============================================================


-- ── 1. WIPE (order respects foreign keys) ────────────────────
TRUNCATE TABLE "Fines"       RESTART IDENTITY CASCADE;
TRUNCATE TABLE "Borrowings"  RESTART IDENTITY CASCADE;
TRUNCATE TABLE "BookCopies"  RESTART IDENTITY CASCADE;
TRUNCATE TABLE "Books"       RESTART IDENTITY CASCADE;
TRUNCATE TABLE "Members"     RESTART IDENTITY CASCADE;

-- MembershipTypes and BookCategories are EF-seeded; leave them.
-- MembershipTypeId: 1=Basic (2 books/7 days), 2=Student (3 books/10 days), 3=Premium (5 books/15 days)
-- BookCategoryId:   1=Fiction, 2=Non-Fiction, 3=Science, 4=History, 5=Biography


-- ── 2. MEMBERS ───────────────────────────────────────────────
INSERT INTO "Members" ("FirstName","LastName","Phone","Email","Password","MembershipTypeId","IsActive","IsAdmin","JoinDate")
VALUES
  -- [1] Alice — Basic, active, will have an unpaid fine (Rs.40 < Rs.500, can still borrow)
  ('Alice', 'Johnson', '9876543210', 'alice@example.com',
   '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 1, true,  false, NOW() - INTERVAL '90 days'),

  -- [2] Bob — Student, active, 2 active OVERDUE borrowings + 1 paid fine + 1 unpaid fine
  ('Bob', 'Smith', '9123456780', 'bob@example.com',
   '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 2, true,  false, NOW() - INTERVAL '60 days'),

  -- [3] Carol — Premium, active, clean record (good demo of on-time returns)
  ('Carol', 'Patel', '9000011112', 'carol@example.com',
   '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 3, true,  false, NOW() - INTERVAL '30 days'),

  -- [4] David — Student, INACTIVE → tests blocked login
  ('David', 'Lee', '9111122223', 'david@example.com',
   '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 2, false, false, NOW() - INTERVAL '120 days'),

  -- [5] Eve — Basic, active, unpaid fine Rs.550 → BORROWING BLOCKED (> Rs.500 threshold)
  ('Eve', 'Sharma', '9222233334', 'eve@example.com',
   '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 1, true,  false, NOW() - INTERVAL '180 days'),

  -- [6] Admin
  ('Admin', 'User', '0000000000', 'admin@example.com',
   '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 3, true,  true,  NOW());


-- ── 3. BOOKS ─────────────────────────────────────────────────
INSERT INTO "Books" ("Title","Author","ISBN","BookCategoryId","PublicationYear")
VALUES
  -- Fiction (CategoryId=1)
  ('The Great Gatsby',          'F. Scott Fitzgerald', '978-0743273565', 1, 1925),  -- BookId=1
  ('To Kill a Mockingbird',     'Harper Lee',          '978-0061935466', 1, 1960),  -- BookId=2
  -- Science (CategoryId=3)
  ('A Brief History of Time',   'Stephen Hawking',     '978-0553380163', 3, 1988),  -- BookId=3
  -- Non-Fiction (CategoryId=2)
  ('Sapiens',                   'Yuval Noah Harari',   '978-0062316110', 2, 2011),  -- BookId=4
  -- Biography (CategoryId=5)
  ('The Diary of a Young Girl', 'Anne Frank',          '978-0553296983', 5, 1947),  -- BookId=5
  -- Fiction (CategoryId=1)
  ('1984',                      'George Orwell',       '978-0451524935', 1, 1949),  -- BookId=6
  -- Science (CategoryId=3)
  ('Cosmos',                    'Carl Sagan',          '978-0345331359', 3, 1980),  -- BookId=7
  -- History (CategoryId=4)
  ('Guns, Germs, and Steel',    'Jared Diamond',       '978-0393354324', 4, 1997);  -- BookId=8


-- ── 4. BOOK COPIES ───────────────────────────────────────────
INSERT INTO "BookCopies" ("BookId","AccessionNumber","Condition","Status")
VALUES
  -- The Great Gatsby (BookId=1)
  (1, 'ACC-0001', 'Good',    'Available'),    -- CopyId=1
  (1, 'ACC-0002', 'Good',    'Borrowed'),     -- CopyId=2  ← Alice active borrowing

  -- To Kill a Mockingbird (BookId=2)
  (2, 'ACC-0003', 'Good',    'Available'),    -- CopyId=3  ← demo: mark this damaged
  (2, 'ACC-0004', 'Damaged', 'Unavailable'),  -- CopyId=4  ← already damaged

  -- A Brief History of Time (BookId=3)
  (3, 'ACC-0005', 'Good',    'Available'),    -- CopyId=5
  (3, 'ACC-0006', 'Good',    'Borrowed'),     -- CopyId=6  ← Bob overdue

  -- Sapiens (BookId=4)
  (4, 'ACC-0007', 'Good',    'Available'),    -- CopyId=7
  (4, 'ACC-0008', 'Good',    'Available'),    -- CopyId=8

  -- The Diary of a Young Girl (BookId=5)
  (5, 'ACC-0009', 'Good',    'Borrowed'),     -- CopyId=9  ← Carol active borrowing

  -- 1984 (BookId=6)
  (6, 'ACC-0010', 'Good',    'Available'),    -- CopyId=10
  (6, 'ACC-0011', 'Good',    'Available'),    -- CopyId=11

  -- Cosmos (BookId=7)
  (7, 'ACC-0012', 'Good',    'Available'),    -- CopyId=12

  -- Guns, Germs, and Steel (BookId=8)
  (8, 'ACC-0013', 'Good',    'Available'),    -- CopyId=13
  (8, 'ACC-0014', 'Good',    'Borrowed');     -- CopyId=14 ← Bob overdue


-- ── 5. BORROWINGS ────────────────────────────────────────────

-- (a) Active borrowings (Status = Borrowed)
INSERT INTO "Borrowings" ("MemberId","BookCopyId","BorrowDate","DueDate","ReturnDate","Status")
VALUES
  -- BorrowingId=1: Alice borrowed Gatsby copy 2 — on time (Basic: 2 books / 7 days)
  (1, 2,  NOW() - INTERVAL '3 days',  NOW() + INTERVAL '4 days',  NULL, 'Borrowed'),

  -- BorrowingId=2: Bob borrowed Hawking copy 2 — OVERDUE by 5 days (Student: 3 books / 10 days)
  (2, 6,  NOW() - INTERVAL '15 days', NOW() - INTERVAL '5 days',  NULL, 'Borrowed'),

  -- BorrowingId=3: Carol borrowed Anne Frank — on time (Premium: 5 books / 15 days)
  (3, 9,  NOW() - INTERVAL '5 days',  NOW() + INTERVAL '10 days', NULL, 'Borrowed'),

  -- BorrowingId=4: Bob borrowed Guns Germs copy 2 — OVERDUE by 2 days
  (2, 14, NOW() - INTERVAL '12 days', NOW() - INTERVAL '2 days',  NULL, 'Borrowed');


-- (b) Returned borrowings (historical — makes Reports meaningful)
INSERT INTO "Borrowings" ("MemberId","BookCopyId","BorrowDate","DueDate","ReturnDate","Status")
VALUES
  -- BorrowingId=5: Alice returned Mockingbird on time
  (1, 3,  NOW() - INTERVAL '30 days', NOW() - INTERVAL '16 days', NOW() - INTERVAL '18 days', 'Returned'),

  -- BorrowingId=6: Bob returned Sapiens 3 days late → paid fine below
  (2, 7,  NOW() - INTERVAL '45 days', NOW() - INTERVAL '31 days', NOW() - INTERVAL '28 days', 'Returned'),

  -- BorrowingId=7: Carol returned 1984 on time
  (3, 10, NOW() - INTERVAL '20 days', NOW() - INTERVAL '10 days', NOW() - INTERVAL '12 days', 'Returned'),

  -- BorrowingId=8: Alice returned Cosmos 4 days late → unpaid fine below
  (1, 12, NOW() - INTERVAL '50 days', NOW() - INTERVAL '40 days', NOW() - INTERVAL '36 days', 'Returned'),

  -- BorrowingId=9: Carol returned Gatsby on time (boosts Most Borrowed count for Gatsby)
  (3, 1,  NOW() - INTERVAL '60 days', NOW() - INTERVAL '46 days', NOW() - INTERVAL '48 days', 'Returned'),

  -- BorrowingId=10: Eve returned 1984 copy 2 — 55 days late → large unpaid fine below
  (5, 11, NOW() - INTERVAL '80 days', NOW() - INTERVAL '70 days', NOW() - INTERVAL '15 days', 'Returned');


-- ── 6. FINES ─────────────────────────────────────────────────
INSERT INTO "Fines" ("MemberId","BorrowingId","Amount","IssuedDate","PaidDate","IsPaid")
VALUES
  -- Bob's PAID fine: 3 days late × Rs.10 = Rs.30
  (2, 6, 30.00, NOW() - INTERVAL '28 days', NOW() - INTERVAL '25 days', true),

  -- Alice's UNPAID fine: 4 days late × Rs.10 = Rs.40
  -- → Alice appears in "Members with Pending Fines" report
  -- → Alice can still borrow (Rs.40 < Rs.500 threshold)
  (1, 8, 40.00, NOW() - INTERVAL '36 days', NULL, false),

  -- Eve's UNPAID fine: 55 days late × Rs.10 = Rs.550
  -- → Eve appears in "Members with Pending Fines" report
  -- → Eve is BLOCKED from borrowing (Rs.550 > Rs.500 threshold)
  (5, 10, 550.00, NOW() - INTERVAL '15 days', NULL, false);

-- Note: Bob's 2 active overdue borrowings (IDs 2 & 4) have NO fines yet.
-- They are created automatically by fn_return_book when returned.
-- This lets you demo the fine-generation flow live.


-- ── VERIFICATION QUERIES ─────────────────────────────────────
-- Run these manually after seeding to confirm data:

-- Members overview:
-- SELECT m."Id", m."FirstName", m."LastName", mt."Name" AS membership, m."IsActive", m."IsAdmin"
-- FROM "Members" m JOIN "MembershipTypes" mt ON m."MembershipTypeId" = mt."Id";

-- Active borrowings with overdue flag:
-- SELECT b."Id", m."FirstName", bk."Title", b."DueDate",
--        CASE WHEN b."DueDate" < NOW() THEN 'OVERDUE' ELSE 'On Time' END AS status
-- FROM "Borrowings" b
-- JOIN "Members" m ON b."MemberId" = m."Id"
-- JOIN "BookCopies" cp ON b."BookCopyId" = cp."Id"
-- JOIN "Books" bk ON cp."BookId" = bk."Id"
-- WHERE b."Status" = 'Borrowed';

-- Pending fines with block check:
-- SELECT m."FirstName", f."Amount",
--        CASE WHEN f."Amount" > 500 THEN 'BLOCKED' ELSE 'OK' END AS borrow_status
-- FROM "Fines" f JOIN "Members" m ON f."MemberId" = m."Id"
-- WHERE f."IsPaid" = false;

-- Test stored procedures:
-- SELECT calculate_member_fine(1);           -- Alice: should return 40
-- SELECT calculate_member_fine(5);           -- Eve:   should return 550
-- SELECT * FROM get_available_books_by_category(1);   -- Fiction books
-- SELECT * FROM get_member_borrowing_summary(2);      -- Bob's summary
