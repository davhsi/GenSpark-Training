namespace LibraryAPI.Models;

public class Member
{
    public int MemberId { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public DateTime JoinDate { get; set; } = DateTime.UtcNow;

}
