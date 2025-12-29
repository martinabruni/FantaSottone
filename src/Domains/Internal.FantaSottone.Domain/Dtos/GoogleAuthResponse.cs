namespace Internal.FantaSottone.Domain.Dtos;

/// <summary>
/// Response for Google authentication
/// </summary>
public sealed class GoogleAuthResponse
{
    /// <summary>
    /// JWT token for subsequent API calls
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// User email from Google account
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// User ID in the system
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Indicates if this was the user's first login
    /// </summary>
    public bool IsFirstLogin { get; set; }
}
