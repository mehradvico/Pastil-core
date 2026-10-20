using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProviderRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProviderRoleAssignments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<long>(type: "bigint", nullable: false),
                    ProviderRoleId = table.Column<long>(type: "bigint", nullable: false),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderRoleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderRoleAssignments_ProviderRoles_ProviderRoleId",
                        column: x => x.ProviderRoleId,
                        principalTable: "ProviderRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderRoleAssignments_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRoleAssignments_ProviderId_ProviderRoleId",
                table: "ProviderRoleAssignments",
                columns: new[] { "ProviderId", "ProviderRoleId" },
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRoleAssignments_ProviderRoleId",
                table: "ProviderRoleAssignments",
                column: "ProviderRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRoles_Label",
                table: "ProviderRoles",
                column: "Label",
                unique: true,
                filter: "[Deleted] = 0");

            // ---- کاتالوگ نقش‌ها (کلید ثابت = Label؛ کد با Label کار می‌کند نه Id) ----
            migrationBuilder.Sql(@"
INSERT INTO ProviderRoles (Name, Label, Active, Deleted) VALUES
 (N'فروشگاه', N'Store', 1, 0),
 (N'کلینیک', N'Clinic', 1, 0),
 (N'مربی', N'Coach', 1, 0),
 (N'آرایشگاه', N'Groomer', 1, 0),
 (N'پانسیون', N'Pansion', 1, 0),
 (N'مدرسه', N'School', 1, 0);");

            // ---- تبدیل تخصص‌های قبلی (enum) به نقش‌ها: 1 فروشنده→Store، 2 آرایشگر→Groomer، 3 مربی→Coach، 4 دامپزشک→Clinic، 5 پانسیون‌دار→Pansion ----
            migrationBuilder.Sql(@"
INSERT INTO ProviderRoleAssignments (ProviderId, ProviderRoleId, Approved, Deleted)
SELECT c.ProviderId, r.Id, c.Approved, c.Deleted
FROM ProviderCapabilities c
JOIN ProviderRoles r ON r.Label = CASE c.[Type]
    WHEN 1 THEN N'Store' WHEN 2 THEN N'Groomer' WHEN 3 THEN N'Coach' WHEN 4 THEN N'Clinic' WHEN 5 THEN N'Pansion' END;

-- ارائه‌دهندگانی که مدرسه دارند ولی نقش مدرسه ندارند
INSERT INTO ProviderRoleAssignments (ProviderId, ProviderRoleId, Approved, Deleted)
SELECT cp.ProviderId, r.Id, MAX(CAST(sc.Approve AS int)), 0
FROM Schools sc
JOIN Companions cp ON cp.Id = sc.CompanionId
JOIN ProviderRoles r ON r.Label = N'School'
WHERE cp.ProviderId IS NOT NULL AND sc.Deleted = 0
  AND NOT EXISTS (SELECT 1 FROM ProviderRoleAssignments a WHERE a.ProviderId = cp.ProviderId AND a.ProviderRoleId = r.Id AND a.Deleted = 0)
GROUP BY cp.ProviderId, r.Id;");

            migrationBuilder.DropTable(
                name: "ProviderCapabilities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProviderCapabilities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<long>(type: "bigint", nullable: false),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderCapabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderCapabilities_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderCapabilities_ProviderId_Type",
                table: "ProviderCapabilities",
                columns: new[] { "ProviderId", "Type" },
                unique: true,
                filter: "[Deleted] = 0");

            // بازگردانی نقش‌های قابل‌نگاشت (مدرسه در enum قدیمی نبود و برگردانده نمی‌شود)
            migrationBuilder.Sql(@"
INSERT INTO ProviderCapabilities (ProviderId, [Type], Approved, Deleted)
SELECT a.ProviderId, CASE r.Label WHEN N'Store' THEN 1 WHEN N'Groomer' THEN 2 WHEN N'Coach' THEN 3 WHEN N'Clinic' THEN 4 WHEN N'Pansion' THEN 5 END, a.Approved, a.Deleted
FROM ProviderRoleAssignments a JOIN ProviderRoles r ON r.Id = a.ProviderRoleId
WHERE r.Label IN (N'Store', N'Groomer', N'Coach', N'Clinic', N'Pansion');");

            migrationBuilder.DropTable(
                name: "ProviderRoleAssignments");

            migrationBuilder.DropTable(
                name: "ProviderRoles");
        }
    }
}
