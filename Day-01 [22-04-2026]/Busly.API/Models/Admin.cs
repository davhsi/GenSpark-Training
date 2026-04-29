using System.ComponentModel.DataAnnotations.Schema;

namespace Busly.API.Models;

[Table("admin")]
public class Admin
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("username")]
    public string Username { get; set; } = null!;

    [Column("password_hash")]
    public string PasswordHash { get; set; } = null!;

    [Column("email")]
    public string Email { get; set; } = null!;

    // Navigation properties
    public ICollection<BusOperator> ApprovedOperators { get; set; } = new List<BusOperator>();
    public ICollection<Route> CreatedRoutes { get; set; } = new List<Route>();
    public ICollection<Bus> ApprovedBuses { get; set; } = new List<Bus>();
    public ICollection<TcVersion> PublishedTcVersions { get; set; } = new List<TcVersion>();
}
