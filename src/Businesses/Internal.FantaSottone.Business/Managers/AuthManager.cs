namespace Internal.FantaSottone.Business.Managers;

using Google.Apis.Auth;
using Internal.FantaSottone.Domain.Dtos;
using Internal.FantaSottone.Domain.Managers;
using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

internal sealed class AuthManager : IAuthManager
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthManager> _logger;

    public AuthManager(
        IPlayerRepository playerRepository,
        IGameRepository gameRepository,
        IUserRepository userRepository,
        IConfiguration configuration,
        ILogger<AuthManager> logger)
    {
        _playerRepository = playerRepository;
        _gameRepository = gameRepository;
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AppResult<GoogleAuthResponse>> GoogleAuthAsync(
        GoogleAuthRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.IdToken))
                return AppResult<GoogleAuthResponse>.BadRequest("Google ID token is required");

            // Validate Google token and extract payload
            var googleSettings = _configuration.GetSection("Authentication:Google");
            var clientId = googleSettings["client_id"];

            if (string.IsNullOrEmpty(clientId))
            {
                _logger.LogError("Google client_id not configured");
                return AppResult<GoogleAuthResponse>.InternalServerError("Google authentication not properly configured");
            }

            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [clientId]
                };

                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "Invalid Google token provided");
                return AppResult<GoogleAuthResponse>.Unauthorized("Invalid Google token");
            }

            var email = payload.Email;
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Google token did not contain email");
                return AppResult<GoogleAuthResponse>.BadRequest("Email not found in Google token");
            }

            // Check if email is verified
            if (!payload.EmailVerified)
            {
                _logger.LogWarning("Google account email not verified: {Email}", email);
                return AppResult<GoogleAuthResponse>.Unauthorized("Please verify your Google account email");
            }

            // Check if user exists
            var userResult = await _userRepository.GetByEmailAsync(email, cancellationToken);
            bool isFirstLogin = userResult.IsFailure;
            User user;

            if (isFirstLogin)
            {
                // First login - create new user
                _logger.LogInformation("First login for email {Email}, creating new user", email);

                user = new User
                {
                    Email = email,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createResult = await _userRepository.AddAsync(user, cancellationToken);
                if (createResult.IsFailure)
                {
                    _logger.LogError("Failed to create user for email {Email}", email);
                    return AppResult<GoogleAuthResponse>.InternalServerError("Failed to create user account");
                }

                user = createResult.Value!;
                _logger.LogInformation("Created new user {UserId} for email {Email}", user.Id, email);
            }
            else
            {
                // Returning user
                user = userResult.Value!;
                _logger.LogInformation("User {UserId} logged in with email {Email}", user.Id, email);
            }

            // Generate JWT token for user
            var token = GenerateUserJwtToken(user);

            var response = new GoogleAuthResponse
            {
                Token = token,
                Email = user.Email,
                UserId = user.Id,
                IsFirstLogin = isFirstLogin
            };

            return AppResult<GoogleAuthResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google authentication");
            return AppResult<GoogleAuthResponse>.InternalServerError($"Authentication error: {ex.Message}");
        }
    }

    public async Task<AppResult<LoginResult>> LoginAsync(
        string username,
        string accessCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Find player by credentials
            var playerResult = await _playerRepository.GetByCredentialsAsync(username, accessCode, cancellationToken);
            if (playerResult.IsFailure)
            {
                _logger.LogWarning("Login failed for username {Username}", username);
                return AppResult<LoginResult>.Unauthorized("Invalid username or access code");
            }

            var player = playerResult.Value!;
            if (player.GameId is null)
            {
                _logger.LogWarning("Player {PlayerId} ({Username}) has no associated game", player.Id, username);
                return AppResult<LoginResult>.Unauthorized("Player has no associated game");
            }

            // Get game
            var gameResult = await _gameRepository.GetByIdAsync(player.GameId.Value, cancellationToken);
            if (gameResult.IsFailure)
            {
                _logger.LogError("Game {GameId} not found for player {PlayerId}", player.GameId, player.Id);
                return AppResult<LoginResult>.NotFound("Game not found");
            }

            var game = gameResult.Value!;

            // Generate JWT token for player/game session
            var token = GeneratePlayerJwtToken(player, game);

            var result = new LoginResult
            {
                Token = token,
                Game = game,
                Player = player
            };

            _logger.LogInformation("Player {PlayerId} ({Username}) logged in to game {GameId}", player.Id, username, game.Id);
            return AppResult<LoginResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username {Username}", username);
            return AppResult<LoginResult>.InternalServerError($"Login error: {ex.Message}");
        }
    }

    private string GenerateUserJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "FantaSottone";
        var audience = jwtSettings["Audience"] ?? "FantaSottone";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "480"); // 8 hours default

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("userId", user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GeneratePlayerJwtToken(Player player, Game game)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "FantaSottone";
        var audience = jwtSettings["Audience"] ?? "FantaSottone";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "480"); // 8 hours default

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, player.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, player.Username),
            new Claim("playerId", player.Id.ToString()),
            new Claim("gameId", game.Id.ToString()),
            new Claim("isCreator", player.IsCreator.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
