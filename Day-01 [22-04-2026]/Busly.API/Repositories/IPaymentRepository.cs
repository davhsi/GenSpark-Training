using Busly.API.Models;

namespace Busly.API.Repositories;

public interface IPaymentRepository
{
    Task<Payment> CreatePaymentAsync(Payment payment);
    Task UpdatePaymentStatusAsync(Guid paymentId, string status, string? transactionRef = null);
    Task<Payment?> GetPaymentByBookingIdAsync(Guid bookingId);
}
