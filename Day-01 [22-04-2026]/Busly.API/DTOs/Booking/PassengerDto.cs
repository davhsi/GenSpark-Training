namespace Busly.API.DTOs.Booking;

public class PassengerDto
{
    public Guid SeatId { get; set; }
    public string Name { get; set; } = null!;
    public int Age { get; set; }
    public string Gender { get; set; } = null!;
}
