namespace Busly.API.Helpers;

public static class RefundCalculatorService
{
    /// <summary>
    /// Calculates the refund amount for a customer-triggered cancellation.
    /// Rules:
    ///   > 24 hours before departure → 85% of baseFare
    ///   12–24 hours before departure → 50% of baseFare
    ///   &lt; 12 hours before departure → 0%
    /// Convenience fee is NEVER refunded on customer cancellations.
    /// </summary>
    public static decimal Calculate(TimeOnly departureTime, DateOnly journeyDate, decimal baseFare)
    {
        var departureDateTime = journeyDate.ToDateTime(departureTime);
        var hoursUntilDeparture = (departureDateTime - DateTime.UtcNow).TotalHours;

        return hoursUntilDeparture switch
        {
            > 24 => Math.Round(baseFare * 0.85m, 2),
            >= 12 => Math.Round(baseFare * 0.50m, 2),
            _ => 0m
        };
    }
}
