using System.ComponentModel.DataAnnotations;

namespace Busly.API.DTOs.Operator;

public class UpdatePriceRequest
{
    [Required]
    public decimal BasePrice { get; set; }
}
