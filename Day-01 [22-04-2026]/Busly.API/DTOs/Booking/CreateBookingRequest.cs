namespace Busly.API.DTOs.Booking;

public class CreateBookingRequest
{
    public Guid BusId { get; set; }
    public DateOnly JourneyDate { get; set; }
    public List<PassengerDto> Passengers { get; set; } = new();
    public string? CouponCode { get; set; }
}
