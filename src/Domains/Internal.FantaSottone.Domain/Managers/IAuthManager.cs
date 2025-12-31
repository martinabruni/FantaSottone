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

    /// <summary>
    /// Registers a new user with email and password
    /// </summary>
    Task<AppResult<EmailAuthResponse>> RegisterWithEmailAsync(
        EmailRegisterRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user with email and password
    /// </summary>
    Task<AppResult<EmailAuthResponse>> LoginWithEmailAsync(
        EmailAuthRequest request,
        CancellationToken cancellationToken = default);
}
