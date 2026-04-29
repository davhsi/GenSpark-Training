namespace Busly.API.DTOs.Search;

public class BusSearchResultDto
{
    public Guid BusId { get; set; }
    public string? BusName { get; set; }
    public string? BusNumber { get; set; }
    public string? OperatorName { get; set; }
    public string? SourceCity { get; set; }
    public string? DestinationCity { get; set; }
    public decimal? BasePrice { get; set; }
    public int AvailableSeats { get; set; }
}
