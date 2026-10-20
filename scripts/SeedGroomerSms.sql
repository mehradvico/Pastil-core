-- پیامک‌های آرایشگاه (Groomer): ساخت MessageType و فعال‌سازی (SmsSetting). idempotent.
-- قالب‌ها را جداگانه در کاوه‌نگار (Lookup) بسازید؛ نام قالب = Label (بدون زیرخط). متن‌ها: backend/Docs/GROOMER_FA.md
BEGIN TRANSACTION;

DECLARE @SmsNumberId BIGINT = (SELECT TOP 1 Id FROM SmsNumbers ORDER BY Id);  -- شماره‌ی Kavenegar Lookup
IF @SmsNumberId IS NULL THROW 51000, 'SmsNumbers is empty', 1;

DECLARE @t TABLE (Label NVARCHAR(100), Name NVARCHAR(200), Body NVARCHAR(1000));
INSERT INTO @t VALUES
(N'GroomerApprovedForOwner', N'آرایشگاه - تأیید آرایشگاه (به مالک)', N'آرایشگاه %token در پاستیل تأیید شد و اکنون نمایش داده می‌شود.'+CHAR(10)+N'پاستیل'),
(N'GroomerRejectedForOwner', N'آرایشگاه - رد آرایشگاه (به مالک)', N'آرایشگاه %token رد شد.'+CHAR(10)+N'دلیل: %token2'+CHAR(10)+N'پاستیل'),
(N'GroomerReserveForUser', N'آرایشگاه - ثبت رزرو (به کاربر)', N'%token عزیز، رزرو شما در %token2 برای %token3 ساعت %token10 ثبت شد و منتظر تأیید آرایشگاه است.'+CHAR(10)+N'پاستیل'),
(N'GroomerReserveForGroomer', N'آرایشگاه - رزرو جدید (به آرایشگاه)', N'رزرو جدید در %token برای %token2 در %token3 ساعت %token10 ثبت شد. لطفاً در اپ تأیید یا رد کنید.'+CHAR(10)+N'پاستیل'),
(N'GroomerReserveApprovedForUser', N'آرایشگاه - تأیید رزرو (به کاربر)', N'%token عزیز، رزرو شما در %token2 برای %token3 ساعت %token10 تأیید شد.'+CHAR(10)+N'پاستیل'),
(N'GroomerReserveRejectedForUser', N'آرایشگاه - رد رزرو (به کاربر)', N'%token عزیز، رزرو شما در %token2 رد شد و مبلغ به کیف پولتان برگشت.'+CHAR(10)+N'دلیل: %token3'+CHAR(10)+N'پاستیل'),
(N'GroomerReserveExpiredForUser', N'آرایشگاه - عدم پاسخ آرایشگاه و لغو رزرو (به کاربر)', N'%token عزیز، آرایشگاه %token2 در مهلت مقرر پاسخ نداد؛ رزرو لغو و مبلغ به کیف پولتان برگشت.'+CHAR(10)+N'پاستیل'),
(N'GroomerReserveCancelledForGroomer', N'آرایشگاه - لغو رزرو توسط مشتری (به آرایشگاه)', N'رزرو %token در %token2 ساعت %token3 توسط مشتری لغو شد.'+CHAR(10)+N'پاستیل'),
(N'GroomerReserveCancelledByAdminForUser', N'آرایشگاه - لغو رزرو توسط پشتیبانی (به کاربر)', N'%token عزیز، رزرو شما در %token2 توسط پشتیبانی لغو شد.'+CHAR(10)+N'دلیل: %token3'+CHAR(10)+N'پاستیل'),
(N'GroomerReserveRescheduledForUser', N'آرایشگاه - تغییر زمان رزرو (به کاربر)', N'%token عزیز، زمان رزرو شما در %token2 به %token3 ساعت %token10 تغییر کرد.'+CHAR(10)+N'پاستیل'),
(N'GroomerReserveRescheduledForGroomer', N'آرایشگاه - تغییر زمان رزرو توسط پشتیبانی (به آرایشگاه)', N'زمان رزرو %token به %token2 ساعت %token3 تغییر کرد.'+CHAR(10)+N'پاستیل'),
(N'GroomerStaffInvite', N'آرایشگاه - دعوت به پرسنل (به کاربر)', N'%token عزیز، شما به پرسنل آرایشگاه %token2 دعوت شده‌اید. برای پذیرش وارد اپ پاستیل شوید.'+CHAR(10)+N'پاستیل');

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
