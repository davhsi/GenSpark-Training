namespace SNSModelLibrary
{
    public partial class Notification : IComparable<Notification>
    {
        public int CompareTo(Notification? other)
        {
            if (other == null) return 1;
            
            // Compare by timestamp in descending order (newest first)
            return other.TimeStamp.CompareTo(this.TimeStamp);
        }
    }
}
