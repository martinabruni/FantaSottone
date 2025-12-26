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
}
