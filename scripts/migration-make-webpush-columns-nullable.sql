BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914122106_MakeWebPushColumnsNullable'
)
BEGIN
    DROP INDEX [IX_PushSubscriptions_Endpoint] ON [PushSubscriptions];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914122106_MakeWebPushColumnsNullable'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PushSubscriptions]') AND [c].[name] = N'P256dh');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [PushSubscriptions] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [PushSubscriptions] ALTER COLUMN [P256dh] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914122106_MakeWebPushColumnsNullable'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PushSubscriptions]') AND [c].[name] = N'Endpoint');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [PushSubscriptions] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [PushSubscriptions] ALTER COLUMN [Endpoint] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914122106_MakeWebPushColumnsNullable'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PushSubscriptions]') AND [c].[name] = N'Auth');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [PushSubscriptions] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [PushSubscriptions] ALTER COLUMN [Auth] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914122106_MakeWebPushColumnsNullable'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PushSubscriptions_Endpoint] ON [PushSubscriptions] ([Endpoint]) WHERE [Endpoint] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914122106_MakeWebPushColumnsNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260914122106_MakeWebPushColumnsNullable', N'9.0.0');
END;

COMMIT;
GO

