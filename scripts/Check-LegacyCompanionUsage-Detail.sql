-- فقط خواندنی (۲۰۲۶-۱۱-۰۱): جزئیات «نمایندگی قدیمی»هایی که هنوز خدمت فعال یا پکیج مشاوره دارند.
-- هیچ داده‌ای تغییر نمی‌کند.
SET NOCOUNT ON;

-- ۱) ۶ نمایندگی با خدمت فعال: آیا به کلینیک/مربی/آرایشگاه منتقل شده‌اند؟
SELECT
    c.Id, c.Name,
    (SELECT COUNT(*) FROM CompanionAssistances a WHERE a.CompanionId = c.Id AND a.Deleted = 0 AND a.Active = 1) AS ActiveServices,
    (SELECT TOP 1 k.Id FROM Clinics k WHERE k.LegacyCompanionId = c.Id AND k.Deleted = 0) AS ClinicId,
    (SELECT TOP 1 k.Id FROM Coaches k WHERE k.LegacyCompanionId = c.Id AND k.Deleted = 0) AS CoachId,
    (SELECT TOP 1 g.Id FROM Groomers g WHERE g.LegacyCompanionId = c.Id AND g.Deleted = 0) AS GroomerId,
    (SELECT TOP 1 p.Id FROM Pansions p WHERE p.CompanionId = c.Id AND p.Deleted = 0) AS PansionId,
    (SELECT TOP 1 s.Id FROM Schools s WHERE s.CompanionId = c.Id AND s.Deleted = 0) AS SchoolId,
    (SELECT MAX(r.CreateDate)
       FROM CompanionReserves r
       JOIN CompanionAssistanceUsers au ON au.Id = r.CompanionAssistanceUserId
       JOIN CompanionAssistances ca ON ca.Id = au.CompanionAssistanceId
      WHERE ca.CompanionId = c.Id) AS LastReserve
FROM Companions c
WHERE c.Deleted = 0
  AND EXISTS (SELECT 1 FROM CompanionAssistances a WHERE a.CompanionId = c.Id AND a.Deleted = 0 AND a.Active = 1)
ORDER BY c.Id;

-- ۲) ۴۰ پکیج مشاوره با مالک Companion: به تفکیک نمایندگی و مقصد انتقال
SELECT
    p.CompanionId, c.Name,
    COUNT(*) AS Packages,
    (SELECT COUNT(*) FROM ConsultationPurchases cp WHERE cp.CompanionId = p.CompanionId) AS PurchasesOnCompanion,
    (SELECT TOP 1 k.Id FROM Clinics k WHERE k.LegacyCompanionId = p.CompanionId AND k.Deleted = 0) AS ClinicId,
    (SELECT TOP 1 k.Id FROM Coaches k WHERE k.LegacyCompanionId = p.CompanionId AND k.Deleted = 0) AS CoachId
FROM ConsultationPackages p
JOIN Companions c ON c.Id = p.CompanionId
WHERE p.CompanionId IS NOT NULL
GROUP BY p.CompanionId, c.Name
ORDER BY p.CompanionId;
