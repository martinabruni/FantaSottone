namespace Internal.FantaSottone.Business.Managers;

using Internal.FantaSottone.Domain.Managers;
using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Internal.FantaSottone.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

internal sealed class GameManager : IGameManager
{
    private readonly IGameRepository _gameRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IRuleRepository _ruleRepository;
    private readonly FantaSottoneContext _context;
    private readonly ILogger<GameManager> _logger;

    public GameManager(
        IGameRepository gameRepository,
        IPlayerRepository playerRepository,
        IRuleRepository ruleRepository,
        FantaSottoneContext context,
        ILogger<GameManager> logger)
    {
        _gameRepository = gameRepository;
        _playerRepository = playerRepository;
        _ruleRepository = ruleRepository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// CRITICAL: Creates a complete game with players and rules in a single transaction
    /// </summary>
    public async Task<AppResult<StartGameResult>> StartGameAsync(
        string name,
        int initialScore,
        List<(string Username, string AccessCode, bool IsCreator)> players,
        List<(string Name, RuleType RuleType, int ScoreDelta)> rules,
        CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(name))
            {
                await transaction.RollbackAsync(cancellationToken);
                return AppResult<StartGameResult>.BadRequest("Game name is required");
            }

            if (players == null || players.Count == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return AppResult<StartGameResult>.BadRequest("At least one player is required");
            }

            if (rules == null || rules.Count == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return AppResult<StartGameResult>.BadRequest("At least one rule is required");
            }

            var creatorCount = players.Count(p => p.IsCreator);
            if (creatorCount != 1)
            {
                await transaction.RollbackAsync(cancellationToken);
                return AppResult<StartGameResult>.BadRequest("Exactly one creator player is required");
            }

            _logger.LogInformation(
                "Starting game '{GameName}' with {PlayerCount} players and {RuleCount} rules",
                name, players.Count, rules.Count);

            // 1. Create game in Draft status first
            var game = new Game
            {
                Name = name,
                InitialScore = initialScore,
                Status = GameStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var gameResult = await _gameRepository.AddAsync(game, cancellationToken);
            if (gameResult.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError("Failed to create game: {Errors}", string.Join(", ", gameResult.Errors.Select(e => e.Message)));
                return AppResult<StartGameResult>.BadRequest(gameResult.Errors);
            }

            game = gameResult.Value!;

            // 2. Create players
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

                var playerResult = await _playerRepository.AddAsync(player, cancellationToken);
                if (playerResult.IsFailure)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError("Failed to create player {Username}: {Errors}", 
                        playerData.Username, 
                        string.Join(", ", playerResult.Errors.Select(e => e.Message)));
                    
                    // Check if it's a duplicate username/access code conflict
                    if (playerResult.StatusCode == AppStatusCode.Conflict)
                    {
                        return AppResult<StartGameResult>.Conflict(
                            $"Player username '{playerData.Username}' or access code already exists in this game",
                            "DUPLICATE_PLAYER_CREDENTIALS");
                    }
                    
                    return AppResult<StartGameResult>.InternalServerError("Failed to create player");
                }

                var createdPlayer = playerResult.Value!;
                createdPlayers.Add(createdPlayer);

                if (playerData.IsCreator)
                    creatorPlayer = createdPlayer;
            }

            // 3. Update game with CreatorPlayerId
            if (creatorPlayer != null)
            {
                game.CreatorPlayerId = creatorPlayer.Id;
                game.Status = GameStatus.Started;
                game.UpdatedAt = DateTime.UtcNow;

                var updateGameResult = await _gameRepository.UpdateAsync(game, cancellationToken);
                if (updateGameResult.IsFailure)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError("Failed to update game with creator: {Errors}", 
                        string.Join(", ", updateGameResult.Errors.Select(e => e.Message)));
                    return AppResult<StartGameResult>.InternalServerError("Failed to update game with creator");
                }

                game = updateGameResult.Value!;
            }

            // 4. Create rules
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

                var ruleResult = await _ruleRepository.AddAsync(rule, cancellationToken);
                if (ruleResult.IsFailure)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError("Failed to create rule {RuleName}: {Errors}", 
                        ruleData.Name, 
                        string.Join(", ", ruleResult.Errors.Select(e => e.Message)));
                    
                    // Check if it's a duplicate rule name conflict
                    if (ruleResult.StatusCode == AppStatusCode.Conflict)
                    {
                        return AppResult<StartGameResult>.Conflict(
                            $"Rule name '{ruleData.Name}' already exists in this game",
                            "DUPLICATE_RULE_NAME");
                    }
                    
                    return AppResult<StartGameResult>.InternalServerError("Failed to create rule");
                }
            }

            // 5. Commit transaction
            await transaction.CommitAsync(cancellationToken);

            // 6. Prepare credentials response
            var credentials = players.Select(p => (p.Username, p.AccessCode, p.IsCreator)).ToList();

            var result = new StartGameResult
            {
                GameId = game.Id,
                Credentials = credentials
            };

            _logger.LogInformation(
                "Game {GameId} ({GameName}) started successfully with {PlayerCount} players and {RuleCount} rules",
                game.Id, game.Name, players.Count, rules.Count);

            return AppResult<StartGameResult>.Created(result);
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Database error while starting game '{GameName}'", name);
            
            // Try to provide more specific error based on constraint violation
            if (ex.InnerException?.Message.Contains("UX_PlayerEntity_GameId_Username") == true)
            {
                return AppResult<StartGameResult>.Conflict("Duplicate player username in game", "DUPLICATE_USERNAME");
            }
            if (ex.InnerException?.Message.Contains("UX_PlayerEntity_GameId_AccessCode") == true)
            {
                return AppResult<StartGameResult>.Conflict("Duplicate player access code in game", "DUPLICATE_ACCESS_CODE");
            }
            if (ex.InnerException?.Message.Contains("UX_RuleEntity_GameId_Name") == true)
            {
                return AppResult<StartGameResult>.Conflict("Duplicate rule name in game", "DUPLICATE_RULE_NAME");
            }
            
            return AppResult<StartGameResult>.InternalServerError($"Database error: {ex.Message}");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Unexpected error while starting game '{GameName}'", name);
            return AppResult<StartGameResult>.InternalServerError($"Failed to create game: {ex.Message}");
        }
    }
}
