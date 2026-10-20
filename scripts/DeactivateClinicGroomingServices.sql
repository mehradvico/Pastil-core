-- غیرفعال‌سازی خدمت‌های گرومینگِ کلینیک/نماینده (CompanionAssistances). فقط Active=0؛ چیزی حذف نمی‌شود.
-- پیش‌نیاز: در پنل خدماتِ گرومینگ را با تیک «خدمت گرومینگ» علامت بزنید (Assistances.IsGrooming = 1).
-- @Commit = 0 فقط گزارش می‌دهد؛ @Commit = 1 اعمال می‌کند. رزروهای قبلی دست‌نخورده می‌مانند.
DECLARE @Commit BIT = 0;

BEGIN TRANSACTION;

SELECT ca.Id AS CompanionAssistanceId, ca.CompanionId, c.Name AS CompanionName, a.Name AS AssistanceName, ca.Active
FROM CompanionAssistances ca
JOIN Assistances a ON a.Id = ca.AssistanceId
JOIN Companions c ON c.Id = ca.CompanionId
WHERE a.IsGrooming = 1 AND ca.Deleted = 0 AND ca.Active = 1;

UPDATE ca SET ca.Active = 0
FROM CompanionAssistances ca
JOIN Assistances a ON a.Id = ca.AssistanceId
WHERE a.IsGrooming = 1 AND ca.Deleted = 0 AND ca.Active = 1;

SELECT @@ROWCOUNT AS DeactivatedRows;

IF @Commit = 1 COMMIT; ELSE ROLLBACK;
