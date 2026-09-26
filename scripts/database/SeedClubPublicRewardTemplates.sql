/*
  جوایز عمومی پاستیل کلاب
  - این اسکریپت idempotent است؛ اجرای دوباره جایزه‌ی هم‌نام را تکرار نمی‌کند.
  - IsAutomationAllowed = 1 یعنی جایزه هنگام باز شدن کلاب، برای هر کاربر واجدشرایط
    به یک پیشنهاد تأییدشده تبدیل و در «مسیر جوایز» او نمایش داده می‌شود.
  - پیش از اجرا، مبلغ‌ها و PointCost را با سیاست مالی نهایی تأیید کنید.
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @Now datetime2 = SYSUTCDATETIME();

INSERT INTO dbo.ClubRewardTemplates
    (Name, Title, ShortDescription, Description, RewardType, ApplicationMethod, PointCost,
     StartDate, EndDate, ExpirationType, ExpirationValue, FixedExpirationDate,
     BenefitValue, MaximumBenefitValue, FundingType, IsAutomationAllowed, IsManualAllowed,
     Active, NotificationLevel, PictureId, Terms, CreateDate, UpdateDate)
SELECT source.Name, source.Title, source.ShortDescription, source.Description, source.RewardType,
       source.ApplicationMethod, source.PointCost, NULL, NULL, 4, NULL, NULL,
       source.BenefitValue, source.MaximumBenefitValue, 1, 1, 0, 1, source.NotificationLevel,
       NULL, source.Terms, @Now, NULL
FROM (VALUES
    (N'club-public-pansion-discount-120k', N'۱۲۰ هزار تومان تخفیف پانسیون',
     N'با ۷۵۰ امتیاز، برای رزرو پانسیون حیوان خانگی‌تان تخفیف بگیرید.',
     N'پس از دریافت جایزه، کد تخفیف یک‌بارمصرف برای رزرو پانسیون در کیف مزایای شما افزوده می‌شود.',
     1, 3, 750, CAST(120000 AS decimal(18,2)), CAST(NULL AS decimal(18,2)), 2,
     N'معتبر تا ۳۰ روز پس از فعال‌شدن جایزه.'),
    (N'club-public-clinic-discount-100k', N'۱۰۰ هزار تومان تخفیف کلینیک',
     N'با ۶۵۰ امتیاز، در رزرو خدمات کلینیک شریک پاستیل تخفیف بگیرید.',
     N'پس از دریافت جایزه، کد تخفیف یک‌بارمصرف برای رزرو خدمات کلینیک در کیف مزایای شما افزوده می‌شود.',
     1, 2, 650, CAST(100000 AS decimal(18,2)), CAST(NULL AS decimal(18,2)), 2,
     N'معتبر تا ۳۰ روز پس از فعال‌شدن جایزه.'),
    (N'club-public-grooming-discount-80k', N'۸۰ هزار تومان تخفیف آرایشگاه',
     N'با ۵۵۰ امتیاز، برای خدمات آرایش و شست‌وشوی پت تخفیف بگیرید.',
     N'پس از دریافت جایزه، کد تخفیف یک‌بارمصرف برای رزرو خدمات آرایشگاه در کیف مزایای شما افزوده می‌شود.',
     1, 2, 550, CAST(80000 AS decimal(18,2)), CAST(NULL AS decimal(18,2)), 1,
     N'معتبر تا ۳۰ روز پس از فعال‌شدن جایزه.'),
    (N'club-public-online-service-credit-60k', N'۶۰ هزار تومان اعتبار خدمات آنلاین',
     N'با ۴۵۰ امتیاز، اعتبار تشویقی برای دریافت خدمات آنلاین پاستیل بگیرید.',
     N'اعتبار پس از دریافت جایزه در کیف مزایای شما ثبت می‌شود و در سرویس‌های آنلاین مجاز قابل مصرف است.',
     4, 2, 450, CAST(60000 AS decimal(18,2)), CAST(NULL AS decimal(18,2)), 1,
     N'معتبر تا ۳۰ روز پس از فعال‌شدن جایزه.'),
    (N'club-public-school-discount-100k', N'۱۰۰ هزار تومان تخفیف مدرسه',
     N'با ۷۰۰ امتیاز، برای ثبت‌نام یا رزرو کلاس مدرسه‌ی پت تخفیف بگیرید.',
     N'پس از دریافت جایزه، کد تخفیف یک‌بارمصرف برای رزرو مدرسه در کیف مزایای شما افزوده می‌شود.',
     1, 2, 700, CAST(100000 AS decimal(18,2)), CAST(NULL AS decimal(18,2)), 2,
     N'معتبر تا ۳۰ روز پس از فعال‌شدن جایزه.'),
    (N'club-public-fixed-discount-75k', N'۷۵ هزار تومان تخفیف خرید',
     N'با ۲۵۰ امتیاز، یک کد تخفیف برای خرید محصولات پاستیل دریافت کنید.',
     N'پس از دریافت جایزه، کد تخفیف یک‌بارمصرف به کیف مزایای شما افزوده می‌شود.',
     1, 1, 250, CAST(75000 AS decimal(18,2)), CAST(NULL AS decimal(18,2)), 1,
     N'معتبر تا ۳۰ روز پس از فعال‌شدن جایزه.'),
    (N'club-public-free-delivery', N'ارسال رایگان سفارش',
     N'با ۵۰۰ امتیاز، هزینه ارسال سفارش بعدی‌تان را تا سقف مشخص دریافت نکنید.',
     N'مزیت ارسال رایگان برای یک سفارش فعال می‌شود و در کیف مزایا قابل مشاهده است.',
     3, 1, 500, CAST(80000 AS decimal(18,2)), CAST(80000 AS decimal(18,2)), 2,
     N'معتبر تا ۳۰ روز پس از فعال‌شدن جایزه.'),
    (N'club-public-wallet-credit-150k', N'۱۵۰ هزار تومان اعتبار پاستیل',
     N'با ۱۰۰۰ امتیاز، اعتبار تشویقی برای سفارش یا رزرو دریافت کنید.',
     N'اعتبار پس از دریافت جایزه به کیف مزایای شما افزوده می‌شود و در سرویس‌های مجاز قابل مصرف است.',
     4, 1, 1000, CAST(150000 AS decimal(18,2)), CAST(NULL AS decimal(18,2)), 3,
     N'معتبر تا ۳۰ روز پس از فعال‌شدن جایزه.')
) AS source(Name, Title, ShortDescription, Description, RewardType, ApplicationMethod, PointCost, BenefitValue, MaximumBenefitValue, NotificationLevel, Terms)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ClubRewardTemplates target WHERE target.Name = source.Name);

-- هر جایزه برای سرویس مجاز خودش فعال است؛ جزئیات مصرف در عنوان و روش اعمال آن مشخص می‌شود.
INSERT INTO dbo.ClubRewardTargets (RewardTemplateId, TargetType, TargetId, IncludeChildren)
SELECT template.Id, 1, NULL, 0
FROM dbo.ClubRewardTemplates template
WHERE template.Name IN (
    N'club-public-pansion-discount-120k',
    N'club-public-clinic-discount-100k',
    N'club-public-grooming-discount-80k',
    N'club-public-online-service-credit-60k',
    N'club-public-school-discount-100k',
    N'club-public-fixed-discount-75k',
    N'club-public-free-delivery',
    N'club-public-wallet-credit-150k'
)
  AND NOT EXISTS (
      SELECT 1 FROM dbo.ClubRewardTargets target
      WHERE target.RewardTemplateId = template.Id AND target.TargetType = 1 AND target.TargetId IS NULL
  );

COMMIT TRANSACTION;
