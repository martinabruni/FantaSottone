namespace Internal.FantaSottone.Domain.Models;

public sealed class Game : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public int InitialScore { get; set; }
    public GameStatus Status { get; set; }
    public int? CreatorPlayerId { get; set; }
    public int? WinnerPlayerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}