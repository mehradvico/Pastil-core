using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedSchoolClassReminderPushType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 82 AND Label <> N'PushSchoolClassReminder5Min')
                    THROW 51000, 'PushType ID 82 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushSchoolClassReminder5Min' AND Id <> 82)
                    THROW 51000, 'PushSchoolClassReminder5Min is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 82)
                    INSERT INTO PushTypes (Id, Name, Label)
                    VALUES (82, N'یادآوری ۵ دقیقه مانده به شروع کلاس زنده‌ی مدرسه', N'PushSchoolClassReminder5Min');

                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 82)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES
                    (
                        82,
                        N'کلاس شما به‌زودی شروع می‌شود',
                        N'کلاس دوره‌ی {0} تا ۵ دقیقه‌ی دیگر شروع می‌شود.',
                        N'/schoolSession/{1}',
                        NULL,
                        N'school-class-reminder-5min',
                        1
                    );

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId = 82
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
                WHERE pattern.PushTypeId = 82;

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId = 82;

                DELETE FROM PushPatterns WHERE PushTypeId = 82;
                DELETE FROM PushTypes
                WHERE Id = 82
                  AND Label = N'PushSchoolClassReminder5Min';
                """);
        }
    }
}
