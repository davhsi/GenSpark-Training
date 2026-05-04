namespace SimpleNotificationSystem.Services
{
    internal class SMSNotificationService : Interfaces.INotificationService
    {
        public void SendNotification(string contactInfo)
        {
            Console.WriteLine($"Sending SMS notification to {contactInfo}");
        }
    }
}