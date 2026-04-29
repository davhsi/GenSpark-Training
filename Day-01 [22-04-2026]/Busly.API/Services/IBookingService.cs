using Busly.API.DTOs.Booking;

namespace Busly.API.Services;

public interface IBookingService
{
    Task<BookingDto> CreateBookingAsync(CreateBookingRequest request, Guid customerId);
    Task<BookingDto> ProcessPaymentAsync(Guid bookingId, Guid customerId);
    Task<List<BookingDto>> GetCustomerBookingsAsync(Guid customerId);
    Task<List<BookingDto>> GetOperatorBookingsAsync(Guid operatorId);
    Task<BookingDto?> GetBookingByIdAsync(Guid bookingId, Guid customerId);
    Task<byte[]> GenerateTicketPdfAsync(Guid bookingId, Guid customerId);
}
