namespace SimpleNotificationSystem.Services
{
    internal class EmailNotificationService : Interfaces.INotificationService
    {
        public void SendNotification(string contactInfo)
        {
            Console.WriteLine($"Sending email notification to {contactInfo}");
        }
    }
}