BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006113523_AddSchoolLocation'
)
BEGIN
    ALTER TABLE [Schools] ADD [Location] geography NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006113523_AddSchoolLocation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261006113523_AddSchoolLocation', N'9.0.0');
END;

COMMIT;
GO

