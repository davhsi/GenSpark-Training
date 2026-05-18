namespace LibraryManagement.Model.Models
{
    /// <summary>
    /// Represents a book title in the library catalog.
    /// Each book can have multiple physical copies (BookCopy).
    /// </summary>
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";

        /// <summary>International Standard Book Number. Optional; must be unique if provided.</summary>
        public string? ISBN { get; set; }

        public int BookCategoryId { get; set; }
        public int PublicationYear { get; set; }

        // Navigation properties
        public BookCategory? BookCategory { get; set; }
        public ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();
    }
}
