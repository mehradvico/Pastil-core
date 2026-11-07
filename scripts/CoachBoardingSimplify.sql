BEGIN TRANSACTION;
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CoachServicePackageTypes]') AND [c].[name] = N'MaxDays');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [CoachServicePackageTypes] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [CoachServicePackageTypes] DROP COLUMN [MaxDays];

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CoachServicePackageTypes]') AND [c].[name] = N'MinDays');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [CoachServicePackageTypes] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [CoachServicePackageTypes] DROP COLUMN [MinDays];

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Coaches]') AND [c].[name] = N'ServiceBoarding');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Coaches] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Coaches] DROP COLUMN [ServiceBoarding];

ALTER TABLE [CoachServicePackageTypes] ADD [DailyPrice] float NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261105083506_CoachBoardingSimplify', N'9.0.0');

COMMIT;
GO

