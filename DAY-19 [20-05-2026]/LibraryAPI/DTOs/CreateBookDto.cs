namespace LibraryAPI.DTOs;

public class CreateBookDto
{
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string? ISBN { get; set; }
    public int PublicationYear { get; set; }
    public int AvailableCopies { get; set; }
}
