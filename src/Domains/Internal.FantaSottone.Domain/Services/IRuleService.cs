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
    /// <param name="ruleId">The rule ID to update</param>
    /// <param name="gameId">The game ID</param>
    /// <param name="requestingPlayerId">The Player.Id of the requesting user (NOT UserId!)</param>
    /// <param name="name">New rule name</param>
    /// <param name="ruleType">New rule type</param>
    /// <param name="scoreDelta">New score delta</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<AppResult<Rule>> UpdateRuleAsync(int ruleId, int gameId, int requestingPlayerId, string name, RuleType ruleType, int scoreDelta, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new rule for a game (creator only)
    /// </summary>
    /// <param name="gameId">The game ID</param>
    /// <param name="requestingPlayerId">The Player.Id of the requesting user (NOT UserId!)</param>
    /// <param name="name">Rule name</param>
    /// <param name="ruleType">Rule type (Bonus/Malus)</param>
    /// <param name="scoreDelta">Score delta</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<AppResult<Rule>> CreateRuleAsync(int gameId, int requestingPlayerId, string name, RuleType ruleType, int scoreDelta, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a rule if not yet assigned (creator only)
    /// </summary>
    /// <param name="ruleId">The rule ID to delete</param>
    /// <param name="gameId">The game ID</param>
    /// <param name="requestingPlayerId">The Player.Id of the requesting user (NOT UserId!)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<AppResult> DeleteRuleAsync(int ruleId, int gameId, int requestingPlayerId, CancellationToken cancellationToken = default);
}