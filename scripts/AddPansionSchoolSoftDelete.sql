BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001104654_AddPansionSchoolSoftDelete'
)
BEGIN
    ALTER TABLE [Schools] ADD [DeleteDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001104654_AddPansionSchoolSoftDelete'
)
BEGIN
    ALTER TABLE [Schools] ADD [Deleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001104654_AddPansionSchoolSoftDelete'
)
BEGIN
    ALTER TABLE [Schools] ADD [DeletedByUserId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001104654_AddPansionSchoolSoftDelete'
)
BEGIN
    ALTER TABLE [Pansions] ADD [DeleteDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001104654_AddPansionSchoolSoftDelete'
)
BEGIN
    ALTER TABLE [Pansions] ADD [Deleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001104654_AddPansionSchoolSoftDelete'
)
BEGIN
    ALTER TABLE [Pansions] ADD [DeletedByUserId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261001104654_AddPansionSchoolSoftDelete'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261001104654_AddPansionSchoolSoftDelete', N'9.0.0');
END;

COMMIT;
GO

