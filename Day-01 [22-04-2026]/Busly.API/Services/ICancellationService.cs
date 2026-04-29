using Busly.API.DTOs.Cancellation;

namespace Busly.API.Services;

public interface ICancellationService
{
    /// <summary>
    /// Customer-triggered cancellation. Verifies ownership and booking status,
    /// calculates refund, persists the cancellation, and enqueues a notification email.
    /// </summary>
    Task<CancellationDto> CancelBookingAsync(Guid bookingId, Guid customerId);

    /// <summary>
    /// Operator-triggered cascade when a bus is disabled or removed.
    /// Cancels all confirmed future bookings for the bus and issues compensation coupons.
    /// </summary>
    Task ProcessOperatorCascadeAsync(Guid busId, Guid operatorId);
}
