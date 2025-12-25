namespace Internal.FantaSottone.Infrastructure.Repositories;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Infrastructure.Mappers;
using Internal.FantaSottone.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

internal sealed class PlayerRepository : IPlayerRepository
{
    private readonly FantaSottoneContext _context;

    public PlayerRepository(FantaSottoneContext context)
    {
        _context = context;
    }

    public async Task<Player?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PlayerEntity.FindAsync([id], cancellationToken);
        return entity?.ToDomain();
    }

    public async Task<Player?> GetByCredentialsAsync(string username, string accessCode, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PlayerEntity
            .FirstOrDefaultAsync(p => p.Username == username && p.AccessCode == accessCode, cancellationToken);
        return entity?.ToDomain();
    }

    public async Task<IEnumerable<Player>> GetLeaderboardAsync(int gameId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PlayerEntity
            .Where(p => p.GameId == gameId)
            .OrderByDescending(p => p.CurrentScore)
            .ThenBy(p => p.Id) // Tie-break by Id ASC
            .ToListAsync(cancellationToken);
        return entities.Select(e => e.ToDomain());
    }

    public async Task<IEnumerable<Player>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PlayerEntity
            .Where(p => p.GameId == gameId)
            .ToListAsync(cancellationToken);
        return entities.Select(e => e.ToDomain());
    }

    public async Task<int> CountPlayersWithScoreLessThanOrEqualToZeroAsync(int gameId, CancellationToken cancellationToken = default)
    {
        return await _context.PlayerEntity
            .Where(p => p.GameId == gameId && p.CurrentScore <= 0)
            .CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Player>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.PlayerEntity.ToListAsync(cancellationToken);
        return entities.Select(e => e.ToDomain());
    }

    public async Task<Player> AddAsync(Player entity, CancellationToken cancellationToken = default)
    {
        var dbEntity = entity.ToEntity();
        await _context.PlayerEntity.AddAsync(dbEntity, cancellationToken);
        return dbEntity.ToDomain();
    }

    public Task<Player> UpdateAsync(Player entity, CancellationToken cancellationToken = default)
    {
        var dbEntity = entity.ToEntity();
        _context.PlayerEntity.Update(dbEntity);
        return Task.FromResult(dbEntity.ToDomain());
    }

    public Task DeleteAsync(Player entity, CancellationToken cancellationToken = default)
    {
        var dbEntity = entity.ToEntity();
        _context.PlayerEntity.Remove(dbEntity);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
