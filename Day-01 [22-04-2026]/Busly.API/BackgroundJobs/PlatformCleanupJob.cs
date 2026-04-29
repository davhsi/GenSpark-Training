using Busly.API.Repositories;
using Busly.API.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Busly.API.BackgroundJobs;

public class PlatformCleanupJob : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PlatformCleanupJob> _logger;
    private Task? _executingTask;
    private CancellationTokenSource? _cts;

    public PlatformCleanupJob(IServiceScopeFactory scopeFactory, ILogger<PlatformCleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executingTask = RunAsync(_cts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts is not null)
        {
            await _cts.CancelAsync();
        }

        if (_executingTask is not null)
        {
            try
            {
                await _executingTask;
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown — swallow gracefully
            }
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));

        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var expiredLocksCount = 0;
                var timeoutBookingsCount = 0;

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    
                    // 1. Expire Seat Locks
                    try
                    {
                        var seatRepo = scope.ServiceProvider.GetRequiredService<ISeatRepository>();
                        await seatRepo.ExpireLocksAsync();
                        expiredLocksCount = 1; // We don't get exact count from current implementation
                        _logger.LogDebug("Successfully expired seat locks");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to expire seat locks during cleanup");
                    }

                    // 2. Handle Payment Timeouts (15 minute window)
                    try
                    {
                        var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                        await bookingRepo.HandlePaymentTimeoutsAsync(TimeSpan.FromMinutes(15));
                        timeoutBookingsCount = 1; // We don't get exact count from current implementation
                        _logger.LogDebug("Successfully handled payment timeouts");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to handle payment timeouts during cleanup");
                    }

                    stopwatch.Stop();
                    _logger.LogInformation(
                        "Platform cleanup job completed at {Time}. Duration: {Duration}ms. Locks expired: {ExpiredLocks}, Payment timeouts: {TimeoutBookings}", 
                        DateTime.UtcNow, 
                        stopwatch.ElapsedMilliseconds,
                        expiredLocksCount,
                        timeoutBookingsCount);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    stopwatch.Stop();
                    _logger.LogError(ex, "Critical error occurred during platform cleanup. Duration: {Duration}ms", stopwatch.ElapsedMilliseconds);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown requested — exit loop gracefully
        }
    }
}
