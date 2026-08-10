using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPastilClubRewardCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClubRewardTemplates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RewardType = table.Column<int>(type: "int", nullable: false),
                    PointCost = table.Column<long>(type: "bigint", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ExpirationType = table.Column<int>(type: "int", nullable: false),
                    ExpirationValue = table.Column<int>(type: "int", nullable: true),
                    FixedExpirationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BenefitValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MaximumBenefitValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    FundingType = table.Column<int>(type: "int", nullable: false),
                    IsAutomationAllowed = table.Column<bool>(type: "bit", nullable: false),
                    IsManualAllowed = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    NotificationLevel = table.Column<int>(type: "int", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    Terms = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubRewardTemplates", x => x.Id);
                    table.CheckConstraint("CK_ClubRewardTemplate_PointCost", "[PointCost] > 0");
                    table.ForeignKey(
                        name: "FK_ClubRewardTemplates_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ClubRewardOffers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RewardTemplateId = table.Column<long>(type: "bigint", nullable: false),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    AutomationRuleId = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PointCostSnapshot = table.Column<long>(type: "bigint", nullable: false),
                    GeneratedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ApprovedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RejectedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ApprovedByAdminId = table.Column<long>(type: "bigint", nullable: true),
                    RejectedByAdminId = table.Column<long>(type: "bigint", nullable: true),
                    RejectReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RedeemedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubRewardOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubRewardOffers_ClubRewardTemplates_RewardTemplateId",
                        column: x => x.RewardTemplateId,
                        principalTable: "ClubRewardTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubRewardOffers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClubRewardPetTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RewardTemplateId = table.Column<long>(type: "bigint", nullable: false),
                    PetTypeId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubRewardPetTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubRewardPetTypes_ClubRewardTemplates_RewardTemplateId",
                        column: x => x.RewardTemplateId,
                        principalTable: "ClubRewardTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubRewardPetTypes_Pets_PetTypeId",
                        column: x => x.PetTypeId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClubRewardTargets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RewardTemplateId = table.Column<long>(type: "bigint", nullable: false),
                    TargetType = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<long>(type: "bigint", nullable: true),
                    IncludeChildren = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubRewardTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubRewardTargets_ClubRewardTemplates_RewardTemplateId",
                        column: x => x.RewardTemplateId,
                        principalTable: "ClubRewardTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClubRewardRedemptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RewardOfferId = table.Column<long>(type: "bigint", nullable: false),
                    RewardTemplateId = table.Column<long>(type: "bigint", nullable: false),
                    PointTransactionId = table.Column<long>(type: "bigint", nullable: false),
                    BenefitType = table.Column<int>(type: "int", nullable: false),
                    BenefitReferenceId = table.Column<long>(type: "bigint", nullable: true),
                    PointSpent = table.Column<long>(type: "bigint", nullable: false),
                    RedeemedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubRewardRedemptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubRewardRedemptions_ClubPointTransactions_PointTransactionId",
                        column: x => x.PointTransactionId,
                        principalTable: "ClubPointTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubRewardRedemptions_ClubRewardOffers_RewardOfferId",
                        column: x => x.RewardOfferId,
                        principalTable: "ClubRewardOffers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubRewardRedemptions_ClubRewardTemplates_RewardTemplateId",
                        column: x => x.RewardTemplateId,
                        principalTable: "ClubRewardTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubRewardRedemptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardOffers_RewardTemplateId_Status",
                table: "ClubRewardOffers",
                columns: new[] { "RewardTemplateId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardOffers_UserId_RewardTemplateId",
                table: "ClubRewardOffers",
                columns: new[] { "UserId", "RewardTemplateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardOffers_UserId_Status_ExpiresAt",
                table: "ClubRewardOffers",
                columns: new[] { "UserId", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardPetTypes_PetTypeId",
                table: "ClubRewardPetTypes",
                column: "PetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardPetTypes_RewardTemplateId_PetTypeId",
                table: "ClubRewardPetTypes",
                columns: new[] { "RewardTemplateId", "PetTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardRedemptions_IdempotencyKey",
                table: "ClubRewardRedemptions",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardRedemptions_PointTransactionId",
                table: "ClubRewardRedemptions",
                column: "PointTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardRedemptions_RewardOfferId",
                table: "ClubRewardRedemptions",
                column: "RewardOfferId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardRedemptions_RewardTemplateId",
                table: "ClubRewardRedemptions",
                column: "RewardTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardRedemptions_UserId_RedeemedDate",
                table: "ClubRewardRedemptions",
                columns: new[] { "UserId", "RedeemedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardTargets_RewardTemplateId_TargetType_TargetId",
                table: "ClubRewardTargets",
                columns: new[] { "RewardTemplateId", "TargetType", "TargetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardTemplates_Active_StartDate_EndDate",
                table: "ClubRewardTemplates",
                columns: new[] { "Active", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardTemplates_PictureId",
                table: "ClubRewardTemplates",
                column: "PictureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClubRewardPetTypes");

            migrationBuilder.DropTable(
                name: "ClubRewardRedemptions");

            migrationBuilder.DropTable(
                name: "ClubRewardTargets");

            migrationBuilder.DropTable(
                name: "ClubRewardOffers");

            migrationBuilder.DropTable(
                name: "ClubRewardTemplates");
        }
    }
}
