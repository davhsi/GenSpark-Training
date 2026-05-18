using LibraryManagement.DAL.Contexts;
using LibraryManagement.DAL.Repositories.Interfaces;
using LibraryManagement.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories.Implementations
{
    /// <summary>
    /// All report queries are delegated to PostgreSQL stored procedures.
    /// EF Core maps SP result rows directly to existing entity types.
    /// Navigation properties that require a second lookup are populated
    /// via a follow-up EF query — no DTOs needed.
    /// </summary>
    public class ReportRepository : IReportRepository
    {
        private readonly LibraryContext _context;

        public ReportRepository(LibraryContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Calls fn_get_member_borrowing_history.
        /// Navigation properties (BookCopy → Book, Member) are populated
        /// via a second EF query on the returned IDs.
        /// </summary>
        public List<Borrowing> GetMemberBorrowingHistory(int memberId)
        {
            var ids = _context.Database
                .SqlQuery<int>($"SELECT \"Id\" FROM fn_get_member_borrowing_history({memberId})")
                .ToList();

            return _context.Borrowings
                .Include(b => b.BookCopy).ThenInclude(bc => bc!.Book)
                .Include(b => b.Member)
                .Where(b => ids.Contains(b.Id))
                .OrderByDescending(b => b.BorrowDate)
                .ToList();
        }

        /// <summary>
        /// Calls fn_get_currently_borrowed_books.
        /// Navigation properties populated via follow-up EF query.
        /// </summary>
        public List<Borrowing> GetCurrentlyBorrowedBooks()
        {
            var ids = _context.Database
                .SqlQuery<int>($"SELECT \"Id\" FROM fn_get_currently_borrowed_books()")
                .ToList();

            return _context.Borrowings
                .Include(b => b.BookCopy).ThenInclude(bc => bc!.Book)
                .Include(b => b.Member)
                .Where(b => ids.Contains(b.Id))
                .OrderBy(b => b.DueDate)
                .ToList();
        }

        /// <summary>
        /// Calls fn_get_overdue_books.
        /// Navigation properties populated via follow-up EF query.
        /// </summary>
        public List<Borrowing> GetOverdueBooks()
        {
            var ids = _context.Database
                .SqlQuery<int>($"SELECT \"Id\" FROM fn_get_overdue_books()")
                .ToList();

            return _context.Borrowings
                .Include(b => b.BookCopy).ThenInclude(bc => bc!.Book)
                .Include(b => b.Member)
                .Where(b => ids.Contains(b.Id))
                .OrderBy(b => b.DueDate)
                .ToList();
        }

        /// <summary>
        /// Calls fn_get_members_with_pending_fines.
        /// MembershipType and Fines navigation properties populated via follow-up EF query.
        /// </summary>
        public List<Member> GetMembersWithPendingFines()
        {
            var ids = _context.Database
                .SqlQuery<int>($"SELECT \"Id\" FROM fn_get_members_with_pending_fines()")
                .ToList();

            return _context.Members
                .Include(m => m.MembershipType)
                .Include(m => m.Fines)
                .Where(m => ids.Contains(m.Id))
                .ToList();
        }

        /// <summary>
        /// Calls fn_get_most_borrowed_books which returns book IDs + borrow counts.
        /// Books are then loaded from EF (with category) and paired with the count.
        /// </summary>
        public List<(Book Book, int BorrowCount)> GetMostBorrowedBooks()
        {
            // SP returns book id + borrow count as a flat scalar pair
            // We project into a value type EF can map: use a keyless entity workaround
            // by reading both columns and zipping with loaded books.
            var spRows = _context.Database
                .SqlQueryRaw<BookBorrowCountRow>(
                    "SELECT \"Id\" AS \"BookId\", \"BorrowCount\" FROM fn_get_most_borrowed_books()")
                .ToList();

            var bookIds = spRows.Select(r => r.BookId).ToList();

            var books = _context.Books
                .Include(b => b.BookCategory)
                .Where(b => bookIds.Contains(b.Id))
                .ToDictionary(b => b.Id);

            return spRows
                .Where(r => books.ContainsKey(r.BookId))
                .Select(r => (Book: books[r.BookId], BorrowCount: (int)r.BorrowCount))
                .ToList();
        }

        /// <summary>
        /// Calls fn_get_available_books_by_category which returns flat (CategoryId, BookId) rows.
        /// Groups them into (BookCategory, List&lt;Book&gt;) tuples in C#.
        /// </summary>
        public List<(BookCategory Category, List<Book> Books)> GetAvailableBooksByCategory()
        {
            var spRows = _context.Database
                .SqlQueryRaw<AvailableBookRow>(
                    "SELECT \"CategoryId\", \"BookId\" FROM fn_get_available_books_by_category()")
                .ToList();

            var categoryIds = spRows.Select(r => r.CategoryId).Distinct().ToList();
            var bookIds     = spRows.Select(r => r.BookId).Distinct().ToList();

            var categories = _context.BookCategories
                .Where(c => categoryIds.Contains(c.Id))
                .ToDictionary(c => c.Id);

            var books = _context.Books
                .Include(b => b.BookCopies)
                .Where(b => bookIds.Contains(b.Id))
                .ToDictionary(b => b.Id);

            return spRows
                .GroupBy(r => r.CategoryId)
                .Where(g => categories.ContainsKey(g.Key))
                .Select(g => (
                    Category: categories[g.Key],
                    Books: g
                        .Where(r => books.ContainsKey(r.BookId))
                        .Select(r => books[r.BookId])
                        .ToList()
                ))
                .OrderBy(x => x.Category.Name)
                .ToList();
        }

        // ── Private projection types used only for SqlQueryRaw mapping ──────────

        /// <summary>Projection for fn_get_most_borrowed_books result rows.</summary>
        private class BookBorrowCountRow
        {
            public int  BookId     { get; set; }
            public long BorrowCount { get; set; }
        }

        /// <summary>Projection for fn_get_available_books_by_category result rows.</summary>
        private class AvailableBookRow
        {
            public int CategoryId { get; set; }
            public int BookId     { get; set; }
        }
    }
}
