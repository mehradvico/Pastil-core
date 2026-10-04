BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004063923_AddProductOrderUserDelivery'
)
BEGIN
    ALTER TABLE [ProductOrders] ADD [UserReceiveNote] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004063923_AddProductOrderUserDelivery'
)
BEGIN
    ALTER TABLE [ProductOrders] ADD [UserReceived] bit NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004063923_AddProductOrderUserDelivery'
)
BEGIN
    ALTER TABLE [ProductOrders] ADD [UserReceivedDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004063923_AddProductOrderUserDelivery'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261004063923_AddProductOrderUserDelivery', N'9.0.0');
END;

COMMIT;
GO

