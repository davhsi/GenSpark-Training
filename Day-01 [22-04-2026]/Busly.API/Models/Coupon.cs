using System.ComponentModel.DataAnnotations.Schema;

namespace Busly.API.Models;

[Table("coupon")]
public class Coupon
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("code")]
    public string Code { get; set; } = null!;

    [Column("discount_value")]
    public decimal? DiscountValue { get; set; }

    [Column("discount_type")]
    public string? DiscountType { get; set; }

    [Column("issued_to_customer")]
    public Guid? IssuedToCustomer { get; set; }

    [Column("cancellation_id")]
    public Guid? CancellationId { get; set; }

    [Column("is_used")]
    public bool IsUsed { get; set; }

    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    // Navigation properties
    public Customer? Customer { get; set; }
    public Cancellation? Cancellation { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
