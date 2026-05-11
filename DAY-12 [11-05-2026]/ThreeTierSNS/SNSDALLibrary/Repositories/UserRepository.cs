using SNSModelLibrary;
using Npgsql;
using System.Data;

namespace SNSDALLibrary
{
    public class UserRepository
    {
        public User Create(User user)
        {
            NpgsqlConnection connection = new NpgsqlConnection(DbConnectionHelper.ConnectionString);
            string query = $"INSERT INTO users (first_name, last_name, email, country_code, phone_number) VALUES ('{user.Name.FirstName}', '{user.Name.LastName}', '{user.Email}', '{user.Phone.CountryCode}', '{user.Phone.Number}') RETURNING id;";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);

            try
            {
                connection.Open();
                user.Id = (int)command.ExecuteScalar()!;
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating user: " + ex.Message);
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        public User? GetByEmail(string email)
        {
            NpgsqlConnection connection = new NpgsqlConnection(DbConnectionHelper.ConnectionString);
            string query = $"SELECT id, first_name, last_name, email, country_code, phone_number FROM users WHERE email = '{email}';";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);

            try
            {
                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    User user = new User
                    {
                        Id = reader.GetInt32(0),
                        Name = new Name { FirstName = reader.GetString(1), LastName = reader.GetString(2) },
                        Email = reader.GetString(3),
                        Phone = new Phone { CountryCode = reader.GetString(4), Number = reader.GetString(5) }
                    };
                    reader.Close();
                    return user;
                }
                reader.Close();
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching user by email: " + ex.Message);
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        public bool Update(string email, User updatedUser)
        {
            NpgsqlConnection connection = new NpgsqlConnection(DbConnectionHelper.ConnectionString);
            string query = $"UPDATE users SET first_name = '{updatedUser.Name.FirstName}', last_name = '{updatedUser.Name.LastName}', country_code = '{updatedUser.Phone.CountryCode}', phone_number = '{updatedUser.Phone.Number}' WHERE email = '{email}';";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);

            try
            {
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating user: " + ex.Message);
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        public bool Delete(string email)
        {
            NpgsqlConnection connection = new NpgsqlConnection(DbConnectionHelper.ConnectionString);
            string query = $"DELETE FROM users WHERE email = '{email}';";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);

            try
            {
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting user: " + ex.Message);
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        public List<User> GetAll()
        {
            List<User> users = new List<User>();
            NpgsqlConnection connection = new NpgsqlConnection(DbConnectionHelper.ConnectionString);
            string query = "SELECT id, first_name, last_name, email, country_code, phone_number FROM users;";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);

            try
            {
                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new User
                    {
                        Id = reader.GetInt32(0),
                        Name = new Name { FirstName = reader.GetString(1), LastName = reader.GetString(2) },
                        Email = reader.GetString(3),
                        Phone = new Phone { CountryCode = reader.GetString(4), Number = reader.GetString(5) }
                    });
                }
                reader.Close();
                return users;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching all users: " + ex.Message);
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
    }
}
