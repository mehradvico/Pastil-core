using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedCompanionReserveAssignedPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS
                (
                    SELECT 1
                    FROM PushTypes
                    WHERE Id = 45 AND Label <> N'PushCompanionReserveAssigned'
                )
                    THROW 51000, 'PushType ID 45 is already assigned to another label.', 1;

                IF EXISTS
                (
                    SELECT 1
                    FROM PushTypes
                    WHERE Label = N'PushCompanionReserveAssigned' AND Id <> 45
                )
                    THROW 51000, 'PushCompanionReserveAssigned is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 45)
                    INSERT INTO PushTypes (Id, Name, Label)
                    VALUES (45, N'تخصیص رزرو خدمت به کاربر نمایندگی', N'PushCompanionReserveAssigned');

                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 45)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES
                    (
                        45,
                        N'رزرو جدید برای شما',
                        N'خدمت {0} برای کاربر {1} به شما اختصاص داده شد.',
                        N'/operator',
                        NULL,
                        N'companion-reserve-assigned',
                        1
                    );

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId = 45
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
                WHERE pattern.PushTypeId = 45;

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId = 45;

                DELETE FROM PushPatterns WHERE PushTypeId = 45;
                DELETE FROM PushTypes
                WHERE Id = 45 AND Label = N'PushCompanionReserveAssigned';
                """);

        }
    }
}
