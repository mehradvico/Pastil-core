using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExpressShipping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ExpressShippingEnabled",
                table: "Stores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "ExpressSurcharge",
                table: "ShippingQuotes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsExpress",
                table: "ShippingQuotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "BoostAmount",
                table: "Shipments",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BoostLevel",
                table: "Shipments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ExpressSurcharge",
                table: "Shipments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsExpress",
                table: "Shipments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShippingIsExpress",
                table: "ProductOrderStores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShippingIsExpress",
                table: "CartStores",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpressShippingEnabled",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "ExpressSurcharge",
                table: "ShippingQuotes");

            migrationBuilder.DropColumn(
                name: "IsExpress",
                table: "ShippingQuotes");

            migrationBuilder.DropColumn(
                name: "BoostAmount",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "BoostLevel",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ExpressSurcharge",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "IsExpress",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShippingIsExpress",
                table: "ProductOrderStores");

            migrationBuilder.DropColumn(
                name: "ShippingIsExpress",
                table: "CartStores");
        }
    }
}
