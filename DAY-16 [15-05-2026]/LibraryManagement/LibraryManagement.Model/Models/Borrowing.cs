namespace LibraryManagement.Model.Models
{
    /// <summary>
    /// Represents a single book borrowing transaction.
    /// Created when a member borrows a copy; updated when returned.
    /// </summary>
    public class Borrowing
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int BookCopyId { get; set; }

        /// <summary>Stored in UTC (PostgreSQL timestamptz). Set automatically at borrow time.</summary>
        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;

        /// <summary>Calculated as BorrowDate + MembershipType.MaxBorrowDays by the stored procedure.</summary>
        public DateTime DueDate { get; set; }

        /// <summary>Null while still borrowed. Set to UTC timestamp when returned.</summary>
        public DateTime? ReturnDate { get; set; }

        /// <summary>Valid values: Borrowed | Returned.</summary>
        public string Status { get; set; } = "Borrowed";

        // Navigation properties
        public Member? Member { get; set; }
        public BookCopy? BookCopy { get; set; }

        /// <summary>Populated only if the book was returned late and a fine was issued.</summary>
        public Fine Fine { get; set; } = null!;
    }
}