using WordGame.Exceptions;
using WordGame.Models;
using WordGame.Repositories;

namespace WordGame;

class Program
{
    static void Main()
    {
        UserRepository userRepository = new UserRepository();
        ScoreRepository scoreRepository = new ScoreRepository();
        WordRepository wordRepository = new WordRepository();

        var engine = new GameEngine(wordRepository, new GuessValidator(), new FeedbackGenerator(), scoreRepository);

        User? currentUser = null;

        while (currentUser == null)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("*************** WORDLE ***************");
            Console.ResetColor();
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register");
            Console.Write("Choice: ");
            string? choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.Write("Username: ");
                string? username = Console.ReadLine();
                Console.Write("Password: ");
                string? password = Console.ReadLine();

                var user = userRepository.GetByUsername(username ?? "");
                if (user != null && user.Password == password)
                {
                    currentUser = user;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Login Successful!");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid username or password.");
                    Console.ResetColor();
                }
            }
            else if (choice == "2")
            {
                Console.Write("Username: ");
                string? username = Console.ReadLine();
                Console.Write("Password: ");
                string? password = Console.ReadLine();

                try
                {
                    if (userRepository.GetByUsername(username ?? "") != null)
                        throw new UserAlreadyExistsException("Username already taken.");

                    currentUser = userRepository.Create(new User { UserName = username ?? "", Password = password ?? "" });
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Registration Successful!");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
            }
        }

        bool exitGame = false;
        while (!exitGame)
        {
            Console.WriteLine("************************************");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n--- Main Menu ---");
            Console.ResetColor();
            Console.WriteLine("1. Play Wordle");
            Console.WriteLine("2. View Statistics");
            Console.WriteLine("3. Exit");
            Console.Write("Choice: ");
            string? mainChoice = Console.ReadLine();

            if (mainChoice == "1")
            {
                // Start game
                GameState state = engine.StartNewGame();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n*************** WORDLE ***************");
                Console.WriteLine($"Welcome, {currentUser.UserName}!");
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
                        GuessResult result = engine.ProcessGuess(input ?? string.Empty, state, currentUser);

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
            else if (mainChoice == "2")
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n--- Game Statistics ---");
                Console.ResetColor();

                // 1. Last 5 Scores
                Console.WriteLine("\n[ Your Last 5 Scores ]");
                var lastFive = scoreRepository.GetLastFiveScores(currentUser.UserId);
                if (lastFive.Any())
                {
                    foreach (var s in lastFive)
                    {
                        Console.Write("- ");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write(s.Value);
                        Console.ResetColor();
                        Console.WriteLine(" points");
                    }
                }
                else Console.WriteLine("No games played yet.");

                // 2. Best Score
                int best = scoreRepository.GetBestScore(currentUser.UserId);
                Console.Write("\n[ Your All-Time Best Score ]: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(best);
                Console.ResetColor();
                Console.WriteLine(" points");

                // 3. Global Top 5
                Console.WriteLine("\n[ Global Top 5 Scorers ]");
                var topScorers = scoreRepository.GetTopScorers(5);
                for (int i = 0; i < topScorers.Count; i++)
                {
                    Console.Write($"{i + 1}. {topScorers[i].Username}: ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(topScorers[i].Score);
                    Console.ResetColor();
                    Console.WriteLine(" points");
                }
            }
            else if (mainChoice == "3")
            {
                exitGame = true;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Goodbye!");
                Console.ResetColor();
            }
        }
    }
}