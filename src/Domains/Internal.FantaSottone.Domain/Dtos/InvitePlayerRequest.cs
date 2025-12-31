namespace Internal.FantaSottone.Domain.Dtos;

/// <summary>
/// Request to invite a user to a game
/// </summary>
public sealed class InvitePlayerRequest
{
    public int GameId { get; set; }
    public int UserId { get; set; }
}
