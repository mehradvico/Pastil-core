-- فاز ۰ اتوماسیون پیام (سند pastil-automation، تأیید ۱۴۰۵/۰۸/۲۳). چندبار اجرا شدن بی‌خطر است.
-- 1) قید PointCost قالب کلاب: >= 0 (برای هدیه‌ی سیستمی با امتیاز صفر)
-- 2) قالب کلاب PetProfileGift (اعتبار تشویقی ۲۰۰ هزار تومانی)
-- 3) قاعده‌ی امتیاز رزرو آرایشگاه/کلینیک/مربی (۱۰۰ امتیاز)
-- 4) پوش‌های ۱۷۵ تا ۱۷۸ + متن تازه‌ی پوش اول سبد (۹۳)
-- 5) پیامک‌های PaymentFailedReminder و UnpaidReserveReminder (الگوی کاوه‌نگار با همین نام‌ها لازم است)
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261114090000_AutomationPhase0')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_ClubRewardTemplate_PointCost' AND definition = N'([PointCost]>(0))')
    BEGIN
        ALTER TABLE ClubRewardTemplates DROP CONSTRAINT CK_ClubRewardTemplate_PointCost;
        ALTER TABLE ClubRewardTemplates ADD CONSTRAINT CK_ClubRewardTemplate_PointCost CHECK ([PointCost] >= 0);
    END;

    IF NOT EXISTS (SELECT 1 FROM ClubRewardTemplates WHERE Name = N'PetProfileGift')
        INSERT INTO ClubRewardTemplates (Name, Title, ShortDescription, Description, RewardType, PointCost, StartDate, EndDate,
            ExpirationType, ExpirationValue, FixedExpirationDate, BenefitValue, MaximumBenefitValue, FundingType,
            IsAutomationAllowed, IsManualAllowed, Active, NotificationLevel, PictureId, Terms, CreateDate, UpdateDate, ApplicationMethod)
        VALUES (N'PetProfileGift', N'هدیه تکمیل پروفایل پت',
            N'۲۰۰ هزار تومان اعتبار خرید برای تکمیل پروفایل اولین پتت',
            N'با فعال کردن این هدیه، ۲۰۰ هزار تومان اعتبار به کیف پولت اضافه می‌شود و در همه‌ی خریدها و رزروهای پاستیل قابل استفاده است.',
            4, 0, NULL, NULL, 4, NULL, NULL, 200000, NULL, 1, 0, 0, 1, 3, NULL,
            N'فقط یک‌بار برای هر کاربر، برای اولین پت. اعتبار تا ۳۰ روز بعد از تکمیل پروفایل پت معتبر است.',
            SYSUTCDATETIME(), NULL, 1);

    IF NOT EXISTS (SELECT 1 FROM ClubPointRules WHERE EventType = 9)
        INSERT INTO ClubPointRules (Name, EventType, PointAmount, DailyLimit, MonthlyLimit, LifetimeLimit, Active, StartDate, EndDate, Description, CreateDate, UpdateDate)
        VALUES (N'رزرو آرایشگاه', 9, 100, NULL, NULL, NULL, 1, NULL, NULL, N'به ازای هر رزرو آرایشگاه که انجام شود امتیاز باشگاه پاستیل می‌گیرید.', SYSUTCDATETIME(), NULL);
    IF NOT EXISTS (SELECT 1 FROM ClubPointRules WHERE EventType = 10)
        INSERT INTO ClubPointRules (Name, EventType, PointAmount, DailyLimit, MonthlyLimit, LifetimeLimit, Active, StartDate, EndDate, Description, CreateDate, UpdateDate)
        VALUES (N'رزرو کلینیک', 10, 100, NULL, NULL, NULL, 1, NULL, NULL, N'به ازای هر رزرو کلینیک که انجام شود امتیاز باشگاه پاستیل می‌گیرید.', SYSUTCDATETIME(), NULL);
    IF NOT EXISTS (SELECT 1 FROM ClubPointRules WHERE EventType = 11)
        INSERT INTO ClubPointRules (Name, EventType, PointAmount, DailyLimit, MonthlyLimit, LifetimeLimit, Active, StartDate, EndDate, Description, CreateDate, UpdateDate)
        VALUES (N'رزرو مربی', 11, 100, NULL, NULL, NULL, 1, NULL, NULL, N'به ازای هر رزرو مربی که انجام شود امتیاز باشگاه پاستیل می‌گیرید.', SYSUTCDATETIME(), NULL);

    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 175 AND Label <> N'PushClubPetProfileGift')
        THROW 51000, 'PushType ID 175 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 176 AND Label <> N'PushPaymentFailed')
        THROW 51000, 'PushType ID 176 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 177 AND Label <> N'PushUnpaidReserve')
        THROW 51000, 'PushType ID 177 is already assigned to another label.', 1;
    IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 178 AND Label <> N'PushAbandonedCartSecond')
        THROW 51000, 'PushType ID 178 is already assigned to another label.', 1;

    SET IDENTITY_INSERT PushTypes ON;
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 175)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (175, N'هدیه‌ی تکمیل پروفایل پت در کلاب (به کاربر)', N'PushClubPetProfileGift');
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 176)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (176, N'پرداخت ناموفق (به کاربر)', N'PushPaymentFailed');
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 177)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (177, N'رزرو پرداخت‌نشده (به کاربر)', N'PushUnpaidReserve');
    IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 178)
        INSERT INTO PushTypes (Id, Name, Label) VALUES (178, N'یادآوری دوم سبد رها شده (به کاربر)', N'PushAbandonedCartSecond');
    SET IDENTITY_INSERT PushTypes OFF;

    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 175)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (175, N'هدیه‌ت توی کلاب آماده‌ست 🎁', N'۲۰۰ هزار تومان هدیه‌ت توی کلاب آماده‌ست. تا ۳۰ روز فرصت داری فعالش کنی و خرید کنی.', N'/club', NULL, N'club-pet-profile-gift', 1);
    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 176)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (176, N'پرداختت کامل نشد', N'اگه پولی کم شده تا ۷۲ ساعت برمی‌گرده. دوباره امتحان کن.', N'{0}', NULL, N'payment-failed', 1);
    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 177)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (177, N'نوبتت هنوز قطعی نشده', N'نوبت «{0}» هنوز قطعی نشده و ممکنه پر بشه. پرداختش کن.', N'{1}', NULL, N'unpaid-reserve', 1);
    IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 178)
        INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
        VALUES (178, N'هنوز چیزی توی سبدت مونده', N'با یک لمس سفارشت رو ثبت کن.', N'/store/basket', NULL, N'abandoned-cart-2', 1);
    UPDATE PushPatterns SET Title = N'سبدت منتظرته 🛒', Body = N'خریدت رو کامل کن تا زودتر به دستت برسه.' WHERE PushTypeId = 93;

    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    SELECT pattern.Id, 1
    FROM PushPatterns pattern
    WHERE pattern.PushTypeId IN (175, 176, 177, 178)
      AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);

    IF NOT EXISTS (SELECT 1 FROM MessageTypes WHERE Label = N'PaymentFailedReminder')
        INSERT INTO MessageTypes (Label, Body, Description, Pattern, Name)
        VALUES (N'PaymentFailedReminder',
            N'%token عزیز، پرداختت در پاستیل کامل نشد. برای تکمیل دوباره وارد شو: https://app.pastil.pet',
            N'یادآوری پرداخت ناموفق (۱ ساعت بعد)',
            N'%token عزیز، پرداختت در پاستیل کامل نشد. برای تکمیل دوباره وارد شو: https://app.pastil.pet',
            N'یادآوری پرداخت ناموفق');
    IF NOT EXISTS (SELECT 1 FROM MessageTypes WHERE Label = N'UnpaidReserveReminder')
        INSERT INTO MessageTypes (Label, Body, Description, Pattern, Name)
        VALUES (N'UnpaidReserveReminder',
            N'%token عزیز، نوبتت در پاستیل هنوز پرداخت نشده و قطعی نیست. پرداخت: https://app.pastil.pet/reserve',
            N'یادآوری رزرو پرداخت‌نشده (۳ ساعت بعد)',
            N'%token عزیز، نوبتت در پاستیل هنوز پرداخت نشده و قطعی نیست. پرداخت: https://app.pastil.pet/reserve',
            N'یادآوری رزرو پرداخت‌نشده');

    DECLARE @SmsNumberId bigint = (SELECT TOP 1 s.SmsNumberId FROM SmsSettings s JOIN MessageTypes t ON t.Id = s.SmsTypeId WHERE t.Label = N'UserAbandonedCart');
    IF @SmsNumberId IS NULL
        THROW 51001, 'SMS number of UserAbandonedCart not found.', 1;
    INSERT INTO SmsSettings (SmsNumberId, SmsTypeId)
    SELECT @SmsNumberId, t.Id FROM MessageTypes t
    WHERE t.Label IN (N'PaymentFailedReminder', N'UnpaidReserveReminder')
      AND NOT EXISTS (SELECT 1 FROM SmsSettings s WHERE s.SmsTypeId = t.Id);

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20261114090000_AutomationPhase0', N'9.0.0');
END;
COMMIT;
