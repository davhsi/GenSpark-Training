using WordGame.Interfaces;

namespace WordGame;

public class FeedbackGenerator : IFeedbackGenerator
{
    public string GetFeedback(string guess, string target)
    {
        char[] result = new char[5];
        Dictionary<char, int> targetCount = new Dictionary<char, int>();
        foreach (char c in target)
        {
            if (targetCount.ContainsKey(c)) targetCount[c]++;
            else targetCount[c] = 1;
        }
        // Firstly mark G , correct letter in correct place.
        for (int i = 0; i < 5; i++)
        {
            if (guess[i] == target[i])
            {
                result[i] = 'G';
                targetCount[guess[i]]--;
            }
        }
        // In the second try we mark the Ys and Xs, we are taking into account the number of times
        // a character appeared to and marking it in a way it is conveyed to the user.
        for (int i = 0; i < 5; i++)
        {
            if (result[i] == 'G') continue;
            if (targetCount.ContainsKey(guess[i]) && targetCount[guess[i]] > 0)
            {
                result[i] = 'Y';
                targetCount[guess[i]]--;
            }
            else
            {
                result[i] = 'X';
            }
        }
        return new string(result);
    }
}