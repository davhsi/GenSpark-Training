using WordGame.Interfaces;

namespace WordGame.Repositories;

public class WordRepository : IWordRepository
{
    private readonly string[] _words = { "APPLE", "MANGO", "GRAPE", "TRAIN", "PLANT", "BRAIN" };
    private readonly Random _random = new Random();

    public string GetRandomWord()
    {
        return _words[_random.Next(_words.Length)];
    }

    public IReadOnlyList<string> GetAllWords()
    {
        return Array.AsReadOnly(_words);
    }
}
