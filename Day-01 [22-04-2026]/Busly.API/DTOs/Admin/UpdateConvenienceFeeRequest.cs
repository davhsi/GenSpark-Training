using System.ComponentModel.DataAnnotations;

namespace Busly.API.DTOs.Admin;

public class UpdateConvenienceFeeRequest
{
    [Required]
    public string FeeType { get; set; } = "flat"; // "flat" or "percent"

    [Required]
    [Range(0, double.MaxValue)]
    public decimal FeeValue { get; set; }
}
