using WordGame.Interfaces;
using WordGame.Models;

namespace WordGame;

public class GameEngine : IGameEngine
{
    private readonly IWordRepository _wordRepository;
    private readonly IGuessValidator _validator;
    private readonly IFeedbackGenerator _feedbackGenerator;
    private readonly IScoreRepository _scoreRepository;

    private readonly string[] _attemptComments =
    {
        "Genius!",        // Attempt 1
        "Excellent!",     // Attempt 2
        "Great job!",     // Attempt 3
        "Good work!",     // Attempt 4
        "Nice try!",      // Attempt 5
        "That was close!" // Attempt 6
    };

    public GameEngine(IWordRepository wordRepository, IGuessValidator validator, IFeedbackGenerator feedbackGenerator, IScoreRepository scoreRepository)
    {
        _wordRepository = wordRepository;
        _validator = validator;
        _feedbackGenerator = feedbackGenerator;
        _scoreRepository = scoreRepository;
    }

    public GameState StartNewGame()
    {
        return new GameState
        {
            SecretWord = _wordRepository.GetRandomWord(),
            AttemptsLeft = 6
        };
    }

    public GuessResult ProcessGuess(string input, GameState state)
    {
        return ProcessGuess(input, state, null);
    }

    public GuessResult ProcessGuess(string input, GameState state, User? currentUser)
    {
        string guess = _validator.Validate(input);

        if (state.PreviousGuesses.Contains(guess))
        {
            return new GuessResult
            {
                Guess = guess,
                IsDuplicate = true
            };
        }

        state.PreviousGuesses.Add(guess);
        string feedback = _feedbackGenerator.GetFeedback(guess, state.SecretWord);
        bool isCorrect = guess == state.SecretWord;

        var result = new GuessResult
        {
            Guess = guess,
            Feedback = feedback,
            IsCorrect = isCorrect
        };

        if (isCorrect)
        {
            int attemptNumber = state.AttemptsMade + 1;
            state.Score = CalculateScore(attemptNumber);
            state.IsWon = true;
            state.IsGameOver = true;
            result.Score = state.Score;
            result.AttemptComment = _attemptComments[attemptNumber - 1];

            if (currentUser != null)
            {
                _scoreRepository.AddScore(new Score { UserId = currentUser.UserId, Value = state.Score });
            }
        }
        else
        {
            state.AttemptsLeft--;
            if (state.AttemptsLeft <= 0)
            {
                state.IsGameOver = true;
            }
        }

        return result;
    }

    private int CalculateScore(int attemptNumber)
    {
        return 100 - ((attemptNumber - 1) * 10);
    }
}