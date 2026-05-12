using Npgsql;
using System.Data;
using WordGame.Interfaces;

namespace WordGame.Repositories;

public class WordRepository : IWordRepository
{
    public string GetRandomWord()
    {
        NpgsqlConnection connection = new NpgsqlConnection(DbConnectionHelper.ConnectionString);
        string query = "SELECT word FROM words ORDER BY RANDOM() LIMIT 1;";
        NpgsqlCommand command = new NpgsqlCommand(query, connection);

        try
        {
            connection.Open();
            return (string)command.ExecuteScalar()!;
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching random word: " + ex.Message);
        }
        finally
        {
            connection?.Close();
        }
    }

    public IReadOnlyList<string> GetAllWords()
    {
        NpgsqlConnection connection = new NpgsqlConnection(DbConnectionHelper.ConnectionString);
        string query = "SELECT word FROM words;";
        NpgsqlCommand command = new NpgsqlCommand(query, connection);
        List<string> words = new List<string>();

        try
        {
            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    words.Add(reader.GetString(0));
                }
            }
            return words.AsReadOnly();
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching all words: " + ex.Message);
        }
        finally
        {
            connection?.Close();
        }
    }
}
