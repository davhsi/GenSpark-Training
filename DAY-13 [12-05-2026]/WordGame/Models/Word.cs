namespace WordGame.Models;
public class Word
{
    public int Id { get; set; }
    public string Text { get; set; } = "";

    public Word() { }

    public Word(int id, string text)
    {
        Id = id;
        Text = text;
    }
}