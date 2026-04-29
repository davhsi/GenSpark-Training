using Busly.API.Data;
using Busly.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Busly.API.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly AppDbContext _db;

    public CouponRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Coupon?> GetCouponByCodeAsync(string code)
    {
        return await _db.Coupons
            .FirstOrDefaultAsync(c => c.Code == code);
    }

    public async Task MarkCouponUsedAsync(Guid couponId)
    {
        var coupon = await _db.Coupons.FindAsync(couponId);
        if (coupon is null) return;

        coupon.IsUsed = true;
        await _db.SaveChangesAsync();
    }

    public async Task<Coupon> CreateCouponAsync(Coupon coupon)
    {
        _db.Coupons.Add(coupon);
        await _db.SaveChangesAsync();
        return coupon;
    }

    public async Task<Coupon?> GetCouponByIdAsync(Guid couponId)
    {
        return await _db.Coupons.FindAsync(couponId);
    }
}
