using System.ComponentModel.DataAnnotations.Schema;

namespace Busly.API.Models;

[Table("customer")]
public class Customer
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("username")]
    public string Username { get; set; } = null!;

    [Column("password_hash")]
    public string PasswordHash { get; set; } = null!;

    [Column("email")]
    public string Email { get; set; } = null!;

    [Column("name")]
    public string? Name { get; set; }

    [Column("age")]
    public int? Age { get; set; }

    [Column("gender")]
    public string? Gender { get; set; }

    [Column("tc_accepted")]
    public bool TcAccepted { get; set; }

    [Column("tc_version")]
    public string? TcVersion { get; set; }

    [Column("tc_accepted_at")]
    public DateTime? TcAcceptedAt { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    // Navigation properties
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<SeatLock> SeatLocks { get; set; } = new List<SeatLock>();
    public ICollection<Coupon> IssuedCoupons { get; set; } = new List<Coupon>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
