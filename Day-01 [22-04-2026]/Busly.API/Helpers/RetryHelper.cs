using Busly.API.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Busly.API.Helpers;

public static class RetryHelper
{
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        int[]? delaysMs = null)
    {
        delaysMs ??= new[] { 50, 100, 200 }; // Default delays: 50ms, 100ms, 200ms
        
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && 
                                              (pgEx.SqlState == "40P01" || // deadlock
                                               pgEx.SqlState == "23505")) // unique violation
            {
                if (attempt == maxRetries - 1) 
                {
                    // Last attempt, rethrow the exception
                    if (pgEx.SqlState == "23505")
                        throw new SeatAlreadyLockedException("Seat is already locked for this journey date");
                    else
                        throw;
                }
                
                // Wait before retrying
                await Task.Delay(delaysMs[Math.Min(attempt, delaysMs.Length - 1)]);
                continue;
            }
        }
        
        throw new InvalidOperationException("Maximum retry attempts exceeded");
    }
}
