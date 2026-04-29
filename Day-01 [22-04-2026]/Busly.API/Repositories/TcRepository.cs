using Busly.API.Data;
using Busly.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Busly.API.Repositories;

public class TcRepository : ITcRepository
{
    private readonly AppDbContext _db;

    public TcRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TcVersion?> GetActiveAsync()
    {
        return await _db.TcVersions.FirstOrDefaultAsync(v => v.IsActive);
    }

    public async Task<List<TcVersion>> GetAllAsync()
    {
        return await _db.TcVersions
            .OrderByDescending(v => v.PublishedAt)
            .ToListAsync();
    }

    public async Task CreateAsync(TcVersion tc)
    {
        await _db.TcVersions.AddAsync(tc);
        await _db.SaveChangesAsync();
    }

    public async Task DeactivateAllExceptAsync(Guid tcId)
    {
        var versions = await _db.TcVersions.Where(v => v.Id != tcId && v.IsActive).ToListAsync();
        foreach (var v in versions)
        {
            v.IsActive = false;
        }
        await _db.SaveChangesAsync();
    }
}
