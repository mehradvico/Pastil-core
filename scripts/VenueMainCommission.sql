-- کمیسیون اصلی برای پانسیون و مدرسه (پیش‌فرض ۰؛ رفتار فعلی تا تعیین درصد عوض نمی‌شود).
-- پیش از استقرار Api اجرا شود. چندبار اجرا شدن بی‌خطر است.
-- قفل کوتاه: اگر کوئری طولانی جلوی ALTER را گرفت، به‌جای صف‌کردن بقیه‌ی کوئری‌ها خطا می‌دهد و کامل برمی‌گردد؛ دوباره اجرا کنید.
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 3000;
BEGIN TRANSACTION;

IF COL_LENGTH(N'[Pansions]', N'CommissionPercent') IS NULL
    ALTER TABLE [Pansions] ADD [CommissionPercent] decimal(5,2) NOT NULL CONSTRAINT [DF_Pansions_CommissionPercent] DEFAULT 0;

IF COL_LENGTH(N'[Schools]', N'CommissionPercent') IS NULL
    ALTER TABLE [Schools] ADD [CommissionPercent] decimal(5,2) NOT NULL CONSTRAINT [DF_Schools_CommissionPercent] DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261109065951_VenueMainCommission')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261109065951_VenueMainCommission', N'9.0.0');

COMMIT;
GO
