-- تعرفه‌ی پانسیون/مهد بر اساس نوع و سایز پت (ستون‌های اختیاری؛ ردیف‌های فعلی دست نمی‌خورند و همان قیمت پایه‌ی مرکز را می‌گیرند).
-- پیش از استقرار Api اجرا شود. چندبار اجرا شدن بی‌خطر است.
SET NOCOUNT ON;
BEGIN TRANSACTION;

IF COL_LENGTH(N'[PansionPets]', N'PetSize') IS NULL
    ALTER TABLE [PansionPets] ADD [PetSize] nvarchar(16) NULL;

IF COL_LENGTH(N'[PansionPets]', N'PansionPrice') IS NULL
    ALTER TABLE [PansionPets] ADD [PansionPrice] float NULL;

IF COL_LENGTH(N'[PansionPets]', N'SchoolPrice') IS NULL
    ALTER TABLE [PansionPets] ADD [SchoolPrice] float NULL;

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261109060148_PansionPetRates')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261109060148_PansionPetRates', N'9.0.0');

COMMIT;
GO
