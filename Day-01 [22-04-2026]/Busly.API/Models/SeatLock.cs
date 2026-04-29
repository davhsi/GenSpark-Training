using System.ComponentModel.DataAnnotations.Schema;

namespace Busly.API.Models;

[Table("seat_lock")]
public class SeatLock
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("seat_id")]
    public Guid? SeatId { get; set; }

    [Column("customer_id")]
    public Guid? CustomerId { get; set; }

    [Column("bus_id")]
    public Guid? BusId { get; set; }

    [Column("journey_date")]
    public DateOnly JourneyDate { get; set; }

    [Column("locked_at")]
    public DateTime? LockedAt { get; set; }

    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    // Navigation properties
    public Seat? Seat { get; set; }
    public Customer? Customer { get; set; }
    public Bus? Bus { get; set; }
}
