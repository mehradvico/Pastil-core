using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SchoolReserveSettlement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SchoolReserveId",
                table: "SettlementCompanions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Permitted",
                table: "SchoolReserves",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_SettlementCompanions_SchoolReserveId",
                table: "SettlementCompanions",
                column: "SchoolReserveId");

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementCompanions_SchoolReserves_SchoolReserveId",
                table: "SettlementCompanions",
                column: "SchoolReserveId",
                principalTable: "SchoolReserves",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SettlementCompanions_SchoolReserves_SchoolReserveId",
                table: "SettlementCompanions");

            migrationBuilder.DropIndex(
                name: "IX_SettlementCompanions_SchoolReserveId",
                table: "SettlementCompanions");

            migrationBuilder.DropColumn(
                name: "SchoolReserveId",
                table: "SettlementCompanions");

            migrationBuilder.DropColumn(
                name: "Permitted",
                table: "SchoolReserves");
        }
    }
}
