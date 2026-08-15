using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedPastilClubUiTestData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                SET NOCOUNT ON;

                DECLARE @Now datetime2 = SYSUTCDATETIME();
                DECLARE @NowOffset datetimeoffset = TODATETIMEOFFSET(@Now, '+00:00');
                DECLARE @UserId bigint =
                (
                    SELECT TOP (1) [Id]
                    FROM [Users]
                    WHERE [Deleted] = 0 AND [Locked] = 0
                    ORDER BY CASE WHEN [Mobile] = N'09339750767' THEN 0 ELSE 1 END, [Id]
                );

                IF @UserId IS NULL
                    RETURN;

                -------------------------------------------------------------------------------
                -- Reward templates used only for UI verification.
                -------------------------------------------------------------------------------
                DECLARE @Templates TABLE
                (
                    [Sequence] int NOT NULL,
                    [Name] nvarchar(200) NOT NULL,
                    [Title] nvarchar(250) NOT NULL,
                    [ShortDescription] nvarchar(500) NULL,
                    [RewardType] int NOT NULL,
                    [ApplicationMethod] int NOT NULL,
                    [PointCost] bigint NOT NULL,
                    [BenefitValue] decimal(18, 2) NULL,
                    [MaximumBenefitValue] decimal(18, 2) NULL,
                    [NotificationLevel] int NOT NULL
                );

                INSERT INTO @Templates
                    ([Sequence], [Name], [Title], [ShortDescription], [RewardType], [ApplicationMethod],
                     [PointCost], [BenefitValue], [MaximumBenefitValue], [NotificationLevel])
                VALUES
                    (1, N'ui-test-club-pending-discount', N'تخفیف تستی در انتظار تأیید', N'برای تست کارت پیشنهاد در انتظار تأیید', 1, 1, 100, 25000, 25000, 1),
                    (2, N'ui-test-club-approved-discount', N'تخفیف تستی تأییدشده', N'برای تست جایزه آماده دریافت', 2, 1, 150, 15, 75000, 2),
                    (3, N'ui-test-club-rejected-delivery', N'ارسال رایگان ردشده', N'برای تست نمایش علت رد پیشنهاد', 3, 1, 120, NULL, 50000, 1),
                    (4, N'ui-test-club-redeemed-companion', N'جایزه دریافت‌شده خدمات نماینده', N'تخفیف ثابت رزرو خدمات نماینده', 1, 2, 120, 30000, 30000, 1),
                    (5, N'ui-test-club-failed-pansion', N'جایزه ناموفق پانسیون', N'برای تست وضعیت ناموفق دریافت جایزه', 2, 3, 180, 10, 50000, 2),
                    (6, N'ui-test-club-cancelled-wallet', N'اعتبار هدیه لغوشده', N'برای تست وضعیت لغوشده اعتبار هدیه', 4, 1, 250, 100000, 100000, 3),
                    (7, N'ui-test-club-expired-redemption', N'جایزه دریافت‌شده منقضی', N'برای تست جایزه دریافت‌شده منقضی', 1, 1, 300, 40000, 40000, 2),
                    (8, N'ui-test-club-expired-offer', N'پیشنهاد منقضی‌شده', N'برای تست پیشنهاد منقضی‌شده در لیست', 2, 1, 200, 12, 60000, 1),
                    (9, N'ui-test-club-cancelled-offer', N'پیشنهاد لغوشده پانسیون', N'برای تست پیشنهاد لغوشده توسط مدیر', 1, 3, 220, 50000, 50000, 2);

                INSERT INTO [ClubRewardTemplates]
                    ([Name], [Title], [ShortDescription], [Description], [RewardType], [ApplicationMethod],
                     [PointCost], [StartDate], [EndDate], [ExpirationType], [ExpirationValue], [FixedExpirationDate],
                     [BenefitValue], [MaximumBenefitValue], [FundingType], [IsAutomationAllowed], [IsManualAllowed],
                     [Active], [NotificationLevel], [PictureId], [Terms], [CreateDate], [UpdateDate])
                SELECT
                    source.[Name], source.[Title], source.[ShortDescription],
                    N'این رکورد برای تست رابط کاربری پاستیل‌کلاب ایجاد شده است.',
                    source.[RewardType], source.[ApplicationMethod], source.[PointCost],
                    DATEADD(day, -30, @NowOffset), DATEADD(day, 90, @NowOffset), 4, 30, NULL,
                    source.[BenefitValue], source.[MaximumBenefitValue], 1, CAST(1 AS bit), CAST(1 AS bit),
                    CAST(1 AS bit), source.[NotificationLevel], NULL,
                    N'داده تستی است و فقط برای بررسی ظاهر، فیلترها و فرم‌های پنل استفاده می‌شود.', @Now, NULL
                FROM @Templates source
                WHERE NOT EXISTS
                (
                    SELECT 1 FROM [ClubRewardTemplates] existing WHERE existing.[Name] = source.[Name]
                );

                INSERT INTO [ClubRewardTargets] ([RewardTemplateId], [TargetType], [TargetId], [IncludeChildren])
                SELECT template.[Id], 1, NULL, CAST(1 AS bit)
                FROM [ClubRewardTemplates] template
                INNER JOIN @Templates source ON source.[Name] = template.[Name]
                WHERE NOT EXISTS
                (
                    SELECT 1 FROM [ClubRewardTargets] target
                    WHERE target.[RewardTemplateId] = template.[Id]
                      AND target.[TargetType] = 1
                      AND target.[TargetId] IS NULL
                );

                -------------------------------------------------------------------------------
                -- Point account and a varied point ledger for filters/charts.
                -------------------------------------------------------------------------------
                IF NOT EXISTS (SELECT 1 FROM [ClubPointAccounts] WHERE [UserId] = @UserId)
                BEGIN
                    INSERT INTO [ClubPointAccounts]
                        ([UserId], [AvailablePoint], [DebtPoint], [LifetimeEarnedPoint], [LifetimeSpentPoint],
                         [LifetimeReversedPoint], [CreateDate], [LastUpdateDate])
                    VALUES (@UserId, 0, 0, 0, 0, 0, @Now, @Now);
                END;

                DECLARE @PointAccountId bigint;
                DECLARE @BaseAvailable bigint;
                DECLARE @BaseDebt bigint;

                SELECT
                    @PointAccountId = [Id],
                    @BaseAvailable = [AvailablePoint],
                    @BaseDebt = [DebtPoint]
                FROM [ClubPointAccounts]
                WHERE [UserId] = @UserId;

                IF NOT EXISTS
                (
                    SELECT 1 FROM [ClubPointTransactions]
                    WHERE [IdempotencyKey] = N'ui-test:club:ledger:01'
                )
                BEGIN
                    INSERT INTO [ClubPointTransactions]
                        ([UserId], [PointAccountId], [TransactionType], [Amount], [AvailableBefore], [AvailableAfter],
                         [DebtBefore], [DebtAfter], [SourceType], [SourceId], [PointRuleId], [ReferralId],
                         [RewardRedemptionId], [ParentTransactionId], [Description], [IdempotencyKey], [CreateDate],
                         [CreatedByUserId], [CreatedByAdminId])
                    VALUES
                        (@UserId, @PointAccountId, 1,  2500, @BaseAvailable,        @BaseAvailable + 2500, @BaseDebt, @BaseDebt, 1, 910001, (SELECT TOP 1 [Id] FROM [ClubPointRules] WHERE [EventType] = 1), NULL, NULL, NULL, N'تکمیل سفارش محصول - داده تستی', N'ui-test:club:ledger:01', DATEADD(day, -24, @Now), @UserId, NULL),
                        (@UserId, @PointAccountId, 1,   100, @BaseAvailable + 2500, @BaseAvailable + 2600, @BaseDebt, @BaseDebt, 2, 910002, (SELECT TOP 1 [Id] FROM [ClubPointRules] WHERE [EventType] = 2), NULL, NULL, NULL, N'تکمیل رزرو نماینده - داده تستی', N'ui-test:club:ledger:02', DATEADD(day, -20, @Now), @UserId, NULL),
                        (@UserId, @PointAccountId, 9,   100, @BaseAvailable + 2600, @BaseAvailable + 2700, @BaseDebt, @BaseDebt, 6, 910003, (SELECT TOP 1 [Id] FROM [ClubPointRules] WHERE [EventType] = 6), NULL, NULL, NULL, N'امتیاز معرفی دوست - داده تستی', N'ui-test:club:ledger:03', DATEADD(day, -17, @Now), @UserId, NULL),
                        (@UserId, @PointAccountId, 7,   250, @BaseAvailable + 2700, @BaseAvailable + 2950, @BaseDebt, @BaseDebt, 9, NULL, NULL, NULL, NULL, NULL, N'افزایش دستی امتیاز توسط مدیر - داده تستی', N'ui-test:club:ledger:04', DATEADD(day, -14, @Now), NULL, NULL),
                        (@UserId, @PointAccountId, 2,  -300, @BaseAvailable + 2950, @BaseAvailable + 2650, @BaseDebt, @BaseDebt, 8, 910005, NULL, NULL, NULL, NULL, N'مصرف امتیاز برای جایزه - داده تستی', N'ui-test:club:ledger:05', DATEADD(day, -11, @Now), @UserId, NULL),
                        (@UserId, @PointAccountId, 3,   -50, @BaseAvailable + 2650, @BaseAvailable + 2600, @BaseDebt, @BaseDebt, 1, 910001, NULL, NULL, NULL, NULL, N'برگشت امتیاز سفارش لغوشده - داده تستی', N'ui-test:club:ledger:06', DATEADD(day, -9, @Now), NULL, NULL),
                        (@UserId, @PointAccountId, 4,   100, @BaseAvailable + 2600, @BaseAvailable + 2700, @BaseDebt, @BaseDebt, 8, 910005, NULL, NULL, NULL, NULL, N'بازگشت امتیاز جایزه - داده تستی', N'ui-test:club:ledger:07', DATEADD(day, -7, @Now), @UserId, NULL),
                        (@UserId, @PointAccountId, 8,  -100, @BaseAvailable + 2700, @BaseAvailable + 2600, @BaseDebt, @BaseDebt, 9, NULL, NULL, NULL, NULL, NULL, N'کاهش دستی امتیاز توسط مدیر - داده تستی', N'ui-test:club:ledger:08', DATEADD(day, -5, @Now), NULL, NULL),
                        (@UserId, @PointAccountId, 5,  -100, @BaseAvailable + 2600, @BaseAvailable + 2600, @BaseDebt, @BaseDebt + 100, 10, NULL, NULL, NULL, NULL, NULL, N'ایجاد بدهی امتیازی - داده تستی', N'ui-test:club:ledger:09', DATEADD(day, -3, @Now), NULL, NULL),
                        (@UserId, @PointAccountId, 6,  -100, @BaseAvailable + 2600, @BaseAvailable + 2500, @BaseDebt + 100, @BaseDebt, 10, NULL, NULL, NULL, NULL, NULL, N'تسویه بدهی امتیازی - داده تستی', N'ui-test:club:ledger:10', DATEADD(day, -1, @Now), @UserId, NULL);

                    UPDATE [ClubPointAccounts]
                    SET [AvailablePoint] = [AvailablePoint] + 2500,
                        [LifetimeEarnedPoint] = [LifetimeEarnedPoint] + 2950,
                        [LifetimeSpentPoint] = [LifetimeSpentPoint] + 500,
                        [LifetimeReversedPoint] = [LifetimeReversedPoint] + 50,
                        [LastUpdateDate] = @Now
                    WHERE [Id] = @PointAccountId;
                END;

                -------------------------------------------------------------------------------
                -- Offers: pending, approved, rejected, redeemed, expired and cancelled.
                -------------------------------------------------------------------------------
                INSERT INTO [ClubRewardOffers]
                    ([UserId], [RewardTemplateId], [SourceType], [AutomationRuleId], [Status], [PointCostSnapshot],
                     [GeneratedDate], [ApprovedDate], [RejectedDate], [ApprovedByAdminId], [RejectedByAdminId],
                     [RejectReason], [ExpiresAt], [RedeemedDate], [CreateDate], [UpdateDate])
                SELECT
                    @UserId,
                    template.[Id],
                    CASE WHEN source.[Sequence] IN (2, 5, 8) THEN 2 ELSE 1 END,
                    NULL,
                    CASE
                        WHEN source.[Sequence] = 1 THEN 1
                        WHEN source.[Sequence] = 2 THEN 2
                        WHEN source.[Sequence] = 3 THEN 3
                        WHEN source.[Sequence] BETWEEN 4 AND 7 THEN 4
                        WHEN source.[Sequence] = 8 THEN 2
                        ELSE 6
                    END,
                    template.[PointCost],
                    DATEADD(day, -source.[Sequence], @NowOffset),
                    CASE WHEN source.[Sequence] IN (2, 4, 5, 6, 7, 8) THEN DATEADD(day, -source.[Sequence] + 1, @NowOffset) ELSE NULL END,
                    CASE WHEN source.[Sequence] = 3 THEN DATEADD(day, -2, @NowOffset) ELSE NULL END,
                    NULL,
                    NULL,
                    CASE WHEN source.[Sequence] = 3 THEN N'شرایط دریافت جایزه تکمیل نشده است؛ داده تستی پنل.' ELSE NULL END,
                    CASE WHEN source.[Sequence] IN (7, 8) THEN DATEADD(day, -1, @NowOffset) ELSE DATEADD(day, 30, @NowOffset) END,
                    CASE WHEN source.[Sequence] BETWEEN 4 AND 7 THEN DATEADD(day, -source.[Sequence] + 2, @NowOffset) ELSE NULL END,
                    DATEADD(day, -source.[Sequence], @Now),
                    NULL
                FROM @Templates source
                INNER JOIN [ClubRewardTemplates] template ON template.[Name] = source.[Name]
                WHERE NOT EXISTS
                (
                    SELECT 1 FROM [ClubRewardOffers] existing
                    WHERE existing.[UserId] = @UserId
                      AND existing.[RewardTemplateId] = template.[Id]
                );

                -------------------------------------------------------------------------------
                -- Redemptions with every redemption status and their spend transactions.
                -------------------------------------------------------------------------------
                DECLARE @RewardAvailable bigint =
                (
                    SELECT [AvailablePoint] FROM [ClubPointAccounts] WHERE [Id] = @PointAccountId
                );

                IF @RewardAvailable >= 850
                   AND NOT EXISTS
                   (
                       SELECT 1 FROM [ClubRewardRedemptions]
                       WHERE [IdempotencyKey] = N'ui-test:club:redemption:04'
                   )
                BEGIN
                    DECLARE @RedeemRows TABLE
                    (
                        [Sequence] int NOT NULL,
                        [OfferId] bigint NOT NULL,
                        [TemplateId] bigint NOT NULL,
                        [PointCost] bigint NOT NULL,
                        [SpentBefore] bigint NOT NULL,
                        [RedemptionStatus] int NOT NULL,
                        [BenefitType] int NOT NULL
                    );

                    INSERT INTO @RedeemRows
                        ([Sequence], [OfferId], [TemplateId], [PointCost], [SpentBefore], [RedemptionStatus], [BenefitType])
                    SELECT source.[Sequence], offer.[Id], template.[Id], template.[PointCost],
                        CASE source.[Sequence] WHEN 4 THEN 0 WHEN 5 THEN 120 WHEN 6 THEN 300 ELSE 550 END,
                        CASE source.[Sequence] WHEN 4 THEN 1 WHEN 5 THEN 2 WHEN 6 THEN 3 ELSE 4 END,
                        CASE source.[Sequence] WHEN 4 THEN 1 WHEN 5 THEN 1 WHEN 6 THEN 3 ELSE 1 END
                    FROM @Templates source
                    INNER JOIN [ClubRewardTemplates] template ON template.[Name] = source.[Name]
                    INNER JOIN [ClubRewardOffers] offer
                        ON offer.[UserId] = @UserId AND offer.[RewardTemplateId] = template.[Id]
                    WHERE source.[Sequence] BETWEEN 4 AND 7;

                    INSERT INTO [ClubPointTransactions]
                        ([UserId], [PointAccountId], [TransactionType], [Amount], [AvailableBefore], [AvailableAfter],
                         [DebtBefore], [DebtAfter], [SourceType], [SourceId], [PointRuleId], [ReferralId],
                         [RewardRedemptionId], [ParentTransactionId], [Description], [IdempotencyKey], [CreateDate],
                         [CreatedByUserId], [CreatedByAdminId])
                    SELECT
                        @UserId, @PointAccountId, 2, -row.[PointCost],
                        @RewardAvailable - row.[SpentBefore],
                        @RewardAvailable - row.[SpentBefore] - row.[PointCost],
                        @BaseDebt, @BaseDebt, 8, row.[OfferId], NULL, NULL, NULL, NULL,
                        N'دریافت جایزه پاستیل‌کلاب - داده تستی',
                        N'ui-test:club:redeem-spend:' + RIGHT(N'0' + CAST(row.[Sequence] AS nvarchar(2)), 2),
                        DATEADD(hour, row.[Sequence], DATEADD(day, -2, @Now)), @UserId, NULL
                    FROM @RedeemRows row;

                    INSERT INTO [ClubRewardRedemptions]
                        ([UserId], [RewardOfferId], [RewardTemplateId], [PointTransactionId], [BenefitType],
                         [BenefitReferenceId], [PointSpent], [RedeemedDate], [ExpiresAt], [Status], [IdempotencyKey])
                    SELECT
                        @UserId, row.[OfferId], row.[TemplateId], transactionItem.[Id], row.[BenefitType], NULL,
                        row.[PointCost], DATEADD(hour, row.[Sequence], DATEADD(day, -2, @NowOffset)),
                        CASE WHEN row.[Sequence] = 7 THEN DATEADD(day, -1, @NowOffset) ELSE DATEADD(day, 28, @NowOffset) END,
                        row.[RedemptionStatus],
                        N'ui-test:club:redemption:' + RIGHT(N'0' + CAST(row.[Sequence] AS nvarchar(2)), 2)
                    FROM @RedeemRows row
                    INNER JOIN [ClubPointTransactions] transactionItem
                        ON transactionItem.[IdempotencyKey] = N'ui-test:club:redeem-spend:' + RIGHT(N'0' + CAST(row.[Sequence] AS nvarchar(2)), 2);

                    UPDATE transactionItem
                    SET [RewardRedemptionId] = redemption.[Id]
                    FROM [ClubPointTransactions] transactionItem
                    INNER JOIN [ClubRewardRedemptions] redemption
                        ON redemption.[PointTransactionId] = transactionItem.[Id]
                    WHERE transactionItem.[IdempotencyKey] LIKE N'ui-test:club:redeem-spend:%';

                    UPDATE [ClubPointAccounts]
                    SET [AvailablePoint] = [AvailablePoint] - 850,
                        [LifetimeSpentPoint] = [LifetimeSpentPoint] + 850,
                        [LastUpdateDate] = @Now
                    WHERE [Id] = @PointAccountId;
                END;

                -------------------------------------------------------------------------------
                -- Discount codes shown below Pastil Club in the panel.
                -------------------------------------------------------------------------------
                DECLARE @RebateTypes TABLE
                (
                    [Name] nvarchar(200) NOT NULL,
                    [CodeValue] nvarchar(100) NOT NULL,
                    [TypeLabel] nvarchar(200) NOT NULL,
                    [PriceValue] float NOT NULL,
                    [IsPriceRebate] bit NOT NULL,
                    [MinCartPrice] float NOT NULL,
                    [UseCount] int NOT NULL,
                    [UsedCount] int NOT NULL,
                    [MaxUsePerUser] int NOT NULL
                );

                INSERT INTO @RebateTypes
                    ([Name], [CodeValue], [TypeLabel], [PriceValue], [IsPriceRebate], [MinCartPrice], [UseCount], [UsedCount], [MaxUsePerUser])
                VALUES
                    (N'کد تستی سفارش محصول', N'UITEST-CLUB-CART-20', N'RebateType_Cart', 20, 0, 200000, 100, 12, 1),
                    (N'کد تستی خدمات نماینده', N'UITEST-CLUB-COMPANION', N'RebateType_CompanionReserve', 30000, 1, 100000, 50, 7, 1),
                    (N'کد تستی رزرو پانسیون', N'UITEST-CLUB-PANSION', N'RebateType_PansionReserve', 15, 0, 300000, 75, 21, 2),
                    (N'کد تستی پاستیل AI', N'UITEST-CLUB-AI', N'RebateType_PastilAI', 25, 0, 0, 30, 3, 1);

                INSERT INTO [Rebate]
                    ([UserId], [TypeId], [CodeValue], [PriceValue], [MinCartPrice], [StartDatetime], [EndDatetime],
                     [IsPriceRebate], [Active], [Deleted], [UseCount], [UsedCount], [MaxUsePerUser], [ProductId],
                     [ClubRewardId], [Name])
                SELECT
                    CASE WHEN source.[CodeValue] = N'UITEST-CLUB-COMPANION' THEN @UserId ELSE NULL END,
                    codeItem.[Id], source.[CodeValue], source.[PriceValue], source.[MinCartPrice],
                    DATEADD(day, -10, @Now), DATEADD(day, 30, @Now), source.[IsPriceRebate], CAST(1 AS bit),
                    CAST(0 AS bit), source.[UseCount], source.[UsedCount], source.[MaxUsePerUser], NULL, NULL, source.[Name]
                FROM @RebateTypes source
                CROSS APPLY
                (
                    SELECT TOP (1) [Id]
                    FROM [Codes]
                    WHERE [Label] = source.[TypeLabel]
                ) codeItem
                WHERE NOT EXISTS
                (
                    SELECT 1 FROM [Rebate] existing
                    WHERE existing.[CodeValue] = source.[CodeValue] AND existing.[Deleted] = 0
                );

                -- Connect the completed test redemption to a real test coupon when its code type exists.
                DECLARE @CompletedRedemptionId bigint =
                (
                    SELECT TOP (1) [Id] FROM [ClubRewardRedemptions]
                    WHERE [IdempotencyKey] = N'ui-test:club:redemption:04'
                );
                DECLARE @CompanionRebateId bigint =
                (
                    SELECT TOP (1) [Id] FROM [Rebate]
                    WHERE [CodeValue] = N'UITEST-CLUB-COMPANION' AND [Deleted] = 0
                );

                IF @CompletedRedemptionId IS NOT NULL
                   AND @CompanionRebateId IS NOT NULL
                   AND NOT EXISTS
                   (
                       SELECT 1 FROM [ClubCoupons]
                       WHERE [RewardRedemptionId] = @CompletedRedemptionId
                   )
                BEGIN
                    INSERT INTO [ClubCoupons]
                        ([RewardRedemptionId], [UserId], [RebateId], [Code], [ExpiresAt], [Used], [UsedDate],
                         [OrderId], [ReservationId], [PaymentId], [CreateDate])
                    VALUES
                        (@CompletedRedemptionId, @UserId, @CompanionRebateId, N'UITEST-CLUB-COMPANION',
                         DATEADD(day, 30, @NowOffset), CAST(0 AS bit), NULL, NULL, NULL, NULL, @Now);

                    UPDATE redemption
                    SET [BenefitReferenceId] = coupon.[Id]
                    FROM [ClubRewardRedemptions] redemption
                    INNER JOIN [ClubCoupons] coupon ON coupon.[RewardRedemptionId] = redemption.[Id]
                    WHERE redemption.[Id] = @CompletedRedemptionId;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Test rows may be edited or referenced while the UI is under verification.
            // They are intentionally retained on rollback to avoid deleting user-visible history.
        }
    }
}
