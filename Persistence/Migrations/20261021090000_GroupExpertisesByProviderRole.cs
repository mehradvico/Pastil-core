using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    [DbContext(typeof(DataBaseContext))]
    [Migration("20261021090000_GroupExpertisesByProviderRole")]
    public partial class GroupExpertisesByProviderRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ProviderRoleId",
                table: "Expertises",
                type: "bigint",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE e SET ProviderRoleId = r.Id
FROM dbo.Expertises e
CROSS JOIN dbo.ProviderRoles r
WHERE r.Label = N'Clinic' AND r.Deleted = 0
  AND e.ProviderRoleId IS NULL
  AND e.Name IN (N'دامپزشک عمومی', N'جراح دندان پزشکی', N'جراح عقیم سازی', N'فیزیوتراپی');");

            migrationBuilder.CreateIndex(
                name: "IX_Expertises_ProviderRoleId",
                table: "Expertises",
                column: "ProviderRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expertises_ProviderRoles_ProviderRoleId",
                table: "Expertises",
                column: "ProviderRoleId",
                principalTable: "ProviderRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expertises_ProviderRoles_ProviderRoleId",
                table: "Expertises");
            migrationBuilder.DropIndex(
                name: "IX_Expertises_ProviderRoleId",
                table: "Expertises");
            migrationBuilder.DropColumn(
                name: "ProviderRoleId",
                table: "Expertises");
        }
    }
}
