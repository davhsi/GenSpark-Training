using System.ComponentModel.DataAnnotations.Schema;

namespace Busly.API.Models;

[Table("bus_operating_days")]
public class BusOperatingDay
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("bus_id")]
    public Guid BusId { get; set; }

    [Column("day_of_week")]
    public int DayOfWeek { get; set; } // 1=Monday, 2=Tuesday, ..., 7=Sunday

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public Bus Bus { get; set; } = null!;
}
