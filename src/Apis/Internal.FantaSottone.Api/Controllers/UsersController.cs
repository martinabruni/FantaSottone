namespace Internal.FantaSottone.Api.Controllers;

using Internal.FantaSottone.Domain.Dtos;
using Internal.FantaSottone.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all games for the authenticated user (Dashboard)
    /// </summary>
    [HttpGet("me/games")]
    [ProducesResponseType(typeof(IEnumerable<GameListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyGames(CancellationToken cancellationToken)
    {
        // TODO: Replace with actual authenticated user ID from JWT token
        // For now, expecting userId in header or query parameter
        if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) ||
            !int.TryParse(userIdHeader, out var userId))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "User not authenticated",
                Detail = "X-User-Id header is required"
            });
        }

        var result = await _userService.GetUserGamesAsync(userId, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve games",
                Detail = string.Join("; ", result.Errors.Select(e => e.Message))
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Searches for users by username (for inviting to games)
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<UserSearchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchUsers([FromQuery] string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid search query",
                Detail = "Query parameter is required"
            });
        }

        var result = await _userService.SearchUsersAsync(query, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Search failed",
                Detail = string.Join("; ", result.Errors.Select(e => e.Message))
            });
        }

        return Ok(result.Value);
    }
}
