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

    public async Task<AppResult<Player>> GetByCredentialsAsync(string username, string accessCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.PlayerEntity
                .FirstOrDefaultAsync(p => p.Username == username && p.AccessCode == accessCode, cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("Player with username {Username} not found or invalid access code", username);
                return AppResult<Player>.Unauthorized("Invalid username or access code");
            }

            var domainEntity = entity.Adapt<Player>();
            return AppResult<Player>.Success(domainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving player by credentials for username {Username}", username);
            return AppResult<Player>.InternalServerError($"Error retrieving player: {ex.Message}");
        }
    }

    public async Task<AppResult<IEnumerable<Player>>> GetLeaderboardAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.PlayerEntity
                .Where(p => p.GameId == gameId)
                .OrderByDescending(p => p.CurrentScore)
                .ThenBy(p => p.Id) // Tie-break by Id ASC
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
                .Where(p => p.GameId == gameId)
                .ToListAsync(cancellationToken);

            var domainEntities = entities.Adapt<IEnumerable<Player>>();
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

            return AppResult<int>.Success(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting players with low score for game {GameId}", gameId);
            return AppResult<int>.InternalServerError($"Error counting players: {ex.Message}");
        }
    }
}
