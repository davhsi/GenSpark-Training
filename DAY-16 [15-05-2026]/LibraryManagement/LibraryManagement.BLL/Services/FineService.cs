using LibraryManagement.BLL.Interfaces;
using LibraryManagement.DAL.Contexts;
using LibraryManagement.DAL.Repositories.Implementations;
using LibraryManagement.DAL.Repositories.Interfaces;
using LibraryManagement.Model.Exceptions;
using LibraryManagement.Model.Models;

namespace LibraryManagement.BLL.Services
{
    public class FineService : IFineService
    {
        private readonly IFineRepository _repository;

        public FineService()
        {
            _repository = new FineRepository(new LibraryContext());
        }

        public List<Fine> GetUnpaidFines(int memberId)
        {
            return _repository.GetUnpaidFines(memberId);
        }

        public List<Fine> GetAllFines(int memberId)
        {
            return _repository.GetAllFines(memberId);
        }

        /// <summary>Marks the fine as fully paid in one shot.</summary>
        /// <exception cref="FineNotFoundException">Thrown when the fine ID is invalid or already paid.</exception>
        public void PayFine(int fineId)
        {
            var fine = _repository.GetFineById(fineId);

            if (fine == null)
                throw new FineNotFoundException(fineId);

            fine.IsPaid   = true;
            fine.PaidDate = DateTime.UtcNow;
            _repository.SaveChanges();
        }
    }
}
