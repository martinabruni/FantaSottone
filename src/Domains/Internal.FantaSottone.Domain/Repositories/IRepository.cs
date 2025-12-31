namespace Internal.FantaSottone.Domain.Repositories;

using Internal.FantaSottone.Domain.Results;

/// <summary>
/// Generic repository interface for data access operations
/// All operations return AppResult for consistent error handling
/// SaveChanges removed - each operation auto-saves
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The primary key type</typeparam>
public interface IRepository<TEntity, TKey>
    where TEntity : class
    where TKey : notnull
{
    /// <summary>
    /// Retrieves an entity by its identifier
    /// </summary>
    Task<AppResult<TEntity>> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities
    /// </summary>
    Task<AppResult<IEnumerable<TEntity>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity and saves changes
    /// </summary>
    Task<AppResult<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity and saves changes
    /// </summary>
    Task<AppResult<TEntity>> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity and saves changes
    /// </summary>
    Task<AppResult> DeleteAsync(TKey key, CancellationToken cancellationToken = default);
}
