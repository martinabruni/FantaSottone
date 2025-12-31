CREATE TABLE [dbo].[GameEntity] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [Name]            NVARCHAR (100) NOT NULL,
    [InitialScore]    INT            NOT NULL,
    [Status]          TINYINT        NOT NULL,
    [CreatorPlayerId] INT            NULL,
    [WinnerPlayerId]  INT            NULL,
    [CreatedAt]       DATETIME2 (3)  CONSTRAINT [DF_GameEntity_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAt]       DATETIME2 (3)  CONSTRAINT [DF_GameEntity_UpdatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_GameEntity] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_GameEntity_Status] CHECK ([Status]=(3) OR [Status]=(2) OR [Status]=(1)),
    CONSTRAINT [FK_GameEntity_CreatorPlayer] FOREIGN KEY ([CreatorPlayerId]) REFERENCES [dbo].[PlayerEntity] ([Id]),
    CONSTRAINT [FK_GameEntity_WinnerPlayer] FOREIGN KEY ([WinnerPlayerId]) REFERENCES [dbo].[PlayerEntity] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_GameEntity_Status]
    ON [dbo].[GameEntity]([Status] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_GameEntity_UpdatedAt]
    ON [dbo].[GameEntity]([UpdatedAt] DESC);

