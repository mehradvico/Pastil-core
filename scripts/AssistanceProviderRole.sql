-- Assistance.ProviderRoleId: هر خدمتِ کاتالوگ مالکِ یک نقش (ProviderRole) است. Idempotent. قبل از دیپلوی Api اجرا شود.
-- Migration: 20261024103024_AssistanceProviderRole
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261024103024_AssistanceProviderRole')
BEGIN
    ALTER TABLE [Assistances] ADD [ProviderRoleId] bigint NULL;
END;

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261024103024_AssistanceProviderRole')
BEGIN
    CREATE INDEX [IX_Assistances_ProviderRoleId] ON [Assistances] ([ProviderRoleId]);
END;

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261024103024_AssistanceProviderRole')
BEGIN
    ALTER TABLE [Assistances] ADD CONSTRAINT [FK_Assistances_ProviderRoles_ProviderRoleId] FOREIGN KEY ([ProviderRoleId]) REFERENCES [ProviderRoles] ([Id]);
END;

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261024103024_AssistanceProviderRole')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20261024103024_AssistanceProviderRole', N'9.0.0');
END;

COMMIT;
GO

-- ---------------------------------------------------------------------------------------------
-- Backfill (فقط برای ردیف‌های بدون نقش؛ چندباره اجرا شدنش بی‌خطر است). بعدش ادمین از پنل «خدمات» بقیه را دسته‌بندی کند.
--  1) گرومینگ  → Groomer
--  2) خدمتی که فقط مراکز مربی (CoachServices) از آن استفاده می‌کنند → Coach
--  3) بقیه‌ی خدمات غیر گرومینگ → Clinic (همان رفتار قبلیِ کلینیک؛ مربی دیگر آن‌ها را نمی‌بیند)
-- خدمات نماینده‌ی قدیمی (Companion) با نقشِ Clinic/بدون نقش همچنان قابل ثبت است.
-- ---------------------------------------------------------------------------------------------
BEGIN TRANSACTION;

DECLARE @Groomer bigint = (SELECT TOP 1 Id FROM ProviderRoles WHERE Label = N'Groomer' AND Deleted = 0);
DECLARE @Clinic  bigint = (SELECT TOP 1 Id FROM ProviderRoles WHERE Label = N'Clinic'  AND Deleted = 0);
DECLARE @Coach   bigint = (SELECT TOP 1 Id FROM ProviderRoles WHERE Label = N'Coach'   AND Deleted = 0);

IF @Groomer IS NOT NULL
    UPDATE Assistances SET ProviderRoleId = @Groomer WHERE ProviderRoleId IS NULL AND IsGrooming = 1;

IF @Coach IS NOT NULL
    UPDATE a SET ProviderRoleId = @Coach
    FROM Assistances a
    WHERE a.ProviderRoleId IS NULL AND a.IsGrooming = 0
      AND EXISTS (SELECT 1 FROM CoachServices s WHERE s.AssistanceId = a.Id AND s.Deleted = 0)
      AND NOT EXISTS (SELECT 1 FROM ClinicServices s WHERE s.AssistanceId = a.Id AND s.Deleted = 0)
      AND NOT EXISTS (SELECT 1 FROM CompanionAssistances s WHERE s.AssistanceId = a.Id AND s.Deleted = 0);

IF @Clinic IS NOT NULL
    UPDATE Assistances SET ProviderRoleId = @Clinic WHERE ProviderRoleId IS NULL AND IsGrooming = 0;

COMMIT;
GO
