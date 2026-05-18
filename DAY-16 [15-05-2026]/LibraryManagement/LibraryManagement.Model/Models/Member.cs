namespace LibraryManagement.Model.Models
{
    /// <summary>
    /// Represents a library member who can borrow books.
    /// </summary>
    public class Member
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string? Email { get; set; }

        /// <summary>SHA-256 hex-encoded hash. Never stored as plaintext.</summary>
        public string Password { get; set; } = "";

        public int MembershipTypeId { get; set; }

        /// <summary>False = account deactivated; member cannot borrow until reactivated.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>True = user is an administrator.</summary>
        public bool IsAdmin { get; set; } = false;

        /// <summary>Stored in UTC to avoid timezone conflicts with PostgreSQL timestamptz.</summary>
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public MembershipType? MembershipType { get; set; }
        public ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();
        public ICollection<Fine> Fines { get; set; } = new List<Fine>();
    }
}