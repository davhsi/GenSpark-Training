namespace SimpleNotificationSystem.Models
{
    internal class Notification
    {
        public required string Message { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.Now; // gets the current timestamp as default

        public Notification(string message, DateTime timeStamp)
        {
            Message = message;
            TimeStamp = timeStamp;
        }


    }
}