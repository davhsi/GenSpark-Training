using Busly.API.Data;
using Busly.API.Models;
using Busly.API.DTOs.Admin;
using Busly.API.Services;
using Microsoft.EntityFrameworkCore;

namespace Busly.API.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _db;
    private readonly ISeatLockService _seatLockService;

    public BookingRepository(AppDbContext db, ISeatLockService seatLockService)
    {
        _db = db;
        _seatLockService = seatLockService;
    }

    public async Task<Booking> CreateBookingAsync(Booking booking)
    {
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();
        return booking;
    }

    public async Task CreateBookedSeatsAsync(List<BookedSeat> bookedSeats)
    {
        _db.BookedSeats.AddRange(bookedSeats);
        await _db.SaveChangesAsync();
    }

    public async Task<Booking?> GetExistingBookingAsync(Guid customerId, Guid seatId, DateOnly journeyDate)
    {
        return await _db.BookedSeats
            .Where(bs =>
                bs.SeatId == seatId &&
                bs.JourneyDate == journeyDate &&
                bs.Booking != null &&
                bs.Booking.CustomerId == customerId &&
                (bs.Booking.Status == "CONFIRMED" || bs.Booking.Status == "PAYMENT_PENDING"))
            .Select(bs => bs.Booking)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Booking>> GetBookingsByCustomerAsync(Guid customerId)
    {
        return await _db.Bookings
            .Where(b => b.CustomerId == customerId)
            .Include(b => b.BookedSeats)
                .ThenInclude(bs => bs.Seat) // Include seat details
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetBookingsByOperatorAsync(Guid operatorId)
    {
        return await _db.Bookings
            .Where(b => b.Bus != null && b.Bus.OperatorId == operatorId)
            .Include(b => b.BookedSeats)
                .ThenInclude(bs => bs.Seat) // Include seat details
            .Include(b => b.Customer)
            .Include(b => b.Bus)
                .ThenInclude(bus => bus!.Operator)
            .Include(b => b.Bus)
                .ThenInclude(bus => bus!.Route)
            .Include(b => b.Bus)
                .ThenInclude(bus => bus!.BusStops)
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync();
    }

    public async Task<Booking?> GetBookingByIdAsync(Guid bookingId)
    {
        return await _db.Bookings
            .Include(b => b.BookedSeats)
                .ThenInclude(bs => bs.Seat) // Include seat details for PDF
            .Include(b => b.Bus)
                .ThenInclude(bus => bus!.Operator)
            .Include(b => b.Bus)
                .ThenInclude(bus => bus!.Route)
            .Include(b => b.Bus)
                .ThenInclude(bus => bus!.BusStops)
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
    }

    public async Task<Booking?> GetBookingByPnrAsync(string pnr)
    {
        return await _db.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Bus)
                .ThenInclude(bus => bus!.Route)
            .Include(b => b.Bus)
                .ThenInclude(bus => bus!.BusStops)
            .Include(b => b.BookedSeats)
                .ThenInclude(bs => bs.Seat)
            .Include(b => b.Cancellations)
            .FirstOrDefaultAsync(b => b.Pnr == pnr.ToUpper());
    }

    public async Task UpdateBookingStatusAsync(Guid bookingId, string status)
    {
        var booking = await _db.Bookings.FindAsync(bookingId);
        if (booking is null) return;

        booking.Status = status;
        await _db.SaveChangesAsync();
    }

    public async Task UpdateBookingCouponAsync(Guid bookingId, Guid couponId, decimal totalAmount)
    {
        var booking = await _db.Bookings.FindAsync(bookingId);
        if (booking is null) return;

        booking.CouponId = couponId;
        booking.TotalAmount = totalAmount;
        await _db.SaveChangesAsync();
    }

    public async Task<List<Booking>> GetConfirmedFutureBookingsByBusAsync(Guid busId)
    {
        return await _db.Bookings
            .Where(b => b.BusId == busId
                     && b.Status == "CONFIRMED"
                     && b.JourneyDate >= DateOnly.FromDateTime(DateTime.UtcNow))
            .Include(b => b.Customer)
            .ToListAsync();
    }

    public async Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync()
    {
        return await _db.Bookings
            .Where(b => b.Status == "CONFIRMED" && b.BookedAt.HasValue)
            .GroupBy(b => new { b.BookedAt!.Value.Year, b.BookedAt!.Value.Month })
            .Select(g => new MonthlyRevenueDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalConvenienceFee = g.Sum(b => b.ConvenienceFee ?? 0)
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync();
    }

    public async Task<List<OperatorRevenueDto>> GetOperatorRevenueAsync()
    {
        return await _db.Bookings
            .Include(b => b.Bus!.Operator)
            .Where(b => b.Status == "CONFIRMED")
            .GroupBy(b => b.Bus!.Operator!.CompanyName)
            .Select(g => new OperatorRevenueDto
            {
                OperatorName = g.Key,
                BookingCount = g.Count(),
                TotalBaseFare = g.Sum(b => b.BaseFare ?? 0),
                TotalConvenienceFee = g.Sum(b => b.ConvenienceFee ?? 0)
            })
            .ToListAsync();
    }

    public async Task<List<Booking>> GetAllBookingsAsync()
    {
        return await _db.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Bus)
            .ThenInclude(bus => bus!.Route)
            .Include(b => b.Bus)
            .ThenInclude(bus => bus!.BusStops)
            .Include(b => b.BookedSeats)
            .ThenInclude(bs => bs.Seat)
            .Include(b => b.Cancellations)
            .ToListAsync();
    }

    public async Task HandlePaymentTimeoutsAsync(TimeSpan timeout)
    {
        var cutoff = DateTime.UtcNow.Subtract(timeout);

        // Find bookings in PAYMENT_PENDING state that were created before the cutoff
        var timedOutBookings = await _db.Bookings
            .Include(b => b.Payments)
            .Where(b => b.Status == "PAYMENT_PENDING" && b.BookedAt < cutoff)
            .ToListAsync();

        if (!timedOutBookings.Any()) return;

        foreach (var booking in timedOutBookings)
        {
            // Revert Booking status
            booking.Status = "INITIATED";

            // Revert Payment status
            foreach (var payment in booking.Payments.Where(p => p.Status == "PENDING"))
            {
                payment.Status = "FAILED";
            }

            // Immediately release associated seat locks for this customer/trip
            if (booking.CustomerId.HasValue && booking.BusId.HasValue)
            {
                await _seatLockService.ReleaseLocksByCustomerJourneyAsync(
                    booking.CustomerId.Value, 
                    booking.BusId.Value, 
                    booking.JourneyDate);
            }
        }

        await _db.SaveChangesAsync();
    }
}
