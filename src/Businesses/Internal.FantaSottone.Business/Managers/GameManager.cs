// src/Businesses/Internal.FantaSottone.Business/Managers/GameManager.cs

namespace Internal.FantaSottone.Business.Managers;

using Internal.FantaSottone.Domain.Managers;
using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Mapster;
using Microsoft.Extensions.Logging;

internal sealed class GameManager : IGameManager
{
    private readonly IGameRepository _gameRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IRuleRepository _ruleRepository;
    private readonly IRuleAssignmentRepository _ruleAssignmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GameManager> _logger;

    public GameManager(
        IGameRepository gameRepository,
        IPlayerRepository playerRepository,
        IRuleRepository ruleRepository,
        IRuleAssignmentRepository ruleAssignmentRepository,
        IUserRepository userRepository,
        ILogger<GameManager> logger)
    {
        _gameRepository = gameRepository;
        _playerRepository = playerRepository;
        _ruleRepository = ruleRepository;
        _ruleAssignmentRepository = ruleAssignmentRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<AppResult<IEnumerable<Game>>> GetUserGamesAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all players for this user
            var allPlayers = await _playerRepository.GetAllAsync(cancellationToken);
            if (allPlayers.IsFailure)
                return AppResult<IEnumerable<Game>>.InternalServerError("Failed to retrieve players");

            var userPlayers = allPlayers.Value!.Where(p => p.UserId == userId);

            var games = new List<Game>();
            foreach (var player in userPlayers)
            {
                if (player.GameId.HasValue)
                {
                    var gameResult = await _gameRepository.GetByIdAsync(player.GameId.Value, cancellationToken);
                    if (gameResult.IsSuccess)
                    {
                        games.Add(gameResult.Value!);
                    }
                }
            }

            return AppResult<IEnumerable<Game>>.Success(games.Distinct());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving games for user {UserId}", userId);
            return AppResult<IEnumerable<Game>>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<(Game game, Player creatorPlayer, List<string> invitedEmails, List<string> invalidEmails)>> CreateGameWithEmailInvitesAsync(
        string name,
        int initialScore,
        int creatorUserId,
        List<string> invitedEmails,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate at least one invited email is provided (creator + 1 invited = 2 players minimum)
            if (invitedEmails == null || invitedEmails.Count == 0)
                return AppResult<(Game, Player, List<string>, List<string>)>.BadRequest("Almeno un altro giocatore deve essere invitato per creare una partita");

            // Validate creator user exists
            var creatorUserResult = await _userRepository.GetByIdAsync(creatorUserId, cancellationToken);
            if (creatorUserResult.IsFailure)
                return AppResult<(Game, Player, List<string>, List<string>)>.NotFound("Creator user not found");

            // Create game first
            var game = new Game
            {
                Name = name,
                InitialScore = initialScore,
                Status = GameStatus.Started,
                CreatorPlayerId = null, // Will be set after creating creator player
                WinnerPlayerId = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var addGameResult = await _gameRepository.AddAsync(game, cancellationToken);
            if (addGameResult.IsFailure)
                return AppResult<(Game, Player, List<string>, List<string>)>.InternalServerError(
                    $"Failed to create game: {addGameResult.Errors.FirstOrDefault()?.Message}");

            game = addGameResult.Value!;

            // Create creator player
            var creatorPlayer = new Player
            {
                GameId = game.Id,
                UserId = creatorUserId,
                IsCreator = true,
                CurrentScore = initialScore,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var addCreatorResult = await _playerRepository.AddAsync(creatorPlayer, cancellationToken);
            if (addCreatorResult.IsFailure)
            {
                await _gameRepository.DeleteAsync(game.Id, cancellationToken); // Cleanup
                return AppResult<(Game, Player, List<string>, List<string>)>.InternalServerError(
                    $"Failed to create creator player: {addCreatorResult.Errors.FirstOrDefault()?.Message}");
            }

            creatorPlayer = addCreatorResult.Value!;

            // Update game with creator player ID
            game.CreatorPlayerId = creatorPlayer.Id;
            await _gameRepository.UpdateAsync(game, cancellationToken);

            // Process email invitations
            var validEmails = new List<string>();
            var invalidEmails = new List<string>();

            foreach (var email in invitedEmails.Distinct())
            {
                // Skip creator's own email
                if (email.Equals(creatorUserResult.Value!.Email, StringComparison.OrdinalIgnoreCase))
                    continue;

                var userResult = await _userRepository.GetByEmailAsync(email, cancellationToken);
                if (userResult.IsFailure)
                {
                    invalidEmails.Add(email);
                    continue;
                }

                var user = userResult.Value!;

                // Check if user is already a player in this game
                var existsResult = await _playerRepository.ExistsAsync(game.Id, user.Id, cancellationToken);
                if (existsResult.IsSuccess && existsResult.Value!)
                {
                    validEmails.Add(email); // Already invited
                    continue;
                }

                // Create player for invited user
                var invitedPlayer = new Player
                {
                    GameId = game.Id,
                    UserId = user.Id,
                    IsCreator = false,
                    CurrentScore = initialScore,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var addPlayerResult = await _playerRepository.AddAsync(invitedPlayer, cancellationToken);
                if (addPlayerResult.IsSuccess)
                {
                    validEmails.Add(email);
                    _logger.LogInformation("User {UserId} ({Email}) invited to game {GameId}", user.Id, email, game.Id);
                }
                else
                {
                    invalidEmails.Add(email);
                    _logger.LogWarning("Failed to invite user {Email} to game {GameId}", email, game.Id);
                }
            }

            return AppResult<(Game, Player, List<string>, List<string>)>.Success((game, creatorPlayer, validEmails, invalidEmails));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating game with email invites");
            return AppResult<(Game, Player, List<string>, List<string>)>.InternalServerError($"Service error: {ex.Message}");
        }
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
            return await _gameRepository.DeleteAsync(id, cancellationToken);
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

    public async Task<AppResult<(Game game, Player winner, IEnumerable<Player> leaderboard)>> EndGameAsync(
        int gameId,
        int requestingUserId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var gameResult = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
            if (gameResult.IsFailure)
                return gameResult.Adapt<AppResult<(Game game, Player winner, IEnumerable<Player> leaderboard)>>();

            var game = gameResult.Value!;

            if (game.Status == GameStatus.Ended)
            {
                _logger.LogWarning("Attempt to end already ended game {GameId}", gameId);
                return AppResult<(Game game, Player winner, IEnumerable<Player> leaderboard)>.BadRequest("Game has already ended");
            }

            // FIX: Get the player for the requesting user in this game
            var requestingPlayerResult = await _playerRepository.GetByGameAndUserAsync(gameId, requestingUserId, cancellationToken);
            if (requestingPlayerResult.IsFailure)
            {
                _logger.LogWarning("User {UserId} attempted to end game {GameId} but is not a player", requestingUserId, gameId);
                return AppResult<(Game game, Player winner, IEnumerable<Player> leaderboard)>.Forbidden("You are not a player in this game");
            }

            var requestingPlayer = requestingPlayerResult.Value!;

            // FIX: Compare player IDs correctly
            if (game.CreatorPlayerId != requestingPlayer.Id)
            {
                _logger.LogWarning("Player {PlayerId} (User {UserId}) attempted to end game but is not creator",
                    requestingPlayer.Id, requestingUserId);
                return AppResult<(Game game, Player winner, IEnumerable<Player> leaderboard)>.Forbidden("Only the game creator can end the game");
            }

            var playersResult = await _playerRepository.GetLeaderboardAsync(gameId, cancellationToken);
            if (playersResult.IsFailure)
                return AppResult<(Game game, Player winner, IEnumerable<Player> leaderboard)>.InternalServerError("Failed to retrieve players for winner determination");

            var playersList = playersResult.Value!.ToList();

            if (playersList.Count == 0)
            {
                _logger.LogWarning("Attempt to end game {GameId} with no players", gameId);
                return AppResult<(Game game, Player winner, IEnumerable<Player> leaderboard)>.BadRequest("Cannot end game with no players");
            }

            var winner = playersList.First();

            game.Status = GameStatus.Ended;
            game.WinnerPlayerId = winner.Id;
            game.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Game {GameId} ended by user {UserId}, winner is player {PlayerId}",
                gameId, requestingUserId, winner.Id);
            var result = await _gameRepository.UpdateAsync(game, cancellationToken);
            if (result.IsFailure)
            {
                _logger.LogError("Failed to update game {GameId} after ending", gameId);
                return AppResult<(Game game, Player winner, IEnumerable<Player> leaderboard)>.InternalServerError("Failed to update game");
            }

            return AppResult<(Game game, Player winner, IEnumerable<Player> leaderboard)>.Success((game, winner, playersList));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error ending game {GameId}", gameId);
            return AppResult<(Game game, Player winner, IEnumerable<Player> leaderboard)>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<bool> ShouldEndGameAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if all rules have been assigned
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

            // Check if all players except one have score <= 0
            var allPlayersResult = await _playerRepository.GetByGameIdAsync(gameId, cancellationToken);
            if (allPlayersResult.IsSuccess)
            {
                var players = allPlayersResult.Value!.ToList();
                var playersWithPositiveScore = players.Count(p => p.CurrentScore > 0);

                // If only one player has a positive score (or all have <= 0), end the game
                if (playersWithPositiveScore <= 1)
                {
                    _logger.LogInformation("Game {GameId} should end: only {Count} player(s) with positive score",
                        gameId, playersWithPositiveScore);
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if game {GameId} should end", gameId);
            return false;
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
                return AppResult<Game>.Success(game);
            }

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

    public async Task<AppResult<Player>> InvitePlayerAsync(int gameId, int userId, int requestingUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            var gameResult = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
            if (gameResult.IsFailure)
                return AppResult<Player>.NotFound($"Game with ID {gameId} not found");

            var game = gameResult.Value!;

            var isCreatorResult = await _gameRepository.IsUserCreatorAsync(gameId, requestingUserId, cancellationToken);
            if (isCreatorResult.IsFailure || !isCreatorResult.Value!)
            {
                _logger.LogWarning("User {UserId} attempted to invite to game {GameId} but is not creator", requestingUserId, gameId);
                return AppResult<Player>.Forbidden("Only the game creator can invite players");
            }

            var userResult = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (userResult.IsFailure)
                return AppResult<Player>.NotFound($"User with ID {userId} not found");

            var existsResult = await _playerRepository.ExistsAsync(gameId, userId, cancellationToken);
            if (existsResult.IsSuccess && existsResult.Value!)
                return AppResult<Player>.Conflict("User is already a player in this game");

            var player = new Player
            {
                GameId = gameId,
                UserId = userId,
                IsCreator = false,
                CurrentScore = game.InitialScore,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("User {UserId} invited to game {GameId} by creator {CreatorId}", userId, gameId, requestingUserId);
            return await _playerRepository.AddAsync(player, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error inviting user {UserId} to game {GameId}", userId, gameId);
            return AppResult<Player>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<Player>> JoinGameAsync(int gameId, int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var gameResult = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
            if (gameResult.IsFailure)
                return AppResult<Player>.NotFound($"Game with ID {gameId} not found");

            var playerResult = await _playerRepository.GetByGameAndUserAsync(gameId, userId, cancellationToken);
            if (playerResult.IsFailure)
            {
                _logger.LogWarning("User {UserId} attempted to join game {GameId} but is not a player", userId, gameId);
                return AppResult<Player>.Forbidden("You are not a player in this game");
            }

            _logger.LogInformation("User {UserId} joined game {GameId}", userId, gameId);
            return playerResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error joining game {GameId} for user {UserId}", gameId, userId);
            return AppResult<Player>.InternalServerError($"Service error: {ex.Message}");
        }
    }
}