-- ستون Assistances.IsGrooming (خدمت گرومینگ فقط برای آرایشگاه). idempotent؛ معادل مایگریشن AddAssistanceIsGrooming.
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261019122523_AddAssistanceIsGrooming')
BEGIN
    IF COL_LENGTH('Assistances', 'IsGrooming') IS NULL
        ALTER TABLE [Assistances] ADD [IsGrooming] bit NOT NULL CONSTRAINT [DF_Assistances_IsGrooming] DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261019122523_AddAssistanceIsGrooming')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261019122523_AddAssistanceIsGrooming', N'9.0.0');
END;

COMMIT;
