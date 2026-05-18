using LibraryManagement.Model.Models;

namespace LibraryManagement.BLL.Interfaces
{
    /// <summary>
    /// Business logic contract for report queries.
    /// </summary>
    public interface IReportService
    {
        /// <summary>Returns the complete borrowing history for a member, newest first.</summary>
        List<Borrowing> GetMemberBorrowingHistory(int memberId);

        /// <summary>Returns all currently borrowed (unreturned) books.</summary>
        List<Borrowing> GetCurrentlyBorrowedBooks();

        /// <summary>Returns all overdue borrowings.</summary>
        List<Borrowing> GetOverdueBooks();

        /// <summary>Returns all members who have unpaid fines.</summary>
        List<Member> GetMembersWithPendingFines();

        /// <summary>Returns books ranked by borrow count.</summary>
        List<(Book Book, int BorrowCount)> GetMostBorrowedBooks();

        /// <summary>Returns available books grouped by category.</summary>
        List<(BookCategory Category, List<Book> Books)> GetAvailableBooksByCategory();
    }
}
