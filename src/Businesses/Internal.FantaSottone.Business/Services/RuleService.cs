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
        var rule = await _ruleRepository.GetByIdAsync(id, cancellationToken);
        return rule != null
            ? AppResult<Rule>.Success(rule)
            : AppResult<Rule>.NotFound($"Rule with ID {id} not found");
    }

    public async Task<AppResult<IEnumerable<Rule>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rules = await _ruleRepository.GetAllAsync(cancellationToken);
        return AppResult<IEnumerable<Rule>>.Success(rules);
    }

    public async Task<AppResult<Rule>> CreateAsync(Rule entity, CancellationToken cancellationToken = default)
    {
        await _ruleRepository.AddAsync(entity, cancellationToken);
        await _ruleRepository.SaveChangesAsync(cancellationToken);
        return AppResult<Rule>.Created(entity);
    }

    public async Task<AppResult<Rule>> UpdateAsync(Rule entity, CancellationToken cancellationToken = default)
    {
        var existing = await _ruleRepository.GetByIdAsync(entity.Id, cancellationToken);
        if (existing == null)
            return AppResult<Rule>.NotFound($"Rule with ID {entity.Id} not found");

        await _ruleRepository.UpdateAsync(entity, cancellationToken);
        await _ruleRepository.SaveChangesAsync(cancellationToken);
        return AppResult<Rule>.Success(entity);
    }

    public async Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var rule = await _ruleRepository.GetByIdAsync(id, cancellationToken);
        if (rule == null)
            return AppResult.NotFound($"Rule with ID {id} not found");

        await _ruleRepository.DeleteAsync(rule, cancellationToken);
        await _ruleRepository.SaveChangesAsync(cancellationToken);
        return AppResult.Success();
    }

    public async Task<AppResult<IEnumerable<(Rule rule, RuleAssignment? assignment)>>> GetRulesWithAssignmentsAsync(int gameId, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
        if (game == null)
            return AppResult<IEnumerable<(Rule rule, RuleAssignment? assignment)>>.NotFound($"Game with ID {gameId} not found");

        var rules = await _ruleRepository.GetByGameIdAsync(gameId, cancellationToken);
        var rulesList = rules.ToList();

        var result = new List<(Rule rule, RuleAssignment? assignment)>();

        foreach (var rule in rulesList)
        {
            var assignment = await _ruleAssignmentRepository.GetByRuleIdAsync(rule.Id, cancellationToken);
            result.Add((rule, assignment));
        }

        return AppResult<IEnumerable<(Rule rule, RuleAssignment? assignment)>>.Success(result);
    }

    public async Task<AppResult<IEnumerable<Rule>>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
    {
        var rules = await _ruleRepository.GetByGameIdAsync(gameId, cancellationToken);
        return AppResult<IEnumerable<Rule>>.Success(rules);
    }

    public async Task<AppResult<Rule>> UpdateRuleAsync(int ruleId, int gameId, int creatorPlayerId, string name, RuleType ruleType, int scoreDelta, CancellationToken cancellationToken = default)
    {
        var rule = await _ruleRepository.GetByIdAsync(ruleId, cancellationToken);
        if (rule == null)
            return AppResult<Rule>.NotFound($"Rule with ID {ruleId} not found");

        var game = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
        if (game == null)
            return AppResult<Rule>.NotFound($"Game with ID {gameId} not found");

        // Check if requester is creator
        if (game.CreatorPlayerId != creatorPlayerId)
            return AppResult<Rule>.Forbidden("Only the game creator can modify rules");

        // Check if rule is already assigned
        var isAssigned = await _ruleAssignmentRepository.IsRuleAssignedAsync(ruleId, cancellationToken);
        if (isAssigned)
            return AppResult<Rule>.Conflict("Cannot modify a rule that has already been assigned", "RULE_ALREADY_ASSIGNED");

        rule.Name = name;
        rule.RuleType = ruleType;
        rule.ScoreDelta = scoreDelta;
        rule.UpdatedAt = DateTime.UtcNow;

        await _ruleRepository.UpdateAsync(rule, cancellationToken);
        await _ruleRepository.SaveChangesAsync(cancellationToken);

        return AppResult<Rule>.Success(rule);
    }
}
