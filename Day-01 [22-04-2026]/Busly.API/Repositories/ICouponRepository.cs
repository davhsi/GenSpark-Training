using Busly.API.Models;

namespace Busly.API.Repositories;

public interface ICouponRepository
{
    Task<Coupon?> GetCouponByCodeAsync(string code);
    Task MarkCouponUsedAsync(Guid couponId);
    Task<Coupon> CreateCouponAsync(Coupon coupon);
    Task<Coupon?> GetCouponByIdAsync(Guid couponId);
}
