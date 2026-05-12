using Npgsql;
using System.Data;
using WordGame.Interfaces;
using WordGame.Models;

namespace WordGame.Repositories;

public class ScoreRepository : IScoreRepository
{
    public void AddScore(Score score)
    {
        NpgsqlConnection connection = new NpgsqlConnection(DbConnectionHelper.ConnectionString);
        string query = $"INSERT INTO scores (userid, score) VALUES ({score.UserId}, {score.Value});";
        NpgsqlCommand command = new NpgsqlCommand(query, connection);

        try
        {
            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new Exception("Error adding score: " + ex.Message);
        }
        finally
        {
            connection?.Close();
        }
    }

    public List<Score> GetScoresByUserId(int userId)
    {
        NpgsqlConnection connection = new NpgsqlConnection(DbConnectionHelper.ConnectionString);
        string query = $"SELECT * FROM scores WHERE userid = {userId};";
        NpgsqlCommand command = new NpgsqlCommand(query, connection);
        List<Score> scores = new List<Score>();

        try
        {
            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    scores.Add(new Score
                    {
                        Id = reader.GetInt32(0),
                        UserId = reader.GetInt32(1),
                        Value = reader.GetInt32(2)
                    });
                }
            }
            return scores;
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching scores: " + ex.Message);
        }
        finally
        {
            connection?.Close();
        }
    }
    
    public List<Score> GetLastFiveScores(int userId)
    {
        NpgsqlConnection connection = new NpgsqlConnection(DbConnectionHelper.ConnectionString);
        string query = $"SELECT * FROM scores WHERE userid = {userId} ORDER BY id DESC LIMIT 5;";
        NpgsqlCommand command = new NpgsqlCommand(query, connection);
        List<Score> scores = new List<Score>();

        try
        {
            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    scores.Add(new Score
                    {
                        Id = reader.GetInt32(0),
                        UserId = reader.GetInt32(1),
                        Value = reader.GetInt32(2)
                    });
                }
            }
            return scores;
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching last five scores: " + ex.Message);
        }
        finally
        {
            connection?.Close();
        }
    }

    public int GetBestScore(int userId)
    {
        NpgsqlConnection connection = new NpgsqlConnection(DbConnectionHelper.ConnectionString);
        string query = $"SELECT MAX(score) FROM scores WHERE userid = {userId};";
        NpgsqlCommand command = new NpgsqlCommand(query, connection);

        try
        {
            connection.Open();
            var result = command.ExecuteScalar();
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching best score: " + ex.Message);
        }
        finally
        {
            connection?.Close();
        }
    }

    public List<(string Username, int Score)> GetTopScorers(int count)
    {
        NpgsqlConnection connection = new NpgsqlConnection(DbConnectionHelper.ConnectionString);
        string query = $"SELECT u.username, MAX(s.score) as best_score FROM scores s JOIN users u ON s.userid = u.userid GROUP BY u.username ORDER BY best_score DESC LIMIT {count};";
        NpgsqlCommand command = new NpgsqlCommand(query, connection);
        var leaders = new List<(string Username, int Score)>();

        try
        {
            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    leaders.Add((reader.GetString(0), reader.GetInt32(1)));
                }
            }
            return leaders;
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching top scorers: " + ex.Message);
        }
        finally
        {
            connection?.Close();
        }
    }
}
