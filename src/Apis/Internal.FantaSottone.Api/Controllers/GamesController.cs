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
    /// Creates a new game with players and rules (public endpoint for initial game setup)
    /// </summary>
    //[AllowAnonymous]
    //[HttpPost("start")]
    //[ProducesResponseType(typeof(StartGameResponse), StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //public async Task<IActionResult> StartGame([FromBody] StartGameRequest request, CancellationToken cancellationToken)
    //{
    //    // Map request to domain
    //    var players = request.Players.Select(p => (p.Username, p.AccessCode, p.IsCreator)).ToList();
    //    var rules = request.Rules.Select(r => (r.Name, (RuleType)r.RuleType, r.ScoreDelta)).ToList();

    //    var result = await _gameManager.StartGameAsync(
    //        request.Name,
    //        request.InitialScore,
    //        players,
    //        rules,
    //        cancellationToken);

    //    if (result.IsFailure)
    //    {
    //        return StatusCode((int)result.StatusCode, new ProblemDetails
    //        {
    //            Status = (int)result.StatusCode,
    //            Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to start game",
    //            Detail = string.Join("; ", result.Errors.Select(e => e.Message))
    //        });
    //    }

    //    var startGameResult = result.Value!;

    //    var response = new StartGameResponse
    //    {
    //        GameId = startGameResult.GameId,
    //        Credentials = startGameResult.Credentials.Select(c => new PlayerCredentialDto
    //        {
    //            Username = c.Username,
    //            AccessCode = c.AccessCode,
    //            IsCreator = c.IsCreator
    //        }).ToList()
    //    };

    //    return Ok(response);
    //}

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

        // MODIFICATO: Usare p.User?.Email invece di p.Username
        var response = result.Value!.Select(p => new LeaderboardPlayerDto
        {
            Id = p.Id,
            Email = p.User?.Email ?? "N/A",  // ✅ CAMBIATO DA Username A Email
            CurrentScore = p.CurrentScore,
            IsCreator = p.IsCreator
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

                // MODIFICATO: Usare playerResult.Value!.User?.Email invece di playerResult.Value!.Username
                var playerEmail = playerResult.IsSuccess
                    ? playerResult.Value!.User?.Email ?? "N/A"  // ✅ CAMBIATO DA Username A Email
                    : "Unknown";

                assignmentInfo = new RuleAssignmentInfoDto
                {
                    RuleAssignmentId = assignment.Id,
                    AssignedToPlayerId = assignment.AssignedToPlayerId,
                    AssignedToEmail = playerEmail,  // ✅ CAMBIATO DA AssignedToUsername A AssignedToEmail
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignRule(int gameId, int ruleId, CancellationToken cancellationToken)
    {
        var playerId = GetAuthenticatedPlayerId();

        var result = await _ruleAssignmentService.AssignRuleAsync(gameId, ruleId, playerId, cancellationToken);

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
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateRule(int gameId, [FromBody] CreateRuleRequest request, CancellationToken cancellationToken)
    {
        var creatorPlayerId = GetAuthenticatedPlayerId();

        var result = await _ruleService.CreateRuleAsync(
            gameId,
            creatorPlayerId,
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
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateRule(int gameId, int ruleId, [FromBody] UpdateRuleRequest request, CancellationToken cancellationToken)
    {
        var creatorPlayerId = GetAuthenticatedPlayerId();

        var result = await _ruleService.UpdateRuleAsync(
            ruleId,
            gameId,
            creatorPlayerId,
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

        // Assume che result.Value sia IEnumerable<RuleAssignment>
        var response = new List<AssignmentHistoryDto>();

        foreach (var assignment in result.Value!)
        {
            var ruleResult = await _ruleService.GetByIdAsync(assignment.RuleId, cancellationToken);
            var playerResult = await _playerService.GetByIdAsync(assignment.AssignedToPlayerId, cancellationToken);

            // MODIFICATO: Usare playerResult.Value!.User?.Email invece di playerResult.Value!.Username
            var playerEmail = playerResult.IsSuccess
                ? playerResult.Value!.User?.Email ?? "N/A"  // ✅ CAMBIATO DA Username A Email
                : "Unknown";

            response.Add(new AssignmentHistoryDto
            {
                Id = assignment.Id,
                RuleId = assignment.RuleId,
                RuleName = ruleResult.IsSuccess ? ruleResult.Value!.Name : "Unknown",
                AssignedToPlayerId = assignment.AssignedToPlayerId,
                AssignedToEmail = playerEmail,  // ✅ CAMBIATO DA AssignedToUsername A AssignedToEmail
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
                Email = winner.User?.Email ?? "N/A",  // ✅ CAMBIATO DA Username A Email
                CurrentScore = winner.CurrentScore
            },
            Leaderboard = leaderboard.Select(p => new LeaderboardPlayerDto
            {
                Id = p.Id,
                Email = p.User?.Email ?? "N/A",  // ✅ CAMBIATO DA Username A Email
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
    /// Helper method to extract authenticated player ID from JWT token (for player-specific endpoints)
    /// </summary>
    private int GetAuthenticatedPlayerId()
    {
        var playerIdClaim = User.FindFirst("playerId")?.Value;
        return int.TryParse(playerIdClaim, out var playerId) ? playerId : 0;
    }
}