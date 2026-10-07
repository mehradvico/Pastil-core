using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedOrderAdjustPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 107 AND Label <> N'PushOrderCancelledByAdminStore')
                    THROW 51000, 'PushType ID 107 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushOrderCancelledByAdminStore' AND Id <> 107)
                    THROW 51000, 'PushOrderCancelledByAdminStore is already assigned to another ID.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 108 AND Label <> N'PushOrderCancelledByStoreAdmin')
                    THROW 51000, 'PushType ID 108 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushOrderCancelledByStoreAdmin' AND Id <> 108)
                    THROW 51000, 'PushOrderCancelledByStoreAdmin is already assigned to another ID.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 109 AND Label <> N'PushOrderChangedByAdminStore')
                    THROW 51000, 'PushType ID 109 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushOrderChangedByAdminStore' AND Id <> 109)
                    THROW 51000, 'PushOrderChangedByAdminStore is already assigned to another ID.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 110 AND Label <> N'PushOrderChangedByStoreAdmin')
                    THROW 51000, 'PushType ID 110 is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = N'PushOrderChangedByStoreAdmin' AND Id <> 110)
                    THROW 51000, 'PushOrderChangedByStoreAdmin is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;

                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 107)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (107, N'سفارش توسط پاستیل لغو شد (به فروشنده)', N'PushOrderCancelledByAdminStore');
                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 108)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (108, N'سفارش توسط فروشگاه لغو شد (به ادمین)', N'PushOrderCancelledByStoreAdmin');
                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 109)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (109, N'سفارش توسط پاستیل تغییر کرد (به فروشنده)', N'PushOrderChangedByAdminStore');
                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 110)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (110, N'فروشنده سفارش را تغییر داد (به ادمین)', N'PushOrderChangedByStoreAdmin');

                SET IDENTITY_INSERT PushTypes OFF;

                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 107)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (107, N'سفارش لغو شد', N'سفارش {0} توسط پاستیل لغو شد.', N'/sellerProfile/orders', NULL, N'order-cancel-admin-store', 1);
                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 108)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (108, N'سفارش توسط فروشگاه لغو شد', N'سفارش {0} توسط فروشگاه {1} لغو شد.', N'/', NULL, N'order-cancel-store-admin', 1);
                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 109)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (109, N'سفارش تغییر کرد', N'سفارش {0} توسط پاستیل تغییر کرد.', N'/sellerProfile/orders', NULL, N'order-changed-admin-store', 1);
                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 110)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (110, N'سفارش توسط فروشنده تغییر کرد', N'فروشنده سفارش {0} را تغییر داد.', N'/', NULL, N'order-changed-store-admin', 1);

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId IN (107, 108, 109, 110)
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
                WHERE pattern.PushTypeId IN (107, 108, 109, 110);

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId IN (107, 108, 109, 110);

                DELETE FROM PushPatterns WHERE PushTypeId IN (107, 108, 109, 110);
                DELETE FROM PushTypes
                WHERE (Id = 107 AND Label = N'PushOrderCancelledByAdminStore')
                   OR (Id = 108 AND Label = N'PushOrderCancelledByStoreAdmin')
                   OR (Id = 109 AND Label = N'PushOrderChangedByAdminStore')
                   OR (Id = 110 AND Label = N'PushOrderChangedByStoreAdmin');
                """);
        }
    }
}
