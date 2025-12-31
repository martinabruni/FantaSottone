namespace Internal.FantaSottone.Domain.Models;

public sealed class Player : BaseModel
{
    public int? GameId { get; set; }
    public int? UserId { get; set; }
    public bool IsCreator { get; set; }
    public int CurrentScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public User? User { get; set; }
}
