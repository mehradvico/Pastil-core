using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_pastilmatch01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PastilMatchProfiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserPetId = table.Column<long>(type: "bigint", nullable: false),
                    EnergyLevelId = table.Column<long>(type: "bigint", nullable: false),
                    SocialLevelId = table.Column<long>(type: "bigint", nullable: false),
                    LikeCount = table.Column<int>(type: "int", nullable: false),
                    LiveLocation = table.Column<Point>(type: "geography", nullable: true),
                    CityId = table.Column<long>(type: "bigint", nullable: true),
                    NeighborhoodId = table.Column<long>(type: "bigint", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: true),
                    AdminDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerificationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastActiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatchProfiles_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PastilMatchProfiles_Codes_EnergyLevelId",
                        column: x => x.EnergyLevelId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchProfiles_Codes_SocialLevelId",
                        column: x => x.SocialLevelId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchProfiles_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PastilMatchProfiles_UserPets_UserPetId",
                        column: x => x.UserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchReportReasons",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsDescriptionRequired = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchReportReasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchProfileGoals",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PastilMatchProfileId = table.Column<long>(type: "bigint", nullable: false),
                    PastilMatchGoalId = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchProfileGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatchProfileGoals_Codes_PastilMatchGoalId",
                        column: x => x.PastilMatchGoalId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchProfileGoals_PastilMatchProfiles_PastilMatchProfileId",
                        column: x => x.PastilMatchProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchProfileLikes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LikerProfileId = table.Column<long>(type: "bigint", nullable: false),
                    LikedProfileId = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchProfileLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatchProfileLikes_PastilMatchProfiles_LikedProfileId",
                        column: x => x.LikedProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchProfileLikes_PastilMatchProfiles_LikerProfileId",
                        column: x => x.LikerProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderProfileId = table.Column<long>(type: "bigint", nullable: false),
                    ReceiverProfileId = table.Column<long>(type: "bigint", nullable: false),
                    PastilMatchGoalId = table.Column<long>(type: "bigint", nullable: false),
                    StatusId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompatibilityPercent = table.Column<int>(type: "int", nullable: false),
                    ResponseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatchRequests_Codes_PastilMatchGoalId",
                        column: x => x.PastilMatchGoalId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchRequests_Codes_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchRequests_PastilMatchProfiles_ReceiverProfileId",
                        column: x => x.ReceiverProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchRequests_PastilMatchProfiles_SenderProfileId",
                        column: x => x.SenderProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PastilMatchRequestId = table.Column<long>(type: "bigint", nullable: false),
                    FirstProfileId = table.Column<long>(type: "bigint", nullable: false),
                    SecondProfileId = table.Column<long>(type: "bigint", nullable: false),
                    PastilMatchGoalId = table.Column<long>(type: "bigint", nullable: false),
                    StatusId = table.Column<long>(type: "bigint", nullable: false),
                    CompatibilityPercent = table.Column<int>(type: "int", nullable: false),
                    CloseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatches_Codes_PastilMatchGoalId",
                        column: x => x.PastilMatchGoalId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatches_Codes_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatches_PastilMatchProfiles_FirstProfileId",
                        column: x => x.FirstProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatches_PastilMatchProfiles_SecondProfileId",
                        column: x => x.SecondProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatches_PastilMatchRequests_PastilMatchRequestId",
                        column: x => x.PastilMatchRequestId,
                        principalTable: "PastilMatchRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchBlocks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlockerUserId = table.Column<long>(type: "bigint", nullable: false),
                    BlockedUserId = table.Column<long>(type: "bigint", nullable: false),
                    PastilMatchId = table.Column<long>(type: "bigint", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatchBlocks_PastilMatches_PastilMatchId",
                        column: x => x.PastilMatchId,
                        principalTable: "PastilMatches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PastilMatchBlocks_Users_BlockedUserId",
                        column: x => x.BlockedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchBlocks_Users_BlockerUserId",
                        column: x => x.BlockerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PastilMatchId = table.Column<long>(type: "bigint", nullable: false),
                    SenderProfileId = table.Column<long>(type: "bigint", nullable: true),
                    PastilMatchMessageTypeId = table.Column<long>(type: "bigint", nullable: false),
                    ReplyToMessageId = table.Column<long>(type: "bigint", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false),
                    EditDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    PinDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatchMessages_Codes_PastilMatchMessageTypeId",
                        column: x => x.PastilMatchMessageTypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchMessages_PastilMatchMessages_ReplyToMessageId",
                        column: x => x.ReplyToMessageId,
                        principalTable: "PastilMatchMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PastilMatchMessages_PastilMatchProfiles_SenderProfileId",
                        column: x => x.SenderProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PastilMatchMessages_PastilMatches_PastilMatchId",
                        column: x => x.PastilMatchId,
                        principalTable: "PastilMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchMessageAttachments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PastilMatchMessageId = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchMessageAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatchMessageAttachments_PastilMatchMessages_PastilMatchMessageId",
                        column: x => x.PastilMatchMessageId,
                        principalTable: "PastilMatchMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchMessageReactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PastilMatchMessageId = table.Column<long>(type: "bigint", nullable: false),
                    ReactorProfileId = table.Column<long>(type: "bigint", nullable: false),
                    Reaction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchMessageReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatchMessageReactions_PastilMatchMessages_PastilMatchMessageId",
                        column: x => x.PastilMatchMessageId,
                        principalTable: "PastilMatchMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchMessageReactions_PastilMatchProfiles_ReactorProfileId",
                        column: x => x.ReactorProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchReports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReporterUserId = table.Column<long>(type: "bigint", nullable: false),
                    ReportedUserId = table.Column<long>(type: "bigint", nullable: false),
                    ReportedProfileId = table.Column<long>(type: "bigint", nullable: true),
                    PastilMatchId = table.Column<long>(type: "bigint", nullable: true),
                    PastilMatchMessageId = table.Column<long>(type: "bigint", nullable: true),
                    PastilMatchReportReasonId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdminDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatchReports_PastilMatchMessages_PastilMatchMessageId",
                        column: x => x.PastilMatchMessageId,
                        principalTable: "PastilMatchMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PastilMatchReports_PastilMatchProfiles_ReportedProfileId",
                        column: x => x.ReportedProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PastilMatchReports_PastilMatchReportReasons_PastilMatchReportReasonId",
                        column: x => x.PastilMatchReportReasonId,
                        principalTable: "PastilMatchReportReasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchReports_PastilMatches_PastilMatchId",
                        column: x => x.PastilMatchId,
                        principalTable: "PastilMatches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PastilMatchReports_Users_ReportedUserId",
                        column: x => x.ReportedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchReports_Users_ReporterUserId",
                        column: x => x.ReporterUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchBlocks_BlockedUserId",
                table: "PastilMatchBlocks",
                column: "BlockedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchBlocks_BlockerUserId",
                table: "PastilMatchBlocks",
                column: "BlockerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchBlocks_PastilMatchId",
                table: "PastilMatchBlocks",
                column: "PastilMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatches_FirstProfileId",
                table: "PastilMatches",
                column: "FirstProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatches_PastilMatchGoalId",
                table: "PastilMatches",
                column: "PastilMatchGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatches_PastilMatchRequestId",
                table: "PastilMatches",
                column: "PastilMatchRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatches_SecondProfileId",
                table: "PastilMatches",
                column: "SecondProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatches_StatusId",
                table: "PastilMatches",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchMessageAttachments_PastilMatchMessageId",
                table: "PastilMatchMessageAttachments",
                column: "PastilMatchMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchMessageReactions_PastilMatchMessageId",
                table: "PastilMatchMessageReactions",
                column: "PastilMatchMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchMessageReactions_ReactorProfileId",
                table: "PastilMatchMessageReactions",
                column: "ReactorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchMessages_PastilMatchId",
                table: "PastilMatchMessages",
                column: "PastilMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchMessages_PastilMatchMessageTypeId",
                table: "PastilMatchMessages",
                column: "PastilMatchMessageTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchMessages_ReplyToMessageId",
                table: "PastilMatchMessages",
                column: "ReplyToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchMessages_SenderProfileId",
                table: "PastilMatchMessages",
                column: "SenderProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfileGoals_PastilMatchGoalId",
                table: "PastilMatchProfileGoals",
                column: "PastilMatchGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfileGoals_PastilMatchProfileId",
                table: "PastilMatchProfileGoals",
                column: "PastilMatchProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfileLikes_LikedProfileId",
                table: "PastilMatchProfileLikes",
                column: "LikedProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfileLikes_LikerProfileId",
                table: "PastilMatchProfileLikes",
                column: "LikerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfiles_CityId",
                table: "PastilMatchProfiles",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfiles_EnergyLevelId",
                table: "PastilMatchProfiles",
                column: "EnergyLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfiles_NeighborhoodId",
                table: "PastilMatchProfiles",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfiles_SocialLevelId",
                table: "PastilMatchProfiles",
                column: "SocialLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfiles_UserPetId",
                table: "PastilMatchProfiles",
                column: "UserPetId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchReports_PastilMatchId",
                table: "PastilMatchReports",
                column: "PastilMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchReports_PastilMatchMessageId",
                table: "PastilMatchReports",
                column: "PastilMatchMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchReports_PastilMatchReportReasonId",
                table: "PastilMatchReports",
                column: "PastilMatchReportReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchReports_ReportedProfileId",
                table: "PastilMatchReports",
                column: "ReportedProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchReports_ReportedUserId",
                table: "PastilMatchReports",
                column: "ReportedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchReports_ReporterUserId",
                table: "PastilMatchReports",
                column: "ReporterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchRequests_PastilMatchGoalId",
                table: "PastilMatchRequests",
                column: "PastilMatchGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchRequests_ReceiverProfileId",
                table: "PastilMatchRequests",
                column: "ReceiverProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchRequests_SenderProfileId",
                table: "PastilMatchRequests",
                column: "SenderProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchRequests_StatusId",
                table: "PastilMatchRequests",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PastilMatchBlocks");

            migrationBuilder.DropTable(
                name: "PastilMatchMessageAttachments");

            migrationBuilder.DropTable(
                name: "PastilMatchMessageReactions");

            migrationBuilder.DropTable(
                name: "PastilMatchProfileGoals");

            migrationBuilder.DropTable(
                name: "PastilMatchProfileLikes");

            migrationBuilder.DropTable(
                name: "PastilMatchReports");

            migrationBuilder.DropTable(
                name: "PastilMatchMessages");

            migrationBuilder.DropTable(
                name: "PastilMatchReportReasons");

            migrationBuilder.DropTable(
                name: "PastilMatches");

            migrationBuilder.DropTable(
                name: "PastilMatchRequests");

            migrationBuilder.DropTable(
                name: "PastilMatchProfiles");
        }
    }
}
