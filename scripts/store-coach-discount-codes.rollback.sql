/* =====================================================================
   رول‌بک scripts/store-coach-discount-codes.sql
   همان مقادیر «تنظیمات» اسکریپت اصلی را اینجا هم بگذارید.
   ===================================================================== */

/* ---------- تنظیمات (همان مقادیر اسکریپت اصلی) ---------- */
DECLARE @StoreId    bigint        = 0;
DECLARE @StoreCode  nvarchar(100) = N'petshop10';
DECLARE @CoachCode  nvarchar(100) = N'coach20';
DECLARE @DiscountId bigint        = NULL;   -- Id تخفیف ثبت‌شده (از خروجی اسکریپت اصلی). NULL = آخرین تخفیف «کل فروشگاه» این پت‌شاپ

SET XACT_ABORT ON;

DECLARE @StoreType bigint = (SELECT Id FROM dbo.Codes WHERE Label = N'DiscountType_Store');
SET @DiscountId = COALESCE(@DiscountId,
    (SELECT TOP 1 Id FROM dbo.Discounts WHERE StoreId = @StoreId AND TypeId = @StoreType AND Deleted = 0 ORDER BY Id DESC));

IF @DiscountId IS NULL OR NOT EXISTS (SELECT 1 FROM dbo.Discounts WHERE Id = @DiscountId AND StoreId = @StoreId)
    THROW 50010, N'تخفیف این پت‌شاپ پیدا نشد؛ @DiscountId را دستی بدهید', 1;

-- قبل از اجرا ببینید چه چیزی برمی‌گردد
SELECT N'discount' AS [what], Id, StoreId, [Percent], EndDate FROM dbo.Discounts WHERE Id = @DiscountId;
SELECT N'rebate' AS [what], Id, CodeValue, UsedCount FROM dbo.Rebate WHERE CodeValue IN (@StoreCode, @CoachCode) AND Deleted = 0;

BEGIN TRAN;

/* ---------- ۱) حذف تخفیف پت‌شاپ و برگرداندن قیمت محصولات ---------- */
DELETE FROM dbo.Discounts WHERE Id = @DiscountId;   -- هیچ جدولی به Discounts کلید خارجی ندارد

DECLARE @Ids nvarchar(50) = CAST(@StoreId AS nvarchar(50));
EXEC dbo.UpdateProductItemDiscount @FilterType = N'Store', @FilterIds = @Ids;
UPDATE dbo.Stores SET MaxDiscountPercent =
    (SELECT ISNULL(MAX([Percent]), 0) FROM dbo.Discounts
     WHERE StoreId = @StoreId AND Active = 1 AND Deleted = 0 AND (EndDate IS NULL OR EndDate >= CAST(GETDATE() AS date)))
WHERE Id = @StoreId;

/* ---------- ۲) حذف دو کد تخفیف ---------- */
DECLARE @Rebates TABLE (Id bigint PRIMARY KEY);
INSERT INTO @Rebates SELECT Id FROM dbo.Rebate WHERE CodeValue IN (@StoreCode, @CoachCode) AND Deleted = 0;

-- از سبدهای باز برداشته شود
UPDATE dbo.Carts SET RebateId = NULL, RebatePrice = 0 WHERE RebateId IN (SELECT Id FROM @Rebates);

-- کدی که در سفارش/رزرو/پرداخت استفاده شده باید بماند (حسابداری) ⇒ فقط غیرفعال و Deleted می‌شود؛ بقیه کامل پاک می‌شوند
-- همه‌ی جدول‌هایی که به Rebate کلید خارجی دارند خودکار چک می‌شوند (temp table چون table variable به dynamic SQL نمی‌رسد)
IF OBJECT_ID('tempdb..#r') IS NOT NULL DROP TABLE #r;
IF OBJECT_ID('tempdb..#u') IS NOT NULL DROP TABLE #u;
CREATE TABLE #r (Id bigint PRIMARY KEY); INSERT INTO #r SELECT Id FROM @Rebates;
CREATE TABLE #u (Id bigint PRIMARY KEY);

DECLARE @sql nvarchar(max) = N'';
SELECT @sql += N'INSERT INTO #u SELECT r.Id FROM #r r WHERE r.Id NOT IN (SELECT Id FROM #u) AND EXISTS (SELECT 1 FROM '
             + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name)
             + N' x WHERE x.' + QUOTENAME(c.name) + N' = r.Id);' + CHAR(10)
FROM sys.foreign_key_columns fkc
JOIN sys.tables  t ON t.object_id = fkc.parent_object_id
JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
WHERE fkc.referenced_object_id = OBJECT_ID(N'dbo.Rebate')
  AND t.name NOT IN (N'Carts', N'UserRebates');
IF @sql <> N'' EXEC sp_executesql @sql;

DECLARE @Used TABLE (Id bigint PRIMARY KEY);
INSERT INTO @Used SELECT Id FROM #u;

UPDATE dbo.Rebate SET Active = 0, Deleted = 1 WHERE Id IN (SELECT Id FROM @Used);

DELETE FROM dbo.UserRebates WHERE RebateId IN (SELECT Id FROM @Rebates WHERE Id NOT IN (SELECT Id FROM @Used));
DELETE FROM dbo.Rebate      WHERE Id       IN (SELECT Id FROM @Rebates WHERE Id NOT IN (SELECT Id FROM @Used));

COMMIT;

SELECT N'kept (used, soft-deleted)' AS [what], Id FROM @Used;
SELECT Id, StoreId, BasePrice, Price FROM dbo.ProductItems WHERE StoreId = @StoreId ORDER BY Id DESC OFFSET 0 ROWS FETCH NEXT 5 ROWS ONLY;
SELECT Id, MaxDiscountPercent FROM dbo.Stores WHERE Id = @StoreId;
GO

/* =====================================================================
   ۳) حذف ستون‌های TargetType / TargetId  — ⚠️ فقط اگر کد بک هم به نسخه‌ی قبل برگشته
   اگر بکِ جدید (با Rebate.TargetType) هنوز بالاست، این بخش را اجرا نکنید:
   EF خطای «Invalid column name 'TargetType'» می‌دهد و همه‌ی کدهای تخفیف از کار می‌افتند.
   ===================================================================== */
/*
BEGIN TRAN;

-- کدهایی که بعداً از پنل با محدودیت ساخته شده‌اند، بعد از حذف ستون «همگانی» می‌شوند ⇒ اول غیرفعال شوند
UPDATE dbo.Rebate SET Active = 0 WHERE TargetType IS NOT NULL AND TargetType <> 1 AND Deleted = 0;

ALTER TABLE dbo.Rebate DROP COLUMN TargetId;
ALTER TABLE dbo.Rebate DROP COLUMN TargetType;
DELETE FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'20261114120000_RebateTarget';

COMMIT;
*/
