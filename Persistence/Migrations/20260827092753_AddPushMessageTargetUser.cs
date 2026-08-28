using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPushMessageTargetUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "PushMessages",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PushMessages_UserId",
                table: "PushMessages",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PushMessages_Users_UserId",
                table: "PushMessages",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PushMessages_Users_UserId",
                table: "PushMessages");

            migrationBuilder.DropIndex(
                name: "IX_PushMessages_UserId",
                table: "PushMessages");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PushMessages");
        }
    }
}
