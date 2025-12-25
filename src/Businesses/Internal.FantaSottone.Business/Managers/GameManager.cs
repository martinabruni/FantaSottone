namespace Internal.FantaSottone.Business.Managers;

using Internal.FantaSottone.Domain.Managers;
using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Internal.FantaSottone.Domain.Services;

internal sealed class GameManager : IGameManager
{
    private readonly IGameRepository _gameRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IRuleRepository _ruleRepository;

    public GameManager(
        IGameRepository gameRepository,
        IPlayerRepository playerRepository,
        IRuleRepository ruleRepository)
    {
        _gameRepository = gameRepository;
        _playerRepository = playerRepository;
        _ruleRepository = ruleRepository;
    }

    public async Task<AppResult<StartGameResult>> StartGameAsync(
        string name,
        int initialScore,
        List<(string Username, string AccessCode, bool IsCreator)> players,
        List<(string Name, RuleType RuleType, int ScoreDelta)> rules,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create game in Draft status first
            var game = new Game
            {
                Name = name,
                InitialScore = initialScore,
                Status = GameStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _gameRepository.AddAsync(game, cancellationToken);
            await _gameRepository.SaveChangesAsync(cancellationToken);

            // Create players
            var createdPlayers = new List<Player>();
            Player? creatorPlayer = null;

            foreach (var playerData in players)
            {
                var player = new Player
                {
                    GameId = game.Id,
                    Username = playerData.Username,
                    AccessCode = playerData.AccessCode,
                    IsCreator = playerData.IsCreator,
                    CurrentScore = initialScore,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _playerRepository.AddAsync(player, cancellationToken);
                createdPlayers.Add(player);

                if (playerData.IsCreator)
                    creatorPlayer = player;
            }

            await _playerRepository.SaveChangesAsync(cancellationToken);

            // Update game with CreatorPlayerId
            if (creatorPlayer != null)
            {
                game.CreatorPlayerId = creatorPlayer.Id;
            }

            // Transition to Started status
            game.Status = GameStatus.Started;
            game.UpdatedAt = DateTime.UtcNow;

            await _gameRepository.UpdateAsync(game, cancellationToken);
            await _gameRepository.SaveChangesAsync(cancellationToken);

            // Create rules
            foreach (var ruleData in rules)
            {
                var rule = new Rule
                {
                    GameId = game.Id,
                    Name = ruleData.Name,
                    RuleType = ruleData.RuleType,
                    ScoreDelta = ruleData.ScoreDelta,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _ruleRepository.AddAsync(rule, cancellationToken);
            }

            await _ruleRepository.SaveChangesAsync(cancellationToken);

            // Prepare credentials response
            var credentials = players.Select(p => (p.Username, p.AccessCode, p.IsCreator)).ToList();

            var result = new StartGameResult
            {
                GameId = game.Id,
                Credentials = credentials
            };

            return AppResult<StartGameResult>.Created(result);
        }
        catch (Exception ex)
        {
            return AppResult<StartGameResult>.InternalServerError($"Failed to create game: {ex.Message}");
        }
    }
}
