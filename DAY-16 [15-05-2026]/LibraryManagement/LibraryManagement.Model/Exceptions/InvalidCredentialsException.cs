namespace LibraryManagement.Model.Exceptions
{
    /// <summary>
    /// Thrown when login credentials (email/password) do not match any active member.
    /// </summary>
    public class InvalidCredentialsException : LibraryException
    {
        public InvalidCredentialsException()
            : base("Invalid email or password. Please try again.") { }
    }
}
