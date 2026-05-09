using SNSModelLibrary;
using SNSDALLibrary;

namespace SNSBALLibrary.Services
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
    
            if (!string.IsNullOrEmpty(fName)) user.Name.FirstName = fName;
            if (!string.IsNullOrEmpty(lName)) user.Name.LastName = lName;
            if (!string.IsNullOrEmpty(phone)) user.Phone.Number = phone;

            return _userRepository.Update(email, user);
        }

        public bool DeleteUser(string email)
        {
            return _userRepository.Delete(email);
        }
    }
}
