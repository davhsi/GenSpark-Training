using LibraryManagement.BLL.Interfaces;
using LibraryManagement.DAL.Contexts;
using LibraryManagement.DAL.Repositories.Implementations;
using LibraryManagement.DAL.Repositories.Interfaces;
using LibraryManagement.Model.Models;

namespace LibraryManagement.BLL.Services
{
    /// <summary>
    /// Provides report and history queries for both member and admin use.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repository;

        public ReportService()
        {
            _repository = new ReportRepository(new LibraryContext());
        }

        public List<Borrowing> GetMemberBorrowingHistory(int memberId)
            => _repository.GetMemberBorrowingHistory(memberId);

        public List<Borrowing> GetCurrentlyBorrowedBooks()
            => _repository.GetCurrentlyBorrowedBooks();

        public List<Borrowing> GetOverdueBooks()
            => _repository.GetOverdueBooks();

        public List<Member> GetMembersWithPendingFines()
            => _repository.GetMembersWithPendingFines();

        public List<(Book Book, int BorrowCount)> GetMostBorrowedBooks()
            => _repository.GetMostBorrowedBooks();

        public List<(BookCategory Category, List<Book> Books)> GetAvailableBooksByCategory()
            => _repository.GetAvailableBooksByCategory();
    }
}
