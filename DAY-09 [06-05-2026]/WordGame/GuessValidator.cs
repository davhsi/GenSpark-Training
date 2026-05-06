using WordGame.Exceptions;
using WordGame.Interfaces;

namespace WordGame;

public class GuessValidator : IGuessValidator
{
    public string Validate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new InvalidGuessException("Empty input");
            
        string guess = input.Trim().ToUpper();
        
        if (guess.Length != 5)
            throw new InvalidGuessException("Guess word must be 5 letters");
            
        foreach (char c in guess)
            if (!char.IsLetter(c))
                throw new InvalidGuessException("Invalid, Guess word only contains letters");
                
        return guess;
    }
}