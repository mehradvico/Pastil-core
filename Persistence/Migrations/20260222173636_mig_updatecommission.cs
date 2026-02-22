using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_updatecommission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrePaymentPrice",
                table: "CompanionAssistances");

            migrationBuilder.AddColumn<double>(
                name: "SiteShare",
                table: "ProductOrders",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StoreShare",
                table: "ProductOrders",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SiteShare",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "StoreShare",
                table: "ProductOrders");

            migrationBuilder.AddColumn<double>(
                name: "PrePaymentPrice",
                table: "CompanionAssistances",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
