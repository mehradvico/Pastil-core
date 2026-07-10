using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_petbreed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Race",
                table: "UserPets");

            migrationBuilder.AddColumn<bool>(
                name: "IsMixBreed",
                table: "UserPets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "PetBreed2Id",
                table: "UserPets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PetBreedId",
                table: "UserPets",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NoticeTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoticeTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PetBreeds",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PetId = table.Column<long>(type: "bigint", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetBreeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PetBreeds_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PetBreeds_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPets_PetBreed2Id",
                table: "UserPets",
                column: "PetBreed2Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserPets_PetBreedId",
                table: "UserPets",
                column: "PetBreedId");

            migrationBuilder.CreateIndex(
                name: "IX_PetBreeds_PetId",
                table: "PetBreeds",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_PetBreeds_PictureId",
                table: "PetBreeds",
                column: "PictureId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPets_PetBreeds_PetBreed2Id",
                table: "UserPets",
                column: "PetBreed2Id",
                principalTable: "PetBreeds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPets_PetBreeds_PetBreedId",
                table: "UserPets",
                column: "PetBreedId",
                principalTable: "PetBreeds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPets_PetBreeds_PetBreed2Id",
                table: "UserPets");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPets_PetBreeds_PetBreedId",
                table: "UserPets");

            migrationBuilder.DropTable(
                name: "NoticeTypes");

            migrationBuilder.DropTable(
                name: "PetBreeds");

            migrationBuilder.DropIndex(
                name: "IX_UserPets_PetBreed2Id",
                table: "UserPets");

            migrationBuilder.DropIndex(
                name: "IX_UserPets_PetBreedId",
                table: "UserPets");

            migrationBuilder.DropColumn(
                name: "IsMixBreed",
                table: "UserPets");

            migrationBuilder.DropColumn(
                name: "PetBreed2Id",
                table: "UserPets");

            migrationBuilder.DropColumn(
                name: "PetBreedId",
                table: "UserPets");

            migrationBuilder.AddColumn<string>(
                name: "Race",
                table: "UserPets",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
