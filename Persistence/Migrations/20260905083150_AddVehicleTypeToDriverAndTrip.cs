using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleTypeToDriverAndTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "VehicleTypeId",
                table: "Trips",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "VehicleTypeId",
                table: "Drivers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_VehicleTypeId",
                table: "Trips",
                column: "VehicleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_VehicleTypeId",
                table: "Drivers",
                column: "VehicleTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Codes_VehicleTypeId",
                table: "Drivers",
                column: "VehicleTypeId",
                principalTable: "Codes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Codes_VehicleTypeId",
                table: "Trips",
                column: "VehicleTypeId",
                principalTable: "Codes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Codes_VehicleTypeId",
                table: "Drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Codes_VehicleTypeId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_VehicleTypeId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_VehicleTypeId",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "VehicleTypeId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "VehicleTypeId",
                table: "Drivers");
        }
    }
}
