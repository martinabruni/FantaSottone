namespace Internal.FantaSottone.Domain.Models;

/// <summary>
/// Game status enumeration
/// </summary>
public enum GameStatus : byte
{
    /// <summary>
    /// Game is in draft, players and rules can be configured
    /// </summary>
    Draft = 1,

    /// <summary>
    /// Game has started, rules can be assigned
    /// </summary>
    Started = 2,

    /// <summary>
    /// Game has ended, winner determined
    /// </summary>
    Ended = 3
}
