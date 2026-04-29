namespace Busly.API.DTOs.Search;

public class SeatLockDto
{
    public Guid LockId { get; set; }
    public Guid SeatId { get; set; }
    public Guid BusId { get; set; }
    public DateOnly JourneyDate { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
