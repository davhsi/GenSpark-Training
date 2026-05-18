namespace LibraryManagement.Model.Exceptions
{
    /// <summary>
    /// Base exception for all Library Management domain errors.
    /// Catch this to handle any library-specific exception generically.
    /// </summary>
    public class LibraryException : Exception
    {
        public LibraryException(string message) : base(message) { }
        public LibraryException(string message, Exception inner) : base(message, inner) { }
    }
}
