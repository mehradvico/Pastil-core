-- افزودن گزینه‌های «فوری» به کاتالوگ پایه‌ی خدمات آنلاین (CompanionAssistancePackageOnlines)
-- مطابق الگوی موجود (چت با مربی / تماس پاستیلی(درون‌برنامه) / تماس مستقیم / ویدیو کال پاستیلی)
-- این ۴ ردیف نسخه‌ی «فوری» همان کانال‌ها هستند:
--   چت فوری، تماس فوری داخل برنامه، تماس فوری با شماره شخصی، ویدیو کال فوری
-- قیمت‌ها فقط پیش‌فرض اولیه‌ی نمایشی هستند؛ قیمت واقعی هر Companion را در سطح
-- CompanionAssistancePackageOnlineSelection برای هر پکیجش جدا تعیین می‌کند.

INSERT INTO CompanionAssistancePackageOnlines (Name, Price, Active, Deleted)
VALUES
    (N'چت فوری با مربی', 0, 1, 0),
    (N'تماس فوری داخل برنامه با مربی', 0, 1, 0),
    (N'تماس فوری با شماره شخصی مربی', 0, 1, 0),
    (N'ویدیو کال فوری با مربی', 0, 1, 0);

SELECT * FROM CompanionAssistancePackageOnlines ORDER BY Id;
