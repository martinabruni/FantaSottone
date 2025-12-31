namespace Internal.FantaSottone.Domain.Repositories;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Results;

/// <summary>
/// Repository interface for Game entity
/// </summary>
public interface IGameRepository : IRepository<Game, int>
{
    /// <summary>
    /// Gets game with navigation properties loaded
    /// </summary>
    Task<AppResult<Game>> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is the creator of a game
    /// </summary>
    /// <param name="gameId">The game identifier</param>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AppResult containing true if user is creator</returns>
    Task<AppResult<bool>> IsUserCreatorAsync(int gameId, int userId, CancellationToken cancellationToken = default);
}
