namespace LibraryManagement.Model.Models
{
    /// <summary>
    /// Represents a fine issued to a member for returning a book late or in damaged condition.
    /// Created automatically by proc_return_book.
    /// </summary>
    public class Fine
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int BorrowingId { get; set; }

        /// <summary>Total fine amount. Calculated by proc_return_book (₹10/day overdue + ₹500 if damaged).</summary>
        public decimal Amount { get; set; }

        /// <summary>UTC timestamp when the fine was issued.</summary>
        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp when the fine was fully paid. Null while unpaid.</summary>
        public DateTime? PaidDate { get; set; }

        public bool IsPaid { get; set; } = false;

        // Navigation properties
        public Member? Member { get; set; }
        public Borrowing? Borrowing { get; set; }
    }
}
