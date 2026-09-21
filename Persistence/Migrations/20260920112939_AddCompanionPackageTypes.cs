using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanionPackageTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanionAssistancePackageTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionAssistancePackageId = table.Column<long>(type: "bigint", nullable: false),
                    CompanionAssistanceTypeId = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    PrePaymentPrice = table.Column<double>(type: "float", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionAssistancePackageTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionAssistancePackageTypes_Codes_CompanionAssistanceTypeId",
                        column: x => x.CompanionAssistanceTypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionAssistancePackageTypes_CompanionAssistancePackages_CompanionAssistancePackageId",
                        column: x => x.CompanionAssistancePackageId,
                        principalTable: "CompanionAssistancePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssistancePackageTypes_CompanionAssistancePackageId_CompanionAssistanceTypeId",
                table: "CompanionAssistancePackageTypes",
                columns: new[] { "CompanionAssistancePackageId", "CompanionAssistanceTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssistancePackageTypes_CompanionAssistanceTypeId",
                table: "CompanionAssistancePackageTypes",
                column: "CompanionAssistanceTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanionAssistancePackageTypes");
        }
    }
}
