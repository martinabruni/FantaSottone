namespace Internal.FantaSottone.Domain.Models;

public sealed class RuleAssignment : BaseModel
{
    public int RuleId { get; set; }
    public int GameId { get; set; }
    public int AssignedToPlayerId { get; set; }
    public int ScoreDeltaApplied { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}