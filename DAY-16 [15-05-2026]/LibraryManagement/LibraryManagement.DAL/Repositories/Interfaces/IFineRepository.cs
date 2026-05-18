using LibraryManagement.Model.Models;

namespace LibraryManagement.DAL.Repositories.Interfaces
{
    /// <summary>
    /// Data access contract for fine-related database operations.
    /// </summary>
    public interface IFineRepository
    {
        /// <summary>Returns all unpaid fines for a given member, including book details.</summary>
        List<Fine> GetUnpaidFines(int memberId);

        /// <summary>Returns all fines (paid and unpaid) for a given member, including book details.</summary>
        List<Fine> GetAllFines(int memberId);

        /// <summary>Finds a specific unpaid fine by its ID. Returns null if already paid or not found.</summary>
        Fine? GetFineById(int fineId);

        /// <summary>Persists all pending changes to the database.</summary>
        void SaveChanges();
    }
}
