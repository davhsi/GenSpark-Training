namespace Busly.API.Repositories;

public interface IAuditRepository
{
    Task LogAsync(
        Guid actorId,
        string actorRole,
        string action,
        string entityType,
        Guid? entityId,
        string? metadata = null);
    
    Task<List<Models.AuditLog>> GetAllAsync();
}
