using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_scoreandreward : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BonusCode",
                table: "Users",
                newName: "ReferralCode");

            migrationBuilder.RenameColumn(
                name: "BonusCode",
                table: "ProductOrders",
                newName: "ReferralCode");

            migrationBuilder.RenameColumn(
                name: "BonusCode",
                table: "Carts",
                newName: "ReferralCode");

            migrationBuilder.AddColumn<double>(
                name: "CurrentScore",
                table: "Users",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "ClubRewardId",
                table: "Rebate",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxUsePerUser",
                table: "Rebate",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ClubRewards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequiredScore = table.Column<double>(type: "float", nullable: false),
                    RebateId = table.Column<long>(type: "bigint", nullable: false),
                    ValidityDays = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    RebateId1 = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubRewards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubRewards_Rebate_RebateId1",
                        column: x => x.RebateId1,
                        principalTable: "Rebate",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScoreTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionTypeId = table.Column<long>(type: "bigint", nullable: false),
                    ReferenceId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScoreTransactions_Codes_TransactionTypeId",
                        column: x => x.TransactionTypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScoreTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRebates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RebateId = table.Column<long>(type: "bigint", nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRebates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRebates_Rebate_RebateId",
                        column: x => x.RebateId,
                        principalTable: "Rebate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRebates_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewards_RebateId1",
                table: "ClubRewards",
                column: "RebateId1");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreTransactions_TransactionTypeId",
                table: "ScoreTransactions",
                column: "TransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreTransactions_UserId",
                table: "ScoreTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRebates_RebateId",
                table: "UserRebates",
                column: "RebateId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRebates_UserId",
                table: "UserRebates",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClubRewards");

            migrationBuilder.DropTable(
                name: "ScoreTransactions");

            migrationBuilder.DropTable(
                name: "UserRebates");

            migrationBuilder.DropColumn(
                name: "CurrentScore",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ClubRewardId",
                table: "Rebate");

            migrationBuilder.DropColumn(
                name: "MaxUsePerUser",
                table: "Rebate");

            migrationBuilder.RenameColumn(
                name: "ReferralCode",
                table: "Users",
                newName: "BonusCode");

            migrationBuilder.RenameColumn(
                name: "ReferralCode",
                table: "ProductOrders",
                newName: "BonusCode");

            migrationBuilder.RenameColumn(
                name: "ReferralCode",
                table: "Carts",
                newName: "BonusCode");
        }
    }
}
