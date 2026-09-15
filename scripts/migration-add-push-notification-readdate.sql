BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915080241_AddPushNotificationReadDate'
)
BEGIN
    ALTER TABLE [PushNotifications] ADD [ReadDateUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260915080241_AddPushNotificationReadDate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260915080241_AddPushNotificationReadDate', N'9.0.0');
END;

COMMIT;
GO

