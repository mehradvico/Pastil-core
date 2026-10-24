using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AssistanceProviderRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ProviderRoleId",
                table: "Assistances",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assistances_ProviderRoleId",
                table: "Assistances",
                column: "ProviderRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assistances_ProviderRoles_ProviderRoleId",
                table: "Assistances",
                column: "ProviderRoleId",
                principalTable: "ProviderRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assistances_ProviderRoles_ProviderRoleId",
                table: "Assistances");

            migrationBuilder.DropIndex(
                name: "IX_Assistances_ProviderRoleId",
                table: "Assistances");

            migrationBuilder.DropColumn(
                name: "ProviderRoleId",
                table: "Assistances");
        }
    }
}
