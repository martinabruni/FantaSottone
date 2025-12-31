namespace Internal.FantaSottone.Infrastructure.Repositories;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Internal.FantaSottone.Infrastructure.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

internal sealed class PlayerRepository : BaseRepository<Player, PlayerEntity, int>, IPlayerRepository
{
    public PlayerRepository(FantaSottoneContext context, ILogger<PlayerRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<AppResult<IEnumerable<Player>>> GetLeaderboardAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.PlayerEntity
                .Include(p => p.User) // Include User for email
                .Where(p => p.GameId == gameId)
                .OrderByDescending(p => p.CurrentScore)
                .ThenBy(p => p.Id) // Tie-break by Id ASC
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var domainEntities = entities.Adapt<IEnumerable<Player>>();
            _logger.LogInformation("Retrieved leaderboard for game {GameId} with {Count} players", gameId, entities.Count);
            return AppResult<IEnumerable<Player>>.Success(domainEntities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leaderboard for game {GameId}", gameId);
            return AppResult<IEnumerable<Player>>.InternalServerError($"Error retrieving leaderboard: {ex.Message}");
        }
    }

    public async Task<AppResult<IEnumerable<Player>>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.PlayerEntity
                .Include(p => p.User)
                .Where(p => p.GameId == gameId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var domainEntities = entities.Adapt<IEnumerable<Player>>();
            _logger.LogInformation("Retrieved {Count} players for game {GameId}", entities.Count, gameId);
            return AppResult<IEnumerable<Player>>.Success(domainEntities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving players for game {GameId}", gameId);
            return AppResult<IEnumerable<Player>>.InternalServerError($"Error retrieving players: {ex.Message}");
        }
    }

    public async Task<AppResult<int>> CountPlayersWithScoreLessThanOrEqualToZeroAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _context.PlayerEntity
                .Where(p => p.GameId == gameId && p.CurrentScore <= 0)
                .CountAsync(cancellationToken);

            _logger.LogInformation("Counted {Count} players with score <= 0 in game {GameId}", count, gameId);
            return AppResult<int>.Success(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting players with score <= 0 in game {GameId}", gameId);
            return AppResult<int>.InternalServerError($"Error counting players: {ex.Message}");
        }
    }

    public async Task<AppResult<Player>> GetByGameAndUserAsync(int gameId, int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // FIX: Query PlayerEntity directly instead of the view to get proper mapping
            // and include User for email information
            var entity = await _context.PlayerEntity
                .Include(p => p.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.GameId == gameId && p.UserId == userId, cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("Player not found for game {GameId} and user {UserId}", gameId, userId);
                return AppResult<Player>.NotFound($"Player not found in this game");
            }

            // Use Mapster for consistent mapping from PlayerEntity to Player
            var domainEntity = entity.Adapt<Player>();
            return AppResult<Player>.Success(domainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving player for game {GameId} and user {UserId}", gameId, userId);
            return AppResult<Player>.InternalServerError($"Error retrieving player: {ex.Message}");
        }
    }

    public async Task<AppResult<bool>> ExistsAsync(int gameId, int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _context.PlayerEntity
                .AnyAsync(p => p.GameId == gameId && p.UserId == userId, cancellationToken);

            return AppResult<bool>.Success(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if player exists for game {GameId} and user {UserId}", gameId, userId);
            return AppResult<bool>.InternalServerError($"Error checking player existence: {ex.Message}");
        }
    }
}