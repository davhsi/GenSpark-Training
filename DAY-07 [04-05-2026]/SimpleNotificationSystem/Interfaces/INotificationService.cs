namespace SimpleNotificationSystem.Interfaces
{
    internal interface INotificationService
    {
       public void SendNotification(string contactInfo);
       // both EmailNotificationService and EmailNotificationService must implement this method.
       // Future WhatsAppNotificationService also must implement it.
    }
}