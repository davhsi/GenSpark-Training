using LibraryManagement.DAL.Contexts;
using LibraryManagement.DAL.Repositories.Interfaces;
using LibraryManagement.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories.Implementations
{
    /// <summary>
    /// EF Core implementation of book and book copy data access.
    /// </summary>
    public class BookRepository : IBookRepository
    {
        private readonly LibraryContext _context;

        /// <summary>Initializes the repository with the given EF Core context.</summary>
        public BookRepository(LibraryContext context)
        {
            _context = context;
        }

        /// <summary>Inserts a new book into the catalog and saves immediately.</summary>
        public void Add(Book book)
        {
            _context.Books.Add(book);
            _context.SaveChanges();
        }

        /// <summary>Returns all books with their category and copies eagerly loaded.</summary>
        public List<Book> GetAllWithDetails()
        {
            return _context.Books
                .Include(b => b.BookCategory)
                .Include(b => b.BookCopies)
                .ToList();
        }

        /// <summary>Returns a single book by ID with its category and all copies. Returns null if not found.</summary>
        public Book? GetByIdWithDetails(int bookId)
        {
            return _context.Books
                .Include(b => b.BookCategory)
                .Include(b => b.BookCopies)
                .FirstOrDefault(b => b.Id == bookId);
        }

        /// <summary>Returns all book categories from the database.</summary>
        public List<BookCategory> GetCategories()
        {
            return _context.BookCategories.ToList();
        }

        /// <summary>Inserts a new book copy and saves immediately.</summary>
        public void AddCopy(BookCopy copy)
        {
            _context.BookCopies.Add(copy);
            _context.SaveChanges();
        }

        /// <summary>Returns all copies of a given book regardless of status.</summary>
        public List<BookCopy> GetAllCopiesByBook(int bookId)
        {
            return _context.BookCopies
                .Where(bc => bc.BookId == bookId)
                .OrderBy(bc => bc.AccessionNumber)
                .ToList();
        }

        /// <summary>Returns only the copies of a given book that have Status = 'Available'.</summary>
        public List<BookCopy> GetAvailableCopies(int bookId)
        {
            return _context.BookCopies
                .Where(bc => bc.BookId == bookId && bc.Status == "Available")
                .ToList();
        }

        /// <summary>Searches books by title, author, or category name (case-insensitive).</summary>
        public List<Book> SearchBooks(string keyword)
        {
            var lowerKeyword = keyword.ToLower();
            return _context.Books
                .Include(b => b.BookCategory)
                .Include(b => b.BookCopies)
                .Where(b => b.Title.ToLower().Contains(lowerKeyword)
                         || b.Author.ToLower().Contains(lowerKeyword)
                         || b.BookCategory!.Name.ToLower().Contains(lowerKeyword))
                .ToList();
        }

        /// <summary>Updates the status field of a specific book copy and saves immediately.</summary>
        public void UpdateCopyStatus(int copyId, string status)
        {
            var copy = _context.BookCopies.Find(copyId);
            if (copy != null)
            {
                copy.Status = status;
                _context.SaveChanges();
            }
        }

        /// <summary>Updates the metadata of an existing book and saves immediately.</summary>
        public void UpdateBook(Book book)
        {
            var existing = _context.Books.Find(book.Id);
            if (existing != null)
            {
                existing.Title           = book.Title;
                existing.Author          = book.Author;
                existing.ISBN            = book.ISBN;
                existing.PublicationYear = book.PublicationYear;
                existing.BookCategoryId  = book.BookCategoryId;
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes a book and all its copies. EF Cascade handles the copies.
        /// Caller must ensure no copies have active borrowings before calling.
        /// </summary>
        public void DeleteBook(int bookId)
        {
            var book = _context.Books.Find(bookId);
            if (book != null)
            {
                _context.Books.Remove(book);
                _context.SaveChanges();
            }
        }

        /// <summary>Returns a specific book copy by ID. Returns null if not found.</summary>
        public BookCopy? GetCopyById(int copyId)
        {
            return _context.BookCopies.Find(copyId);
        }

        /// <summary>Deletes a specific book copy by ID and saves immediately.</summary>
        public void DeleteCopy(int copyId)
        {
            var copy = _context.BookCopies.Find(copyId);
            if (copy != null)
            {
                _context.BookCopies.Remove(copy);
                _context.SaveChanges();
            }
        }
    }
}
