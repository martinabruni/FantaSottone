CREATE TABLE [dbo].[RuleAssignmentEntity] (
    [Id]                 INT           IDENTITY (1, 1) NOT NULL,
    [RuleId]             INT           NOT NULL,
    [GameId]             INT           NOT NULL,
    [AssignedToPlayerId] INT           NOT NULL,
    [ScoreDeltaApplied]  INT           NOT NULL,
    [AssignedAt]         DATETIME2 (3) CONSTRAINT [DF_RuleAssignmentEntity_AssignedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    [CreatedAt]          DATETIME2 (3) CONSTRAINT [DF_RuleAssignmentEntity_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAt]          DATETIME2 (3) CONSTRAINT [DF_RuleAssignmentEntity_UpdatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_RuleAssignmentEntity] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RuleAssignmentEntity_GameEntity] FOREIGN KEY ([GameId]) REFERENCES [dbo].[GameEntity] ([Id]),
    CONSTRAINT [FK_RuleAssignmentEntity_PlayerEntity] FOREIGN KEY ([AssignedToPlayerId]) REFERENCES [dbo].[PlayerEntity] ([Id]),
    CONSTRAINT [FK_RuleAssignmentEntity_RuleEntity] FOREIGN KEY ([RuleId]) REFERENCES [dbo].[RuleEntity] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_RuleAssignmentEntity_GameId_AssignedAt]
    ON [dbo].[RuleAssignmentEntity]([GameId] ASC, [AssignedAt] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_RuleAssignmentEntity_GameId_RuleId]
    ON [dbo].[RuleAssignmentEntity]([GameId] ASC, [RuleId] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_RuleAssignmentEntity_RuleId]
    ON [dbo].[RuleAssignmentEntity]([RuleId] ASC);

