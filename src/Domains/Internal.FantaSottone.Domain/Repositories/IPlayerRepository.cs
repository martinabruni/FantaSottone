namespace Internal.FantaSottone.Domain.Repositories;

using Internal.FantaSottone.Domain.Models;

/// <summary>
/// Repository interface for Player entity
/// </summary>
public interface IPlayerRepository : IRepository<Player, int>
{
    /// <summary>
    /// Gets a player by username and access code
    /// </summary>
    Task<Player?> GetByCredentialsAsync(string username, string accessCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all players for a game ordered by score descending (leaderboard)
    /// </summary>
    Task<IEnumerable<Player>> GetLeaderboardAsync(int gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets players by game ID
    /// </summary>
    Task<IEnumerable<Player>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts players with score less than or equal to zero for a game
    /// </summary>
    Task<int> CountPlayersWithScoreLessThanOrEqualToZeroAsync(int gameId, CancellationToken cancellationToken = default);
}
