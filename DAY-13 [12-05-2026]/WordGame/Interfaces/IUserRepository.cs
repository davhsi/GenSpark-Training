using WordGame.Models;

namespace WordGame.Interfaces;

public interface IUserRepository
{
    User Create(User user);
    User? GetByUsername(string username);
}
