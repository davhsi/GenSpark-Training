namespace Busly.API.DTOs.Admin;

public class CreateTcRequest
{
    public string Version { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime? EffectiveAt { get; set; }
}
