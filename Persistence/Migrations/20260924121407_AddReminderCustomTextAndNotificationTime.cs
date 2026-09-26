using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderCustomTextAndNotificationTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomText",
                table: "Reminders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "NotificationTime",
                table: "Reminders",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(9, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomText",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "NotificationTime",
                table: "Reminders");
        }
    }
}
