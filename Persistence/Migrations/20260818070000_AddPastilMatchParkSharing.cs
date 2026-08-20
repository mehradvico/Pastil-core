using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPastilMatchParkSharing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ParkId",
                table: "PastilMatchMessages",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchMessages_ParkId",
                table: "PastilMatchMessages",
                column: "ParkId");

            migrationBuilder.AddForeignKey(
                name: "FK_PastilMatchMessages_Parks_ParkId",
                table: "PastilMatchMessages",
                column: "ParkId",
                principalTable: "Parks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PastilMatchMessages_Parks_ParkId",
                table: "PastilMatchMessages");

            migrationBuilder.DropIndex(
                name: "IX_PastilMatchMessages_ParkId",
                table: "PastilMatchMessages");

            migrationBuilder.DropColumn(
                name: "ParkId",
                table: "PastilMatchMessages");
        }
    }
}
