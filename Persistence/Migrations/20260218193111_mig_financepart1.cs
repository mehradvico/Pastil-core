using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_financepart1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SharePercent",
                table: "Companions");

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionPercent",
                table: "Stores",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DailyCommissionPercent",
                table: "Pansions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyCommissionPercent",
                table: "Pansions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionPercent",
                table: "CompanionAssistances",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommissionPercent",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "DailyCommissionPercent",
                table: "Pansions");

            migrationBuilder.DropColumn(
                name: "HourlyCommissionPercent",
                table: "Pansions");

            migrationBuilder.DropColumn(
                name: "CommissionPercent",
                table: "CompanionAssistances");

            migrationBuilder.AddColumn<int>(
                name: "SharePercent",
                table: "Companions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
