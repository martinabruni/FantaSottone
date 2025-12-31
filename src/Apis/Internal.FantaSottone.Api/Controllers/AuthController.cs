namespace Internal.FantaSottone.Api.Controllers;

using Internal.FantaSottone.Domain.Dtos;
using Internal.FantaSottone.Domain.Managers;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller for email/password authentication
/// </summary>
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
    /// Registers a new user with email and password
    /// </summary>
    /// <param name="request">Email registration request containing email and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(EmailAuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register(
        [FromBody] EmailRegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authManager.RegisterWithEmailAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Registration failed",
                Detail = string.Join("; ", result.Errors.Select(e => e.Message))
            });
        }

        var response = result.Value!;
        _logger.LogInformation("User registered successfully with email {Email}", response.Email);

        return Ok(response);
    }

    /// <summary>
    /// Authenticates user with email and password
    /// </summary>
    /// <param name="request">Email authentication request containing email and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(EmailAuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login(
        [FromBody] EmailAuthRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authManager.LoginWithEmailAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new ProblemDetails
            {
                Status = (int)result.StatusCode,
                Title = result.Errors.FirstOrDefault()?.Message ?? "Authentication failed",
                Detail = string.Join("; ", result.Errors.Select(e => e.Message))
            });
        }

        var response = result.Value!;
        _logger.LogInformation("User logged in successfully with email {Email}", response.Email);

        return Ok(response);
    }
}
