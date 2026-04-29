namespace Busly.API.DTOs.Search;

public class SeatAvailabilityDto
{
    public Guid SeatId { get; set; }
    public int? SeatNumber { get; set; }
    public string? SeatType { get; set; }
    public string? Deck { get; set; }
    public string Status { get; set; } = "AVAILABLE"; // AVAILABLE, LOCKED, BOOKED
    public string? PassengerGender { get; set; }
    public DateTime? LockExpiresAt { get; set; } // When lock expires (null if not locked)
}
