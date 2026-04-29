using System.ComponentModel.DataAnnotations.Schema;

namespace Busly.API.Models;

[Table("booking")]
public class Booking
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("pnr")]
    public string? Pnr { get; set; }

    [Column("customer_id")]
    public Guid? CustomerId { get; set; }

    [Column("bus_id")]
    public Guid? BusId { get; set; }

    [Column("journey_date")]
    public DateOnly JourneyDate { get; set; }

    [Column("base_fare")]
    public decimal? BaseFare { get; set; }

    [Column("convenience_fee")]
    public decimal? ConvenienceFee { get; set; }

    [Column("total_amount")]
    public decimal? TotalAmount { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("booked_at")]
    public DateTime? BookedAt { get; set; }

    [Column("coupon_id")]
    public Guid? CouponId { get; set; }

    // Navigation properties
    public Customer? Customer { get; set; }
    public Bus? Bus { get; set; }
    public Coupon? Coupon { get; set; }
    public ICollection<BookedSeat> BookedSeats { get; set; } = new List<BookedSeat>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Cancellation> Cancellations { get; set; } = new List<Cancellation>();
}
