using EnhancedSNS.Interfaces;
using EnhancedSNS.Models;

namespace EnhancedSNS.Repositories
{
    internal class NotificationRepository : AbstractRepository<int, Notification>
    {
        private int _nextId = 1;

        public override Notification Create(Notification item)
        {
            item.Id = _nextId++;
            _items[item.Id] = item;
            return item;
        }

        public List<Notification>? GetByType(string type)
        {
            var notifications = _items.Values.Where(n => n.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
            return notifications.Count > 0 ? notifications : null;
        }

        public List<Notification>? GetByDateRange(DateTime startDate, DateTime endDate)
        {
            var notifications = _items.Values
                .Where(n => n.TimeStamp >= startDate && n.TimeStamp <= endDate)
                .OrderByDescending(n => n.TimeStamp)
                .ToList();
            return notifications.Count > 0 ? notifications : null;
        }
    }
}
