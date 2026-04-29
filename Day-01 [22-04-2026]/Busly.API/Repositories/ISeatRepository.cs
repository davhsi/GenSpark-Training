using Busly.API.DTOs.Search;
using Busly.API.Models;

namespace Busly.API.Repositories;

public interface ISeatRepository
{
    Task BulkInsertSeatsAsync(List<Seat> seats);
    Task<List<Seat>> GetSeatsByLayoutAsync(Guid layoutId);
    Task<List<SeatAvailabilityDto>> GetSeatAvailabilityAsync(Guid busId, DateOnly journeyDate);
    Task<SeatLock> CreateSeatLockAsync(SeatLock seatLock);
    Task<List<SeatLock>> CreateBulkSeatLocksAsync(List<SeatLock> seatLocks);
    Task<bool> HasActiveLockOrBookingAsync(Guid seatId, Guid busId, DateOnly journeyDate);
    Task<SeatLock?> GetActiveLockAsync(Guid lockId);
    Task<SeatLock?> ExtendLockAsync(Guid lockId, Guid customerId, TimeSpan extension);
    Task<SeatLock?> ForceReleaseLockAsync(Guid lockId);
    Task<List<SeatLock>> GetCustomerActiveLocksAsync(Guid customerId);
    Task ReleaseLocksByCustomerJourneyAsync(Guid customerId, Guid busId, DateOnly journeyDate);
    Task ReleaseLockAsync(Guid lockId);
    Task<int> CleanupExpiredLocksAsync();
    Task ExpireLocksAsync();
}
