using System.ComponentModel.DataAnnotations;

namespace Busly.API.DTOs.Auth;

public class RegisterCustomerRequest
{
    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;

    [Required]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms and conditions")]
    public bool AcceptedTerms { get; set; }
}
