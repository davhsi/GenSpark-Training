namespace LibraryManagement.Model.Exceptions
{
    /// <summary>
    /// Thrown when a book ID does not exist in the catalog.
    /// </summary>
    public class BookNotFoundException : LibraryException
    {
        public BookNotFoundException(int bookId)
            : base($"Book with ID {bookId} was not found.") { }
    }
}
