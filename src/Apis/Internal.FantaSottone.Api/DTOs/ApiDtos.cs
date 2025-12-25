namespace Internal.FantaSottone.Api.DTOs;

// ========== Authentication ==========

public sealed class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public GameDto Game { get; set; } = null!;
    public PlayerDto Player { get; set; } = null!;
}

// ========== Game Setup ==========

public sealed class StartGameRequest
{
    public string Name { get; set; } = string.Empty;
    public int InitialScore { get; set; }
    public List<PlayerCreateDto> Players { get; set; } = [];
    public List<RuleCreateDto> Rules { get; set; } = [];
}

public sealed class PlayerCreateDto
{
    public string Username { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
    public bool IsCreator { get; set; }
}

public sealed class RuleCreateDto
{
    public string Name { get; set; } = string.Empty;
    public int RuleType { get; set; } // 1=Bonus, 2=Malus
    public int ScoreDelta { get; set; }
}

public sealed class StartGameResponse
{
    public int GameId { get; set; }
    public List<PlayerCredentialDto> Credentials { get; set; } = [];
}

public sealed class PlayerCredentialDto
{
    public string Username { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
    public bool IsCreator { get; set; }
}

// ========== Runtime ==========

public sealed class LeaderboardPlayerDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public int CurrentScore { get; set; }
    public bool IsCreator { get; set; }
}

public sealed class RuleWithAssignmentDto
{
    public RuleDto Rule { get; set; } = null!;
    public RuleAssignmentInfoDto? Assignment { get; set; }
}

public sealed class RuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RuleType { get; set; }
    public int ScoreDelta { get; set; }
}

public sealed class RuleAssignmentInfoDto
{
    public int RuleAssignmentId { get; set; }
    public int AssignedToPlayerId { get; set; }
    public string AssignedToUsername { get; set; } = string.Empty;
    public string AssignedAt { get; set; } = string.Empty;
}

public sealed class AssignRuleRequest
{
    public int PlayerId { get; set; }
}

public sealed class AssignRuleResponse
{
    public AssignmentDto Assignment { get; set; } = null!;
    public UpdatedPlayerDto UpdatedPlayer { get; set; } = null!;
    public GameStatusDto GameStatus { get; set; } = null!;
}

public sealed class AssignmentDto
{
    public int Id { get; set; }
    public int RuleId { get; set; }
    public int AssignedToPlayerId { get; set; }
    public string AssignedAt { get; set; } = string.Empty;
    public int ScoreDeltaApplied { get; set; }
}

public sealed class UpdatedPlayerDto
{
    public int Id { get; set; }
    public int CurrentScore { get; set; }
}

public sealed class GameStatusDto
{
    public int Status { get; set; }
    public int? WinnerPlayerId { get; set; }
}

public sealed class GameStatusResponse
{
    public GameInfoDto Game { get; set; } = null!;
    public WinnerDto? Winner { get; set; }
}

public sealed class GameInfoDto
{
    public int Id { get; set; }
    public int Status { get; set; }
    public int? WinnerPlayerId { get; set; }
}

public sealed class WinnerDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public int CurrentScore { get; set; }
}

public sealed class AssignmentAuditDto
{
    public int Id { get; set; }
    public int RuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public int AssignedToPlayerId { get; set; }
    public string AssignedToUsername { get; set; } = string.Empty;
    public int ScoreDeltaApplied { get; set; }
    public string AssignedAt { get; set; } = string.Empty;
}

// ========== Game End ==========

public sealed class EndGameResponse
{
    public EndGameInfoDto Game { get; set; } = null!;
    public WinnerDto Winner { get; set; } = null!;
    public List<LeaderboardPlayerDto> Leaderboard { get; set; } = [];
}

public sealed class EndGameInfoDto
{
    public int Id { get; set; }
    public int Status { get; set; }
    public int WinnerPlayerId { get; set; }
}

// ========== Rule Update ==========

public sealed class UpdateRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public int RuleType { get; set; }
    public int ScoreDelta { get; set; }
}

public sealed class UpdateRuleResponse
{
    public RuleDto Rule { get; set; } = null!;
}

// ========== Common DTOs ==========

public sealed class GameDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int InitialScore { get; set; }
    public int Status { get; set; }
    public int? CreatorPlayerId { get; set; }
    public int? WinnerPlayerId { get; set; }
}

public sealed class PlayerDto
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsCreator { get; set; }
    public int CurrentScore { get; set; }
}
