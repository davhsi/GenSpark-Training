namespace LibraryManagement.Model.Exceptions
{
    /// <summary>
    /// Thrown when a provided phone number does not match the expected format (10 digits).
    /// </summary>
    public class InvalidPhoneException : LibraryException
    {
        public InvalidPhoneException(string phone)
            : base($"'{phone}' is not a valid phone number. Expected 10 digits.") { }
    }
}
