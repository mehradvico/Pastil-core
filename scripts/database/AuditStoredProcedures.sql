-- AuditStoredProcedures.sql  (فقط خواندنی — هیچ چیزی را تغییر نمی‌دهد)
--
-- هدف: مطمئن شویم stored procedureهایی که Api صدا می‌زند (پارامتر @FilterIds/@FilterType) هیچ‌جا SQL پویا با
-- الحاق رشته نمی‌سازند (خطر SQL injection مرتبه‌ی دوم). این procedureها در ریپو نیستند و فقط روی خود دیتابیس دیده می‌شوند.
-- خروجی هر بخش را برای بررسی بفرستید.

SET NOCOUNT ON;

------------------------------------------------------------------------------
-- ۱) procedureهای مورد استفاده‌ی کد: آیا وجود دارند و چه پارامتری می‌گیرند؟
------------------------------------------------------------------------------
SELECT p.name AS ProcedureName, pr.name AS ParameterName, TYPE_NAME(pr.user_type_id) AS ParameterType, pr.max_length AS MaxLength
FROM sys.procedures p
LEFT JOIN sys.parameters pr ON pr.object_id = p.object_id
WHERE p.name IN ('UpdateCommentLikes', 'UpdateCompanionCommentsRate', 'UpdateDiscussionAnswerLikes', 'UpdatePansionComments',
                 'UpdateProductComments', 'UpdateProductItemDiscount', 'UpdateStoreCommentsRate', 'ToggleStoryLike')
ORDER BY p.name, pr.parameter_id;

------------------------------------------------------------------------------
-- ۲) مشکوک‌ها: هر procedure/function/trigger/view که SQL پویا اجرا می‌کند.
--    هر ردیف باید دستی بازبینی شود؛ اگر مقدار پارامتر (مثل @FilterIds) با + به رشته الحاق و EXEC می‌شود، آسیب‌پذیر است.
------------------------------------------------------------------------------
SELECT o.type_desc AS ObjectType,
       SCHEMA_NAME(o.schema_id) + '.' + o.name AS ObjectName,
       CASE WHEN m.definition LIKE '%sp_executesql%' THEN 1 ELSE 0 END AS UsesSpExecuteSql,
       CASE WHEN m.definition LIKE '%EXEC(%' OR m.definition LIKE '%EXEC (%' OR m.definition LIKE '%EXECUTE(%' OR m.definition LIKE '%EXECUTE (%' THEN 1 ELSE 0 END AS UsesExecString,
       CASE WHEN m.definition LIKE '%FilterIds%' THEN 1 ELSE 0 END AS MentionsFilterIds
FROM sys.sql_modules m
JOIN sys.objects o ON o.object_id = m.object_id
WHERE o.is_ms_shipped = 0
  AND (m.definition LIKE '%sp_executesql%'
       OR m.definition LIKE '%EXEC(%' OR m.definition LIKE '%EXEC (%'
       OR m.definition LIKE '%EXECUTE(%' OR m.definition LIKE '%EXECUTE (%')
ORDER BY o.type_desc, ObjectName;

------------------------------------------------------------------------------
-- ۳) متن کامل procedureهای Api (برای بازبینی چشمی نحوه‌ی استفاده از @FilterIds)
------------------------------------------------------------------------------
SELECT p.name AS ProcedureName, m.definition AS Definition
FROM sys.procedures p
JOIN sys.sql_modules m ON m.object_id = p.object_id
WHERE p.name IN ('UpdateCommentLikes', 'UpdateCompanionCommentsRate', 'UpdateDiscussionAnswerLikes', 'UpdatePansionComments',
                 'UpdateProductComments', 'UpdateProductItemDiscount', 'UpdateStoreCommentsRate', 'ToggleStoryLike')
ORDER BY p.name;

------------------------------------------------------------------------------
-- راهنمای تفسیر:
--  * امن:   WHERE Id IN (SELECT value FROM STRING_SPLIT(@FilterIds, ','))  یا  join با پارامتر جدولی
--  * ناامن: SET @sql = 'UPDATE ... WHERE Id IN (' + @FilterIds + ')'; EXEC(@sql)
--    اگر ناامن بود، به sp_executesql با پارامتر یا STRING_SPLIT تبدیل شود. (در C# فقط شناسه‌های داخلی به @FilterIds می‌رسند،
--    پس فعلاً قابل‌سوءاستفاده از بیرون نیست؛ این چک برای ایمنی در برابر تغییرهای آینده است.)
------------------------------------------------------------------------------
