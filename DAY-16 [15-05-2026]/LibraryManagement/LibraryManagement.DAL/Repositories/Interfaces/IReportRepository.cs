using LibraryManagement.Model.Models;

namespace LibraryManagement.DAL.Repositories.Interfaces
{
    /// <summary>
    /// Data access contract for report and history queries.
    /// </summary>
    public interface IReportRepository
    {
        /// <summary>Returns the full borrowing history for a member, ordered by most recent first.</summary>
        List<Borrowing> GetMemberBorrowingHistory(int memberId);

        /// <summary>Returns all borrowings that are currently active (not yet returned).</summary>
        List<Borrowing> GetCurrentlyBorrowedBooks();

        /// <summary>Returns all active borrowings where DueDate has passed.</summary>
        List<Borrowing> GetOverdueBooks();

        /// <summary>Returns all members who have at least one unpaid fine.</summary>
        List<Member> GetMembersWithPendingFines();

        /// <summary>Returns books ranked by how many times they have been borrowed, descending.</summary>
        List<(Book Book, int BorrowCount)> GetMostBorrowedBooks();

        /// <summary>Returns available copies grouped by category.</summary>
        List<(BookCategory Category, List<Book> Books)> GetAvailableBooksByCategory();
    }
}
