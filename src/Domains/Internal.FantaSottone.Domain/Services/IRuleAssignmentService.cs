namespace Internal.FantaSottone.Domain.Services;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Results;

/// <summary>
/// Service interface for RuleAssignment operations
/// </summary>
public interface IRuleAssignmentService : IService<RuleAssignment, int>
{
    /// <summary>
    /// Assigns a rule to a player (atomic operation)
    /// </summary>
    Task<AppResult<RuleAssignment>> AssignRuleAsync(int ruleId, int gameId, int playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all assignments for a game (audit trail)
    /// </summary>
    Task<AppResult<IEnumerable<RuleAssignment>>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a rule is already assigned
    /// </summary>
    Task<bool> IsRuleAssignedAsync(int ruleId, CancellationToken cancellationToken = default);
}
