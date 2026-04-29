namespace Busly.API.DTOs.Admin;

public class OperatorDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Status { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? TcVersion { get; set; }
    public DateTime? TcAcceptedAt { get; set; }
}
