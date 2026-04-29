using System.ComponentModel.DataAnnotations.Schema;

namespace Busly.API.Models;

[Table("cancellation")]
public class Cancellation
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("booking_id")]
    public Guid? BookingId { get; set; }

    [Column("cancelled_by")]
    public string? CancelledBy { get; set; }

    [Column("cancelled_at")]
    public DateTime? CancelledAt { get; set; }

    [Column("refund_amount")]
    public decimal? RefundAmount { get; set; }

    [Column("refund_status")]
    public string? RefundStatus { get; set; }

    // Navigation properties
    public Booking? Booking { get; set; }
    public Coupon? Coupon { get; set; }
}
