using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedCompanionReserveReschedulePush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @Types TABLE (Id bigint, Name nvarchar(200), Label nvarchar(200), Title nvarchar(200), Body nvarchar(500), Url nvarchar(200), Tag nvarchar(200));
                INSERT INTO @Types (Id, Name, Label, Title, Body, Url, Tag) VALUES
                    (119, N'تغییر زمان رزرو کلینیک/مربی/آرایشگاه (به مشتری)', N'PushCompanionReserveRescheduledUser', N'زمان رزرو شما تغییر کرد', N'زمان رزرو شما در {0} به {1} ساعت {2} تغییر کرد. علت: {3}', N'/reserve', N'companion-reserve-rescheduled'),
                    (120, N'تغییر زمان رزرو متصل به سفر پت‌رسان (به راننده)', N'PushCompanionReserveRescheduledDriver', N'زمان سفر پت‌رسان تغییر کرد', N'زمان سفر پت‌رسان رزرو {2} تغییر کرد. زمان جدید حرکت: {0} ساعت {1}', N'/driverProfile', N'companion-reserve-rescheduled-driver');

                IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Id = s.Id WHERE t.Label <> s.Label)
                    THROW 51000, 'A reserve reschedule push type ID is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Label = s.Label WHERE t.Id <> s.Id)
                    THROW 51000, 'A reserve reschedule push label is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;
                INSERT INTO PushTypes (Id, Name, Label)
                SELECT s.Id, s.Name, s.Label FROM @Types s
                WHERE NOT EXISTS (SELECT 1 FROM PushTypes t WHERE t.Id = s.Id);
                SET IDENTITY_INSERT PushTypes OFF;

                INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                SELECT s.Id, s.Title, s.Body, s.Url, NULL, s.Tag, 1 FROM @Types s
                WHERE NOT EXISTS (SELECT 1 FROM PushPatterns p WHERE p.PushTypeId = s.Id);

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId IN (119, 120)
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
                WHERE pattern.PushTypeId IN (119, 120);

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId IN (119, 120);

                DELETE FROM PushPatterns WHERE PushTypeId IN (119, 120);
                DELETE FROM PushTypes
                WHERE (Id = 119 AND Label = N'PushCompanionReserveRescheduledUser')
                   OR (Id = 120 AND Label = N'PushCompanionReserveRescheduledDriver');
                """);
        }
    }
}
