namespace Internal.FantaSottone.Domain.Dtos;

/// <summary>
/// Response for email/password authentication
/// </summary>
public sealed class EmailAuthResponse
{
    /// <summary>
    /// JWT token for subsequent API calls
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// User email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User ID in the system
    /// </summary>
    public int UserId { get; set; }
}
