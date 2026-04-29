using System.Security.Cryptography;
using System.Text;

namespace Busly.API.Services;

public interface ICaptchaService
{
    string GenerateCaptcha();
    bool ValidateCaptcha(string userInput, string sessionCaptcha);
}

public class CaptchaService : ICaptchaService
{
    // Use static dictionary to persist across service instances
    private static readonly Dictionary<string, CaptchaSession> _sessions = new();
    private static readonly object _lock = new();

    public string GenerateCaptcha()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        var captcha = new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        var sessionId = Guid.NewGuid().ToString("N")[..8];
        
        Console.WriteLine($"=== CAPTCHA GENERATION ===");
        Console.WriteLine($"Generated Captcha: '{captcha}'");
        Console.WriteLine($"Session ID: '{sessionId}'");
        
        lock (_lock)
        {
            _sessions[sessionId] = new CaptchaSession
            {
                CaptchaText = captcha,
                CreatedAt = DateTime.UtcNow,
                Attempts = 0
            };
            Console.WriteLine($"Stored session. Total sessions: {_sessions.Count}");
        }

        return $"{sessionId}:{captcha}";
    }

    public bool ValidateCaptcha(string userInput, string sessionToken)
    {
        Console.WriteLine($"=== CAPTCHA VALIDATION DEBUG ===");
        Console.WriteLine($"User Input: '{userInput}'");
        Console.WriteLine($"Session Token: '{sessionToken}'");
        
        if (string.IsNullOrEmpty(sessionToken) || !sessionToken.Contains(':'))
        {
            Console.WriteLine("ERROR: Invalid session token format");
            return false;
        }

        var parts = sessionToken.Split(':', 2);
        if (parts.Length != 2)
        {
            Console.WriteLine("ERROR: Session token split failed");
            return false;
        }

        var sessionId = parts[0];
        Console.WriteLine($"Session ID: '{sessionId}'");
        
        lock (_lock)
        {
            Console.WriteLine($"Total active sessions: {_sessions.Count}");
            foreach (var kvp in _sessions)
            {
                Console.WriteLine($"Session: {kvp.Key} -> {kvp.Value.CaptchaText} (Created: {kvp.Value.CreatedAt}, Attempts: {kvp.Value.Attempts})");
            }
            
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                Console.WriteLine($"ERROR: Session ID '{sessionId}' not found");
                return false;
            }

            Console.WriteLine($"Found session - Expected Captcha: '{session.CaptchaText}', Created: {session.CreatedAt}, Attempts: {session.Attempts}");

            // Check if expired (5 minutes)
            var age = DateTime.UtcNow - session.CreatedAt;
            Console.WriteLine($"Session age: {age.TotalMinutes:F2} minutes");
            if (age > TimeSpan.FromMinutes(5))
            {
                Console.WriteLine("ERROR: Session expired");
                _sessions.Remove(sessionId);
                return false;
            }

            // Check attempts (max 3)
            if (session.Attempts >= 3)
            {
                Console.WriteLine("ERROR: Max attempts exceeded");
                _sessions.Remove(sessionId);
                return false;
            }

            session.Attempts++;
            Console.WriteLine($"Incremented attempts to: {session.Attempts}");

            var isMatch = userInput.Equals(session.CaptchaText, StringComparison.OrdinalIgnoreCase);
            Console.WriteLine($"Captcha match result: {isMatch} (User: '{userInput}' vs Expected: '{session.CaptchaText}')");
            
            if (isMatch)
            {
                Console.WriteLine("SUCCESS: Captcha validated, removing session");
                _sessions.Remove(sessionId);
                return true;
            }

            Console.WriteLine("FAILED: Captcha mismatch");
            return false;
        }
    }

    private class CaptchaSession
    {
        public string CaptchaText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int Attempts { get; set; }
    }
}
