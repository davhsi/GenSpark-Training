namespace Busly.API.DTOs.Booking;

public class BookedSeatDto
{
    public Guid SeatId { get; set; }
    public string? PassengerName { get; set; }
    public int? PassengerAge { get; set; }
    public string? PassengerGender { get; set; }
}
