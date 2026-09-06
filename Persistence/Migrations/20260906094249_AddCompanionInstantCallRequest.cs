using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanionInstantCallRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanionInstantCallRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionAssistancePackageOnlineSelectionId = table.Column<long>(type: "bigint", nullable: false),
                    BookerId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionInstantCallRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionInstantCallRequests_CompanionAssistancePackageOnlineSelections_CompanionAssistancePackageOnlineSelectionId",
                        column: x => x.CompanionAssistancePackageOnlineSelectionId,
                        principalTable: "CompanionAssistancePackageOnlineSelections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionInstantCallRequests_Users_BookerId",
                        column: x => x.BookerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanionInstantCallRequests_BookerId",
                table: "CompanionInstantCallRequests",
                column: "BookerId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionInstantCallRequests_CompanionAssistancePackageOnlineSelectionId",
                table: "CompanionInstantCallRequests",
                column: "CompanionAssistancePackageOnlineSelectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanionInstantCallRequests");
        }
    }
}
