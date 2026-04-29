using Busly.API.Data;
using Busly.API.DTOs.Cancellation;
using Busly.API.Helpers;
using Busly.API.Models;
using Busly.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Busly.API.Services;

public class CancellationService : ICancellationService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly ICancellationRepository _cancellationRepo;
    private readonly ICouponRepository _couponRepo;
    private readonly IEmailService _emailService;
    private readonly AppDbContext _db;
    private readonly ILogger<CancellationService> _logger;
    private readonly IAuditRepository _auditRepo;

    public CancellationService(
        IBookingRepository bookingRepo,
        ICancellationRepository cancellationRepo,
        ICouponRepository couponRepo,
        IEmailService emailService,
        AppDbContext db,
        ILogger<CancellationService> logger,
        IAuditRepository auditRepo)
    {
        _bookingRepo      = bookingRepo;
        _cancellationRepo = cancellationRepo;
        _couponRepo       = couponRepo;
        _emailService     = emailService;
        _db               = db;
        _logger           = logger;
        _auditRepo        = auditRepo;
    }

    // ── Customer-triggered cancellation ──────────────────────────────────────

    public async Task<CancellationDto> CancelBookingAsync(Guid bookingId, Guid customerId)
    {
        var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);

        if (booking is null)
            throw new KeyNotFoundException($"Booking {bookingId} not found.");

        if (booking.CustomerId != customerId)
            throw new UnauthorizedAccessException("You are not authorised to cancel this booking.");

        if (booking.Status != "CONFIRMED")
            throw new InvalidOperationException("Booking is not confirmed.");

        // Determine departure time from the first BOARDING stop
        var departureTime = booking.Bus?.BusStops
            .Where(s => s.Type == "BOARDING" && s.ScheduledTime.HasValue)
            .Select(s => s.ScheduledTime!.Value)
            .FirstOrDefault() ?? TimeOnly.MinValue;

        var refundAmount = RefundCalculatorService.Calculate(
            departureTime,
            booking.JourneyDate,
            booking.BaseFare ?? 0m);

        var cancellation = new Cancellation
        {
            Id           = Guid.NewGuid(),
            BookingId    = bookingId,
            CancelledBy  = "customer",
            RefundAmount = refundAmount,
            RefundStatus = "PENDING",
            CancelledAt  = DateTime.UtcNow
        };

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _cancellationRepo.CreateCancellationAsync(cancellation);
            await _bookingRepo.UpdateBookingStatusAsync(bookingId, "CANCELLED");
            await _auditRepo.LogAsync(customerId, "customer", "CANCEL_BOOKING", "booking", bookingId);
            await _auditRepo.LogAsync(customerId, "customer", "CREATE_CANCELLATION", "cancellation", cancellation.Id);
            
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        // Enqueue email AFTER commit so the booking status is already CANCELLED in the DB
        booking.Status = "CANCELLED";
        await _emailService.EnqueueCancellationAsync(booking, refundAmount, "customer");

        return new CancellationDto
        {
            CancellationId = cancellation.Id,
            BookingId      = bookingId,
            CancelledBy    = cancellation.CancelledBy,
            RefundAmount   = cancellation.RefundAmount,
            RefundStatus   = cancellation.RefundStatus,
            CancelledAt    = cancellation.CancelledAt
        };
    }

    // ── Operator cascade cancellation ─────────────────────────────────────────

    public async Task ProcessOperatorCascadeAsync(Guid busId, Guid operatorId)
    {
        var bookings = await _bookingRepo.GetConfirmedFutureBookingsByBusAsync(busId);
        if (!bookings.Any()) return;

        // Requirement 14.5: process entire cascade in a SINGLE transaction
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            foreach (var booking in bookings)
            {
                var cancellation = new Cancellation
                {
                    Id           = Guid.NewGuid(),
                    BookingId    = booking.Id,
                    CancelledBy  = "operator",
                    RefundAmount = booking.TotalAmount, // Full refund for operator cancellations
                    RefundStatus = "PENDING",
                    CancelledAt  = DateTime.UtcNow
                };

                await _cancellationRepo.CreateCancellationAsync(cancellation);
                await _bookingRepo.UpdateBookingStatusAsync(booking.Id, "CANCELLED");
                await _auditRepo.LogAsync(operatorId, "operator", "CASCADE_CANCEL_BOOKING", "booking", booking.Id);
                await _auditRepo.LogAsync(operatorId, "operator", "CREATE_CANCELLATION", "cancellation", cancellation.Id);

                var couponCode = CouponGenerator.Generate();
                var coupon = new Coupon
                {
                    Id               = Guid.NewGuid(),
                    Code             = couponCode,
                    DiscountType     = "flat",
                    DiscountValue    = booking.TotalAmount, // Compensation coupon
                    ExpiresAt        = DateTime.UtcNow.AddDays(30),
                    IssuedToCustomer = booking.CustomerId,
                    CancellationId   = cancellation.Id,
                    IsUsed           = false
                };

                await _couponRepo.CreateCouponAsync(coupon);
                await _auditRepo.LogAsync(operatorId, "operator", "ISSUE_COUPON", "coupon", coupon.Id);

                // Enqueue notifications
                var compensationAmount = booking.TotalAmount ?? 0m;
                await _emailService.EnqueueCancellationAsync(booking, compensationAmount, "operator");

                if (booking.CustomerId.HasValue)
                {
                    await _emailService.EnqueueCouponAsync(
                        booking.CustomerId.Value,
                        booking.Customer?.Email ?? "",
                        couponCode,
                        compensationAmount);
                }
            }

            await transaction.CommitAsync();
            _logger.LogInformation("Operator cascade completed successfully for Bus {BusId}. {Count} bookings processed.", busId, bookings.Count);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Operator cascade failed for Bus {BusId}. Entire transaction rolled back.", busId);
            throw;
        }
    }
}
