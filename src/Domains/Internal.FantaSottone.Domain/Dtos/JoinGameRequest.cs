namespace Internal.FantaSottone.Domain.Dtos;

/// <summary>
/// Request to join/activate a game for the current session
/// </summary>
public sealed class JoinGameRequest
{
    public int GameId { get; set; }
}
