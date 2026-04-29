namespace Busly.API.DTOs.Operator;

public class OperatingDayDto
{
    public int DayOfWeek { get; set; } // 1=Monday, 2=Tuesday, ..., 7=Sunday
    public bool IsActive { get; set; }
}

public class UpdateOperatingDaysRequest
{
    public Guid BusId { get; set; }
    public List<OperatingDayDto> OperatingDays { get; set; } = new();
}
