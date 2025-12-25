namespace Internal.FantaSottone.Business.Managers;

using Internal.FantaSottone.Domain.Managers;
using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

internal sealed class AuthManager : IAuthManager
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IConfiguration _configuration;

    public AuthManager(
        IPlayerRepository playerRepository,
        IGameRepository gameRepository,
        IConfiguration configuration)
    {
        _playerRepository = playerRepository;
        _gameRepository = gameRepository;
        _configuration = configuration;
    }

    public async Task<AppResult<LoginResult>> LoginAsync(
        string username,
        string accessCode,
        CancellationToken cancellationToken = default)
    {
        // Find player by credentials
        var player = await _playerRepository.GetByCredentialsAsync(username, accessCode, cancellationToken);
        if (player == null)
            return AppResult<LoginResult>.Unauthorized("Invalid username or access code");

        // Get game
        var game = await _gameRepository.GetByIdAsync(player.GameId, cancellationToken);
        if (game == null)
            return AppResult<LoginResult>.NotFound("Game not found");

        // Generate JWT token
        var token = GenerateJwtToken(player, game);

        var result = new LoginResult
        {
            Token = token,
            Game = game,
            Player = player
        };

        return AppResult<LoginResult>.Success(result);
    }

    private string GenerateJwtToken(Player player, Game game)
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
