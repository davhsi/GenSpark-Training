using System.Collections.Concurrent;
using System.Net;

namespace Busly.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly ConcurrentDictionary<string, List<DateTime>> _requests;
    private readonly TimeSpan _window = TimeSpan.FromMinutes(1);
    private readonly int _maxRequests = 30; // Max 30 requests per minute per IP

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _requests = new ConcurrentDictionary<string, List<DateTime>>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = GetClientIpAddress(context);
        
        // Skip rate limiting for health checks and admin endpoints
        if (context.Request.Path.StartsWithSegments("/health") || 
            context.Request.Path.StartsWithSegments("/admin"))
        {
            await _next(context);
            return;
        }

        var now = DateTime.UtcNow;
        var requests = _requests.GetOrAdd(ipAddress, _ => new List<DateTime>());
        
        // Clean old requests outside the window
        requests.RemoveAll(t => now - t > _window);
        
        if (requests.Count >= _maxRequests)
        {
            _logger.LogWarning("Rate limit exceeded for IP: {IpAddress}, Count: {Count}", ipAddress, requests.Count);
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            await context.Response.WriteAsync("Too many requests. Please try again later.");
            return;
        }

        requests.Add(now);
        
        await _next(context);
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded headers (common with load balancers)
        var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(ipAddress))
        {
            return ipAddress.Split(',')[0].Trim();
        }

        // Check for Real IP header
        ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(ipAddress))
        {
            return ipAddress;
        }

        // Fall back to remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
