namespace Internal.FantaSottone.Domain.Services;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Results;

/// <summary>
/// Service interface for Player operations
/// </summary>
public interface IPlayerService : IService<Player, int>
{
    /// <summary>
    /// Updates player score
    /// </summary>
    Task<AppResult<Player>> UpdateScoreAsync(int playerId, int newScore, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets players by game ID
    /// </summary>
    Task<AppResult<IEnumerable<Player>>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default);
}
