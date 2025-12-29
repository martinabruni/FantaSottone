namespace Internal.FantaSottone.Domain.Dtos;

/// <summary>
/// Represents a game in the user's dashboard
/// </summary>
public sealed class GameListItemDto
{
    public int GameId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public string CreatorEmail { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public int CurrentScore { get; set; }
    public bool IsCreator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
