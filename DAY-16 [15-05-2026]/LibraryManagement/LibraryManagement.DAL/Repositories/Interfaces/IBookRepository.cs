using LibraryManagement.Model.Models;

namespace LibraryManagement.DAL.Repositories.Interfaces
{
    /// <summary>
    /// Data access contract for book and book copy database operations.
    /// </summary>
    public interface IBookRepository
    {
        /// <summary>Inserts a new book into the database.</summary>
        void Add(Book book);

        /// <summary>Returns all books with their category and copies included.</summary>
        List<Book> GetAllWithDetails();

        /// <summary>Returns a single book by ID with category and all copies. Returns null if not found.</summary>
        Book? GetByIdWithDetails(int bookId);

        /// <summary>Returns all book categories (Fiction, Science, etc.).</summary>
        List<BookCategory> GetCategories();

        /// <summary>Inserts a new book copy linked to an existing book.</summary>
        void AddCopy(BookCopy copy);

        /// <summary>Returns all copies of a specific book regardless of status.</summary>
        List<BookCopy> GetAllCopiesByBook(int bookId);

        /// <summary>Returns all copies of a specific book that have Status = 'Available'.</summary>
        List<BookCopy> GetAvailableCopies(int bookId);

        /// <summary>Searches books by title, author, or category name.</summary>
        List<Book> SearchBooks(string keyword);

        /// <summary>Updates the status of a specific book copy (e.g., Available, Borrowed, Damaged).</summary>
        void UpdateCopyStatus(int copyId, string status);

        /// <summary>Updates the metadata of an existing book (title, author, ISBN, year, category).</summary>
        void UpdateBook(Book book);

        /// <summary>
        /// Deletes a book and all its copies from the catalog.
        /// Only safe to call when no copies have active borrowings.
        /// </summary>
        void DeleteBook(int bookId);

        /// <summary>Returns a specific book copy by ID. Returns null if not found.</summary>
        BookCopy? GetCopyById(int copyId);

        /// <summary>Deletes a specific book copy from the database.</summary>
        void DeleteCopy(int copyId);
    }
}
