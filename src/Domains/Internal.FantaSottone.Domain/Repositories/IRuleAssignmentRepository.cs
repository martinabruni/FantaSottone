namespace Internal.FantaSottone.Domain.Repositories;

using Internal.FantaSottone.Domain.Models;

/// <summary>
/// Repository interface for RuleAssignment entity
/// </summary>
public interface IRuleAssignmentRepository : IRepository<RuleAssignment, int>
{
    /// <summary>
    /// Gets all assignments for a game ordered by assigned date
    /// </summary>
    Task<IEnumerable<RuleAssignment>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a rule is already assigned
    /// </summary>
    Task<bool> IsRuleAssignedAsync(int ruleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets assignment by rule ID
    /// </summary>
    Task<RuleAssignment?> GetByRuleIdAsync(int ruleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts assigned rules for a game
    /// </summary>
    Task<int> CountByGameIdAsync(int gameId, CancellationToken cancellationToken = default);
}
