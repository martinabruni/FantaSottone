namespace Internal.FantaSottone.Infrastructure.Repositories;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Infrastructure.Mappers;
using Internal.FantaSottone.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

internal sealed class RuleRepository : IRuleRepository
{
    private readonly FantaSottoneContext _context;

    public RuleRepository(FantaSottoneContext context)
    {
        _context = context;
    }

    public async Task<Rule?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.RuleEntity.FindAsync([id], cancellationToken);
        return entity?.ToDomain();
    }

    public async Task<IEnumerable<Rule>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.RuleEntity
            .Where(r => r.GameId == gameId)
            .ToListAsync(cancellationToken);
        return entities.Select(e => e.ToDomain());
    }

    public async Task<int> CountByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
    {
        return await _context.RuleEntity
            .Where(r => r.GameId == gameId)
            .CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Rule>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.RuleEntity.ToListAsync(cancellationToken);
        return entities.Select(e => e.ToDomain());
    }

    public async Task<Rule> AddAsync(Rule entity, CancellationToken cancellationToken = default)
    {
        var dbEntity = entity.ToEntity();
        await _context.RuleEntity.AddAsync(dbEntity, cancellationToken);
        return dbEntity.ToDomain();
    }

    public Task<Rule> UpdateAsync(Rule entity, CancellationToken cancellationToken = default)
    {
        var dbEntity = entity.ToEntity();
        _context.RuleEntity.Update(dbEntity);
        return Task.FromResult(dbEntity.ToDomain());
    }

    public Task DeleteAsync(Rule entity, CancellationToken cancellationToken = default)
    {
        var dbEntity = entity.ToEntity();
        _context.RuleEntity.Remove(dbEntity);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
