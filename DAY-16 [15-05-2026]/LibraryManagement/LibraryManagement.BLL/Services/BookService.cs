using LibraryManagement.BLL.Interfaces;
using LibraryManagement.DAL.Contexts;
using LibraryManagement.DAL.Repositories.Implementations;
using LibraryManagement.DAL.Repositories.Interfaces;
using LibraryManagement.Model.Exceptions;
using LibraryManagement.Model.Models;

namespace LibraryManagement.BLL.Services
{
    /// <summary>
    /// Manages book catalog operations including adding, editing, deleting books,
    /// managing copies, and querying available copies for borrowing.
    /// </summary>
    public class BookService : IBookService
    {
        private readonly IBookRepository _repository;

        /// <summary>Initializes the service with a concrete BookRepository.</summary>
        public BookService()
        {
            _repository = new BookRepository(new LibraryContext());
        }

        /// <summary>Adds a new book title to the catalog.</summary>
        public void AddBook(Book book)
        {
            _repository.Add(book);
        }

        /// <summary>Returns all books with their categories and copies.</summary>
        public List<Book> GetAllBooks()
        {
            return _repository.GetAllWithDetails();
        }

        /// <summary>Returns a single book by ID with its category and all copies. Returns null if not found.</summary>
        public Book? GetBookById(int bookId)
        {
            return _repository.GetByIdWithDetails(bookId);
        }

        /// <summary>Returns all book categories for use in dropdowns and filters.</summary>
        public List<BookCategory> GetBookCategories()
        {
            return _repository.GetCategories();
        }

        /// <summary>Adds a new physical copy of an existing book to the library.</summary>
        public void AddBookCopy(BookCopy copy)
        {
            _repository.AddCopy(copy);
        }

        /// <summary>Returns all copies of a book regardless of status (for admin inventory view).</summary>
        public List<BookCopy> GetAllCopiesByBook(int bookId)
        {
            return _repository.GetAllCopiesByBook(bookId);
        }

        /// <summary>Returns all copies of a book that are currently available to borrow.</summary>
        public List<BookCopy> GetAvailableCopies(int bookId)
        {
            return _repository.GetAvailableCopies(bookId);
        }

        /// <summary>Searches books by title, author, or category name.</summary>
        public List<Book> SearchBooks(string keyword)
        {
            return _repository.SearchBooks(keyword);
        }

        /// <summary>Updates the status of a specific copy (e.g., Damaged, Lost, Available).</summary>
        public void UpdateCopyStatus(int copyId, string status)
        {
            _repository.UpdateCopyStatus(copyId, status);
        }

        /// <exception cref="BookNotFoundException">Thrown if the book ID does not exist.</exception>
        public void UpdateBook(int bookId, string title, string author, string? isbn, int publicationYear, int categoryId)
        {
            var existing = _repository.GetByIdWithDetails(bookId);
            if (existing == null)
                throw new BookNotFoundException(bookId);

            existing.Title           = title;
            existing.Author          = author;
            existing.ISBN            = isbn;
            existing.PublicationYear = publicationYear;
            existing.BookCategoryId  = categoryId;

            _repository.UpdateBook(existing);
        }

        /// <exception cref="BookNotFoundException">Thrown if the book ID does not exist.</exception>
        /// <exception cref="LibraryException">Thrown if any copy has an active borrowing.</exception>
        public void DeleteBook(int bookId)
        {
            var book = _repository.GetByIdWithDetails(bookId);
            if (book == null)
                throw new BookNotFoundException(bookId);

            bool hasActiveBorrowings = book.BookCopies
                .Any(bc => bc.Status == "Borrowed");

            if (hasActiveBorrowings)
                throw new LibraryException(
                    "Cannot delete this book. One or more copies are currently borrowed. " +
                    "Wait for all copies to be returned before deleting.");

            _repository.DeleteBook(bookId);
        }

        /// <summary>Returns a specific book copy by ID. Returns null if not found.</summary>
        public BookCopy? GetCopyById(int copyId)
        {
            return _repository.GetCopyById(copyId);
        }

        /// <summary>Deletes a specific book copy by ID.</summary>
        public void DeleteBookCopy(int copyId)
        {
            _repository.DeleteCopy(copyId);
        }
    }
}
