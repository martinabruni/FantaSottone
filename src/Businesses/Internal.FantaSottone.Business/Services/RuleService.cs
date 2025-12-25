namespace Internal.FantaSottone.Business.Services;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Internal.FantaSottone.Domain.Services;
using Microsoft.Extensions.Logging;

internal sealed class RuleService : IRuleService
{
    private readonly IRuleRepository _ruleRepository;
    private readonly IRuleAssignmentRepository _ruleAssignmentRepository;
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<RuleService> _logger;

    public RuleService(
        IRuleRepository ruleRepository,
        IRuleAssignmentRepository ruleAssignmentRepository,
        IGameRepository gameRepository,
        ILogger<RuleService> logger)
    {
        _ruleRepository = ruleRepository;
        _ruleAssignmentRepository = ruleAssignmentRepository;
        _gameRepository = gameRepository;
        _logger = logger;
    }

    public async Task<AppResult<Rule>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _ruleRepository.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error getting rule {RuleId}", id);
            return AppResult<Rule>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<IEnumerable<Rule>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _ruleRepository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error getting all rules");
            return AppResult<IEnumerable<Rule>>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<Rule>> CreateAsync(Rule entity, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _ruleRepository.AddAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error creating rule");
            return AppResult<Rule>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<Rule>> UpdateAsync(Rule entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingResult = await _ruleRepository.GetByIdAsync(entity.Id, cancellationToken);
            if (existingResult.IsFailure)
                return existingResult;

            return await _ruleRepository.UpdateAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error updating rule {RuleId}", entity.Id);
            return AppResult<Rule>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var ruleResult = await _ruleRepository.GetByIdAsync(id, cancellationToken);
            if (ruleResult.IsFailure)
                return AppResult.NotFound(ruleResult.Errors.First().Message);

            return await _ruleRepository.DeleteAsync(ruleResult.Value!, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error deleting rule {RuleId}", id);
            return AppResult.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<IEnumerable<(Rule rule, RuleAssignment? assignment)>>> GetRulesWithAssignmentsAsync(
        int gameId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify game exists
            var gameResult = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
            if (gameResult.IsFailure)
                return AppResult<IEnumerable<(Rule rule, RuleAssignment? assignment)>>.NotFound($"Game with ID {gameId} not found");

            // Get all rules for game
            var rulesResult = await _ruleRepository.GetByGameIdAsync(gameId, cancellationToken);
            if (rulesResult.IsFailure)
                return AppResult<IEnumerable<(Rule rule, RuleAssignment? assignment)>>.InternalServerError("Failed to retrieve rules");

            var rulesList = rulesResult.Value!.ToList();
            var result = new List<(Rule rule, RuleAssignment? assignment)>();

            // For each rule, get its assignment (if any)
            foreach (var rule in rulesList)
            {
                var assignmentResult = await _ruleAssignmentRepository.GetByRuleIdAsync(rule.Id, cancellationToken);
                
                // If NotFound, that's OK - the rule is not assigned yet
                var assignment = assignmentResult.IsSuccess ? assignmentResult.Value : null;
                result.Add((rule, assignment));
            }

            return AppResult<IEnumerable<(Rule rule, RuleAssignment? assignment)>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error getting rules with assignments for game {GameId}", gameId);
            return AppResult<IEnumerable<(Rule rule, RuleAssignment? assignment)>>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<IEnumerable<Rule>>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _ruleRepository.GetByGameIdAsync(gameId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error getting rules for game {GameId}", gameId);
            return AppResult<IEnumerable<Rule>>.InternalServerError($"Service error: {ex.Message}");
        }
    }

    public async Task<AppResult<Rule>> UpdateRuleAsync(
        int ruleId, 
        int gameId, 
        int creatorPlayerId, 
        string name, 
        RuleType ruleType, 
        int scoreDelta, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify rule exists
            var ruleResult = await _ruleRepository.GetByIdAsync(ruleId, cancellationToken);
            if (ruleResult.IsFailure)
                return ruleResult;

            // Verify game exists
            var gameResult = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
            if (gameResult.IsFailure)
                return AppResult<Rule>.NotFound($"Game with ID {gameId} not found");

            var game = gameResult.Value!;

            // Check if requester is creator
            if (game.CreatorPlayerId != creatorPlayerId)
            {
                _logger.LogWarning("Player {PlayerId} attempted to modify rule but is not creator", creatorPlayerId);
                return AppResult<Rule>.Forbidden("Only the game creator can modify rules");
            }

            // Check if rule is already assigned
            var isAssignedResult = await _ruleAssignmentRepository.IsRuleAssignedAsync(ruleId, cancellationToken);
            if (isAssignedResult.IsFailure)
                return AppResult<Rule>.InternalServerError("Failed to check rule assignment status");

            if (isAssignedResult.Value!)
            {
                _logger.LogWarning("Attempt to modify already assigned rule {RuleId}", ruleId);
                return AppResult<Rule>.Conflict("Cannot modify a rule that has already been assigned", "RULE_ALREADY_ASSIGNED");
            }

            // Update rule
            var rule = ruleResult.Value!;
            rule.Name = name;
            rule.RuleType = ruleType;
            rule.ScoreDelta = scoreDelta;
            rule.UpdatedAt = DateTime.UtcNow;

            return await _ruleRepository.UpdateAsync(rule, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error updating rule {RuleId}", ruleId);
            return AppResult<Rule>.InternalServerError($"Service error: {ex.Message}");
        }
    }
}
