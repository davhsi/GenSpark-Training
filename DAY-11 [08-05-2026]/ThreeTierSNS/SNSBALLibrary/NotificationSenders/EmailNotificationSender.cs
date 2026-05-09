using System;
using SNSBALLibrary.Interfaces;
using SNSModelLibrary;

namespace SNSBALLibrary.NotificationSenders
{
    public class EmailNotificationSender : INotificationSender
    {   
        // method to send email notifications
        public void Send(User user, Notification notification)
        {
            Console.WriteLine($"Sending email to {user.Email}...");
            Console.WriteLine($"Message: {notification.Message}");
            Console.WriteLine($"Email sent to {user.Email} successfully!");
        }
    }
}
