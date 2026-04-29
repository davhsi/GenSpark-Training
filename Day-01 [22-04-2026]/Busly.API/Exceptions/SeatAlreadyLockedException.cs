namespace Busly.API.Exceptions;

public class SeatAlreadyLockedException : InvalidOperationException
{
    public SeatAlreadyLockedException() : base("Seat is already locked or booked")
    {
    }

    public SeatAlreadyLockedException(string message) : base(message)
    {
    }

    public SeatAlreadyLockedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
