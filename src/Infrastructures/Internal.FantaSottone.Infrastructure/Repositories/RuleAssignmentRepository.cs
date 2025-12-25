namespace Internal.FantaSottone.Infrastructure.Repositories;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Infrastructure.Mappers;
using Internal.FantaSottone.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

internal sealed class RuleAssignmentRepository : IRuleAssignmentRepository
{
    private readonly FantaSottoneContext _context;

    public RuleAssignmentRepository(FantaSottoneContext context)
    {
        _context = context;
    }

    public async Task<RuleAssignment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.RuleAssignmentEntity.FindAsync([id], cancellationToken);
        return entity?.ToDomain();
    }

    public async Task<IEnumerable<RuleAssignment>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.RuleAssignmentEntity
            .Where(ra => ra.GameId == gameId)
            .OrderByDescending(ra => ra.AssignedAt)
            .ToListAsync(cancellationToken);
        return entities.Select(e => e.ToDomain());
    }

    public async Task<bool> IsRuleAssignedAsync(int ruleId, CancellationToken cancellationToken = default)
    {
        return await _context.RuleAssignmentEntity
            .AnyAsync(ra => ra.RuleId == ruleId, cancellationToken);
    }

    public async Task<RuleAssignment?> GetByRuleIdAsync(int ruleId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.RuleAssignmentEntity
            .FirstOrDefaultAsync(ra => ra.RuleId == ruleId, cancellationToken);
        return entity?.ToDomain();
    }

    public async Task<int> CountByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
    {
        return await _context.RuleAssignmentEntity
            .Where(ra => ra.GameId == gameId)
            .CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<RuleAssignment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.RuleAssignmentEntity.ToListAsync(cancellationToken);
        return entities.Select(e => e.ToDomain());
    }

    public async Task<RuleAssignment> AddAsync(RuleAssignment entity, CancellationToken cancellationToken = default)
    {
        var dbEntity = entity.ToEntity();
        await _context.RuleAssignmentEntity.AddAsync(dbEntity, cancellationToken);
        return dbEntity.ToDomain();
    }

    public Task<RuleAssignment> UpdateAsync(RuleAssignment entity, CancellationToken cancellationToken = default)
    {
        var dbEntity = entity.ToEntity();
        _context.RuleAssignmentEntity.Update(dbEntity);
        return Task.FromResult(dbEntity.ToDomain());
    }

    public Task DeleteAsync(RuleAssignment entity, CancellationToken cancellationToken = default)
    {
        var dbEntity = entity.ToEntity();
        _context.RuleAssignmentEntity.Remove(dbEntity);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
