using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedSchoolClassStartingPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 64 AND Label <> N'PushSchoolClassStarting')
                    THROW 51000, 'PushType ID 64 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushSchoolClassStarting' AND Id <> 64)
                    THROW 51000, 'PushSchoolClassStarting is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 64)
                    INSERT INTO PushTypes (Id, Name, Label)
                    VALUES (64, N'شروع کلاس زنده‌ی مدرسه', N'PushSchoolClassStarting');

                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 64)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES
                    (
                        64,
                        N'کلاس شما شروع شد',
                        N'کلاس دوره‌ی {0} همین الان شروع شد. برای ورود ضربه بزنید.',
                        N'/schoolSession/{1}',
                        NULL,
                        N'school-class-starting',
                        1
                    );

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId = 64
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
                WHERE pattern.PushTypeId = 64;

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId = 64;

                DELETE FROM PushPatterns WHERE PushTypeId = 64;
                DELETE FROM PushTypes
                WHERE Id = 64
                  AND Label = N'PushSchoolClassStarting';
                """);
        }
    }
}
