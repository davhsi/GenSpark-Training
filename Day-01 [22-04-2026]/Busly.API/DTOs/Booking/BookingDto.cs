namespace Busly.API.DTOs.Booking;

public class BookingDto
{
    public Guid Id { get; set; }
    public string Pnr { get; set; } = null!;  // first 8 chars of Id
    public Guid BusId { get; set; }
    public DateOnly JourneyDate { get; set; }
    public TimeOnly? DepartureTime { get; set; }  // Actual bus departure time
    public decimal? BaseFare { get; set; }
    public decimal? ConvenienceFee { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Status { get; set; }
    public DateTime? BookedAt { get; set; }
    public List<BookedSeatDto> Seats { get; set; } = new();
}
