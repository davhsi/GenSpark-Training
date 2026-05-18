namespace LibraryManagement.Model.Exceptions
{
    /// <summary>
    /// Thrown when a member ID does not exist in the database.
    /// </summary>
    public class MemberNotFoundException : LibraryException
    {
        public MemberNotFoundException(int memberId)
            : base($"Member with ID {memberId} was not found.") { }
    }
}
