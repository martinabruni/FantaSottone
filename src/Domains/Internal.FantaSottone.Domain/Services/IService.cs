namespace Internal.FantaSottone.Domain.Services;

using Internal.FantaSottone.Domain.Results;

/// <summary>
/// Generic service interface for CRUD operations
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The primary key type</typeparam>
public interface IService<TEntity, TKey>
    where TEntity : class
    where TKey : notnull
{
    /// <summary>
    /// Retrieves an entity by its identifier
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AppResult containing the entity if found</returns>
    Task<AppResult<TEntity>> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AppResult containing the list of entities</returns>
    Task<AppResult<IEnumerable<TEntity>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new entity
    /// </summary>
    /// <param name="entity">The entity to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AppResult containing the created entity</returns>
    Task<AppResult<TEntity>> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AppResult containing the updated entity</returns>
    Task<AppResult<TEntity>> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its identifier
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AppResult indicating the operation outcome</returns>
    Task<AppResult> DeleteAsync(TKey id, CancellationToken cancellationToken = default);
}
