namespace Internal.FantaSottone.Domain.Services;

using Internal.FantaSottone.Domain.Dtos;
using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Results;

/// <summary>
/// Service interface for user management operations
/// </summary>
public interface IUserService : IService<User, int>
{
    /// <summary>
    /// Searches for users by username pattern (for invite feature)
    /// </summary>
    /// <param name="searchTerm">Search term to filter usernames</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AppResult containing list of matching users</returns>
    Task<AppResult<IEnumerable<UserSearchDto>>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all games that a user is participating in
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AppResult containing list of games for the user's dashboard</returns>
    Task<AppResult<IEnumerable<GameListItemDto>>> GetUserGamesAsync(int userId, CancellationToken cancellationToken = default);
}
