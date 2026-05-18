using LibraryManagement.Model.Models;

namespace LibraryManagement.BLL.Interfaces
{
    /// <summary>
    /// Business logic contract for fine retrieval and payment.
    /// </summary>
    public interface IFineService
    {
        /// <summary>Returns all unpaid fines for a member, including associated book details.</summary>
        List<Fine> GetUnpaidFines(int memberId);

        /// <summary>Returns all fines (paid and unpaid) for a member.</summary>
        List<Fine> GetAllFines(int memberId);

        /// <summary>
        /// Marks a fine as fully paid.
        /// </summary>
        /// <exception cref="Model.Exceptions.FineNotFoundException">Thrown if the fine ID is invalid or already paid.</exception>
        void PayFine(int fineId);
    }
}
