using Internal.FantaSottone.Domain.Models;

namespace Internal.FantaSottone.Domain.Dtos;

/// <summary>
/// DTO for a game invitation
/// </summary>
public sealed class GameInvitationDto
{
    public int GameId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public int InitialScore { get; set; }
    public GameStatus Status { get; set; }
    public int PlayerCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Response containing user's game invitations
/// </summary>
public sealed class GetUserGamesResponse
{
    public IEnumerable<GameInvitationDto> Games { get; set; } = [];
}
