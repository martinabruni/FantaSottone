namespace Internal.FantaSottone.Domain.Results;

/// <summary>
/// Response wrapper for StartGame operation
/// </summary>
public sealed class StartGameResult
{
    public int GameId { get; set; }
    public List<(string Username, string AccessCode, bool IsCreator)> Credentials { get; set; } = [];
}

/// <summary>
/// Response wrapper for Login operation
/// </summary>
public sealed class LoginResult
{
    public string Token { get; set; } = string.Empty;
    public required object Game { get; set; }
    public required object Player { get; set; }
}
