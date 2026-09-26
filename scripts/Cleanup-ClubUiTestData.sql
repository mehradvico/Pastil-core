/*
  پاک‌سازی داده‌ی تستی پاستیل‌کلاب که مایگریشن 20260811084853_SeedPastilClubUiTestData ساخته است.

  فقط ردیف‌هایی حذف می‌شوند که با این نشانه‌ها ساخته شده‌اند:
    - ClubRewardTemplates.Name                LIKE 'ui-test-club-%'
    - ClubPointTransactions.IdempotencyKey    LIKE 'ui-test:club:%'
    - ClubRewardRedemptions.IdempotencyKey    LIKE 'ui-test:club:%'
    - Rebate.CodeValue                        LIKE 'UITEST-CLUB-%'

  روش استفاده:
    1) اول همین‌طور اجرا کن (@Apply = 0): فقط پیش‌نمایش و شمارش می‌دهد و آخرش ROLLBACK می‌کند.
    2) اگر شمارش‌ها درست بود، @Apply را 1 کن و دوباره اجرا کن.
    3) قبلش از دیتابیس بکاپ بگیر.

  نکته: مایگریشن اصلی این داده‌ها را برای «اولین کاربر فعال» (یا موبایل 09339750767) ساخته؛
  پیش‌نمایش اول، همان کاربر را نشان می‌دهد تا مطمئن شوی حساب امتیاز درستی اصلاح می‌شود.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Apply bit = 0;   -- 0 = پیش‌نمایش (ROLLBACK)، 1 = اعمال واقعی (COMMIT)

BEGIN TRAN;

-- ── پیش‌نمایش ─────────────────────────────────────────────────────────────
SELECT 'Templates'      AS [What], COUNT(*) AS [Rows] FROM [ClubRewardTemplates] WHERE [Name] LIKE N'ui-test-club-%'
UNION ALL SELECT 'Offers',        COUNT(*) FROM [ClubRewardOffers] o JOIN [ClubRewardTemplates] t ON t.[Id] = o.[RewardTemplateId] WHERE t.[Name] LIKE N'ui-test-club-%'
UNION ALL SELECT 'Redemptions',   COUNT(*) FROM [ClubRewardRedemptions] WHERE [IdempotencyKey] LIKE N'ui-test:club:%'
UNION ALL SELECT 'Coupons',       COUNT(*) FROM [ClubCoupons] c JOIN [ClubRewardRedemptions] r ON r.[Id] = c.[RewardRedemptionId] WHERE r.[IdempotencyKey] LIKE N'ui-test:club:%'
UNION ALL SELECT 'PointTx',       COUNT(*) FROM [ClubPointTransactions] WHERE [IdempotencyKey] LIKE N'ui-test:club:%'
UNION ALL SELECT 'Rebates',       COUNT(*) FROM [Rebate] WHERE [CodeValue] LIKE N'UITEST-CLUB-%' AND [Deleted] = 0;

-- کاربری که داده‌ی تستی روی حساب امتیازش نشسته (قبل از اصلاح)
SELECT a.[UserId], u.[Mobile], a.[AvailablePoint], a.[LifetimeEarnedPoint], a.[LifetimeSpentPoint], a.[LifetimeReversedPoint]
FROM [ClubPointAccounts] a
JOIN [Users] u ON u.[Id] = a.[UserId]
WHERE EXISTS (SELECT 1 FROM [ClubPointTransactions] t WHERE t.[UserId] = a.[UserId] AND t.[IdempotencyKey] LIKE N'ui-test:club:%');

-- ── اصلاح حساب امتیاز (معکوسِ همان اعدادی که مایگریشن اضافه کرده بود) ────────
-- فقط اگر تراکنش دفتر تستی شماره 01 وجود داشته باشد.
IF EXISTS (SELECT 1 FROM [ClubPointTransactions] WHERE [IdempotencyKey] = N'ui-test:club:ledger:01')
BEGIN
    DECLARE @HadRedeem bit = CASE WHEN EXISTS (SELECT 1 FROM [ClubRewardRedemptions] WHERE [IdempotencyKey] = N'ui-test:club:redemption:04') THEN 1 ELSE 0 END;

    UPDATE a
    SET a.[AvailablePoint]        = a.[AvailablePoint] - 2500 + CASE WHEN @HadRedeem = 1 THEN 850 ELSE 0 END,
        a.[LifetimeEarnedPoint]   = a.[LifetimeEarnedPoint] - 2950,
        a.[LifetimeSpentPoint]    = a.[LifetimeSpentPoint] - 500 - CASE WHEN @HadRedeem = 1 THEN 850 ELSE 0 END,
        a.[LifetimeReversedPoint] = a.[LifetimeReversedPoint] - 50,
        a.[LastUpdateDate]        = SYSUTCDATETIME()
    FROM [ClubPointAccounts] a
    WHERE a.[UserId] IN (SELECT [UserId] FROM [ClubPointTransactions] WHERE [IdempotencyKey] = N'ui-test:club:ledger:01');
END;

-- ── حذف (به ترتیب وابستگی) ────────────────────────────────────────────────
UPDATE [ClubPointTransactions] SET [RewardRedemptionId] = NULL
WHERE [IdempotencyKey] LIKE N'ui-test:club:%' AND [RewardRedemptionId] IS NOT NULL;

-- دریافت‌هایی که یا کلید تستی دارند یا به قالب/پیشنهادِ تستی وصل‌اند (این‌ها جلوی حذف پیشنهاد/قالب را می‌گرفتند)
DECLARE @TestRedemptions TABLE ([Id] bigint PRIMARY KEY);
INSERT INTO @TestRedemptions ([Id])
SELECT r.[Id] FROM [ClubRewardRedemptions] r
WHERE r.[IdempotencyKey] LIKE N'ui-test:club:%'
   OR r.[RewardTemplateId] IN (SELECT [Id] FROM [ClubRewardTemplates] WHERE [Name] LIKE N'ui-test-club-%')
   OR r.[RewardOfferId] IN (SELECT o.[Id] FROM [ClubRewardOffers] o JOIN [ClubRewardTemplates] t ON t.[Id] = o.[RewardTemplateId] WHERE t.[Name] LIKE N'ui-test-club-%');

-- هشدار: دریافتی که کلید تستی ندارد ولی به قالب تستی وصل است (یعنی کاربر واقعی گرفته)
SELECT r.[Id], r.[UserId], r.[IdempotencyKey], r.[PointSpent], r.[RedeemedDate]
FROM [ClubRewardRedemptions] r
WHERE r.[Id] IN (SELECT [Id] FROM @TestRedemptions) AND r.[IdempotencyKey] NOT LIKE N'ui-test:club:%';

UPDATE [ClubPointTransactions] SET [RewardRedemptionId] = NULL
WHERE [RewardRedemptionId] IN (SELECT [Id] FROM @TestRedemptions);

DELETE FROM [ClubCoupons] WHERE [RewardRedemptionId] IN (SELECT [Id] FROM @TestRedemptions);

DELETE FROM [ClubRewardRedemptions] WHERE [Id] IN (SELECT [Id] FROM @TestRedemptions);

DELETE FROM [ClubPointTransactions] WHERE [IdempotencyKey] LIKE N'ui-test:club:%';

DELETE o FROM [ClubRewardOffers] o
JOIN [ClubRewardTemplates] t ON t.[Id] = o.[RewardTemplateId]
WHERE t.[Name] LIKE N'ui-test-club-%';

DELETE g FROM [ClubRewardTargets] g
JOIN [ClubRewardTemplates] t ON t.[Id] = g.[RewardTemplateId]
WHERE t.[Name] LIKE N'ui-test-club-%';

DELETE FROM [ClubRewardTemplates] WHERE [Name] LIKE N'ui-test-club-%';

-- کدهای تخفیف تستی: حذف نرم (مثل بقیه‌ی سیستم)
UPDATE [Rebate] SET [Deleted] = 1, [Active] = 0
WHERE [CodeValue] LIKE N'UITEST-CLUB-%' AND [Deleted] = 0;

IF @Apply = 1
BEGIN
    COMMIT TRAN;
    SELECT 'COMMITTED' AS [Result];
END
ELSE
BEGIN
    ROLLBACK TRAN;
    SELECT 'ROLLED BACK (preview only) - set @Apply = 1 to apply' AS [Result];
END
