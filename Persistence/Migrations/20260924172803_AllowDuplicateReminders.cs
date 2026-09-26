using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AllowDuplicateReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reminders_UserPetId_ReminderTypeId_ReminderCycleId_StartDate",
                table: "Reminders");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserPetId_ReminderTypeId_ReminderCycleId_StartDate",
                table: "Reminders",
                columns: new[] { "UserPetId", "ReminderTypeId", "ReminderCycleId", "StartDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reminders_UserPetId_ReminderTypeId_ReminderCycleId_StartDate",
                table: "Reminders");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserPetId_ReminderTypeId_ReminderCycleId_StartDate",
                table: "Reminders",
                columns: new[] { "UserPetId", "ReminderTypeId", "ReminderCycleId", "StartDate" },
                unique: true,
                filter: "[Deleted] = 0");
        }
    }
}
