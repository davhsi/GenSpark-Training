using WordGame.Exceptions;
using WordGame.Models;
using WordGame.Repositories;

namespace WordGame;

class Program
{
    static void Main()
    {
        var engine = new GameEngine(new WordRepository(), new GuessValidator(), new FeedbackGenerator());

        // Start game
        GameState state = engine.StartNewGame();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n*************** WORDLE ***************");
        Console.ResetColor();
        Console.WriteLine("Guess the secret 5 letter word. Total Attempts: 6.\n");

        while (!state.IsGameOver)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Attempts left: {state.AttemptsLeft}");
            Console.ResetColor();
            Console.Write("Please enter your guess: ");
            string? input = Console.ReadLine();

            try
            {
                GuessResult result = engine.ProcessGuess(input ?? string.Empty, state);

                if (result.IsDuplicate)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Oops! You already entered this guess word '{result.Guess}'!");
                    Console.ResetColor();
                    continue;
                }

                // Display guess and feedback with colors
                Console.WriteLine();
                for (int i = 0; i < result.Guess.Length; i++)
                    Console.Write(result.Guess[i] + " ");
                Console.WriteLine();

                for (int i = 0; i < result.Feedback.Length; i++)
                {
                    Console.ForegroundColor = result.Feedback[i] switch
                    {
                        'G' => ConsoleColor.Green,
                        'Y' => ConsoleColor.Yellow,
                        'X' => ConsoleColor.Gray,
                        _ => ConsoleColor.White
                    };
                    Console.Write(result.Feedback[i] + " ");
                }
                Console.ResetColor();
                Console.WriteLine("\n");

                if (result.IsCorrect)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"You guessed the word successfully - {result.AttemptComment}");
                    Console.WriteLine($"Score: {result.Score} points");
                    Console.ResetColor();
                }
            }
            catch (InvalidGuessException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
        }

        if (!state.IsWon)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nGame Over! The secret word was: {state.SecretWord}");
            Console.WriteLine("That was close!");
            Console.ResetColor();
        }
    }
}