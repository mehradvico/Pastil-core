-- اگر بعد از deploy و «همگام‌سازی دسترسی‌ها»، آیتم «درخواست افزودن به کاتالوگ» در سایدبار پنل نیامد:
-- همگام‌سازی فقط هنگام «ساخت» یک permission مقدار IsMenu را از AdminPermissionCatalog می‌گذارد و برای ردیف
-- موجود دستش نمی‌زند (تا ویرایش‌های دستی حفظ شود). اگر ردیف MissingProduct وقتی ساخته شده که کنترلر هنوز در
-- MenuControllers نبود، IsMenu=0 مانده و منو خالی می‌ماند. این اسکریپت همان را درست می‌کند.
--
-- قدم ۱ (فقط‌خواندنی): ردیف‌های این کنترلر را ببینید. ردیف «لنگر» = ردیفی که ParentId آن یک گروه ریشه‌ای
-- (مثل «مدیریت فروشگاه») است؛ باید IsMenu = 1 داشته باشد. بقیه‌ی ردیف‌ها (اکشن‌ها) عمداً IsMenu = 0 هستند.
SELECT p.Id, p.Controller, p.Action, p.Label, p.IsMenu, p.Priority, p.ParentId,
       parent.Label AS ParentLabel, parent.ParentId AS ParentsParentId, p.Deleted
FROM Permissions p
LEFT JOIN Permissions parent ON parent.Id = p.ParentId
WHERE p.Area = 'Admin' AND p.Controller = 'MissingProduct'
ORDER BY p.ParentId, p.Id;

-- قدم ۲ (تغییر): فقط ردیف لنگرِ زیرِ گروه ریشه را منو می‌کند؛ اکشن‌ها دست‌نخورده می‌مانند.
-- اول با ROLLBACK اجرا کنید و تعداد ردیف‌های تغییرکرده (باید ۱ باشد) را ببینید؛ بعد COMMIT کنید.
BEGIN TRANSACTION;

UPDATE p
SET p.IsMenu = 1
FROM Permissions p
JOIN Permissions parent ON parent.Id = p.ParentId
WHERE p.Area = 'Admin'
  AND p.Controller = 'MissingProduct'
  AND parent.ParentId IS NULL   -- زیر یک گروه ریشه‌ای (یعنی لنگر، نه اکشن)
  AND p.Deleted = 0
  AND p.IsMenu = 0;

SELECT @@ROWCOUNT AS ChangedRows;

ROLLBACK TRANSACTION;   -- بعد از اطمینان، این خط را به COMMIT TRANSACTION تغییر دهید

-- بعد از COMMIT: در پنل یک‌بار خارج و دوباره وارد شوید (یا Ctrl+F5)؛ منو فقط هنگام بارگذاری پنل خوانده می‌شود.
