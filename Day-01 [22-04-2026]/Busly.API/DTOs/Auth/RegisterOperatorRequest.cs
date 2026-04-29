using System.ComponentModel.DataAnnotations;

namespace Busly.API.DTOs.Auth;

public class RegisterOperatorRequest
{
    [Required]
    public string CompanyName { get; set; } = null!;

    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;

    public string? Phone { get; set; }

    [Required]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms and conditions")]
    public bool AcceptedTerms { get; set; }
}
