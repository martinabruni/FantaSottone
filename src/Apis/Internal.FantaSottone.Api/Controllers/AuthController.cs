namespace Internal.FantaSottone.Api.Controllers;

using Internal.FantaSottone.Domain.Dtos;
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
    /// Registers a new user in the system
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var registerResult = await _authManager.RegisterAsync(request, cancellationToken);

        if (registerResult.IsFailure)
        {
            return StatusCode((int)registerResult.StatusCode, new ProblemDetails
            {
                Status = (int)registerResult.StatusCode,
                Title = registerResult.Errors.FirstOrDefault()?.Message ?? "Registration failed",
                Detail = string.Join("; ", registerResult.Errors.Select(e => e.Message))
            });
        }

        // Auto-login after registration
        var loginRequest = new LoginRequest
        {
            Username = request.Username,
            Password = request.Password
        };

        var loginResult = await _authManager.LoginUserAsync(loginRequest, cancellationToken);

        if (loginResult.IsFailure)
        {
            _logger.LogWarning("User registered but auto-login failed for {Username}", request.Username);
            return StatusCode(StatusCodes.Status201Created, new { message = "User registered successfully. Please login." });
        }

        return StatusCode(StatusCodes.Status201Created, loginResult.Value);
    }

    /// <summary>
    /// Authenticates a user with username and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authManager.LoginUserAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Authentication failed",
                Detail = string.Join("; ", result.Errors.Select(e => e.Message))
            });
        }

        return Ok(result.Value);
    }
}
