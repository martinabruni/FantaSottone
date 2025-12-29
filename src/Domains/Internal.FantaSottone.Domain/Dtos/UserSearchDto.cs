namespace Internal.FantaSottone.Domain.Dtos;

/// <summary>
/// Simple user information for search/invite features
/// </summary>
public sealed class UserSearchDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
}
