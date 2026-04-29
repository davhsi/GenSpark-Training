using Busly.API.Models;

namespace Busly.API.Services;

public interface IPdfService
{
    Task<byte[]> GenerateTicketAsync(Booking booking);
}
