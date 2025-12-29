namespace Internal.FantaSottone.Domain.Managers;

using Internal.FantaSottone.Domain.Dtos;
using Internal.FantaSottone.Domain.Results;

/// <summary>
/// Manager for authentication operations
/// </summary>
public interface IAuthManager
{
    /// <summary>
    /// Authenticates a player and returns JWT token with game/player info
    /// </summary>
    Task<AppResult<LoginResult>> LoginAsync(
        string username,
        string accessCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user with Google OAuth
    /// Creates user on first login
    /// </summary>
    Task<AppResult<GoogleAuthResponse>> GoogleAuthAsync(
        GoogleAuthRequest request,
        CancellationToken cancellationToken = default);
}
