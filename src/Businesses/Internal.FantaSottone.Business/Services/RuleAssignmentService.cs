namespace Internal.FantaSottone.Business.Services;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Internal.FantaSottone.Domain.Services;
using Microsoft.EntityFrameworkCore;

internal sealed class RuleAssignmentService : IRuleAssignmentService
{
    private readonly IRuleAssignmentRepository _ruleAssignmentRepository;
    private readonly IRuleRepository _ruleRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IGameRepository _gameRepository;

    public RuleAssignmentService(
        IRuleAssignmentRepository ruleAssignmentRepository,
        IRuleRepository ruleRepository,
        IPlayerRepository playerRepository,
        IGameRepository gameRepository)
    {
        _ruleAssignmentRepository = ruleAssignmentRepository;
        _ruleRepository = ruleRepository;
        _playerRepository = playerRepository;
        _gameRepository = gameRepository;
    }

    public async Task<AppResult<RuleAssignment>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var assignment = await _ruleAssignmentRepository.GetByIdAsync(id, cancellationToken);
        return assignment != null
            ? AppResult<RuleAssignment>.Success(assignment)
            : AppResult<RuleAssignment>.NotFound($"RuleAssignment with ID {id} not found");
    }

    public async Task<AppResult<IEnumerable<RuleAssignment>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var assignments = await _ruleAssignmentRepository.GetAllAsync(cancellationToken);
        return AppResult<IEnumerable<RuleAssignment>>.Success(assignments);
    }

    public async Task<AppResult<RuleAssignment>> CreateAsync(RuleAssignment entity, CancellationToken cancellationToken = default)
    {
        await _ruleAssignmentRepository.AddAsync(entity, cancellationToken);
        await _ruleAssignmentRepository.SaveChangesAsync(cancellationToken);
        return AppResult<RuleAssignment>.Created(entity);
    }

    public async Task<AppResult<RuleAssignment>> UpdateAsync(RuleAssignment entity, CancellationToken cancellationToken = default)
    {
        var existing = await _ruleAssignmentRepository.GetByIdAsync(entity.Id, cancellationToken);
        if (existing == null)
            return AppResult<RuleAssignment>.NotFound($"RuleAssignment with ID {entity.Id} not found");

        await _ruleAssignmentRepository.UpdateAsync(entity, cancellationToken);
        await _ruleAssignmentRepository.SaveChangesAsync(cancellationToken);
        return AppResult<RuleAssignment>.Success(entity);
    }

    public async Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var assignment = await _ruleAssignmentRepository.GetByIdAsync(id, cancellationToken);
        if (assignment == null)
            return AppResult.NotFound($"RuleAssignment with ID {id} not found");

        await _ruleAssignmentRepository.DeleteAsync(assignment, cancellationToken);
        await _ruleAssignmentRepository.SaveChangesAsync(cancellationToken);
        return AppResult.Success();
    }

    public async Task<AppResult<RuleAssignment>> AssignRuleAsync(int ruleId, int gameId, int playerId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify game exists
            var game = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
            if (game == null)
                return AppResult<RuleAssignment>.NotFound($"Game with ID {gameId} not found");

            // Verify rule exists and belongs to this game
            var rule = await _ruleRepository.GetByIdAsync(ruleId, cancellationToken);
            if (rule == null)
                return AppResult<RuleAssignment>.NotFound($"Rule with ID {ruleId} not found");

            if (rule.GameId != gameId)
                return AppResult<RuleAssignment>.BadRequest("Rule does not belong to this game");

            // Verify player exists and belongs to this game
            var player = await _playerRepository.GetByIdAsync(playerId, cancellationToken);
            if (player == null)
                return AppResult<RuleAssignment>.NotFound($"Player with ID {playerId} not found");

            if (player.GameId != gameId)
                return AppResult<RuleAssignment>.BadRequest("Player does not belong to this game");

            // Check if rule already assigned (atomic check)
            var existingAssignment = await _ruleAssignmentRepository.GetByRuleIdAsync(ruleId, cancellationToken);
            if (existingAssignment != null)
                return AppResult<RuleAssignment>.Conflict($"Rule '{rule.Name}' has already been assigned", "RULE_ALREADY_ASSIGNED");

            // Create assignment
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

            // Update player score
            player.CurrentScore += rule.ScoreDelta;
            player.UpdatedAt = DateTime.UtcNow;

            await _ruleAssignmentRepository.AddAsync(assignment, cancellationToken);
            await _playerRepository.UpdateAsync(player, cancellationToken);
            await _ruleAssignmentRepository.SaveChangesAsync(cancellationToken);

            return AppResult<RuleAssignment>.Success(assignment);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UX_RuleAssignmentEntity_RuleId") == true)
        {
            // Catch unique constraint violation (race condition)
            return AppResult<RuleAssignment>.Conflict("Rule has already been assigned by another player", "RULE_ALREADY_ASSIGNED");
        }
    }

    public async Task<AppResult<IEnumerable<RuleAssignment>>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
        if (game == null)
            return AppResult<IEnumerable<RuleAssignment>>.NotFound($"Game with ID {gameId} not found");

        var assignments = await _ruleAssignmentRepository.GetByGameIdAsync(gameId, cancellationToken);
        return AppResult<IEnumerable<RuleAssignment>>.Success(assignments);
    }

    public async Task<bool> IsRuleAssignedAsync(int ruleId, CancellationToken cancellationToken = default)
    {
        return await _ruleAssignmentRepository.IsRuleAssignedAsync(ruleId, cancellationToken);
    }
}
