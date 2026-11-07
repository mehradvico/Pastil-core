using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CoachBoardingSimplify : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxDays",
                table: "CoachServicePackageTypes");

            migrationBuilder.DropColumn(
                name: "MinDays",
                table: "CoachServicePackageTypes");

            migrationBuilder.DropColumn(
                name: "ServiceBoarding",
                table: "Coaches");

            migrationBuilder.AddColumn<double>(
                name: "DailyPrice",
                table: "CoachServicePackageTypes",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyPrice",
                table: "CoachServicePackageTypes");

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

            migrationBuilder.AddColumn<bool>(
                name: "ServiceBoarding",
                table: "Coaches",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
