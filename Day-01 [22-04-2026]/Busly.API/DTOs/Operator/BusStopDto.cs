namespace Busly.API.DTOs.Operator;

public class BusStopDto
{
    public Guid Id { get; set; }
    public string? Type { get; set; } // BOARDING or DROPPING
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? ScheduledTime { get; set; } // HH:mm:ss format or similar
}
