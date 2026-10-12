-- پیامک‌های ارسال اکسپرس: ساخت MessageType و فعال‌سازی (SmsSetting). idempotent.
-- قالب‌ها را جداگانه در کاوه‌نگار (Lookup) بسازید؛ نام قالب = Label (بدون زیرخط). متن‌ها: backend/Docs/MIARE_EXPRESS_SHIPPING_FA.md
BEGIN TRANSACTION;

DECLARE @SmsNumberId BIGINT = (SELECT TOP 1 Id FROM SmsNumbers ORDER BY Id);  -- شماره‌ی Kavenegar Lookup
IF @SmsNumberId IS NULL THROW 51000, 'SmsNumbers is empty', 1;

DECLARE @t TABLE (Label NVARCHAR(100), Name NVARCHAR(200), Body NVARCHAR(1000));
INSERT INTO @t VALUES
(N'ProductOrderExpressStore', N'ارسال اکسپرس - سفارش جدید، همین الان پیک بگیرید (پیامک به فروشگاه)', N'سفارش اکسپرس %token ثبت شد.'+CHAR(10)+N'همین الان کالا را آماده و در اپ فروشنده «درخواست پیک» را بزنید (تا ساعت %token2).'+CHAR(10)+N'پاستیل'),
(N'ProductOrderExpressUser', N'ارسال اکسپرس - ثبت سفارش، ۲ تا ۳ ساعت کاری (پیامک به مشتری)', N'%token عزیز، سفارش اکسپرس %token2 ثبت شد.'+CHAR(10)+N'زمان تحویل: %token3'+CHAR(10)+N'پاستیل');

INSERT INTO MessageTypes (Name, Label, Body, Description, Pattern)
SELECT t.Name, t.Label, t.Body, t.Name, t.Body
FROM @t t
WHERE NOT EXISTS (SELECT 1 FROM MessageTypes m WHERE m.Label = t.Label);

INSERT INTO SmsSettings (SmsNumberId, SmsTypeId)
SELECT @SmsNumberId, m.Id
FROM MessageTypes m
JOIN @t t ON t.Label = m.Label
WHERE NOT EXISTS (SELECT 1 FROM SmsSettings s WHERE s.SmsTypeId = m.Id);

UPDATE m SET m.Name = t.Name, m.Description = t.Name FROM MessageTypes m JOIN @t t ON t.Label = m.Label;

SELECT m.Id, m.Label, s.SmsNumberId FROM MessageTypes m
JOIN @t t ON t.Label = m.Label LEFT JOIN SmsSettings s ON s.SmsTypeId = m.Id;

COMMIT;
