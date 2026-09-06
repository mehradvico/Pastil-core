using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanionReserveOnlineSelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CompanionAssistancePackageOnlineSelectionId",
                table: "CompanionReserves",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserves_CompanionAssistancePackageOnlineSelectionId",
                table: "CompanionReserves",
                column: "CompanionAssistancePackageOnlineSelectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanionReserves_CompanionAssistancePackageOnlineSelections_CompanionAssistancePackageOnlineSelectionId",
                table: "CompanionReserves",
                column: "CompanionAssistancePackageOnlineSelectionId",
                principalTable: "CompanionAssistancePackageOnlineSelections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanionReserves_CompanionAssistancePackageOnlineSelections_CompanionAssistancePackageOnlineSelectionId",
                table: "CompanionReserves");

            migrationBuilder.DropIndex(
                name: "IX_CompanionReserves_CompanionAssistancePackageOnlineSelectionId",
                table: "CompanionReserves");

            migrationBuilder.DropColumn(
                name: "CompanionAssistancePackageOnlineSelectionId",
                table: "CompanionReserves");
        }
    }
}
