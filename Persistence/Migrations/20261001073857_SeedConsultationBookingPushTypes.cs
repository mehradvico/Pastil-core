using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedConsultationBookingPushTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id IN (83, 84, 85) AND Label NOT IN (N'PushConsultationBookedAgent', N'PushConsultationBookedUser', N'PushConsultationBookingReminder'))
                    THROW 51000, 'PushType IDs 83-85 are already assigned to other labels.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label IN (N'PushConsultationBookedAgent', N'PushConsultationBookedUser', N'PushConsultationBookingReminder') AND Id NOT IN (83, 84, 85))
                    THROW 51000, 'Consultation booking push labels are already assigned to other IDs.', 1;

                SET IDENTITY_INSERT PushTypes ON;

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 83)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (83, N'رزرو مشاوره ساعت‌دار (اعلان به نماینده)', N'PushConsultationBookedAgent');
                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 84)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (84, N'رزرو مشاوره ساعت‌دار (اعلان به کاربر)', N'PushConsultationBookedUser');
                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 85)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (85, N'یادآوری ۳۰ دقیقه مانده به مشاوره‌ی رزروشده', N'PushConsultationBookingReminder');

                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 83)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (83, N'رزرو مشاوره‌ی ساعت‌دار', N'{0} یک مشاوره‌ی {2} برای {3} رزرو کرده است.', N'/consultations/manage', NULL, N'consultation-booked-agent', 1);
                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 84)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (84, N'مشاوره‌ی شما رزرو شد', N'مشاوره‌ی شما در {0} برای {2} ثبت شد؛ در همان ساعت نماینده با شما ارتباط می‌گیرد.', N'/consultations', NULL, N'consultation-booked-user', 1);
                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 85)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (85, N'یادآوری مشاوره', N'مشاوره‌ی {0} تا ۳۰ دقیقه‌ی دیگر شروع می‌شود.', N'{2}', NULL, N'consultation-booking-reminder', 1);

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId IN (83, 84, 85)
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
                WHERE pattern.PushTypeId IN (83, 84, 85);

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId IN (83, 84, 85);

                DELETE FROM PushPatterns WHERE PushTypeId IN (83, 84, 85);
                DELETE FROM PushTypes
                WHERE Id IN (83, 84, 85)
                  AND Label IN (N'PushConsultationBookedAgent', N'PushConsultationBookedUser', N'PushConsultationBookingReminder');
                """);
        }
    }
}
