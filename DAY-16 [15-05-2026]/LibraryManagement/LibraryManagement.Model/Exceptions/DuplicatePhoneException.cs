namespace LibraryManagement.Model.Exceptions
{
    /// <summary>
    /// Thrown when trying to register or update a member with a phone number that already exists.
    /// </summary>
    public class DuplicatePhoneException : LibraryException
    {
        public DuplicatePhoneException(string phone)
            : base($"An account with the phone number '{phone}' already exists.") { }
    }
}
