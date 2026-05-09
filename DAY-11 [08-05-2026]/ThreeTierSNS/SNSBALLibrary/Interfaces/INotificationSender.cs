using SNSModelLibrary;

namespace SNSBALLibrary.Interfaces
{
    public interface INotificationSender
    {
        void Send(User user, Notification notification);
    }
}
