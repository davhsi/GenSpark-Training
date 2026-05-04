using SimpleNotificationSystem.Models;
using SimpleNotificationSystem.Interfaces;

namespace SimpleNotificationSystem.Services
{
    internal class NotificationManager
    {
        public void SendNotificationBySMS(User user)
        {
            INotificationService smsService = new SMSNotificationService();
            smsService.SendNotification(user.Phone.CountryCode + user.Phone.Number);
            Console.WriteLine();
        }

        public void SendNotificationByEmail(User user)
        {
            INotificationService emailService = new EmailNotificationService();
            emailService.SendNotification(user.Email);
            Console.WriteLine();
        }
    }
}
