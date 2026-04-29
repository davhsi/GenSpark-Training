using System.ComponentModel.DataAnnotations.Schema;

namespace Busly.API.Models;

[Table("route")]
public class Route
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("source_city")]
    public string SourceCity { get; set; } = null!;

    [Column("destination_city")]
    public string DestinationCity { get; set; } = null!;

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("created_by_admin")]
    public Guid? CreatedByAdmin { get; set; }

    // Navigation properties
    public Admin? Admin { get; set; }
    public ICollection<Bus> Buses { get; set; } = new List<Bus>();
}
