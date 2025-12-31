namespace Internal.FantaSottone.Domain.Models;

/// <summary>
/// Represents a user in the system
/// </summary>
public sealed class User : BaseModel
{
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? Password { get; set; }
}
