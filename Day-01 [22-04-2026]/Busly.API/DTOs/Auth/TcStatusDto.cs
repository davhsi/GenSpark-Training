namespace Busly.API.DTOs.Auth;

public class TcStatusDto
{
    public bool HasAcceptedTc { get; set; }
    public string? LastAcceptedVersion { get; set; }
    public DateTime? LastAcceptedAt { get; set; }
    public string? CurrentActiveVersion { get; set; }
    public bool NeedsToAcceptCurrent { get; set; }
}
