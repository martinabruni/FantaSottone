namespace Internal.FantaSottone.Infrastructure.Mappers;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Infrastructure.Models;

/// <summary>
/// Extension methods for mapping between Domain models and Infrastructure entities
/// </summary>
internal static class EntityMapper
{
    // Game mappings
    public static Game ToDomain(this GameEntity entity)
    {
        return new Game
        {
            Id = entity.Id,
            Name = entity.Name,
            InitialScore = entity.InitialScore,
            Status = (GameStatus)entity.Status,
            CreatorPlayerId = entity.CreatorPlayerId,
            WinnerPlayerId = entity.WinnerPlayerId,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static GameEntity ToEntity(this Game model)
    {
        return new GameEntity
        {
            Id = model.Id,
            Name = model.Name,
            InitialScore = model.InitialScore,
            Status = (byte)model.Status,
            CreatorPlayerId = model.CreatorPlayerId,
            WinnerPlayerId = model.WinnerPlayerId,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        };
    }

    // Player mappings
    public static Player ToDomain(this PlayerEntity entity)
    {
        return new Player
        {
            Id = entity.Id,
            GameId = entity.GameId,
            Username = entity.Username,
            AccessCode = entity.AccessCode,
            IsCreator = entity.IsCreator,
            CurrentScore = entity.CurrentScore,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static PlayerEntity ToEntity(this Player model)
    {
        return new PlayerEntity
        {
            Id = model.Id,
            GameId = model.GameId,
            Username = model.Username,
            AccessCode = model.AccessCode,
            IsCreator = model.IsCreator,
            CurrentScore = model.CurrentScore,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        };
    }

    // Rule mappings
    public static Rule ToDomain(this RuleEntity entity)
    {
        return new Rule
        {
            Id = entity.Id,
            GameId = entity.GameId,
            Name = entity.Name,
            RuleType = (RuleType)entity.RuleType,
            ScoreDelta = entity.ScoreDelta,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static RuleEntity ToEntity(this Rule model)
    {
        return new RuleEntity
        {
            Id = model.Id,
            GameId = model.GameId,
            Name = model.Name,
            RuleType = (byte)model.RuleType,
            ScoreDelta = model.ScoreDelta,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        };
    }

    // RuleAssignment mappings
    public static RuleAssignment ToDomain(this RuleAssignmentEntity entity)
    {
        return new RuleAssignment
        {
            Id = entity.Id,
            RuleId = entity.RuleId,
            GameId = entity.GameId,
            AssignedToPlayerId = entity.AssignedToPlayerId,
            ScoreDeltaApplied = entity.ScoreDeltaApplied,
            AssignedAt = entity.AssignedAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static RuleAssignmentEntity ToEntity(this RuleAssignment model)
    {
        return new RuleAssignmentEntity
        {
            Id = model.Id,
            RuleId = model.RuleId,
            GameId = model.GameId,
            AssignedToPlayerId = model.AssignedToPlayerId,
            ScoreDeltaApplied = model.ScoreDeltaApplied,
            AssignedAt = model.AssignedAt,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        };
    }
}
