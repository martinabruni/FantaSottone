namespace Internal.FantaSottone.Domain.Dtos;

/// <summary>
/// Request to create a new game and invite players by email
/// </summary>
public sealed class CreateGameRequest
{
    /// <summary>
    /// Name of the game
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Initial score for all players
    /// </summary>
    public int InitialScore { get; set; }

    /// <summary>
    /// List of email addresses to invite
    /// </summary>
    public List<string> InvitedEmails { get; set; } = [];
}

/// <summary>
/// Response after creating a game
/// </summary>
public sealed class CreateGameResponse
{
    public int GameId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public int CreatorPlayerId { get; set; }
    public List<string> InvitedEmails { get; set; } = [];
    public List<string> InvalidEmails { get; set; } = [];
}
