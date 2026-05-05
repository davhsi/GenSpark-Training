namespace SimpleNotificationSystem.Services
{
    internal class EmailNotificationService : Interfaces.INotificationService
    {
        public void SendNotification(string contactInfo, string message)
        {
            Console.WriteLine($"Sending email notification to {contactInfo}");
            Console.WriteLine($"Message: {message}");
        }
    }
}