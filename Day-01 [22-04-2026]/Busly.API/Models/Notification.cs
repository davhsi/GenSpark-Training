using System.ComponentModel.DataAnnotations.Schema;

namespace Busly.API.Models;

[Table("notification")]
public class Notification
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("customer_id")]
    public Guid? CustomerId { get; set; }

    [Column("operator_id")]
    public Guid? OperatorId { get; set; }

    [Column("type")]
    public string? Type { get; set; }

    [Column("message")]
    public string? Message { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("sent_at")]
    public DateTime? SentAt { get; set; }

    // Navigation properties
    public Customer? Customer { get; set; }
    public BusOperator? Operator { get; set; }
}
