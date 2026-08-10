using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPastilClubPointIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ClubPointTransactions_UserId_PointRuleId_CreateDate",
                table: "ClubPointTransactions",
                columns: new[] { "UserId", "PointRuleId", "CreateDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClubPointTransactions_UserId_PointRuleId_CreateDate",
                table: "ClubPointTransactions");
        }
    }
}
