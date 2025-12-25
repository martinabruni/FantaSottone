namespace Internal.FantaSottone.Domain.Repositories;

using Internal.FantaSottone.Domain.Models;

/// <summary>
/// Repository interface for Game entity
/// </summary>
public interface IGameRepository : IRepository<Game, int>
{
    /// <summary>
    /// Gets game with navigation properties loaded
    /// </summary>
    Task<Game?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
}
