using System;
using SNSBALLibrary.Interfaces;
using SNSModelLibrary;

namespace SNSBALLibrary.NotificationSenders
{
    public class SmsNotificationSender : INotificationSender
    {
        // method to send SMS notifications
        public void Send(User user, Notification notification)
        {
            Console.WriteLine($"Sending SMS to {user.Phone.CountryCode} {user.Phone.Number}...");
            Console.WriteLine($"Message: {notification.Message}");
            Console.WriteLine($"SMS sent to {user.Phone.CountryCode} {user.Phone.Number} successfully!");
        }
    }
}
