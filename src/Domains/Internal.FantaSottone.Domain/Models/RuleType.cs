namespace Internal.FantaSottone.Domain.Models;

/// <summary>
/// Rule type enumeration
/// </summary>
public enum RuleType : byte
{
    /// <summary>
    /// Bonus rule - adds points (positive ScoreDelta)
    /// </summary>
    Bonus = 1,

    /// <summary>
    /// Malus rule - subtracts points (negative ScoreDelta)
    /// </summary>
    Malus = 2
}
