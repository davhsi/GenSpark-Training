using System.ComponentModel.DataAnnotations;

namespace Busly.API.DTOs.Admin;

public class CreateRouteRequest
{
    [Required]
    public string SourceCity { get; set; } = null!;

    [Required]
    public string DestinationCity { get; set; } = null!;
}
