namespace SNSModelLibrary
{
    public partial class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; } = "";
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow; // gets the current timestamp as default
        public string Type { get; set; } = "General"; // SMS, Email, General
        public string RecipientName { get; set; } = "";
        public virtual User? User { get; set; }

        public Notification() { }

        public Notification(int userId, string message, string type = "General")
        {
            UserId = userId;
            Message = message;
            Type = type;
            TimeStamp = DateTime.UtcNow;
        }

        public override string ToString()
        {
            string recipientInfo = string.IsNullOrWhiteSpace(RecipientName) ? "" : $" (To: {RecipientName})";
            return $"[{TimeStamp:yyyy-MM-dd HH:mm:ss}] {Type}: {Message}{recipientInfo}";
        }
    }
}