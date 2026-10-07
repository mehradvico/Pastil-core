using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStorePreparationAndShipmentReady : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxPreparationMinutes",
                table: "Stores",
                type: "int",
                nullable: false,
                defaultValue: 120);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadyAtUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadyDeadlineUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadyReminderSentAtUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxPreparationMinutes",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "ReadyAtUtc",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ReadyDeadlineUtc",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ReadyReminderSentAtUtc",
                table: "Shipments");
        }
    }
}
