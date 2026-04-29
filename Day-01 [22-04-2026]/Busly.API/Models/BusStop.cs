using System.ComponentModel.DataAnnotations.Schema;

namespace Busly.API.Models;

[Table("bus_stop")]
public class BusStop
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("bus_id")]
    public Guid? BusId { get; set; }

    [Column("type")]
    public string? Type { get; set; }

    [Column("city")]
    public string? City { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("scheduled_time")]
    public TimeOnly? ScheduledTime { get; set; }

    // Navigation properties
    public Bus? Bus { get; set; }
}
