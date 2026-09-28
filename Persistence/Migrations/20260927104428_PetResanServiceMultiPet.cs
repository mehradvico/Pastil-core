using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PetResanServiceMultiPet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PetResanServicePets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PetResanServiceId = table.Column<long>(type: "bigint", nullable: false),
                    UserPetId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetResanServicePets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PetResanServicePets_PetResanServices_PetResanServiceId",
                        column: x => x.PetResanServiceId,
                        principalTable: "PetResanServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PetResanServicePets_UserPets_UserPetId",
                        column: x => x.UserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PetResanServicePets_PetResanServiceId",
                table: "PetResanServicePets",
                column: "PetResanServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PetResanServicePets_UserPetId",
                table: "PetResanServicePets",
                column: "UserPetId");

            // بک‌فیل: سرویس‌های قبلی هرکدام یک پت (UserPetId) داشتند؛ همان را به جدول جدید هم منتقل می‌کنیم
            // تا کدهایی که از این به بعد فقط از Pets می‌خوانند، سرویس‌های قدیمی را هم ببینند.
            migrationBuilder.Sql(@"
INSERT INTO PetResanServicePets (PetResanServiceId, UserPetId)
SELECT s.Id, s.UserPetId
FROM PetResanServices s
WHERE NOT EXISTS (SELECT 1 FROM PetResanServicePets p WHERE p.PetResanServiceId = s.Id);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PetResanServicePets");
        }
    }
}
