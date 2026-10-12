using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedExpressShippingPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @Types TABLE (Id bigint, Name nvarchar(200), Label nvarchar(200), Title nvarchar(200), Body nvarchar(500), Url nvarchar(200), Tag nvarchar(200));
                INSERT INTO @Types (Id, Name, Label, Title, Body, Url, Tag) VALUES
                    (121, N'ارسال اکسپرس: همین الان پیک بگیرید (به فروشگاه)', N'PushShipmentExpressAwaitingSeller', N'سفارش اکسپرس', N'سفارش اکسپرس {0} ثبت شد؛ همین الان کالا را آماده و «درخواست پیک» را بزنید (تا ساعت {1}).', N'/sellerProfile/orders', N'shipment-express-seller'),
                    (122, N'ارسال اکسپرس: سفارش ثبت شد، ۲ تا ۳ ساعت کاری (به مشتری)', N'PushOrderExpressUser', N'سفارش اکسپرس شما ثبت شد', N'سفارش اکسپرس {0} ثبت شد؛ ظرف {1} تحویل می‌شود.', N'/orders/{0}', N'order-express-user');

                IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Id = s.Id WHERE t.Label <> s.Label)
                    THROW 51000, 'An express shipping push type ID is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Label = s.Label WHERE t.Id <> s.Id)
                    THROW 51000, 'An express shipping push label is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;
                INSERT INTO PushTypes (Id, Name, Label)
                SELECT s.Id, s.Name, s.Label FROM @Types s
                WHERE NOT EXISTS (SELECT 1 FROM PushTypes t WHERE t.Id = s.Id);
                SET IDENTITY_INSERT PushTypes OFF;

                INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                SELECT s.Id, s.Title, s.Body, s.Url, NULL, s.Tag, 1 FROM @Types s
                WHERE NOT EXISTS (SELECT 1 FROM PushPatterns p WHERE p.PushTypeId = s.Id);

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId IN (121, 122)
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
                WHERE pattern.PushTypeId IN (121, 122);

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId IN (121, 122);

                DELETE FROM PushPatterns WHERE PushTypeId IN (121, 122);
                DELETE FROM PushTypes
                WHERE (Id = 121 AND Label = N'PushShipmentExpressAwaitingSeller')
                   OR (Id = 122 AND Label = N'PushOrderExpressUser');
                """);
        }
    }
}
