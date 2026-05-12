namespace WordGame.Models;

public class User
{
    public int UserId { get; set; }
    public string UserName { get; set; } = "Guest";
    public string Password { get; set; } = "password";

    public User() { }

    public User(int userId, string userName, string password)
    {
        UserId = userId;
        UserName = userName;
        Password = password;
    }
}
