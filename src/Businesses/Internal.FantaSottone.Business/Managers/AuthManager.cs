namespace Internal.FantaSottone.Business.Managers;

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
using System.Security.Cryptography;
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

            // Generate JWT token
            var token = GenerateJwtToken(player, game);

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

    private string GenerateJwtToken(Domain.Models.Player player, Domain.Models.Game game)
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

    public async Task<AppResult<User>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return AppResult<User>.BadRequest("Username is required");

        if (string.IsNullOrWhiteSpace(request.Password))
            return AppResult<User>.BadRequest("Password is required");

        if (string.IsNullOrWhiteSpace(request.Email))
            return AppResult<User>.BadRequest("Email is required");

        if (request.Password.Length < 6)
            return AppResult<User>.BadRequest("Password must be at least 6 characters long");

        var existingUser = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUser.IsSuccess)
            return AppResult<User>.Conflict("Username already exists");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _userRepository.AddAsync(user, cancellationToken);
    }

    public async Task<AppResult<LoginResponse>> LoginUserAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return AppResult<LoginResponse>.BadRequest("Username is required");

        if (string.IsNullOrWhiteSpace(request.Password))
            return AppResult<LoginResponse>.BadRequest("Password is required");

        var userResult = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (userResult.IsFailure)
            return AppResult<LoginResponse>.Unauthorized("Invalid username or password");

        var user = userResult.Value!;

        if (!ValidatePassword(request.Password, user.PasswordHash))
            return AppResult<LoginResponse>.Unauthorized("Invalid username or password");

        var token = GenerateUserToken(user);

        var response = new LoginResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Token = token
        };

        return AppResult<LoginResponse>.Created(response);
    }

    public bool ValidatePassword(string password, string passwordHash)
    {
        var hashedInput = HashPassword(password);
        return hashedInput == passwordHash;
    }

    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private string GenerateUserToken(User user)
    {
        var tokenData = $"{user.Id}:{user.Username}:{DateTime.UtcNow:O}";
        var tokenBytes = Encoding.UTF8.GetBytes(tokenData);
        return Convert.ToBase64String(tokenBytes);
    }
}
