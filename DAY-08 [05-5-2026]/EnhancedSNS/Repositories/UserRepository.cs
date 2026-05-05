using EnhancedSNS.Interfaces;
using EnhancedSNS.Models;

namespace EnhancedSNS.Repositories
{
    internal class UserRepository : AbstractRepository<int, User>
    {
        private int _nextId = 1;

        public override User Create(User item)
        {
            var key = _nextId++;
            _items[key] = item;
            return item;
        }

        public User? GetByEmail(string email)
        {
            return _items.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public List<User>? GetByFirstName(string firstName)
        {
            var users = _items.Values.Where(u => u.Name.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase)).ToList();
            return users.Count > 0 ? users : null;
        }
    }
}
