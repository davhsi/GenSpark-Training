namespace Busly.API.DTOs.Operator;

public class LayoutDto
{
    public Guid Id { get; set; }
    public string? LayoutName { get; set; }
    public int? TotalSeats { get; set; }
    public SeatConfigDto? SeatConfig { get; set; }
}
