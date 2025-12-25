namespace Internal.FantaSottone.Business.Managers;

using Internal.FantaSottone.Domain.Managers;
using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;

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
            // Step 1: Validate input
            var creatorData = players.FirstOrDefault(p => p.IsCreator);
            if (creatorData == default)
                return AppResult<StartGameResult>.BadRequest("At least one player must be marked as creator");

            if (players.Count == 0)
                return AppResult<StartGameResult>.BadRequest("At least one player is required");

            // Step 2: Create the creator player first
            var creatorPlayer = new Player
            {
                Username = creatorData.Username,
                AccessCode = creatorData.AccessCode,
                IsCreator = true,
                CurrentScore = initialScore,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var addCreatorResult = await _playerRepository.AddAsync(creatorPlayer, cancellationToken);
            if (addCreatorResult.IsFailure)
                return AppResult<StartGameResult>.InternalServerError($"Failed to create creator player: {addCreatorResult.Errors.FirstOrDefault()?.Message}");

            creatorPlayer = addCreatorResult.Value;

            // Step 3: Create game with CreatorPlayerId set
            var game = new Game
            {
                Name = name,
                InitialScore = initialScore,
                Status = GameStatus.Started,
                CreatorPlayerId = creatorPlayer?.Id,
                WinnerPlayerId = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var addGameResult = await _gameRepository.AddAsync(game, cancellationToken);
            if (addGameResult.IsFailure)
                return AppResult<StartGameResult>.InternalServerError($"Failed to create game: {addGameResult.Errors.FirstOrDefault()?.Message}");

            game = addGameResult.Value;

            // Step 4: Update creator player with correct GameId
            creatorPlayer.GameId = game.Id;
            creatorPlayer.UpdatedAt = DateTime.UtcNow;

            var updateCreatorResult = await _playerRepository.UpdateAsync(creatorPlayer, cancellationToken);
            if (updateCreatorResult.IsFailure)
                return AppResult<StartGameResult>.InternalServerError($"Failed to update creator player: {updateCreatorResult.Errors.FirstOrDefault()?.Message}");

            // Step 5: Create other players
            var createdPlayers = new List<Player> { creatorPlayer };

            foreach (var playerData in players.Where(p => !p.IsCreator))
            {
                var player = new Player
                {
                    GameId = game.Id,
                    Username = playerData.Username,
                    AccessCode = playerData.AccessCode,
                    IsCreator = false,
                    CurrentScore = initialScore,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var addPlayerResult = await _playerRepository.AddAsync(player, cancellationToken);
                if (addPlayerResult.IsFailure)
                    return AppResult<StartGameResult>.InternalServerError($"Failed to create player {playerData.Username}: {addPlayerResult.Errors.FirstOrDefault()?.Message}");

                createdPlayers.Add(addPlayerResult.Value);
            }

            // Step 6: Create rules
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

                var addRuleResult = await _ruleRepository.AddAsync(rule, cancellationToken);
                if (addRuleResult.IsFailure)
                    return AppResult<StartGameResult>.InternalServerError($"Failed to create rule {ruleData.Name}: {addRuleResult.Errors.FirstOrDefault()?.Message}");
            }

            // Step 7: Prepare credentials response
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