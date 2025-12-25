namespace Internal.FantaSottone.Infrastructure.Repositories;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Internal.FantaSottone.Infrastructure.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

internal sealed class RuleAssignmentRepository : BaseRepository<RuleAssignment, RuleAssignmentEntity, int>, IRuleAssignmentRepository
{
    public RuleAssignmentRepository(FantaSottoneContext context, ILogger<RuleAssignmentRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<AppResult<IEnumerable<RuleAssignment>>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.RuleAssignmentEntity
                .Where(ra => ra.GameId == gameId)
                .OrderByDescending(ra => ra.AssignedAt)
                .ToListAsync(cancellationToken);

            var domainEntities = entities.Adapt<IEnumerable<RuleAssignment>>();
            return AppResult<IEnumerable<RuleAssignment>>.Success(domainEntities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignments for game {GameId}", gameId);
            return AppResult<IEnumerable<RuleAssignment>>.InternalServerError($"Error retrieving assignments: {ex.Message}");
        }
    }

    public async Task<AppResult<bool>> IsRuleAssignedAsync(int ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _context.RuleAssignmentEntity
                .AnyAsync(ra => ra.RuleId == ruleId, cancellationToken);

            return AppResult<bool>.Success(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if rule {RuleId} is assigned", ruleId);
            return AppResult<bool>.InternalServerError($"Error checking rule assignment: {ex.Message}");
        }
    }

    public async Task<AppResult<RuleAssignment>> GetByRuleIdAsync(int ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.RuleAssignmentEntity
                .FirstOrDefaultAsync(ra => ra.RuleId == ruleId, cancellationToken);

            if (entity == null)
            {
                // Not finding an assignment is not necessarily an error - the rule might not be assigned yet
                return AppResult<RuleAssignment>.NotFound($"No assignment found for rule {ruleId}");
            }

            var domainEntity = entity.Adapt<RuleAssignment>();
            return AppResult<RuleAssignment>.Success(domainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignment for rule {RuleId}", ruleId);
            return AppResult<RuleAssignment>.InternalServerError($"Error retrieving assignment: {ex.Message}");
        }
    }

    public async Task<AppResult<int>> CountByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _context.RuleAssignmentEntity
                .Where(ra => ra.GameId == gameId)
                .CountAsync(cancellationToken);

            return AppResult<int>.Success(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting assignments for game {GameId}", gameId);
            return AppResult<int>.InternalServerError($"Error counting assignments: {ex.Message}");
        }
    }

    /// <summary>
    /// Override AddAsync to handle unique constraint for "La prima che" logic
    /// </summary>
    public override async Task<AppResult<RuleAssignment>> AddAsync(RuleAssignment entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var dbEntity = entity.Adapt<RuleAssignmentEntity>();
            await _dbSet.AddAsync(dbEntity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var savedEntity = dbEntity.Adapt<RuleAssignment>();
            _logger.LogInformation("Rule {RuleId} assigned to player {PlayerId} in game {GameId}", 
                entity.RuleId, entity.AssignedToPlayerId, entity.GameId);
            return AppResult<RuleAssignment>.Created(savedEntity);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UX_RuleAssignmentEntity_RuleId") == true)
        {
            // This is the "La prima che" mechanism - race condition detected
            _logger.LogWarning(ex, "Rule {RuleId} already assigned (race condition)", entity.RuleId);
            return AppResult<RuleAssignment>.Conflict("Rule has already been assigned by another player", "RULE_ALREADY_ASSIGNED");
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true || 
                                            ex.InnerException?.Message.Contains("duplicate") == true)
        {
            _logger.LogWarning(ex, "Unique constraint violation when assigning rule {RuleId}", entity.RuleId);
            return AppResult<RuleAssignment>.Conflict("A record with these values already exists", "DUPLICATE_RECORD");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning rule {RuleId} to player {PlayerId}", entity.RuleId, entity.AssignedToPlayerId);
            return AppResult<RuleAssignment>.InternalServerError($"Error assigning rule: {ex.Message}");
        }
    }
}
