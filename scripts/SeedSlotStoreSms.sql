-- پیامک‌های رسمی به فروشگاه برای سفارش‌های ارسال میاره (بازه‌ای/اکسپرس): سفارش جدید + یادآوری مهلت پاسخ‌گویی. idempotent.
-- قالب‌ها را جداگانه در کاوه‌نگار (Lookup) بسازید؛ نام قالب = Label. متن‌ها: backend/Docs/MIARE_SHIPPING_SLOTS_FA.md
BEGIN TRANSACTION;

DECLARE @SmsNumberId BIGINT = (SELECT TOP 1 Id FROM SmsNumbers ORDER BY Id);  -- شماره‌ی Kavenegar Lookup
IF @SmsNumberId IS NULL THROW 51000, 'SmsNumbers is empty', 1;

DECLARE @t TABLE (Label NVARCHAR(100), Name NVARCHAR(200), Body NVARCHAR(1000));
INSERT INTO @t VALUES
(N'ProductOrderSlotStore', N'ارسال میاره - سفارش جدید در انتظار تأیید (پیامک به فروشگاه)',
 N'فروشگاه محترم، شما یک سفارش جدید دارید.'+CHAR(10)+N'شماره سفارش: %token'+CHAR(10)+N'لطفاً تا %token2 در پنل فروشنده پاستیل آن را تأیید یا رد فرمایید.'+CHAR(10)+N'پاستیل'),
(N'ProductOrderSellerReminderStore', N'ارسال میاره - یادآوری مهلت تأیید یا رد سفارش (پیامک به فروشگاه)',
 N'فروشگاه محترم، سفارش شماره %token در پنل فروشنده شما ثبت شده و هنوز پاسخی برای آن ثبت نشده است.'+CHAR(10)+N'مهلت پاسخ‌گویی شما تا %token2 است؛ لطفاً در اسرع وقت آن را تأیید یا رد فرمایید.'+CHAR(10)+N'پاستیل');

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
