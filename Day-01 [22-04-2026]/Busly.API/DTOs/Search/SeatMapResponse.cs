namespace Busly.API.DTOs.Search;

public class SeatMapResponse
{
    public string? LayoutConfig { get; set; }  // raw seat_config JSONB string
    public List<SeatAvailabilityDto> SeatStatuses { get; set; } = new();
}
