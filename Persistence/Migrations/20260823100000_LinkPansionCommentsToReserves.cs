using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LinkPansionCommentsToReserves : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PansionReserveId",
                table: "PansionComments",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PansionComments_PansionReserveId",
                table: "PansionComments",
                column: "PansionReserveId",
                unique: true,
                filter: "[PansionReserveId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_PansionComments_PansionReserves_PansionReserveId",
                table: "PansionComments",
                column: "PansionReserveId",
                principalTable: "PansionReserves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PansionComments_PansionReserves_PansionReserveId",
                table: "PansionComments");

            migrationBuilder.DropIndex(
                name: "IX_PansionComments_PansionReserveId",
                table: "PansionComments");

            migrationBuilder.DropColumn(
                name: "PansionReserveId",
                table: "PansionComments");
        }
    }
}
