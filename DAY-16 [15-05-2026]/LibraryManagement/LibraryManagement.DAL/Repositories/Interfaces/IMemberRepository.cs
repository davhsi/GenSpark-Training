using LibraryManagement.Model.Models;

namespace LibraryManagement.DAL.Repositories.Interfaces
{
    /// <summary>
    /// Data access contract for member-related database operations.
    /// </summary>
    public interface IMemberRepository
    {
        /// <summary>Inserts a new member into the database.</summary>
        void Add(Member member);

        /// <summary>Returns all members with their membership type included.</summary>
        List<Member> GetAllWithMembership();

        /// <summary>Returns a single member by ID, including borrowings and fines.</summary>
        Member? GetByIdWithDetails(int id);

        /// <summary>Finds a member by their email address. Returns null if not found.</summary>
        Member? GetByEmail(string email);

        /// <summary>Finds a member by their phone number. Returns null if not found.</summary>
        Member? GetByPhone(string phone);

        /// <summary>Returns all available membership types (Basic, Student, Premium).</summary>
        List<MembershipType> GetMembershipTypes();

        /// <summary>Searches members by phone or email containing the given keyword.</summary>
        List<Member> SearchMembers(string keyword);

        /// <summary>Activates or deactivates a member account.</summary>
        void UpdateMemberStatus(int memberId, bool isActive);

        /// <summary>Updates a member's profile fields: first name, last name, phone, email, membership type.</summary>
        void UpdateProfile(int memberId, string firstName, string lastName, string phone, string email, int membershipTypeId);

        /// <summary>Updates a member's hashed password.</summary>
        void UpdatePassword(int memberId, string hashedPassword);
    }
}
