using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProviderUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProviderUserRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderUserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderUserRoles_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderUserRolePermissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderUserRoleId = table.Column<long>(type: "bigint", nullable: false),
                    PermissionKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderUserRolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderUserRolePermissions_ProviderUserRoles_ProviderUserRoleId",
                        column: x => x.ProviderUserRoleId,
                        principalTable: "ProviderUserRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProviderUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ProviderUserRoleId = table.Column<long>(type: "bigint", nullable: false),
                    UserAccept = table.Column<bool>(type: "bit", nullable: true),
                    InviteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderUsers_ProviderUserRoles_ProviderUserRoleId",
                        column: x => x.ProviderUserRoleId,
                        principalTable: "ProviderUserRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderUsers_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderUserRolePermissions_ProviderUserRoleId_PermissionKey",
                table: "ProviderUserRolePermissions",
                columns: new[] { "ProviderUserRoleId", "PermissionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderUserRoles_ProviderId_Name",
                table: "ProviderUserRoles",
                columns: new[] { "ProviderId", "Name" },
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderUsers_ProviderId_UserId",
                table: "ProviderUsers",
                columns: new[] { "ProviderId", "UserId" },
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderUsers_ProviderUserRoleId",
                table: "ProviderUsers",
                column: "ProviderUserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderUsers_UserId",
                table: "ProviderUsers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderUserRolePermissions");

            migrationBuilder.DropTable(
                name: "ProviderUsers");

            migrationBuilder.DropTable(
                name: "ProviderUserRoles");
        }
    }
}
