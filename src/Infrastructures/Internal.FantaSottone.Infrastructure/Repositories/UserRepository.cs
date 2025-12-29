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
    
    public async Task<AppResult<IEnumerable<UserSearchDto>>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _context.UserEntity
                .Where(u => u.Username.Contains(searchTerm))
                .Select(u => new UserSearchDto
                {
                    UserId = u.Id,
                    Username = u.Username,
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
                    CreatorUsername = p.Game.CreatorPlayer.User.Username,
                    Status = p.Game.Status,
                    StatusText = GetStatusText(p.Game.Status),
                    CurrentScore = p.CurrentScore,
                    IsCreator = p.IsCreator,
                    UpdatedAt = p.Game.UpdatedAt
                })
                .OrderByDescending(g => g.UpdatedAt)
                .ToListAsync(cancellationToken);

            return AppResult<IEnumerable<GameListItemDto>>.Success(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving games for user {UserId}", userId);
            return AppResult<IEnumerable<GameListItemDto>>.InternalServerError($"Database error: {ex.Message}");
        }
    }

    private static string GetStatusText(byte status)
    {
        return status switch
        {
            1 => "Pending",
            2 => "InProgress",
            3 => "Ended",
            _ => "Unknown"
        };
    }
}
