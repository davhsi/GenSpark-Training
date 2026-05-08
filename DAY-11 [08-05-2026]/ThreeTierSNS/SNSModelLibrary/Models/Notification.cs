namespace SNSModelLibrary
{
    public partial class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; } = "";
        public DateTime TimeStamp { get; set; } = DateTime.Now; // gets the current timestamp as default
        public string Type { get; set; } = "General"; // SMS, Email, General

        public Notification() { }

        public Notification(string message, string type = "General")
        {
            Message = message;
            Type = type;
            TimeStamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{TimeStamp:yyyy-MM-dd HH:mm:ss}] {Type}: {Message}";
        }
    }
}