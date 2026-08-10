using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPetMemories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Memories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    MemoryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Memories_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserMemories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    UserPetId = table.Column<long>(type: "bigint", nullable: false),
                    MemoryId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMemories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMemories_Memories_MemoryId",
                        column: x => x.MemoryId,
                        principalTable: "Memories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMemories_UserPets_UserPetId",
                        column: x => x.UserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMemories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Memories_Deleted_MemoryDate",
                table: "Memories",
                columns: new[] { "Deleted", "MemoryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Memories_MemoryDate",
                table: "Memories",
                column: "MemoryDate");

            migrationBuilder.CreateIndex(
                name: "IX_Memories_PictureId",
                table: "Memories",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMemories_MemoryId",
                table: "UserMemories",
                column: "MemoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMemories_UserId_Deleted",
                table: "UserMemories",
                columns: new[] { "UserId", "Deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_UserMemories_UserPetId_Deleted",
                table: "UserMemories",
                columns: new[] { "UserPetId", "Deleted" });

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 23 AND ISNULL(Label, N'') <> N'PushMemoryReminder')
    THROW 51000, 'PushType Id 23 is already assigned to another label.', 1;

IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 23)
BEGIN
    SET IDENTITY_INSERT PushTypes ON;
    INSERT INTO PushTypes (Id, Name, Label)
    VALUES (23, N'یادآوری ثبت خاطره روزانه', N'PushMemoryReminder');
    SET IDENTITY_INSERT PushTypes OFF;
END;

IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 23)
BEGIN
    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
    VALUES (23, N'Push_Title', N'PushMemoryReminder', N'/memories', NULL, N'memory-daily-reminder', 1);

    DECLARE @MemoryPushPatternId bigint = SCOPE_IDENTITY();
    INSERT INTO PushSettings (PushPatternId, IsEnabled)
    VALUES (@MemoryPushPatternId, 1);
END;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE n
FROM PushNotifications n
INNER JOIN PushPatterns p ON p.Id = n.PushPatternId
WHERE p.PushTypeId = 23;

DELETE s
FROM PushSettings s
INNER JOIN PushPatterns p ON p.Id = s.PushPatternId
WHERE p.PushTypeId = 23;

DELETE FROM PushPatterns WHERE PushTypeId = 23;
DELETE FROM PushTypes WHERE Id = 23 AND Label = N'PushMemoryReminder';
");

            migrationBuilder.DropTable(
                name: "UserMemories");

            migrationBuilder.DropTable(
                name: "Memories");
        }
    }
}
