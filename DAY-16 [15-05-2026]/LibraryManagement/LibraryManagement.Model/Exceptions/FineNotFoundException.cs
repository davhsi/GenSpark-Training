namespace LibraryManagement.Model.Exceptions
{
    /// <summary>
    /// Thrown when a fine ID does not exist or is already fully paid.
    /// </summary>
    public class FineNotFoundException : LibraryException
    {
        public FineNotFoundException(int fineId)
            : base($"Fine with ID {fineId} was not found or is already paid.") { }
    }
}
