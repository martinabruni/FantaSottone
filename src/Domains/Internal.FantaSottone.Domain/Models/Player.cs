namespace Internal.FantaSottone.Domain.Models;

public sealed class Player : BaseModel
{
    public int GameId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
    public bool IsCreator { get; set; }
    public int CurrentScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}