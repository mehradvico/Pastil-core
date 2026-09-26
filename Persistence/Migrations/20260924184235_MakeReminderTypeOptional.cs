using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeReminderTypeOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reminders_ReminderTypes_ReminderTypeId",
                table: "Reminders");

            migrationBuilder.AlterColumn<long>(
                name: "ReminderTypeId",
                table: "Reminders",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_Reminders_ReminderTypes_ReminderTypeId",
                table: "Reminders",
                column: "ReminderTypeId",
                principalTable: "ReminderTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reminders_ReminderTypes_ReminderTypeId",
                table: "Reminders");

            migrationBuilder.AlterColumn<long>(
                name: "ReminderTypeId",
                table: "Reminders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reminders_ReminderTypes_ReminderTypeId",
                table: "Reminders",
                column: "ReminderTypeId",
                principalTable: "ReminderTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
