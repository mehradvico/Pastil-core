using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedTripDriverReminderPushType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 81 AND Label <> N'PushTripDriverUpcomingReminder')
                    THROW 51000, 'PushType ID 81 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushTripDriverUpcomingReminder' AND Id <> 81)
                    THROW 51000, 'PushTripDriverUpcomingReminder is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 81)
                    INSERT INTO PushTypes (Id, Name, Label)
                    VALUES (81, N'یادآوری سفر/سرویس رزروشده به راننده', N'PushTripDriverUpcomingReminder');

                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 81)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES
                    (
                        81,
                        N'یادآوری سفر رزروشده',
                        N'{0} شما تا ۳۰ دقیقه‌ی دیگر شروع می‌شود، لطفاً آماده باشید.',
                        N'/driver/trip/{1}',
                        NULL,
                        N'trip-driver-upcoming-reminder',
                        1
                    );

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId = 81
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM PushSettings setting
                      WHERE setting.PushPatternId = pattern.Id
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE notification
                FROM PushNotifications notification
                INNER JOIN PushPatterns pattern ON pattern.Id = notification.PushPatternId
                WHERE pattern.PushTypeId = 81;

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId = 81;

                DELETE FROM PushPatterns WHERE PushTypeId = 81;
                DELETE FROM PushTypes
                WHERE Id = 81
                  AND Label = N'PushTripDriverUpcomingReminder';
                """);
        }
    }
}
