using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Busly.API.DTOs.Operator;
using Busly.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Busly.API.Controllers;

[ApiController]
[Route("operator")]
[Authorize(Policy = "Operator")]
public class OperatorController : ControllerBase
{
    private readonly IOperatorService _operatorService;
    private readonly IBookingService _bookingService;

    public OperatorController(IOperatorService operatorService, IBookingService bookingService)
    {
        _operatorService = operatorService;
        _bookingService  = bookingService;
    }

    private bool TryGetOperatorId(out Guid operatorId)
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out operatorId);
    }

    // POST /operator/layouts
    [HttpPost("layouts")]
    public async Task<IActionResult> CreateLayout([FromBody] CreateLayoutRequest request)
    {
        if (!TryGetOperatorId(out var operatorId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            var layout = await _operatorService.CreateLayoutAsync(request, operatorId);
            return StatusCode(201, layout);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    // GET /operator/layouts
    [HttpGet("layouts")]
    public async Task<IActionResult> GetLayouts()
    {
        if (!TryGetOperatorId(out var operatorId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            var layouts = await _operatorService.GetLayoutsAsync(operatorId);
            return Ok(layouts);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    // POST /operator/buses
    [HttpPost("buses")]
    public async Task<IActionResult> RegisterBus([FromBody] RegisterBusRequest request)
    {
        if (!TryGetOperatorId(out var operatorId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            var bus = await _operatorService.RegisterBusAsync(request, operatorId);
            return StatusCode(201, bus);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    // POST /operator/buses/{id}/boarding-points
    [HttpPost("buses/{id:guid}/boarding-points")]
    public async Task<IActionResult> AddBoardingPoint(Guid id, [FromBody] AddBusStopRequest request)
    {
        if (!TryGetOperatorId(out var operatorId))
            return Unauthorized(new { message = "Invalid token" });

        request.Type = "BOARDING";
        try
        {
            await _operatorService.AddBusStopAsync(id, request, operatorId);
            return StatusCode(201, new { message = "Boarding point added" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    // POST /operator/buses/{id}/dropping-points
    [HttpPost("buses/{id:guid}/dropping-points")]
    public async Task<IActionResult> AddDroppingPoint(Guid id, [FromBody] AddBusStopRequest request)
    {
        if (!TryGetOperatorId(out var operatorId))
            return Unauthorized(new { message = "Invalid token" });

        request.Type = "DROPPING";
        try
        {
            await _operatorService.AddBusStopAsync(id, request, operatorId);
            return StatusCode(201, new { message = "Dropping point added" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    // PATCH /operator/buses/{id}/price
    [HttpPatch("buses/{id:guid}/price")]
    public async Task<IActionResult> UpdateBusPrice(Guid id, [FromBody] UpdatePriceRequest request)
    {
        if (!TryGetOperatorId(out var operatorId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            await _operatorService.UpdateBusPriceAsync(id, request, operatorId);
            return Ok(new { message = "Price updated" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    // PATCH /operator/buses/{id}/staff
    [HttpPatch("buses/{id:guid}/staff")]
    public async Task<IActionResult> UpdateBusStaff(Guid id, [FromBody] UpdateStaffRequest request)
    {
        if (!TryGetOperatorId(out var operatorId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            await _operatorService.UpdateBusStaffAsync(id, request, operatorId);
            return Ok(new { message = "Staff updated" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    // PATCH /operator/buses/{id}/disable
    [HttpPatch("buses/{id:guid}/disable")]
    public async Task<IActionResult> DisableBus(Guid id)
    {
        if (!TryGetOperatorId(out var operatorId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            await _operatorService.DisableBusAsync(id, operatorId);
            return Ok(new { message = "Bus disabled" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    // DELETE /operator/buses/{id}
    [HttpDelete("buses/{id:guid}")]
    public async Task<IActionResult> RemoveBus(Guid id)
    {
        if (!TryGetOperatorId(out var operatorId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            await _operatorService.RemoveBusAsync(id, operatorId);
            return Ok(new { message = "Bus removed" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    // GET /operator/buses
    [HttpGet("buses")]
    public async Task<IActionResult> GetBuses()
    {
        if (!TryGetOperatorId(out var operatorId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            var buses = await _operatorService.GetBusesAsync(operatorId);
            return Ok(buses);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    // GET /operator/buses/{id}
    [HttpGet("buses/{id:guid}")]
    public async Task<IActionResult> GetBus(Guid id)
    {
        if (!TryGetOperatorId(out var operatorId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            var buses = await _operatorService.GetBusesAsync(operatorId);
            var bus = buses.FirstOrDefault(b => b.Id == id);

            if (bus is null) return NotFound(new { message = "Bus not found" });
            return Ok(bus);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpGet("bookings")]
    public async Task<IActionResult> GetBookings()
    {
        if (!TryGetOperatorId(out var operatorId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            var bookings = await _bookingService.GetOperatorBookingsAsync(operatorId);
            return Ok(bookings);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    // DELETE /operator/buses/stops/{id}
    [HttpDelete("buses/stops/{id:guid}")]
    public async Task<IActionResult> RemoveBusStop(Guid id)
    {
        if (!TryGetOperatorId(out var operatorId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            await _operatorService.RemoveBusStopAsync(id, operatorId);
            return Ok(new { message = "Stop removed" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    // PUT /operator/buses/{busId}/operating-days
    [HttpPut("buses/{busId:guid}/operating-days")]
    public async Task<IActionResult> UpdateOperatingDays(Guid busId, [FromBody] UpdateOperatingDaysRequest request)
    {
        if (!TryGetOperatorId(out var operatorId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            request.BusId = busId;
            await _operatorService.UpdateOperatingDaysAsync(request, operatorId);
            return Ok(new { message = "Operating days updated successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    // DELETE /operator/layouts/{id}
    [HttpDelete("layouts/{id:guid}")]
    public async Task<IActionResult> RemoveLayout(Guid id)
    {
        if (!TryGetOperatorId(out var operatorId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            await _operatorService.RemoveLayoutAsync(id, operatorId);
            return Ok(new { message = "Layout removed" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // GET /operator/profile
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        if (!TryGetOperatorId(out var operatorId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            var profile = await _operatorService.GetProfileWithoutApprovalCheckAsync(operatorId);
            return Ok(profile);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }
}
