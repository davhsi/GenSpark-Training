using LibraryManagement.BLL.Interfaces;
using LibraryManagement.BLL.Services;
using LibraryManagement.Model.Exceptions;
using LibraryManagement.Model.Models;

namespace LibraryManagement.UI.Menus
{
    public static class AdminBookUI
    {
        private static readonly IBookService _bookService = new BookService();

        public static void DisplayMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("           BOOK MANAGEMENT           ");
                Console.WriteLine("========================================");
                Console.WriteLine("1. Add New Book");
                Console.WriteLine("2. Edit Book");
                Console.WriteLine("3. Delete Book");
                Console.WriteLine("4. Add New Copy to Existing Book");
                Console.WriteLine("5. View All Copies of a Book");
                Console.WriteLine("6. Mark Copy Status");
                Console.WriteLine("7. Delete Specific Copy");
                Console.WriteLine("0. Back");
                Console.WriteLine("========================================");
                Console.Write("Select: ");
                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1": AddBook(); break;
                    case "2": EditBook(); break;
                    case "3": DeleteBook(); break;
                    case "4": AddBookCopy(); break;
                    case "5": ViewAllCopies(); break;
                    case "6": MarkCopyStatus(); break;
                    case "7": DeleteBookCopy(); break;
                    case "0": return;
                    default:
                        Console.WriteLine("Invalid option. Press any key...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static void AddBook()
        {
            Console.Clear();
            Console.WriteLine("--- Add New Book ---");
            Console.Write("Title: ");
            string title = Console.ReadLine() ?? "";
            Console.Write("Author: ");
            string author = Console.ReadLine() ?? "";
            Console.Write("ISBN (leave blank to skip): ");
            string isbn = Console.ReadLine() ?? "";

            Console.Write("Publication Year: ");
            if (!int.TryParse(Console.ReadLine(), out int year) || !IsValidPublicationYear(year))
            {
                Console.WriteLine("Invalid publication year. Please enter a year no later than the current year. Press any key...");
                Console.ReadKey();
                return;
            }

            var categories = _bookService.GetBookCategories();
            if (categories.Count == 0)
            {
                Console.WriteLine("No categories available. Cannot add book.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nCategories:");
            foreach (var c in categories)
                Console.WriteLine($"  {c.Id}. {c.Name}");
            Console.Write("Select Category ID: ");

            if (!int.TryParse(Console.ReadLine(), out int categoryId) || !categories.Any(c => c.Id == categoryId))
            {
                Console.WriteLine("Invalid Category ID. Press any key...");
                Console.ReadKey();
                return;
            }

            try
            {
                var book = new Book
                {
                    Title = title,
                    Author = author,
                    ISBN = string.IsNullOrWhiteSpace(isbn) ? null : isbn,
                    PublicationYear = year,
                    BookCategoryId = categoryId
                };

                _bookService.AddBook(book);
                Console.WriteLine($"\nBook '{title}' added successfully! (ID: {book.Id})");
                Console.WriteLine("Use option 4 to add physical copies.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        private static bool IsValidPublicationYear(int year)
        {
            int currentYear = DateTime.Now.Year;
            return year <= currentYear;
        }

        private static void EditBook()
        {
            Console.Clear();
            Console.WriteLine("--- Edit Book ---");
            Console.Write("Enter Book ID to edit: ");

            if (!int.TryParse(Console.ReadLine(), out int bookId))
            {
                Console.WriteLine("Invalid Book ID. Press any key...");
                Console.ReadKey();
                return;
            }

            var book = _bookService.GetBookById(bookId);
            if (book == null)
            {
                Console.WriteLine($"Book with ID {bookId} not found. Press any key...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\nCurrent values for: {book.Title}");
            Console.WriteLine($"  Author          : {book.Author}");
            Console.WriteLine($"  ISBN            : {book.ISBN ?? "(none)"}");
            Console.WriteLine($"  Publication Year: {book.PublicationYear}");
            Console.WriteLine($"  Category        : {book.BookCategory?.Name}");
            Console.WriteLine("\nEnter new values (press Enter to keep current):");

            Console.Write($"Title [{book.Title}]: ");
            string titleInput = Console.ReadLine() ?? "";
            string title = string.IsNullOrWhiteSpace(titleInput) ? book.Title : titleInput;

            Console.Write($"Author [{book.Author}]: ");
            string authorInput = Console.ReadLine() ?? "";
            string author = string.IsNullOrWhiteSpace(authorInput) ? book.Author : authorInput;

            Console.Write($"ISBN [{book.ISBN ?? "(none)"}]: ");
            string isbnInput = Console.ReadLine() ?? "";
            string? isbn = string.IsNullOrWhiteSpace(isbnInput) ? book.ISBN : isbnInput;

            Console.Write($"Publication Year [{book.PublicationYear}]: ");
            string yearInput = Console.ReadLine() ?? "";
            int year;
            if (string.IsNullOrWhiteSpace(yearInput))
            {
                year = book.PublicationYear;
            }
            else if (!int.TryParse(yearInput, out int parsedYear) || !IsValidPublicationYear(parsedYear))
            {
                Console.WriteLine("Invalid publication year. Please enter a year no later than the current year. Press any key...");
                Console.ReadKey();
                return;
            }
            else
            {
                year = parsedYear;
            }

            var categories = _bookService.GetBookCategories();
            Console.WriteLine("\nCategories:");
            foreach (var c in categories)
                Console.WriteLine($"  {c.Id}. {c.Name}");
            Console.Write($"Category ID [{book.BookCategoryId}]: ");
            string catInput = Console.ReadLine() ?? "";
            int categoryId = int.TryParse(catInput, out int parsedCat) && categories.Any(c => c.Id == parsedCat)
                ? parsedCat
                : book.BookCategoryId;

            try
            {
                _bookService.UpdateBook(bookId, title, author, isbn, year, categoryId);
                Console.WriteLine($"\nBook '{title}' updated successfully.");
            }
            catch (BookNotFoundException ex)
            {
                Console.WriteLine($"\n{ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        private static void DeleteBook()
        {
            Console.Clear();
            Console.WriteLine("--- Delete Book ---");
            Console.Write("Enter Book ID to delete: ");

            if (!int.TryParse(Console.ReadLine(), out int bookId))
            {
                Console.WriteLine("Invalid Book ID. Press any key...");
                Console.ReadKey();
                return;
            }

            var book = _bookService.GetBookById(bookId);
            if (book == null)
            {
                Console.WriteLine($"Book with ID {bookId} not found. Press any key...");
                Console.ReadKey();
                return;
            }

            int totalCopies = book.BookCopies.Count;
            int borrowedCopies = book.BookCopies.Count(bc => bc.Status == "Borrowed");

            Console.WriteLine($"\nBook: {book.Title} by {book.Author}");
            Console.WriteLine($"Total copies: {totalCopies}  |  Currently borrowed: {borrowedCopies}");

            if (borrowedCopies > 0)
            {
                Console.WriteLine("\nCannot delete — one or more copies are currently borrowed.");
                Console.WriteLine("Press any key...");
                Console.ReadKey();
                return;
            }

            Console.Write($"\nThis will permanently delete '{book.Title}' and all {totalCopies} copy(s). Confirm? (y/n): ");
            string confirm = Console.ReadLine()?.ToLower() ?? "";

            if (confirm != "y" && confirm != "yes")
            {
                Console.WriteLine("Deletion cancelled.");
                Console.WriteLine("Press any key...");
                Console.ReadKey();
                return;
            }

            try
            {
                _bookService.DeleteBook(bookId);
                Console.WriteLine($"\nBook '{book.Title}' and all its copies have been deleted.");
            }
            catch (LibraryException ex)
            {
                Console.WriteLine($"\n{ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        private static void AddBookCopy()
        {
            Console.Clear();
            Console.WriteLine("--- Add Physical Copy ---");
            Console.Write("Enter Book ID: ");

            if (!int.TryParse(Console.ReadLine(), out int bookId))
            {
                Console.WriteLine("Invalid Book ID. Press any key...");
                Console.ReadKey();
                return;
            }

            // Verify the book exists and show its title
            var book = _bookService.GetBookById(bookId);
            if (book == null)
            {
                Console.WriteLine($"Book with ID {bookId} not found. Press any key...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"Adding copy for: {book.Title} by {book.Author}");
            Console.Write("Accession Number (e.g. ACC-1001): ");
            string accNum = Console.ReadLine() ?? "";

            Console.Write("Condition (Good / New / Damaged): ");
            string condition = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(accNum) || string.IsNullOrWhiteSpace(condition))
            {
                Console.WriteLine("Accession Number and Condition are required. Press any key...");
                Console.ReadKey();
                return;
            }

            try
            {
                var copy = new BookCopy
                {
                    BookId = bookId,
                    AccessionNumber = accNum,
                    Condition = condition,
                    Status = "Available"
                };

                _bookService.AddBookCopy(copy);
                Console.WriteLine($"\nCopy '{accNum}' added successfully to '{book.Title}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine("Make sure the Accession Number is unique.");
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        private static void ViewAllCopies()
        {
            Console.Clear();
            Console.WriteLine("--- View All Copies of a Book ---");
            Console.Write("Enter Book ID: ");

            if (!int.TryParse(Console.ReadLine(), out int bookId))
            {
                Console.WriteLine("Invalid Book ID. Press any key...");
                Console.ReadKey();
                return;
            }

            var book = _bookService.GetBookById(bookId);
            if (book == null)
            {
                Console.WriteLine($"Book with ID {bookId} not found. Press any key...");
                Console.ReadKey();
                return;
            }

            var copies = _bookService.GetAllCopiesByBook(bookId);

            Console.WriteLine($"\n{book.Title} by {book.Author}");
            Console.WriteLine(new string('-', 70));

            if (copies.Count == 0)
            {
                Console.WriteLine("No physical copies exist for this book.");
            }
            else
            {
                Console.WriteLine($"{"Copy ID",-10} {"Accession No",-15} {"Condition",-12} {"Status",-12}");
                Console.WriteLine(new string('-', 50));
                foreach (var c in copies)
                    Console.WriteLine($"{c.Id,-10} {c.AccessionNumber,-15} {c.Condition,-12} {c.Status,-12}");

                int available = copies.Count(c => c.Status == "Available");
                int borrowed = copies.Count(c => c.Status == "Borrowed");
                int unavailable = copies.Count(c => c.Status is "Damaged" or "Unavailable");

                Console.WriteLine(new string('-', 50));
                Console.WriteLine($"Total: {copies.Count}  |  Available: {available}  |  Borrowed: {borrowed}  |  Out of service: {unavailable}");
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        private static void MarkCopyStatus()
        {
            Console.Clear();
            Console.WriteLine("--- Mark Copy Status ---");
            Console.Write("Enter Copy ID: ");

            if (!int.TryParse(Console.ReadLine(), out int copyId))
            {
                Console.WriteLine("Invalid Copy ID. Press any key...");
                Console.ReadKey();
                return;
            }

            // Look up the copy so the admin can confirm they have the right one
            var allBooks = _bookService.GetAllBooks();
            var copy = allBooks
                .SelectMany(b => b.BookCopies.Select(bc => new { Book = b, Copy = bc }))
                .FirstOrDefault(x => x.Copy.Id == copyId);

            if (copy == null)
            {
                Console.WriteLine($"Copy ID {copyId} not found. Press any key...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\nBook      : {copy.Book.Title} by {copy.Book.Author}");
            Console.WriteLine($"Accession : {copy.Copy.AccessionNumber}");
            Console.WriteLine($"Condition : {copy.Copy.Condition}");
            Console.WriteLine($"Status    : {copy.Copy.Status}");

            Console.WriteLine("\nSelect new status:");
            Console.WriteLine("1. Available");
            Console.WriteLine("2. Damaged");
            Console.WriteLine("3. Unavailable");
            Console.Write("Select: ");
            string choice = Console.ReadLine() ?? "";

            string status = choice switch
            {
                "1" => "Available",
                "2" => "Damaged",
                "3" => "Unavailable",
                _ => ""
            };

            if (string.IsNullOrEmpty(status))
            {
                Console.WriteLine("Invalid selection. Press any key...");
                Console.ReadKey();
                return;
            }

            try
            {
                _bookService.UpdateCopyStatus(copyId, status);
                Console.WriteLine($"Copy {copyId} status updated to '{status}' successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static void DeleteBookCopy()
        {
            Console.Clear();
            Console.WriteLine("--- Delete Specific Copy ---");
            Console.Write("Enter Copy ID to delete: ");

            if (!int.TryParse(Console.ReadLine(), out int copyId))
            {
                Console.WriteLine("Invalid Copy ID. Press any key...");
                Console.ReadKey();
                return;
            }

            var copy = _bookService.GetCopyById(copyId);
            if (copy == null)
            {
                Console.WriteLine($"Copy with ID {copyId} not found. Press any key...");
                Console.ReadKey();
                return;
            }

            var book = _bookService.GetBookById(copy.BookId);
            Console.WriteLine($"\nBook: {book?.Title}");
            Console.WriteLine($"Accession Number: {copy.AccessionNumber}");
            Console.WriteLine($"Condition: {copy.Condition}");
            Console.WriteLine($"Status: {copy.Status}");

            if (copy.Status == "Borrowed")
            {
                Console.WriteLine("\nCannot delete — this copy is currently borrowed.");
                Console.WriteLine("Press any key...");
                Console.ReadKey();
                return;
            }

            Console.Write($"\nThis will permanently delete copy '{copy.AccessionNumber}'. Confirm? (y/n): ");
            string confirm = Console.ReadLine()?.ToLower() ?? "";

            if (confirm != "y" && confirm != "yes")
            {
                Console.WriteLine("Deletion cancelled.");
                Console.WriteLine("Press any key...");
                Console.ReadKey();
                return;
            }

            try
            {
                _bookService.DeleteBookCopy(copyId);
                Console.WriteLine($"\nCopy '{copy.AccessionNumber}' has been deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }
    }
}
