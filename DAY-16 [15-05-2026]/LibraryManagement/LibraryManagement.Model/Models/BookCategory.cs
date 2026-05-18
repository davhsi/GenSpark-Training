namespace LibraryManagement.Model.Models
{
    /// <summary>
    /// Represents a genre or category used to classify books (e.g., Fiction, Science, History).
    /// </summary>
    public class BookCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }

        // Navigation property
        public ICollection<Book>? Books { get; set; }
    }
}
