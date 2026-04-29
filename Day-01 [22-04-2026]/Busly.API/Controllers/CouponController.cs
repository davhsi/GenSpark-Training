using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Busly.API.DTOs.Cancellation;
using Busly.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Busly.API.Controllers;

[ApiController]
[Route("")]
public class CouponController : ControllerBase
{
    private readonly ICouponRepository _couponRepo;

    public CouponController(ICouponRepository couponRepo)
    {
        _couponRepo = couponRepo;
    }

    // POST /bookings/apply-coupon
    [HttpPost("bookings/apply-coupon")]
    [Authorize(Policy = "Customer")]
    public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var customerId))
            return Unauthorized(new { message = "Invalid token" });

        var coupon = await _couponRepo.GetCouponByCodeAsync(request.CouponCode);

        if (coupon is null)
            return BadRequest(new { message = "Coupon not found" });

        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt.Value < DateTime.UtcNow)
            return BadRequest(new { message = "Coupon has expired" });

        if (coupon.IsUsed)
            return BadRequest(new { message = "Coupon has already been used" });

        if (coupon.IssuedToCustomer != customerId)
            return BadRequest(new { message = "Coupon is not issued to this customer" });

        return Ok(new
        {
            discountValue = coupon.DiscountValue,
            discountType = coupon.DiscountType,
            code = coupon.Code
        });
    }
}
