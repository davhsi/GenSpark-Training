using LibraryManagement.BLL.Interfaces;
using LibraryManagement.BLL.Services;

namespace LibraryManagement.UI.Menus
{
    /// <summary>
    /// Member-facing book browsing. Read-only — no add/edit operations.
    /// </summary>
    public static class BookUI
    {
        private static readonly IBookService _bookService = new BookService();

        /// <summary>Shows the browse/search menu and loops until the user goes back.</summary>
        public static void DisplayMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("           BROWSE BOOKS                 ");
                Console.WriteLine("========================================");
                Console.WriteLine("1. List All Books");
                Console.WriteLine("2. Search Books");
                Console.WriteLine("0. Back");
                Console.WriteLine("========================================");
                Console.Write("Select: ");
                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1": ListBooks(); break;
                    case "2": SearchBooks(); break;
                    case "0": return;
                    default:
                        Console.WriteLine("Invalid option. Press any key...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // Lists every book in the catalog with live availability counts
        private static void ListBooks()
        {
            Console.Clear();
            Console.WriteLine("--- All Books ---");
            var books = _bookService.GetAllBooks();

            if (books.Count == 0)
            {
                Console.WriteLine("No books found in the library.");
                Console.WriteLine("Press any key...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"{"ID",-5} {"Title",-35} {"Author",-20} {"Category",-15} {"Available",-10}");
            Console.WriteLine(new string('-', 85));
            foreach (var b in books)
            {
                int avail = b.BookCopies.Count(bc => bc.Status == "Available");
                string availability = avail > 0 ? $"{avail} copy(s)" : "Unavailable";
                Console.WriteLine($"{b.Id,-5} {b.Title,-35} {b.Author,-20} {b.BookCategory?.Name,-15} {availability,-10}");
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        // Searches the catalog by keyword and prints matching results
        private static void SearchBooks()
        {
            Console.Clear();
            Console.WriteLine("--- Search Books ---");
            Console.Write("Enter keyword (title / author / category): ");
            string keyword = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(keyword))
            {
                Console.WriteLine("Search keyword cannot be empty. Press any key...");
                Console.ReadKey();
                return;
            }

            var books = _bookService.SearchBooks(keyword);

            if (books.Count == 0)
            {
                Console.WriteLine($"No books found matching '{keyword}'.");
            }
            else
            {
                Console.WriteLine($"\n{"ID",-5} {"Title",-35} {"Author",-20} {"Category",-15} {"Available",-10}");
                Console.WriteLine(new string('-', 85));
                foreach (var b in books)
                {
                    int avail = b.BookCopies.Count(bc => bc.Status == "Available");
                    string availability = avail > 0 ? $"{avail} copy(s)" : "Unavailable";
                    Console.WriteLine($"{b.Id,-5} {b.Title,-35} {b.Author,-20} {b.BookCategory?.Name,-15} {availability,-10}");
                }
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }
    }
}
