using System.ComponentModel.DataAnnotations;

namespace Busly.API.DTOs.Search;

public class BulkSeatLockRequest
{
    [Required]
    public List<Guid> SeatIds { get; set; } = new();

    [Required]
    public Guid BusId { get; set; }

    [Required]
    public DateOnly JourneyDate { get; set; }
}
