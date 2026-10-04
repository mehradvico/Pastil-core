using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedCompanionDebtPushTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id IN (86, 87) AND Label NOT IN (N'PushCompanionDebtReminder', N'PushCompanionDebtCollected'))
                    THROW 51000, 'PushType IDs 86-87 are already assigned to other labels.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label IN (N'PushCompanionDebtReminder', N'PushCompanionDebtCollected') AND Id NOT IN (86, 87))
                    THROW 51000, 'Companion debt push labels are already assigned to other IDs.', 1;

                SET IDENTITY_INSERT PushTypes ON;

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 86)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (86, N'یادآوری بدهی خدمت به کلینیک (روزی ۳ بار)', N'PushCompanionDebtReminder');
                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 87)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (87, N'کسر خودکار بدهی خدمت از کیف پول', N'PushCompanionDebtCollected');

                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 86)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (86, N'بدهی پرداخت‌نشده', N'شما {0} تومان بابت خدمات دریافت‌شده بدهکار هستید و موجودی کیف پولتان برای پرداخت کافی نیست. لطفاً کیف پول را شارژ و بدهی را پرداخت کنید.', N'/reserve', NULL, N'companion-debt-reminder', 1);
                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 87)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (87, N'بدهی شما پرداخت شد', N'{0} تومان بابت خدمات «{2}» از کیف پول شما کسر شد و بدهی‌تان پرداخت شد.', N'/wallet', NULL, N'companion-debt-collected', 1);

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId IN (86, 87)
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
                WHERE pattern.PushTypeId IN (86, 87);

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId IN (86, 87);

                DELETE FROM PushPatterns WHERE PushTypeId IN (86, 87);
                DELETE FROM PushTypes
                WHERE Id IN (86, 87)
                  AND Label IN (N'PushCompanionDebtReminder', N'PushCompanionDebtCollected');
                """);
        }
    }
}
