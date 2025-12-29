CREATE TABLE [dbo].[PlayerEntity] (
    [Id]           INT           IDENTITY (1, 1) NOT NULL,
    [GameId]       INT           NULL,
    [IsCreator]    BIT           CONSTRAINT [DF_PlayerEntity_IsCreator] DEFAULT ((0)) NOT NULL,
    [CurrentScore] INT           NOT NULL,
    [CreatedAt]    DATETIME2 (3) CONSTRAINT [DF_PlayerEntity_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAt]    DATETIME2 (3) CONSTRAINT [DF_PlayerEntity_UpdatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    [UserId]       INT           NULL,
    [Username]     NVARCHAR (30) NULL,
    [AccessCode]   NVARCHAR (32) NULL,
    CONSTRAINT [PK_PlayerEntity] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PlayerEntity_GameEntity] FOREIGN KEY ([GameId]) REFERENCES [dbo].[GameEntity] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PlayerEntity_UserEntity] FOREIGN KEY ([UserId]) REFERENCES [dbo].[UserEntity] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_PlayerEntity_GameId_CurrentScore]
    ON [dbo].[PlayerEntity]([GameId] ASC, [CurrentScore] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_PlayerEntity_UserId]
    ON [dbo].[PlayerEntity]([UserId] ASC)
    INCLUDE([GameId], [IsCreator], [CurrentScore], [CreatedAt], [UpdatedAt]);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_PlayerEntity_GameId_UserId_NotNull]
    ON [dbo].[PlayerEntity]([GameId] ASC, [UserId] ASC) WHERE ([UserId] IS NOT NULL);

