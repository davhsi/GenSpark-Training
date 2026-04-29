using Busly.API.Models;
using Busly.API.DTOs.Admin;

namespace Busly.API.Repositories;

public interface IBookingRepository
{
    Task<Booking> CreateBookingAsync(Booking booking);
    Task CreateBookedSeatsAsync(List<BookedSeat> bookedSeats);
    Task<Booking?> GetExistingBookingAsync(Guid customerId, Guid seatId, DateOnly journeyDate);
    Task<List<Booking>> GetBookingsByCustomerAsync(Guid customerId);
    Task<List<Booking>> GetBookingsByOperatorAsync(Guid operatorId);
    Task<Booking?> GetBookingByIdAsync(Guid bookingId);
    Task<Booking?> GetBookingByPnrAsync(string pnr);
    Task UpdateBookingStatusAsync(Guid bookingId, string status);
    Task UpdateBookingCouponAsync(Guid bookingId, Guid couponId, decimal totalAmount);
    Task<List<Booking>> GetConfirmedFutureBookingsByBusAsync(Guid busId);
    Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync();
    Task<List<OperatorRevenueDto>> GetOperatorRevenueAsync();
    Task<List<Booking>> GetAllBookingsAsync();
    Task HandlePaymentTimeoutsAsync(TimeSpan timeout);
}
