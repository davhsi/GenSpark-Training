using LibraryManagement.DAL.Contexts;
using LibraryManagement.Model.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagement.DAL
{
    /// <summary>
    /// Wipes all transactional tables and inserts a predictable set of test
    /// records so every test run starts from the same known state.
    ///
    /// All seed members share the password: Test@123
    ///
    /// Scenarios covered:
    ///   Alice   (Basic)    — 1 active borrowing (on time), 1 unpaid fine Rs.40, rich history
    ///   Bob     (Student)  — 2 active OVERDUE borrowings, 1 paid fine, borrowing history
    ///   Carol   (Premium)  — 1 active borrowing (on time), clean fine record
    ///   David   (Student)  — INACTIVE account (tests blocked login)
    ///   Eve     (Basic)    — unpaid fine Rs.550 > Rs.500 (tests borrowing block)
    ///   Admin   (Premium)  — admin account
    ///
    /// Usage — call once at app startup (e.g. from Program.cs):
    ///     DataSeeder.Seed();
    /// </summary>
    public static class DataSeeder
    {
        // SHA-256("Test@123")
        private static readonly string _testPasswordHash = HashPassword("Test@123");

        public static void Seed()
        {
            using var ctx = new LibraryContext();

            Console.WriteLine("[Seeder] Starting seed…");

            // ── 1. Wipe transactional data (FK order) ─────────
            ctx.Database.ExecuteSqlRaw("TRUNCATE TABLE \"Fines\"      RESTART IDENTITY CASCADE");
            ctx.Database.ExecuteSqlRaw("TRUNCATE TABLE \"Borrowings\" RESTART IDENTITY CASCADE");
            ctx.Database.ExecuteSqlRaw("TRUNCATE TABLE \"BookCopies\" RESTART IDENTITY CASCADE");
            ctx.Database.ExecuteSqlRaw("TRUNCATE TABLE \"Books\"      RESTART IDENTITY CASCADE");
            ctx.Database.ExecuteSqlRaw("TRUNCATE TABLE \"Members\"    RESTART IDENTITY CASCADE");

            // ── 2. Members ────────────────────────────────────
            // MembershipTypeId: 1=Basic (2 books/7 days), 2=Student (3 books/10 days), 3=Premium (5 books/15 days)
            var members = new[]
            {
                // [0] Alice — Basic, active, has unpaid fine (Rs.40 < Rs.500, can still borrow)
                new Member { FirstName = "Alice",  LastName = "Johnson", Phone = "9876543210", Email = "alice@example.com", Password = _testPasswordHash, MembershipTypeId = 1, IsActive = true,  IsAdmin = false, JoinDate = DateTime.UtcNow.AddDays(-90) },
                // [1] Bob — Student, active, 2 active OVERDUE borrowings + 1 paid fine + 1 unpaid fine
                new Member { FirstName = "Bob",    LastName = "Smith",   Phone = "9123456780", Email = "bob@example.com",   Password = _testPasswordHash, MembershipTypeId = 2, IsActive = true,  IsAdmin = false, JoinDate = DateTime.UtcNow.AddDays(-60) },
                // [2] Carol — Premium, active, clean record
                new Member { FirstName = "Carol",  LastName = "Patel",   Phone = "9000011112", Email = "carol@example.com", Password = _testPasswordHash, MembershipTypeId = 3, IsActive = true,  IsAdmin = false, JoinDate = DateTime.UtcNow.AddDays(-30) },
                // [3] David — Student, INACTIVE → tests blocked login
                new Member { FirstName = "David",  LastName = "Lee",     Phone = "9111122223", Email = "david@example.com", Password = _testPasswordHash, MembershipTypeId = 2, IsActive = false, IsAdmin = false, JoinDate = DateTime.UtcNow.AddDays(-120) },
                // [4] Eve — Basic, active, unpaid fines > Rs.500 → tests borrowing block
                new Member { FirstName = "Eve",    LastName = "Sharma",  Phone = "9222233334", Email = "eve@example.com",   Password = _testPasswordHash, MembershipTypeId = 1, IsActive = true,  IsAdmin = false, JoinDate = DateTime.UtcNow.AddDays(-180) },
                // [5] Admin
                new Member { FirstName = "Admin",  LastName = "User",    Phone = "0000000000", Email = "admin@example.com", Password = _testPasswordHash, MembershipTypeId = 3, IsActive = true,  IsAdmin = true,  JoinDate = DateTime.UtcNow },
            };
            ctx.Members.AddRange(members);
            ctx.SaveChanges();

            // ── 3. Books ──────────────────────────────────────
            // BookCategoryId: 1=Fiction, 2=Non-Fiction, 3=Science, 4=History, 5=Biography
            var books = new[]
            {
                // [0] Fiction
                new Book { Title = "The Great Gatsby",          Author = "F. Scott Fitzgerald", ISBN = "978-0743273565", BookCategoryId = 1, PublicationYear = 1925 },
                // [1] Fiction
                new Book { Title = "To Kill a Mockingbird",     Author = "Harper Lee",          ISBN = "978-0061935466", BookCategoryId = 1, PublicationYear = 1960 },
                // [2] Science
                new Book { Title = "A Brief History of Time",   Author = "Stephen Hawking",     ISBN = "978-0553380163", BookCategoryId = 3, PublicationYear = 1988 },
                // [3] Non-Fiction
                new Book { Title = "Sapiens",                   Author = "Yuval Noah Harari",   ISBN = "978-0062316110", BookCategoryId = 2, PublicationYear = 2011 },
                // [4] Biography
                new Book { Title = "The Diary of a Young Girl", Author = "Anne Frank",          ISBN = "978-0553296983", BookCategoryId = 5, PublicationYear = 1947 },
                // [5] Fiction
                new Book { Title = "1984",                      Author = "George Orwell",       ISBN = "978-0451524935", BookCategoryId = 1, PublicationYear = 1949 },
                // [6] Science
                new Book { Title = "Cosmos",                    Author = "Carl Sagan",          ISBN = "978-0345331359", BookCategoryId = 3, PublicationYear = 1980 },
                // [7] History
                new Book { Title = "Guns, Germs, and Steel",    Author = "Jared Diamond",       ISBN = "978-0393354324", BookCategoryId = 4, PublicationYear = 1997 },
            };
            ctx.Books.AddRange(books);
            ctx.SaveChanges();

            // ── 4. Book Copies ─────────────────────────────────
            var copies = new[]
            {
                // The Great Gatsby — 2 copies
                /* [0]  */ new BookCopy { BookId = books[0].Id, AccessionNumber = "ACC-0001", Condition = "Good",    Status = "Available"   },
                /* [1]  */ new BookCopy { BookId = books[0].Id, AccessionNumber = "ACC-0002", Condition = "Good",    Status = "Borrowed"    }, // Alice active

                // To Kill a Mockingbird — 2 copies (1 damaged → demo Mark Damaged feature)
                /* [2]  */ new BookCopy { BookId = books[1].Id, AccessionNumber = "ACC-0003", Condition = "Good",    Status = "Available"   },
                /* [3]  */ new BookCopy { BookId = books[1].Id, AccessionNumber = "ACC-0004", Condition = "Damaged", Status = "Unavailable" },

                // A Brief History of Time — 2 copies
                /* [4]  */ new BookCopy { BookId = books[2].Id, AccessionNumber = "ACC-0005", Condition = "Good",    Status = "Available"   },
                /* [5]  */ new BookCopy { BookId = books[2].Id, AccessionNumber = "ACC-0006", Condition = "Good",    Status = "Borrowed"    }, // Bob overdue

                // Sapiens — 2 copies
                /* [6]  */ new BookCopy { BookId = books[3].Id, AccessionNumber = "ACC-0007", Condition = "Good",    Status = "Available"   },
                /* [7]  */ new BookCopy { BookId = books[3].Id, AccessionNumber = "ACC-0008", Condition = "Good",    Status = "Available"   },

                // The Diary of a Young Girl — 1 copy
                /* [8]  */ new BookCopy { BookId = books[4].Id, AccessionNumber = "ACC-0009", Condition = "Good",    Status = "Borrowed"    }, // Carol active

                // 1984 — 2 copies
                /* [9]  */ new BookCopy { BookId = books[5].Id, AccessionNumber = "ACC-0010", Condition = "Good",    Status = "Available"   },
                /* [10] */ new BookCopy { BookId = books[5].Id, AccessionNumber = "ACC-0011", Condition = "Good",    Status = "Available"   },

                // Cosmos — 1 copy
                /* [11] */ new BookCopy { BookId = books[6].Id, AccessionNumber = "ACC-0012", Condition = "Good",    Status = "Available"   },

                // Guns, Germs, and Steel — 2 copies
                /* [12] */ new BookCopy { BookId = books[7].Id, AccessionNumber = "ACC-0013", Condition = "Good",    Status = "Available"   },
                /* [13] */ new BookCopy { BookId = books[7].Id, AccessionNumber = "ACC-0014", Condition = "Good",    Status = "Borrowed"    }, // Bob overdue
            };
            ctx.BookCopies.AddRange(copies);
            ctx.SaveChanges();

            // ── 5. Borrowings ─────────────────────────────────

            // (a) Active borrowings
            var activeBorrowings = new[]
            {
                // Alice — on time (Basic: 2 books / 7 days max)
                /* [0] */ new Borrowing { MemberId = members[0].Id, BookCopyId = copies[1].Id,  BorrowDate = DateTime.UtcNow.AddDays(-3),  DueDate = DateTime.UtcNow.AddDays(4),  Status = "Borrowed" },

                // Bob — OVERDUE by 5 days (Student: 3 books / 10 days max) → fn_return_book will generate fine
                /* [1] */ new Borrowing { MemberId = members[1].Id, BookCopyId = copies[5].Id,  BorrowDate = DateTime.UtcNow.AddDays(-15), DueDate = DateTime.UtcNow.AddDays(-5), Status = "Borrowed" },

                // Carol — on time (Premium: 5 books / 15 days max)
                /* [2] */ new Borrowing { MemberId = members[2].Id, BookCopyId = copies[8].Id,  BorrowDate = DateTime.UtcNow.AddDays(-5),  DueDate = DateTime.UtcNow.AddDays(10), Status = "Borrowed" },

                // Bob — OVERDUE by 2 days → another fine scenario
                /* [3] */ new Borrowing { MemberId = members[1].Id, BookCopyId = copies[13].Id, BorrowDate = DateTime.UtcNow.AddDays(-12), DueDate = DateTime.UtcNow.AddDays(-2), Status = "Borrowed" },
            };
            ctx.Borrowings.AddRange(activeBorrowings);
            ctx.SaveChanges();

            // (b) Returned (historical) borrowings — makes Reports interesting
            var returnedBorrowings = new[]
            {
                // Alice returned on time
                /* [4] */ new Borrowing { MemberId = members[0].Id, BookCopyId = copies[2].Id,  BorrowDate = DateTime.UtcNow.AddDays(-30), DueDate = DateTime.UtcNow.AddDays(-16), ReturnDate = DateTime.UtcNow.AddDays(-18), Status = "Returned" },

                // Bob returned 3 days late → paid fine seeded below
                /* [5] */ new Borrowing { MemberId = members[1].Id, BookCopyId = copies[6].Id,  BorrowDate = DateTime.UtcNow.AddDays(-45), DueDate = DateTime.UtcNow.AddDays(-31), ReturnDate = DateTime.UtcNow.AddDays(-28), Status = "Returned" },

                // Carol returned on time
                /* [6] */ new Borrowing { MemberId = members[2].Id, BookCopyId = copies[9].Id,  BorrowDate = DateTime.UtcNow.AddDays(-20), DueDate = DateTime.UtcNow.AddDays(-10), ReturnDate = DateTime.UtcNow.AddDays(-12), Status = "Returned" },

                // Alice returned 4 days late → unpaid fine seeded below (shows in Pending Fines report)
                /* [7] */ new Borrowing { MemberId = members[0].Id, BookCopyId = copies[11].Id, BorrowDate = DateTime.UtcNow.AddDays(-50), DueDate = DateTime.UtcNow.AddDays(-40), ReturnDate = DateTime.UtcNow.AddDays(-36), Status = "Returned" },

                // Carol returned on time (second history entry — boosts Most Borrowed count)
                /* [8] */ new Borrowing { MemberId = members[2].Id, BookCopyId = copies[0].Id,  BorrowDate = DateTime.UtcNow.AddDays(-60), DueDate = DateTime.UtcNow.AddDays(-46), ReturnDate = DateTime.UtcNow.AddDays(-48), Status = "Returned" },

                // Eve returned 55 days late → generates large unpaid fine (> Rs.500 block)
                /* [9] */ new Borrowing { MemberId = members[4].Id, BookCopyId = copies[10].Id, BorrowDate = DateTime.UtcNow.AddDays(-80), DueDate = DateTime.UtcNow.AddDays(-70), ReturnDate = DateTime.UtcNow.AddDays(-15), Status = "Returned" },
            };
            ctx.Borrowings.AddRange(returnedBorrowings);
            ctx.SaveChanges();

            // ── 6. Fines ──────────────────────────────────────
            var fines = new[]
            {
                // Bob's PAID fine: 3 days late × Rs.10 = Rs.30
                new Fine
                {
                    MemberId    = members[1].Id,
                    BorrowingId = returnedBorrowings[1].Id,
                    Amount      = 30m,
                    IssuedDate  = DateTime.UtcNow.AddDays(-28),
                    PaidDate    = DateTime.UtcNow.AddDays(-25),
                    IsPaid      = true,
                },

                // Alice's UNPAID fine: 4 days late × Rs.10 = Rs.40
                // → Alice appears in "Members with Pending Fines" report
                // → Alice can still borrow (Rs.40 < Rs.500 threshold)
                new Fine
                {
                    MemberId    = members[0].Id,
                    BorrowingId = returnedBorrowings[3].Id,
                    Amount      = 40m,
                    IssuedDate  = DateTime.UtcNow.AddDays(-36),
                    PaidDate    = null,
                    IsPaid      = false,
                },

                // Eve's UNPAID fine: 55 days late × Rs.10 = Rs.550
                // → Eve appears in "Members with Pending Fines" report
                // → Eve is BLOCKED from borrowing (Rs.550 > Rs.500 threshold)
                new Fine
                {
                    MemberId    = members[4].Id,
                    BorrowingId = returnedBorrowings[5].Id,
                    Amount      = 550m,
                    IssuedDate  = DateTime.UtcNow.AddDays(-15),
                    PaidDate    = null,
                    IsPaid      = false,
                },
            };
            ctx.Fines.AddRange(fines);
            ctx.SaveChanges();

            Console.WriteLine("[Seeder] Done!");
            Console.WriteLine();
            Console.WriteLine("  Test accounts (password: Test@123)");
            Console.WriteLine("  ─────────────────────────────────────────────────────────────");
            Console.WriteLine("  alice@example.com  — Basic,   active,  unpaid fine Rs.40");
            Console.WriteLine("  bob@example.com    — Student, active,  2 overdue borrowings");
            Console.WriteLine("  carol@example.com  — Premium, active,  clean record");
            Console.WriteLine("  david@example.com  — Student, INACTIVE (login blocked)");
            Console.WriteLine("  eve@example.com    — Basic,   active,  BORROWING BLOCKED (Rs.550 fine)");
            Console.WriteLine("  admin@example.com  — Admin");
            Console.WriteLine();
            Console.WriteLine("  Membership tiers: Basic (2 books/7 days) | Student (3 books/10 days) | Premium (5 books/15 days)");
            Console.WriteLine();
            Console.WriteLine("  Demo scenarios:");
            Console.WriteLine("  • Return Bob's overdue books → fn_return_book generates fines");
            Console.WriteLine("  • Try borrowing as Eve       → blocked (unpaid fine > Rs.500)");
            Console.WriteLine("  • Try logging in as David    → blocked (inactive account)");
            Console.WriteLine("  • Admin Reports              → all 6 reports have data");
            Console.WriteLine("  • Admin Mark Copy Damaged    → try ACC-0003 (copy ID 3)");
            Console.WriteLine("  • Admin Search Member        → search '9876' or 'alice'");
        }

        private static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
