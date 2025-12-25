namespace Internal.FantaSottone.Domain.Managers;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Results;

/// <summary>
/// Manager for game setup operations
/// </summary>
public interface IGameManager
{
    /// <summary>
    /// Creates a new game with players and rules
    /// </summary>
    Task<AppResult<StartGameResult>> StartGameAsync(
        string name,
        int initialScore,
        List<(string Username, string AccessCode, bool IsCreator)> players,
        List<(string Name, RuleType RuleType, int ScoreDelta)> rules,
        CancellationToken cancellationToken = default);
}
