using Busly.API.Data;
using Busly.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Busly.API.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly AppDbContext _db;

    public AuditRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(
        Guid actorId,
        string actorRole,
        string action,
        string entityType,
        Guid? entityId,
        string? metadata = null)
    {
        var log = new AuditLog
        {
            ActorId    = actorId,
            ActorRole  = actorRole,
            Action     = action,
            EntityType = entityType,
            EntityId   = entityId,
            Metadata   = metadata
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetAllAsync()
    {
        return await _db.AuditLogs
            .OrderByDescending(l => l.PerformedAt)
            .ToListAsync();
    }
}
