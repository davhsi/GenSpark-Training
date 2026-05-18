namespace LibraryManagement.Model.Exceptions
{
    /// <summary>
    /// Thrown when attempting to deactivate a member who still has unreturned books.
    /// </summary>
    public class MemberHasActiveBorrowingsException : LibraryException
    {
        public MemberHasActiveBorrowingsException(int count)
            : base($"Cannot deactivate member. They have {count} unreturned book(s). All books must be returned first.") { }
    }
}
