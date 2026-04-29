using System.ComponentModel.DataAnnotations.Schema;

namespace Busly.API.Models;

[Table("bus_layout")]
public class BusLayout
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("operator_id")]
    public Guid OperatorId { get; set; }

    [Column("layout_name")]
    public string? LayoutName { get; set; }

    [Column("total_seats")]
    public int? TotalSeats { get; set; }

    /// <summary>
    /// Raw JSONB column. Deserialized to SeatConfigDto in the service layer.
    /// </summary>
    [Column("seat_config")]
    public string? SeatConfig { get; set; }

    // Navigation properties
    public ICollection<Bus> Buses { get; set; } = new List<Bus>();
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
