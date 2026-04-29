using System.ComponentModel.DataAnnotations;

namespace Busly.API.DTOs.Operator;

public class RegisterBusRequest
{
    [Required]
    public Guid RouteId { get; set; }

    [Required]
    public Guid LayoutId { get; set; }

    [Required]
    public string BusNumber { get; set; } = null!;

    [Required]
    public string BusName { get; set; } = null!;

    [Required]
    public string OwnerName { get; set; } = string.Empty;
    public string? OwnerPhone { get; set; }
    public string? OwnerEmail { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public string? ConductorName { get; set; }
    public string? ConductorPhone { get; set; }
    [Required]
    public decimal BasePrice { get; set; }
    public List<OperatingDayDto>? OperatingDays { get; set; }
}
