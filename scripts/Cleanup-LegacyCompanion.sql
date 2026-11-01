-- پاک‌سازی باقی‌مانده‌های «نمایندگی قدیمی» (۲۰۲۶-۱۱-۰۱). فقط تغییرات نرم و برگشت‌پذیر:
--   ۱) خدمت‌های فعال قدیمی نمایندگی‌هایی که به کلینیک/مربی/آرایشگاه منتقل شده‌اند → Active = 0
--   ۲) پکیج‌های مشاوره‌ی با مالک Companion که هیچ خریدی ندارند → Active = 0, Deleted = 1
--   ۳) نوبت‌های مشاوره‌ی با مالک Companion → Active = 0, Deleted = 1
-- شناسه‌های تغییرکرده در خروجی چاپ می‌شوند تا در صورت نیاز برگردانده شوند.
-- اگر تعداد ردیف‌ها با پیش‌نمایش (بخش ۰) نخواند، ROLLBACK کنید.
SET NOCOUNT ON;
SET XACT_ABORT ON;

-- ۰) پیش‌نمایش ------------------------------------------------------------
SELECT N'خریدهای مشاوره با مالک Companion (باید ۰ باشد تا مسیر کد حذف شود)' AS [Check], COUNT(*) AS Cnt
FROM ConsultationPurchases WHERE CompanionId IS NOT NULL
UNION ALL
SELECT N'نوبت‌های مشاوره‌ی فعال با مالک Companion', COUNT(*)
FROM ConsultationAvailabilities WHERE CompanionId IS NOT NULL AND Deleted = 0
UNION ALL
SELECT N'نمایندگی‌های منتقل‌نشده با خدمت فعال (باید ۰ باشد)', COUNT(DISTINCT c.Id)
FROM Companions c
JOIN CompanionAssistances a ON a.CompanionId = c.Id AND a.Deleted = 0 AND a.Active = 1
WHERE c.Deleted = 0
  AND NOT EXISTS (SELECT 1 FROM Clinics k WHERE k.LegacyCompanionId = c.Id AND k.Deleted = 0)
  AND NOT EXISTS (SELECT 1 FROM Coaches k WHERE k.LegacyCompanionId = c.Id AND k.Deleted = 0)
  AND NOT EXISTS (SELECT 1 FROM Groomers g WHERE g.LegacyCompanionId = c.Id AND g.Deleted = 0);

-- وضعیت نمایندگی‌های صاحب پکیج (۶، ۱۰۰۱۶، ۱۰۰۱۷ منتقل نشده‌اند؛ آیا حذف‌شده‌اند؟)
SELECT c.Id, c.Name, c.Deleted, c.Active
FROM Companions c
WHERE c.Id IN (SELECT DISTINCT CompanionId FROM ConsultationPackages WHERE CompanionId IS NOT NULL);
GO

BEGIN TRANSACTION;

-- ۱) خدمت‌های قدیمی مراکز منتقل‌شده
DECLARE @services TABLE (Id bigint);
UPDATE a SET a.Active = 0
OUTPUT inserted.Id INTO @services
FROM CompanionAssistances a
JOIN Companions c ON c.Id = a.CompanionId
WHERE a.Deleted = 0 AND a.Active = 1
  AND (EXISTS (SELECT 1 FROM Clinics k WHERE k.LegacyCompanionId = c.Id AND k.Deleted = 0)
    OR EXISTS (SELECT 1 FROM Coaches k WHERE k.LegacyCompanionId = c.Id AND k.Deleted = 0)
    OR EXISTS (SELECT 1 FROM Groomers g WHERE g.LegacyCompanionId = c.Id AND g.Deleted = 0));
SELECT 'CompanionAssistances deactivated' AS [Step], COUNT(*) AS Cnt,
       STRING_AGG(CAST(Id AS varchar(20)), ',') AS Ids FROM @services;

-- ۲) پکیج‌های مشاوره‌ی بدون خرید با مالک Companion
DECLARE @packages TABLE (Id bigint);
UPDATE p SET p.Active = 0, p.Deleted = 1, p.UpdateDate = GETDATE()
OUTPUT inserted.Id INTO @packages
FROM ConsultationPackages p
WHERE p.CompanionId IS NOT NULL AND p.Deleted = 0
  AND NOT EXISTS (SELECT 1 FROM ConsultationPurchases cp WHERE cp.ConsultationPackageId = p.Id);
SELECT 'ConsultationPackages soft-deleted' AS [Step], COUNT(*) AS Cnt,
       STRING_AGG(CAST(Id AS varchar(20)), ',') AS Ids FROM @packages;

-- ۳) نوبت‌های مشاوره با مالک Companion
DECLARE @slots TABLE (Id bigint);
UPDATE s SET s.Active = 0, s.Deleted = 1
OUTPUT inserted.Id INTO @slots
FROM ConsultationAvailabilities s
WHERE s.CompanionId IS NOT NULL AND s.Deleted = 0;
SELECT 'ConsultationAvailabilities soft-deleted' AS [Step], COUNT(*) AS Cnt,
       STRING_AGG(CAST(Id AS varchar(20)), ',') AS Ids FROM @slots;

-- بعد از بررسی خروجی: COMMIT برای اعمال، ROLLBACK برای لغو
-- COMMIT;
-- ROLLBACK;
