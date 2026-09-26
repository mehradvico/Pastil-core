using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAssistanceExpertises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssistanceExpertises",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssistanceId = table.Column<long>(type: "bigint", nullable: false),
                    ExpertiseId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistanceExpertises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssistanceExpertises_Assistances_AssistanceId",
                        column: x => x.AssistanceId,
                        principalTable: "Assistances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssistanceExpertises_Expertises_ExpertiseId",
                        column: x => x.ExpertiseId,
                        principalTable: "Expertises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssistanceExpertises_AssistanceId_ExpertiseId",
                table: "AssistanceExpertises",
                columns: new[] { "AssistanceId", "ExpertiseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssistanceExpertises_ExpertiseId",
                table: "AssistanceExpertises",
                column: "ExpertiseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssistanceExpertises");
        }
    }
}
