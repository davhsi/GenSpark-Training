using Busly.API.Models;
using System.Threading.Channels;

namespace Busly.API.Services;

public interface IEmailService
{
    Task EnqueueBookingConfirmationAsync(Booking booking, byte[]? pdfBytes = null);
    Task EnqueueCancellationAsync(Booking booking, decimal refundAmount, string cancelledBy);
    Task EnqueueCouponAsync(Guid customerId, string email, string couponCode, decimal amount);
    ChannelReader<EmailMessage> Reader { get; }
}
