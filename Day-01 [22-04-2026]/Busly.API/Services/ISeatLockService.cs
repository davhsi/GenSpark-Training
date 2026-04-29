using Busly.API.DTOs.Search;

namespace Busly.API.Services;

public interface ISeatLockService
{
    Task<SeatLockDto> CreateLockAsync(CreateSeatLockRequest request, Guid customerId);
    Task<BulkSeatLockResponse> CreateBulkLocksAsync(BulkSeatLockRequest request, Guid customerId);
    Task<SeatLockDto?> ExtendLockAsync(Guid lockId, Guid customerId);
    Task<SeatLockDto?> ForceReleaseLockAsync(Guid lockId);
    Task<List<SeatLockDto>> GetCustomerActiveLocksAsync(Guid customerId);
    Task ReleaseLocksByCustomerJourneyAsync(Guid customerId, Guid busId, DateOnly journeyDate);
    Task ReleaseLockAsync(Guid lockId, Guid customerId);
}
