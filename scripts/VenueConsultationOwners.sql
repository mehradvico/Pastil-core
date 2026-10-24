BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationPurchases]') AND [c].[name] = N'CompanionId');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationPurchases] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [ConsultationPurchases] ALTER COLUMN [CompanionId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    ALTER TABLE [ConsultationPurchases] ADD [ClinicId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    ALTER TABLE [ConsultationPurchases] ADD [CoachId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationPackages]') AND [c].[name] = N'CompanionId');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationPackages] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [ConsultationPackages] ALTER COLUMN [CompanionId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    ALTER TABLE [ConsultationPackages] ADD [ClinicId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    ALTER TABLE [ConsultationPackages] ADD [CoachId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConsultationAvailabilities]') AND [c].[name] = N'CompanionId');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [ConsultationAvailabilities] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [ConsultationAvailabilities] ALTER COLUMN [CompanionId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    ALTER TABLE [ConsultationAvailabilities] ADD [ClinicId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    ALTER TABLE [ConsultationAvailabilities] ADD [CoachId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    ALTER TABLE [CoachUsers] ADD [ConsultationOnline] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    ALTER TABLE [CoachUsers] ADD [ConsultationOnlineExpiresAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    ALTER TABLE [ClinicUsers] ADD [ConsultationOnline] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    ALTER TABLE [ClinicUsers] ADD [ConsultationOnlineExpiresAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    CREATE TABLE [SettlementConsultations] (
        [Id] bigint NOT NULL IDENTITY,
        [SettlementId] bigint NOT NULL,
        [ConsultationPurchaseId] bigint NOT NULL,
        CONSTRAINT [PK_SettlementConsultations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SettlementConsultations_ConsultationPurchases_ConsultationPurchaseId] FOREIGN KEY ([ConsultationPurchaseId]) REFERENCES [ConsultationPurchases] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SettlementConsultations_Settlements_SettlementId] FOREIGN KEY ([SettlementId]) REFERENCES [Settlements] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    CREATE INDEX [IX_ConsultationPurchases_ClinicId_ScheduledStart] ON [ConsultationPurchases] ([ClinicId], [ScheduledStart]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    CREATE INDEX [IX_ConsultationPurchases_ClinicId_Status] ON [ConsultationPurchases] ([ClinicId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    CREATE INDEX [IX_ConsultationPurchases_CoachId_ScheduledStart] ON [ConsultationPurchases] ([CoachId], [ScheduledStart]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    CREATE INDEX [IX_ConsultationPurchases_CoachId_Status] ON [ConsultationPurchases] ([CoachId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    EXEC(N'ALTER TABLE [ConsultationPurchases] ADD CONSTRAINT [CK_ConsultationPurchases_Owner] CHECK ((CASE WHEN [CompanionId] IS NULL THEN 0 ELSE 1 END) + (CASE WHEN [ClinicId] IS NULL THEN 0 ELSE 1 END) + (CASE WHEN [CoachId] IS NULL THEN 0 ELSE 1 END) = 1)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    CREATE INDEX [IX_ConsultationPackages_ClinicId_ChannelId] ON [ConsultationPackages] ([ClinicId], [ChannelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    CREATE INDEX [IX_ConsultationPackages_CoachId_ChannelId] ON [ConsultationPackages] ([CoachId], [ChannelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    EXEC(N'ALTER TABLE [ConsultationPackages] ADD CONSTRAINT [CK_ConsultationPackages_Owner] CHECK ((CASE WHEN [CompanionId] IS NULL THEN 0 ELSE 1 END) + (CASE WHEN [ClinicId] IS NULL THEN 0 ELSE 1 END) + (CASE WHEN [CoachId] IS NULL THEN 0 ELSE 1 END) = 1)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    CREATE INDEX [IX_ConsultationAvailabilities_ClinicId_WeekDayId] ON [ConsultationAvailabilities] ([ClinicId], [WeekDayId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    CREATE INDEX [IX_ConsultationAvailabilities_CoachId_WeekDayId] ON [ConsultationAvailabilities] ([CoachId], [WeekDayId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    EXEC(N'ALTER TABLE [ConsultationAvailabilities] ADD CONSTRAINT [CK_ConsultationAvailabilities_Owner] CHECK ((CASE WHEN [CompanionId] IS NULL THEN 0 ELSE 1 END) + (CASE WHEN [ClinicId] IS NULL THEN 0 ELSE 1 END) + (CASE WHEN [CoachId] IS NULL THEN 0 ELSE 1 END) = 1)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SettlementConsultations_ConsultationPurchaseId] ON [SettlementConsultations] ([ConsultationPurchaseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    CREATE INDEX [IX_SettlementConsultations_SettlementId] ON [SettlementConsultations] ([SettlementId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    ALTER TABLE [ConsultationAvailabilities] ADD CONSTRAINT [FK_ConsultationAvailabilities_Clinics_ClinicId] FOREIGN KEY ([ClinicId]) REFERENCES [Clinics] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    ALTER TABLE [ConsultationAvailabilities] ADD CONSTRAINT [FK_ConsultationAvailabilities_Coaches_CoachId] FOREIGN KEY ([CoachId]) REFERENCES [Coaches] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    ALTER TABLE [ConsultationPackages] ADD CONSTRAINT [FK_ConsultationPackages_Clinics_ClinicId] FOREIGN KEY ([ClinicId]) REFERENCES [Clinics] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    ALTER TABLE [ConsultationPackages] ADD CONSTRAINT [FK_ConsultationPackages_Coaches_CoachId] FOREIGN KEY ([CoachId]) REFERENCES [Coaches] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    ALTER TABLE [ConsultationPurchases] ADD CONSTRAINT [FK_ConsultationPurchases_Clinics_ClinicId] FOREIGN KEY ([ClinicId]) REFERENCES [Clinics] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    ALTER TABLE [ConsultationPurchases] ADD CONSTRAINT [FK_ConsultationPurchases_Coaches_CoachId] FOREIGN KEY ([CoachId]) REFERENCES [Coaches] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024054709_VenueConsultationOwners'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261024054709_VenueConsultationOwners', N'9.0.0');
END;

COMMIT;
GO

