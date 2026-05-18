namespace LibraryManagement.Model.Exceptions
{
    /// <summary>
    /// Thrown when a provided email address does not match a valid email format.
    /// </summary>
    public class InvalidEmailException : LibraryException
    {
        public InvalidEmailException(string email)
            : base($"'{email}' is not a valid email address.") { }
    }
}
