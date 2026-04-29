using Busly.API.Data;
using Busly.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Busly.API.Repositories;

public class CancellationRepository : ICancellationRepository
{
    private readonly AppDbContext _db;

    public CancellationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Cancellation> CreateCancellationAsync(Cancellation cancellation)
    {
        _db.Cancellations.Add(cancellation);
        await _db.SaveChangesAsync();
        return cancellation;
    }

    public async Task UpdateRefundStatusAsync(Guid cancellationId, string refundStatus)
    {
        var cancellation = await _db.Cancellations.FindAsync(cancellationId);
        if (cancellation is null) return;

        cancellation.RefundStatus = refundStatus;
        await _db.SaveChangesAsync();
    }

    public async Task<Cancellation?> GetCancellationByBookingIdAsync(Guid bookingId)
    {
        return await _db.Cancellations
            .FirstOrDefaultAsync(c => c.BookingId == bookingId);
    }
}
