using LibraryManagement.BLL.Interfaces;
using LibraryManagement.BLL.Services;
using LibraryManagement.BLL.Session;
using LibraryManagement.Model.Models;

namespace LibraryManagement.UI.Menus
{
    /// <summary>
    /// Handles all member-facing borrowing operations: viewing active borrowings,
    /// borrowing history, borrowing a new book, and returning a borrowed book.
    /// </summary>
    public static class BorrowingUI
    {
        private static readonly IBookService _bookService = new BookService();
        private static readonly IBorrowingService _borrowingService = new BorrowingService();
        private static readonly IReportService _reportService = new ReportService();

        /// <summary>Displays the current member's active (unreturned) borrowings.</summary>
        public static void ShowMyActiveBorrowings()
        {
            Console.Clear();
            Console.WriteLine("--- My Active Borrowings ---");
            var active = GetMyActiveBorrowings();

            if (active.Count == 0)
            {
                Console.WriteLine("You have no active borrowings.");
            }
            else
            {
                Console.WriteLine($"{"ID",-5} {"Book Title",-35} {"Borrow Date",-15} {"Due Date",-15}");
                Console.WriteLine(new string('-', 70));
                foreach (var b in active)
                    Console.WriteLine($"{b.Id,-5} {b.BookCopy?.Book?.Title,-35} {b.BorrowDate.ToString("yyyy-MM-dd"),-15} {b.DueDate.ToString("yyyy-MM-dd"),-15}");
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        /// <summary>Displays the full borrowing history for the current member, including returned books.</summary>
        public static void ShowMyBorrowingHistory()
        {
            Console.Clear();
            Console.WriteLine("--- My Borrowing History ---");
            var history = _reportService.GetMemberBorrowingHistory(UserSession.CurrentMember!.Id);

            if (history.Count == 0)
            {
                Console.WriteLine("No borrowing history found.");
            }
            else
            {
                Console.WriteLine($"{"ID",-5} {"Book Title",-35} {"Borrow Date",-15} {"Status",-12}");
                Console.WriteLine(new string('-', 67));
                foreach (var b in history)
                    Console.WriteLine($"{b.Id,-5} {b.BookCopy?.Book?.Title,-35} {b.BorrowDate.ToString("yyyy-MM-dd"),-15} {b.Status,-12}");
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        /// <summary>
        /// Prompts the member to select a book and available copy, then calls
        /// the borrow stored procedure via the service layer.
        /// </summary>
        public static void BorrowBook()
        {
            Console.Clear();
            Console.WriteLine("--- Borrow a Book ---");
            Console.WriteLine("(Tip: Go to 'Browse / Search Books' first to find a Book ID)");
            Console.Write("\nEnter Book ID to borrow: ");

            if (!int.TryParse(Console.ReadLine(), out int bookId) || bookId <= 0)
            {
                Console.WriteLine("Invalid book ID. Press any key...");
                Console.ReadKey();
                return;
            }

            var book = _bookService.GetBookById(bookId);
            if (book == null)
            {
                Console.WriteLine("Book not found. Press any key...");
                Console.ReadKey();
                return;
            }

            var copies = _bookService.GetAvailableCopies(bookId);
            if (copies.Count == 0)
            {
                Console.WriteLine($"No available copies for '{book.Title}'. Press any key...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\nBorrowing: {book.Title} by {book.Author}");
            Console.WriteLine($"Available Copies ({copies.Count} found):");
            foreach (var c in copies) Console.WriteLine($"  Copy ID: {c.Id}  |  Accession No: {c.AccessionNumber}");

            Console.Write("\nSelect Copy ID: ");
            if (!int.TryParse(Console.ReadLine(), out int copyId) || copyId <= 0)
            {
                Console.WriteLine("Invalid copy ID. Press any key...");
                Console.ReadKey();
                return;
            }

            if (!copies.Any(c => c.Id == copyId))
            {
                Console.WriteLine("Selected copy is not available. Press any key...");
                Console.ReadKey();
                return;
            }

            string result = _borrowingService.BorrowBook(UserSession.CurrentMember!.Id, copyId);
            Console.WriteLine($"\n{result}");
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        /// <summary>
        /// Lists the member's active borrowings and processes a return.
        /// Validates that the selected borrowing ID belongs to the current member.
        /// </summary>
        public static void ReturnBook()
        {
            Console.Clear();
            Console.WriteLine("--- Return a Book ---");

            var active = GetMyActiveBorrowings();

            if (active.Count == 0)
            {
                Console.WriteLine("You have no active borrowings to return.");
                Console.WriteLine("Press any key...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"{"Borrowing ID",-15} {"Book Title",-35} {"Due Date",-15}");
            Console.WriteLine(new string('-', 65));
            foreach (var b in active)
                Console.WriteLine($"{b.Id,-15} {b.BookCopy?.Book?.Title,-35} {b.DueDate.ToString("yyyy-MM-dd"),-15}");

            Console.Write("\nEnter Borrowing ID to return: ");
            if (!int.TryParse(Console.ReadLine(), out int borrowId))
            {
                Console.WriteLine("Invalid ID. Press any key...");
                Console.ReadKey();
                return;
            }

            if (!active.Any(b => b.Id == borrowId))
            {
                Console.WriteLine("That borrowing does not belong to your account. Press any key...");
                Console.ReadKey();
                return;
            }

            Console.Write("Is the book being returned in damaged condition? (y/n): ");
            string damageInput = Console.ReadLine()?.Trim().ToLower() ?? "n";
            bool isDamaged = damageInput == "y" || damageInput == "yes";

            if (isDamaged)
                Console.WriteLine("Note: A Rs.500 damage fine will be added to your account.");

            string result = _borrowingService.ReturnBook(borrowId, isDamaged);
            Console.WriteLine($"\n{result}");
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        // Helper: returns the current member's active (unreturned) borrowings
        private static List<Borrowing> GetMyActiveBorrowings()
        {
            return _reportService
                .GetMemberBorrowingHistory(UserSession.CurrentMember!.Id)
                .Where(b => b.Status == "Borrowed")
                .ToList();
        }
    }
}
