-- شماره کارت و شبای پیش‌ثبت‌نام ارائه‌دهنده (ProviderOnboardingApplications.CardNumber / Iban، هر دو nullable، رمزشده توسط Api)
-- + نوع پیامک «تأیید درخواست ارائه‌دهندگی» (ProviderOnboardingApprovedForUser).
-- چندبار اجرا شدن بی‌خطر است. پیش از دیپلوی Api اجرا شود؛ بدون ستون‌ها Api روی این جدول خطا می‌دهد.
-- قبل از اجرا قالب کاوه‌نگار ProviderOnboardingApprovedForUser باید ساخته شده باشد. متن قالب:
-- پذیرنده گرامی %token
-- درخواست ارائه دهندگی شما در پاستیل ثبت و تأیید شد و اکنون می‌توانید وارد سایت شوید:
-- https://pastil.pet/provider-register
-- برای ادامه فرآیند با پشتیبانی پاستیل ارتباط برقرار کنید.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261116090000_ProviderOnboardingBankInfo')
BEGIN
    IF COL_LENGTH(N'ProviderOnboardingApplications', N'CardNumber') IS NULL
        ALTER TABLE [ProviderOnboardingApplications] ADD [CardNumber] nvarchar(512) NULL;
    IF COL_LENGTH(N'ProviderOnboardingApplications', N'Iban') IS NULL
        ALTER TABLE [ProviderOnboardingApplications] ADD [Iban] nvarchar(512) NULL;
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20261116090000_ProviderOnboardingBankInfo', N'9.0.0');
END;

DECLARE @Label nvarchar(100) = N'ProviderOnboardingApprovedForUser';
DECLARE @Pattern nvarchar(1000) = N'پذیرنده گرامی %token' + NCHAR(10)
    + N'درخواست ارائه دهندگی شما در پاستیل ثبت و تأیید شد و اکنون می‌توانید وارد سایت شوید:' + NCHAR(10)
    + N'https://pastil.pet/provider-register' + NCHAR(10)
    + N'برای ادامه فرآیند با پشتیبانی پاستیل ارتباط برقرار کنید.';

IF NOT EXISTS (SELECT 1 FROM MessageTypes WHERE Label = @Label)
    INSERT INTO MessageTypes (Label, Body, Description, Pattern, Name)
    VALUES (@Label, @Pattern, N'اطلاع تأیید درخواست ارائه‌دهندگی به متقاضی', @Pattern, N'تأیید درخواست ارائه‌دهندگی');

DECLARE @TypeId bigint = (SELECT Id FROM MessageTypes WHERE Label = @Label);
DECLARE @NumberId bigint = (SELECT TOP 1 s.SmsNumberId FROM SmsSettings s JOIN MessageTypes t ON t.Id = s.SmsTypeId WHERE t.Label = N'PansionReserveForUser');

IF @NumberId IS NULL
    THROW 50201, N'شماره‌ی پیامک پیدا نشد.', 1;

IF NOT EXISTS (SELECT 1 FROM SmsSettings WHERE SmsTypeId = @TypeId)
    INSERT INTO SmsSettings (SmsNumberId, SmsTypeId) VALUES (@NumberId, @TypeId);

COMMIT;
GO
