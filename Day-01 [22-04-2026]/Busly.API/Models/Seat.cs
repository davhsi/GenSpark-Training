using System.ComponentModel.DataAnnotations.Schema;

namespace Busly.API.Models;

[Table("seat")]
public class Seat
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("layout_id")]
    public Guid? LayoutId { get; set; }

    [Column("seat_number")]
    public int? SeatNumber { get; set; }

    [Column("seat_type")]
    public string? SeatType { get; set; }

    [Column("deck")]
    public string? Deck { get; set; }

    [Column("row_num")]
    public int Row { get; set; }

    [Column("col_num")]
    public int Col { get; set; }

    // Navigation properties
    public BusLayout? Layout { get; set; }
    public ICollection<BookedSeat> BookedSeats { get; set; } = new List<BookedSeat>();
    public ICollection<SeatLock> SeatLocks { get; set; } = new List<SeatLock>();
}
