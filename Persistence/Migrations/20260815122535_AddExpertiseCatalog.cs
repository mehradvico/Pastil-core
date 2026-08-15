using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExpertiseCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.CompanionUsers', N'Expertise') IS NOT NULL
                    ALTER TABLE dbo.CompanionUsers DROP COLUMN Expertise;
                """);

            migrationBuilder.AddColumn<long>(
                name: "ExpertiseId",
                table: "CompanionUsers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Expertises",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expertises", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanionUsers_ExpertiseId",
                table: "CompanionUsers",
                column: "ExpertiseId");

            migrationBuilder.CreateIndex(
                name: "IX_Expertises_Name_Deleted",
                table: "Expertises",
                columns: new[] { "Name", "Deleted" });

            migrationBuilder.AddForeignKey(
                name: "FK_CompanionUsers_Expertises_ExpertiseId",
                table: "CompanionUsers",
                column: "ExpertiseId",
                principalTable: "Expertises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanionUsers_Expertises_ExpertiseId",
                table: "CompanionUsers");

            migrationBuilder.DropTable(
                name: "Expertises");

            migrationBuilder.DropIndex(
                name: "IX_CompanionUsers_ExpertiseId",
                table: "CompanionUsers");

            migrationBuilder.DropColumn(
                name: "ExpertiseId",
                table: "CompanionUsers");

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.CompanionUsers', N'Expertise') IS NULL
                    ALTER TABLE dbo.CompanionUsers ADD Expertise nvarchar(max) NULL;
                """);
        }
    }
}
