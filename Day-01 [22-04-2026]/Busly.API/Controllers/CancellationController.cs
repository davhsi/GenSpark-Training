using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Busly.API.Repositories;
using Busly.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Busly.API.Controllers;

[ApiController]
[Route("")]
public class CancellationController : ControllerBase
{
    private readonly ICancellationService _cancellationService;
    private readonly ICancellationRepository _cancellationRepo;
    private readonly IBookingRepository _bookingRepo;

    public CancellationController(
        ICancellationService cancellationService,
        ICancellationRepository cancellationRepo,
        IBookingRepository bookingRepo)
    {
        _cancellationService = cancellationService;
        _cancellationRepo = cancellationRepo;
        _bookingRepo = bookingRepo;
    }

    // POST /bookings/{id}/cancel
    [HttpPost("bookings/{id:guid}/cancel")]
    [Authorize(Policy = "Customer")]
    public async Task<IActionResult> CancelBooking(Guid id)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            var result = await _cancellationService.CancelBookingAsync(id, customerId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Booking not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new { message = "Access denied" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /bookings/{id}/refund-complete
    [HttpPost("bookings/{id:guid}/refund-complete")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> CompleteRefund(Guid id)
    {
        var cancellation = await _cancellationRepo.GetCancellationByBookingIdAsync(id);
        if (cancellation is null)
            return NotFound(new { message = "Cancellation not found" });

        await _cancellationRepo.UpdateRefundStatusAsync(cancellation.Id, "PROCESSED");
        await _bookingRepo.UpdateBookingStatusAsync(id, "REFUNDED");

        return Ok(new { message = "Refund processed" });
    }
}
