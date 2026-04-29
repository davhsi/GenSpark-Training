using System.ComponentModel.DataAnnotations.Schema;

namespace Busly.API.Models;

[Table("audit_log")]
public class AuditLog
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("actor_id")]
    public Guid? ActorId { get; set; }

    [Column("actor_role")]
    public string? ActorRole { get; set; }

    [Column("action")]
    public string Action { get; set; } = null!;

    [Column("entity_type")]
    public string EntityType { get; set; } = null!;

    [Column("entity_id")]
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Raw JSONB column. Deserialized in the service layer as needed.
    /// </summary>
    [Column("metadata")]
    public string? Metadata { get; set; }

    [Column("performed_at")]
    public DateTime? PerformedAt { get; set; }
}
