using SimpleNotificationSystem.Models;

namespace SimpleNotificationSystem.Services
{
    internal class UserService
    {
        private List<User> _users = new List<User>();

        public void AddUser(User user)
        {
            _users.Add(user);
        }

        public List<User> GetAllUsers()
        {
            return _users;
        }

        public User? GetUserByEmail(string email)
        {
            return _users.Find(u => u.Email == email);
        }

        public bool DeleteUser(string email)
        {
            User? user = GetUserByEmail(email);
            if (user != null)
            {
                _users.Remove(user);
                return true;
            }
            return false;
        }
    }
}