using SNSModelLibrary;
using Npgsql;
using System.Data;

namespace SNSDALLibrary
{
    public class NotificationRepository
    {
        public void Create(Notification notification)
        {
            NpgsqlConnection connection = new NpgsqlConnection(DbConnectionHelper.ConnectionString);
            string query = $"INSERT INTO notifications (user_id, message, time_stamp, type) VALUES ({notification.UserId}, '{notification.Message}', '{notification.TimeStamp:yyyy-MM-dd HH:mm:ss}', '{notification.Type}') RETURNING id;";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);

            try
            {
                connection.Open();
                notification.Id = (int)command.ExecuteScalar()!;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating notification: " + ex.Message);
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        public List<Notification> GetAllWithUserDetails()
        {
            List<Notification> notifications = new List<Notification>();
            NpgsqlConnection connection = new NpgsqlConnection(DbConnectionHelper.ConnectionString);
            // INNER JOIN to get user details for each notification
            string query = """
                SELECT n.id, n.message, n.time_stamp, n.type, n.user_id, u.first_name, u.last_name, u.email 
                FROM notifications n 
                INNER JOIN users u ON n.user_id = u.id 
                ORDER BY n.time_stamp DESC;
                """;
            
            NpgsqlCommand command = new NpgsqlCommand(query, connection);

            try
            {
                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    notifications.Add(new Notification
                    {
                        Id = reader.GetInt32(0),
                        Message = reader.GetString(1),
                        TimeStamp = reader.GetDateTime(2),
                        Type = reader.GetString(3),
                        UserId = reader.GetInt32(4),
                        RecipientName = $"{reader.GetString(5)} {reader.GetString(6)} <{reader.GetString(7)}>"
                    });
                }
                reader.Close();
                return notifications;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching notifications with user details: " + ex.Message);
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        public List<Notification> GetAll()
        {
            return GetAllWithUserDetails();
        }
    }
}
