using SNSModelLibrary;
using Microsoft.EntityFrameworkCore;

namespace SNSDALLibrary
{
    public class UserRepository
    {
        private readonly UserContext _context;

        public UserRepository()
        {
            _context = new UserContext();
        }

        public User Create(User user)
        {
            try
            {
                _context.Users.Add(user);
                _context.SaveChanges();
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating user: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        public User? GetByEmail(string email)
        {
            try
            {
                return _context.Users.FirstOrDefault(u => u.Email == email);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching user by email: " + ex.Message);
            }
        }

        public bool Update(string email, User updatedUser)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == email);
                if (user == null) return false;

                user.Name.FirstName = updatedUser.Name.FirstName;
                user.Name.LastName = updatedUser.Name.LastName;
                user.Phone.CountryCode = updatedUser.Phone.CountryCode;
                user.Phone.Number = updatedUser.Phone.Number;

                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating user: " + ex.Message);
            }
        }

        public bool Delete(string email)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == email);
                if (user == null) return false;

                _context.Users.Remove(user);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting user: " + ex.Message);
            }
        }

        public List<User> GetAll()
        {
            try
            {
                return _context.Users.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching all users: " + ex.Message);
            }
        }
    }
}
