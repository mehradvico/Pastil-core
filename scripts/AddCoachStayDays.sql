BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021081853_AddCoachStayDays'
)
BEGIN
    ALTER TABLE [CoachServicePackageTypes] ADD [SessionCount] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021081853_AddCoachStayDays'
)
BEGIN
    ALTER TABLE [CoachServicePackageTypes] ADD [StayDays] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021081853_AddCoachStayDays'
)
BEGIN
    ALTER TABLE [CoachReserves] ADD [SessionCount] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021081853_AddCoachStayDays'
)
BEGIN
    ALTER TABLE [CoachReserves] ADD [StayDays] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021081853_AddCoachStayDays'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261021081853_AddCoachStayDays', N'9.0.0');
END;

COMMIT;
GO

