-- =============================================
-- Migration 001: Authentication & Multi-Game Support Improvements
-- Date: 2025-12-28
-- Description: Add indexes to optimize user-centric queries
-- =============================================

USE [sqldb-fantastn-webapp-dev];
GO

-- =============================================
-- Add index on PlayerEntity.UserId for efficient user games lookup
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PlayerEntity_UserId' AND object_id = OBJECT_ID('dbo.PlayerEntity'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_PlayerEntity_UserId] 
    ON [dbo].[PlayerEntity]([UserId] ASC)
    INCLUDE ([GameId], [IsCreator], [CurrentScore], [CreatedAt], [UpdatedAt])
    WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) 
    ON [PRIMARY];
    
    PRINT 'Created index IX_PlayerEntity_UserId';
END
ELSE
BEGIN
    PRINT 'Index IX_PlayerEntity_UserId already exists';
END
GO

-- =============================================
-- Add index on GameEntity for user dashboard queries
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GameEntity_UpdatedAt' AND object_id = OBJECT_ID('dbo.GameEntity'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_GameEntity_UpdatedAt] 
    ON [dbo].[GameEntity]([UpdatedAt] DESC)
    WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) 
    ON [PRIMARY];
    
    PRINT 'Created index IX_GameEntity_UpdatedAt';
END
ELSE
BEGIN
    PRINT 'Index IX_GameEntity_UpdatedAt already exists';
END
GO

-- =============================================
-- Verification Queries
-- =============================================

-- Verify indexes were created
SELECT 
    i.name AS IndexName,
    OBJECT_NAME(i.object_id) AS TableName,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName,
    ic.is_included_column AS IsIncluded
FROM sys.indexes AS i
INNER JOIN sys.index_columns AS ic 
    ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.name IN ('IX_PlayerEntity_UserId', 'IX_GameEntity_UpdatedAt')
ORDER BY i.name, ic.key_ordinal;
GO

PRINT 'Migration 001 completed successfully';
GO
