BEGIN TRANSACTION;
ALTER TABLE [ConsultationPackages] ADD [Approved] bit NOT NULL DEFAULT CAST(1 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261025074809_ConsultationPackageApproved', N'9.0.0');

COMMIT;
GO

