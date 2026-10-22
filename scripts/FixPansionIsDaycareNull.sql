-- پانسیون‌های قدیمی با IsDaycare = NULL در فهرست‌ها دیده می‌شوند ولی قابل رزرو نیستند (نوع شبانه/مهد مشخص نیست).
-- قاعده: فقط قیمت مهد (SchoolPrice > 0 و PansionPrice = 0) ⇒ مهد (1)؛ در غیر این صورت ⇒ پانسیون شبانه (0).
-- اول بخش «پیش‌نمایش» را اجرا کنید و بعد UPDATE را.

-- پیش‌نمایش
SELECT Id, Name, PansionPrice, SchoolPrice, IsDaycare,
       CASE WHEN SchoolPrice > 0 AND PansionPrice = 0 THEN 1 ELSE 0 END AS NewIsDaycare
FROM Pansions
WHERE IsDaycare IS NULL AND Deleted = 0;

-- اعمال (بعد از بررسی پیش‌نمایش، کامنت را بردارید)
-- UPDATE Pansions
-- SET IsDaycare = CASE WHEN SchoolPrice > 0 AND PansionPrice = 0 THEN 1 ELSE 0 END
-- WHERE IsDaycare IS NULL AND Deleted = 0;
