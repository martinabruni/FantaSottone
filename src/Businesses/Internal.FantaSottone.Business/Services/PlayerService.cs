namespace Internal.FantaSottone.Business.Services;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Internal.FantaSottone.Domain.Services;

internal sealed class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _playerRepository;

    public PlayerService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public async Task<AppResult<Player>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var player = await _playerRepository.GetByIdAsync(id, cancellationToken);
        return player != null
            ? AppResult<Player>.Success(player)
            : AppResult<Player>.NotFound($"Player with ID {id} not found");
    }

    public async Task<AppResult<IEnumerable<Player>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var players = await _playerRepository.GetAllAsync(cancellationToken);
        return AppResult<IEnumerable<Player>>.Success(players);
    }

    public async Task<AppResult<Player>> CreateAsync(Player entity, CancellationToken cancellationToken = default)
    {
        await _playerRepository.AddAsync(entity, cancellationToken);
        await _playerRepository.SaveChangesAsync(cancellationToken);
        return AppResult<Player>.Created(entity);
    }

    public async Task<AppResult<Player>> UpdateAsync(Player entity, CancellationToken cancellationToken = default)
    {
        var existing = await _playerRepository.GetByIdAsync(entity.Id, cancellationToken);
        if (existing == null)
            return AppResult<Player>.NotFound($"Player with ID {entity.Id} not found");

        await _playerRepository.UpdateAsync(entity, cancellationToken);
        await _playerRepository.SaveChangesAsync(cancellationToken);
        return AppResult<Player>.Success(entity);
    }

    public async Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var player = await _playerRepository.GetByIdAsync(id, cancellationToken);
        if (player == null)
            return AppResult.NotFound($"Player with ID {id} not found");

        await _playerRepository.DeleteAsync(player, cancellationToken);
        await _playerRepository.SaveChangesAsync(cancellationToken);
        return AppResult.Success();
    }

    public async Task<AppResult<Player>> GetByCredentialsAsync(string username, string accessCode, CancellationToken cancellationToken = default)
    {
        var player = await _playerRepository.GetByCredentialsAsync(username, accessCode, cancellationToken);
        return player != null
            ? AppResult<Player>.Success(player)
            : AppResult<Player>.Unauthorized("Invalid username or access code");
    }

    public async Task<AppResult<Player>> UpdateScoreAsync(int playerId, int newScore, CancellationToken cancellationToken = default)
    {
        var player = await _playerRepository.GetByIdAsync(playerId, cancellationToken);
        if (player == null)
            return AppResult<Player>.NotFound($"Player with ID {playerId} not found");

        player.CurrentScore = newScore;
        player.UpdatedAt = DateTime.UtcNow;

        await _playerRepository.UpdateAsync(player, cancellationToken);
        await _playerRepository.SaveChangesAsync(cancellationToken);

        return AppResult<Player>.Success(player);
    }

    public async Task<AppResult<IEnumerable<Player>>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
    {
        var players = await _playerRepository.GetByGameIdAsync(gameId, cancellationToken);
        return AppResult<IEnumerable<Player>>.Success(players);
    }
}
