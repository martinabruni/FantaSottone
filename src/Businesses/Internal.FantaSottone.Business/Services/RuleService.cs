namespace Internal.FantaSottone.Business.Services;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Internal.FantaSottone.Domain.Services;

internal sealed class RuleService : IRuleService
{
    private readonly IRuleRepository _ruleRepository;
    private readonly IRuleAssignmentRepository _ruleAssignmentRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IPlayerRepository _playerRepository;

    public RuleService(
        IRuleRepository ruleRepository,
        IRuleAssignmentRepository ruleAssignmentRepository,
        IGameRepository gameRepository,
        IPlayerRepository playerRepository)
    {
        _ruleRepository = ruleRepository;
        _ruleAssignmentRepository = ruleAssignmentRepository;
        _gameRepository = gameRepository;
        _playerRepository = playerRepository;
    }

    public async Task<AppResult<Rule>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var ruleResult = await _ruleRepository.GetByIdAsync(id, cancellationToken);
        if (ruleResult.IsFailure)
            return AppResult<Rule>.NotFound($"Rule with ID {id} not found");

        return AppResult<Rule>.Success(ruleResult.Value!);
    }

    public async Task<AppResult<IEnumerable<Rule>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rulesResult = await _ruleRepository.GetAllAsync(cancellationToken);
        if (rulesResult.IsFailure)
            return AppResult<IEnumerable<Rule>>.InternalServerError("Failed to retrieve rules");

        return AppResult<IEnumerable<Rule>>.Success(rulesResult.Value!);
    }

    public async Task<AppResult<Rule>> CreateAsync(Rule entity, CancellationToken cancellationToken = default)
    {
        var addResult = await _ruleRepository.AddAsync(entity, cancellationToken);
        if (addResult.IsFailure)
            return AppResult<Rule>.InternalServerError($"Failed to create rule: {addResult.Errors.FirstOrDefault()?.Message}");

        return AppResult<Rule>.Created(addResult.Value!);
    }

    public async Task<AppResult<Rule>> UpdateAsync(Rule entity, CancellationToken cancellationToken = default)
    {
        var existingResult = await _ruleRepository.GetByIdAsync(entity.Id, cancellationToken);
        if (existingResult.IsFailure)
            return AppResult<Rule>.NotFound($"Rule with ID {entity.Id} not found");

        var updateResult = await _ruleRepository.UpdateAsync(entity, cancellationToken);
        if (updateResult.IsFailure)
            return AppResult<Rule>.InternalServerError($"Failed to update rule: {updateResult.Errors.FirstOrDefault()?.Message}");

        return AppResult<Rule>.Success(updateResult.Value!);
    }

    public async Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var deleteResult = await _ruleRepository.DeleteAsync(id, cancellationToken);
        if (deleteResult.IsFailure)
            return AppResult.InternalServerError($"Failed to delete rule: {deleteResult.Errors.FirstOrDefault()?.Message}");

        return AppResult.Success();
    }

    public async Task<AppResult<IEnumerable<(Rule rule, RuleAssignment? assignment)>>> GetRulesWithAssignmentsAsync(int gameId, CancellationToken cancellationToken = default)
    {
        var gameResult = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
        if (gameResult.IsFailure)
            return AppResult<IEnumerable<(Rule rule, RuleAssignment? assignment)>>.NotFound($"Game with ID {gameId} not found");

        var rulesResult = await _ruleRepository.GetByGameIdAsync(gameId, cancellationToken);
        if (rulesResult.IsFailure)
            return AppResult<IEnumerable<(Rule rule, RuleAssignment? assignment)>>.InternalServerError("Failed to retrieve rules");

        var rulesList = rulesResult.Value!.ToList();
        var result = new List<(Rule rule, RuleAssignment? assignment)>();

        foreach (var rule in rulesList)
        {
            var assignmentResult = await _ruleAssignmentRepository.GetByRuleIdAsync(rule.Id, cancellationToken);
            var assignment = assignmentResult.IsSuccess ? assignmentResult.Value : null;
            result.Add((rule, assignment));
        }

        return AppResult<IEnumerable<(Rule rule, RuleAssignment? assignment)>>.Success(result);
    }

    public async Task<AppResult<IEnumerable<Rule>>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
    {
        var rulesResult = await _ruleRepository.GetByGameIdAsync(gameId, cancellationToken);
        if (rulesResult.IsFailure)
            return AppResult<IEnumerable<Rule>>.InternalServerError("Failed to retrieve rules");

        return AppResult<IEnumerable<Rule>>.Success(rulesResult.Value!);
    }

    /// <summary>
    /// Updates a rule if not yet assigned (creator only)
    /// </summary>
    /// <param name="ruleId">The rule ID to update</param>
    /// <param name="gameId">The game ID</param>
    /// <param name="requestingPlayerId">The Player.Id of the requesting user (NOT UserId!)</param>
    /// <param name="name">New rule name</param>
    /// <param name="ruleType">New rule type</param>
    /// <param name="scoreDelta">New score delta</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<AppResult<Rule>> UpdateRuleAsync(int ruleId, int gameId, int requestingPlayerId, string name, RuleType ruleType, int scoreDelta, CancellationToken cancellationToken = default)
    {
        var ruleResult = await _ruleRepository.GetByIdAsync(ruleId, cancellationToken);
        if (ruleResult.IsFailure)
            return AppResult<Rule>.NotFound($"Rule with ID {ruleId} not found");

        var gameResult = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
        if (gameResult.IsFailure)
            return AppResult<Rule>.NotFound($"Game with ID {gameId} not found");

        // Check if requester is creator
        // IMPORTANT: game.CreatorPlayerId is a Player.Id, so we compare with requestingPlayerId (also Player.Id)
        var game = gameResult.Value!;
        if (game.CreatorPlayerId != requestingPlayerId)
            return AppResult<Rule>.Forbidden("Only the game creator can modify rules");

        // Check if rule is already assigned
        var isAssignedResult = await _ruleAssignmentRepository.IsRuleAssignedAsync(ruleId, cancellationToken);
        if (isAssignedResult.IsSuccess && isAssignedResult.Value)
            return AppResult<Rule>.Conflict("Cannot modify a rule that has already been assigned", "RULE_ALREADY_ASSIGNED");

        var rule = ruleResult.Value!;
        rule.Name = name;
        rule.RuleType = ruleType;
        rule.ScoreDelta = scoreDelta;
        rule.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _ruleRepository.UpdateAsync(rule, cancellationToken);
        if (updateResult.IsFailure)
            return AppResult<Rule>.InternalServerError($"Failed to update rule: {updateResult.Errors.FirstOrDefault()?.Message}");

        return AppResult<Rule>.Success(updateResult.Value!);
    }

    /// <summary>
    /// Creates a new rule for a game (creator only)
    /// </summary>
    /// <param name="gameId">The game ID</param>
    /// <param name="requestingPlayerId">The Player.Id of the requesting user (NOT UserId!)</param>
    /// <param name="name">Rule name</param>
    /// <param name="ruleType">Rule type (Bonus/Malus)</param>
    /// <param name="scoreDelta">Score delta</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<AppResult<Rule>> CreateRuleAsync(int gameId, int requestingPlayerId, string name, RuleType ruleType, int scoreDelta, CancellationToken cancellationToken = default)
    {
        var gameResult = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
        if (gameResult.IsFailure)
            return AppResult<Rule>.NotFound($"Game with ID {gameId} not found");

        // Check if requester is creator
        // IMPORTANT: game.CreatorPlayerId is a Player.Id, so we compare with requestingPlayerId (also Player.Id)
        var game = gameResult.Value!;
        if (game.CreatorPlayerId != requestingPlayerId)
            return AppResult<Rule>.Forbidden("Only the game creator can create rules");

        var rule = new Rule
        {
            GameId = gameId,
            Name = name,
            RuleType = ruleType,
            ScoreDelta = scoreDelta,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var addResult = await _ruleRepository.AddAsync(rule, cancellationToken);
        if (addResult.IsFailure)
            return AppResult<Rule>.InternalServerError($"Failed to create rule: {addResult.Errors.FirstOrDefault()?.Message}");

        return AppResult<Rule>.Created(addResult.Value!);
    }

    /// <summary>
    /// Deletes a rule if not yet assigned (creator only)
    /// </summary>
    /// <param name="ruleId">The rule ID to delete</param>
    /// <param name="gameId">The game ID</param>
    /// <param name="requestingPlayerId">The Player.Id of the requesting user (NOT UserId!)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<AppResult> DeleteRuleAsync(int ruleId, int gameId, int requestingPlayerId, CancellationToken cancellationToken = default)
    {
        var ruleResult = await _ruleRepository.GetByIdAsync(ruleId, cancellationToken);
        if (ruleResult.IsFailure)
            return AppResult.NotFound($"Rule with ID {ruleId} not found");

        var rule = ruleResult.Value!;
        if (rule.GameId != gameId)
            return AppResult.BadRequest("Rule does not belong to this game");

        var gameResult = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
        if (gameResult.IsFailure)
            return AppResult.NotFound($"Game with ID {gameId} not found");

        // Check if requester is creator
        // IMPORTANT: game.CreatorPlayerId is a Player.Id, so we compare with requestingPlayerId (also Player.Id)
        var game = gameResult.Value!;
        if (game.CreatorPlayerId != requestingPlayerId)
            return AppResult.Forbidden("Only the game creator can delete rules");

        // Check if rule is already assigned
        var isAssignedResult = await _ruleAssignmentRepository.IsRuleAssignedAsync(ruleId, cancellationToken);
        if (isAssignedResult.IsSuccess && isAssignedResult.Value)
            return AppResult.Conflict("Cannot delete a rule that has already been assigned", "RULE_ALREADY_ASSIGNED");

        var deleteResult = await _ruleRepository.DeleteAsync(ruleId, cancellationToken);
        if (deleteResult.IsFailure)
            return AppResult.InternalServerError($"Failed to delete rule: {deleteResult.Errors.FirstOrDefault()?.Message}");

        return AppResult.Success();
    }
}