BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024075533_AddUserActivityEvents'
)
BEGIN
    CREATE TABLE [UserActivityEvents] (
        [Id] bigint NOT NULL IDENTITY,
        [UserId] bigint NOT NULL,
        [SessionId] uniqueidentifier NULL,
        [Type] nvarchar(20) NOT NULL,
        [Screen] nvarchar(300) NULL,
        [Target] nvarchar(200) NULL,
        [Platform] nvarchar(20) NULL,
        [DurationSeconds] int NULL,
        [CreateDate] datetime2 NOT NULL,
        CONSTRAINT [PK_UserActivityEvents] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024075533_AddUserActivityEvents'
)
BEGIN
    CREATE INDEX [IX_UserActivityEvents_UserId_CreateDate] ON [UserActivityEvents] ([UserId], [CreateDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024075533_AddUserActivityEvents'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261024075533_AddUserActivityEvents', N'9.0.0');
END;

COMMIT;
GO

