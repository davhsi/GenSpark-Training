using Busly.API.DTOs.Search;
using Busly.API.Models;
using Busly.API.Repositories;
using Busly.API.Exceptions;
using Busly.API.Helpers;

namespace Busly.API.Services;

public class SeatLockService : ISeatLockService
{
    private readonly ISeatRepository _seatRepo;
    private readonly ISeatLockSecurityService _securityService;
    private readonly IDateValidationService _dateValidationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SeatLockService> _logger;

    public SeatLockService(
        ISeatRepository seatRepo,
        ISeatLockSecurityService securityService,
        IDateValidationService dateValidationService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<SeatLockService> logger)
    {
        _seatRepo = seatRepo;
        _securityService = securityService;
        _dateValidationService = dateValidationService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<SeatLockDto> CreateLockAsync(CreateSeatLockRequest request, Guid customerId)
    {
        var ipAddress = GetClientIpAddress();
        
        // Date validation - CRITICAL: Prevent past date bookings
        _dateValidationService.ValidateJourneyDate(request.JourneyDate);
        
        // Security checks
        if (!await _securityService.CanCreateLockAsync(ipAddress, customerId, request.BusId))
        {
            await _securityService.RecordLockAttemptAsync(ipAddress, customerId, false);
            throw new InvalidOperationException("Rate limit exceeded or suspicious activity detected. Please try again later.");
        }

        return await RetryHelper.ExecuteWithRetryAsync(async () =>
        {
            try
            {
                // INSERT-first approach - no pre-checks needed
                // Unique constraint will handle concurrent attempts
                var seatLock = new SeatLock
                {
                    SeatId = request.SeatId,
                    BusId = request.BusId,
                    CustomerId = customerId,
                    JourneyDate = request.JourneyDate,
                    LockedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    IsActive = true
                };

                var created = await _seatRepo.CreateSeatLockAsync(seatLock);
                
                // Record successful attempt
                await _securityService.RecordLockAttemptAsync(ipAddress, customerId, true);

                return new SeatLockDto
                {
                    LockId = created.Id,
                    SeatId = created.SeatId!.Value,
                    BusId = created.BusId!.Value,
                    JourneyDate = created.JourneyDate,
                    ExpiresAt = created.ExpiresAt
                };
            }
            catch (Exception)
            {
                await _securityService.RecordLockAttemptAsync(ipAddress, customerId, false);
                throw;
            }
        });
    }

    public async Task<BulkSeatLockResponse> CreateBulkLocksAsync(BulkSeatLockRequest request, Guid customerId)
    {
        var ipAddress = GetClientIpAddress();
        
        // Date validation - CRITICAL: Prevent past date bookings
        _dateValidationService.ValidateJourneyDate(request.JourneyDate);
        
        // Enhanced security for bulk operations
        if (!await _securityService.CanCreateLockAsync(ipAddress, customerId, request.BusId))
        {
            await _securityService.RecordLockAttemptAsync(ipAddress, customerId, false);
            throw new InvalidOperationException("Rate limit exceeded or suspicious activity detected. Please try again later.");
        }

        // Additional check for bulk operations - limit to max 4 seats
        if (request.SeatIds.Count > 4)
        {
            throw new InvalidOperationException("Cannot lock more than 4 seats per booking");
        }

        var successfulLocks = new List<SeatLockDto>();
        var failedSeatIds   = new List<Guid>();

        // Lock each seat individually so a conflict on one doesn't block the others
        foreach (var seatId in request.SeatIds)
        {
            try
            {
                var seatLock = new SeatLock
                {
                    SeatId      = seatId,
                    BusId       = request.BusId,
                    CustomerId  = customerId,
                    JourneyDate = request.JourneyDate,
                    LockedAt    = DateTime.UtcNow,
                    ExpiresAt   = DateTime.UtcNow.AddMinutes(10),
                    IsActive    = true
                };

                var created = await _seatRepo.CreateSeatLockAsync(seatLock);
                successfulLocks.Add(new SeatLockDto
                {
                    LockId      = created.Id,
                    SeatId      = created.SeatId!.Value,
                    BusId       = created.BusId!.Value,
                    JourneyDate = created.JourneyDate,
                    ExpiresAt   = created.ExpiresAt
                });
            }
            catch (SeatAlreadyLockedException)
            {
                failedSeatIds.Add(seatId);
            }
        }

        await _securityService.RecordLockAttemptAsync(ipAddress, customerId, failedSeatIds.Count == 0);

        return new BulkSeatLockResponse
        {
            SuccessfulLocks = successfulLocks,
            FailedSeatIds   = failedSeatIds
        };
    }

    private string GetClientIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return "unknown";

        // Check for forwarded headers (common with load balancers)
        var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(ipAddress))
        {
            return ipAddress.Split(',')[0].Trim();
        }

        // Check for Real IP header
        ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(ipAddress))
        {
            return ipAddress;
        }

        // Fall back to remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    public async Task<SeatLockDto?> ExtendLockAsync(Guid lockId, Guid customerId)
    {
        var extendedLock = await _seatRepo.ExtendLockAsync(lockId, customerId, TimeSpan.FromMinutes(10));
        
        if (extendedLock is null)
            return null;

        return new SeatLockDto
        {
            LockId = extendedLock.Id,
            SeatId = extendedLock.SeatId!.Value,
            BusId = extendedLock.BusId!.Value,
            JourneyDate = extendedLock.JourneyDate,
            ExpiresAt = extendedLock.ExpiresAt
        };
    }

    public async Task<SeatLockDto?> ForceReleaseLockAsync(Guid lockId)
    {
        var releasedLock = await _seatRepo.ForceReleaseLockAsync(lockId);
        
        if (releasedLock is null)
            return null;

        return new SeatLockDto
        {
            LockId = releasedLock.Id,
            SeatId = releasedLock.SeatId!.Value,
            BusId = releasedLock.BusId!.Value,
            JourneyDate = releasedLock.JourneyDate,
            ExpiresAt = releasedLock.ExpiresAt
        };
    }

    public async Task<List<SeatLockDto>> GetCustomerActiveLocksAsync(Guid customerId)
    {
        var activeLocks = await _seatRepo.GetCustomerActiveLocksAsync(customerId);
        
        return activeLocks.Select(l => new SeatLockDto
        {
            LockId = l.Id,
            SeatId = l.SeatId!.Value,
            BusId = l.BusId!.Value,
            JourneyDate = l.JourneyDate,
            ExpiresAt = l.ExpiresAt
        }).ToList();
    }

    public async Task ReleaseLocksByCustomerJourneyAsync(Guid customerId, Guid busId, DateOnly journeyDate)
    {
        await _seatRepo.ReleaseLocksByCustomerJourneyAsync(customerId, busId, journeyDate);
    }

    public async Task ReleaseLockAsync(Guid lockId, Guid customerId)
    {
        var seatLock = await _seatRepo.GetActiveLockAsync(lockId)
            ?? throw new KeyNotFoundException($"Active lock {lockId} not found.");

        if (seatLock.CustomerId != customerId)
            throw new UnauthorizedAccessException("You do not own this seat lock.");

        await _seatRepo.ReleaseLockAsync(lockId);
    }
}
