using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedProductOrderNotReceivedStorePushType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 90 AND Label <> N'PushProductOrderNotReceivedStore')
                    THROW 51000, 'PushType ID 90 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushProductOrderNotReceivedStore' AND Id <> 90)
                    THROW 51000, 'PushProductOrderNotReceivedStore is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 90)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (90, N'مشتری سفارش را تحویل نگرفته (اعلان به فروشگاه)', N'PushProductOrderNotReceivedStore');

                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 90)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (90, N'مشتری سفارش را تحویل نگرفته', N'مشتری ({0}) سفارش {2} را تحویل نگرفته است. لطفاً با مشتری تماس بگیرید و پیگیری کنید.', N'/sellerProfile/orders', NULL, N'product-order-not-received-store', 1);

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId = 90
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM PushSettings setting
                      WHERE setting.PushPatternId = pattern.Id
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE notification
                FROM PushNotifications notification
                INNER JOIN PushPatterns pattern ON pattern.Id = notification.PushPatternId
                WHERE pattern.PushTypeId = 90;

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId = 90;

                DELETE FROM PushPatterns WHERE PushTypeId = 90;
                DELETE FROM PushTypes
                WHERE Id = 90
                  AND Label = N'PushProductOrderNotReceivedStore';
                """);
        }
    }
}
