namespace LibraryManagement.Model.Models
{
    /// <summary>
    /// Represents a single physical copy of a book.
    /// A book can have many copies; each copy is independently tracked.
    /// </summary>
    public class BookCopy
    {
        public int Id { get; set; }
        public int BookId { get; set; }

        /// <summary>Unique barcode/accession number assigned to this physical copy by the library.</summary>
        public string AccessionNumber { get; set; } = "";

        /// <summary>Physical condition: Good, Damaged, etc.</summary>
        public string Condition { get; set; } = "Good";

        /// <summary>Valid values: Available | Borrowed | Damaged | Unavailable.</summary>
        public string Status { get; set; } = "Available";

        // Navigation properties
        public Book? Book { get; set; }
        public ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();
    }
}
