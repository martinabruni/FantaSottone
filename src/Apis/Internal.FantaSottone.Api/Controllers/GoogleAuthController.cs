namespace Internal.FantaSottone.Api.Controllers;

using Internal.FantaSottone.Domain.Dtos;
using Internal.FantaSottone.Domain.Managers;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller for Google OAuth authentication
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class GoogleAuthController : ControllerBase
{
    private readonly IAuthManager _authManager;
    private readonly ILogger<GoogleAuthController> _logger;

    public GoogleAuthController(IAuthManager authManager, ILogger<GoogleAuthController> logger)
    {
        _authManager = authManager;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates user with Google OAuth token
    /// Creates user account on first login
    /// </summary>
    /// <param name="request">Google authentication request containing ID token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(GoogleAuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GoogleLogin(
        [FromBody] GoogleAuthRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authManager.GoogleAuthAsync(request, cancellationToken);

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
        
        if (response.IsFirstLogin)
        {
            _logger.LogInformation("First login completed for user {UserId} with email {Email}", 
                response.UserId, response.Email);
        }

        return Ok(response);
    }
}
