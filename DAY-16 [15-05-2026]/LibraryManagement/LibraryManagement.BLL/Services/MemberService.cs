using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using LibraryManagement.BLL.Interfaces;
using LibraryManagement.DAL.Contexts;
using LibraryManagement.DAL.Repositories.Implementations;
using LibraryManagement.DAL.Repositories.Interfaces;
using LibraryManagement.Model.Exceptions;
using LibraryManagement.Model.Models;

namespace LibraryManagement.BLL.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _repository;

        public MemberService()
        {
            _repository = new MemberRepository(new LibraryContext());
        }

        // Hashes a plaintext password using SHA-256
        private static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }

        // Validates email format using a standard pattern
        private static bool IsValidEmail(string email) =>
            Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        // Validates phone: exactly 10 digits
        private static bool IsValidPhone(string phone) =>
            Regex.IsMatch(phone, @"^\d{10}$");

        /// <exception cref="InvalidEmailException"/>
        /// <exception cref="InvalidPhoneException"/>
        /// <exception cref="DuplicateEmailException"/>
        /// <exception cref="InvalidMembershipTypeException"/>
        public void RegisterMember(Member member, string password)
        {
            if (!IsValidEmail(member.Email!))
                throw new InvalidEmailException(member.Email!);

            if (!IsValidPhone(member.Phone!))
                throw new InvalidPhoneException(member.Phone!);

            if (_repository.GetByEmail(member.Email!) != null)
                throw new DuplicateEmailException(member.Email!);

            if (_repository.GetByPhone(member.Phone!) != null)
                throw new DuplicatePhoneException(member.Phone!);

            var types = _repository.GetMembershipTypes();
            if (!types.Any(t => t.Id == member.MembershipTypeId))
                throw new InvalidMembershipTypeException(member.MembershipTypeId);

            member.Password = HashPassword(password);
            _repository.Add(member);
        }

        /// <exception cref="MemberNotFoundException"/>
        /// <exception cref="MemberInactiveException"/>
        /// <exception cref="InvalidCredentialsException"/>
        public Member Login(string email, string password)
        {
            var member = _repository.GetByEmail(email);

            if (member == null)
                throw new MemberNotFoundException(-1);

            if (!member.IsActive)
                throw new MemberInactiveException(member.Id);

            if (member.Password != HashPassword(password))
                throw new InvalidCredentialsException();

            return member;
        }

        public List<Member> GetAllMembers()
        {
            return _repository.GetAllWithMembership();
        }

        public Member? GetMemberById(int id)
        {
            return _repository.GetByIdWithDetails(id);
        }

        public List<MembershipType> GetMembershipTypes()
        {
            return _repository.GetMembershipTypes();
        }

        public List<Member> SearchMembers(string keyword)
        {
            return _repository.SearchMembers(keyword);
        }

        /// <summary>
        /// Deactivation is blocked if the member has unreturned books or unpaid fines.
        /// Activation has no guards.
        /// </summary>
        /// <exception cref="MemberHasActiveBorrowingsException"/>
        /// <exception cref="MemberHasUnpaidFinesException"/>
        public void UpdateMemberStatus(int memberId, bool isActive)
        {
            // Guards only apply when deactivating
            if (!isActive)
            {
                var member = _repository.GetByIdWithDetails(memberId);
                if (member == null)
                    throw new MemberNotFoundException(memberId);

                int activeBorrowings = member.Borrowings.Count(b => b.Status == "Borrowed");
                if (activeBorrowings > 0)
                    throw new MemberHasActiveBorrowingsException(activeBorrowings);

                decimal unpaidFines = member.Fines
                    .Where(f => !f.IsPaid)
                    .Sum(f => f.Amount);
                if (unpaidFines > 0)
                    throw new MemberHasUnpaidFinesException(unpaidFines);
            }

            _repository.UpdateMemberStatus(memberId, isActive);
        }

        /// <exception cref="MemberNotFoundException"/>
        /// <exception cref="InvalidEmailException"/>
        /// <exception cref="InvalidPhoneException"/>
        /// <exception cref="DuplicateEmailException"/>
        /// <exception cref="DuplicatePhoneException"/>
        /// <exception cref="InvalidMembershipTypeException"/>
        public void UpdateProfile(int memberId, string firstName, string lastName, string phone, string email, int membershipTypeId)
        {
            var member = _repository.GetByIdWithDetails(memberId);
            if (member == null)
                throw new MemberNotFoundException(memberId);

            if (!IsValidEmail(email))
                throw new InvalidEmailException(email);

            if (!IsValidPhone(phone))
                throw new InvalidPhoneException(phone);

            // Check email uniqueness — exclude the member's own current email
            var emailOwner = _repository.GetByEmail(email);
            if (emailOwner != null && emailOwner.Id != memberId)
                throw new DuplicateEmailException(email);

            // Check phone uniqueness — exclude the member's own current phone
            var phoneOwner = _repository.GetByPhone(phone);
            if (phoneOwner != null && phoneOwner.Id != memberId)
                throw new DuplicatePhoneException(phone);

            var types = _repository.GetMembershipTypes();
            if (!types.Any(t => t.Id == membershipTypeId))
                throw new InvalidMembershipTypeException(membershipTypeId);

            _repository.UpdateProfile(memberId, firstName, lastName, phone, email, membershipTypeId);
        }

        /// <exception cref="MemberNotFoundException"/>
        /// <exception cref="InvalidCredentialsException"/>
        public void ChangePassword(int memberId, string currentPassword, string newPassword)
        {
            var member = _repository.GetByIdWithDetails(memberId);
            if (member == null)
                throw new MemberNotFoundException(memberId);

            if (member.Password != HashPassword(currentPassword))
                throw new InvalidCredentialsException();

            _repository.UpdatePassword(memberId, HashPassword(newPassword));
        }
    }
}
