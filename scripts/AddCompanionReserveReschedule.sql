BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008111132_AddCompanionReserveReschedule'
)
BEGIN
    CREATE TABLE [CompanionReserveReschedules] (
        [Id] bigint NOT NULL IDENTITY,
        [CompanionReserveId] bigint NOT NULL,
        [OldDoDate] datetime2 NOT NULL,
        [OldCompanionTimeId] bigint NULL,
        [NewDoDate] datetime2 NOT NULL,
        [NewCompanionTimeId] bigint NULL,
        [OldStartAt] datetime2 NULL,
        [NewStartAt] datetime2 NOT NULL,
        [Reason] nvarchar(max) NULL,
        [ActorKind] int NOT NULL,
        [ActorUserId] bigint NOT NULL,
        [LinkedTripId] bigint NULL,
        [UserNotified] bit NOT NULL,
        [DriverNotified] bit NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        CONSTRAINT [PK_CompanionReserveReschedules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CompanionReserveReschedules_CompanionReserves_CompanionReserveId] FOREIGN KEY ([CompanionReserveId]) REFERENCES [CompanionReserves] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008111132_AddCompanionReserveReschedule'
)
BEGIN
    CREATE INDEX [IX_CompanionReserveReschedules_CompanionReserveId] ON [CompanionReserveReschedules] ([CompanionReserveId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261008111132_AddCompanionReserveReschedule'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261008111132_AddCompanionReserveReschedule', N'9.0.0');
END;

COMMIT;
GO

