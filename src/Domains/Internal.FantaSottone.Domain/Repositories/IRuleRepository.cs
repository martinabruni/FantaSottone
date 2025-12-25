namespace Internal.FantaSottone.Domain.Repositories;

using Internal.FantaSottone.Domain.Models;

/// <summary>
/// Repository interface for Rule entity
/// </summary>
public interface IRuleRepository : IRepository<Rule, int>
{
    /// <summary>
    /// Gets rules by game ID
    /// </summary>
    Task<IEnumerable<Rule>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts total rules for a game
    /// </summary>
    Task<int> CountByGameIdAsync(int gameId, CancellationToken cancellationToken = default);
}
