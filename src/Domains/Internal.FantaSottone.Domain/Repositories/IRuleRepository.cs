namespace Internal.FantaSottone.Domain.Repositories;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Results;

/// <summary>
/// Repository interface for Rule entity
/// </summary>
public interface IRuleRepository : IRepository<Rule, int>
{
    /// <summary>
    /// Gets rules by game ID
    /// </summary>
    Task<AppResult<IEnumerable<Rule>>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts total rules for a game
    /// </summary>
    Task<AppResult<int>> CountByGameIdAsync(int gameId, CancellationToken cancellationToken = default);
}
