using Busly.API.Data;
using Busly.API.DTOs.Search;
using Microsoft.EntityFrameworkCore;
using BuslyRoute = Busly.API.Models.Route;

namespace Busly.API.Repositories;

public class RouteRepository : IRouteRepository
{
    private readonly AppDbContext _db;

    public RouteRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<BuslyRoute> CreateAsync(BuslyRoute route)
    {
        _db.Routes.Add(route);
        await _db.SaveChangesAsync();
        return route;
    }

    public Task<List<BuslyRoute>> GetAllActiveAsync() =>
        _db.Routes.Where(r => r.IsActive).ToListAsync();

    public Task<List<BuslyRoute>> GetAllAsync() =>
        _db.Routes.ToListAsync();

    public async Task<BuslyRoute?> GetByIdAsync(Guid id) =>
        await _db.Routes.FindAsync(id);

    public async Task ToggleAsync(Guid id)
    {
        var route = await _db.Routes.FindAsync(id);
        if (route is null) return;

        route.IsActive = !route.IsActive;
        await _db.SaveChangesAsync();
    }

    public async Task<BuslyRoute?> GetByCitiesAsync(string source, string destination) =>
        await _db.Routes.FirstOrDefaultAsync(r =>
            r.SourceCity.ToLower() == source.ToLower() &&
            r.DestinationCity.ToLower() == destination.ToLower());

    public async Task UpdateAsync(BuslyRoute route)
    {
        _db.Routes.Update(route);
        await _db.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(string sourceCity, string destinationCity) =>
        _db.Routes.AnyAsync(r =>
            r.SourceCity.ToLower() == sourceCity.ToLower() &&
            r.DestinationCity.ToLower() == destinationCity.ToLower());

    public async Task<List<string>> GetCitySuggestionsAsync(string query)
    {
        // Simple approach to eliminate duplicates with proper trimming
        var sql = @"
SELECT DISTINCT TRIM(BOTH FROM source_city) as city
FROM route 
WHERE TRIM(BOTH FROM LOWER(source_city)) LIKE '%' || TRIM(BOTH FROM LOWER({0})) || '%'
UNION
SELECT DISTINCT TRIM(BOTH FROM destination_city) as city  
FROM route 
WHERE TRIM(BOTH FROM LOWER(destination_city)) LIKE '%' || TRIM(BOTH FROM LOWER({0})) || '%'
ORDER BY city
LIMIT 10";

        var results = await _db.Database
            .SqlQueryRaw<CityDto>(sql, query)
            .ToListAsync();

        // Additional normalization in C# to ensure uniqueness
        var cities = results
            .Select(c => c.City?.Trim())
            .Where(city => !string.IsNullOrWhiteSpace(city))
            .Select(city => {
                var normalized = city!.Trim();
                if (normalized.Length > 0)
                    return char.ToUpper(normalized[0]) + normalized.Substring(1).ToLower();
                return normalized;
            })
            .Distinct()
            .Take(10)
            .ToList();

        return cities;
    }
}
