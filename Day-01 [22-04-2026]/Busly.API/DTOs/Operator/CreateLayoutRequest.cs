using System.ComponentModel.DataAnnotations;

namespace Busly.API.DTOs.Operator;

public class CreateLayoutRequest
{
    [Required]
    public string LayoutName { get; set; } = null!;

    [Required]
    public SeatConfigDto SeatConfig { get; set; } = null!;
}
