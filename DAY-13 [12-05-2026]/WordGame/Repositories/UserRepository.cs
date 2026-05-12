using Npgsql;
using WordGame.Interfaces;
using WordGame.Models;

namespace WordGame.Repositories;

public class UserRepository : IUserRepository
{
    public User Create(User user)
    {
        NpgsqlConnection connection = new NpgsqlConnection(DbConnectionHelper.ConnectionString);
        string query = $"INSERT INTO users (username, password) VALUES ('{user.UserName}', '{user.Password}') RETURNING userid;";
        NpgsqlCommand command = new NpgsqlCommand(query, connection);

        try
        {
            connection.Open();
            // Console.WriteLine("Connection success");
            user.UserId = (int)command.ExecuteScalar()!;
            return user;
        }
        catch (Exception ex)
        {
            throw new Exception("Error creating user: " + ex.Message);
        }
        finally
        {
            connection?.Close();
        }
    }

    public User? GetByUsername(string username)
    {
        NpgsqlConnection connection = new NpgsqlConnection(DbConnectionHelper.ConnectionString);
        string query = $"SELECT * FROM users WHERE username = '{username}';";
        NpgsqlCommand command = new NpgsqlCommand(query, connection);

        try
        {
            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new User
                    {
                        UserId = reader.GetInt32(0),
                        UserName = reader.GetString(1),
                        Password = reader.GetString(2)
                    };
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching user: " + ex.Message);
        }
        finally
        {
            connection?.Close();
        }
    }
}
