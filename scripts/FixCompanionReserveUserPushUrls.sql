BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922061318_FixCompanionReserveUserPushUrls'
)
BEGIN

    UPDATE p
    SET p.Url = N'/reserve/{2}'
    FROM PushPatterns p
    JOIN PushTypes t ON t.Id = p.PushTypeId
    WHERE t.Label IN (N'PushCompleteReserveUser', N'PushCancelReserveUser')
      AND p.Url = N'/reserve';

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922061318_FixCompanionReserveUserPushUrls'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260922061318_FixCompanionReserveUserPushUrls', N'9.0.0');
END;

COMMIT;
GO

