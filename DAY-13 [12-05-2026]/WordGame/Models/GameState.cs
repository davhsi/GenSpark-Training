namespace WordGame.Models;

public class GameState
{
    public string SecretWord { get; set; } = string.Empty;
    public int AttemptsLeft { get; set; } = 6;
    public bool IsGameOver { get; set; } = false;
    public bool IsWon { get; set; } = false;
    public int Score { get; set; } = 0;
    public HashSet<string> PreviousGuesses { get; set; } = new HashSet<string>();

    public int AttemptsMade => 6 - AttemptsLeft;
}
