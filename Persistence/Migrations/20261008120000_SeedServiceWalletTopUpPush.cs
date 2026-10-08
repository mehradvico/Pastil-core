using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    // فقط seed داده (بدون تغییر ساختار)؛ به همین دلیل Designer/تغییر Snapshot ندارد.
    [DbContext(typeof(DataBaseContext))]
    [Migration("20261008120000_SeedServiceWalletTopUpPush")]
    public partial class SeedServiceWalletTopUpPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 118 AND Label <> N'PushTripServiceWalletTopUp')
                    THROW 51000, 'PushType ID 118 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushTripServiceWalletTopUp' AND Id <> 118)
                    THROW 51000, 'PushTripServiceWalletTopUp is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 118)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (118, N'کمبود موجودی کیف پول برای سفرهای سرویس هفتگی پت‌رسان (به کاربر)', N'PushTripServiceWalletTopUp');

                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 118)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (118, N'کیف پول را شارژ کنید', N'برای کسر خودکار سفرهای سرویس هفتگی پت‌رسان، حدود {0} تومان به کیف پول شما کم است. لطفاً کیف پول را شارژ کنید.', N'/wallet', NULL, N'trip-service-wallet-topup', 1);

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId = 118
                  AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE notification
                FROM PushNotifications notification
                INNER JOIN PushPatterns pattern ON pattern.Id = notification.PushPatternId
                WHERE pattern.PushTypeId = 118;

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId = 118;

                DELETE FROM PushPatterns WHERE PushTypeId = 118;
                DELETE FROM PushTypes WHERE Id = 118 AND Label = N'PushTripServiceWalletTopUp';
                """);
        }
    }
}
