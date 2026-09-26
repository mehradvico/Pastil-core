using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTripExtraPetAndVehiclePrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ExtraPetPrice",
                table: "PriceCalculations",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PickupVehicleExtraPrice",
                table: "PriceCalculations",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraPetPrice",
                table: "PriceCalculations");

            migrationBuilder.DropColumn(
                name: "PickupVehicleExtraPrice",
                table: "PriceCalculations");
        }
    }
}
