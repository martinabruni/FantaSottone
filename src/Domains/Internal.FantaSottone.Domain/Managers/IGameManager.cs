namespace Internal.FantaSottone.Domain.Managers;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Results;

/// <summary>
/// Manager for game operations
/// </summary>
public interface IGameManager
{
    Task<bool> ShouldEndGameAsync(int gameId, CancellationToken cancellationToken = default);
    Task<AppResult<Game>> TryAutoEndGameAsync(int gameId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets all games a user has been invited to
    /// </summary>
    Task<AppResult<IEnumerable<Game>>> GetUserGamesAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new game and invites players by email
    /// </summary>
    Task<AppResult<(Game game, Player creatorPlayer, List<string> invitedEmails, List<string> invalidEmails)>> CreateGameWithEmailInvitesAsync(
        string name,
        int initialScore,
        int creatorUserId,
        List<string> invitedEmails,
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
    Task<AppResult<(Game game, Player winner, IEnumerable<Player> leaderboard)>> EndGameAsync(
        int gameId,
        int requestingUserId,
        CancellationToken cancellationToken = default);

    Task<AppResult<Player>> InvitePlayerAsync(int gameId, int userId, int requestingUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Joins a game for the current user (activates the player for this session)
    /// </summary>
    Task<AppResult<Player>> JoinGameAsync(int gameId, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a game (transitions from Draft to Started status, creator only)
    /// </summary>
    Task<AppResult<Game>> StartGameAsync(int gameId, int requestingUserId, CancellationToken cancellationToken = default);
}
