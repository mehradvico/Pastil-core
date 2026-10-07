using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedTripOngoingPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // هر دو نوع tag یکسان (trip-ongoing) دارند تا پوش پایان، اعلان ماندگار را جایگزین کند.
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 111 AND Label <> N'PushTripOngoing')
                    THROW 51000, 'PushType ID 111 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushTripOngoing' AND Id <> 111)
                    THROW 51000, 'PushTripOngoing is already assigned to another ID.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 112 AND Label <> N'PushTripOngoingEnd')
                    THROW 51000, 'PushType ID 112 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushTripOngoingEnd' AND Id <> 112)
                    THROW 51000, 'PushTripOngoingEnd is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 111)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (111, N'سفر پت‌رسان در جریان است - اعلان ماندگار (به مسافر)', N'PushTripOngoing');
                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 112)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (112, N'پایان سفر پت‌رسان - بستن اعلان ماندگار (به مسافر)', N'PushTripOngoingEnd');

                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 111)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (111, N'سفر پت‌رسان در جریان است', N'{0} در مسیر مقصد است. زمان تقریبی رسیدن به مقصد {1}', N'/trip', NULL, N'trip-ongoing', 1);
                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 112)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (112, N'سفر پت‌رسان', N'سفر {0} به پایان رسید.', N'/trip', NULL, N'trip-ongoing', 1);

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId IN (111, 112)
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
                WHERE pattern.PushTypeId IN (111, 112);

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId IN (111, 112);

                DELETE FROM PushPatterns WHERE PushTypeId IN (111, 112);
                DELETE FROM PushTypes
                WHERE (Id = 111 AND Label = N'PushTripOngoing')
                   OR (Id = 112 AND Label = N'PushTripOngoingEnd');
                """);
        }
    }
}
