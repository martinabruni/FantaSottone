namespace Internal.FantaSottone.Domain.Services;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Results;

/// <summary>
/// Service interface for Game operations
/// </summary>
public interface IGameService : IService<Game, int>
{
    /// <summary>
    /// Gets the leaderboard for a game
    /// </summary>
    Task<AppResult<IEnumerable<Player>>> GetLeaderboardAsync(int gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends a game and determines the winner
    /// </summary>
    Task<AppResult<Game>> EndGameAsync(int gameId, int creatorPlayerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if all rules are assigned or if 3+ players have score <= 0
    /// </summary>
    Task<bool> ShouldEndGameAsync(int gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Automatically ends game if conditions met, returns winner
    /// </summary>
    Task<AppResult<Game>> TryAutoEndGameAsync(int gameId, CancellationToken cancellationToken = default);
}
