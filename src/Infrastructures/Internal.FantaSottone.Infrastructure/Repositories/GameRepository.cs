namespace Internal.FantaSottone.Infrastructure.Repositories;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Internal.FantaSottone.Infrastructure.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

internal sealed class GameRepository : BaseRepository<Game, GameEntity, int>, IGameRepository
{
    public GameRepository(FantaSottoneContext context, ILogger<GameRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<AppResult<Game>> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.GameEntity
                .Include(g => g.PlayerEntity)
                .Include(g => g.RuleEntity)
                .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("Game with ID {GameId} not found (with details)", id);
                return AppResult<Game>.NotFound($"Game with ID {id} not found");
            }

            var domainEntity = entity.Adapt<Game>();
            return AppResult<Game>.Success(domainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Game with ID {GameId} (with details)", id);
            return AppResult<Game>.InternalServerError($"Error retrieving game with details: {ex.Message}");
        }
    }

    public async Task<AppResult<bool>> IsUserCreatorAsync(int gameId, int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var game = await _context.GameEntity
                .Include(g => g.CreatorPlayer)
                .FirstOrDefaultAsync(g => g.Id == gameId, cancellationToken);

            if (game == null)
                return AppResult<bool>.NotFound($"Game with ID {gameId} not found");

            var isCreator = game.CreatorPlayer?.UserId == userId;
            return AppResult<bool>.Success(isCreator);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} is creator of game {GameId}", userId, gameId);
            return AppResult<bool>.InternalServerError($"Error checking creator status: {ex.Message}");
        }
    }
}

