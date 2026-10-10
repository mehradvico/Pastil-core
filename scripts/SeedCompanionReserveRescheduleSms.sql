-- پیامک‌های تغییر زمان رزرو (کلینیک / مربی / آرایشگاه): ساخت MessageType و فعال‌سازی (SmsSetting). idempotent.
-- قالب‌ها را جداگانه در کاوه‌نگار (Lookup) بسازید؛ نام قالب = Label بدون `_` (اینجا بدون زیرخط است). متن‌ها: backend/Docs/COMPANION_RESERVE_RESCHEDULE_FA.md
BEGIN TRANSACTION;

DECLARE @SmsNumberId BIGINT = (SELECT TOP 1 Id FROM SmsNumbers ORDER BY Id);  -- شماره‌ی Kavenegar Lookup
IF @SmsNumberId IS NULL THROW 51000, 'SmsNumbers is empty', 1;

DECLARE @t TABLE (Label NVARCHAR(100), Name NVARCHAR(200), Body NVARCHAR(1000));
INSERT INTO @t VALUES
(N'CompanionReserveRescheduledUser', N'تغییر زمان رزرو (کلینیک، مربی، آرایشگاه) - پیامک به مشتری', N'زمان رزرو شما در %token از %token2 به %token3 ساعت %token10 تغییر کرد.'+CHAR(10)+N'علت: %token20'+CHAR(10)+N'پاستیل'),
(N'CompanionReserveRescheduledDriver', N'تغییر زمان رزرو (کلینیک، مربی، آرایشگاه) - پیامک به راننده‌ی پت‌رسان', N'راننده‌ی گرامی، زمان سفر پت‌رسان مربوط به رزرو %token3 تغییر کرد.'+CHAR(10)+N'زمان جدید حرکت: %token ساعت %token2'+CHAR(10)+N'پاستیل');

INSERT INTO MessageTypes (Name, Label, Body, Description, Pattern)
SELECT t.Name, t.Label, t.Body, t.Name, t.Body
FROM @t t
WHERE NOT EXISTS (SELECT 1 FROM MessageTypes m WHERE m.Label = t.Label);

INSERT INTO SmsSettings (SmsNumberId, SmsTypeId)
SELECT @SmsNumberId, m.Id
FROM MessageTypes m
JOIN @t t ON t.Label = m.Label
WHERE NOT EXISTS (SELECT 1 FROM SmsSettings s WHERE s.SmsTypeId = m.Id);

-- اگر قبلاً با نام‌های قبلی ساخته‌اید، فقط نام نمایشی به‌روز می‌شود (Label و قالب دست نمی‌خورد)
UPDATE m SET m.Name = t.Name, m.Description = t.Name FROM MessageTypes m JOIN @t t ON t.Label = m.Label;

SELECT m.Id, m.Label, s.SmsNumberId FROM MessageTypes m
JOIN @t t ON t.Label = m.Label LEFT JOIN SmsSettings s ON s.SmsTypeId = m.Id;

COMMIT;
