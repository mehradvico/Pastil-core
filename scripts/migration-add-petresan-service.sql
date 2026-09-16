BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916073726_AddPetResanService'
)
BEGIN
    ALTER TABLE [Trips] ADD [PetResanServiceScheduleId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916073726_AddPetResanService'
)
BEGIN
    CREATE TABLE [PetResanServices] (
        [Id] bigint NOT NULL IDENTITY,
        [UserId] bigint NOT NULL,
        [UserPetId] bigint NOT NULL,
        [Origin] geography NULL,
        [Destination] geography NULL,
        [FromAddress] nvarchar(max) NULL,
        [ToAddress] nvarchar(max) NULL,
        [StartDate] datetime2 NOT NULL,
        [TotalWeeks] int NULL,
        [EndDate] datetime2 NULL,
        [Active] bit NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [CancelDate] datetime2 NULL,
        [PricePerOccurrence] float NOT NULL,
        CONSTRAINT [PK_PetResanServices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PetResanServices_UserPets_UserPetId] FOREIGN KEY ([UserPetId]) REFERENCES [UserPets] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PetResanServices_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916073726_AddPetResanService'
)
BEGIN
    CREATE TABLE [PetResanServiceSchedules] (
        [Id] bigint NOT NULL IDENTITY,
        [PetResanServiceId] bigint NOT NULL,
        [WeekDayId] bigint NOT NULL,
        [Time] nvarchar(max) NULL,
        [Active] bit NOT NULL,
        CONSTRAINT [PK_PetResanServiceSchedules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PetResanServiceSchedules_PetResanServices_PetResanServiceId] FOREIGN KEY ([PetResanServiceId]) REFERENCES [PetResanServices] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PetResanServiceSchedules_WeekDays_WeekDayId] FOREIGN KEY ([WeekDayId]) REFERENCES [WeekDays] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916073726_AddPetResanService'
)
BEGIN
    CREATE INDEX [IX_Trips_PetResanServiceScheduleId] ON [Trips] ([PetResanServiceScheduleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916073726_AddPetResanService'
)
BEGIN
    CREATE INDEX [IX_PetResanServices_UserId] ON [PetResanServices] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916073726_AddPetResanService'
)
BEGIN
    CREATE INDEX [IX_PetResanServices_UserPetId] ON [PetResanServices] ([UserPetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916073726_AddPetResanService'
)
BEGIN
    CREATE INDEX [IX_PetResanServiceSchedules_PetResanServiceId] ON [PetResanServiceSchedules] ([PetResanServiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916073726_AddPetResanService'
)
BEGIN
    CREATE INDEX [IX_PetResanServiceSchedules_WeekDayId] ON [PetResanServiceSchedules] ([WeekDayId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916073726_AddPetResanService'
)
BEGIN
    ALTER TABLE [Trips] ADD CONSTRAINT [FK_Trips_PetResanServiceSchedules_PetResanServiceScheduleId] FOREIGN KEY ([PetResanServiceScheduleId]) REFERENCES [PetResanServiceSchedules] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916073726_AddPetResanService'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260916073726_AddPetResanService', N'9.0.0');
END;

COMMIT;
GO

