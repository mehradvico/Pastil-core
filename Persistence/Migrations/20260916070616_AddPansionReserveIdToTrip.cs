using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPansionReserveIdToTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PansionReserveId",
                table: "Trips",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_PansionReserveId",
                table: "Trips",
                column: "PansionReserveId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_PansionReserves_PansionReserveId",
                table: "Trips",
                column: "PansionReserveId",
                principalTable: "PansionReserves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trips_PansionReserves_PansionReserveId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_PansionReserveId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "PansionReserveId",
                table: "Trips");
        }
    }
}
