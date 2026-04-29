using Busly.API.DTOs.PNR;
using Busly.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Busly.API.Controllers;

[ApiController]
[Route("")]
public class PnrController : ControllerBase
{
    private readonly IPnrService _pnrService;
    private readonly ICaptchaService _captchaService;

    public PnrController(IPnrService pnrService, ICaptchaService captchaService)
    {
        _pnrService = pnrService;
        _captchaService = captchaService;
    }

    // GET /captcha
    [HttpGet("captcha")]
    [AllowAnonymous]
    public IActionResult GetCaptcha()
    {
        var captchaToken = _captchaService.GenerateCaptcha();
        var parts = captchaToken.Split(':', 2);
        
        return Ok(new 
        { 
            sessionId = parts[0],
            captchaText = parts[1],
            message = "Please enter this captcha text to verify you are human"
        });
    }

    // POST /pnr/lookup
    [HttpPost("pnr/lookup")]
    [AllowAnonymous]
    public async Task<IActionResult> LookupPnr([FromBody] PnrLookupRequest request)
    {
        Console.WriteLine($"PNR Lookup Request: Pnr={request.Pnr}, CaptchaToken={request.CaptchaToken}, CaptchaInput={request.CaptchaInput}");
        
        if (string.IsNullOrWhiteSpace(request.Pnr))
            return BadRequest(new { message = "PNR is required" });

        if (request.Pnr.Length != 8)
            return BadRequest(new { message = "PNR must be 8 characters long" });

        if (string.IsNullOrWhiteSpace(request.CaptchaToken))
            return BadRequest(new { message = "Captcha verification is required" });

        if (string.IsNullOrWhiteSpace(request.CaptchaInput))
            return BadRequest(new { message = "Captcha input is required" });

        // Validate captcha
        var isValidCaptcha = _captchaService.ValidateCaptcha(request.CaptchaInput, request.CaptchaToken);
        Console.WriteLine($"Captcha validation result: {isValidCaptcha}");
        if (!isValidCaptcha)
            return BadRequest(new { message = "Invalid or expired captcha. Please try again." });

        try
        {
            var result = await _pnrService.GetBookingByPnrAsync(request.Pnr);
            
            if (result == null)
                return NotFound(new { message = "PNR not found. Please check your PNR and try again." });

            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while looking up your PNR. Please try again later." });
        }
    }
}
