namespace Busly.API.DTOs.Admin;

public class TcVersionDto
{
    public Guid Id { get; set; }
    public string Version { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime? PublishedAt { get; set; }
    public DateTime? EffectiveAt { get; set; }
    public bool IsActive { get; set; }
}
