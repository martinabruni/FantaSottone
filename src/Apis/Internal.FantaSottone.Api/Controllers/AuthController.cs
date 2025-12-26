namespace Internal.FantaSottone.Api.Controllers;

using Internal.FantaSottone.Api.DTOs;
using Internal.FantaSottone.Domain.Managers;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthManager _authManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthManager authManager, ILogger<AuthController> logger)
    {
        _authManager = authManager;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a player with username and access code
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authManager.LoginAsync(request.Username, request.AccessCode, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Authentication failed",
                Detail = string.Join("; ", result.Errors.Select(e => e.Message))
            });
        }

        var loginResult = result.Value!;
        var game = (Internal.FantaSottone.Domain.Models.Game)loginResult.Game;
        var player = (Internal.FantaSottone.Domain.Models.Player)loginResult.Player;

        var response = new LoginResponse
        {
            Token = loginResult.Token,
            Game = new GameDto
            {
                Id = game.Id,
                Name = game.Name,
                InitialScore = game.InitialScore,
                Status = (int)game.Status,
                CreatorPlayerId = game.CreatorPlayerId,
                WinnerPlayerId = game.WinnerPlayerId
            },
            Player = new PlayerDto
            {
                Id = player.Id,
                GameId = player.GameId,
                Username = player.Username,
                IsCreator = player.IsCreator,
                CurrentScore = player.CurrentScore
            }
        };

        return Ok(response);
    }
}
