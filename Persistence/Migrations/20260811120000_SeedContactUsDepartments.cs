using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    [DbContext(typeof(DataBaseContext))]
    [Migration("20260811120000_SeedContactUsDepartments")]
    public partial class SeedContactUsDepartments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                SET NOCOUNT ON;

                DECLARE @Roles nvarchar(max) = N'Customer,Admin,Companion,Operator,Store';

                IF NOT EXISTS (SELECT 1 FROM [ContactUsGroups] WHERE [Label] = N'contact-pastil')
                    INSERT INTO [ContactUsGroups] ([Name], [Label], [Active], [Priority], [Roles])
                    VALUES (N'ارتباط با پاستیل', N'contact-pastil', 1, 1, @Roles);

                IF NOT EXISTS (SELECT 1 FROM [ContactUsGroups] WHERE [Label] = N'driver-request')
                    INSERT INTO [ContactUsGroups] ([Name], [Label], [Active], [Priority], [Roles])
                    VALUES (N'درخواست راننده', N'driver-request', 1, 2, @Roles);

                IF NOT EXISTS (SELECT 1 FROM [ContactUsGroups] WHERE [Label] = N'companion-request')
                    INSERT INTO [ContactUsGroups] ([Name], [Label], [Active], [Priority], [Roles])
                    VALUES (N'درخواست نمایندگی', N'companion-request', 1, 3, @Roles);

                IF NOT EXISTS (SELECT 1 FROM [ContactUsGroups] WHERE [Label] = N'pet-shop-request')
                    INSERT INTO [ContactUsGroups] ([Name], [Label], [Active], [Priority], [Roles])
                    VALUES (N'درخواست پت شاپ', N'pet-shop-request', 1, 4, @Roles);

                IF NOT EXISTS (SELECT 1 FROM [ContactUsGroups] WHERE [Label] = N'special-product-request')
                    INSERT INTO [ContactUsGroups] ([Name], [Label], [Active], [Priority], [Roles])
                    VALUES (N'درخواست محصول خاص', N'special-product-request', 1, 5, @Roles);

                IF NOT EXISTS (SELECT 1 FROM [ContactUsGroups] WHERE [Label] = N'advertising-request')
                    INSERT INTO [ContactUsGroups] ([Name], [Label], [Active], [Priority], [Roles])
                    VALUES (N'درخواست تبلیغات', N'advertising-request', 1, 6, @Roles);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM [ContactUsGroups]
                WHERE [Label] IN
                (
                    N'contact-pastil',
                    N'driver-request',
                    N'companion-request',
                    N'pet-shop-request',
                    N'special-product-request',
                    N'advertising-request'
                )
                AND NOT EXISTS
                (
                    SELECT 1
                    FROM [ContactUses]
                    WHERE [ContactUses].[ContactUsGroupId] = [ContactUsGroups].[Id]
                );
                """);
        }
    }
}
