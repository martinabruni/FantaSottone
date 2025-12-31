namespace Internal.FantaSottone.Domain.Dtos;

/// <summary>
/// Request for Google authentication
/// </summary>
public sealed class GoogleAuthRequest
{
    /// <summary>
    /// Google ID token received from frontend
    /// </summary>
    public string IdToken { get; set; } = string.Empty;
}
