namespace Busly.API.DTOs.Admin;

public class OperatingDayDto
{
    public int DayOfWeek { get; set; } // 1=Monday, 2=Tuesday, ..., 7=Sunday
    public bool IsActive { get; set; }
}

public class BusDto
{
    public Guid Id { get; set; }
    public string? BusNumber { get; set; }
    public string? BusName { get; set; }
    public string? OwnerName { get; set; }
    public string? OwnerPhone { get; set; }
    public string? OwnerEmail { get; set; }
    public string? Status { get; set; }
    public Guid? OperatorId { get; set; }
    public Guid? RouteId { get; set; }
    public decimal? BasePrice { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public string? ConductorName { get; set; }
    public string? ConductorPhone { get; set; }
    public string? SourceCity { get; set; }
    public string? DestinationCity { get; set; }
    public string? LayoutName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<OperatingDayDto>? OperatingDays { get; set; }
}
