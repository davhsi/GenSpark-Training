using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Busly.API.DTOs.Auth;
using Busly.API.Repositories;
using Busly.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Busly.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAuthRepository _authRepository;

    public AuthController(IAuthService authService, IAuthRepository authRepository)
    {
        _authService    = authService;
        _authRepository = authRepository;
    }

    // POST /auth/register/customer
    [HttpPost("register/customer")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerRequest request)
    {
        var existing = await _authRepository.GetCustomerByEmailAsync(request.Email);
        if (existing is not null)
            return Conflict(new { message = "Email already registered" });

        var customer = await _authService.RegisterCustomerAsync(request);
        return StatusCode(201, new { message = "Customer registered successfully", userId = customer.Id });
    }

    // POST /auth/register/operator
    [HttpPost("register/operator")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterOperator([FromBody] RegisterOperatorRequest request)
    {
        var existing = await _authRepository.GetOperatorByEmailAsync(request.Email);
        if (existing is not null)
            return Conflict(new { message = "Email already registered" });

        var busOperator = await _authService.RegisterOperatorAsync(request);
        return StatusCode(201, new { message = "Operator registered. Awaiting admin approval.", userId = busOperator.Id });
    }

    // POST /auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);

            // Store JWT in an HttpOnly cookie — never exposed to JavaScript
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure   = false, // set to true in production (HTTPS only)
                SameSite = SameSiteMode.Lax,
                Expires  = DateTimeOffset.UtcNow.AddMinutes(60)
            };
            Response.Cookies.Append("busly_token", response.Token, cookieOptions);

            // Return role and userId but NOT the token — client never needs to read it
            return Ok(new { role = response.Role, email = response.Email, userId = response.UserId });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }
    }

    // POST /auth/logout
    [HttpPost("logout")]
    [AllowAnonymous]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("busly_token");
        return Ok(new { message = "Logged out successfully" });
    }

    // POST /auth/accept-tc
    [HttpPost("accept-tc")]
    [Authorize]
    public async Task<IActionResult> AcceptTc()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized(new { message = "Invalid token" });

        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        await _authService.AcceptTcAsync(userId, role);
        return Ok(new { message = "T&C accepted successfully" });
    }

    // GET /auth/tc-status
    [HttpGet("tc-status")]
    [Authorize]
    public async Task<IActionResult> GetTcStatus()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized(new { message = "Invalid token" });

        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var tcStatus = await _authService.GetTcStatusAsync(userId, role);
        return Ok(tcStatus);
    }

    // GET /auth/me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized(new { message = "Invalid token" });

        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var profile = await _authService.GetProfileAsync(userId, role);
        return Ok(profile);
    }
}
