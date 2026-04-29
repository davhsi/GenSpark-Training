namespace Busly.API.DTOs.Cancellation;

public class CancellationDto
{
    public Guid CancellationId { get; set; }
    public Guid BookingId { get; set; }
    public string? CancelledBy { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? RefundStatus { get; set; }
    public DateTime? CancelledAt { get; set; }
}
