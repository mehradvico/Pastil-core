/* =====================================================================
   تخفیف فروشگاه + کد تخفیف مختص فروشگاه / مربی
   قبل از اجرا: فقط مقادیر بخش «تنظیمات» را عوض کنید. همه‌چیز در یک تراکنش است.
   ===================================================================== */

/* ---------- ۰) ستون‌های جدید Rebate (اگر migration «20261114120000_RebateTarget» اجرا نشده) ---------- */
IF COL_LENGTH('dbo.Rebate', 'TargetType') IS NULL
    ALTER TABLE dbo.Rebate ADD TargetType int NULL;
IF COL_LENGTH('dbo.Rebate', 'TargetId') IS NULL
    ALTER TABLE dbo.Rebate ADD TargetId bigint NULL;
GO
IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'20261114120000_RebateTarget')
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion) VALUES (N'20261114120000_RebateTarget', N'9.0.0');
GO

/* ---------- تنظیمات ---------- */
DECLARE @StoreId          bigint        = 0;      -- شناسه پت‌شاپ
DECLARE @StoreDiscountPct int           = 10;     -- درصد تخفیف کل محصولات پت‌شاپ
DECLARE @StoreDiscountEnd date          = '2026-12-31';
DECLARE @DiscountGroupId  bigint        = NULL;   -- NULL = اولین گروه تخفیف فعال

DECLARE @StoreCode        nvarchar(100) = N'petshop10';   -- کد تخفیف مختص پت‌شاپ (حروف کوچک، بدون فاصله)
DECLARE @StoreCodePct     float         = 15;             -- درصد (۱ تا ۹۹)

DECLARE @CoachId          bigint        = 0;              -- شناسه مربی
DECLARE @CoachCode        nvarchar(100) = N'coach20';     -- کد تخفیف مختص مربی
DECLARE @CoachCodePct     float         = 20;

DECLARE @Start date = CAST(GETDATE() AS date), @End date = '2026-12-31';
DECLARE @UseCount int = 1000, @MaxUsePerUser int = 1;

/* ---------- بررسی‌ها ---------- */
DECLARE @StoreType  bigint = (SELECT Id FROM dbo.Codes WHERE Label = N'DiscountType_Store');
DECLARE @CartType   bigint = (SELECT Id FROM dbo.Codes WHERE Label = N'RebateType_Cart');
DECLARE @CoachType  bigint = (SELECT Id FROM dbo.Codes WHERE Label = N'RebateType_CoachReserve');
SET @DiscountGroupId = COALESCE(@DiscountGroupId, (SELECT TOP 1 Id FROM dbo.DiscountGroups WHERE Active = 1 ORDER BY Id));

IF NOT EXISTS (SELECT 1 FROM dbo.Stores  WHERE Id = @StoreId AND Deleted = 0) THROW 50001, N'StoreId پیدا نشد', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.Coaches WHERE Id = @CoachId)                 THROW 50002, N'CoachId پیدا نشد', 1;
IF @StoreType IS NULL OR @CartType IS NULL OR @CoachType IS NULL                THROW 50003, N'کدهای نوع تخفیف در جدول Codes نیست', 1;
IF @DiscountGroupId IS NULL                                                     THROW 50004, N'هیچ DiscountGroup فعالی نیست', 1;
IF EXISTS (SELECT 1 FROM dbo.Rebate WHERE Deleted = 0 AND CodeValue IN (@StoreCode, @CoachCode)) THROW 50005, N'کد تکراری است', 1;

BEGIN TRAN;

/* ---------- ۱) تخفیف روی همه محصولات یک پت‌شاپ (سهم خود فروشگاه) ---------- */
INSERT INTO dbo.Discounts (TypeId, DiscountGroupId, EndDate, StoreId, [Percent], Synced, Active, Deleted, FundedByPastil)
VALUES (@StoreType, @DiscountGroupId, @StoreDiscountEnd, @StoreId, @StoreDiscountPct, 0, 1, 0, 0);

-- بدون این دو خط قیمت محصولات عوض نمی‌شود (همان کاری که API بعد از ثبت تخفیف می‌کند)
DECLARE @Ids nvarchar(50) = CAST(@StoreId AS nvarchar(50));
EXEC dbo.UpdateProductItemDiscount @FilterType = N'Store', @FilterIds = @Ids;
UPDATE dbo.Stores SET MaxDiscountPercent =
    (SELECT ISNULL(MAX([Percent]), 0) FROM dbo.Discounts
     WHERE StoreId = @StoreId AND Active = 1 AND Deleted = 0 AND (EndDate IS NULL OR EndDate >= CAST(GETDATE() AS date)))
WHERE Id = @StoreId;

/* ---------- ۲) کد تخفیف فقط برای خرید از همین پت‌شاپ (TargetType 2 = Store) ---------- */
INSERT INTO dbo.Rebate (Name, TypeId, CodeValue, PriceValue, MinCartPrice, StartDatetime, EndDatetime,
                        IsPriceRebate, Active, Deleted, UseCount, UsedCount, MaxUsePerUser, TargetType, TargetId)
VALUES (N'کد اختصاصی پت‌شاپ', @CartType, @StoreCode, @StoreCodePct, 0, @Start, @End,
        0, 1, 0, @UseCount, 0, @MaxUsePerUser, 2, @StoreId);

/* ---------- ۳) کد تخفیف فقط برای رزرو همین مربی (TargetType 15 = Coach) ---------- */
INSERT INTO dbo.Rebate (Name, TypeId, CodeValue, PriceValue, MinCartPrice, StartDatetime, EndDatetime,
                        IsPriceRebate, Active, Deleted, UseCount, UsedCount, MaxUsePerUser, TargetType, TargetId)
VALUES (N'کد اختصاصی مربی', @CoachType, @CoachCode, @CoachCodePct, 0, @Start, @End,
        0, 1, 0, @UseCount, 0, @MaxUsePerUser, 15, @CoachId);

COMMIT;

/* ---------- نتیجه (این خروجی را بفرستید) ---------- */
SELECT Id, TypeId, StoreId, [Percent], EndDate, Active FROM dbo.Discounts WHERE StoreId = @StoreId AND Deleted = 0 ORDER BY Id DESC;
SELECT TOP 5 Id, StoreId, BasePrice, Price FROM dbo.ProductItems WHERE StoreId = @StoreId ORDER BY Id DESC;
SELECT Id, Name, CodeValue, TypeId, PriceValue, TargetType, TargetId, StartDatetime, EndDatetime, Active
FROM dbo.Rebate WHERE CodeValue IN (@StoreCode, @CoachCode);
