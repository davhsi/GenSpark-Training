namespace Busly.API.Helpers;

public static class CouponGenerator
{
    /// <summary>
    /// Generates a unique coupon code in the format BUSLY-XXXXXXXX
    /// where X is an uppercase alphanumeric character.
    /// </summary>
    public static string Generate()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpper();
        return $"BUSLY-{suffix}";
    }
}
