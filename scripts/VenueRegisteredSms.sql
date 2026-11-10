-- نوع پیامک «ثبت مرکز جدید برای مالک». فقط داده؛ چندبار اجرا شدن بی‌خطر است.
-- قبل از اجرا قالب VenueRegisteredForOwner باید در پنل کاوه‌نگار تأیید شده باشد.
-- متن قالب:
-- پذیرنده محترم پاستیل
-- ثبت نام %token شما با نام %token2 در سوپراپلیکیشن پاستیل انجام شد.
-- جهت آگاهی از فرایند کار و تکمیل پروفایل و خدمات خود با پشتیبانی پاستیل تماس حاصل نمایید.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @Label nvarchar(100) = N'VenueRegisteredForOwner';
DECLARE @Pattern nvarchar(1000) = N'پذیرنده محترم پاستیل' + NCHAR(10)
    + N'ثبت نام %token شما با نام %token2 در سوپراپلیکیشن پاستیل انجام شد.' + NCHAR(10)
    + N'جهت آگاهی از فرایند کار و تکمیل پروفایل و خدمات خود با پشتیبانی پاستیل تماس حاصل نمایید.';

IF NOT EXISTS (SELECT 1 FROM MessageTypes WHERE Label = @Label)
    INSERT INTO MessageTypes (Label, Body, Description, Pattern, Name)
    VALUES (@Label, @Pattern, N'اطلاع ثبت مرکز جدید به مالک', @Pattern, N'ثبت مرکز جدید برای مالک');

DECLARE @TypeId bigint = (SELECT Id FROM MessageTypes WHERE Label = @Label);
DECLARE @NumberId bigint = (SELECT TOP 1 s.SmsNumberId FROM SmsSettings s JOIN MessageTypes t ON t.Id = s.SmsTypeId WHERE t.Label = N'PansionReserveForUser');

IF @NumberId IS NULL
    THROW 50201, N'شماره‌ی پیامک پیدا نشد.', 1;

IF NOT EXISTS (SELECT 1 FROM SmsSettings WHERE SmsTypeId = @TypeId)
    INSERT INTO SmsSettings (SmsNumberId, SmsTypeId) VALUES (@NumberId, @TypeId);

COMMIT;
GO
