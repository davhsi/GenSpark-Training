namespace Busly.API.DTOs.Operator;

public class OperatorProfileDto
{
    public string Id { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public bool IsApproved => Status == "APPROVED";
    public bool IsPending => Status == "PENDING";
    public bool IsRejected => Status == "REJECTED";
    public bool IsDisabled => Status == "DISABLED";
}
