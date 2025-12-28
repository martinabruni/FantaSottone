namespace Internal.FantaSottone.Domain.Managers;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Results;

/// <summary>
/// Manager for game operations
/// </summary>
public interface IGameManager
{
    /// <summary>
    /// Creates a new game with players and rules
    /// </summary>
    Task<AppResult<StartGameResult>> StartGameAsync(
        string name,
        int initialScore,
        List<(string Username, string AccessCode, bool IsCreator)> players,
        List<(string Name, RuleType RuleType, int ScoreDelta)> rules,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a game by its identifier
    /// </summary>
    Task<AppResult<Game>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all games
    /// </summary>
    Task<AppResult<IEnumerable<Game>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new game
    /// </summary>
    Task<AppResult<Game>> CreateAsync(Game entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing game
    /// </summary>
    Task<AppResult<Game>> UpdateAsync(Game entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a game by its identifier
    /// </summary>
    Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken = default);

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

    /// <summary>
    /// Invites a registered user to join a game
    /// </summary>
    Task<AppResult<Player>> InvitePlayerAsync(int gameId, int userId, int requestingUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a user is a player in a specific game and sets it as active
    /// </summary>
    Task<AppResult<Player>> JoinGameAsync(int gameId, int userId, CancellationToken cancellationToken = default);
}
