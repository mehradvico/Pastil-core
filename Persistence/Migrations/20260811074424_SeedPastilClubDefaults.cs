using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedPastilClubDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                SET NOCOUNT ON;

                DECLARE @Now datetime2 = SYSUTCDATETIME();

                DECLARE @PointRules TABLE
                (
                    [Name] nvarchar(200) NOT NULL,
                    [EventType] int NOT NULL,
                    [PointAmount] bigint NOT NULL,
                    [DailyLimit] int NULL,
                    [MonthlyLimit] int NULL,
                    [LifetimeLimit] int NULL,
                    [Description] nvarchar(1000) NULL
                );

                INSERT INTO @PointRules
                    ([Name], [EventType], [PointAmount], [DailyLimit], [MonthlyLimit], [LifetimeLimit], [Description])
                VALUES
                    (N'تکمیل سفارش محصول', 1, 50, NULL, NULL, NULL, N'پس از تکمیل موفق سفارش محصول؛ لغو یا بازپرداخت باعث برگشت امتیاز می‌شود.'),
                    (N'تکمیل رزرو نماینده', 2, 100, NULL, NULL, NULL, N'پس از تکمیل موفق رزرو خدمات نماینده؛ لغو یا بازپرداخت باعث برگشت امتیاز می‌شود.'),
                    (N'تکمیل رزرو پانسیون', 3, 100, NULL, NULL, NULL, N'پس از تکمیل موفق رزرو پانسیون؛ لغو یا بازپرداخت باعث برگشت امتیاز می‌شود.'),
                    (N'تکمیل پروفایل پت', 4, 80, NULL, NULL, NULL, N'برای هر پت کامل، فقط یک‌بار با کلید یکتای همان پت محاسبه می‌شود.'),
                    (N'ثبت خاطره روزانه', 5, 10, 1, 31, NULL, N'حداکثر یک خاطره امتیازدار در هر روز به وقت تهران.'),
                    (N'معرفی کاربر - معرف', 6, 100, 10, 100, NULL, N'امتیاز کاربر معرف پس از ثبت و تأیید معرفی معتبر.'),
                    (N'معرفی کاربر - کاربر جدید', 7, 100, NULL, NULL, 1, N'امتیاز خوش‌آمدگویی کاربر جدید برای یک معرفی معتبر؛ فقط یک‌بار در طول عمر.'),
                    (N'معرفی کسب‌وکار - کاربر جدید', 8, 100, NULL, NULL, 1, N'امتیاز کاربر معرفی‌شده توسط نماینده یا فروشگاه؛ فقط یک‌بار در طول عمر.');

                INSERT INTO [ClubPointRules]
                    ([Name], [EventType], [PointAmount], [DailyLimit], [MonthlyLimit], [LifetimeLimit], [Active],
                     [StartDate], [EndDate], [Description], [CreateDate], [UpdateDate])
                SELECT
                    source.[Name], source.[EventType], source.[PointAmount], source.[DailyLimit], source.[MonthlyLimit],
                    source.[LifetimeLimit], CAST(1 AS bit), NULL, NULL, source.[Description], @Now, NULL
                FROM @PointRules source
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM [ClubPointRules] existing
                    WHERE existing.[EventType] = source.[EventType]
                );

                DECLARE @RewardTemplates TABLE
                (
                    [Name] nvarchar(200) NOT NULL,
                    [Title] nvarchar(250) NOT NULL,
                    [ShortDescription] nvarchar(500) NULL,
                    [Description] nvarchar(4000) NULL,
                    [RewardType] int NOT NULL,
                    [ApplicationMethod] int NOT NULL,
                    [PointCost] bigint NOT NULL,
                    [ExpirationType] int NOT NULL,
                    [ExpirationValue] int NULL,
                    [BenefitValue] decimal(18, 2) NULL,
                    [MaximumBenefitValue] decimal(18, 2) NULL,
                    [NotificationLevel] int NOT NULL,
                    [Terms] nvarchar(4000) NULL
                );

                INSERT INTO @RewardTemplates
                    ([Name], [Title], [ShortDescription], [Description], [RewardType], [ApplicationMethod], [PointCost],
                     [ExpirationType], [ExpirationValue], [BenefitValue], [MaximumBenefitValue], [NotificationLevel], [Terms])
                VALUES
                    (N'club-product-order-20-percent', N'۲۰ درصد تخفیف سفارش محصول',
                     N'تخفیف ۲۰ درصدی خرید محصول تا سقف ۱۰۰ هزار تومان',
                     N'قالب عمومی تخفیف سفارش محصول برای ارائه دستی یا خودکار به اعضای پاستیل‌کلاب.',
                     2, 1, 300, 2, 7, 20, 100000, 2,
                     N'قابل استفاده برای یک سفارش محصول، فقط توسط دریافت‌کننده جایزه و تا پایان اعتبار.'),
                    (N'club-product-order-free-delivery', N'ارسال رایگان سفارش محصول',
                     N'ارسال رایگان تا سقف ۱۰۰ هزار تومان',
                     N'قالب عمومی ارسال رایگان برای سفارش محصول.',
                     3, 1, 250, 2, 7, NULL, 100000, 2,
                     N'قابل استفاده برای یک سفارش محصول، فقط توسط دریافت‌کننده جایزه و تا پایان اعتبار.'),
                    (N'club-companion-reservation-fixed-50000', N'۵۰ هزار تومان تخفیف خدمات نماینده',
                     N'تخفیف ثابت برای رزرو خدمات نماینده',
                     N'قالب عمومی تخفیف رزرو خدمات نماینده.',
                     1, 2, 300, 2, 7, 50000, 50000, 1,
                     N'قابل استفاده برای یک رزرو نماینده، فقط توسط دریافت‌کننده جایزه و تا پایان اعتبار.'),
                    (N'club-pansion-reservation-fixed-50000', N'۵۰ هزار تومان تخفیف رزرو پانسیون',
                     N'تخفیف ثابت برای رزرو پانسیون',
                     N'قالب عمومی تخفیف رزرو پانسیون.',
                     1, 3, 300, 2, 7, 50000, 50000, 1,
                     N'قابل استفاده برای یک رزرو پانسیون، فقط توسط دریافت‌کننده جایزه و تا پایان اعتبار.'),
                    (N'club-global-wallet-credit-100000', N'۱۰۰ هزار تومان اعتبار هدیه',
                     N'اعتبار کیف پول تبلیغاتی قابل استفاده در اکوسیستم پاستیل',
                     N'قالب عمومی اعتبار هدیه پاستیل‌کلاب با تأمین مالی پاستیل.',
                     4, 1, 600, 3, 10, 100000, 100000, 3,
                     N'اعتبار قابل برداشت یا انتقال نیست و فقط تا پایان اعتبار در پرداخت‌های مجاز قابل استفاده است.');

                INSERT INTO [ClubRewardTemplates]
                    ([Name], [Title], [ShortDescription], [Description], [RewardType], [ApplicationMethod], [PointCost],
                     [StartDate], [EndDate], [ExpirationType], [ExpirationValue], [FixedExpirationDate], [BenefitValue],
                     [MaximumBenefitValue], [FundingType], [IsAutomationAllowed], [IsManualAllowed], [Active],
                     [NotificationLevel], [PictureId], [Terms], [CreateDate], [UpdateDate])
                SELECT
                    source.[Name], source.[Title], source.[ShortDescription], source.[Description], source.[RewardType],
                    source.[ApplicationMethod], source.[PointCost], NULL, NULL, source.[ExpirationType],
                    source.[ExpirationValue], NULL, source.[BenefitValue], source.[MaximumBenefitValue], 1,
                    CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), source.[NotificationLevel], NULL,
                    source.[Terms], @Now, NULL
                FROM @RewardTemplates source
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM [ClubRewardTemplates] existing
                    WHERE existing.[Name] = source.[Name]
                );

                INSERT INTO [ClubRewardTargets] ([RewardTemplateId], [TargetType], [TargetId], [IncludeChildren])
                SELECT template.[Id], 1, NULL, CAST(1 AS bit)
                FROM [ClubRewardTemplates] template
                INNER JOIN @RewardTemplates source ON source.[Name] = template.[Name]
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM [ClubRewardTargets] target
                    WHERE target.[RewardTemplateId] = template.[Id]
                      AND target.[TargetType] = 1
                      AND target.[TargetId] IS NULL
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Defaults can become editable business data or be referenced by the point ledger
            // and reward history. They are intentionally retained on rollback to avoid data loss.
        }
    }
}
