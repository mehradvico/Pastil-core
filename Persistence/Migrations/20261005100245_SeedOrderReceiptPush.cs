using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedOrderReceiptPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 104 AND Label <> N'PushOrderAskReceived')
                    THROW 51000, 'PushType ID 104 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushOrderAskReceived' AND Id <> 104)
                    THROW 51000, 'PushOrderAskReceived is already assigned to another ID.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 105 AND Label <> N'PushOrderReceivedStore')
                    THROW 51000, 'PushType ID 105 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushOrderReceivedStore' AND Id <> 105)
                    THROW 51000, 'PushOrderReceivedStore is already assigned to another ID.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 106 AND Label <> N'PushOrderReceivedAdmin')
                    THROW 51000, 'PushType ID 106 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushOrderReceivedAdmin' AND Id <> 106)
                    THROW 51000, 'PushOrderReceivedAdmin is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 104)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (104, N'آیا سفارش را تحویل گرفتید؟ (به مشتری، راس پایان بازه)', N'PushOrderAskReceived');
                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 105)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (105, N'مشتری سفارش را تحویل گرفت (به فروشنده)', N'PushOrderReceivedStore');
                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 106)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (106, N'مشتری سفارش را تحویل گرفت (به ادمین)', N'PushOrderReceivedAdmin');

                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 104)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (104, N'فروشگاه پاستیل', N'آیا سفارش {0} را تحویل گرفتید؟', N'/orders/{0}', NULL, N'order-ask-received', 1);
                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 105)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (105, N'سفارش تحویل داده شد', N'سفارش {0} با موفقیت به کاربر تحویل داده شد.', N'/sellerProfile/orders', NULL, N'order-received-store', 1);
                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 106)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (106, N'سفارش تحویل داده شد', N'سفارش {0} با موفقیت به کاربر تحویل داده شد.', N'/', NULL, N'order-received-admin', 1);

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId IN (104, 105, 106)
                  AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);

                -- متن پوش ادمین «مشتری تحویل نگرفتم زد» عوض شد
                UPDATE PushPatterns
                SET Title = N'سفارش به کاربر نرسیده است', Body = N'سفارش {0} در ساعت مقرر به کاربر نرسیده است.'
                WHERE PushTypeId = 102;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE notification
                FROM PushNotifications notification
                INNER JOIN PushPatterns pattern ON pattern.Id = notification.PushPatternId
                WHERE pattern.PushTypeId IN (104, 105, 106);

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId IN (104, 105, 106);

                DELETE FROM PushPatterns WHERE PushTypeId IN (104, 105, 106);
                DELETE FROM PushTypes
                WHERE (Id = 104 AND Label = N'PushOrderAskReceived')
                   OR (Id = 105 AND Label = N'PushOrderReceivedStore')
                   OR (Id = 106 AND Label = N'PushOrderReceivedAdmin');
                """);
        }
    }
}
