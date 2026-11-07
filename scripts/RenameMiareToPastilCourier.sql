-- نام نمایشیِ «میاره» برای مشتری/فروشنده → «پیک پاستیل» (۲۰۲۶-۱۱-۰۷).
-- فقط داده (بدون تغییر جدول)؛ چندبار اجرا شدن بی‌خطر است.
-- عوض می‌شود: نام روش ارسال (Codes.DeliveryType_Miare) که در سبد خرید دیده می‌شود + عنوان/متن پوش‌ها.
-- عوض نمی‌شود: Label/Value کدها (منطق برنامه به آن‌ها وابسته است)، PushTypes.Name (برچسب داخلی پنل)،
-- و پوش ادمینِ «ثبت مشکل در میاره» (PushShipmentNotReceivedAdmin) چون به پنل خود میاره اشاره دارد.
SET NOCOUNT ON;
BEGIN TRANSACTION;

-- پیش‌نمایش
SELECT 'Codes' AS Src, Id, Name FROM Codes WHERE Label = 'DeliveryType_Miare';
SELECT 'PushPatterns' AS Src, p.Id, t.Label, p.Title, p.Body
FROM PushPatterns p INNER JOIN PushTypes t ON t.Id = p.PushTypeId
WHERE (p.Title LIKE N'%میاره%' OR p.Body LIKE N'%میاره%') AND t.Label <> N'PushShipmentNotReceivedAdmin';

UPDATE Codes
SET Name = N'پیک پاستیل'
WHERE Label = 'DeliveryType_Miare' AND Name <> N'پیک پاستیل';
PRINT CONCAT(N'Codes: ', @@ROWCOUNT);

-- «پیک میاره» اول، بعد «میاره»ی تنها (تا «پیک پیک پاستیل» ساخته نشود)
UPDATE p
SET p.Title = REPLACE(REPLACE(p.Title, N'پیک میاره', N'پیک پاستیل'), N'میاره', N'پیک پاستیل'),
    p.Body  = REPLACE(REPLACE(p.Body,  N'پیک میاره', N'پیک پاستیل'), N'میاره', N'پیک پاستیل')
FROM PushPatterns p INNER JOIN PushTypes t ON t.Id = p.PushTypeId
WHERE (p.Title LIKE N'%میاره%' OR p.Body LIKE N'%میاره%') AND t.Label <> N'PushShipmentNotReceivedAdmin';
PRINT CONCAT(N'PushPatterns: ', @@ROWCOUNT);

COMMIT TRANSACTION;
