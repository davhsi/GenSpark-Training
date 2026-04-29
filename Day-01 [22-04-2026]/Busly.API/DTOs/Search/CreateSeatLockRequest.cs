namespace Busly.API.DTOs.Search;

public class CreateSeatLockRequest
{
    public Guid SeatId { get; set; }
    public Guid BusId { get; set; }
    public DateOnly JourneyDate { get; set; }
}
