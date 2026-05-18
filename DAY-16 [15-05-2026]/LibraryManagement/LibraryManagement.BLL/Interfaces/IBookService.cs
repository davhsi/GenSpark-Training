using LibraryManagement.Model.Models;

namespace LibraryManagement.BLL.Interfaces
{
    /// <summary>
    /// Business logic contract for book browsing and copy management.
    /// </summary>
    public interface IBookService
    {
        /// <summary>Adds a new book record to the library catalog.</summary>
        void AddBook(Book book);

        /// <summary>Returns all books with their categories and copies.</summary>
        List<Book> GetAllBooks();

        /// <summary>
        /// Returns a single book by ID with its category and all copies.
        /// Returns null if not found.
        /// </summary>
        Book? GetBookById(int bookId);

        /// <summary>Returns all available book categories for catalog use.</summary>
        List<BookCategory> GetBookCategories();

        /// <summary>Adds a new physical copy of an existing book.</summary>
        void AddBookCopy(BookCopy copy);

        /// <summary>Returns all copies of a book regardless of status (for admin inventory view).</summary>
        List<BookCopy> GetAllCopiesByBook(int bookId);

        /// <summary>Returns all copies of a book that are currently available to borrow.</summary>
        List<BookCopy> GetAvailableCopies(int bookId);

        /// <summary>Searches books by title, author, or category name.</summary>
        List<Book> SearchBooks(string keyword);

        /// <summary>Updates the status of a specific copy (e.g., Damaged, Lost, Available).</summary>
        void UpdateCopyStatus(int copyId, string status);

        /// <summary>
        /// Updates the metadata of an existing book.
        /// </summary>
        /// <exception cref="Model.Exceptions.BookNotFoundException">Thrown if the book ID does not exist.</exception>
        void UpdateBook(int bookId, string title, string author, string? isbn, int publicationYear, int categoryId);

        /// <summary>
        /// Deletes a book and all its copies from the catalog.
        /// </summary>
        /// <exception cref="Model.Exceptions.BookNotFoundException">Thrown if the book ID does not exist.</exception>
        /// <exception cref="LibraryException">Thrown if any copy has an active borrowing.</exception>
        void DeleteBook(int bookId);

        /// <summary>Returns a specific book copy by ID. Returns null if not found.</summary>
        BookCopy? GetCopyById(int copyId);

        /// <summary>Deletes a specific book copy by ID.</summary>
        void DeleteBookCopy(int copyId);
    }
}
