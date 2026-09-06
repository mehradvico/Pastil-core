using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedCompanionOnlineReservePushes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 59 AND Label <> N'PushOnlineReserveConfirmedUser')
                    THROW 51000, 'PushType ID 59 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushOnlineReserveConfirmedUser' AND Id <> 59)
                    THROW 51000, 'PushOnlineReserveConfirmedUser is already assigned to another ID.', 1;

                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 60 AND Label <> N'PushOnlineReserveReminderBeforeCompanion')
                    THROW 51000, 'PushType ID 60 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushOnlineReserveReminderBeforeCompanion' AND Id <> 60)
                    THROW 51000, 'PushOnlineReserveReminderBeforeCompanion is already assigned to another ID.', 1;

                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 61 AND Label <> N'PushOnlineReserveReminderAtTimeCompanion')
                    THROW 51000, 'PushType ID 61 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushOnlineReserveReminderAtTimeCompanion' AND Id <> 61)
                    THROW 51000, 'PushOnlineReserveReminderAtTimeCompanion is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 59)
                    INSERT INTO PushTypes (Id, Name, Label)
                    VALUES (59, N'تأیید رزرو خدمت آنلاین برای کاربر', N'PushOnlineReserveConfirmedUser');

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 60)
                    INSERT INTO PushTypes (Id, Name, Label)
                    VALUES (60, N'یادآوری ۱۰ دقیقه مانده به خدمت آنلاین برای نماینده', N'PushOnlineReserveReminderBeforeCompanion');

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 61)
                    INSERT INTO PushTypes (Id, Name, Label)
                    VALUES (61, N'یادآوری سر زمان خدمت آنلاین برای نماینده', N'PushOnlineReserveReminderAtTimeCompanion');

                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 59)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES
                    (
                        59,
                        N'رزرو شما ثبت شد',
                        N'رزرو شما برای «{0}» با روش ارتباطی «{1}» در ساعت {2} ثبت شد. سر ساعت مقرر آماده باشید.',
                        NULL,
                        NULL,
                        N'online-reserve-confirmed-user',
                        1
                    );

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 60)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES
                    (
                        60,
                        N'۱۰ دقیقه تا خدمت آنلاین',
                        N'۱۰ دقیقه دیگر باید با {0} درباره‌ی «{1}» از طریق «{2}» ارتباط برقرار کنید.',
                        N'/operator',
                        NULL,
                        N'online-reserve-reminder-before-companion',
                        1
                    );

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 61)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES
                    (
                        61,
                        N'زمان خدمت آنلاین رسید',
                        N'همین الان باید با {0} درباره‌ی «{1}» از طریق «{2}» ارتباط برقرار کنید.',
                        N'/operator',
                        NULL,
                        N'online-reserve-reminder-attime-companion',
                        1
                    );

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId IN (59, 60, 61)
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
                WHERE pattern.PushTypeId IN (59, 60, 61);

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId IN (59, 60, 61);

                DELETE FROM PushPatterns WHERE PushTypeId IN (59, 60, 61);
                DELETE FROM PushTypes
                WHERE Id IN (59, 60, 61)
                  AND Label IN (N'PushOnlineReserveConfirmedUser', N'PushOnlineReserveReminderBeforeCompanion', N'PushOnlineReserveReminderAtTimeCompanion');
                """);
        }
    }
}
