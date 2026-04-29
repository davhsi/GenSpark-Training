using Busly.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Busly.API.Services;

public interface IConfigService
{
    Task<string> GetConfigAsync(string key, string defaultValue);
    Task SetConfigAsync(string key, string value);
}

public class ConfigService : IConfigService
{
    private readonly AppDbContext _db;

    public ConfigService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string> GetConfigAsync(string key, string defaultValue)
    {
        var config = await _db.PlatformConfigs.FindAsync(key);
        return config?.Value ?? defaultValue;
    }

    public async Task SetConfigAsync(string key, string value)
    {
        var config = await _db.PlatformConfigs.FindAsync(key);
        if (config == null)
        {
            _db.PlatformConfigs.Add(new Models.PlatformConfig { Key = key, Value = value });
        }
        else
        {
            config.Value = value;
        }
        await _db.SaveChangesAsync();
    }
}
