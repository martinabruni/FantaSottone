namespace Internal.FantaSottone.Business.Services;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Internal.FantaSottone.Domain.Services;
using Microsoft.Extensions.Logging;

internal sealed class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IRuleRepository _ruleRepository;
    private readonly IRuleAssignmentRepository _ruleAssignmentRepository;
    private readonly ILogger<GameService> _logger;

    public GameService(
        IGameRepository gameRepository,
        IPlayerRepository playerRepository,
        IRuleRepository ruleRepository,
        IRuleAssignmentRepository ruleAssignmentRepository,
        ILogger<GameService> logger)
    {
        _gameRepository = gameRepository;
        _playerRepository = playerRepository;
        _ruleRepository = ruleRepository;
        _ruleAssignmentRepository = ruleAssignmentRepository;
        _logger = logger;
    }

    public async Task<AppResult<Game>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _gameRepository.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error getting game {GameId}", id);
            return AppResult<Game>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<IEnumerable<Game>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _gameRepository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error getting all games");
            return AppResult<IEnumerable<Game>>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<Game>> CreateAsync(Game entity, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _gameRepository.AddAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error creating game");
            return AppResult<Game>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<Game>> UpdateAsync(Game entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingResult = await _gameRepository.GetByIdAsync(entity.Id, cancellationToken);
            if (existingResult.IsFailure)
                return existingResult;

            return await _gameRepository.UpdateAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error updating game {GameId}", entity.Id);
            return AppResult<Game>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var gameResult = await _gameRepository.GetByIdAsync(id, cancellationToken);
            if (gameResult.IsFailure)
                return AppResult.NotFound(gameResult.Errors.First().Message);

            return await _gameRepository.DeleteAsync(gameResult.Value!, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error deleting game {GameId}", id);
            return AppResult.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<IEnumerable<Player>>> GetLeaderboardAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify game exists
            var gameResult = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
            if (gameResult.IsFailure)
                return AppResult<IEnumerable<Player>>.NotFound($"Game with ID {gameId} not found");

            return await _playerRepository.GetLeaderboardAsync(gameId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error getting leaderboard for game {GameId}", gameId);
            return AppResult<IEnumerable<Player>>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<Game>> EndGameAsync(int gameId, int creatorPlayerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var gameResult = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
            if (gameResult.IsFailure)
                return gameResult;

            var game = gameResult.Value!;

            if (game.Status == GameStatus.Ended)
            {
                _logger.LogWarning("Attempt to end already ended game {GameId}", gameId);
                return AppResult<Game>.BadRequest("Game has already ended");
            }

            // Check if requester is creator
            if (game.CreatorPlayerId != creatorPlayerId)
            {
                _logger.LogWarning("Player {PlayerId} attempted to end game but is not creator", creatorPlayerId);
                return AppResult<Game>.Forbidden("Only the game creator can end the game");
            }

            // Determine winner (highest score, tie-break by Id ASC)
            var playersResult = await _playerRepository.GetLeaderboardAsync(gameId, cancellationToken);
            if (playersResult.IsFailure)
                return AppResult<Game>.InternalServerError("Failed to retrieve players for winner determination");

            var playersList = playersResult.Value!.ToList();

            if (playersList.Count == 0)
            {
                _logger.LogWarning("Attempt to end game {GameId} with no players", gameId);
                return AppResult<Game>.BadRequest("Cannot end game with no players");
            }

            var winner = playersList.First(); // Already ordered by score DESC, Id ASC

            game.Status = GameStatus.Ended;
            game.WinnerPlayerId = winner.Id;
            game.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Game {GameId} ended, winner is player {PlayerId}", gameId, winner.Id);
            return await _gameRepository.UpdateAsync(game, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error ending game {GameId}", gameId);
            return AppResult<Game>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<bool> ShouldEndGameAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Condition 1: All rules assigned
            var totalRulesResult = await _ruleRepository.CountByGameIdAsync(gameId, cancellationToken);
            var assignedRulesResult = await _ruleAssignmentRepository.CountByGameIdAsync(gameId, cancellationToken);

            if (totalRulesResult.IsSuccess && assignedRulesResult.IsSuccess)
            {
                var totalRules = totalRulesResult.Value!;
                var assignedRules = assignedRulesResult.Value!;

                if (totalRules > 0 && totalRules == assignedRules)
                {
                    _logger.LogInformation("Game {GameId} should end: all {Count} rules assigned", gameId, totalRules);
                    return true;
                }
            }

            // Condition 2: 3 or more players with score <= 0
            var playersWithLowScoreResult = await _playerRepository.CountPlayersWithScoreLessThanOrEqualToZeroAsync(gameId, cancellationToken);

            if (playersWithLowScoreResult.IsSuccess && playersWithLowScoreResult.Value! >= 3)
            {
                _logger.LogInformation("Game {GameId} should end: {Count} players with score <= 0", gameId, playersWithLowScoreResult.Value);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if game {GameId} should end", gameId);
            return false; // Don't end on error
        }
    }

    public async Task<AppResult<Game>> TryAutoEndGameAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            var shouldEnd = await ShouldEndGameAsync(gameId, cancellationToken);
            if (!shouldEnd)
                return AppResult<Game>.BadRequest("Game end conditions not met");

            var gameResult = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
            if (gameResult.IsFailure)
                return gameResult;

            var game = gameResult.Value!;

            if (game.Status == GameStatus.Ended)
            {
                _logger.LogInformation("Game {GameId} already ended", gameId);
                return AppResult<Game>.Success(game); // Already ended
            }

            // Determine winner
            var playersResult = await _playerRepository.GetLeaderboardAsync(gameId, cancellationToken);
            if (playersResult.IsFailure)
                return AppResult<Game>.InternalServerError("Failed to retrieve players for winner determination");

            var playersList = playersResult.Value!.ToList();

            if (playersList.Count == 0)
            {
                _logger.LogWarning("Cannot auto-end game {GameId} with no players", gameId);
                return AppResult<Game>.BadRequest("Cannot end game with no players");
            }

            var winner = playersList.First();

            game.Status = GameStatus.Ended;
            game.WinnerPlayerId = winner.Id;
            game.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Game {GameId} auto-ended, winner is player {PlayerId}", gameId, winner.Id);
            return await _gameRepository.UpdateAsync(game, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error auto-ending game {GameId}", gameId);
            return AppResult<Game>.InternalServerError($"Service error: {ex.Message}");
        }
    }
}
