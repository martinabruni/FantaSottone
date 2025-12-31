namespace Internal.FantaSottone.Domain.Dtos;

/// <summary>
/// Request to invite a user to a game by email
/// </summary>
public sealed class InvitePlayerByEmailRequest
{
    public string Email { get; set; } = string.Empty;
}
