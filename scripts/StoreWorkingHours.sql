-- ساعت کاری فروشگاه برای محدود کردن ارسال اکسپرس (StoreWorkingHours، ۲۰۲۶-۱۱-۰۱). تکرارپذیر: فقط موارد ناموجود ساخته می‌شوند.
-- فروشگاهی که ساعت کاری ثبت نکند رفتار قبلی را دارد (فقط بازه‌ی کلی اکسپرس از تنظیمات اعمال می‌شود).
-- ترتیب: اول این اسکریپت، بعد دیپلوی Api و Payment (هر دو از ShippingQuoteService استفاده می‌کنند).
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'[StoreWorkingHours]', N'U') IS NULL
    EXEC(N'CREATE TABLE [StoreWorkingHours] (
    [Id] bigint NOT NULL IDENTITY,
    [StoreId] bigint NOT NULL,
    [DayOfWeek] int NOT NULL,
    [OpenTime] time NOT NULL,
    [CloseTime] time NOT NULL,
    CONSTRAINT [PK_StoreWorkingHours] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StoreWorkingHours_Stores_StoreId] FOREIGN KEY ([StoreId]) REFERENCES [Stores] ([Id]) ON DELETE CASCADE
)');

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StoreWorkingHours_StoreId_DayOfWeek' AND object_id = OBJECT_ID(N'[StoreWorkingHours]'))
    EXEC(N'CREATE UNIQUE INDEX [IX_StoreWorkingHours_StoreId_DayOfWeek] ON [StoreWorkingHours] ([StoreId], [DayOfWeek])');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261031234949_StoreWorkingHours')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261031234949_StoreWorkingHours', N'9.0.0');

COMMIT;
GO
