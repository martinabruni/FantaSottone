namespace Internal.FantaSottone.Business.Services;

using Internal.FantaSottone.Domain.Managers;
using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Internal.FantaSottone.Domain.Services;
using Internal.FantaSottone.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

internal sealed class RuleAssignmentService : IRuleAssignmentService
{
    private readonly IRuleAssignmentRepository _ruleAssignmentRepository;
    private readonly IRuleRepository _ruleRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IGameManager _gameManager;
    private readonly FantaSottoneContext _context;
    private readonly ILogger<RuleAssignmentService> _logger;

    public RuleAssignmentService(
        IRuleAssignmentRepository ruleAssignmentRepository,
        IRuleRepository ruleRepository,
        IPlayerRepository playerRepository,
        IGameRepository gameRepository,
        IGameManager gameManager,
        FantaSottoneContext context,
        ILogger<RuleAssignmentService> logger)
    {
        _ruleAssignmentRepository = ruleAssignmentRepository;
        _ruleRepository = ruleRepository;
        _playerRepository = playerRepository;
        _gameRepository = gameRepository;
        _gameManager = gameManager;
        _context = context;
        _logger = logger;
    }

    public async Task<AppResult<RuleAssignment>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _ruleAssignmentRepository.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error getting rule assignment {AssignmentId}", id);
            return AppResult<RuleAssignment>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<IEnumerable<RuleAssignment>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _ruleAssignmentRepository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error getting all rule assignments");
            return AppResult<IEnumerable<RuleAssignment>>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<RuleAssignment>> CreateAsync(RuleAssignment entity, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _ruleAssignmentRepository.AddAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error creating rule assignment");
            return AppResult<RuleAssignment>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<RuleAssignment>> UpdateAsync(RuleAssignment entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingResult = await _ruleAssignmentRepository.GetByIdAsync(entity.Id, cancellationToken);
            if (existingResult.IsFailure)
                return existingResult;

            return await _ruleAssignmentRepository.UpdateAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error updating rule assignment {AssignmentId}", entity.Id);
            return AppResult<RuleAssignment>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _ruleAssignmentRepository.DeleteAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error deleting rule assignment {AssignmentId}", id);
            return AppResult.InternalServerError($"Service error: {ex.Message}");
        }
    }

    /// <summary>
    /// CRITICAL: Atomically assigns a rule to a player (LA PRIMA CHE mechanism)
    /// Uses transaction to ensure player score update and assignment creation are atomic
    /// </summary>
    public async Task<AppResult<RuleAssignment>> AssignRuleAsync(
        int ruleId,
        int gameId,
        int playerId,
        CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Verify game exists
            var gameResult = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
            if (gameResult.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return AppResult<RuleAssignment>.NotFound($"Game with ID {gameId} not found");
            }

            var game = gameResult.Value!;

            // 1.1 Verify game is not in Draft state - rules cannot be assigned in draft
            if (game.Status == GameStatus.Draft)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogWarning("Attempt to assign rule in game {GameId} which is in Draft status", gameId);
                return AppResult<RuleAssignment>.BadRequest("Le regole non possono essere assegnate quando la partita è in stato bozza. Avvia la partita prima di assegnare regole.");
            }

            // 2. Verify rule exists and belongs to this game
            var ruleResult = await _ruleRepository.GetByIdAsync(ruleId, cancellationToken);
            if (ruleResult.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return AppResult<RuleAssignment>.NotFound($"Rule with ID {ruleId} not found");
            }

