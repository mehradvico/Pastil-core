using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShipmentLifecycleColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourierRetryCount",
                table: "Shipments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DisputeReportedAtUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LateNotifiedAtUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippedNotifiedAtUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PostShippedNotifiedAtUtc",
                table: "ProductOrders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourierRetryCount",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DisputeReportedAtUtc",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "LateNotifiedAtUtc",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShippedNotifiedAtUtc",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PostShippedNotifiedAtUtc",
                table: "ProductOrders");
        }
    }
}
