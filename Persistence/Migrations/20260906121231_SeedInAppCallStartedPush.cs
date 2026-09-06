using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedInAppCallStartedPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 63 AND Label <> N'PushInAppCallStarted')
                    THROW 51000, 'PushType ID 63 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushInAppCallStarted' AND Id <> 63)
                    THROW 51000, 'PushInAppCallStarted is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 63)
                    INSERT INTO PushTypes (Id, Name, Label)
                    VALUES (63, N'شروع تماس درون‌برنامه‌ای توسط نماینده', N'PushInAppCallStarted');

                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 63)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES
                    (
                        63,
                        N'تماس درون‌برنامه‌ای',
                        N'{0} با شما تماس می‌گیرد. برای پاسخ ضربه بزنید.',
                        N'/call/{1}',
                        NULL,
                        N'in-app-call-started',
                        1
                    );

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId = 63
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
                WHERE pattern.PushTypeId = 63;

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId = 63;

                DELETE FROM PushPatterns WHERE PushTypeId = 63;
                DELETE FROM PushTypes
                WHERE Id = 63
                  AND Label = N'PushInAppCallStarted';
                """);
        }
    }
}
