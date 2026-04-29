using Busly.API.Data;
using Busly.API.Models;
using Busly.API.Services;
using MailKit.Net.Smtp;
using MimeKit;

namespace Busly.API.BackgroundJobs;

public class EmailDispatchJob : IHostedService
{
    private readonly IEmailService _emailService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailDispatchJob> _logger;

    private Task? _executingTask;
    private CancellationTokenSource? _cts;

    public EmailDispatchJob(
        IEmailService emailService,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<EmailDispatchJob> logger)
    {
        _emailService = emailService;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executingTask = RunAsync(_cts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts is not null)
        {
            await _cts.CancelAsync();
        }

        if (_executingTask is not null)
        {
            try
            {
                await _executingTask;
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown — swallow gracefully
            }
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        var reader = _emailService.Reader;

        try
        {
            await foreach (var message in reader.ReadAllAsync(ct))
            {
                await ProcessMessageAsync(message, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown requested — exit loop gracefully
        }
    }

    private async Task ProcessMessageAsync(EmailMessage message, CancellationToken ct)
    {
        // Resolve SMTP configuration — env vars take precedence over appsettings
        var host =
            Environment.GetEnvironmentVariable("BUSLY_SMTP_HOST")
            ?? _configuration["Smtp:Host"]
            ?? string.Empty;

        var portStr = _configuration["Smtp:Port"];
        var port = int.TryParse(portStr, out var p) ? p : 587;

        var username =
            Environment.GetEnvironmentVariable("BUSLY_SMTP_USERNAME")
            ?? _configuration["Smtp:Username"]
            ?? string.Empty;

        var password =
            Environment.GetEnvironmentVariable("BUSLY_SMTP_PASSWORD")
            ?? _configuration["Smtp:Password"]
            ?? string.Empty;

        var fromEmail =
            _configuration["Smtp:FromEmail"]
            ?? string.Empty;

        string status;

        try
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("Busly", fromEmail));
            mimeMessage.To.Add(new MailboxAddress(string.Empty, message.To));
            mimeMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder { TextBody = message.Body };

            if (message.PdfAttachment != null)
            {
                bodyBuilder.Attachments.Add(
                    message.AttachmentFileName ?? "ticket.pdf",
                    message.PdfAttachment,
                    new ContentType("application", "pdf"));
            }

            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable, ct);
            await client.AuthenticateAsync(username, password, ct);
            await client.SendAsync(mimeMessage, ct);
            await client.DisconnectAsync(true, ct);

            _logger.LogInformation("Email sent to {To} with subject '{Subject}'", message.To, message.Subject);
            status = "SENT";
        }
        catch (OperationCanceledException)
        {
            throw; // propagate cancellation — do not write a notification row
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} with subject '{Subject}'", message.To, message.Subject);
            status = "FAILED";
        }

        await WriteNotificationAsync(message, status);
    }

    private async Task WriteNotificationAsync(EmailMessage message, string status)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                CustomerId = message.CustomerId,
                Type = "EMAIL",
                Message = message.Subject,
                Status = status,
                SentAt = DateTime.UtcNow
            };

            db.Notifications.Add(notification);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write notification row for email to {To}", message.To);
        }
    }
}
