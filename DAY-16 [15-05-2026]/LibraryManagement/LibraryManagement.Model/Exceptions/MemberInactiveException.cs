namespace LibraryManagement.Model.Exceptions
{
    /// <summary>
    /// Thrown when an operation is attempted on a deactivated member account.
    /// </summary>
    public class MemberInactiveException : LibraryException
    {
        public MemberInactiveException(int memberId)
            : base($"Member account (ID: {memberId}) is inactive. Please contact the library.") { }
    }
}
