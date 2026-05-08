using SNSModelLibrary;

namespace SNSBALLibrary
{
    public class EmailNotificationService : INotificationService
    {
        public bool SendNotification(User user, string message)
        {
            if (string.IsNullOrEmpty(user.Email))
            {
                Console.WriteLine("Cannot send email: User email is empty");
                return false;
            }

            Console.WriteLine($"Message sent successfully via Email to {user.Email}: {message}");
            return true;
        }
    }
}
