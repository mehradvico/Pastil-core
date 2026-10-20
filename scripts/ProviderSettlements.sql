BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019073251_ProviderSettlements'
)
BEGIN
    ALTER TABLE [Settlements] ADD [Kind] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019073251_ProviderSettlements'
)
BEGIN
    ALTER TABLE [Settlements] ADD [ProviderId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019073251_ProviderSettlements'
)
BEGIN
    CREATE INDEX [IX_Settlements_ProviderId_Kind] ON [Settlements] ([ProviderId], [Kind]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019073251_ProviderSettlements'
)
BEGIN
    ALTER TABLE [Settlements] ADD CONSTRAINT [FK_Settlements_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019073251_ProviderSettlements'
)
BEGIN

    UPDATE Settlements SET Kind = CASE WHEN StoreId IS NOT NULL THEN 2 ELSE 1 END;

    UPDATE s SET s.ProviderId = c.ProviderId
    FROM Settlements s JOIN Companions c ON c.Id = s.CompanionId
    WHERE s.CompanionId IS NOT NULL AND c.ProviderId IS NOT NULL;

    UPDATE s SET s.ProviderId = st.ProviderId
    FROM Settlements s JOIN Stores st ON st.Id = s.StoreId
    WHERE s.StoreId IS NOT NULL AND st.ProviderId IS NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019073251_ProviderSettlements'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261019073251_ProviderSettlements', N'9.0.0');
END;

COMMIT;
GO


-- ============================================================
-- بعد از اجرا بازبینی کنید (همه باید خالی باشند وگرنه تسویه برای آن‌ها ممکن نیست):
-- ۱) نمایندگی/فروشگاهِ حذف‌نشده‌ای که ارائه‌دهنده ندارد:
-- SELECT 'Companion' AS Kind, Id, Name FROM Companions WHERE Deleted = 0 AND ProviderId IS NULL
-- UNION ALL SELECT 'Store', Id, Name FROM Stores WHERE Deleted = 0 AND ProviderId IS NULL;
-- ۲) تسویه‌ی قدیمی که ارائه‌دهنده‌اش پیدا نشد:
-- SELECT Id, Kind, CompanionId, StoreId, PaidPrice, CreateDate FROM Settlements WHERE ProviderId IS NULL;
-- ============================================================
