namespace Busly.API.DTOs.Admin;

public class RouteDto
{
    public Guid Id { get; set; }
    public string SourceCity { get; set; } = null!;
    public string DestinationCity { get; set; } = null!;
    public bool IsActive { get; set; }
}
