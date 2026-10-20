using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShipmentCourierTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CourierLatitude",
                table: "Shipments",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CourierLocationAtUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CourierLongitude",
                table: "Shipments",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CourierName",
                table: "Shipments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CourierPhone",
                table: "Shipments",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMiareEventAtUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastMiareState",
                table: "Shipments",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShipmentEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShipmentId = table.Column<long>(type: "bigint", nullable: false),
                    State = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    ReceivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CourierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    PayloadHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", maxLength: 6000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipmentEvents_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentEvents_ShipmentId_ReceivedAtUtc",
                table: "ShipmentEvents",
                columns: new[] { "ShipmentId", "ReceivedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShipmentEvents");

            migrationBuilder.DropColumn(
                name: "CourierLatitude",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "CourierLocationAtUtc",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "CourierLongitude",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "CourierName",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "CourierPhone",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "LastMiareEventAtUtc",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "LastMiareState",
                table: "Shipments");
        }
    }
}
