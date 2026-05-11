using SNSModelLibrary;
using SNSDALLibrary;

namespace SNSBALLibrary.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        private static readonly System.Text.RegularExpressions.Regex PhoneRegex = new(@"^\d{10}$");
        private static readonly System.Text.RegularExpressions.Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        private static readonly System.Text.RegularExpressions.Regex CountryCodeRegex = new(@"^\+\d{1,4}$");

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User AddUser(string firstName, string lastName, string email, string countryCode, string phoneNumber)
        {
            // name validation
            if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required.");
            if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required.");

            // email validation
            if (!EmailRegex.IsMatch(email))
                throw new SNSModelLibrary.Exceptions.InvalidEmailException("Invalid email format.");
            
            // duplicate check
            if (_userRepository.GetByEmail(email) != null)
                throw new SNSModelLibrary.Exceptions.UserAlreadyExistsException($"User with email '{email}' already exists.");

            // country code validation
            if (!CountryCodeRegex.IsMatch(countryCode))
                throw new ArgumentException("Country code must start with '+' followed by 1-4 digits (e.g., +91).");

            // phone validation
            if (!PhoneRegex.IsMatch(phoneNumber))
                throw new SNSModelLibrary.Exceptions.InvalidPhoneNumberException("Phone number must be exactly 10 digits.");

            var user = new User(firstName, lastName, email, countryCode, phoneNumber);
            return _userRepository.Create(user);
        }

        public User GetUserByEmail(string email)
        {
            var user = _userRepository.GetByEmail(email);
            if (user == null)
                // throw a custom exception if the user is not found
                throw new SNSModelLibrary.Exceptions.UserNotFoundException($"User '{email}' not found.");
            return user;
        }

        public List<User> GetAllUsers()
        {
            // using LINQ to order users by email in ascending order before returning the list
            return _userRepository.GetAll().OrderBy(u => u.Email).ToList();
        }

        public bool UpdateUser(string email, string? fName, string? lName, string? phone)
        {
            var user = _userRepository.GetByEmail(email);
            if (user == null) return false;

            if (!string.IsNullOrEmpty(phone))
            {
                if (!PhoneRegex.IsMatch(phone))
                    throw new SNSModelLibrary.Exceptions.InvalidPhoneNumberException("Phone number must be exactly 10 digits.");
                user.Phone.Number = phone;
            }
    
            if (!string.IsNullOrWhiteSpace(fName)) user.Name.FirstName = fName;
            if (!string.IsNullOrWhiteSpace(lName)) user.Name.LastName = lName;

            return _userRepository.Update(email, user);
        }

        public bool DeleteUser(string email)
        {
            return _userRepository.Delete(email);
        }
    }
}
