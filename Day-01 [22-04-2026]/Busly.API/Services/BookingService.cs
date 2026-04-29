using Busly.API.Data;
using Busly.API.DTOs.Booking;
using Busly.API.Models;
using Busly.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Busly.API.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly ICouponRepository _couponRepo;
    private readonly ISeatRepository _seatRepo;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IPdfService _pdfService;
    private readonly IEmailService _emailService;
    private readonly IConfigService _configService;
    private readonly IAuditRepository _auditRepo;
    private readonly IDateValidationService _dateValidationService;

    public BookingService(
        IBookingRepository bookingRepo,
        IPaymentRepository paymentRepo,
        ICouponRepository couponRepo,
        ISeatRepository seatRepo,
        AppDbContext db,
        IConfiguration config,
        IPdfService pdfService,
        IEmailService emailService,
        IConfigService configService,
        IAuditRepository auditRepo,
        IDateValidationService dateValidationService)
    {
        _bookingRepo = bookingRepo;
        _paymentRepo = paymentRepo;
        _couponRepo = couponRepo;
        _seatRepo = seatRepo;
        _db = db;
        _config = config;
        _pdfService = pdfService;
        _emailService = emailService;
        _configService = configService;
        _auditRepo = auditRepo;
        _dateValidationService = dateValidationService;
    }

    public async Task<BookingDto> CreateBookingAsync(CreateBookingRequest request, Guid customerId)
    {
        // 0. CRITICAL: Date validation - Prevent past date bookings
        _dateValidationService.ValidateBookingDate(request.JourneyDate);
        
        // 1. T&C check
        var customer = await _db.Customers.FindAsync(customerId)
            ?? throw new InvalidOperationException("Customer not found.");

        var activeTc = await _db.TcVersions
            .Where(t => t.IsActive)
            .FirstOrDefaultAsync();

        if (activeTc != null && customer.TcVersion != activeTc.Version)
        {
            throw new InvalidOperationException("TC_REACCEPTANCE_REQUIRED");
        }

        // 2. Idempotency: check if any seat already has a booking for this customer
        foreach (var passenger in request.Passengers)
        {
            var existing = await _bookingRepo.GetExistingBookingAsync(
                customerId, passenger.SeatId, request.JourneyDate);

            if (existing != null)
            {
                // Release any locks the customer may have created for this attempt
                // before returning the existing booking
                await _seatRepo.ReleaseLocksByCustomerJourneyAsync(
                    customerId, request.BusId, request.JourneyDate);
                return MapToDto(existing);
            }
        }

        // 3. Get bus with layout
        var bus = await _db.Buses
            .Include(b => b.Layout)
            .FirstOrDefaultAsync(b => b.Id == request.BusId)
            ?? throw new InvalidOperationException("Bus not found.");
        
        // 4. Fare calculation
        var seatCount = request.Passengers.Count;
        var baseFare = (bus.BasePrice ?? 0) * seatCount;

        var feeType = await _configService.GetConfigAsync("convenience_fee_type", _config["ConvenienceFee:Type"] ?? "flat");
        var feeValue = await _configService.GetConfigAsync("convenience_fee_value", _config["ConvenienceFee:Value"] ?? "0");
        decimal convenienceFee;

        var parsedFeeValue = decimal.TryParse(feeValue, out var fv) ? fv : 0m;
        if (feeType.Equals("percent", StringComparison.OrdinalIgnoreCase))
        {
            convenienceFee = baseFare * (parsedFeeValue / 100);
        }
        else
        {
            convenienceFee = parsedFeeValue;
        }

        var totalAmount = baseFare + convenienceFee;

        // 5. Coupon validation
        Coupon? validCoupon = null;
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var coupon = await _couponRepo.GetCouponByCodeAsync(request.CouponCode);

            if (coupon == null
                || coupon.IsUsed
                || (coupon.ExpiresAt.HasValue && coupon.ExpiresAt.Value < DateTime.UtcNow)
                || coupon.IssuedToCustomer != customerId)
            {
                throw new InvalidOperationException("Invalid coupon");
            }

            // Apply discount
            if (coupon.DiscountType?.Equals("flat", StringComparison.OrdinalIgnoreCase) == true)
            {
                totalAmount -= coupon.DiscountValue ?? 0;
            }
            else if (coupon.DiscountType?.Equals("percent", StringComparison.OrdinalIgnoreCase) == true)
            {
                totalAmount -= baseFare * ((coupon.DiscountValue ?? 0) / 100);
            }

            if (totalAmount < 0) totalAmount = 0;
            validCoupon = coupon;
        }

        // 6. Create Booking entity
        var booking = new Booking
        {
            Id             = Guid.NewGuid(),
            CustomerId     = customerId,
            BusId          = request.BusId,
            JourneyDate    = request.JourneyDate,
            BaseFare       = baseFare,
            ConvenienceFee = convenienceFee,
            TotalAmount    = totalAmount,
            Status         = "INITIATED",
            BookedAt       = DateTime.UtcNow,
            CouponId       = validCoupon?.Id   // set upfront so the first INSERT has the correct total
        };
        // Derive PNR from the booking ID and store it — enables O(1) indexed lookup
        booking.Pnr = booking.Id.ToString("N")[..8].ToUpper();

        await _bookingRepo.CreateBookingAsync(booking);

        // 7. Create BookedSeat entities
        var bookedSeats = request.Passengers.Select(p => new BookedSeat
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            SeatId = p.SeatId,
            BusId = request.BusId,
            JourneyDate = request.JourneyDate,
            PassengerName = p.Name,
            PassengerAge = p.Age,
            PassengerGender = p.Gender
        }).ToList();

        await _bookingRepo.CreateBookedSeatsAsync(bookedSeats);
        booking.BookedSeats = bookedSeats;

        // 8. Mark coupon as applied (couponId already set on booking above)
        if (validCoupon != null)
        {
            // UpdateBookingCouponAsync is now only needed if totalAmount changed after creation
            // (it's already correct, but call it to keep the coupon_id FK consistent)
            await _bookingRepo.UpdateBookingCouponAsync(booking.Id, validCoupon.Id, totalAmount);
            booking.CouponId = validCoupon.Id;
        }

        await _auditRepo.LogAsync(customerId, "customer", "CREATE_BOOKING", "booking", booking.Id);

        return MapToDto(booking);
    }

    public async Task<BookingDto> ProcessPaymentAsync(Guid bookingId, Guid customerId)
    {
        // 1. Get booking and verify ownership
        var booking = await _bookingRepo.GetBookingByIdAsync(bookingId)
            ?? throw new InvalidOperationException("Booking not found.");

        if (booking.CustomerId != customerId)
            throw new UnauthorizedAccessException("Access denied.");

        if (booking.Status != "INITIATED")
            throw new InvalidOperationException("Booking is not in INITIATED status.");

        // 2. Update booking status to PAYMENT_PENDING
        await _bookingRepo.UpdateBookingStatusAsync(bookingId, "PAYMENT_PENDING");
        booking.Status = "PAYMENT_PENDING";

        // 3. Create Payment record
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            Amount = booking.TotalAmount,
            Status = "PENDING"
        };

        await _paymentRepo.CreatePaymentAsync(payment);
        await _auditRepo.LogAsync(customerId, "customer", "PAYMENT_PENDING", "payment", payment.Id);

        // 4. Simulate payment — generate random transaction ref
        var transactionRef = $"TXN-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        // 5. Update payment to SUCCESS
        await _paymentRepo.UpdatePaymentStatusAsync(payment.Id, "SUCCESS", transactionRef);
        payment.Status = "SUCCESS";
        payment.PaidAt = DateTime.UtcNow;
        payment.TransactionRef = transactionRef;

        // 6. Update booking to CONFIRMED
        await _bookingRepo.UpdateBookingStatusAsync(bookingId, "CONFIRMED");
        booking.Status = "CONFIRMED";

        // 7. Mark coupon used if applicable
        if (booking.CouponId.HasValue)
        {
            await _couponRepo.MarkCouponUsedAsync(booking.CouponId.Value);
        }

        await _auditRepo.LogAsync(customerId, "customer", "PAYMENT_SUCCESS", "payment", payment.Id);
        await _auditRepo.LogAsync(customerId, "customer", "CONFIRM_BOOKING", "booking", bookingId);

        // 8. Release all seat locks for this booking's seats BEFORE generating the ticket
        foreach (var bookedSeat in booking.BookedSeats)
        {
            if (bookedSeat.SeatId.HasValue && bookedSeat.BusId.HasValue)
            {
                var activeLocks = await _db.SeatLocks
                    .Where(sl =>
                        sl.SeatId == bookedSeat.SeatId &&
                        sl.BusId == bookedSeat.BusId &&
                        sl.JourneyDate == bookedSeat.JourneyDate &&
                        sl.IsActive)
                    .ToListAsync();

                foreach (var seatLock in activeLocks)
                {
                    await _seatRepo.ReleaseLockAsync(seatLock.Id);
                }
            }
        }

        // 9. Generate PDF ticket and enqueue confirmation email
        var pdfBytes = await _pdfService.GenerateTicketAsync(booking);
        await _emailService.EnqueueBookingConfirmationAsync(booking, pdfBytes);

        return MapToDto(booking);
    }

    public async Task<List<BookingDto>> GetCustomerBookingsAsync(Guid customerId)
    {
        var bookings = await _bookingRepo.GetBookingsByCustomerAsync(customerId);
        return bookings.Select(MapToDto).ToList();
    }

    public async Task<List<BookingDto>> GetOperatorBookingsAsync(Guid operatorId)
    {
        var op = await _db.BusOperators.FindAsync(operatorId);
        if (op is null || op.Status != "APPROVED")
        {
            throw new UnauthorizedAccessException("Operator account is not approved.");
        }

        var bookings = await _bookingRepo.GetBookingsByOperatorAsync(operatorId);
        return bookings.Select(MapToDto).ToList();
    }

    public async Task<BookingDto?> GetBookingByIdAsync(Guid bookingId, Guid customerId)
    {
        var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);
        if (booking == null) return null;
        if (booking.CustomerId != customerId) return null;
        return MapToDto(booking);
    }

    public async Task<byte[]> GenerateTicketPdfAsync(Guid bookingId, Guid customerId)
    {
        var booking = await _bookingRepo.GetBookingByIdAsync(bookingId)
            ?? throw new InvalidOperationException("Booking not found.");

        if (booking.CustomerId != customerId)
            throw new UnauthorizedAccessException("Access denied.");

        if (booking.Status != "CONFIRMED")
            throw new InvalidOperationException("Ticket is only available for confirmed bookings.");

        return await _pdfService.GenerateTicketAsync(booking);
    }

    // ── Mapping helper ────────────────────────────────────────────────────────

    private static BookingDto MapToDto(Booking booking)
    {
        // Get departure time from first BOARDING stop
        var departureTime = booking.Bus?.BusStops
            .Where(s => s.Type == "BOARDING" && s.ScheduledTime.HasValue)
            .Select(s => s.ScheduledTime!.Value)
            .FirstOrDefault();

        return new BookingDto
        {
            Id             = booking.Id,
            Pnr            = booking.Pnr ?? booking.Id.ToString("N")[..8].ToUpper(), // fallback for legacy rows
            BusId          = booking.BusId ?? Guid.Empty,
            JourneyDate    = booking.JourneyDate,
            DepartureTime  = departureTime,
            BaseFare       = booking.BaseFare,
            ConvenienceFee = booking.ConvenienceFee,
            TotalAmount    = booking.TotalAmount,
            Status         = booking.Status,
            BookedAt       = booking.BookedAt,
            Seats          = booking.BookedSeats.Select(bs => new DTOs.Booking.BookedSeatDto
            {
                SeatId          = bs.SeatId ?? Guid.Empty,
                PassengerName   = bs.PassengerName,
                PassengerAge    = bs.PassengerAge,
                PassengerGender = bs.PassengerGender
            }).ToList()
        };
    }
}
