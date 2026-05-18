using LibraryManagement.Model.Models;

namespace LibraryManagement.BLL.Interfaces
{
    /// <summary>
    /// Business logic contract for borrowing and returning books.
    /// Both operations are executed atomically via PostgreSQL stored procedures.
    /// </summary>
    public interface IBorrowingService
    {
        /// <summary>
        /// Borrows a book copy for a member. Delegates to fn_borrow_book database function.
        /// Returns a status message indicating success or the reason for failure.
        /// </summary>
        string BorrowBook(int memberId, int bookCopyId);

        /// <summary>
        /// Returns a borrowed book. Delegates to proc_return_book.
        /// Pass isDamaged = true if the member is returning the copy in damaged condition.
        /// Automatically issues an overdue fine, a damage fine (Rs.500), or both.
        /// Returns a status message with fine details if applicable.
        /// </summary>
        string ReturnBook(int borrowingId, bool isDamaged);
    }
}
