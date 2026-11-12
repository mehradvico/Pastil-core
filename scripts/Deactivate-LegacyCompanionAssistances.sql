-- غیرفعال‌سازی همهٔ خدمت‌های باقی‌ماندهٔ «نمایندگی قدیمی» (CompanionAssistances) - ۲۰۲۶-۱۱-۱۲
-- Cleanup-LegacyCompanion.sql (۲۰۲۶-۱۱-۰۱) فقط خدمت مراکزِ دارای LegacyCompanionId را غیرفعال کرد؛
-- ۶۹ ردیف از ۱۶ نمایندگی فعال مانده بود. هیچ رزروی (CompanionReserves) به این ردیف‌ها اشاره نمی‌کند.
-- تغییر نرم و برگشت‌پذیر: فقط Active = 0. شناسه‌ها چاپ می‌شوند تا در صورت نیاز برگردانده شوند:
--   UPDATE CompanionAssistances SET Active = 1 WHERE Id IN (...);
SET NOCOUNT ON;
SET XACT_ABORT ON;

-- پیش‌نمایش: رزرو باز روی این خدمت‌ها باید ۰ باشد
SELECT COUNT(*) AS ActiveRows,
       (SELECT COUNT(*) FROM CompanionReserves r
        JOIN CompanionAssistances x ON x.Id = r.CompanionAssistanceId
        WHERE x.Active = 1 AND x.Deleted = 0 AND r.IsCancel = 0 AND r.DoneDate IS NULL) AS OpenReserves
FROM CompanionAssistances WHERE Active = 1 AND Deleted = 0;

BEGIN TRANSACTION;

DECLARE @ids TABLE (Id bigint);
UPDATE CompanionAssistances SET Active = 0
OUTPUT inserted.Id INTO @ids
WHERE Active = 1 AND Deleted = 0;

SELECT COUNT(*) AS Deactivated, STRING_AGG(CAST(Id AS varchar(20)), ',') AS Ids FROM @ids;

-- بعد از بررسی خروجی: COMMIT برای اعمال، ROLLBACK برای لغو
-- COMMIT;
-- ROLLBACK;
