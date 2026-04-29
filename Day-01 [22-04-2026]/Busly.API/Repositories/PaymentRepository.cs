using Busly.API.Data;
using Busly.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Busly.API.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _db;

    public PaymentRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Payment> CreatePaymentAsync(Payment payment)
    {
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
        return payment;
    }

    public async Task UpdatePaymentStatusAsync(Guid paymentId, string status, string? transactionRef = null)
    {
        var payment = await _db.Payments.FindAsync(paymentId);
        if (payment is null) return;

        payment.Status = status;

        if (transactionRef is not null)
        {
            payment.TransactionRef = transactionRef;
            payment.PaidAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<Payment?> GetPaymentByBookingIdAsync(Guid bookingId)
    {
        return await _db.Payments
            .FirstOrDefaultAsync(p => p.BookingId == bookingId);
    }
}
