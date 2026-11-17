using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    // یادآوری فروشنده در موعد ارسال سفارش بازه‌ای (پیامک + پوش): پوش‌های ۱۷۹/۱۸۰ و پیامک‌های ProductOrderReadyNowStore/ProductOrderReadyUrgentStore.
    // فقط داده (بدون تغییر ساختار). هم‌ارز backend/scripts/SellerSlotStartReminders.sql.
    [DbContext(typeof(DataBaseContext))]
    [Migration("20261115150000_SellerSlotStartReminders")]
    public partial class SellerSlotStartReminders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 179 AND Label <> N'PushShipmentReadyNow')
                    THROW 51000, 'PushType ID 179 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 180 AND Label <> N'PushShipmentReadyUrgent')
                    THROW 51000, 'PushType ID 180 is already assigned to another label.', 1;

                SET IDENTITY_INSERT PushTypes ON;
                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 179)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (179, N'موعد ارسال سفارش رسید (به فروشنده)', N'PushShipmentReadyNow');
                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 180)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (180, N'یادآوری فوری «آماده تحویل به پیک» (به فروشنده)', N'PushShipmentReadyUrgent');
                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 179)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (179, N'موعد ارسال سفارش رسید', N'موعد ارسال سفارش {0} فرا رسیده است. آماده‌سازی کنید و تا ساعت {1} «آماده تحویل به پیک» را بزنید.', N'/sellerProfile/orders', NULL, N'shipment-ready-now', 1);
                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 180)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (180, N'عجله کنید؛ مهلت رو به پایان است', N'سفارش {0} هنوز آماده تحویل به پیک نشده. فقط تا ساعت {1} وقت دارید؛ لطفاً عجله کنید، وگرنه لغو می‌شود.', N'/sellerProfile/orders', NULL, N'shipment-ready-urgent', 1);

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId IN (179, 180)
                  AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);

                DECLARE @t TABLE (Label NVARCHAR(100), Name NVARCHAR(200), Body NVARCHAR(1000));
                INSERT INTO @t VALUES
                (N'ProductOrderReadyNowStore', N'ارسال میاره - موعد ارسال رسید (پیامک به فروشگاه)',
                 N'فروشگاه محترم، موعد ارسال سفارش شماره %token فرا رسیده است.' + CHAR(10) + N'لطفاً سفارش را آماده‌سازی کنید و تا %token2 دکمه «آماده تحویل به پیک» را در پنل فروشنده پاستیل بزنید.' + CHAR(10) + N'پاستیل'),
                (N'ProductOrderReadyUrgentStore', N'ارسال میاره - یادآوری فوری آماده تحویل به پیک (پیامک به فروشگاه)',
                 N'فروشگاه محترم، سفارش شماره %token هنوز برای تحویل به پیک آماده نشده است.' + CHAR(10) + N'فقط تا %token2 وقت دارید؛ لطفاً عجله کنید و دکمه «آماده تحویل به پیک» را بزنید، در غیر این صورت سفارش لغو می‌شود.' + CHAR(10) + N'پاستیل');

                INSERT INTO MessageTypes (Name, Label, Body, Description, Pattern)
                SELECT t.Name, t.Label, t.Body, t.Name, t.Body
                FROM @t t
                WHERE NOT EXISTS (SELECT 1 FROM MessageTypes m WHERE m.Label = t.Label);

                DECLARE @SmsNumberId bigint = (SELECT TOP 1 s.SmsNumberId FROM SmsSettings s JOIN MessageTypes m ON m.Id = s.SmsTypeId WHERE m.Label = N'ProductOrderSlotStore');
                IF @SmsNumberId IS NULL SET @SmsNumberId = (SELECT TOP 1 Id FROM SmsNumbers ORDER BY Id);
                IF @SmsNumberId IS NULL THROW 51001, 'SmsNumbers is empty', 1;
                INSERT INTO SmsSettings (SmsNumberId, SmsTypeId)
                SELECT @SmsNumberId, m.Id FROM MessageTypes m JOIN @t t ON t.Label = m.Label
                WHERE NOT EXISTS (SELECT 1 FROM SmsSettings s WHERE s.SmsTypeId = m.Id);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE notification FROM PushNotifications notification
                INNER JOIN PushPatterns pattern ON pattern.Id = notification.PushPatternId
                WHERE pattern.PushTypeId IN (179, 180);
                DELETE setting FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId IN (179, 180);
                DELETE FROM PushPatterns WHERE PushTypeId IN (179, 180);
                DELETE FROM PushTypes WHERE Id IN (179, 180);
                DELETE s FROM SmsSettings s INNER JOIN MessageTypes t ON t.Id = s.SmsTypeId
                WHERE t.Label IN (N'ProductOrderReadyNowStore', N'ProductOrderReadyUrgentStore');
                DELETE FROM MessageTypes WHERE Label IN (N'ProductOrderReadyNowStore', N'ProductOrderReadyUrgentStore');
                """);
        }
    }
}
