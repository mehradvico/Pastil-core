using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedTripDriverSelectionRequiredNotice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NoticeTypeLabels.TripDriverSelectionRequired ("Trip.DriverSelectionRequired") تا الان هیچ‌وقت
            // seed نشده بود — یعنی هر _noticeService.CreateAsync با این Label (رد شدن سفر توسط راننده و نیاز
            // به راننده‌ی دیگر، سفر رزروی/سرویس هفتگی بدون راننده) از قبل هم در NoticeService.CreateAsync به
            // خاموشی رد می‌شد (noticeType == null) — بدون خطا، بدون اعلان، بدون پوش به ادمین. این مایگریشن آن
            // را seed می‌کند تا این مسیرها واقعاً کار کنند. Importance=3 (Critical) یعنی هم realtime پنل، هم
            // پوش واقعی به ادمین‌ها (طبق NoticeEventService.DispatchAsync).
            migrationBuilder.Sql(
                """
                SET NOCOUNT ON;

                DECLARE @NoticeTypes TABLE
                (
                    Label nvarchar(150) NOT NULL,
                    Title nvarchar(200) NOT NULL,
                    Name nvarchar(1000) NOT NULL,
                    NavigationTemplate nvarchar(500) NOT NULL
                );

                INSERT INTO @NoticeTypes (Label, Title, Name, NavigationTemplate)
                VALUES
                    (N'Trip.DriverSelectionRequired', N'نیاز به انتخاب دستی راننده', N'یک سفر پت‌رسان (رزروی یا سرویس هفتگی) نیاز به تخصیص دستی راننده دارد — یا راننده‌ی قبلی آن را رد کرده، یا به موعد حرکتش نزدیک/رسیده و هنوز راننده‌ای قبول نکرده. از پنل «انتخاب راننده» اقدام کنید.', N'/admin/trip');

                UPDATE target
                SET target.Title = source.Title,
                    target.Name = source.Name,
                    target.NavigationTemplate = source.NavigationTemplate,
                    target.Importance = 3,
                    target.IsActive = 1
                FROM NoticeTypes target
                INNER JOIN @NoticeTypes source ON source.Label = target.Label;

                INSERT INTO NoticeTypes (Label, Title, Name, NavigationTemplate, Importance, IsActive)
                SELECT source.Label, source.Title, source.Name, source.NavigationTemplate, 3, 1
                FROM @NoticeTypes source
                WHERE NOT EXISTS
                (
                    SELECT 1 FROM NoticeTypes target WHERE target.Label = source.Label
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM NoticeTypes WHERE Label = N'Trip.DriverSelectionRequired';
                """);
        }
    }
}
