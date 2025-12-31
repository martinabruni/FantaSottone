namespace Internal.FantaSottone.Domain.Dtos;

/// <summary>
/// Request for email/password registration
/// </summary>
public sealed class EmailRegisterRequest
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
