using LibraryManagement.Model.Models;

namespace LibraryManagement.BLL.Interfaces
{
    /// <summary>
    /// Business logic contract for member registration, authentication, and management.
    /// </summary>
    public interface IMemberService
    {
        /// <summary>Validates input, hashes the password, and saves a new member to the database.</summary>
        void RegisterMember(Member member, string password);

        /// <summary>Authenticates a member by email and password. Throws on invalid credentials or inactive account.</summary>
        Member Login(string email, string password);

        /// <summary>Returns all members with their membership type details.</summary>
        List<Member> GetAllMembers();

        /// <summary>Returns a single member by ID with full borrowing and fine details.</summary>
        Member? GetMemberById(int id);

        /// <summary>Returns the list of all membership types available for registration.</summary>
        List<MembershipType> GetMembershipTypes();

        /// <summary>Searches members whose email or phone contains the given keyword.</summary>
        List<Member> SearchMembers(string keyword);

        /// <summary>
        /// Activates or deactivates a member's account.
        /// Deactivation is blocked if the member has active borrowings or unpaid fines.
        /// </summary>
        /// <exception cref="Model.Exceptions.MemberHasActiveBorrowingsException">Thrown when deactivating a member with unreturned books.</exception>
        /// <exception cref="Model.Exceptions.MemberHasUnpaidFinesException">Thrown when deactivating a member with outstanding fines.</exception>
        void UpdateMemberStatus(int memberId, bool isActive);

        /// <summary>
        /// Updates a member's profile: name, phone, email, and membership type.
        /// Validates email/phone format and uniqueness (excluding the member's own current values).
        /// </summary>
        /// <exception cref="Model.Exceptions.InvalidEmailException"/>
        /// <exception cref="Model.Exceptions.InvalidPhoneException"/>
        /// <exception cref="Model.Exceptions.DuplicateEmailException"/>
        /// <exception cref="Model.Exceptions.DuplicatePhoneException"/>
        /// <exception cref="Model.Exceptions.InvalidMembershipTypeException"/>
        void UpdateProfile(int memberId, string firstName, string lastName, string phone, string email, int membershipTypeId);

        /// <summary>
        /// Changes a member's password after verifying the current password.
        /// </summary>
        /// <exception cref="Model.Exceptions.InvalidCredentialsException">Thrown when the current password is wrong.</exception>
        void ChangePassword(int memberId, string currentPassword, string newPassword);
    }
}
