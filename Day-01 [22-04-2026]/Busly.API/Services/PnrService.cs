using Busly.API.DTOs.PNR;
using Busly.API.Models;
using Busly.API.Repositories;

namespace Busly.API.Services;

public interface IPnrService
{
    Task<PnrLookupResponse?> GetBookingByPnrAsync(string pnr);
}

public class PnrService : IPnrService
{
    private readonly IBookingRepository _bookingRepo;

    public PnrService(IBookingRepository bookingRepo)
    {
        _bookingRepo = bookingRepo;
    }

    public async Task<PnrLookupResponse?> GetBookingByPnrAsync(string pnr)
    {
        if (string.IsNullOrWhiteSpace(pnr) || pnr.Length != 8)
            return null;

        // O(1) indexed lookup on the stored pnr column
        var booking = await _bookingRepo.GetBookingByPnrAsync(pnr);

        if (booking == null)
            return null;

        var canCancel = CanCancelBooking(booking);

        return new PnrLookupResponse
        {
            Pnr              = pnr.ToUpper(),
            Status           = booking.Status ?? string.Empty,
            CustomerName     = "Private Information",
            CustomerEmail    = "Private Information",
            JourneyDate      = booking.JourneyDate,
            FromCity         = booking.Bus?.Route?.SourceCity ?? string.Empty,
            ToCity           = booking.Bus?.Route?.DestinationCity ?? string.Empty,
            BusNumber        = booking.Bus?.BusNumber ?? string.Empty,
            DepartureTime    = booking.Bus?.BusStops
                                   .FirstOrDefault(s => s.Type == "BOARDING")?.ScheduledTime ?? TimeOnly.MinValue,
            ArrivalTime      = booking.Bus?.BusStops
                                   .FirstOrDefault(s => s.Type == "DROPPING")?.ScheduledTime ?? TimeOnly.MinValue,
            SeatNumbers      = booking.BookedSeats?.Select(bs => bs.Seat?.SeatNumber?.ToString() ?? "").ToList()
                               ?? new List<string>(),
            TotalAmount      = booking.TotalAmount ?? 0m,
            BookedAt         = booking.BookedAt ?? DateTime.MinValue,
            CanCancel        = canCancel,
            CancellationReason = booking.Cancellations.FirstOrDefault()?.CancelledBy,
            RefundAmount     = booking.Cancellations.FirstOrDefault()?.RefundAmount,
            RefundStatus     = booking.Cancellations.FirstOrDefault()?.RefundStatus
        };
    }

    private static bool CanCancelBooking(Booking booking)
    {
        if (booking.Status != "CONFIRMED")
            return false;

        if (booking.JourneyDate < DateOnly.FromDateTime(DateTime.UtcNow))
            return false;

        var departureTime = booking.Bus?.BusStops
            .Where(s => s.Type == "BOARDING" && s.ScheduledTime.HasValue)
            .Select(s => s.ScheduledTime!.Value)
            .FirstOrDefault();

        if (departureTime.HasValue)
        {
            var journeyDateTime = booking.JourneyDate.ToDateTime(departureTime.Value);
            if (journeyDateTime < DateTime.UtcNow.AddHours(12))
                return false;
        }

        return true;
    }
}
