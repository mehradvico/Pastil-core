using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolReserveIdToWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SchoolReserveId",
                table: "Wallets",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_SchoolReserveId",
                table: "Wallets",
                column: "SchoolReserveId");

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_SchoolReserves_SchoolReserveId",
                table: "Wallets",
                column: "SchoolReserveId",
                principalTable: "SchoolReserves",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_SchoolReserves_SchoolReserveId",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_SchoolReserveId",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "SchoolReserveId",
                table: "Wallets");
        }
    }
}
