using Busly.API.Models;

namespace Busly.API.Repositories;

public interface ITcRepository
{
    Task<TcVersion?> GetActiveAsync();
    Task<List<TcVersion>> GetAllAsync();
    Task CreateAsync(TcVersion tc);
    Task DeactivateAllExceptAsync(Guid tcId);
}
