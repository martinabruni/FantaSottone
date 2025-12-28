namespace Internal.FantaSottone.Domain.Models;

/// <summary>
/// Represents a user in the system
/// </summary>
public sealed class User : BaseModel
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
