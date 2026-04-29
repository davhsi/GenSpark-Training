using System.ComponentModel.DataAnnotations.Schema;

namespace Busly.API.Models;

[Table("payment")]
public class Payment
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("booking_id")]
    public Guid? BookingId { get; set; }

    [Column("amount")]
    public decimal? Amount { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("transaction_ref")]
    public string? TransactionRef { get; set; }

    [Column("paid_at")]
    public DateTime? PaidAt { get; set; }

    // Navigation properties
    public Booking? Booking { get; set; }
}
