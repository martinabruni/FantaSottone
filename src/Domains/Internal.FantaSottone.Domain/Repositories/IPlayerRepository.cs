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
    Task<AppResult<Player>> GetByCredentialsAsync(string username, string accessCode, CancellationToken cancellationToken = default);

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
}
