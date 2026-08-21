using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessPublicCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "BusinessCodeSequence");

            migrationBuilder.AddColumn<string>(
                name: "OrderCode",
                table: "ProductOrders",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReserveCode",
                table: "PansionReserves",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReserveCode",
                table: "CompanionReserves",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrders_OrderCode",
                table: "ProductOrders",
                column: "OrderCode",
                unique: true,
                filter: "[OrderCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PansionReserves_ReserveCode",
                table: "PansionReserves",
                column: "ReserveCode",
                unique: true,
                filter: "[ReserveCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserves_ReserveCode",
                table: "CompanionReserves",
                column: "ReserveCode",
                unique: true,
                filter: "[ReserveCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductOrders_OrderCode",
                table: "ProductOrders");

            migrationBuilder.DropIndex(
                name: "IX_PansionReserves_ReserveCode",
                table: "PansionReserves");

            migrationBuilder.DropIndex(
                name: "IX_CompanionReserves_ReserveCode",
                table: "CompanionReserves");

            migrationBuilder.DropColumn(
                name: "OrderCode",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "ReserveCode",
                table: "PansionReserves");

            migrationBuilder.DropColumn(
                name: "ReserveCode",
                table: "CompanionReserves");

            migrationBuilder.DropSequence(
                name: "BusinessCodeSequence");
        }
    }
}
