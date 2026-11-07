using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CoachBoarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxDays",
                table: "CoachServicePackageTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinDays",
                table: "CoachServicePackageTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DailyPrice",
                table: "CoachReserves",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BoardingCapacity",
                table: "Coaches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ServiceBoarding",
                table: "Coaches",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxDays",
                table: "CoachServicePackageTypes");

            migrationBuilder.DropColumn(
                name: "MinDays",
                table: "CoachServicePackageTypes");

            migrationBuilder.DropColumn(
                name: "DailyPrice",
                table: "CoachReserves");

            migrationBuilder.DropColumn(
                name: "BoardingCapacity",
                table: "Coaches");

            migrationBuilder.DropColumn(
                name: "ServiceBoarding",
                table: "Coaches");
        }
    }
}
