namespace WordGame.Models;

public class GuessResult
{
    public string Guess { get; set; } = string.Empty;
    public string Feedback { get; set; } = string.Empty;  // e.g. "GXYXG"
    public bool IsCorrect { get; set; }
    public bool IsDuplicate { get; set; }
    public int? Score { get; set; }
    public string? AttemptComment { get; set; }
}
