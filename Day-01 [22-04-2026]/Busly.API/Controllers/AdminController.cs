using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Busly.API.Data;
using Busly.API.DTOs.Admin;
using Busly.API.Repositories;
using Busly.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Busly.API.Controllers;

[ApiController]
[Route("")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly AppDbContext _db;

    public AdminController(IAdminService adminService, AppDbContext db)
    {
        _adminService = adminService;
        _db = db;
    }

    // POST /admin/routes
    [HttpPost("admin/routes")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> CreateRoute([FromBody] CreateRouteRequest request)
    {
        var adminIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            var route = await _adminService.CreateRouteAsync(request, adminId);
            // If the route already existed (inactive) it was reactivated — return 200.
            // If it was newly created — return 201.
            // We distinguish by checking if the route existed before; service always returns the route.
            return StatusCode(201, route);
        }
        catch (InvalidOperationException)
        {
            return Conflict(new { message = "Route already exists and is active" });
        }
    }

    // GET /routes
    [HttpGet("routes")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveRoutes()
    {
        var routes = await _adminService.GetActiveRoutesAsync();
        return Ok(routes);
    }

    // GET /admin/routes  (all routes including inactive — admin only)
    [HttpGet("admin/routes")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetAllRoutes()
    {
        var routes = await _adminService.GetAllRoutesAsync();
        return Ok(routes);
    }

    // PATCH /admin/routes/{id}/toggle
    [HttpPatch("admin/routes/{id:guid}/toggle")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ToggleRoute(Guid id)
    {
        var adminIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized(new { message = "Invalid token" });

        await _adminService.ToggleRouteAsync(id, adminId);
        return Ok(new { message = "Route toggled" });
    }

    // GET /admin/operators/pending
    [HttpGet("admin/operators/pending")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetPendingOperators()
    {
        var operators = await _adminService.GetPendingOperatorsAsync();
        return Ok(operators);
    }

    // GET /admin/operators
    [HttpGet("admin/operators")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetAllOperators()
    {
        var operators = await _adminService.GetAllOperatorsAsync();
        return Ok(operators);
    }

    // PATCH /admin/operators/{id}/approve
    [HttpPatch("admin/operators/{id:guid}/approve")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ApproveOperator(Guid id)
    {
        var adminIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized(new { message = "Invalid token" });

        await _adminService.ApproveOperatorAsync(id, adminId);
        return Ok(new { message = "Operator approved" });
    }

    // PATCH /admin/operators/{id}/reject
    [HttpPatch("admin/operators/{id:guid}/reject")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> RejectOperator(Guid id)
    {
        var adminIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized(new { message = "Invalid token" });

        await _adminService.RejectOperatorAsync(id, adminId);
        return Ok(new { message = "Operator rejected" });
    }

    // PATCH /admin/operators/{id}/toggle
    [HttpPatch("admin/operators/{id:guid}/toggle")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ToggleOperator(Guid id)
    {
        var adminIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            await _adminService.ToggleOperatorAsync(id, adminId);
            return Ok(new { message = "Operator status toggled" });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "Operator not found" });
        }
    }

    // GET /admin/buses/pending
    [HttpGet("admin/buses/pending")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetPendingBuses()
    {
        var buses = await _adminService.GetPendingBusesAsync();
        return Ok(buses);
    }

    // GET /admin/buses
    [HttpGet("admin/buses")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetAllBuses()
    {
        var buses = await _adminService.GetAllBusesAsync();
        return Ok(buses);
    }

    // GET /admin/operators/{operatorId}/buses
    [HttpGet("admin/operators/{operatorId:guid}/buses")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetBusesByOperator(Guid operatorId)
    {
        var buses = await _adminService.GetBusesByOperatorAsync(operatorId);
        return Ok(buses);
    }

    // PATCH /admin/buses/{id}/approve
    [HttpPatch("admin/buses/{id:guid}/approve")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ApproveBus(Guid id)
    {
        var adminIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized(new { message = "Invalid token" });

        await _adminService.ApproveBusAsync(id, adminId);
        return Ok(new { message = "Bus approved" });
    }

    // PATCH /admin/buses/{id}/reject
    [HttpPatch("admin/buses/{id:guid}/reject")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> RejectBus(Guid id)
    {
        var adminIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized(new { message = "Invalid token" });

        await _adminService.RejectBusAsync(id, adminId);
        return Ok(new { message = "Bus rejected" });
    }

    // PATCH /admin/buses/{id}/toggle
    [HttpPatch("admin/buses/{id:guid}/toggle")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ToggleBus(Guid id)
    {
        var adminIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            await _adminService.ToggleBusStatusAsync(id, adminId);
            return Ok(new { message = "Bus status toggled" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /admin/revenue
    [HttpGet("admin/revenue")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetRevenue()
    {
        var result = await _adminService.GetMonthlyRevenueAsync();
        return Ok(result);
    }

    // GET /admin/revenue/by-operator
    [HttpGet("admin/revenue/by-operator")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetRevenueByOperator()
    {
        var result = await _adminService.GetOperatorRevenueAsync();
        return Ok(result);
    }

    // GET /admin/tc
    [HttpGet("admin/tc")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetAllTc()
    {
        var versions = await _adminService.GetAllTcVersionsAsync();
        return Ok(versions);
    }

    // GET /tc/current
    [HttpGet("tc/current")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCurrentTc()
    {
        var currentTc = await _adminService.GetCurrentTcAsync();
        if (currentTc == null)
            return NotFound(new { message = "No active T&C found" });
        
        return Ok(currentTc);
    }

    // POST /admin/tc
    [HttpPost("admin/tc")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> PublishTc([FromBody] CreateTcRequest request)
    {
        var adminIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized(new { message = "Invalid token" });

        await _adminService.PublishTcVersionAsync(request, adminId);
        return StatusCode(201, new { message = "T&C version published" });
    }

    // GET /admin/audit-logs
    [HttpGet("admin/audit-logs")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetAuditLogs()
    {
        var logs = await _adminService.GetAuditLogsAsync();
        return Ok(logs);
    }

    // GET /admin/config/convenience-fee
    [HttpGet("admin/config/convenience-fee")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetConvenienceFeeConfig([FromServices] IConfigService configService)
    {
        var feeType  = await configService.GetConfigAsync("convenience_fee_type",  "flat");
        var feeValue = await configService.GetConfigAsync("convenience_fee_value", "0");
        return Ok(new { feeType, feeValue = decimal.Parse(feeValue) });
    }

    // PATCH /admin/config/convenience-fee
    [HttpPatch("admin/config/convenience-fee")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> UpdateConvenienceFeeConfig(
        [FromBody] UpdateConvenienceFeeRequest request,
        [FromServices] IConfigService configService,
        [FromServices] IAuditRepository auditRepo)
    {
        var adminIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized(new { message = "Invalid token" });

        if (request.FeeType != "flat" && request.FeeType != "percent")
            return BadRequest(new { message = "feeType must be 'flat' or 'percent'" });

        if (request.FeeValue < 0)
            return BadRequest(new { message = "feeValue must be non-negative" });

        if (request.FeeType == "percent" && request.FeeValue > 100)
            return BadRequest(new { message = "Percentage fee cannot exceed 100" });

        await configService.SetConfigAsync("convenience_fee_type",  request.FeeType);
        await configService.SetConfigAsync("convenience_fee_value", request.FeeValue.ToString());

        await auditRepo.LogAsync(adminId, "admin", "UPDATE_CONVENIENCE_FEE", "platform_config", null,
            $"{{\"feeType\":\"{request.FeeType}\",\"feeValue\":{request.FeeValue}}}");

        return Ok(new { message = "Convenience fee updated", feeType = request.FeeType, feeValue = request.FeeValue });
    }

    // GET /admin/config/convenience-fee (public — for booking estimate)
    [HttpGet("config/convenience-fee")]
    [AllowAnonymous]
    public async Task<IActionResult> GetConvenienceFeePublic([FromServices] IConfigService configService)
    {
        var feeType  = await configService.GetConfigAsync("convenience_fee_type",  "flat");
        var feeValue = await configService.GetConfigAsync("convenience_fee_value", "0");
        return Ok(new { feeType, feeValue = decimal.Parse(feeValue) });
    }
}
