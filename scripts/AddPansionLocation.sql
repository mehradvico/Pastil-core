BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006112952_AddPansionLocation'
)
BEGIN
    ALTER TABLE [Pansions] ADD [Location] geography NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006112952_AddPansionLocation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261006112952_AddPansionLocation', N'9.0.0');
END;

COMMIT;
GO

