using SNSModelLibrary;

namespace SNSBALLibrary
{
    internal interface INotificationService
    {
        bool SendNotification(User user, string message);
    }
}
