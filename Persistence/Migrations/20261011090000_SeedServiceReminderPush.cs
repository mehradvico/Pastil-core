using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedServiceReminderPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @Types TABLE (Id bigint, Name nvarchar(200), Label nvarchar(200), Title nvarchar(200), Body nvarchar(500), Url nvarchar(200), Tag nvarchar(200));
                INSERT INTO @Types (Id, Name, Label, Title, Body, Url, Tag) VALUES
                    (124, N'یادآوری یک ساعت قبل از سرویس هفتگی پت‌رسان (به راننده)', N'PushTripServiceReminderDriver', N'سرویس پت‌رسان یک ساعت دیگر', N'سرویس پت‌رسان شما ساعت {0} شروع می‌شود؛ لطفاً سر وقت آماده باشید.', N'/driverProfile/suggestedTrips', N'trip-service-reminder'),
                    (125, N'یادآوری یک ساعت قبل از سرویس هفتگی پت‌رسان (به کاربر)', N'PushTripServiceReminderUser', N'سرویس پت‌رسان یک ساعت دیگر', N'سرویس پت‌رسان شما ساعت {0} شروع می‌شود؛ لطفاً پت را آماده نگه دارید.', N'/transportation', N'trip-service-reminder');

                IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Id = s.Id WHERE t.Label <> s.Label)
                    THROW 51000, 'PushType IDs 124/125 are already assigned to other labels.', 1;
                IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Label = s.Label WHERE t.Id <> s.Id)
                    THROW 51000, 'Service reminder labels are already assigned to other IDs.', 1;

                SET IDENTITY_INSERT PushTypes ON;
                INSERT INTO PushTypes (Id, Name, Label)
                SELECT s.Id, s.Name, s.Label FROM @Types s
                WHERE NOT EXISTS (SELECT 1 FROM PushTypes t WHERE t.Id = s.Id);
                SET IDENTITY_INSERT PushTypes OFF;

                INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                SELECT s.Id, s.Title, s.Body, s.Url, NULL, s.Tag, 1 FROM @Types s
                WHERE NOT EXISTS (SELECT 1 FROM PushPatterns p WHERE p.PushTypeId = s.Id);

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId IN (124, 125)
                  AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE notification
                FROM PushNotifications notification
                INNER JOIN PushPatterns pattern ON pattern.Id = notification.PushPatternId
                WHERE pattern.PushTypeId IN (124, 125);

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId IN (124, 125);

                DELETE FROM PushPatterns WHERE PushTypeId IN (124, 125);
                DELETE FROM PushTypes
                WHERE (Id = 124 AND Label = N'PushTripServiceReminderDriver')
                   OR (Id = 125 AND Label = N'PushTripServiceReminderUser');
                """);
        }
    }
}
