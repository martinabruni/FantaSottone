namespace Internal.FantaSottone.Domain.Managers;

using Internal.FantaSottone.Domain.Dtos;
using Internal.FantaSottone.Domain.Models;
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
    /// Registers a new user in the system
    /// </summary>
    Task<AppResult<User>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user and generates a token
    /// </summary>
    Task<AppResult<LoginResponse>> LoginUserAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a password against a hash
    /// </summary>
    bool ValidatePassword(string password, string passwordHash);

    /// <summary>
    /// Hashes a password
    /// </summary>
    string HashPassword(string password);
}
