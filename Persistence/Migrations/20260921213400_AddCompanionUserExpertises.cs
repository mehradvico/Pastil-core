using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanionUserExpertises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanionUserExpertises",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionUserId = table.Column<long>(type: "bigint", nullable: false),
                    ExpertiseId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionUserExpertises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionUserExpertises_CompanionUsers_CompanionUserId",
                        column: x => x.CompanionUserId,
                        principalTable: "CompanionUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanionUserExpertises_Expertises_ExpertiseId",
                        column: x => x.ExpertiseId,
                        principalTable: "Expertises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanionUserExpertises_CompanionUserId_ExpertiseId",
                table: "CompanionUserExpertises",
                columns: new[] { "CompanionUserId", "ExpertiseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanionUserExpertises_ExpertiseId",
                table: "CompanionUserExpertises",
                column: "ExpertiseId");

            // داده‌ی قبلی: هر عضو تیم که یک تخصص تکی داشت، همان یک تخصص در جدول چندتخصصی هم ثبت می‌شود
            migrationBuilder.Sql(@"
INSERT INTO CompanionUserExpertises (CompanionUserId, ExpertiseId)
SELECT cu.Id, cu.ExpertiseId
FROM CompanionUsers cu
WHERE cu.ExpertiseId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM CompanionUserExpertises x WHERE x.CompanionUserId = cu.Id AND x.ExpertiseId = cu.ExpertiseId);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanionUserExpertises");
        }
    }
}
