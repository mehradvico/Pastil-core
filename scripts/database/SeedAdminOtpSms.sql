-- پس از ثبت قالب AdminOtp در پنل کاوه‌نگار اجرا شود. اجرای دوباره امن است.
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @Label nvarchar(100) = N'AdminOtp';
DECLARE @Pattern nvarchar(1000) = N'ادمین پاستیل' + NCHAR(10)
    + N'کد ورود شما به پنل ادمین: %token' + NCHAR(10)
    + N'این کد را در اختیار شخص دیگر قرار ندهید.';
DECLARE @SmsNumberId bigint = (
    SELECT TOP 1 s.SmsNumberId
    FROM SmsSettings s
    JOIN MessageTypes t ON t.Id = s.SmsTypeId
    JOIN SmsNumbers n ON n.Id = s.SmsNumberId
    JOIN SmsProviders p ON p.Id = n.SmsProviderId
    WHERE t.Label = N'Otp' AND p.Label = N'KavenegarLookup'
);

IF @SmsNumberId IS NULL
    THROW 51001, N'شماره کاوه‌نگار Lookup برای OTP پیدا نشد.', 1;

IF NOT EXISTS (SELECT 1 FROM MessageTypes WHERE Label = @Label)
    INSERT INTO MessageTypes (Label, Name, Description, Body, Pattern)
    VALUES (@Label, N'کد ورود ادمین', N'کد شش‌رقمی ورود به پنل ادمین', @Pattern, @Pattern);
ELSE
    UPDATE MessageTypes SET Name = N'کد ورود ادمین',
        Description = N'کد شش‌رقمی ورود به پنل ادمین', Body = @Pattern, Pattern = @Pattern
    WHERE Label = @Label;

DECLARE @SmsTypeId bigint = (SELECT Id FROM MessageTypes WHERE Label = @Label);
IF NOT EXISTS (SELECT 1 FROM SmsSettings WHERE SmsTypeId = @SmsTypeId)
    INSERT INTO SmsSettings (SmsNumberId, SmsTypeId) VALUES (@SmsNumberId, @SmsTypeId);
ELSE
    UPDATE SmsSettings SET SmsNumberId = @SmsNumberId WHERE SmsTypeId = @SmsTypeId;

COMMIT;
