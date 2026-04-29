using System.ComponentModel.DataAnnotations;

namespace Busly.API.DTOs.Operator;

public class AddBusStopRequest
{
    /// <summary>"BOARDING" or "DROPPING"</summary>
    [Required]
    public string Type { get; set; } = null!;

    [Required]
    public string City { get; set; } = null!;

    [Required]
    public string Address { get; set; } = null!;

    /// <summary>Time in "HH:mm" format.</summary>
    [Required]
    public string ScheduledTime { get; set; } = null!;
}
