namespace Busly.API.DTOs.Operator;

public class SeatConfigDto
{
    public int Rows { get; set; }
    public int Cols { get; set; }
    public List<string> Decks { get; set; } = new();
    public List<SeatItemDto> Seats { get; set; } = new();
}

public class SeatItemDto
{
    public int SeatNumber { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public string Type { get; set; } = null!;   // "window" or "aisle"
    public string Deck { get; set; } = null!;   // "lower" or "upper"
}
