using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPastilClubFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClubPointAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AvailablePoint = table.Column<long>(type: "bigint", nullable: false),
                    DebtPoint = table.Column<long>(type: "bigint", nullable: false),
                    LifetimeEarnedPoint = table.Column<long>(type: "bigint", nullable: false),
                    LifetimeSpentPoint = table.Column<long>(type: "bigint", nullable: false),
                    LifetimeReversedPoint = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubPointAccounts", x => x.Id);
                    table.CheckConstraint("CK_ClubPointAccount_AvailablePoint", "[AvailablePoint] >= 0");
                    table.CheckConstraint("CK_ClubPointAccount_DebtPoint", "[DebtPoint] >= 0");
                    table.ForeignKey(
                        name: "FK_ClubPointAccounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClubPointRules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    PointAmount = table.Column<long>(type: "bigint", nullable: false),
                    DailyLimit = table.Column<int>(type: "int", nullable: true),
                    MonthlyLimit = table.Column<int>(type: "int", nullable: true),
                    LifetimeLimit = table.Column<int>(type: "int", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubPointRules", x => x.Id);
                    table.CheckConstraint("CK_ClubPointRule_PointAmount", "[PointAmount] > 0");
                });

            migrationBuilder.CreateTable(
                name: "ClubPointTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    PointAccountId = table.Column<long>(type: "bigint", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    AvailableBefore = table.Column<long>(type: "bigint", nullable: false),
                    AvailableAfter = table.Column<long>(type: "bigint", nullable: false),
                    DebtBefore = table.Column<long>(type: "bigint", nullable: false),
                    DebtAfter = table.Column<long>(type: "bigint", nullable: false),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<long>(type: "bigint", nullable: true),
                    PointRuleId = table.Column<long>(type: "bigint", nullable: true),
                    ReferralId = table.Column<long>(type: "bigint", nullable: true),
                    RewardRedemptionId = table.Column<long>(type: "bigint", nullable: true),
                    ParentTransactionId = table.Column<long>(type: "bigint", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedByAdminId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubPointTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubPointTransactions_ClubPointAccounts_PointAccountId",
                        column: x => x.PointAccountId,
                        principalTable: "ClubPointAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubPointTransactions_ClubPointRules_PointRuleId",
                        column: x => x.PointRuleId,
                        principalTable: "ClubPointRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubPointTransactions_ClubPointTransactions_ParentTransactionId",
                        column: x => x.ParentTransactionId,
                        principalTable: "ClubPointTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubPointTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClubPointAccounts_UserId",
                table: "ClubPointAccounts",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubPointRules_EventType",
                table: "ClubPointRules",
                column: "EventType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubPointTransactions_IdempotencyKey",
                table: "ClubPointTransactions",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubPointTransactions_ParentTransactionId",
                table: "ClubPointTransactions",
                column: "ParentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubPointTransactions_PointAccountId",
                table: "ClubPointTransactions",
                column: "PointAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubPointTransactions_PointRuleId",
                table: "ClubPointTransactions",
                column: "PointRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubPointTransactions_SourceType_SourceId",
                table: "ClubPointTransactions",
                columns: new[] { "SourceType", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClubPointTransactions_UserId_CreateDate",
                table: "ClubPointTransactions",
                columns: new[] { "UserId", "CreateDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClubPointTransactions");

            migrationBuilder.DropTable(
                name: "ClubPointAccounts");

            migrationBuilder.DropTable(
                name: "ClubPointRules");
        }
    }
}
