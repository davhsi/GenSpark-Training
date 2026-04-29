using System.ComponentModel.DataAnnotations.Schema;

namespace Busly.API.Models;

[Table("booked_seat")]
public class BookedSeat
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("booking_id")]
    public Guid? BookingId { get; set; }

    [Column("seat_id")]
    public Guid? SeatId { get; set; }

    [Column("bus_id")]
    public Guid? BusId { get; set; }

    [Column("journey_date")]
    public DateOnly JourneyDate { get; set; }

    [Column("passenger_name")]
    public string? PassengerName { get; set; }

    [Column("passenger_age")]
    public int? PassengerAge { get; set; }

    [Column("passenger_gender")]
    public string? PassengerGender { get; set; }

    // Navigation properties
    public Booking? Booking { get; set; }
    public Seat? Seat { get; set; }
    public Bus? Bus { get; set; }
}
