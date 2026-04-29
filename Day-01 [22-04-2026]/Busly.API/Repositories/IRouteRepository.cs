using BuslyRoute = Busly.API.Models.Route;

namespace Busly.API.Repositories;

public interface IRouteRepository
{
    Task<BuslyRoute> CreateAsync(BuslyRoute route);
    Task<List<BuslyRoute>> GetAllActiveAsync();
    Task<List<BuslyRoute>> GetAllAsync();
    Task<BuslyRoute?> GetByIdAsync(Guid id);
    Task<BuslyRoute?> GetByCitiesAsync(string source, string destination);
    Task ToggleAsync(Guid id);
    Task UpdateAsync(BuslyRoute route);
    Task<bool> ExistsAsync(string sourceCity, string destinationCity);
    Task<List<string>> GetCitySuggestionsAsync(string query);
}
