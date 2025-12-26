namespace Internal.FantaSottone.Domain.Models;

public sealed class Rule : BaseModel
{
    public int GameId { get; set; }
    public string Name { get; set; } = string.Empty;
    public RuleType RuleType { get; set; }
    public int ScoreDelta { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}