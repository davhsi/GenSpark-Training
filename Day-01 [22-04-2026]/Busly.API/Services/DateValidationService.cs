namespace Busly.API.Services;

public interface IDateValidationService
{
    void ValidateJourneyDate(DateOnly journeyDate);
    void ValidateBookingDate(DateOnly journeyDate);
}

public class DateValidationService : IDateValidationService
{
    private readonly ILogger<DateValidationService> _logger;
    
    // Maximum booking window: 90 days in advance
    private const int MAX_BOOKING_DAYS_AHEAD = 90;
    
    // Minimum booking window: Cannot book for today if bus has already departed
    private const int MIN_BOOKING_HOURS_AHEAD = 2;

    public DateValidationService(ILogger<DateValidationService> logger)
    {
        _logger = logger;
    }

    public void ValidateJourneyDate(DateOnly journeyDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var maxDate = today.AddDays(MAX_BOOKING_DAYS_AHEAD);

        // Check if date is in the past
        if (journeyDate < today)
        {
            _logger.LogWarning("Attempted to lock seat for past date: {JourneyDate}, Today: {Today}", journeyDate, today);
            throw new InvalidOperationException($"Cannot book seats for past dates. Journey date must be {today} or later.");
        }

        // Check if date is too far in advance
        if (journeyDate > maxDate)
        {
            _logger.LogWarning("Attempted to lock seat too far in advance: {JourneyDate}, MaxDate: {MaxDate}", journeyDate, maxDate);
            throw new InvalidOperationException($"Cannot book seats more than {MAX_BOOKING_DAYS_AHEAD} days in advance. Maximum booking date is {maxDate}.");
        }

        // Additional check: Don't allow booking for today if it's too late in the day
        var now = DateTime.UtcNow;
        var cutoffTime = new TimeOnly(20, 0); // 8:00 PM cutoff for same-day bookings
        
        if (journeyDate == today && now.TimeOfDay > cutoffTime.ToTimeSpan())
        {
            _logger.LogWarning("Attempted same-day booking after cutoff: {Time}, Cutoff: {Cutoff}", now.TimeOfDay, cutoffTime);
            throw new InvalidOperationException($"Same-day bookings are not allowed after {cutoffTime:hh\\:mm}. Please book for tomorrow or later.");
        }
    }

    public void ValidateBookingDate(DateOnly journeyDate)
    {
        // For actual booking, be more strict - no same-day bookings
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tomorrow = today.AddDays(1);
        var maxDate = today.AddDays(MAX_BOOKING_DAYS_AHEAD);

        // Check if date is in the past or today
        if (journeyDate <= today)
        {
            _logger.LogWarning("Attempted to create booking for past/today date: {JourneyDate}, Today: {Today}", journeyDate, today);
            throw new InvalidOperationException($"Cannot create bookings for past dates or same-day travel. Journey date must be {tomorrow} or later.");
        }

        // Check if date is too far in advance
        if (journeyDate > maxDate)
        {
            _logger.LogWarning("Attempted to create booking too far in advance: {JourneyDate}, MaxDate: {MaxDate}", journeyDate, maxDate);
            throw new InvalidOperationException($"Cannot book seats more than {MAX_BOOKING_DAYS_AHEAD} days in advance. Maximum booking date is {maxDate}.");
        }
    }
}
