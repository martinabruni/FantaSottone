namespace Internal.FantaSottone.Domain.Dtos;

/// <summary>
/// Request for user login
/// </summary>
public sealed class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
