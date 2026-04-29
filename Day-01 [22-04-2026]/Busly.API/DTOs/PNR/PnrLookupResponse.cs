namespace Busly.API.DTOs.PNR;

public class PnrLookupResponse
{
    public string Pnr { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public DateOnly JourneyDate { get; set; }
    public string FromCity { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;
    public string BusNumber { get; set; } = string.Empty;
    public TimeOnly DepartureTime { get; set; }
    public TimeOnly ArrivalTime { get; set; }
    public List<string> SeatNumbers { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public DateTime BookedAt { get; set; }
    public bool CanCancel { get; set; }
    public string? CancellationReason { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? RefundStatus { get; set; }
}
