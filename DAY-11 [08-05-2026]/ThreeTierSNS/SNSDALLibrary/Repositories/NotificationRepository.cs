using SNSModelLibrary;

namespace SNSDALLibrary
{
    public class NotificationRepository
    {
        
        private readonly List<Notification> _notifications = new();
    
        public void Create(Notification notification)
        {
            _notifications.Add(notification);
        }

        public List<Notification> GetAll()
        {
            // using LINQ to order notifications by TimeStamp in descending order.
            return _notifications.OrderByDescending(n => n.TimeStamp).ToList();
        }
    }
}
