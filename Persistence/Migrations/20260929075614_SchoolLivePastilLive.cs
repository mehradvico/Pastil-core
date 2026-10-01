using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SchoolLivePastilLive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderPushSentDate",
                table: "SchoolCourseSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SchoolCourseLiveSessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolCourseSessionId = table.Column<long>(type: "bigint", nullable: false),
                    RoomName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    StartedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WantsRecording = table.Column<bool>(type: "bit", nullable: false),
                    RecordingName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordingDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordingStatusId = table.Column<int>(type: "int", nullable: false),
                    EgressId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolCourseVideoId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolCourseLiveSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolCourseLiveSessions_SchoolCourseSessions_SchoolCourseSessionId",
                        column: x => x.SchoolCourseSessionId,
                        principalTable: "SchoolCourseSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolCourseLiveSessions_SchoolCourseVideos_SchoolCourseVideoId",
                        column: x => x.SchoolCourseVideoId,
                        principalTable: "SchoolCourseVideos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SchoolLiveComments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolCourseLiveSessionId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolLiveComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolLiveComments_SchoolCourseLiveSessions_SchoolCourseLiveSessionId",
                        column: x => x.SchoolCourseLiveSessionId,
                        principalTable: "SchoolCourseLiveSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolLiveComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCourseLiveSessions_SchoolCourseSessionId",
                table: "SchoolCourseLiveSessions",
                column: "SchoolCourseSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCourseLiveSessions_SchoolCourseVideoId",
                table: "SchoolCourseLiveSessions",
                column: "SchoolCourseVideoId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolLiveComments_SchoolCourseLiveSessionId",
                table: "SchoolLiveComments",
                column: "SchoolCourseLiveSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolLiveComments_UserId",
                table: "SchoolLiveComments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchoolLiveComments");

            migrationBuilder.DropTable(
                name: "SchoolCourseLiveSessions");

            migrationBuilder.DropColumn(
                name: "ReminderPushSentDate",
                table: "SchoolCourseSessions");
        }
    }
}
