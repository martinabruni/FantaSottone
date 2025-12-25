namespace Internal.FantaSottone.Business.Services;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Internal.FantaSottone.Domain.Services;

internal sealed class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IRuleRepository _ruleRepository;
    private readonly IRuleAssignmentRepository _ruleAssignmentRepository;

    public GameService(
        IGameRepository gameRepository,
        IPlayerRepository playerRepository,
        IRuleRepository ruleRepository,
        IRuleAssignmentRepository ruleAssignmentRepository)
    {
        _gameRepository = gameRepository;
        _playerRepository = playerRepository;
        _ruleRepository = ruleRepository;
        _ruleAssignmentRepository = ruleAssignmentRepository;
    }

    public async Task<AppResult<Game>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByIdAsync(id, cancellationToken);
        return game != null
            ? AppResult<Game>.Success(game)
            : AppResult<Game>.NotFound($"Game with ID {id} not found");
    }

    public async Task<AppResult<IEnumerable<Game>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var games = await _gameRepository.GetAllAsync(cancellationToken);
        return AppResult<IEnumerable<Game>>.Success(games);
    }

    public async Task<AppResult<Game>> CreateAsync(Game entity, CancellationToken cancellationToken = default)
    {
        await _gameRepository.AddAsync(entity, cancellationToken);
        await _gameRepository.SaveChangesAsync(cancellationToken);
        return AppResult<Game>.Created(entity);
    }

    public async Task<AppResult<Game>> UpdateAsync(Game entity, CancellationToken cancellationToken = default)
    {
        var existing = await _gameRepository.GetByIdAsync(entity.Id, cancellationToken);
        if (existing == null)
            return AppResult<Game>.NotFound($"Game with ID {entity.Id} not found");

        await _gameRepository.UpdateAsync(entity, cancellationToken);
        await _gameRepository.SaveChangesAsync(cancellationToken);
        return AppResult<Game>.Success(entity);
    }

    public async Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByIdAsync(id, cancellationToken);
        if (game == null)
            return AppResult.NotFound($"Game with ID {id} not found");

        await _gameRepository.DeleteAsync(game, cancellationToken);
        await _gameRepository.SaveChangesAsync(cancellationToken);
        return AppResult.Success();
    }

    public async Task<AppResult<IEnumerable<Player>>> GetLeaderboardAsync(int gameId, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
        if (game == null)
            return AppResult<IEnumerable<Player>>.NotFound($"Game with ID {gameId} not found");

        var leaderboard = await _playerRepository.GetLeaderboardAsync(gameId, cancellationToken);
        return AppResult<IEnumerable<Player>>.Success(leaderboard);
    }

    public async Task<AppResult<Game>> EndGameAsync(int gameId, int creatorPlayerId, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
        if (game == null)
            return AppResult<Game>.NotFound($"Game with ID {gameId} not found");

        if (game.Status == GameStatus.Ended)
            return AppResult<Game>.BadRequest("Game has already ended");

        // Check if requester is creator
        if (game.CreatorPlayerId != creatorPlayerId)
            return AppResult<Game>.Forbidden("Only the game creator can end the game");

        // Determine winner (highest score, tie-break by Id ASC)
        var players = await _playerRepository.GetLeaderboardAsync(gameId, cancellationToken);
        var playersList = players.ToList();

        if (playersList.Count == 0)
            return AppResult<Game>.BadRequest("Cannot end game with no players");

        var winner = playersList.First(); // Already ordered by score DESC, Id ASC

        game.Status = GameStatus.Ended;
        game.WinnerPlayerId = winner.Id;
        game.UpdatedAt = DateTime.UtcNow;

        await _gameRepository.UpdateAsync(game, cancellationToken);
        await _gameRepository.SaveChangesAsync(cancellationToken);

        return AppResult<Game>.Success(game);
    }

    public async Task<bool> ShouldEndGameAsync(int gameId, CancellationToken cancellationToken = default)
    {
        // Condition 1: All rules assigned
        var totalRules = await _ruleRepository.CountByGameIdAsync(gameId, cancellationToken);
        var assignedRules = await _ruleAssignmentRepository.CountByGameIdAsync(gameId, cancellationToken);

        if (totalRules > 0 && totalRules == assignedRules)
            return true;

        // Condition 2: 3 or more players with score <= 0
        var playersWithLowScore = await _playerRepository.CountPlayersWithScoreLessThanOrEqualToZeroAsync(gameId, cancellationToken);

        return playersWithLowScore >= 3;
    }

    public async Task<AppResult<Game>> TryAutoEndGameAsync(int gameId, CancellationToken cancellationToken = default)
    {
        var shouldEnd = await ShouldEndGameAsync(gameId, cancellationToken);
        if (!shouldEnd)
            return AppResult<Game>.BadRequest("Game end conditions not met");

        var game = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
        if (game == null)
            return AppResult<Game>.NotFound($"Game with ID {gameId} not found");

        if (game.Status == GameStatus.Ended)
            return AppResult<Game>.Success(game); // Already ended

        // Determine winner
        var players = await _playerRepository.GetLeaderboardAsync(gameId, cancellationToken);
        var playersList = players.ToList();

        if (playersList.Count == 0)
            return AppResult<Game>.BadRequest("Cannot end game with no players");

        var winner = playersList.First();

        game.Status = GameStatus.Ended;
        game.WinnerPlayerId = winner.Id;
        game.UpdatedAt = DateTime.UtcNow;

        await _gameRepository.UpdateAsync(game, cancellationToken);
        await _gameRepository.SaveChangesAsync(cancellationToken);

        return AppResult<Game>.Success(game);
    }
}
