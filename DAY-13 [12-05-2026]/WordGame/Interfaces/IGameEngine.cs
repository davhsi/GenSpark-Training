using WordGame.Models;

namespace WordGame.Interfaces;

public interface IGameEngine
{
    GameState StartNewGame();
    GuessResult ProcessGuess(string input, GameState state);
    GuessResult ProcessGuess(string input, GameState state, User? user);
}
