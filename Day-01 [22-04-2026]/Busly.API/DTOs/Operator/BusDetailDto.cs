namespace Busly.API.DTOs.Operator;

public class BusDetailDto
{
    public Guid Id { get; set; }
    public string? BusNumber { get; set; }
    public string? BusName { get; set; }
    public string? OwnerName { get; set; }
    public string? Status { get; set; }
    public decimal? BasePrice { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public string? ConductorName { get; set; }
    public string? ConductorPhone { get; set; }
    public string? OwnerPhone { get; set; }
    public string? OwnerEmail { get; set; }
    public List<OperatingDayDto>? OperatingDays { get; set; }
    public Guid? RouteId { get; set; }
    public string? SourceCity { get; set; }
    public string? DestinationCity { get; set; }
    public Guid? LayoutId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<BusStopDto> Stops { get; set; } = new();
}
