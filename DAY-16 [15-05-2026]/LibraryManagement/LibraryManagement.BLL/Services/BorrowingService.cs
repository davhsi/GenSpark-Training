using LibraryManagement.BLL.Interfaces;
using LibraryManagement.DAL.Contexts;
using LibraryManagement.DAL.Repositories.Implementations;
using LibraryManagement.DAL.Repositories.Interfaces;

namespace LibraryManagement.BLL.Services
{
    /// <summary>
    /// Delegates borrow/return operations directly to the repository,
    /// which calls the PostgreSQL stored procedures.
    /// No transaction management needed here — fully handled by the SP.
    /// </summary>
    /// <summary>
    /// Delegates borrow and return operations to the repository,
    /// which calls PostgreSQL stored procedures for atomic transaction handling.
    /// </summary>
    public class BorrowingService : IBorrowingService
    {
        private readonly IBorrowingRepository _repository;

        public BorrowingService()
        {
            _repository = new BorrowingRepository(new LibraryContext());
        }

        /// <summary>Passes the borrow request to the stored procedure via the repository.</summary>
        public string BorrowBook(int memberId, int bookCopyId)
        {
            return _repository.BorrowBook(memberId, bookCopyId);
        }

        /// <summary>Passes the return request to the stored procedure via the repository.</summary>
        public string ReturnBook(int borrowingId, bool isDamaged)
        {
            return _repository.ReturnBook(borrowingId, isDamaged);
        }
    }
}
