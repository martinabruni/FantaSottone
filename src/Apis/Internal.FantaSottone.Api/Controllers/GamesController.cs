namespace Internal.FantaSottone.Api.Controllers;

using Internal.FantaSottone.Api.DTOs;
using Internal.FantaSottone.Domain.Dtos;
using Internal.FantaSottone.Domain.Managers;
using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires JWT authentication for all endpoints except those marked with [AllowAnonymous]
public sealed class GamesController : ControllerBase
{
    private readonly IGameManager _gameManager;
    private readonly IPlayerService _playerService;
    private readonly IRuleService _ruleService;
    private readonly IRuleAssignmentService _ruleAssignmentService;
    private readonly ILogger<GamesController> _logger;

    public GamesController(
        IGameManager gameManager,
        IPlayerService playerService,
        IRuleService ruleService,
        IRuleAssignmentService ruleAssignmentService,
        ILogger<GamesController> logger)
    {
        _gameManager = gameManager;
        _playerService = playerService;
        _ruleService = ruleService;
        _ruleAssignmentService = ruleAssignmentService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new game and invites players by email
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType(typeof(CreateGameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == 0)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "User not authenticated",
                Detail = "Invalid or missing user ID in token"
            });
        }

        var result = await _gameManager.CreateGameWithEmailInvitesAsync(
            request.Name,
            request.InitialScore,
            userId,
            request.InvitedEmails,
            cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to create game",
                Detail = string.Join("; ", result.Errors.Select(e => e.Message))
            });
        }

        var (game, creatorPlayer, invitedEmails, invalidEmails) = result.Value!;

        var response = new CreateGameResponse
        {
            GameId = game.Id,
            GameName = game.Name,
            CreatorPlayerId = creatorPlayer.Id,
            InvitedEmails = invitedEmails,
            InvalidEmails = invalidEmails
        };

        return Ok(response);
    }

    /// <summary>
    /// Joins a game (sets it as active for the current session)
    /// </summary>
    [HttpPost("{gameId}/join")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> JoinGame(int gameId, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == 0)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "User not authenticated",
                Detail = "Invalid or missing user ID in token"
            });
        }

        var result = await _gameManager.JoinGameAsync(gameId, userId, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to join game",
                Detail = string.Join("; ", result.Errors.Select(e => e.Message))
            });
        }

        var player = result.Value!;

        // Return game context for the player
        var gameResult = await _gameManager.GetByIdAsync(gameId, cancellationToken);
        if (gameResult.IsFailure)
        {
            return StatusCode((int)gameResult.StatusCode, new ProblemDetails
            {
                Status = (int)gameResult.StatusCode,
                Title = "Game not found after join"
            });
        }

        var game = gameResult.Value!;

        return Ok(new
        {
            message = "Joined game successfully",
            game = new
            {
                id = game.Id,
                name = game.Name,
                status = (int)game.Status,
                initialScore = game.InitialScore
            },
            player = new
            {
                id = player.Id,
                currentScore = player.CurrentScore,
                isCreator = player.IsCreator
            }
        });
    }

    /// <summary>
    /// Starts a game (transitions from Draft to Started status, creator only)
    /// </summary>
    [HttpPost("{gameId}/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartGame(int gameId, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == 0)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "User not authenticated",
                Detail = "Invalid or missing user ID in token"
            });
        }

        var result = await _gameManager.StartGameAsync(gameId, userId, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to start game",
                Detail = string.Join("; ", result.Errors.Select(e => e.Message))
            });
        }

        var game = result.Value!;

        return Ok(new
        {
            message = "Game started successfully",
            game = new
            {
                id = game.Id,
                name = game.Name,
                status = (int)game.Status,
                initialScore = game.InitialScore
            }
        });
    }

    /// <summary>
    /// Invites a registered user to join a game (creator only)
    /// </summary>
    [HttpPost("{gameId}/invite")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> InvitePlayer(int gameId, [FromBody] InvitePlayerRequest request, CancellationToken cancellationToken)
    {
        var requestingUserId = GetAuthenticatedUserId();
        if (requestingUserId == 0)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "User not authenticated",
                Detail = "Invalid or missing user ID in token"
            });
        }

        var result = await _gameManager.InvitePlayerAsync(gameId, request.UserId, requestingUserId, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to invite player",
                Detail = string.Join("; ", result.Errors.Select(e => e.Message))
            });
        }

        return Ok(new { message = "Player invited successfully", playerId = result.Value!.Id });
    }

    /// <summary>
    /// Invites a user to join a game by email (creator only, draft state only)
    /// </summary>
    [HttpPost("{gameId}/invite-by-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> InvitePlayerByEmail(int gameId, [FromBody] InvitePlayerByEmailRequest request, CancellationToken cancellationToken)
    {
        var requestingUserId = GetAuthenticatedUserId();
        if (requestingUserId == 0)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "User not authenticated",
                Detail = "Invalid or missing user ID in token"
            });
        }

        var result = await _gameManager.InvitePlayerByEmailAsync(gameId, request.Email, requestingUserId, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to invite player",
                Detail = string.Join("; ", result.Errors.Select(e => e.Message))
            });
        }

        return Ok(new { message = "Player invited successfully", playerId = result.Value!.Id });
    }

    /// <summary>
    /// Gets the leaderboard for a game
    /// </summary>
    [HttpGet("{gameId}/leaderboard")]
    [ProducesResponseType(typeof(List<LeaderboardPlayerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLeaderboard(int gameId, CancellationToken cancellationToken)
    {
        var result = await _gameManager.GetLeaderboardAsync(gameId, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to get leaderboard"
            });
        }

        // Get game status to include in response
        var gameResult = await _gameManager.GetByIdAsync(gameId, cancellationToken);
        var gameStatus = gameResult.IsSuccess ? (int)gameResult.Value!.Status : (int)GameStatus.Started;

        var response = result.Value!.Select(p => new LeaderboardPlayerDto
        {
            Id = p.Id,
            Email = p.User?.Email ?? "N/A",
            CurrentScore = p.CurrentScore,
            IsCreator = p.IsCreator,
            GameStatus = gameStatus
        }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Gets rules with assignment status for a game
    /// </summary>
    [HttpGet("{gameId}/rules")]
    [ProducesResponseType(typeof(List<RuleWithAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRules(int gameId, CancellationToken cancellationToken)
    {
        var result = await _ruleService.GetRulesWithAssignmentsAsync(gameId, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to get rules"
            });
        }

        var response = new List<RuleWithAssignmentDto>();

        foreach (var (rule, assignment) in result.Value!)
        {
            RuleAssignmentInfoDto? assignmentInfo = null;

            if (assignment != null)
            {
                var playerResult = await _playerService.GetByIdAsync(assignment.AssignedToPlayerId, cancellationToken);

                var playerEmail = playerResult.IsSuccess
                    ? playerResult.Value!.User?.Email ?? "N/A"
                    : "Unknown";

                assignmentInfo = new RuleAssignmentInfoDto
                {
                    RuleAssignmentId = assignment.Id,
                    AssignedToPlayerId = assignment.AssignedToPlayerId,
                    AssignedToEmail = playerEmail,
                    AssignedAt = assignment.AssignedAt.ToString("O")
                };
            }

            response.Add(new RuleWithAssignmentDto
            {
                Rule = new RuleDto
                {
                    Id = rule.Id,
                    Name = rule.Name,
                    RuleType = (int)rule.RuleType,
                    ScoreDelta = rule.ScoreDelta
                },
                Assignment = assignmentInfo
            });
        }

        return Ok(response);
    }

    /// <summary>
    /// Assigns a rule to the authenticated player
    /// </summary>
    [HttpPost("{gameId}/rules/{ruleId}/assign")]
    [ProducesResponseType(typeof(AssignRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignRule(int gameId, int ruleId, CancellationToken cancellationToken)
    {
        // Get player from userId + gameId
        var playerResult = await GetPlayerForGameAsync(gameId, cancellationToken);
        if (playerResult == null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "User not authenticated",
                Detail = "Invalid or missing user ID in token"
            });
        }

        if (playerResult.IsFailure)
        {
            return StatusCode((int)playerResult.StatusCode, new ProblemDetails
            {
                Status = (int)playerResult.StatusCode,
                Title = playerResult.Errors.FirstOrDefault()?.Message ?? "Player not found in this game",
                Detail = "You must be a player in this game to assign rules"
            });
        }

        var player = playerResult.Value!;

        var result = await _ruleAssignmentService.AssignRuleAsync(ruleId, gameId, player.Id, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to assign rule",
                Detail = string.Join("; ", result.Errors.Select(e => e.Message))
            });
        }

        var ruleAssignment = result.Value!;

        var response = new AssignRuleResponse
        {
            Assignment = new AssignmentDto
            {
                Id = ruleAssignment.Id,
                RuleId = ruleAssignment.RuleId,
                AssignedToPlayerId = ruleAssignment.AssignedToPlayerId,
                ScoreDeltaApplied = ruleAssignment.ScoreDeltaApplied,
                AssignedAt = ruleAssignment.AssignedAt.ToString("O")
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Creates a new rule (creator only, pre-assignment)
    /// </summary>
    [HttpPost("{gameId}/rules")]
    [ProducesResponseType(typeof(CreateRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateRule(int gameId, [FromBody] CreateRuleRequest request, CancellationToken cancellationToken)
    {
        // Get player from userId + gameId
        var playerResult = await GetPlayerForGameAsync(gameId, cancellationToken);
        if (playerResult == null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "User not authenticated",
                Detail = "Invalid or missing user ID in token"
            });
        }

        if (playerResult.IsFailure)
        {
            return StatusCode((int)playerResult.StatusCode, new ProblemDetails
            {
                Status = (int)playerResult.StatusCode,
                Title = playerResult.Errors.FirstOrDefault()?.Message ?? "Player not found in this game",
                Detail = "You must be a player in this game to create rules"
            });
        }

        var player = playerResult.Value!;

        var result = await _ruleService.CreateRuleAsync(
            gameId,
            player.Id,
            request.Name,
            (RuleType)request.RuleType,
            request.ScoreDelta,
            cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to create rule",
                Detail = string.Join("; ", result.Errors.Select(e => e.Message))
            });
        }

        var rule = result.Value!;

        var response = new CreateRuleResponse
        {
            Rule = new RuleDto
            {
                Id = rule.Id,
                Name = rule.Name,
                RuleType = (int)rule.RuleType,
                ScoreDelta = rule.ScoreDelta
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Updates a rule (creator only, pre-assignment)
    /// </summary>
    [HttpPut("{gameId}/rules/{ruleId}")]
    [ProducesResponseType(typeof(UpdateRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateRule(int gameId, int ruleId, [FromBody] UpdateRuleRequest request, CancellationToken cancellationToken)
    {
        // Get player from userId + gameId
        var playerResult = await GetPlayerForGameAsync(gameId, cancellationToken);
        if (playerResult == null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "User not authenticated",
                Detail = "Invalid or missing user ID in token"
            });
        }

        if (playerResult.IsFailure)
        {
            return StatusCode((int)playerResult.StatusCode, new ProblemDetails
            {
                Status = (int)playerResult.StatusCode,
                Title = playerResult.Errors.FirstOrDefault()?.Message ?? "Player not found in this game",
                Detail = "You must be a player in this game to update rules"
            });
        }

        var player = playerResult.Value!;

        var result = await _ruleService.UpdateRuleAsync(
            ruleId,
            gameId,
            player.Id,
            request.Name,
            (RuleType)request.RuleType,
            request.ScoreDelta,
            cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to update rule",
                Type = result.Errors.FirstOrDefault()?.Code
            });
        }

        var rule = result.Value!;

        var response = new UpdateRuleResponse
        {
            Rule = new RuleDto
            {
                Id = rule.Id,
                Name = rule.Name,
                RuleType = (int)rule.RuleType,
                ScoreDelta = rule.ScoreDelta
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Deletes a rule (creator only, pre-assignment)
    /// </summary>
    [HttpDelete("{gameId}/rules/{ruleId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteRule(int gameId, int ruleId, CancellationToken cancellationToken)
    {
        // Get player from userId + gameId
        var playerResult = await GetPlayerForGameAsync(gameId, cancellationToken);
        if (playerResult == null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "User not authenticated",
                Detail = "Invalid or missing user ID in token"
            });
        }

        if (playerResult.IsFailure)
        {
            return StatusCode((int)playerResult.StatusCode, new ProblemDetails
            {
                Status = (int)playerResult.StatusCode,
                Title = playerResult.Errors.FirstOrDefault()?.Message ?? "Player not found in this game",
                Detail = "You must be a player in this game to delete rules"
            });
        }

        var player = playerResult.Value!;

        var result = await _ruleService.DeleteRuleAsync(
            ruleId,
            gameId,
            player.Id,
            cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to delete rule",
                Type = result.Errors.FirstOrDefault()?.Code
            });
        }

        return Ok(new { message = "Rule deleted successfully" });
    }

    /// <summary>
    /// Gets assignment history for a game
    /// </summary>
    [HttpGet("{gameId}/assignments")]
    [ProducesResponseType(typeof(List<AssignmentHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAssignmentHistory(int gameId, CancellationToken cancellationToken)
    {
        var result = await _ruleAssignmentService.GetByGameIdAsync(gameId, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to get assignment history"
            });
        }

        var response = new List<AssignmentHistoryDto>();

        foreach (var assignment in result.Value!)
        {
            var ruleResult = await _ruleService.GetByIdAsync(assignment.RuleId, cancellationToken);
            var playerResult = await _playerService.GetByIdAsync(assignment.AssignedToPlayerId, cancellationToken);

            var playerEmail = playerResult.IsSuccess
                ? playerResult.Value!.User?.Email ?? "N/A"
                : "Unknown";

            response.Add(new AssignmentHistoryDto
            {
                Id = assignment.Id,
                RuleId = assignment.RuleId,
                RuleName = ruleResult.IsSuccess ? ruleResult.Value!.Name : "Unknown",
                AssignedToPlayerId = assignment.AssignedToPlayerId,
                AssignedToEmail = playerEmail,
                ScoreDeltaApplied = assignment.ScoreDeltaApplied,
                AssignedAt = assignment.AssignedAt.ToString("O")
            });
        }

        return Ok(response);
    }

    /// <summary>
    /// Ends the game
    /// </summary>
    [HttpPost("{gameId}/end")]
    [ProducesResponseType(typeof(EndGameDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EndGame(int gameId, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == 0)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "User not authenticated",
                Detail = "Invalid or missing user ID in token"
            });
        }

        var result = await _gameManager.EndGameAsync(gameId, userId, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to end game"
            });
        }

        var (game, winner, leaderboard) = result.Value!;

        var response = new EndGameDto
        {
            Game = new GameDto
            {
                Id = game.Id,
                Name = game.Name,
                InitialScore = game.InitialScore,
                Status = (int)game.Status,
                CreatorPlayerId = game.CreatorPlayerId,
                WinnerPlayerId = game.WinnerPlayerId
            },
            Winner = new WinnerDto
            {
                Id = winner.Id,
                Email = winner.User?.Email ?? "N/A",
                CurrentScore = winner.CurrentScore
            },
            Leaderboard = leaderboard.Select(p => new LeaderboardPlayerDto
            {
                Id = p.Id,
                Email = p.User?.Email ?? "N/A",
                CurrentScore = p.CurrentScore,
                IsCreator = p.IsCreator
            }).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Helper method to extract authenticated user ID from JWT token
    /// </summary>
    private int GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Helper method to get the player for the authenticated user in a specific game.
    /// Returns null if user is not authenticated, or AppResult with failure if player not found.
    /// </summary>
    private async Task<Domain.Results.AppResult<Player>?> GetPlayerForGameAsync(int gameId, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == 0)
            return null;

        return await _playerService.GetByGameAndUserAsync(gameId, userId, cancellationToken);
    }
}