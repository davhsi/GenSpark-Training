namespace Busly.API.DTOs.Auth;

public class UserProfileResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string? Name { get; set; }
    public bool TcAccepted { get; set; }
    public string? TcVersion { get; set; }
}
