using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingSlotsAndSellerConfirm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryCode",
                table: "Shipments",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PickupDeadlineUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SellerConfirmDeadlineUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SellerConfirmedAtUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SellerReminderSentAtUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SlotEndUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SlotStartUtc",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippingSlotDate",
                table: "ProductOrderStores",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ShippingSlotId",
                table: "ProductOrderStores",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippingSlotDate",
                table: "CartStores",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ShippingSlotId",
                table: "CartStores",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShippingSlots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingSlots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_Status_SellerConfirmDeadlineUtc",
                table: "Shipments",
                columns: new[] { "Status", "SellerConfirmDeadlineUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrderStores_ShippingSlotId_ShippingSlotDate",
                table: "ProductOrderStores",
                columns: new[] { "ShippingSlotId", "ShippingSlotDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShippingSlots_DayOfWeek_StartTime_EndTime",
                table: "ShippingSlots",
                columns: new[] { "DayOfWeek", "StartTime", "EndTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShippingSlots");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_Status_SellerConfirmDeadlineUtc",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_ProductOrderStores_ShippingSlotId_ShippingSlotDate",
                table: "ProductOrderStores");

            migrationBuilder.DropColumn(
                name: "DeliveryCode",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PickupDeadlineUtc",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "SellerConfirmDeadlineUtc",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "SellerConfirmedAtUtc",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "SellerReminderSentAtUtc",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "SlotEndUtc",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "SlotStartUtc",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShippingSlotDate",
                table: "ProductOrderStores");

            migrationBuilder.DropColumn(
                name: "ShippingSlotId",
                table: "ProductOrderStores");

            migrationBuilder.DropColumn(
                name: "ShippingSlotDate",
                table: "CartStores");

            migrationBuilder.DropColumn(
                name: "ShippingSlotId",
                table: "CartStores");
        }
    }
}
