namespace LibraryManagement.Model.Exceptions
{
    /// <summary>
    /// Thrown when attempting to deactivate a member who has outstanding unpaid fines.
    /// </summary>
    public class MemberHasUnpaidFinesException : LibraryException
    {
        public MemberHasUnpaidFinesException(decimal amount)
            : base($"Cannot deactivate member. They have ₹{amount} in unpaid fines. All fines must be settled first.") { }
    }
}
