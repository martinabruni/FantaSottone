namespace Internal.FantaSottone.Domain.Dtos;

/// <summary>
/// Response for successful login
/// </summary>
public sealed class LoginResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
