-- زمان/عامل لغو سفارش و زمان «در حال پردازش» شدن (ProductOrders): CancelledDate, CancelledByActor, CancelledStoreId, ProcessingDate.
-- معادل migration «20261115180000_OrderCancelAndProcessingDates». چندبار اجرا شدن بی‌خطر است.
-- بدون این ستون‌ها Api (نسخه‌ی جدید) روی هر کوئری ProductOrder خطا می‌دهد؛ پیش از دیپلوی Api (و Payment) اجرا شود.
-- لغوهای قدیمی: زمان از ثبت برگشت پول لغو در کیف پول پر می‌شود؛ عامل لغو نامشخص (NULL) می‌ماند.
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON; -- sqlcmd بدون -I آن را خاموش می‌گیرد و UPDATE روی جدول‌های دارای فهرست خطا می‌دهد
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261115180000_OrderCancelAndProcessingDates')
BEGIN
    IF COL_LENGTH(N'ProductOrders', N'CancelledDate') IS NULL
        ALTER TABLE [ProductOrders] ADD [CancelledDate] datetime2 NULL;
    IF COL_LENGTH(N'ProductOrders', N'CancelledByActor') IS NULL
        ALTER TABLE [ProductOrders] ADD [CancelledByActor] int NULL;
    IF COL_LENGTH(N'ProductOrders', N'CancelledStoreId') IS NULL
        ALTER TABLE [ProductOrders] ADD [CancelledStoreId] bigint NULL;
    IF COL_LENGTH(N'ProductOrders', N'ProcessingDate') IS NULL
        ALTER TABLE [ProductOrders] ADD [ProcessingDate] datetime2 NULL;

    EXEC(N'
        UPDATE o SET o.CancelledDate = w.CreateDate
        FROM ProductOrders o
        CROSS APPLY (
            SELECT TOP 1 CreateDate FROM Wallets
            WHERE Name = N''OrderCancelRefund:'' + o.Id AND Deleted = 0
            ORDER BY CreateDate
        ) w
        WHERE o.CancelledDate IS NULL;');

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20261115180000_OrderCancelAndProcessingDates', N'9.0.0');
END;

COMMIT TRANSACTION;