            var rule = ruleResult.Value!;
            if (rule.GameId != gameId)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogWarning("Rule {RuleId} does not belong to game {GameId}", ruleId, gameId);
                return AppResult<RuleAssignment>.BadRequest("Rule does not belong to this game");
            }

            // 3. Verify player exists and belongs to this game
            var playerResult = await _playerRepository.GetByIdAsync(playerId, cancellationToken);
            if (playerResult.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return AppResult<RuleAssignment>.NotFound($"Player with ID {playerId} not found");
            }

            var player = playerResult.Value!;
            if (player.GameId != gameId)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogWarning("Player {PlayerId} does not belong to game {GameId}", playerId, gameId);
                return AppResult<RuleAssignment>.BadRequest("Player does not belong to this game");
            }

            // 4. Check if rule already assigned (this is also done atomically by DB constraint, but we check early for better UX)
            var existingAssignmentResult = await _ruleAssignmentRepository.GetByRuleIdAsync(ruleId, cancellationToken);
            if (existingAssignmentResult.IsSuccess)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogWarning("Rule {RuleId} already assigned (early check)", ruleId);
                return AppResult<RuleAssignment>.Conflict($"Rule '{rule.Name}' has already been assigned", "RULE_ALREADY_ASSIGNED");
            }

            // 5. Update player score
            player.CurrentScore += rule.ScoreDelta;
            player.UpdatedAt = DateTime.UtcNow;

            var updatePlayerResult = await _playerRepository.UpdateAsync(player, cancellationToken);
            if (updatePlayerResult.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError("Failed to update player {PlayerId} score", playerId);
                return AppResult<RuleAssignment>.InternalServerError("Failed to update player score");
            }

            // 6. Create assignment (ATOMIC via unique constraint on RuleId)
            var assignment = new RuleAssignment
            {
                RuleId = ruleId,
                GameId = gameId,
                AssignedToPlayerId = playerId,
                ScoreDeltaApplied = rule.ScoreDelta,
                AssignedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var assignResult = await _ruleAssignmentRepository.AddAsync(assignment, cancellationToken);

            if (assignResult.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);

                // If it's a conflict (409), this is the "La prima che" in action
                if (assignResult.StatusCode == AppStatusCode.Conflict)
                {
                    _logger.LogWarning("Race condition detected: Rule {RuleId} was assigned by another player simultaneously", ruleId);
                    return assignResult; // Return the conflict as-is
                }

                _logger.LogError("Failed to create assignment for rule {RuleId}", ruleId);
                return assignResult;
            }

            // 7. Commit transaction
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Rule {RuleId} ({RuleName}) assigned to player {PlayerId} in game {GameId}. Score delta: {ScoreDelta}, New score: {NewScore}",
                ruleId, rule.Name, playerId, gameId, rule.ScoreDelta, player.CurrentScore);

            // 8. Check if game should auto-end after this assignment
            try
            {
                var shouldAutoEnd = await _gameManager.ShouldEndGameAsync(gameId, cancellationToken);
                if (shouldAutoEnd)
                {
                    _logger.LogInformation("Game {GameId} meets auto-end conditions after rule assignment", gameId);
                    var autoEndResult = await _gameManager.TryAutoEndGameAsync(gameId, cancellationToken);
                    if (autoEndResult.IsSuccess)
                    {
                        _logger.LogInformation("Game {GameId} automatically ended", gameId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to auto-end game {GameId}: {Error}",
                            gameId, autoEndResult.Errors.FirstOrDefault()?.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                // Don't fail the assignment if auto-end fails
                _logger.LogError(ex, "Error checking/executing auto-end for game {GameId}", gameId);
            }

            return assignResult;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Service error assigning rule {RuleId} to player {PlayerId} in game {GameId}", ruleId, playerId, gameId);
            return AppResult<RuleAssignment>.InternalServerError($"Service error during rule assignment: {ex.Message}");
        }
    }

    public async Task<AppResult<IEnumerable<RuleAssignment>>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify game exists
            var gameResult = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
            if (gameResult.IsFailure)
                return AppResult<IEnumerable<RuleAssignment>>.NotFound($"Game with ID {gameId} not found");

            return await _ruleAssignmentRepository.GetByGameIdAsync(gameId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error getting assignments for game {GameId}", gameId);
            return AppResult<IEnumerable<RuleAssignment>>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<bool> IsRuleAssignedAsync(int ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _ruleAssignmentRepository.IsRuleAssignedAsync(ruleId, cancellationToken);
            return result.IsSuccess && result.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error checking if rule {RuleId} is assigned", ruleId);
            return false; // Safe default
        }
    }
}
