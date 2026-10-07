using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedCompanionReservePackagePushTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 91 AND Label <> N'PushCompanionReservePackageApproved')
                    THROW 51000, 'PushType ID 91 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 92 AND Label <> N'PushCompanionReservePackageCancelled')
                    THROW 51000, 'PushType ID 92 is already assigned to another label.', 1;

                SET IDENTITY_INSERT PushTypes ON;
                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 91)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (91, N'تأیید پکیج رزرو توسط نماینده (به کاربر)', N'PushCompanionReservePackageApproved');
                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 92)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (92, N'لغو پکیج رزرو و بازگشت مبلغ به کیف پول (به کاربر)', N'PushCompanionReservePackageCancelled');
                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 91)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (91, N'پکیج رزرو تأیید شد', N'نماینده پکیج «{0}» رزرو شما را تأیید کرد.', N'/reserve/{2}', NULL, N'companion-reserve-package-approved', 1);
                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 92)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (92, N'پکیج رزرو لغو شد', N'پکیج «{0}» رزرو شما لغو شد و {3} تومان به کیف پول شما برگشت داده شد.', N'/reserve/{2}', NULL, N'companion-reserve-package-cancelled', 1);

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1 FROM PushPatterns pattern
                WHERE pattern.PushTypeId IN (91, 92)
                  AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE n FROM PushNotifications n INNER JOIN PushPatterns p ON p.Id = n.PushPatternId WHERE p.PushTypeId IN (91, 92);
                DELETE s FROM PushSettings s INNER JOIN PushPatterns p ON p.Id = s.PushPatternId WHERE p.PushTypeId IN (91, 92);
                DELETE FROM PushPatterns WHERE PushTypeId IN (91, 92);
                DELETE FROM PushTypes WHERE Id = 91 AND Label = N'PushCompanionReservePackageApproved';
                DELETE FROM PushTypes WHERE Id = 92 AND Label = N'PushCompanionReservePackageCancelled';
                """);

        }
    }
}
