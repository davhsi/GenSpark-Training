using System.ComponentModel.DataAnnotations.Schema;

namespace Busly.API.Models;

[Table("bus")]
public class Bus
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("operator_id")]
    public Guid? OperatorId { get; set; }

    [Column("route_id")]
    public Guid? RouteId { get; set; }

    [Column("bus_number")]
    public string? BusNumber { get; set; }

    [Column("bus_name")]
    public string? BusName { get; set; }

    [Column("owner_name")]
    public string? OwnerName { get; set; }

    [Column("owner_phone")]
    public string? OwnerPhone { get; set; }

    [Column("owner_email")]
    public string? OwnerEmail { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("base_price")]
    public decimal? BasePrice { get; set; }

    [Column("layout_id")]
    public Guid? LayoutId { get; set; }

    [Column("driver_name")]
    public string? DriverName { get; set; }

    [Column("driver_phone")]
    public string? DriverPhone { get; set; }

    [Column("conductor_name")]
    public string? ConductorName { get; set; }

    [Column("conductor_phone")]
    public string? ConductorPhone { get; set; }

    [Column("approved_by_admin")]
    public Guid? ApprovedByAdmin { get; set; }

    [Column("approved_at")]
    public DateTime? ApprovedAt { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    // Navigation properties
    public BusOperator? Operator { get; set; }
    public Route? Route { get; set; }
    public BusLayout? Layout { get; set; }
    public Admin? ApprovingAdmin { get; set; }
    public ICollection<BusStop> BusStops { get; set; } = new List<BusStop>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<BookedSeat> BookedSeats { get; set; } = new List<BookedSeat>();
    public ICollection<SeatLock> SeatLocks { get; set; } = new List<SeatLock>();
    public ICollection<BusOperatingDay> OperatingDays { get; set; } = new List<BusOperatingDay>();
}
