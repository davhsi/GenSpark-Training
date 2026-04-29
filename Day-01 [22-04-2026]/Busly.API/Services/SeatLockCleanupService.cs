using Busly.API.Services;

namespace Busly.API.Services;

public class SeatLockCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SeatLockCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5); // Run every 5 minutes

    public SeatLockCleanupService(
        IServiceProvider serviceProvider,
        ILogger<SeatLockCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Seat Lock Cleanup Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredLocksAsync();
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during seat lock cleanup");
                // Continue running even if cleanup fails
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        _logger.LogInformation("Seat Lock Cleanup Service stopped");
    }

    private async Task CleanupExpiredLocksAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var securityService = scope.ServiceProvider.GetRequiredService<ISeatLockSecurityService>();
        
        await securityService.CleanupExpiredLocksAsync();
    }
}
