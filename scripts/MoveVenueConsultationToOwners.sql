-- انتقال مشاوره‌ی آنلاین فوری کلینیک‌ها و مربی‌ها از «Companion لنگر» به خودِ مرکز (ClinicId / CoachId).
-- پیش‌نیاز: اسکریپت VenueConsultationOwners.sql (مایگریشن VenueConsultationOwners) اجرا شده باشد.
--
-- ترتیب اجرا (مهم):
--  ۱) VenueConsultationOwners.sql  (ستون‌ها NULLable می‌شوند؛ کد قدیمی هنوز کار می‌کند چون هیچ ردیفی NULL نیست)
--  ۲) دیپلوی Api و Payment نسخه‌ی جدید
--  ۳) همین اسکریپت (مشاوره‌ی مراکز تا اجرای این اسکریپت در لیست‌ها خالی دیده می‌شود)
--
-- هر Clinic/Coach که LegacyCompanionId دارد (چه Companion قدیمی منتقل‌شده، چه لنگر خودکار):
--  * ConsultationPackages / ConsultationPurchases / ConsultationAvailabilities آن Companion به مرکز منتقل می‌شود
--    (ClinicId/CoachId پر و CompanionId خالی؛ قید CK_*_Owner دقیقاً یک مالک می‌خواهد، پس در یک UPDATE انجام می‌شود).
--  * خریدهای «تکمیل‌شده‌ی تسویه‌نشده» از این به بعد در استخر کلینیک/مربی همان ارائه‌دهنده تسویه می‌شوند؛
--    خریدهای قبلاً تسویه‌شده (Permitted = 1) دست‌نخورده می‌مانند و در تسویه‌ی قبلی دیده می‌شوند.
--  * خود Companion پاک نمی‌شود (سابقه‌ی نمایندگی قدیمی؛ با CompanionMigrationFilter از لیست‌ها مخفی است).
--  * اگر Companionی هم به Clinic و هم به Coach وصل باشد (نباید پیش بیاید) رد می‌شود و در گزارش می‌آید.
--  * Idempotent: بعد از انتقال دیگر ردیفی با CompanionId آن مرکز نمی‌ماند؛ دوباره اجرا کردن چیزی تغییر نمی‌دهد.
-- اول با @Commit = 0 اجرا کنید (ROLLBACK)، شمارش‌ها را ببینید؛ بعد 1.

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Commit bit = 0;   -- 0 = فقط بررسی (ROLLBACK)، 1 = ذخیره

BEGIN TRANSACTION;

-- نگاشت Companion → مرکز (فقط مرکزهای حذف‌نشده؛ Companion دوگانه کنار گذاشته می‌شود)
SELECT m.CompanionId, m.Kind, m.VenueId
INTO #Map
FROM (
    SELECT k.LegacyCompanionId AS CompanionId, 'Clinic' AS Kind, k.Id AS VenueId
    FROM Clinics k WHERE k.LegacyCompanionId IS NOT NULL AND k.Deleted = 0
    UNION ALL
    SELECT k.LegacyCompanionId, 'Coach', k.Id
    FROM Coaches k WHERE k.LegacyCompanionId IS NOT NULL AND k.Deleted = 0
) m
WHERE m.CompanionId IN (
    SELECT CompanionId FROM (
        SELECT k.LegacyCompanionId AS CompanionId FROM Clinics k WHERE k.LegacyCompanionId IS NOT NULL AND k.Deleted = 0
        UNION ALL
        SELECT k.LegacyCompanionId FROM Coaches k WHERE k.LegacyCompanionId IS NOT NULL AND k.Deleted = 0
    ) x GROUP BY CompanionId HAVING COUNT(*) = 1);

SELECT N'Companion دوگانه (رد شد)' AS Warning, x.CompanionId
FROM (
    SELECT k.LegacyCompanionId AS CompanionId FROM Clinics k WHERE k.LegacyCompanionId IS NOT NULL AND k.Deleted = 0
    UNION ALL
    SELECT k.LegacyCompanionId FROM Coaches k WHERE k.LegacyCompanionId IS NOT NULL AND k.Deleted = 0
) x GROUP BY x.CompanionId HAVING COUNT(*) > 1;

-- بسته‌ها
UPDATE p SET
    p.ClinicId = CASE WHEN m.Kind = 'Clinic' THEN m.VenueId END,
    p.CoachId = CASE WHEN m.Kind = 'Coach' THEN m.VenueId END,
    p.CompanionId = NULL
FROM ConsultationPackages p
JOIN #Map m ON m.CompanionId = p.CompanionId;
DECLARE @Packages int = @@ROWCOUNT;

-- خریدها (همه‌ی وضعیت‌ها؛ خریدهای در جریان هم به مرکز می‌روند تا نماینده‌ی مرکز ببیند)
UPDATE p SET
    p.ClinicId = CASE WHEN m.Kind = 'Clinic' THEN m.VenueId END,
    p.CoachId = CASE WHEN m.Kind = 'Coach' THEN m.VenueId END,
    p.CompanionId = NULL
FROM ConsultationPurchases p
JOIN #Map m ON m.CompanionId = p.CompanionId;
DECLARE @Purchases int = @@ROWCOUNT;

-- بازه‌های رزرو
UPDATE a SET
    a.ClinicId = CASE WHEN m.Kind = 'Clinic' THEN m.VenueId END,
    a.CoachId = CASE WHEN m.Kind = 'Coach' THEN m.VenueId END,
    a.CompanionId = NULL
FROM ConsultationAvailabilities a
JOIN #Map m ON m.CompanionId = a.CompanionId;
DECLARE @Availabilities int = @@ROWCOUNT;

SELECT 'ConsultationPackages' AS TableName, @Packages AS Moved
UNION ALL SELECT 'ConsultationPurchases', @Purchases
UNION ALL SELECT 'ConsultationAvailabilities', @Availabilities;

-- کنترل: ردیف مشاوره‌ای که هنوز به Companion منتقل‌شده‌ی یک مرکز وصل مانده باشد نباید باشد
SELECT N'باقی‌مانده روی Companion منتقل‌شده' AS Warning, COUNT(*) AS Rows
FROM (
    SELECT CompanionId FROM ConsultationPackages WHERE CompanionId IN (SELECT CompanionId FROM #Map)
    UNION ALL SELECT CompanionId FROM ConsultationPurchases WHERE CompanionId IN (SELECT CompanionId FROM #Map)
    UNION ALL SELECT CompanionId FROM ConsultationAvailabilities WHERE CompanionId IN (SELECT CompanionId FROM #Map)
) r;

IF @Commit = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT N'COMMIT انجام شد.';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT N'ROLLBACK شد (فقط بررسی). برای ذخیره @Commit را 1 کنید.';
END

DROP TABLE IF EXISTS #Map;
