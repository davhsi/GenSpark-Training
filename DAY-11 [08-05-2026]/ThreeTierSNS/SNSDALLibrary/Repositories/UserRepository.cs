using SNSModelLibrary;

namespace SNSDALLibrary
{
    public class UserRepository
    {
        private readonly Dictionary<string, User> _users = new();

        public User Create(User user)
        {
            // using the indexer to add the user
            _users[user.Email] = user;
            return user;
        }

        public User? GetByEmail(string email)
        {
            // using TryGetValue to retrieve the user by email
            _users.TryGetValue(email, out var user);
            return user;
        }

        public bool Update(string email, User updatedUser)
        {
            if (!_users.ContainsKey(email)) return false;
            // using the indexer to update the user
            _users[email] = updatedUser;
            return true;
        }

        public bool Delete(string email)
        {
            return _users.Remove(email);
        }

        public List<User> GetAll()
        {
            return _users.Values.ToList();
        }
    }
}
