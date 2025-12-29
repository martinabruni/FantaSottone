namespace Internal.FantaSottone.Domain.Repositories;

using Internal.FantaSottone.Domain.Dtos;
using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Results;

/// <summary>
/// Repository interface for User entity data access
/// </summary>
public interface IUserRepository : IRepository<User, int>
{
    /// <summary>
    /// Retrieves a user by email address
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AppResult containing the user if found</returns>
    Task<AppResult<User>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for users by username pattern
    /// </summary>
    /// <param name="searchTerm">Search term to filter usernames</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AppResult containing list of matching users</returns>
    Task<AppResult<IEnumerable<UserSearchDto>>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all games that a user is participating in with detailed information
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AppResult containing list of games with creator info and status</returns>
    Task<AppResult<IEnumerable<GameListItemDto>>> GetUserGamesAsync(int userId, CancellationToken cancellationToken = default);
}
