namespace LibraryManagement.Model.Exceptions
{
    /// <summary>
    /// Thrown when trying to register a member with an email that already exists.
    /// </summary>
    public class DuplicateEmailException : LibraryException
    {
        public DuplicateEmailException(string email)
            : base($"An account with the email '{email}' already exists.") { }
    }
}
