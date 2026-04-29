using Busly.API.Data;
using Busly.API.DTOs.Search;
using Busly.API.Models;
using Busly.API.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Busly.API.Repositories;

public class SeatRepository : ISeatRepository
{
    private readonly AppDbContext _db;

    public SeatRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task BulkInsertSeatsAsync(List<Seat> seats)
    {
        _db.Seats.AddRange(seats);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Seat>> GetSeatsByLayoutAsync(Guid layoutId)
    {
        return await _db.Seats
            .Where(s => s.LayoutId == layoutId)
            .ToListAsync();
    }

    public async Task<List<SeatAvailabilityDto>> GetSeatAvailabilityAsync(Guid busId, DateOnly journeyDate)
    {
        var sql = @"
SELECT
    s.id AS ""SeatId"",
    s.seat_number AS ""SeatNumber"",
    s.seat_type AS ""SeatType"",
    s.deck AS ""Deck"",
    CASE
        WHEN bs.id IS NOT NULL THEN 'BOOKED'
        WHEN sl.id IS NOT NULL AND sl.is_active = true AND sl.expires_at > NOW() THEN 'LOCKED'
        ELSE 'AVAILABLE'
    END AS ""Status"",
    bs.passenger_gender AS ""PassengerGender"",
    COALESCE(sl.expires_at, NULL) AS ""LockExpiresAt""
FROM seat s
LEFT JOIN booked_seat bs ON bs.seat_id = s.id AND bs.bus_id = {0} AND bs.journey_date = {1}
LEFT JOIN seat_lock sl ON sl.seat_id = s.id AND sl.bus_id = {0} AND sl.journey_date = {1} AND sl.is_active = true AND sl.expires_at > NOW()
WHERE s.layout_id = (SELECT layout_id FROM bus WHERE id = {0})
ORDER BY s.row_num, s.col_num";

        return await _db.Database
            .SqlQueryRaw<SeatAvailabilityDto>(sql, busId, journeyDate)
            .ToListAsync();
    }

    public async Task<SeatLock> CreateSeatLockAsync(SeatLock seatLock)
    {
        try
        {
            _db.SeatLocks.Add(seatLock);
            await _db.SaveChangesAsync();
            return seatLock;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            throw new SeatAlreadyLockedException("Seat is already locked for this journey date");
        }
    }

    public async Task<bool> HasActiveLockOrBookingAsync(Guid seatId, Guid busId, DateOnly journeyDate)
    {
        // Check for active locks (including TTL enforcement)
        var hasLock = await _db.SeatLocks.AnyAsync(sl =>
            sl.SeatId == seatId &&
            sl.BusId == busId &&
            sl.JourneyDate == journeyDate &&
            sl.IsActive &&
            sl.ExpiresAt > DateTime.UtcNow);

        if (hasLock) return true;

        return await _db.BookedSeats.AnyAsync(bs =>
            bs.SeatId == seatId &&
            bs.BusId == busId &&
            bs.JourneyDate == journeyDate);
    }

    public async Task<SeatLock?> GetActiveLockAsync(Guid lockId)
    {
        return await _db.SeatLocks
            .FirstOrDefaultAsync(sl => sl.Id == lockId && sl.IsActive && sl.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<SeatLock?> ExtendLockAsync(Guid lockId, Guid customerId, TimeSpan extension)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Use SELECT FOR UPDATE to lock the specific lock record
            var seatLock = await _db.SeatLocks
                .FromSqlRaw(
                    "SELECT * FROM seat_lock WHERE id = {0} AND is_active = true AND expires_at > NOW() FOR UPDATE",
                    lockId)
                .FirstOrDefaultAsync();

            if (seatLock is null || seatLock.CustomerId != customerId)
            {
                await transaction.RollbackAsync();
                return null;
            }

            // Extend the lock
            seatLock.ExpiresAt = DateTime.UtcNow.Add(extension);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return seatLock;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<SeatLock?> ForceReleaseLockAsync(Guid lockId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Use SELECT FOR UPDATE to lock the specific lock record
            var seatLock = await _db.SeatLocks
                .FromSqlRaw(
                    "SELECT * FROM seat_lock WHERE id = {0} AND is_active = true AND expires_at > NOW() FOR UPDATE",
                    lockId)
                .FirstOrDefaultAsync();

            if (seatLock is null)
            {
                await transaction.RollbackAsync();
                return null;
            }

            // Force release the lock (admin override)
            seatLock.IsActive = false;
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return seatLock;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<SeatLock>> GetCustomerActiveLocksAsync(Guid customerId)
    {
        return await _db.SeatLocks
            .Where(sl => sl.CustomerId == customerId 
                      && sl.IsActive 
                      && sl.ExpiresAt > DateTime.UtcNow)
            .Include(sl => sl.Seat)
            .Include(sl => sl.Bus)
                .ThenInclude(b => b!.Route)
            .OrderBy(sl => sl.ExpiresAt)
            .ToListAsync();
    }

    public async Task ReleaseLocksByCustomerJourneyAsync(Guid customerId, Guid busId, DateOnly journeyDate)
    {
        var locks = await _db.SeatLocks
            .Where(sl => sl.CustomerId == customerId 
                      && sl.BusId == busId 
                      && sl.JourneyDate == journeyDate 
                      && sl.IsActive)
            .ToListAsync();

        foreach (var lockToRelease in locks)
        {
            lockToRelease.IsActive = false;
        }

        await _db.SaveChangesAsync();
    }

    public async Task ReleaseLockAsync(Guid lockId)
    {
        var seatLock = await _db.SeatLocks.FindAsync(lockId);
        if (seatLock is null) return;

        seatLock.IsActive = false;
        await _db.SaveChangesAsync();
    }

    public async Task<List<SeatLock>> CreateBulkSeatLocksAsync(List<SeatLock> seatLocks)
    {
        // Sort seat IDs to prevent deadlocks during bulk operations
        var sortedLocks = seatLocks
            .OrderBy(sl => sl.SeatId)
            .ThenBy(sl => sl.JourneyDate)
            .ToList();

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.SeatLocks.AddRange(sortedLocks);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return sortedLocks;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            await transaction.RollbackAsync();
            throw new SeatAlreadyLockedException("One or more seats are already locked for this journey date");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> CleanupExpiredLocksAsync()
    {
        return await _db.Database.ExecuteSqlRawAsync(
            "UPDATE seat_lock SET is_active = false WHERE expires_at < NOW() AND is_active = true");
    }

    public async Task ExpireLocksAsync()
    {
        await _db.Database.ExecuteSqlRawAsync(
            "UPDATE seat_lock SET is_active = false WHERE expires_at < NOW() AND is_active = true");
    }
}
