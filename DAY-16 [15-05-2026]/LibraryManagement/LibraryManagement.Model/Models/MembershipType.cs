namespace LibraryManagement.Model.Models
{
    /// <summary>
    /// Defines the borrowing privileges for a member tier.
    /// Seeded values: Basic (2/7 days), Student (3/10 days), Premium (5/15 days).
    /// </summary>
    public class MembershipType
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";

        /// <summary>Maximum number of books this member can have borrowed at the same time.</summary>
        public int MaxBorrowings { get; set; }

        /// <summary>Number of days before a borrowing becomes overdue. Used by fn_borrow_book to set DueDate.</summary>
        public int MaxBorrowDays { get; set; }

        // Navigation property
        public ICollection<Member>? Members { get; set; }
    }
}
