namespace Internal.FantaSottone.Business.Services;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Internal.FantaSottone.Domain.Services;
using Microsoft.Extensions.Logging;

internal sealed class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<PlayerService> _logger;

    public PlayerService(
        IPlayerRepository playerRepository,
        ILogger<PlayerService> logger)
    {
        _playerRepository = playerRepository;
        _logger = logger;
    }

    public async Task<AppResult<Player>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _playerRepository.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error getting player {PlayerId}", id);
            return AppResult<Player>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<IEnumerable<Player>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _playerRepository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error getting all players");
            return AppResult<IEnumerable<Player>>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<Player>> CreateAsync(Player entity, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _playerRepository.AddAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error creating player");
            return AppResult<Player>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<Player>> UpdateAsync(Player entity, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify player exists first
            var existingResult = await _playerRepository.GetByIdAsync(entity.Id, cancellationToken);
            if (existingResult.IsFailure)
                return existingResult;

            return await _playerRepository.UpdateAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error updating player {PlayerId}", entity.Id);
            return AppResult<Player>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var playerResult = await _playerRepository.GetByIdAsync(id, cancellationToken);
            if (playerResult.IsFailure)
                return AppResult.NotFound(playerResult.Errors.First().Message);

            return await _playerRepository.DeleteAsync(playerResult.Value!, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error deleting player {PlayerId}", id);
            return AppResult.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<Player>> GetByCredentialsAsync(string username, string accessCode, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _playerRepository.GetByCredentialsAsync(username, accessCode, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error getting player by credentials for username {Username}", username);
            return AppResult<Player>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<Player>> UpdateScoreAsync(int playerId, int newScore, CancellationToken cancellationToken = default)
    {
        try
        {
            var playerResult = await _playerRepository.GetByIdAsync(playerId, cancellationToken);
            if (playerResult.IsFailure)
                return playerResult;

            var player = playerResult.Value!;
            player.CurrentScore = newScore;
            player.UpdatedAt = DateTime.UtcNow;

            return await _playerRepository.UpdateAsync(player, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error updating score for player {PlayerId}", playerId);
            return AppResult<Player>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<IEnumerable<Player>>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _playerRepository.GetByGameIdAsync(gameId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error getting players for game {GameId}", gameId);
            return AppResult<IEnumerable<Player>>.InternalServerError($"Service error: {ex.Message}");
        }
    }
}
