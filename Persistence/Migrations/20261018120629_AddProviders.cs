using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProviders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProvider",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ProviderId",
                table: "Stores",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProviderId",
                table: "Companions",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Providers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Providers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProviderCapabilities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderCapabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderCapabilities_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderSubscriptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<long>(type: "bigint", nullable: false),
                    PlanId = table.Column<long>(type: "bigint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderSubscriptions_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderSubscriptions_SubscriptionPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stores_ProviderId",
                table: "Stores",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Companions_ProviderId",
                table: "Companions",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderCapabilities_ProviderId_Type",
                table: "ProviderCapabilities",
                columns: new[] { "ProviderId", "Type" },
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_UserId",
                table: "Providers",
                column: "UserId",
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderSubscriptions_PlanId",
                table: "ProviderSubscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderSubscriptions_ProviderId_EndDate",
                table: "ProviderSubscriptions",
                columns: new[] { "ProviderId", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_Level",
                table: "SubscriptionPlans",
                column: "Level");

            migrationBuilder.AddForeignKey(
                name: "FK_Companions_Providers_ProviderId",
                table: "Companions",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_Providers_ProviderId",
                table: "Stores",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ---- پلن‌های پیش‌فرض اشتراک (مزایا بعداً تعریف می‌شود) ----
            migrationBuilder.Sql(@"
INSERT INTO SubscriptionPlans (Name, Level, Active, Deleted) VALUES
 (N'نقره‌ای', 1, 1, 0), (N'طلایی', 2, 1, 0), (N'الماس', 3, 1, 0);");

            // ---- backfill: هر مالک نماینده و هر «صاحب» فروشگاه یک Provider می‌شود ----
            // صاحب فروشگاه = کوچک‌ترین UsersId از جدول StoreUser (فروشگاه‌های چندکاربره را بعد از اجرا دستی بازبینی کنید؛
            // کوئری گزارش در scripts/AddProviders.sql است).
            migrationBuilder.Sql(@"
INSERT INTO Providers (UserId, Active, Approved, Deleted, CreateDate)
SELECT o.UserId, 1, 1, 0, GETDATE()
FROM (
    SELECT c.OwnerId AS UserId FROM Companions c WHERE c.Deleted = 0
    UNION
    SELECT MIN(su.UsersId) FROM StoreUser su JOIN Stores s ON s.Id = su.StoresId WHERE s.Deleted = 0 GROUP BY su.StoresId
) o
JOIN Users u ON u.Id = o.UserId AND u.Deleted = 0;

UPDATE u SET u.IsProvider = 1 FROM Users u JOIN Providers p ON p.UserId = u.Id AND p.Deleted = 0;

UPDATE c SET c.ProviderId = p.Id FROM Companions c JOIN Providers p ON p.UserId = c.OwnerId AND p.Deleted = 0 WHERE c.Deleted = 0;

UPDATE s SET s.ProviderId = p.Id
FROM Stores s
JOIN (SELECT StoresId, MIN(UsersId) AS OwnerUserId FROM StoreUser GROUP BY StoresId) so ON so.StoresId = s.Id
JOIN Providers p ON p.UserId = so.OwnerUserId AND p.Deleted = 0
WHERE s.Deleted = 0;

-- تخصص‌ها: فروشنده (دارای فروشگاه) و پانسیون‌دار (دارای پانسیون)؛ تایید = تایید فروشگاه/پانسیون فعلی
INSERT INTO ProviderCapabilities (ProviderId, Type, Approved, Deleted)
SELECT s.ProviderId, 1, MAX(CAST(s.Approved AS int)), 0 FROM Stores s WHERE s.ProviderId IS NOT NULL AND s.Deleted = 0 GROUP BY s.ProviderId;
INSERT INTO ProviderCapabilities (ProviderId, Type, Approved, Deleted)
SELECT c.ProviderId, 5, MAX(CAST(pa.Approve AS int)), 0
FROM Pansions pa JOIN Companions c ON c.Id = pa.CompanionId
WHERE c.ProviderId IS NOT NULL AND pa.Deleted = 0 GROUP BY c.ProviderId;

-- اشتراک قدیمی (طلایی/نقره‌ای روی Companion) → اشتراک مالک؛ بالاترین سطحِ هنوز-معتبر
INSERT INTO ProviderSubscriptions (ProviderId, PlanId, StartDate, EndDate, CreatedByUserId, CreateDate, Note, Deleted)
SELECT x.ProviderId, (SELECT TOP 1 Id FROM SubscriptionPlans WHERE Level = x.Level), GETDATE(), x.EndDate, NULL, GETDATE(), N'Migrated from legacy Companion account', 0
FROM (
    SELECT c.ProviderId,
           CASE WHEN MAX(c.GoldAccountDate) > GETDATE() THEN 2 ELSE 1 END AS Level,
           CASE WHEN MAX(c.GoldAccountDate) > GETDATE() THEN MAX(c.GoldAccountDate) ELSE MAX(c.SilverAccountDate) END AS EndDate
    FROM Companions c
    WHERE c.ProviderId IS NOT NULL AND c.Deleted = 0
      AND (c.GoldAccountDate > GETDATE() OR c.SilverAccountDate > GETDATE())
    GROUP BY c.ProviderId
) x;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companions_Providers_ProviderId",
                table: "Companions");

            migrationBuilder.DropForeignKey(
                name: "FK_Stores_Providers_ProviderId",
                table: "Stores");

            migrationBuilder.DropTable(
                name: "ProviderCapabilities");

            migrationBuilder.DropTable(
                name: "ProviderSubscriptions");

            migrationBuilder.DropTable(
                name: "Providers");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_Stores_ProviderId",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Companions_ProviderId",
                table: "Companions");

            migrationBuilder.DropColumn(
                name: "IsProvider",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProviderId",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "ProviderId",
                table: "Companions");
        }
    }
}
