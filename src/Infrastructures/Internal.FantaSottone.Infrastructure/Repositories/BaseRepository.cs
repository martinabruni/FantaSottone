namespace Internal.FantaSottone.Infrastructure.Repositories;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Internal.FantaSottone.Infrastructure.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Base repository implementation with Mapster mapping and error handling
/// Automatically saves changes after each operation
/// </summary>
internal abstract class BaseRepository<TDomainEntity, TDbEntity, TKey> : IRepository<TDomainEntity, TKey>
    where TDomainEntity : class, IEntity
    where TDbEntity : class
    where TKey : notnull
{
    protected readonly FantaSottoneContext _context;
    protected readonly DbSet<TDbEntity> _dbSet;
    protected readonly ILogger _logger;

    protected BaseRepository(FantaSottoneContext context, ILogger logger)
    {
        _context = context;
        _dbSet = context.Set<TDbEntity>();
        _logger = logger;
        ConfigureMapping();
    }

    /// <summary>
    /// Configure Mapster mappings for this entity pair
    /// Override in derived classes if custom mapping needed
    /// </summary>
    protected virtual void ConfigureMapping()
    {
        // Default: Auto-mapping between TDbEntity and TDomainEntity
        TypeAdapterConfig<TDbEntity, TDomainEntity>.NewConfig();
        TypeAdapterConfig<TDomainEntity, TDbEntity>.NewConfig();
    }

    public virtual async Task<AppResult<TDomainEntity>> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _dbSet.FindAsync([id], cancellationToken);
            
            if (entity == null)
            {
                _logger.LogWarning("Entity {EntityType} with ID {Id} not found", typeof(TDomainEntity).Name, id);
                return AppResult<TDomainEntity>.NotFound($"{typeof(TDomainEntity).Name} with ID {id} not found");
            }

            var domainEntity = entity.Adapt<TDomainEntity>();
            return AppResult<TDomainEntity>.Success(domainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving {EntityType} with ID {Id}", typeof(TDomainEntity).Name, id);
            return AppResult<TDomainEntity>.InternalServerError($"Error retrieving entity: {ex.Message}");
        }
    }

    public virtual async Task<AppResult<IEnumerable<TDomainEntity>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _dbSet.ToListAsync(cancellationToken);
            var domainEntities = entities.Adapt<IEnumerable<TDomainEntity>>();
            return AppResult<IEnumerable<TDomainEntity>>.Success(domainEntities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all {EntityType}", typeof(TDomainEntity).Name);
            return AppResult<IEnumerable<TDomainEntity>>.InternalServerError($"Error retrieving entities: {ex.Message}");
        }
    }

    public virtual async Task<AppResult<TDomainEntity>> AddAsync(TDomainEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var dbEntity = entity.Adapt<TDbEntity>();
            await _dbSet.AddAsync(dbEntity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var savedEntity = dbEntity.Adapt<TDomainEntity>();
            _logger.LogInformation("Created {EntityType} with ID {Id}", typeof(TDomainEntity).Name, savedEntity.Id);
            return AppResult<TDomainEntity>.Created(savedEntity);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true || 
                                            ex.InnerException?.Message.Contains("duplicate") == true)
        {
            _logger.LogWarning(ex, "Unique constraint violation when creating {EntityType}", typeof(TDomainEntity).Name);
            return AppResult<TDomainEntity>.Conflict("A record with these values already exists", "DUPLICATE_RECORD");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating {EntityType}", typeof(TDomainEntity).Name);
            return AppResult<TDomainEntity>.InternalServerError($"Error creating entity: {ex.Message}");
        }
    }

    public virtual async Task<AppResult<TDomainEntity>> UpdateAsync(TDomainEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var dbEntity = entity.Adapt<TDbEntity>();
            _dbSet.Update(dbEntity);
            await _context.SaveChangesAsync(cancellationToken);

            var updatedEntity = dbEntity.Adapt<TDomainEntity>();
            _logger.LogInformation("Updated {EntityType} with ID {Id}", typeof(TDomainEntity).Name, entity.Id);
            return AppResult<TDomainEntity>.Success(updatedEntity);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency error updating {EntityType} with ID {Id}", typeof(TDomainEntity).Name, entity.Id);
            return AppResult<TDomainEntity>.Conflict("The entity was modified by another process", "CONCURRENCY_ERROR");
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true || 
                                            ex.InnerException?.Message.Contains("duplicate") == true)
        {
            _logger.LogWarning(ex, "Unique constraint violation when updating {EntityType}", typeof(TDomainEntity).Name);
            return AppResult<TDomainEntity>.Conflict("A record with these values already exists", "DUPLICATE_RECORD");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating {EntityType} with ID {Id}", typeof(TDomainEntity).Name, entity.Id);
            return AppResult<TDomainEntity>.InternalServerError($"Error updating entity: {ex.Message}");
        }
    }

    public virtual async Task<AppResult> DeleteAsync(TDomainEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var dbEntity = entity.Adapt<TDbEntity>();
            _dbSet.Remove(dbEntity);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted {EntityType} with ID {Id}", typeof(TDomainEntity).Name, entity.Id);
            return AppResult.Success();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("REFERENCE") == true || 
                                            ex.InnerException?.Message.Contains("FK_") == true)
        {
            _logger.LogWarning(ex, "Foreign key constraint violation when deleting {EntityType}", typeof(TDomainEntity).Name);
            return AppResult.Conflict("Cannot delete entity because it is referenced by other records", "FOREIGN_KEY_VIOLATION");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {EntityType} with ID {Id}", typeof(TDomainEntity).Name, entity.Id);
            return AppResult.InternalServerError($"Error deleting entity: {ex.Message}");
        }
    }
}
