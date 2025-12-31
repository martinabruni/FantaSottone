namespace Internal.FantaSottone.Domain.Dtos;

/// <summary>
/// Request for email/password authentication
/// </summary>
public sealed class EmailAuthRequest
{
    /// <summary>
    /// User email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User password (stored in plain text)
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
