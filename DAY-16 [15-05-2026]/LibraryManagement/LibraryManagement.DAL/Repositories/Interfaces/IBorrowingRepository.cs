namespace LibraryManagement.DAL.Repositories.Interfaces
{
    /// <summary>
    /// Data access contract for borrowing transactions.
    /// Both methods delegate to PostgreSQL stored procedures for atomic execution.
    /// </summary>
    public interface IBorrowingRepository
    {
        /// <summary>
        /// Calls fn_borrow_book database function. Performs all validation and inserts
        /// the borrowing record atomically. Returns a result message.
        /// </summary>
        string BorrowBook(int memberId, int bookCopyId);

        /// <summary>
        /// Calls proc_return_book. Updates borrowing and book copy status,
        /// issues an overdue fine if late, and a Rs.500 damage fine if the copy is returned damaged.
        /// Returns a result message.
        /// </summary>
        string ReturnBook(int borrowingId, bool isDamaged);
    }
}
