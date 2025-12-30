namespace Internal.FantaSottone.Domain.Managers;

using Internal.FantaSottone.Domain.Dtos;
using Internal.FantaSottone.Domain.Results;

/// <summary>
/// Manager for authentication operations
/// </summary>
public interface IAuthManager
{
    /// <summary>
    /// Authenticates a user with Google OAuth
    /// Creates user on first login
    /// </summary>
    Task<AppResult<GoogleAuthResponse>> GoogleAuthAsync(
        GoogleAuthRequest request,
        CancellationToken cancellationToken = default);
}
