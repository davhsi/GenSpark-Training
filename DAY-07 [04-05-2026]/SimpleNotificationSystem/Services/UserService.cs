using SimpleNotificationSystem.Models;
using SimpleNotificationSystem.Interfaces;
using System.Linq;

namespace SimpleNotificationSystem.Services
{
    internal class UserService
    {
        public void AddUser(List<User> users, User user)
        {
            users.Add(user);
        }

        public User? GetUserByEmail(List<User> users, string email)
        {
            return users.Find(u => u.Email == email);
        }

        public User? GetUserByPhoneNumber(List<User> users, string phoneNumber)
        {
            return users.Find(u => u.Phone.Number == phoneNumber);
        }

        public void DeleteUser(List<User> users, User user)
        {
            users.Remove(user);
        }
    }
}