using Busly.API.Models;
using System.Threading.Channels;

namespace Busly.API.Services;

public class EmailService : IEmailService
{
    private readonly Channel<EmailMessage> _channel;

    public EmailService()
    {
        _channel = Channel.CreateUnbounded<EmailMessage>();
    }

    public ChannelReader<EmailMessage> Reader => _channel.Reader;

    public async Task EnqueueBookingConfirmationAsync(Booking booking, byte[]? pdfBytes = null)
    {
        var customerEmail = booking.Customer?.Email ?? string.Empty;
        var pnr = booking.Id.ToString("N")[..8].ToUpper();

        var body = $"""
            Dear {booking.Customer?.Name ?? "Customer"},

            Your booking has been confirmed!

            PNR: {pnr}
            Journey Date: {booking.JourneyDate}
            Total Amount Paid: {booking.TotalAmount:C}
            Status: {booking.Status}

            Thank you for choosing Busly!
            """;

        var message = new EmailMessage
        {
            To = customerEmail,
            Subject = $"Booking Confirmed - PNR: {pnr}",
            Body = body,
            CustomerId = booking.CustomerId,
            PdfAttachment = pdfBytes,
            AttachmentFileName = $"ticket-{pnr}.pdf"
        };

        await _channel.Writer.WriteAsync(message);
    }
 
    public async Task EnqueueCancellationAsync(Booking booking, decimal refundAmount, string cancelledBy)
    {
        var customerEmail = booking.Customer?.Email ?? string.Empty;
        var pnr = booking.Id.ToString("N")[..8].ToUpper();
        
        var reason = cancelledBy == "operator" 
            ? "due to service suspension by the operator" 
            : "as per your request";

        var body = $"""
            Dear {booking.Customer?.Name ?? "Customer"},
            
            We regret to inform you that your booking has been CANCELLED {reason}.
            
            PNR: {pnr}
            Journey Date: {booking.JourneyDate}
            Refund Amount: {refundAmount:C}
            Status: CANCELLED
            
            {(cancelledBy == "operator" ? "Since the operator cancelled this service, a separate email with a compensation coupon has been sent to you." : "")}
            
            Thank you for choosing Busly!
            """;

        var message = new EmailMessage
        {
            To = customerEmail,
            Subject = $"Booking Cancelled - PNR: {pnr}",
            Body = body,
            CustomerId = booking.CustomerId
        };

        await _channel.Writer.WriteAsync(message);
    }

    public async Task EnqueueCouponAsync(Guid customerId, string email, string couponCode, decimal amount)
    {
        var body = $"""
            Dear Customer,
            
            As compensation for the operator-cancelled service, we have issued a discount coupon for your next journey!
            
            Coupon Code: {couponCode}
            Value: {amount:C}
            Validity: 30 Days
            
            You can apply this code during your next booking checkout.
            
            Thank you for your patience.
            """;

        var message = new EmailMessage
        {
            To = email,
            Subject = "Compensation Coupon Issued - Busly",
            Body = body,
            CustomerId = customerId
        };

        await _channel.Writer.WriteAsync(message);
    }
}
