namespace WordGame.Models;
public class Score
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Value { get; set; }

    public Score() { }

    public Score(int id, int userId, int value)
    {
        Id = id;
        UserId = userId;
        Value = value;
    }
}