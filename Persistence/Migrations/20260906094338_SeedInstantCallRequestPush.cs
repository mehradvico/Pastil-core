using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedInstantCallRequestPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 62 AND Label <> N'PushInstantCallRequestCompanion')
                    THROW 51000, 'PushType ID 62 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushInstantCallRequestCompanion' AND Id <> 62)
                    THROW 51000, 'PushInstantCallRequestCompanion is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 62)
                    INSERT INTO PushTypes (Id, Name, Label)
                    VALUES (62, N'درخواست تماس فوری برای نماینده', N'PushInstantCallRequestCompanion');

                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 62)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES
                    (
                        62,
                        N'درخواست تماس فوری',
                        N'کاربر {0} برای «{1}» درخواست تماس فوری داد. برای مشاهده و تماس ضربه بزنید.',
                        N'/operator?instantCallRequestId={2}',
                        NULL,
                        N'instant-call-request-companion',
                        1
                    );

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId = 62
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
                WHERE pattern.PushTypeId = 62;

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId = 62;

                DELETE FROM PushPatterns WHERE PushTypeId = 62;
                DELETE FROM PushTypes
                WHERE Id = 62
                  AND Label = N'PushInstantCallRequestCompanion';
                """);
        }
    }
}
