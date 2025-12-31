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
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthManager> _logger;

    public AuthManager(
        IUserRepository userRepository,
        IConfiguration _configuration,
        ILogger<AuthManager> logger)
    {
        _userRepository = userRepository;
        this._configuration = _configuration;
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
                return AppResult<GoogleAuthResponse>.Unauthorized("Google token missing email");
            }

            // Check if user exists, create if not
            User user;
            bool isFirstLogin = false;

            var userResult = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (userResult.IsFailure)
            {
                // New user - create account
                isFirstLogin = true;
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

    public async Task<AppResult<EmailAuthResponse>> RegisterWithEmailAsync(
        EmailRegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return AppResult<EmailAuthResponse>.BadRequest("Email is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                return AppResult<EmailAuthResponse>.BadRequest("Password is required");

            // Check if user already exists
            var existingUserResult = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUserResult.IsSuccess)
            {
                return AppResult<EmailAuthResponse>.BadRequest("User with this email already exists");
            }

            // Create new user
            var user = new User
            {
                Email = request.Email.Trim(),
                Password = request.Password.Trim(), // Stored in plain text as requested
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await _userRepository.AddAsync(user, cancellationToken);
            if (createResult.IsFailure)
            {
                _logger.LogError("Failed to create user for email {Email}", request.Email);
                return AppResult<EmailAuthResponse>.InternalServerError("Failed to create user account");
            }

            user = createResult.Value!;
            _logger.LogInformation("Created new user {UserId} for email {Email}", user.Id, request.Email);

            // Generate JWT token
            var token = GenerateUserJwtToken(user);

            var response = new EmailAuthResponse
            {
                Token = token,
                Email = user.Email,
                UserId = user.Id
            };

            return AppResult<EmailAuthResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email registration");
            return AppResult<EmailAuthResponse>.InternalServerError($"Registration error: {ex.Message}");
        }
    }

    public async Task<AppResult<EmailAuthResponse>> LoginWithEmailAsync(
        EmailAuthRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return AppResult<EmailAuthResponse>.BadRequest("Email is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                return AppResult<EmailAuthResponse>.BadRequest("Password is required");

            // Get user by email
            var userResult = await _userRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);
            if (userResult.IsFailure)
            {
                return AppResult<EmailAuthResponse>.Unauthorized("Invalid email or password");
            }

            var user = userResult.Value!;

            // Check password (plain text comparison)
            if (user.Password != request.Password.Trim())
            {
                _logger.LogWarning("Failed login attempt for email {Email}", request.Email);
                return AppResult<EmailAuthResponse>.Unauthorized("Invalid email or password");
            }

            _logger.LogInformation("User {UserId} logged in with email {Email}", user.Id, user.Email);

            // Generate JWT token
            var token = GenerateUserJwtToken(user);

            var response = new EmailAuthResponse
            {
                Token = token,
                Email = user.Email,
                UserId = user.Id
            };

            return AppResult<EmailAuthResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email login");
            return AppResult<EmailAuthResponse>.InternalServerError($"Login error: {ex.Message}");
        }
    }

    private string GenerateUserJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT Key not configured"));
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", user.Id.ToString())
        };

        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
