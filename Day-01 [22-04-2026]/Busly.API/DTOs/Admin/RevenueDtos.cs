namespace Busly.API.DTOs.Admin;

public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalConvenienceFee { get; set; }
}

public class OperatorRevenueDto
{
    public string? OperatorName { get; set; }
    public int BookingCount { get; set; }
    public decimal TotalBaseFare { get; set; }
    public decimal TotalConvenienceFee { get; set; }
}
