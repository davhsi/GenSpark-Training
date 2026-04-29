using Busly.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Busly.API.Controllers;

[ApiController]
[Route("admin")]
[Authorize(Policy = "Admin")]
public class AdminSeatLockController : ControllerBase
{
    private readonly ISeatLockService _seatLockService;

    public AdminSeatLockController(ISeatLockService seatLockService)
    {
        _seatLockService = seatLockService;
    }

    // DELETE /admin/seats/lock/{id}
    [HttpDelete("seats/lock/{id:guid}")]
    public async Task<IActionResult> ForceReleaseSeatLock(Guid id)
    {
        try
        {
            var releasedLock = await _seatLockService.ForceReleaseLockAsync(id);
            
            if (releasedLock is null)
                return NotFound(new { message = "Lock not found or already expired" });

            return Ok(new { 
                message = "Seat lock force-released successfully",
                lockId = releasedLock.LockId,
                seatId = releasedLock.SeatId,
                releasedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to force release lock", error = ex.Message });
        }
    }
}
