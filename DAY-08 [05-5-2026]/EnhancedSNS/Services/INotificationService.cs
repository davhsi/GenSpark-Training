using EnhancedSNS.Models;

namespace EnhancedSNS.Services
{
    internal interface INotificationService
    {
        bool SendNotification(User user, string message);
    }
}
