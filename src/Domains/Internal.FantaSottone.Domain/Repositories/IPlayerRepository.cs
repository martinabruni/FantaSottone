namespace Internal.FantaSottone.Domain.Repositories;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Results;

/// <summary>
/// Repository interface for Player entity
/// </summary>
public interface IPlayerRepository : IRepository<Player, int>
{
    /// <summary>
    /// Gets a player by username and access code
    /// </summary>
    Task<AppResult<Player>> GetByCredentialsAsync(string username, string passwordHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all players for a game ordered by score descending (leaderboard)
    /// </summary>
    Task<AppResult<IEnumerable<Player>>> GetLeaderboardAsync(int gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets players by game ID
    /// </summary>
    Task<AppResult<IEnumerable<Player>>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts players with score less than or equal to zero for a game
    /// </summary>
    Task<AppResult<int>> CountPlayersWithScoreLessThanOrEqualToZeroAsync(int gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a player by game ID and user ID
    /// </summary>
    /// <param name="gameId">The game identifier</param>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AppResult containing the player if found</returns>
    Task<AppResult<Player>> GetByGameAndUserAsync(int gameId, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a player exists for a given game and user combination
    /// </summary>
    /// <param name="gameId">The game identifier</param>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AppResult containing true if player exists</returns>
    Task<AppResult<bool>> ExistsAsync(int gameId, int userId, CancellationToken cancellationToken = default);
}
