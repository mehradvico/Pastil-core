using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProviderRoleTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProviderUserRoles_ProviderId_Name",
                table: "ProviderUserRoles");

            migrationBuilder.AlterColumn<long>(
                name: "ProviderId",
                table: "ProviderUserRoles",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "ProviderUserRoles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "ProviderUserRoles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderUserRoles_ProviderId_Name",
                table: "ProviderUserRoles",
                columns: new[] { "ProviderId", "Name" },
                unique: true,
                filter: "[Deleted] = 0 AND [ProviderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderUserRoles_Template_Scope_Name",
                table: "ProviderUserRoles",
                columns: new[] { "Scope", "Name" },
                unique: true,
                filter: "[Deleted] = 0 AND [ProviderId] IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProviderUserRoles_TemplateScope",
                table: "ProviderUserRoles",
                sql: "([ProviderId] IS NULL AND [Scope] IS NOT NULL) OR ([ProviderId] IS NOT NULL AND [Scope] IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProviderUserRoles_ProviderId_Name",
                table: "ProviderUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_ProviderUserRoles_Template_Scope_Name",
                table: "ProviderUserRoles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProviderUserRoles_TemplateScope",
                table: "ProviderUserRoles");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ProviderUserRoles");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "ProviderUserRoles");

            migrationBuilder.AlterColumn<long>(
                name: "ProviderId",
                table: "ProviderUserRoles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderUserRoles_ProviderId_Name",
                table: "ProviderUserRoles",
                columns: new[] { "ProviderId", "Name" },
                unique: true,
                filter: "[Deleted] = 0");
        }
    }
}
