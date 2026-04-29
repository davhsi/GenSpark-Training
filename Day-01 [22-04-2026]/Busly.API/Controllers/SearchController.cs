using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Busly.API.DTOs.Search;
using Busly.API.Services;
using Busly.API.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Busly.API.Controllers;

[ApiController]
[Route("")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly ISeatLockService _seatLockService;

    public SearchController(ISearchService searchService, ISeatLockService seatLockService)
    {
        _searchService = searchService;
        _seatLockService = seatLockService;
    }

    // GET /buses/search
    [AllowAnonymous]
    [HttpGet("buses/search")]
    public async Task<IActionResult> SearchBuses([FromQuery] string from, [FromQuery] string to, [FromQuery] string date)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd." });

        var results = await _searchService.SearchBusesAsync(from, to, parsedDate);
        return Ok(results);
    }

    // GET /buses/{id}/seats
    [AllowAnonymous]
    [HttpGet("buses/{id:guid}/seats")]
    public async Task<IActionResult> GetSeatMap(Guid id, [FromQuery] string date)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd." });

        var seatMap = await _searchService.GetSeatMapAsync(id, parsedDate);
        return Ok(seatMap);
    }

    // GET /buses/{id}
    [AllowAnonymous]
    [HttpGet("buses/{id:guid}")]
    public async Task<IActionResult> GetBusDetails(Guid id)
    {
        try
        {
            var bus = await _searchService.GetBusDetailsAsync(id);
            return Ok(bus);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Bus not found" });
        }
    }

    // GET /cities/autocomplete
    [AllowAnonymous]
    [HttpGet("cities/autocomplete")]
    public async Task<IActionResult> GetCitySuggestions([FromQuery] string? q)
    {
        if (string.IsNullOrEmpty(q))
            return Ok(Array.Empty<string>());

        var suggestions = await _searchService.GetCitySuggestionsAsync(q);
        return Ok(suggestions);
    }

    // POST /seats/lock
    [Authorize(Policy = "Customer")]
    [HttpPost("seats/lock")]
    public async Task<IActionResult> CreateSeatLock([FromBody] CreateSeatLockRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
            return Unauthorized(new { message = "Invalid token" });

        // Check if user already has maximum seats locked
        const int maxSeats = 4;
        var activeLocks = await _seatLockService.GetCustomerActiveLocksAsync(customerId);
        if (activeLocks.Count >= maxSeats)
            return BadRequest(new { message = $"Maximum {maxSeats} seats allowed per booking" });

        try
        {
            var lockDto = await _seatLockService.CreateLockAsync(request, customerId);
            return StatusCode(201, lockDto);
        }
        catch (SeatAlreadyLockedException)
        {
            return Conflict(new { message = "Seat is already locked or booked" });
        }
        catch (InvalidOperationException)
        {
            return Conflict(new { message = "Seat is already locked or booked" });
        }
    }

    // POST /seats/lock/bulk
    [Authorize(Policy = "Customer")]
    [HttpPost("seats/lock/bulk")]
    public async Task<IActionResult> CreateBulkSeatLocks([FromBody] BulkSeatLockRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
            return Unauthorized(new { message = "Invalid token" });

        // Validate maximum seats limit (1-4 seats)
        const int maxSeats = 4;
        if (request.SeatIds == null || request.SeatIds.Count == 0)
            return BadRequest(new { message = "At least one seat must be selected" });
        
        if (request.SeatIds.Count > maxSeats)
            return BadRequest(new { message = $"Maximum {maxSeats} seats allowed per booking" });

        try
        {
            var response = await _seatLockService.CreateBulkLocksAsync(request, customerId);
            
            if (response.AllSuccessful)
                return StatusCode(201, response);
            else
                return Ok(response); // Partial success - return 200 with details
        }
        catch (SeatAlreadyLockedException)
        {
            return Conflict(new { message = "One or more seats are already locked or booked" });
        }
    }

    // PUT /seats/lock/{id}/extend
    [Authorize(Policy = "Customer")]
    [HttpPut("seats/lock/{id:guid}/extend")]
    public async Task<IActionResult> ExtendSeatLock(Guid id)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            var extendedLock = await _seatLockService.ExtendLockAsync(id, customerId);
            
            if (extendedLock is null)
                return NotFound(new { message = "Lock not found or expired" });

            return Ok(extendedLock);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Failed to extend lock" });
        }
    }

    // GET /seats/lock/my-locks
    [Authorize(Policy = "Customer")]
    [HttpGet("seats/lock/my-locks")]
    public async Task<IActionResult> GetMyActiveLocks()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            var activeLocks = await _seatLockService.GetCustomerActiveLocksAsync(customerId);
            return Ok(activeLocks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to retrieve active locks", error = ex.Message });
        }
    }

    // DELETE /seats/lock/{id}
    [Authorize(Policy = "Customer")]
    [HttpDelete("seats/lock/{id:guid}")]
    public async Task<IActionResult> ReleaseSeatLock(Guid id)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            await _seatLockService.ReleaseLockAsync(id, customerId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403);
        }
    }

    // DELETE /seats/lock/by-seat/{seatId}
    [Authorize(Policy = "Customer")]
    [HttpDelete("seats/lock/by-seat/{seatId:guid}")]
    public async Task<IActionResult> ReleaseSeatLockBySeatId(Guid seatId)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            var activeLocks = await _seatLockService.GetCustomerActiveLocksAsync(customerId);
            var lockForSeat = activeLocks.FirstOrDefault(l => l.SeatId == seatId);
            
            if (lockForSeat == null)
                return NotFound();

            await _seatLockService.ReleaseLockAsync(lockForSeat.LockId, customerId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403);
        }
    }
}
