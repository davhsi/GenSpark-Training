using System.Text.RegularExpressions;
using SNSModelLibrary.Exceptions;
using SNSBALLibrary.Interfaces;
using SNSBALLibrary.NotificationSenders;
using SNSDALLibrary;
using SNSModelLibrary;

namespace SNSBALLibrary.Services
{
    public class NotificationService
    {
        private readonly NotificationRepository _repository;

        // email validation regex pattern to ensure valid email format
        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        public NotificationService(NotificationRepository repository)
        {
            _repository = repository;
        }

        public void ProcessAndSendNotification(User user, string type, string message)
        {
            if (string.IsNullOrWhiteSpace(message) || message.Length < 5 || message.Length > 160)
                throw new ArgumentException("Message must be between 5 and 160 characters.");

            INotificationSender sender;


            // email notifcation part
            // checks if the type is email, validates the email format,
            // and creates an EmailNotificationSender instance
            if (type.Equals("Email", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(user.Email) || !EmailRegex.IsMatch(user.Email))
                    throw new InvalidEmailException("Invalid email address.");
                sender = new EmailNotificationSender();
            }

            // sms notification part
            // checks if the type is SMS, validates the phone number format and length,
            // and creates an SmsNotificationSender instance
            else if (type.Equals("SMS", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(user.Phone?.Number) || user.Phone.Number.Length < 10)
                    throw new InvalidPhoneNumberException("Invalid phone number.");
                sender = new SmsNotificationSender();
            }

            // if the type is neither email nor SMS, throw an exception for unsupported type
            else throw new ArgumentException("Unsupported type.");

            var notification = new Notification(user.Id, message, type);
            sender.Send(user, notification);
            _repository.Create(notification); // save the notification to the repository
        }

        public void SendToAllNotifications(List<User> users, string type, string message)
        {
            if (users == null || users.Count == 0)
                throw new ArgumentException("User list is empty.");

            foreach (var user in users)
            {
                ProcessAndSendNotification(user, type, message);
            }
        }
    }
}
