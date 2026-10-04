using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedProductOrderAutoDeliveryPushTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id IN (88, 89) AND Label NOT IN (N'PushProductOrderAutoDeliveryWarning', N'PushProductOrderAutoDelivered'))
                    THROW 51000, 'PushType IDs 88-89 are already assigned to other labels.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label IN (N'PushProductOrderAutoDeliveryWarning', N'PushProductOrderAutoDelivered') AND Id NOT IN (88, 89))
                    THROW 51000, 'Product order auto-delivery push labels are already assigned to other IDs.', 1;

                SET IDENTITY_INSERT PushTypes ON;

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 88)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (88, N'هشدار تأیید خودکار تحویل سفارش (۲ روز قبل)', N'PushProductOrderAutoDeliveryWarning');
                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 89)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (89, N'تأیید خودکار تحویل سفارش بعد از ۷ روز', N'PushProductOrderAutoDelivered');

                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 88)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (88, N'تأیید تحویل سفارش', N'سفارش {2} را تحویل گرفته‌اید؟ اگر تا {3} در «سفارش‌های من» پاسخ ندهید، به‌صورت خودکار «تحویل گرفته شد» ثبت می‌شود.', N'/orders', NULL, N'product-order-auto-delivery-warning', 1);
                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 89)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (89, N'سفارش شما تحویل‌شده ثبت شد', N'۷ روز از ارسال سفارش {2} گذشت و پاسخی ثبت نشد؛ سفارش به‌صورت خودکار «تحویل داده شد» ثبت شد. اگر بسته را دریافت نکرده‌اید، با پشتیبانی تماس بگیرید.', N'/orders', NULL, N'product-order-auto-delivered', 1);

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId IN (88, 89)
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
                WHERE pattern.PushTypeId IN (88, 89);

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId IN (88, 89);

                DELETE FROM PushPatterns WHERE PushTypeId IN (88, 89);
                DELETE FROM PushTypes
                WHERE Id IN (88, 89)
                  AND Label IN (N'PushProductOrderAutoDeliveryWarning', N'PushProductOrderAutoDelivered');
                """);
        }
    }
}
