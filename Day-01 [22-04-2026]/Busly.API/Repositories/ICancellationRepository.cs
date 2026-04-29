using Busly.API.Models;

namespace Busly.API.Repositories;

public interface ICancellationRepository
{
    Task<Cancellation> CreateCancellationAsync(Cancellation cancellation);
    Task UpdateRefundStatusAsync(Guid cancellationId, string refundStatus);
    Task<Cancellation?> GetCancellationByBookingIdAsync(Guid bookingId);
}
