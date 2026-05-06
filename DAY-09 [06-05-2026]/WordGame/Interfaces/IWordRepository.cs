namespace WordGame.Interfaces;

public interface IWordRepository
{
    string GetRandomWord();
    IReadOnlyList<string> GetAllWords();
}
