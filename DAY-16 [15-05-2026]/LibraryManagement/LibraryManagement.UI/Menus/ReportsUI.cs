using LibraryManagement.BLL.Interfaces;
using LibraryManagement.BLL.Services;

namespace LibraryManagement.UI.Menus
{
    /// <summary>
    /// Admin-facing reports menu covering all 6 required report types.
    /// </summary>
    public static class ReportsUI
    {
        private static readonly IReportService _reportService = new ReportService();
        private static readonly IMemberService _memberService = new MemberService();

        public static void DisplayMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("           REPORTS                      ");
                Console.WriteLine("========================================");
                Console.WriteLine("1. Books Currently Borrowed");
                Console.WriteLine("2. Overdue Books");
                Console.WriteLine("3. Members with Pending Fines");
                Console.WriteLine("4. Most Borrowed Books");
                Console.WriteLine("5. Available Books by Category");
                Console.WriteLine("6. Member Borrowing History");
                Console.WriteLine("0. Back");
                Console.WriteLine("========================================");
                Console.Write("Select: ");
                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1": ShowCurrentlyBorrowed(); break;
                    case "2": ShowOverdueBooks(); break;
                    case "3": ShowMembersWithFines(); break;
                    case "4": ShowMostBorrowedBooks(); break;
                    case "5": ShowAvailableByCategory(); break;
                    case "6": ShowMemberHistory(); break;
                    case "0": return;
                    default:
                        Console.WriteLine("Invalid option. Press any key...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static void ShowCurrentlyBorrowed()
        {
            Console.Clear();
            Console.WriteLine("--- Books Currently Borrowed ---");
            var borrowings = _reportService.GetCurrentlyBorrowedBooks();

            if (borrowings.Count == 0)
            {
                Console.WriteLine("No books are currently borrowed.");
            }
            else
            {
                Console.WriteLine($"{"ID",-5} {"Book Title",-35} {"Member",-25} {"Borrow Date",-13} {"Due Date",-13}");
                Console.WriteLine(new string('-', 95));
                foreach (var b in borrowings)
                {
                    string memberName = $"{b.Member?.FirstName} {b.Member?.LastName}";
                    Console.WriteLine($"{b.Id,-5} {b.BookCopy?.Book?.Title,-35} {memberName,-25} {b.BorrowDate.ToString("yyyy-MM-dd"),-13} {b.DueDate.ToString("yyyy-MM-dd"),-13}");
                }
                Console.WriteLine($"\nTotal: {borrowings.Count} book(s) currently borrowed.");
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        private static void ShowOverdueBooks()
        {
            Console.Clear();
            Console.WriteLine("--- Overdue Books ---");
            var overdue = _reportService.GetOverdueBooks();

            if (overdue.Count == 0)
            {
                Console.WriteLine("No overdue books. All borrowings are on time!");
            }
            else
            {
                Console.WriteLine($"{"ID",-5} {"Book Title",-35} {"Member",-25} {"Due Date",-13} {"Days Overdue",-12}");
                Console.WriteLine(new string('-', 95));
                foreach (var b in overdue)
                {
                    int daysOverdue = (int)(DateTime.UtcNow - b.DueDate).TotalDays;
                    string memberName = $"{b.Member?.FirstName} {b.Member?.LastName}";
                    Console.WriteLine($"{b.Id,-5} {b.BookCopy?.Book?.Title,-35} {memberName,-25} {b.DueDate.ToString("yyyy-MM-dd"),-13} {daysOverdue,-12}");
                }
                Console.WriteLine($"\nTotal: {overdue.Count} overdue borrowing(s).");
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        private static void ShowMembersWithFines()
        {
            Console.Clear();
            Console.WriteLine("--- Members with Pending Fines ---");
            var members = _reportService.GetMembersWithPendingFines();

            if (members.Count == 0)
            {
                Console.WriteLine("No members have pending fines.");
            }
            else
            {
                Console.WriteLine($"{"ID",-5} {"Name",-25} {"Email",-25} {"Unpaid Fines",-15} {"Total Outstanding",-18}");
                Console.WriteLine(new string('-', 90));
                foreach (var m in members)
                {
                    var unpaidFines = m.Fines.Where(f => !f.IsPaid).ToList();
                    decimal outstanding = unpaidFines.Sum(f => f.Amount);
                    Console.WriteLine($"{m.Id,-5} {m.FirstName + " " + m.LastName,-25} {m.Email,-25} {unpaidFines.Count,-15} ₹{outstanding,-17}");
                }
                Console.WriteLine($"\nTotal: {members.Count} member(s) with pending fines.");
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        private static void ShowMostBorrowedBooks()
        {
            Console.Clear();
            Console.WriteLine("--- Most Borrowed Books ---");
            var books = _reportService.GetMostBorrowedBooks();

            if (books.Count == 0)
            {
                Console.WriteLine("No borrowing data available yet.");
            }
            else
            {
                Console.WriteLine($"{"Rank",-6} {"Title",-35} {"Author",-20} {"Category",-15} {"Times Borrowed",-15}");
                Console.WriteLine(new string('-', 95));
                int rank = 1;
                foreach (var (book, count) in books)
                {
                    Console.WriteLine($"{rank,-6} {book.Title,-35} {book.Author,-20} {book.BookCategory?.Name,-15} {count,-15}");
                    rank++;
                }
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        private static void ShowAvailableByCategory()
        {
            Console.Clear();
            Console.WriteLine("--- Available Books by Category ---");
            var categories = _reportService.GetAvailableBooksByCategory();

            if (categories.Count == 0)
            {
                Console.WriteLine("No available books found.");
            }
            else
            {
                foreach (var (category, books) in categories)
                {
                    Console.WriteLine($"\n[ {category.Name} ] — {books.Count} book(s) available");
                    Console.WriteLine(new string('-', 60));
                    foreach (var b in books)
                    {
                        int availCount = b.BookCopies.Count(bc => bc.Status == "Available");
                        Console.WriteLine($"  ID: {b.Id,-5} {b.Title,-35} ({availCount} copy(s))");
                    }
                }
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        private static void ShowMemberHistory()
        {
            Console.Clear();
            Console.WriteLine("--- Member Borrowing History ---");
            Console.Write("Enter Member ID: ");

            if (!int.TryParse(Console.ReadLine(), out int memberId) || memberId <= 0)
            {
                Console.WriteLine("Invalid Member ID. Press any key...");
                Console.ReadKey();
                return;
            }

            var member = _memberService.GetMemberById(memberId);
            if (member == null)
            {
                Console.WriteLine("Member not found. Press any key...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\nMember  : {member.FirstName} {member.LastName}");
            Console.WriteLine($"Email   : {member.Email}");
            Console.WriteLine($"Phone   : {member.Phone}");
            Console.WriteLine($"Tier    : {member.MembershipType?.Name}");
            Console.WriteLine($"Status  : {(member.IsActive ? "Active" : "Inactive")}");

            var history = _reportService.GetMemberBorrowingHistory(memberId);

            if (history.Count == 0)
            {
                Console.WriteLine("No borrowing history found for this member.");
            }
            else
            {
                Console.WriteLine($"\n{"ID",-5} {"Book Title",-35} {"Borrow Date",-13} {"Due Date",-13} {"Return Date",-13} {"Status",-10}");
                Console.WriteLine(new string('-', 95));
                foreach (var b in history)
                {
                    string returnDate = b.ReturnDate.HasValue ? b.ReturnDate.Value.ToString("yyyy-MM-dd") : "—";
                    Console.WriteLine($"{b.Id,-5} {b.BookCopy?.Book?.Title,-35} {b.BorrowDate.ToString("yyyy-MM-dd"),-13} {b.DueDate.ToString("yyyy-MM-dd"),-13} {returnDate,-13} {b.Status,-10}");
                }
                Console.WriteLine($"\nTotal: {history.Count} borrowing(s).");
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }
    }
}
