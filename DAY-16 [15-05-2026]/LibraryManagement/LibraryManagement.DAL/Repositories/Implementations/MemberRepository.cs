using LibraryManagement.DAL.Contexts;
using LibraryManagement.DAL.Repositories.Interfaces;
using LibraryManagement.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories.Implementations
{
    /// <summary>
    /// EF Core implementation of member data access.
    /// </summary>
    public class MemberRepository : IMemberRepository
    {
        private readonly LibraryContext _context;

        /// <summary>Initializes the repository with the given EF Core context.</summary>
        public MemberRepository(LibraryContext context)
        {
            _context = context;
        }

        /// <summary>Inserts a new member and saves immediately.</summary>
        public void Add(Member member)
        {
            _context.Members.Add(member);
            _context.SaveChanges();
        }

        /// <summary>Returns all members with their membership type eagerly loaded.</summary>
        public List<Member> GetAllWithMembership()
        {
            return _context.Members.Include(m => m.MembershipType).ToList();
        }

        /// <summary>Returns a member by ID, including membership type, fines, and borrowings.</summary>
        public Member? GetByIdWithDetails(int id)
        {
            return _context.Members
                .Include(m => m.MembershipType)
                .Include(m => m.Fines)
                .Include(m => m.Borrowings)
                .FirstOrDefault(m => m.Id == id);
        }

        /// <summary>Finds a member by exact email match. Returns null if not found.</summary>
        public Member? GetByEmail(string email)
        {
            return _context.Members
                .Include(m => m.MembershipType)
                .FirstOrDefault(m => m.Email == email);
        }

        /// <summary>Finds a member by exact phone match. Returns null if not found.</summary>
        public Member? GetByPhone(string phone)
        {
            return _context.Members
                .FirstOrDefault(m => m.Phone == phone);
        }

        /// <summary>Returns all seeded membership types from the database.</summary>
        public List<MembershipType> GetMembershipTypes()
        {
            return _context.MembershipTypes.ToList();
        }

        /// <summary>Searches members by phone or email containing the keyword.</summary>
        public List<Member> SearchMembers(string keyword)
        {
            return _context.Members
                .Include(m => m.MembershipType)
                .Where(m => m.Phone.Contains(keyword) || (m.Email != null && m.Email.Contains(keyword)))
                .ToList();
        }

        /// <summary>Activates or deactivates a member account by ID.</summary>
        public void UpdateMemberStatus(int memberId, bool isActive)
        {
            var member = _context.Members.Find(memberId);
            if (member != null)
            {
                member.IsActive = isActive;
                _context.SaveChanges();
            }
        }

        /// <summary>Updates a member's profile fields and saves immediately.</summary>
        public void UpdateProfile(int memberId, string firstName, string lastName, string phone, string email, int membershipTypeId)
        {
            var member = _context.Members.Find(memberId);
            if (member != null)
            {
                member.FirstName        = firstName;
                member.LastName         = lastName;
                member.Phone            = phone;
                member.Email            = email;
                member.MembershipTypeId = membershipTypeId;
                _context.SaveChanges();
            }
        }

        /// <summary>Updates a member's hashed password and saves immediately.</summary>
        public void UpdatePassword(int memberId, string hashedPassword)
        {
            var member = _context.Members.Find(memberId);
            if (member != null)
            {
                member.Password = hashedPassword;
                _context.SaveChanges();
            }
        }
    }
}
