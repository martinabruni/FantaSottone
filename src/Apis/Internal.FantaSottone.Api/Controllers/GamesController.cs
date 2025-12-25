namespace Internal.FantaSottone.Api.Controllers;

using Internal.FantaSottone.Api.DTOs;
using Internal.FantaSottone.Domain.Managers;
using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public sealed class GamesController : ControllerBase
{
    private readonly IGameManager _gameManager;
    private readonly IGameService _gameService;
    private readonly IPlayerService _playerService;
    private readonly IRuleService _ruleService;
    private readonly IRuleAssignmentService _ruleAssignmentService;
    private readonly ILogger<GamesController> _logger;

    public GamesController(
        IGameManager gameManager,
        IGameService gameService,
        IPlayerService playerService,
        IRuleService ruleService,
        IRuleAssignmentService ruleAssignmentService,
        ILogger<GamesController> logger)
    {
        _gameManager = gameManager;
        _gameService = gameService;
        _playerService = playerService;
        _ruleService = ruleService;
        _ruleAssignmentService = ruleAssignmentService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new game with players and rules
    /// </summary>
    [HttpPost("start")]
    [ProducesResponseType(typeof(StartGameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartGame([FromBody] StartGameRequest request, CancellationToken cancellationToken)
    {
        // Map request to domain
        var players = request.Players.Select(p => (p.Username, p.AccessCode, p.IsCreator)).ToList();
        var rules = request.Rules.Select(r => (r.Name, (RuleType)r.RuleType, r.ScoreDelta)).ToList();

        var result = await _gameManager.StartGameAsync(
            request.Name,
            request.InitialScore,
            players,
            rules,
            cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to start game",
                Detail = string.Join("; ", result.Errors.Select(e => e.Message))
            });
        }

        var startGameResult = result.Value!;

        var response = new StartGameResponse
        {
            GameId = startGameResult.GameId,
            Credentials = startGameResult.Credentials.Select(c => new PlayerCredentialDto
            {
                Username = c.Username,
                AccessCode = c.AccessCode,
                IsCreator = c.IsCreator
            }).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Gets the leaderboard for a game
    /// </summary>
    [HttpGet("{gameId}/leaderboard")]
    [ProducesResponseType(typeof(List<LeaderboardPlayerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLeaderboard(int gameId, CancellationToken cancellationToken)
    {
        var result = await _gameService.GetLeaderboardAsync(gameId, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to get leaderboard"
            });
        }

        var response = result.Value!.Select(p => new LeaderboardPlayerDto
        {
            Id = p.Id,
            Username = p.Username,
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

        // Need to get player usernames for assignments
        var response = new List<RuleWithAssignmentDto>();

        foreach (var (rule, assignment) in result.Value!)
        {
            RuleAssignmentInfoDto? assignmentInfo = null;

            if (assignment != null)
            {
                var playerResult = await _playerService.GetByIdAsync(assignment.AssignedToPlayerId, cancellationToken);
                var playerUsername = playerResult.IsSuccess ? playerResult.Value!.Username : "Unknown";

                assignmentInfo = new RuleAssignmentInfoDto
                {
                    RuleAssignmentId = assignment.Id,
                    AssignedToPlayerId = assignment.AssignedToPlayerId,
                    AssignedToUsername = playerUsername,
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
    /// Assigns a rule to a player (atomic)
    /// </summary>
    [Authorize]
    [HttpPost("{gameId}/rules/{ruleId}/assign")]
    [ProducesResponseType(typeof(AssignRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignRule(int gameId, int ruleId, [FromBody] AssignRuleRequest request, CancellationToken cancellationToken)
    {
        // Validate that the authenticated player is making the request
        var authenticatedPlayerId = GetAuthenticatedPlayerId();
        if (authenticatedPlayerId != request.PlayerId)
        {
            return Forbid();
        }

        // Assign rule
        var assignResult = await _ruleAssignmentService.AssignRuleAsync(ruleId, gameId, request.PlayerId, cancellationToken);

        if (assignResult.IsFailure)
        {
            return StatusCode((int)assignResult.StatusCode, new ProblemDetails
            {
                Status = (int)assignResult.StatusCode,
                Title = assignResult.Errors.FirstOrDefault()?.Message ?? "Failed to assign rule",
                Type = assignResult.Errors.FirstOrDefault()?.Code
            });
        }

        var assignment = assignResult.Value!;

        // Get updated player
        var playerResult = await _playerService.GetByIdAsync(request.PlayerId, cancellationToken);
        if (playerResult.IsFailure)
        {
            return StatusCode(500, new ProblemDetails { Title = "Failed to retrieve updated player" });
        }

        var player = playerResult.Value!;

        // Check if game should end
        var autoEndResult = await _gameService.TryAutoEndGameAsync(gameId, cancellationToken);
        var game = autoEndResult.IsSuccess ? autoEndResult.Value! : null;

        // Get current game status if auto-end didn't happen
        if (game == null)
        {
            var gameResult = await _gameService.GetByIdAsync(gameId, cancellationToken);
            game = gameResult.Value;
        }

        var response = new AssignRuleResponse
        {
            Assignment = new AssignmentDto
            {
                Id = assignment.Id,
                RuleId = assignment.RuleId,
                AssignedToPlayerId = assignment.AssignedToPlayerId,
                AssignedAt = assignment.AssignedAt.ToString("O"),
                ScoreDeltaApplied = assignment.ScoreDeltaApplied
            },
            UpdatedPlayer = new UpdatedPlayerDto
            {
                Id = player.Id,
                CurrentScore = player.CurrentScore
            },
            GameStatus = new GameStatusDto
            {
                Status = game != null ? (int)game.Status : 2,
                WinnerPlayerId = game?.WinnerPlayerId
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Gets game status and winner
    /// </summary>
    [HttpGet("{gameId}/status")]
    [ProducesResponseType(typeof(GameStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(int gameId, CancellationToken cancellationToken)
    {
        var gameResult = await _gameService.GetByIdAsync(gameId, cancellationToken);

        if (gameResult.IsFailure)
        {
            return StatusCode((int)gameResult.StatusCode, new ProblemDetails
            {
                Status = (int)gameResult.StatusCode,
                Title = gameResult.Errors.FirstOrDefault()?.Message ?? "Game not found"
            });
        }

        var game = gameResult.Value!;

        WinnerDto? winner = null;
        if (game.WinnerPlayerId.HasValue)
        {
            var winnerResult = await _playerService.GetByIdAsync(game.WinnerPlayerId.Value, cancellationToken);
            if (winnerResult.IsSuccess)
            {
                var winnerPlayer = winnerResult.Value!;
                winner = new WinnerDto
                {
                    Id = winnerPlayer.Id,
                    Username = winnerPlayer.Username,
                    CurrentScore = winnerPlayer.CurrentScore
                };
            }
        }

        var response = new GameStatusResponse
        {
            Game = new GameInfoDto
            {
                Id = game.Id,
                Status = (int)game.Status,
                WinnerPlayerId = game.WinnerPlayerId
            },
            Winner = winner
        };

        return Ok(response);
    }

    /// <summary>
    /// Gets assignment audit trail
    /// </summary>
    [HttpGet("{gameId}/assignments")]
    [ProducesResponseType(typeof(List<AssignmentAuditDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAssignments(int gameId, CancellationToken cancellationToken)
    {
        var result = await _ruleAssignmentService.GetByGameIdAsync(gameId, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to get assignments"
            });
        }

        var response = new List<AssignmentAuditDto>();

        foreach (var assignment in result.Value!)
        {
            // Get rule name
            var ruleResult = await _ruleService.GetByIdAsync(assignment.RuleId, cancellationToken);
            var ruleName = ruleResult.IsSuccess ? ruleResult.Value!.Name : "Unknown";

            // Get player username
            var playerResult = await _playerService.GetByIdAsync(assignment.AssignedToPlayerId, cancellationToken);
            var playerUsername = playerResult.IsSuccess ? playerResult.Value!.Username : "Unknown";

            response.Add(new AssignmentAuditDto
            {
                Id = assignment.Id,
                RuleId = assignment.RuleId,
                RuleName = ruleName,
                AssignedToPlayerId = assignment.AssignedToPlayerId,
                AssignedToUsername = playerUsername,
                ScoreDeltaApplied = assignment.ScoreDeltaApplied,
                AssignedAt = assignment.AssignedAt.ToString("O")
            });
        }

        return Ok(response);
    }

    /// <summary>
    /// Ends a game manually (creator only)
    /// </summary>
    [Authorize]
    [HttpPost("{gameId}/end")]
    [ProducesResponseType(typeof(EndGameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EndGame(int gameId, CancellationToken cancellationToken)
    {
        var creatorPlayerId = GetAuthenticatedPlayerId();

        var result = await _gameService.EndGameAsync(gameId, creatorPlayerId, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to end game"
            });
        }

        var game = result.Value!;

        // Get winner
        var winnerResult = await _playerService.GetByIdAsync(game.WinnerPlayerId!.Value, cancellationToken);
        if (winnerResult.IsFailure)
        {
            return StatusCode(500, new ProblemDetails { Title = "Failed to retrieve winner" });
        }

        var winner = winnerResult.Value!;

        // Get leaderboard
        var leaderboardResult = await _gameService.GetLeaderboardAsync(gameId, cancellationToken);
        var leaderboard = leaderboardResult.IsSuccess
            ? leaderboardResult.Value!.Select(p => new LeaderboardPlayerDto
            {
                Id = p.Id,
                Username = p.Username,
                CurrentScore = p.CurrentScore,
                IsCreator = p.IsCreator
            }).ToList()
            : [];

        var response = new EndGameResponse
        {
            Game = new EndGameInfoDto
            {
                Id = game.Id,
                Status = (int)game.Status,
                WinnerPlayerId = game.WinnerPlayerId!.Value
            },
            Winner = new WinnerDto
            {
                Id = winner.Id,
                Username = winner.Username,
                CurrentScore = winner.CurrentScore
            },
            Leaderboard = leaderboard
        };

        return Ok(response);
    }

    /// <summary>
    /// Updates a rule (creator only, pre-assignment)
    /// </summary>
    [Authorize]
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

    private int GetAuthenticatedPlayerId()
    {
        var playerIdClaim = User.FindFirst("playerId")?.Value;
        return int.TryParse(playerIdClaim, out var playerId) ? playerId : 0;
    }
}
