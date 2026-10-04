using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPetMicrochipRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PetMicrochipRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MicrochipCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PublicToken = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    ClientIp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    FoundUserPetId = table.Column<long>(type: "bigint", nullable: true),
                    MatchCount = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FollowUpRequested = table.Column<bool>(type: "bit", nullable: false),
                    FollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AdminNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HandledByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ClosedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetMicrochipRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PetMicrochipRequests_UserPets_FoundUserPetId",
                        column: x => x.FoundUserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PetMicrochipRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PetMicrochipRequests_FoundUserPetId",
                table: "PetMicrochipRequests",
                column: "FoundUserPetId");

            migrationBuilder.CreateIndex(
                name: "IX_PetMicrochipRequests_MicrochipCode",
                table: "PetMicrochipRequests",
                column: "MicrochipCode");

            migrationBuilder.CreateIndex(
                name: "IX_PetMicrochipRequests_PublicToken",
                table: "PetMicrochipRequests",
                column: "PublicToken",
                unique: true,
                filter: "[PublicToken] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PetMicrochipRequests_Status_CreateDate",
                table: "PetMicrochipRequests",
                columns: new[] { "Status", "CreateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PetMicrochipRequests_UserId",
                table: "PetMicrochipRequests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PetMicrochipRequests");
        }
    }
}
