using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SchoolCoursePetsAndGallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchoolCoursePets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolCourseId = table.Column<long>(type: "bigint", nullable: false),
                    PetId = table.Column<long>(type: "bigint", nullable: false),
                    PetBreedId = table.Column<long>(type: "bigint", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolCoursePets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolCoursePets_PetBreeds_PetBreedId",
                        column: x => x.PetBreedId,
                        principalTable: "PetBreeds",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SchoolCoursePets_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolCoursePets_SchoolCourses_SchoolCourseId",
                        column: x => x.SchoolCourseId,
                        principalTable: "SchoolCourses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCoursePets_PetBreedId",
                table: "SchoolCoursePets",
                column: "PetBreedId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCoursePets_PetId",
                table: "SchoolCoursePets",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCoursePets_SchoolCourseId",
                table: "SchoolCoursePets",
                column: "SchoolCourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchoolCoursePets");
        }
    }
}
