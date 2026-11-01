-- فقط خواندنی: آیا هنوز چیزی به «نمایندگی قدیمی» (Companion با خدمت مستقیم) وابسته است؟ (۲۰۲۶-۱۱-۰۱)
-- هیچ داده‌ای تغییر نمی‌کند. خروجی را برای تصمیم درباره‌ی حذف کامل بفرستید.
SET NOCOUNT ON;

-- ۱) نمایندگی‌هایی که هنوز خدمت فعال مستقیم دارند (یعنی واقعاً «نمایندگی قدیمی»اند)
SELECT COUNT(DISTINCT c.Id) AS CompaniesWithActiveServices
FROM Companions c
JOIN CompanionAssistances a ON a.CompanionId = c.Id AND a.Deleted = 0 AND a.Active = 1
WHERE c.Deleted = 0;

-- ۲) رزروهای خدمت نمایندگی که هنوز باز یا آینده‌اند
SELECT COUNT(*) AS OpenOrFutureCompanionReserves
FROM CompanionReserves r
WHERE r.IsCancel = 0 AND r.DoDate >= CAST(GETDATE() AS date);

-- ۳) آخرین رزرو خدمت نمایندگی (برای اینکه ببینیم مسیر هنوز استفاده می‌شود یا نه)
SELECT MAX(r.CreateDate) AS LastCompanionReserveCreated FROM CompanionReserves r;

-- ۴) پکیج‌های مشاوره‌ی فعال که مالکشان هنوز Companion است (نه کلینیک/مربی)
SELECT COUNT(*) AS ConsultationPackagesOwnedByCompanion
FROM ConsultationPackages p
WHERE p.CompanionId IS NOT NULL;

-- ۵) آخرین نمایندگی‌های ساخته‌شده (جدول تاریخ ساخت ندارد؛ بزرگ‌ترین شناسه‌ها = جدیدترین‌ها)
SELECT TOP 5 c.Id, c.Name,
    CASE WHEN EXISTS (SELECT 1 FROM CompanionAssistances a WHERE a.CompanionId = c.Id AND a.Deleted = 0 AND a.Active = 1) THEN 1 ELSE 0 END AS HasActiveServices
FROM Companions c
WHERE c.Deleted = 0
ORDER BY c.Id DESC;
