namespace Internal.FantaSottone.Infrastructure.Repositories;

using Internal.FantaSottone.Domain.Dtos;
using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Internal.FantaSottone.Infrastructure.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// User repository implementation
/// </summary>
internal sealed class UserRepository : BaseRepository<User, UserEntity, int>, IUserRepository
{
    public UserRepository(FantaSottoneContext context, ILogger logger) : base(context, logger)
    {
    }

    public async Task<AppResult<User>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.UserEntity
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (entity == null)
                return AppResult<User>.NotFound($"User with email '{email}' not found");

            return AppResult<User>.Success(entity.Adapt<User>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by email {Email}", email);
            return AppResult<User>.InternalServerError($"Database error: {ex.Message}");
        }
    }

    public async Task<AppResult<IEnumerable<UserSearchDto>>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _context.UserEntity
                .Where(u => u.Email.Contains(searchTerm))
                .Select(u => new UserSearchDto
                {
                    UserId = u.Id,
                    Email = u.Email
                })
                .Take(20) // Limit results
                .ToListAsync(cancellationToken);

            return AppResult<IEnumerable<UserSearchDto>>.Success(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with term {SearchTerm}", searchTerm);
            return AppResult<IEnumerable<UserSearchDto>>.InternalServerError($"Database error: {ex.Message}");
        }
    }

    public async Task<AppResult<IEnumerable<GameListItemDto>>> GetUserGamesAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var games = await _context.PlayerEntity
                .Where(p => p.UserId == userId)
                .Include(p => p.Game)
                .ThenInclude(g => g.CreatorPlayer)
                .ThenInclude(cp => cp.User)
                .Select(p => new GameListItemDto
                {
                    GameId = p.Game.Id,
                    GameName = p.Game.Name,
                    Status = p.Game.Status,
                    CreatorEmail = p.Game.CreatorPlayer!.User.Email,
                    CurrentScore = p.CurrentScore,
                    IsCreator = p.IsCreator,
                    CreatedAt = p.Game.CreatedAt
                })
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync(cancellationToken);

            return AppResult<IEnumerable<GameListItemDto>>.Success(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving games for user {UserId}", userId);
            return AppResult<IEnumerable<GameListItemDto>>.InternalServerError($"Database error: {ex.Message}");
        }
    }
}
