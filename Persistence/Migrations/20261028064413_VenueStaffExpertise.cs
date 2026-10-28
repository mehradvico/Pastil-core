using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VenueStaffExpertise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ExpertiseId",
                table: "GroomerUsers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ExpertiseId",
                table: "CoachUsers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ExpertiseId",
                table: "ClinicUsers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClinicUserExpertises",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicUserId = table.Column<long>(type: "bigint", nullable: false),
                    ExpertiseId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicUserExpertises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicUserExpertises_ClinicUsers_ClinicUserId",
                        column: x => x.ClinicUserId,
                        principalTable: "ClinicUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicUserExpertises_Expertises_ExpertiseId",
                        column: x => x.ExpertiseId,
                        principalTable: "Expertises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachUserExpertises",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachUserId = table.Column<long>(type: "bigint", nullable: false),
                    ExpertiseId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachUserExpertises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachUserExpertises_CoachUsers_CoachUserId",
                        column: x => x.CoachUserId,
                        principalTable: "CoachUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachUserExpertises_Expertises_ExpertiseId",
                        column: x => x.ExpertiseId,
                        principalTable: "Expertises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroomerUserExpertises",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroomerUserId = table.Column<long>(type: "bigint", nullable: false),
                    ExpertiseId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomerUserExpertises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomerUserExpertises_Expertises_ExpertiseId",
                        column: x => x.ExpertiseId,
                        principalTable: "Expertises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerUserExpertises_GroomerUsers_GroomerUserId",
                        column: x => x.GroomerUserId,
                        principalTable: "GroomerUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroomerUsers_ExpertiseId",
                table: "GroomerUsers",
                column: "ExpertiseId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachUsers_ExpertiseId",
                table: "CoachUsers",
                column: "ExpertiseId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicUsers_ExpertiseId",
                table: "ClinicUsers",
                column: "ExpertiseId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicUserExpertises_ClinicUserId_ExpertiseId",
                table: "ClinicUserExpertises",
                columns: new[] { "ClinicUserId", "ExpertiseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicUserExpertises_ExpertiseId",
                table: "ClinicUserExpertises",
                column: "ExpertiseId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachUserExpertises_CoachUserId_ExpertiseId",
                table: "CoachUserExpertises",
                columns: new[] { "CoachUserId", "ExpertiseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoachUserExpertises_ExpertiseId",
                table: "CoachUserExpertises",
                column: "ExpertiseId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerUserExpertises_ExpertiseId",
                table: "GroomerUserExpertises",
                column: "ExpertiseId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerUserExpertises_GroomerUserId_ExpertiseId",
                table: "GroomerUserExpertises",
                columns: new[] { "GroomerUserId", "ExpertiseId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClinicUsers_Expertises_ExpertiseId",
                table: "ClinicUsers",
                column: "ExpertiseId",
                principalTable: "Expertises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CoachUsers_Expertises_ExpertiseId",
                table: "CoachUsers",
                column: "ExpertiseId",
                principalTable: "Expertises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroomerUsers_Expertises_ExpertiseId",
                table: "GroomerUsers",
                column: "ExpertiseId",
                principalTable: "Expertises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClinicUsers_Expertises_ExpertiseId",
                table: "ClinicUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_CoachUsers_Expertises_ExpertiseId",
                table: "CoachUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_GroomerUsers_Expertises_ExpertiseId",
                table: "GroomerUsers");

            migrationBuilder.DropTable(
                name: "ClinicUserExpertises");

            migrationBuilder.DropTable(
                name: "CoachUserExpertises");

            migrationBuilder.DropTable(
                name: "GroomerUserExpertises");

            migrationBuilder.DropIndex(
                name: "IX_GroomerUsers_ExpertiseId",
                table: "GroomerUsers");

            migrationBuilder.DropIndex(
                name: "IX_CoachUsers_ExpertiseId",
                table: "CoachUsers");

            migrationBuilder.DropIndex(
                name: "IX_ClinicUsers_ExpertiseId",
                table: "ClinicUsers");

            migrationBuilder.DropColumn(
                name: "ExpertiseId",
                table: "GroomerUsers");

            migrationBuilder.DropColumn(
                name: "ExpertiseId",
                table: "CoachUsers");

            migrationBuilder.DropColumn(
                name: "ExpertiseId",
                table: "ClinicUsers");
        }
    }
}
