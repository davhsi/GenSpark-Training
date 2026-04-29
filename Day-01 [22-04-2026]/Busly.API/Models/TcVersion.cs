using System.ComponentModel.DataAnnotations.Schema;

namespace Busly.API.Models;

[Table("tc_version")]
public class TcVersion
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("version")]
    public string Version { get; set; } = null!;

    [Column("content")]
    public string Content { get; set; } = null!;

    [Column("published_at")]
    public DateTime? PublishedAt { get; set; }

    [Column("effective_at")]
    public DateTime? EffectiveAt { get; set; }

    [Column("published_by_admin")]
    public Guid? PublishedByAdmin { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    // Navigation properties
    public Admin? Admin { get; set; }
}
