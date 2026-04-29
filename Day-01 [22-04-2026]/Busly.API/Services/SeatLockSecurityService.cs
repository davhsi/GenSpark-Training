using Busly.API.Models;
using Busly.API.Repositories;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace Busly.API.Services;

public interface ISeatLockSecurityService
{
    Task<bool> CanCreateLockAsync(string ipAddress, Guid customerId, Guid busId);
    Task RecordLockAttemptAsync(string ipAddress, Guid customerId, bool success);
    Task CleanupExpiredLocksAsync();
    Task<bool> IsSuspiciousActivityAsync(string ipAddress, Guid customerId);
}

public class SeatLockSecurityService : ISeatLockSecurityService
{
    private readonly ISeatRepository _seatRepo;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SeatLockSecurityService> _logger;
    
    // Rate limiting: Max 5 lock attempts per minute per IP
    private const int MAX_LOCK_ATTEMPTS_PER_MINUTE = 5;
    private const int MAX_LOCKS_PER_CUSTOMER_PER_HOUR = 10;
    private const int MAX_LOCKS_PER_IP_PER_HOUR = 20;
    
    // Track suspicious activity
    private readonly ConcurrentDictionary<string, List<DateTime>> _ipAttempts = new();
    private readonly ConcurrentDictionary<Guid, List<DateTime>> _customerAttempts = new();

    public SeatLockSecurityService(
        ISeatRepository seatRepo,
        IMemoryCache cache,
        ILogger<SeatLockSecurityService> logger)
    {
        _seatRepo = seatRepo;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> CanCreateLockAsync(string ipAddress, Guid customerId, Guid busId)
    {
        var now = DateTime.UtcNow;
        
        // Check IP rate limiting
        var ipKey = $"lock_attempts_ip_{ipAddress}";
        var ipAttempts = _cache.GetOrCreate(ipKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return new List<DateTime>();
        })!;
        
        if (ipAttempts.Count >= MAX_LOCK_ATTEMPTS_PER_MINUTE)
        {
            _logger.LogWarning("Rate limit exceeded for IP: {IpAddress}", ipAddress);
            return false;
        }

        // Check customer rate limiting
        var customerKey = $"lock_attempts_customer_{customerId}";
        var customerAttempts = _cache.GetOrCreate(customerKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return new List<DateTime>();
        })!;
        
        // Clean old attempts
        customerAttempts.RemoveAll(t => now - t > TimeSpan.FromHours(1));
        
        if (customerAttempts.Count >= MAX_LOCKS_PER_CUSTOMER_PER_HOUR)
        {
            _logger.LogWarning("Customer rate limit exceeded: {CustomerId}", customerId);
            return false;
        }

        // Check existing active locks for this customer
        var activeLocks = await _seatRepo.GetCustomerActiveLocksAsync(customerId);
        if (activeLocks.Count >= 4) // Max 4 active locks per customer
        {
            _logger.LogWarning("Customer has too many active locks: {CustomerId}, Count: {Count}", customerId, activeLocks.Count);
            return false;
        }

        // Check for suspicious patterns
        if (await IsSuspiciousActivityAsync(ipAddress, customerId))
        {
            _logger.LogWarning("Suspicious activity detected for IP: {IpAddress}, Customer: {CustomerId}", ipAddress, customerId);
            return false;
        }

        return true;
    }

    public async Task RecordLockAttemptAsync(string ipAddress, Guid customerId, bool success)
    {
        var now = DateTime.UtcNow;
        
        // Record IP attempt
        var ipKey = $"lock_attempts_ip_{ipAddress}";
        var ipAttempts = _cache.Get<List<DateTime>>(ipKey) ?? new List<DateTime>();
        ipAttempts.Add(now);
        _cache.Set(ipKey, ipAttempts, TimeSpan.FromMinutes(1));

        // Record customer attempt
        var customerKey = $"lock_attempts_customer_{customerId}";
        var customerAttempts = _cache.Get<List<DateTime>>(customerKey) ?? new List<DateTime>();
        customerAttempts.Add(now);
        _cache.Set(customerKey, customerAttempts, TimeSpan.FromHours(1));

        // Log failed attempts for monitoring
        if (!success)
        {
            _logger.LogWarning("Failed lock attempt - IP: {IpAddress}, Customer: {CustomerId}", ipAddress, customerId);
        }
    }

    public async Task CleanupExpiredLocksAsync()
    {
        try
        {
            var cleanedCount = await _seatRepo.CleanupExpiredLocksAsync();
            if (cleanedCount > 0)
            {
                _logger.LogInformation("Cleaned up {Count} expired seat locks", cleanedCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired seat locks");
        }
    }

    public async Task<bool> IsSuspiciousActivityAsync(string ipAddress, Guid customerId)
    {
        var now = DateTime.UtcNow;
        
        // Check for rapid successive attempts from same IP
        var recentIpAttempts = _ipAttempts.GetOrAdd(ipAddress, _ => new List<DateTime>())
            .Where(t => now - t < TimeSpan.FromMinutes(5))
            .ToList();
        
        if (recentIpAttempts.Count > 10) // More than 10 attempts in 5 minutes
        {
            return true;
        }

        // Check for multiple customer IDs from same IP (potential account creation abuse)
        var ipCustomerKey = $"ip_customers_{ipAddress}";
        var ipCustomers = _cache.GetOrCreate(ipCustomerKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
            return new HashSet<Guid>();
        })!;
        
        ipCustomers.Add(customerId);
        
        if (ipCustomers.Count > 3)
        {
            return true;
        }

        // Check for seat locking across multiple buses (potential bot activity)
        var customerBusKey = $"customer_buses_{customerId}";
        var customerBuses = _cache.GetOrCreate(customerBusKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return new HashSet<Guid>();
        })!;
        
        var activeLocks = await _seatRepo.GetCustomerActiveLocksAsync(customerId);
        foreach (var lockInfo in activeLocks)
        {
            if (lockInfo.BusId.HasValue)
            {
                customerBuses.Add(lockInfo.BusId.Value);
            }
        }
        
        if (customerBuses.Count > 5)
        {
            return true;
        }

        return false;
    }
}
