CREATE TABLE [dbo].[RuleEntity] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [GameId]     INT            NOT NULL,
    [Name]       NVARCHAR (100) NOT NULL,
    [RuleType]   TINYINT        NOT NULL,
    [ScoreDelta] INT            NOT NULL,
    [CreatedAt]  DATETIME2 (3)  CONSTRAINT [DF_RuleEntity_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAt]  DATETIME2 (3)  CONSTRAINT [DF_RuleEntity_UpdatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_RuleEntity] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_RuleEntity_RuleType] CHECK ([RuleType]=(2) OR [RuleType]=(1)),
    CONSTRAINT [FK_RuleEntity_GameEntity] FOREIGN KEY ([GameId]) REFERENCES [dbo].[GameEntity] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_RuleEntity_GameId]
    ON [dbo].[RuleEntity]([GameId] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_RuleEntity_GameId_Name]
    ON [dbo].[RuleEntity]([GameId] ASC, [Name] ASC);

