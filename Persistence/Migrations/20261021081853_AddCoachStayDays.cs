using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCoachStayDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SessionCount",
                table: "CoachServicePackageTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StayDays",
                table: "CoachServicePackageTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SessionCount",
                table: "CoachReserves",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StayDays",
                table: "CoachReserves",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionCount",
                table: "CoachServicePackageTypes");

            migrationBuilder.DropColumn(
                name: "StayDays",
                table: "CoachServicePackageTypes");

            migrationBuilder.DropColumn(
                name: "SessionCount",
                table: "CoachReserves");

            migrationBuilder.DropColumn(
                name: "StayDays",
                table: "CoachReserves");
        }
    }
}
