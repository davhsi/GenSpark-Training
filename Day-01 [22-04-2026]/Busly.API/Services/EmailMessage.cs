namespace Busly.API.Services;

public class EmailMessage
{
    public string To { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    public byte[]? PdfAttachment { get; set; }
    public string? AttachmentFileName { get; set; }
    public Guid? CustomerId { get; set; }
}
