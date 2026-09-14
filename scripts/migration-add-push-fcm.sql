BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914100302_AddPushSubscriptionProviderAndFcm'
)
BEGIN
    ALTER TABLE [PushSubscriptions] ADD [FcmToken] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914100302_AddPushSubscriptionProviderAndFcm'
)
BEGIN
    ALTER TABLE [PushSubscriptions] ADD [Platform] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914100302_AddPushSubscriptionProviderAndFcm'
)
BEGIN
    ALTER TABLE [PushSubscriptions] ADD [Provider] bigint NOT NULL DEFAULT CAST(1 AS bigint);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914100302_AddPushSubscriptionProviderAndFcm'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260914100302_AddPushSubscriptionProviderAndFcm', N'9.0.0');
END;

COMMIT;
GO

