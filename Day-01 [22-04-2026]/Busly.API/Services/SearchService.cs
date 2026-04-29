using Busly.API.Data;
using Busly.API.DTOs.Search;
using Busly.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Busly.API.Services;

public class SearchService : ISearchService
{
    private readonly AppDbContext _db;
    private readonly ISeatRepository _seatRepo;
    private readonly IRouteRepository _routeRepo;

    public SearchService(AppDbContext db, ISeatRepository seatRepo, IRouteRepository routeRepo)
    {
        _db = db;
        _seatRepo = seatRepo;
        _routeRepo = routeRepo;
    }

    public async Task<List<BusSearchResultDto>> SearchBusesAsync(string from, string to, DateOnly date)
    {
        // Map DateOnly.DayOfWeek (0=Sunday…6=Saturday) to our 1=Monday…7=Sunday convention
        var dotNetDow = (int)date.DayOfWeek;          // 0=Sun, 1=Mon … 6=Sat
        var buslyDow  = dotNetDow == 0 ? 7 : dotNetDow; // convert: Sun→7, Mon→1 … Sat→6

        var buses = await _db.Buses
            .Include(b => b.Operator)
            .Include(b => b.Route)
            .Include(b => b.Layout)
            .Include(b => b.OperatingDays)
            .Where(b =>
                b.Status == "ACTIVE" &&
                b.Route != null &&
                b.Route.SourceCity != null &&
                b.Route.DestinationCity != null &&
                b.Route.SourceCity.ToLower() == from.ToLower() &&
                b.Route.DestinationCity.ToLower() == to.ToLower() &&
                // Only return buses that operate on the requested day
                b.OperatingDays.Any(od => od.DayOfWeek == buslyDow && od.IsActive))
            .ToListAsync();

        var results = new List<BusSearchResultDto>();

        foreach (var bus in buses)
        {
            var confirmedBookings = await _db.BookedSeats
                .CountAsync(bs =>
                    bs.BusId == bus.Id &&
                    bs.JourneyDate == date);

            var activeLocksCount = await _db.SeatLocks
                .CountAsync(sl =>
                    sl.BusId == bus.Id &&
                    sl.JourneyDate == date &&
                    sl.IsActive &&
                    sl.ExpiresAt > DateTime.UtcNow);

            var totalSeats = bus.Layout?.TotalSeats ?? 0;
            var availableSeats = totalSeats - confirmedBookings - activeLocksCount;

            results.Add(new BusSearchResultDto
            {
                BusId = bus.Id,
                BusName = bus.BusName,
                BusNumber = bus.BusNumber,
                OperatorName = bus.Operator?.CompanyName,
                SourceCity = bus.Route?.SourceCity,
                DestinationCity = bus.Route?.DestinationCity,
                BasePrice = bus.BasePrice,
                AvailableSeats = availableSeats < 0 ? 0 : availableSeats
            });
        }

        return results;
    }

    public async Task<SeatMapResponse> GetSeatMapAsync(Guid busId, DateOnly date)
    {
        var bus = await _db.Buses
            .Include(b => b.Layout)
            .FirstOrDefaultAsync(b => b.Id == busId)
            ?? throw new KeyNotFoundException($"Bus {busId} not found.");

        var seatStatuses = await _seatRepo.GetSeatAvailabilityAsync(busId, date);

        return new SeatMapResponse
        {
            LayoutConfig = bus.Layout?.SeatConfig,
            SeatStatuses = seatStatuses
        };
    }

    public Task<List<string>> GetCitySuggestionsAsync(string query)
        => _routeRepo.GetCitySuggestionsAsync(query);

    public async Task<BusSearchResultDto> GetBusDetailsAsync(Guid busId)
    {
        var bus = await _db.Buses
            .Include(b => b.Operator)
            .Include(b => b.Route)
            .Include(b => b.Layout)
            .FirstOrDefaultAsync(b => b.Id == busId)
            ?? throw new KeyNotFoundException($"Bus {busId} not found.");

        // Available seats are not date-specific here (no date param), so return total seats.
        // The seat map endpoint should be used for date-specific availability.
        return new BusSearchResultDto
        {
            BusId = bus.Id,
            BusName = bus.BusName,
            BusNumber = bus.BusNumber,
            OperatorName = bus.Operator?.CompanyName,
            SourceCity = bus.Route?.SourceCity,
            DestinationCity = bus.Route?.DestinationCity,
            BasePrice = bus.BasePrice,
            AvailableSeats = bus.Layout?.TotalSeats ?? 0  // Total capacity; use seat map for date-specific availability
        };
    }
}
