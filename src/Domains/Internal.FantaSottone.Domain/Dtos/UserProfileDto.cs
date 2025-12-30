namespace Internal.FantaSottone.Domain.Dtos;

/// <summary>
/// User profile information
/// </summary>
public sealed class UserProfileDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Response containing user profile
/// </summary>
public sealed class GetUserProfileResponse
{
    public UserProfileDto Profile { get; set; } = null!;
}
