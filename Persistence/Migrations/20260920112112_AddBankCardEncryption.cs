using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBankCardEncryption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardNumberHash",
                table: "UserBankCards",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBankCards_CardNumberHash",
                table: "UserBankCards",
                column: "CardNumberHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserBankCards_CardNumberHash",
                table: "UserBankCards");

            migrationBuilder.DropColumn(
                name: "CardNumberHash",
                table: "UserBankCards");
        }
    }
}
