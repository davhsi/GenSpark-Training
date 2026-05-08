using SNSModelLibrary;

namespace SNSBALLibrary
{
    public class SMSNotificationService : INotificationService
    {
        public bool SendNotification(User user, string message)
        {
            if (string.IsNullOrEmpty(user.Phone.Number))
            {
                Console.WriteLine("Cannot send SMS: User phone number is empty");
                return false;
            }

            Console.WriteLine($"Message sent successfully via SMS to {user.Phone.CountryCode} {user.Phone.Number}: {message}");
            return true;
        }
    }
}
