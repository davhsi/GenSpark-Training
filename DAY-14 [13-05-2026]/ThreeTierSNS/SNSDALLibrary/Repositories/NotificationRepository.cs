using SNSModelLibrary;
using Microsoft.EntityFrameworkCore;

namespace SNSDALLibrary
{
    public class NotificationRepository
    {
        private readonly UserContext _context;

        public NotificationRepository()
        {
            _context = new UserContext();
        }

        public void Create(Notification notification)
        {
            try
            {
                _context.Notifications.Add(notification);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating notification: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        public List<Notification> GetAllWithUserDetails()
        {
            try
            {
                // Using .Include to load the User details
                var notifications = _context.Notifications
                    .Include(n => n.User)
                    .OrderByDescending(n => n.TimeStamp)
                    .ToList();

                foreach (var n in notifications)
                {
                    if (n.User != null)
                    {
                        n.RecipientName = $"{n.User.Name.FirstName} {n.User.Name.LastName} <{n.User.Email}>";
                    }
                }

                return notifications;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching notifications with user details: " + ex.Message);
            }
        }

        public List<Notification> GetAll()
        {
            return GetAllWithUserDetails();
        }
    }
}
