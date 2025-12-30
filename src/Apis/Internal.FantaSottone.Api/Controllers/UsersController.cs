namespace Internal.FantaSottone.Api.Controllers;

using Internal.FantaSottone.Domain.Dtos;
using Internal.FantaSottone.Domain.Managers;
using Internal.FantaSottone.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller for user operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires JWT authentication for all endpoints
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IGameManager _gameManager;
    private readonly IPlayerService _playerService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        IGameManager gameManager,
        IPlayerService playerService,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _gameManager = gameManager;
        _playerService = playerService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current user's profile
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(GetUserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
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

        var result = await _userService.GetByIdAsync(userId, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve user profile",
                Detail = string.Join("; ", result.Errors.Select(e => e.Message))
            });
        }

        var user = result.Value!;
        var response = new GetUserProfileResponse
        {
            Profile = new UserProfileDto
            {
                UserId = user.Id,
                Email = user.Email,
                ProfileImageUrl = null, // TODO: Add profile image support
                CreatedAt = user.CreatedAt
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Gets all games the current user has been invited to
    /// </summary>
    [HttpGet("games")]
    [ProducesResponseType(typeof(GetUserGamesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserGames(CancellationToken cancellationToken)
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

        var gamesResult = await _gameManager.GetUserGamesAsync(userId, cancellationToken);

        if (gamesResult.IsFailure)
        {
            return StatusCode((int)gamesResult.StatusCode, new ProblemDetails
            {
                Status = (int)gamesResult.StatusCode,
                Title = gamesResult.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve games",
                Detail = string.Join("; ", gamesResult.Errors.Select(e => e.Message))
            });
        }

        var games = gamesResult.Value!;
        var gameDtos = new List<GameInvitationDto>();

        foreach (var game in games)
        {
            var playersResult = await _playerService.GetByGameIdAsync(game.Id, cancellationToken);
            var playerCount = playersResult.IsSuccess ? playersResult.Value!.Count() : 0;

            gameDtos.Add(new GameInvitationDto
            {
                GameId = game.Id,
                GameName = game.Name,
                InitialScore = game.InitialScore,
                Status = game.Status,
                PlayerCount = playerCount,
                CreatedAt = game.CreatedAt
            });
        }

        var response = new GetUserGamesResponse
        {
            Games = gameDtos
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
}