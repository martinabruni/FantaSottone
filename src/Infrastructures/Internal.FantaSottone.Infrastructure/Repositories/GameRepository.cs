namespace Internal.FantaSottone.Infrastructure.Repositories;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Infrastructure.Mappers;
using Internal.FantaSottone.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

internal sealed class GameRepository : IGameRepository
{
    private readonly FantaSottoneContext _context;

    public GameRepository(FantaSottoneContext context)
    {
        _context = context;
    }

    public async Task<Game?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.GameEntity.FindAsync([id], cancellationToken);
        return entity?.ToDomain();
    }

    public async Task<Game?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.GameEntity
            .Include(g => g.PlayerEntity)
            .Include(g => g.RuleEntity)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        return entity?.ToDomain();
    }

    public async Task<IEnumerable<Game>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.GameEntity.ToListAsync(cancellationToken);
        return entities.Select(e => e.ToDomain());
    }

    public async Task<Game> AddAsync(Game entity, CancellationToken cancellationToken = default)
    {
        var dbEntity = entity.ToEntity();
        await _context.GameEntity.AddAsync(dbEntity, cancellationToken);
        return dbEntity.ToDomain();
    }

    public Task<Game> UpdateAsync(Game entity, CancellationToken cancellationToken = default)
    {
        var dbEntity = entity.ToEntity();
        _context.GameEntity.Update(dbEntity);
        return Task.FromResult(dbEntity.ToDomain());
    }

    public Task DeleteAsync(Game entity, CancellationToken cancellationToken = default)
    {
        var dbEntity = entity.ToEntity();
        _context.GameEntity.Remove(dbEntity);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
