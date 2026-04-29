using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Busly.API.DTOs.Booking;
using Busly.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Busly.API.Controllers;

[ApiController]
[Route("bookings")]
[Authorize(Policy = "Customer")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    // POST /bookings
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            var booking = await _bookingService.CreateBookingAsync(request, customerId);
            return StatusCode(201, booking);
        }
        catch (InvalidOperationException ex) when (ex.Message == "TC_REACCEPTANCE_REQUIRED")
        {
            return StatusCode(403, new { code = "TC_REACCEPTANCE_REQUIRED" });
        }
        catch (InvalidOperationException ex) when (ex.Message == "Invalid coupon")
        {
            return BadRequest(new { message = "Invalid coupon" });
        }
    }

    // POST /bookings/{id}/pay
    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> ProcessPayment(Guid id)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            var booking = await _bookingService.ProcessPaymentAsync(id, customerId);
            return Ok(booking);
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /bookings/mine
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyBookings()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
            return Unauthorized(new { message = "Invalid token" });

        var bookings = await _bookingService.GetCustomerBookingsAsync(customerId);
        return Ok(bookings);
    }

    // GET /bookings/{id}/ticket
    [HttpGet("{id:guid}/ticket")]
    public async Task<IActionResult> DownloadTicket(Guid id)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            var bytes = await _bookingService.GenerateTicketPdfAsync(id, customerId);
            return File(bytes, "application/pdf", $"ticket-{id}.pdf");
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
