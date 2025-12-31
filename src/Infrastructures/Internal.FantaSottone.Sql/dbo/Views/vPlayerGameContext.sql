create VIEW [dbo].[vPlayerGameContext]
AS
SELECT 
    u.Id AS UserId,
    u.Email,
    p.Id AS PlayerId,
    p.GameId,
    p.IsCreator,
    p.CurrentScore,
    g.Name AS GameName,
    g.Status AS GameStatus,
    g.CreatorPlayerId
FROM dbo.UserEntity u
INNER JOIN dbo.PlayerEntity p ON p.UserId = u.Id
INNER JOIN dbo.GameEntity g ON g.Id = p.GameId;