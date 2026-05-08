using SNSModelLibrary;
using SNSDALLibrary;

namespace SNSBALLibrary
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User AddUser(string firstName, string lastName, string email, string countryCode, string phoneNumber)
        {
            var user = new User(firstName, lastName, email, countryCode, phoneNumber);
            return _userRepository.Create(user);
        }

        public List<User>? GetAllUsers()
        {
            var users = _userRepository.GetAccounts();
            if (users != null)
            {
                users.Sort(); // Uses IComparable implementation
            }
            return users;
        }

        public User? GetUserByEmail(string email)
        {
            return _userRepository.GetByEmail(email);
        }

        public List<User>? GetUsersByFirstName(string firstName)
        {
            return _userRepository.GetByFirstName(firstName);
        }

        public bool UpdateUser(string email, string? newFirstName = null, string? newLastName = null, 
                              string? newEmail = null, string? newPhoneNumber = null)
        {
            var existingUser = _userRepository.GetByEmail(email);
            if (existingUser == null)
                return false;

            if (!string.IsNullOrEmpty(newFirstName))
                existingUser.Name.FirstName = newFirstName;

            if (!string.IsNullOrEmpty(newLastName))
                existingUser.Name.LastName = newLastName;

            if (!string.IsNullOrEmpty(newEmail))
                existingUser.Email = newEmail;

            if (!string.IsNullOrEmpty(newPhoneNumber))
                existingUser.Phone.Number = newPhoneNumber;

            // Note: In a real implementation with proper key management,
            // we would call _userRepository.Update(key, user)
            return true;
        }

        public bool DeleteUser(string email)
        {
            var user = _userRepository.GetByEmail(email);
            if (user == null)
                return false;

            // Note: In a real implementation with proper key management,
            // we would call _userRepository.Delete(key)
            return true;
        }
    }
}
