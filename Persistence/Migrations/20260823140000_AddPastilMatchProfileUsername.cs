using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPastilMatchProfileUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "PastilMatchProfiles",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfiles_Username",
                table: "PastilMatchProfiles",
                column: "Username",
                unique: true,
                filter: "[Username] IS NOT NULL AND [Deleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PastilMatchProfiles_Username",
                table: "PastilMatchProfiles");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "PastilMatchProfiles");
        }
    }
}
