using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SecureReminderAndPushNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE ReminderCycles
SET Cycle = 1
WHERE Cycle <= 0;

;WITH DuplicateReminders AS
(
    SELECT Id,
           ROW_NUMBER() OVER
           (
               PARTITION BY UserPetId,
                            ReminderTypeId,
                            ReminderCycleId,
                            CONVERT(date, StartDate)
               ORDER BY Id
           ) AS RowNumber
    FROM Reminders
    WHERE Deleted = 0
)
UPDATE reminder
SET Deleted = 1
FROM Reminders reminder
INNER JOIN DuplicateReminders duplicate ON duplicate.Id = reminder.Id
WHERE duplicate.RowNumber > 1;
");

            migrationBuilder.DropIndex(
                name: "IX_Reminders_UserPetId",
                table: "Reminders");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserPetId_ReminderTypeId_ReminderCycleId_StartDate",
                table: "Reminders",
                columns: new[] { "UserPetId", "ReminderTypeId", "ReminderCycleId", "StartDate" },
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReminderCycle_Cycle",
                table: "ReminderCycles",
                sql: "[Cycle] > 0");

            migrationBuilder.Sql(@"
IF EXISTS
(
    SELECT 1
    FROM PushTypes
    WHERE (Id = 24 AND Label <> N'PushReminderOneWeekBefore')
       OR (Id = 25 AND Label <> N'PushReminderOneDayBefore')
       OR (Id = 26 AND Label <> N'PushReminderOneDayAfter')
)
    THROW 51000, 'PushType IDs 24-26 are already assigned to another label.', 1;

IF EXISTS
(
    SELECT 1
    FROM PushTypes
    WHERE (Label = N'PushReminderOneWeekBefore' AND Id <> 24)
       OR (Label = N'PushReminderOneDayBefore' AND Id <> 25)
       OR (Label = N'PushReminderOneDayAfter' AND Id <> 26)
)
    THROW 51000, 'Reminder PushType labels are already assigned to another ID.', 1;

SET IDENTITY_INSERT PushTypes ON;

IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 24)
    INSERT INTO PushTypes (Id, Name, Label)
    VALUES (24, N'یادآوری یک هفته مانده به موعد', N'PushReminderOneWeekBefore');

IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 25)
    INSERT INTO PushTypes (Id, Name, Label)
    VALUES (25, N'یادآوری یک روز مانده به موعد', N'PushReminderOneDayBefore');

IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 26)
    INSERT INTO PushTypes (Id, Name, Label)
    VALUES (26, N'یادآوری یک روز پس از موعد', N'PushReminderOneDayAfter');

SET IDENTITY_INSERT PushTypes OFF;

IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 24)
    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
    VALUES (24, N'Push_Title', N'PushPetReminder', N'/reminder', NULL, N'pet-reminder-week-before', 1);

IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 25)
    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
    VALUES (25, N'Push_Title', N'PushPetReminder', N'/reminder', NULL, N'pet-reminder-day-before', 1);

IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 26)
    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
    VALUES (26, N'Push_Title', N'PushPetReminder', N'/reminder', NULL, N'pet-reminder-day-after', 1);

INSERT INTO PushSettings (PushPatternId, IsEnabled)
SELECT pattern.Id, 1
FROM PushPatterns pattern
WHERE pattern.PushTypeId IN (24, 25, 26)
  AND NOT EXISTS
  (
      SELECT 1
      FROM PushSettings setting
      WHERE setting.PushPatternId = pattern.Id
  );
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE notification
FROM PushNotifications notification
INNER JOIN PushPatterns pattern ON pattern.Id = notification.PushPatternId
WHERE pattern.PushTypeId IN (24, 25, 26);

DELETE setting
FROM PushSettings setting
INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
WHERE pattern.PushTypeId IN (24, 25, 26);

DELETE FROM PushPatterns WHERE PushTypeId IN (24, 25, 26);
DELETE FROM PushTypes
WHERE (Id = 24 AND Label = N'PushReminderOneWeekBefore')
   OR (Id = 25 AND Label = N'PushReminderOneDayBefore')
   OR (Id = 26 AND Label = N'PushReminderOneDayAfter');
");

            migrationBuilder.DropIndex(
                name: "IX_Reminders_UserPetId_ReminderTypeId_ReminderCycleId_StartDate",
                table: "Reminders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReminderCycle_Cycle",
                table: "ReminderCycles");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserPetId",
                table: "Reminders",
                column: "UserPetId");
        }
    }
}
