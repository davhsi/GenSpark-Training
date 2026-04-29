using System.ComponentModel.DataAnnotations.Schema;

namespace Busly.API.Models;

[Table("bus_operator")]
public class BusOperator
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("company_name")]
    public string CompanyName { get; set; } = null!;

    [Column("email")]
    public string Email { get; set; } = null!;

    [Column("password_hash")]
    public string PasswordHash { get; set; } = null!;

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("tc_accepted")]
    public bool TcAccepted { get; set; }

    [Column("tc_version")]
    public string? TcVersion { get; set; }

    [Column("tc_accepted_at")]
    public DateTime? TcAcceptedAt { get; set; }

    [Column("approved_by_admin")]
    public Guid? ApprovedByAdmin { get; set; }

    [Column("approved_at")]
    public DateTime? ApprovedAt { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    // Navigation properties
    public Admin? ApprovingAdmin { get; set; }
    public ICollection<Bus> Buses { get; set; } = new List<Bus>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
