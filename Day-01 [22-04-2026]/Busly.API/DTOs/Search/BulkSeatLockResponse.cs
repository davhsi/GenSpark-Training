namespace Busly.API.DTOs.Search;

public class BulkSeatLockResponse
{
    public List<SeatLockDto> SuccessfulLocks { get; set; } = new();
    public List<Guid> FailedSeatIds { get; set; } = new();
    public bool AllSuccessful => FailedSeatIds.Count == 0;
}
