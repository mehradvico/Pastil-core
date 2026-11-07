BEGIN TRANSACTION;
ALTER TABLE [CoachServicePackageTypes] ADD [MaxDays] int NULL;

ALTER TABLE [CoachServicePackageTypes] ADD [MinDays] int NULL;

ALTER TABLE [CoachReserves] ADD [DailyPrice] float NULL;

ALTER TABLE [Coaches] ADD [BoardingCapacity] int NOT NULL DEFAULT 0;

ALTER TABLE [Coaches] ADD [ServiceBoarding] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261105080953_CoachBoarding', N'9.0.0');

COMMIT;
GO

