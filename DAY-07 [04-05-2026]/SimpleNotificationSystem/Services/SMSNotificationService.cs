namespace SimpleNotificationSystem.Services
{
    internal class SMSNotificationService : Interfaces.INotificationService
    {
        public void SendNotification(string contactInfo, string message)
        {
            Console.WriteLine($"Sending SMS notification to {contactInfo}");
            Console.WriteLine($"Message: {message}");
        }
    }
}