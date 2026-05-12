using WordGame.Models;

namespace WordGame.Interfaces;

public interface IScoreRepository
{
    void AddScore(Score score);
    List<Score> GetScoresByUserId(int userId);
    List<Score> GetLastFiveScores(int userId);
    int GetBestScore(int userId);
    List<(string Username, int Score)> GetTopScorers(int count);
}
