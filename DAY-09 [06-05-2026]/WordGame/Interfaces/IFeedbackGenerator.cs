namespace WordGame.Interfaces;

public interface IFeedbackGenerator
{
    string GetFeedback(string guess, string target);
}
