using LibraryManagement.DAL.Contexts;
using LibraryManagement.DAL.Repositories.Interfaces;
using LibraryManagement.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories.Implementations
{
    /// <summary>
    /// EF Core implementation of fine data access.
    /// </summary>
    public class FineRepository : IFineRepository
    {
        private readonly LibraryContext _context;

        /// <summary>Initializes the repository with the given EF Core context.</summary>
        public FineRepository(LibraryContext context)
        {
            _context = context;
        }

        /// <summary>Returns all unpaid fines for a member, with borrowing and book details eagerly loaded.</summary>
        public List<Fine> GetUnpaidFines(int memberId)
        {
            return _context.Fines
                .Include(f => f.Borrowing)
                    .ThenInclude(b => b!.BookCopy)
                        .ThenInclude(bc => bc!.Book)
                .Where(f => f.MemberId == memberId && !f.IsPaid)
                .ToList();
        }

        /// <summary>Returns all fines (paid and unpaid) for a member, with borrowing and book details eagerly loaded.</summary>
        public List<Fine> GetAllFines(int memberId)
        {
            return _context.Fines
                .Include(f => f.Borrowing)
                    .ThenInclude(b => b!.BookCopy)
                        .ThenInclude(bc => bc!.Book)
                .Where(f => f.MemberId == memberId)
                .OrderByDescending(f => f.IssuedDate)
                .ToList();
        }

        /// <summary>Returns a single unpaid fine by ID. Returns null if the fine is already paid or does not exist.</summary>
        public Fine? GetFineById(int fineId)
        {
            return _context.Fines.FirstOrDefault(f => f.Id == fineId && !f.IsPaid);
        }

        /// <summary>Flushes all tracked EF Core changes to the database.</summary>
        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
