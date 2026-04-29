using Busly.API.DTOs.Search;

namespace Busly.API.Services;

public interface ISearchService
{
    Task<List<BusSearchResultDto>> SearchBusesAsync(string from, string to, DateOnly date);
    Task<SeatMapResponse> GetSeatMapAsync(Guid busId, DateOnly date);
    Task<List<string>> GetCitySuggestionsAsync(string query);
    Task<BusSearchResultDto> GetBusDetailsAsync(Guid busId);
}
