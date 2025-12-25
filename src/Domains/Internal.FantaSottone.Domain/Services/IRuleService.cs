namespace Internal.FantaSottone.Domain.Services;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Results;

/// <summary>
/// Service interface for Rule operations
/// </summary>
public interface IRuleService : IService<Rule, int>
{
    /// <summary>
    /// Gets rules with assignment status for a game
    /// </summary>
    Task<AppResult<IEnumerable<(Rule rule, RuleAssignment? assignment)>>> GetRulesWithAssignmentsAsync(int gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets rules by game ID
    /// </summary>
    Task<AppResult<IEnumerable<Rule>>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a rule if not yet assigned
    /// </summary>
    Task<AppResult<Rule>> UpdateRuleAsync(int ruleId, int gameId, int creatorPlayerId, string name, RuleType ruleType, int scoreDelta, CancellationToken cancellationToken = default);
}
