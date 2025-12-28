namespace Internal.FantaSottone.Business.Services;

using Internal.FantaSottone.Domain.Dtos;
using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Internal.FantaSottone.Domain.Services;

/// <summary>
/// User service implementation
/// </summary>
internal sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AppResult<User>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<AppResult<IEnumerable<User>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetAllAsync(cancellationToken);
    }

    public async Task<AppResult<User>> CreateAsync(User entity, CancellationToken cancellationToken = default)
    {
        return await _userRepository.AddAsync(entity, cancellationToken);
    }

    public async Task<AppResult<User>> UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        return await _userRepository.UpdateAsync(entity, cancellationToken);
    }

    public async Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _userRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<AppResult<User>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByUsernameAsync(username, cancellationToken);
    }

    public async Task<AppResult<IEnumerable<UserSearchDto>>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return AppResult<IEnumerable<UserSearchDto>>.BadRequest("Search term is required");

        return await _userRepository.SearchUsersAsync(searchTerm, cancellationToken);
    }

    public async Task<AppResult<IEnumerable<GameListItemDto>>> GetUserGamesAsync(int userId, CancellationToken cancellationToken = default)
    {
        // Verify user exists
        var userResult = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (userResult.IsFailure)
            return AppResult<IEnumerable<GameListItemDto>>.NotFound("User not found");

        return await _userRepository.GetUserGamesAsync(userId, cancellationToken);
    }
}
