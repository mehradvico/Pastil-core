using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanionAssistancePackageOnlineSelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanionAssistancePackageOnlineSelections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionAssistancePackageId = table.Column<long>(type: "bigint", nullable: false),
                    CompanionAssistancePackageOnlineId = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ActivationValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionAssistancePackageOnlineSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionAssistancePackageOnlineSelections_CompanionAssistancePackageOnlines_CompanionAssistancePackageOnlineId",
                        column: x => x.CompanionAssistancePackageOnlineId,
                        principalTable: "CompanionAssistancePackageOnlines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionAssistancePackageOnlineSelections_CompanionAssistancePackages_CompanionAssistancePackageId",
                        column: x => x.CompanionAssistancePackageId,
                        principalTable: "CompanionAssistancePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssistancePackageOnlineSelections_CompanionAssistancePackageId",
                table: "CompanionAssistancePackageOnlineSelections",
                column: "CompanionAssistancePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssistancePackageOnlineSelections_CompanionAssistancePackageOnlineId",
                table: "CompanionAssistancePackageOnlineSelections",
                column: "CompanionAssistancePackageOnlineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanionAssistancePackageOnlineSelections");
        }
    }
}
