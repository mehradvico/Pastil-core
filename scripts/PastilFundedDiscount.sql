-- تخفیف محصولِ دو‌لایه: سهم فروشگاه + سهم پاستیل (۲۰۲۶-۱۱-۱۳) — معادل migration PastilFundedDiscount
-- چند بار اجرا شدن امن است. ترتیب: اول این اسکریپت، بعد دیپلوی Api (Api جدید به ستون‌ها نیاز دارد).
--  • Discounts.FundedByPastil            : ردیف تخفیف «سهم پاستیل» (فقط ادمین می‌سازد)
--  • ProductItems.PastilDiscountPercent  : درصد تخفیف پاستیل (روی قیمتِ بعد از تخفیف فروشگاه)
--  • ProductItems.PastilDiscountAmount   : مبلغ تخفیف پاستیل به‌ازای هر واحد
--  • ProductOrderItems.PastilDiscountUnit: تخفیف پاستیلِ هر واحد هنگام ثبت سفارش (snapshot)
--  • ProductOrders.PastilFundedAmount    : جمع هزینه‌ی تخفیف پاستیل در سفارش (حسابداری)
--  • procedure UpdateProductItemDiscount : قیمت = پایه × (۱ − فروشگاه٪) × (۱ − پاستیل٪)؛ هر لایه بیشترین ردیف فعال خودش
-- بدون ردیفِ FundedByPastil=1 قیمت‌ها و رفتار دقیقاً مثل قبل است.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
SET LOCK_TIMEOUT 15000;
SET XACT_ABORT ON;
BEGIN TRAN;

IF COL_LENGTH('Discounts', 'FundedByPastil') IS NULL
    ALTER TABLE [Discounts] ADD [FundedByPastil] bit NOT NULL CONSTRAINT [DF_Discounts_FundedByPastil] DEFAULT 0;
IF COL_LENGTH('ProductItems', 'PastilDiscountPercent') IS NULL
    ALTER TABLE [ProductItems] ADD [PastilDiscountPercent] int NOT NULL CONSTRAINT [DF_ProductItems_PastilDiscountPercent] DEFAULT 0;
IF COL_LENGTH('ProductItems', 'PastilDiscountAmount') IS NULL
    ALTER TABLE [ProductItems] ADD [PastilDiscountAmount] float NOT NULL CONSTRAINT [DF_ProductItems_PastilDiscountAmount] DEFAULT 0;
IF COL_LENGTH('ProductOrderItems', 'PastilDiscountUnit') IS NULL
    ALTER TABLE [ProductOrderItems] ADD [PastilDiscountUnit] float NOT NULL CONSTRAINT [DF_ProductOrderItems_PastilDiscountUnit] DEFAULT 0;
IF COL_LENGTH('ProductOrders', 'PastilFundedAmount') IS NULL
    ALTER TABLE [ProductOrders] ADD [PastilFundedAmount] float NOT NULL CONSTRAINT [DF_ProductOrders_PastilFundedAmount] DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261113120000_PastilFundedDiscount')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20261113120000_PastilFundedDiscount', N'9.0.0');

COMMIT;
GO
-- procedure (باید در batch جدا باشد)
CREATE OR ALTER PROCEDURE [dbo].[UpdateProductItemDiscount]
    @FilterType nvarchar(50),
    @FilterIds nvarchar(max)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdTable TABLE (Id bigint PRIMARY KEY);
    INSERT INTO @IdTable (Id)
    SELECT DISTINCT TRY_CAST(value AS bigint)
    FROM STRING_SPLIT(@FilterIds, N',')
    WHERE TRY_CAST(value AS bigint) IS NOT NULL;

    ;WITH CategoryCTE AS
    (
        SELECT Id
        FROM Categories
        WHERE Id IN (SELECT Id FROM @IdTable)

        UNION ALL

        SELECT c.Id
        FROM Categories AS c
        INNER JOIN CategoryCTE AS parent ON c.ParentId = parent.Id
    ),
    Ranked AS
    (
        SELECT
            item.Id AS ProductItemId,
            discount.[Percent],
            discount.EndDate,
            discount.DiscountGroupId,
            CAST(ISNULL(discount.FundedByPastil, 0) AS bit) AS FundedByPastil,
            ROW_NUMBER() OVER
            (
                PARTITION BY item.Id, ISNULL(discount.FundedByPastil, 0)
                ORDER BY discount.[Percent] DESC, discount.Id ASC
            ) AS RowNumber
        FROM ProductItems AS item
        INNER JOIN Products AS product ON item.ProductId = product.Id
        LEFT JOIN CategoryProduct AS cp ON cp.ProductsId = product.Id
        LEFT JOIN Discounts AS discount
            ON
            (
                item.Id = discount.ProductItemId
                OR product.Id = discount.ProductId
                OR cp.CategoriesId = discount.CategoryId
                OR product.BrandId = discount.BrandId
            )
            AND discount.Active = 1
            AND discount.Deleted = 0
            AND CAST(discount.EndDate AS date) >= CAST(GETDATE() AS date)
            AND discount.StoreId = item.StoreId
        WHERE item.Active = 1
          AND
          (
              (@FilterType = N'Product' AND product.Id IN (SELECT Id FROM @IdTable))
              OR (@FilterType = N'Category' AND product.CategoryId IN (SELECT Id FROM CategoryCTE))
              OR (@FilterType = N'Brand' AND product.BrandId IN (SELECT Id FROM @IdTable))
              OR (@FilterType = N'Store' AND item.StoreId IN (SELECT Id FROM @IdTable))
              OR (@FilterType = N'ProductItem' AND item.Id IN (SELECT Id FROM @IdTable))
              OR (@FilterType = N'DiscountGroup' AND item.DiscountGroupId IN (SELECT Id FROM @IdTable))
          )
    ),
    Best AS
    (
        SELECT
            ProductItemId,
            ISNULL(MAX(CASE WHEN FundedByPastil = 0 THEN [Percent] END), 0) AS StorePercent,
            ISNULL(MAX(CASE WHEN FundedByPastil = 1 THEN [Percent] END), 0) AS PastilPercent,
            MAX(CASE WHEN FundedByPastil = 0 AND [Percent] IS NOT NULL THEN EndDate END) AS StoreEnd,
            MAX(CASE WHEN FundedByPastil = 1 THEN EndDate END) AS PastilEnd,
            MAX(CASE WHEN FundedByPastil = 0 AND [Percent] IS NOT NULL THEN DiscountGroupId END) AS StoreGroup,
            MAX(CASE WHEN FundedByPastil = 1 THEN DiscountGroupId END) AS PastilGroup
        FROM Ranked
        WHERE RowNumber = 1
        GROUP BY ProductItemId
    ),
    Calc AS
    (
        SELECT
            ProductItemId,
            StorePercent,
            PastilPercent,
            CASE
                WHEN StorePercent > 0 AND PastilPercent > 0 AND StoreEnd IS NOT NULL AND PastilEnd IS NOT NULL
                    THEN CASE WHEN StoreEnd < PastilEnd THEN StoreEnd ELSE PastilEnd END
                WHEN StorePercent > 0 THEN StoreEnd
                ELSE PastilEnd
            END AS EndDate,
            CASE WHEN StorePercent > 0 THEN StoreGroup ELSE PastilGroup END AS DiscountGroupId
        FROM Best
    )
    UPDATE item
    SET DiscountPercent = CAST(ROUND(100.0 * (1.0 - (1.0 - calc.StorePercent / 100.0) * (1.0 - calc.PastilPercent / 100.0)), 0) AS int),
        PastilDiscountPercent = calc.PastilPercent,
        PastilDiscountAmount =
            CASE WHEN calc.PastilPercent > 0
                 THEN CAST(ISNULL(item.BasePrice, 0) * (1.0 - calc.StorePercent / 100.0) AS float)
                      - CAST(CAST(ISNULL(item.BasePrice, 0) * (1.0 - calc.StorePercent / 100.0) * (1.0 - calc.PastilPercent / 100.0) AS bigint) AS float)
                 ELSE 0 END,
        Price = CAST(ISNULL(item.BasePrice, 0) * (1.0 - calc.StorePercent / 100.0) * (1.0 - calc.PastilPercent / 100.0) AS bigint),
        DiscountEndDate = calc.EndDate,
        DiscountGroupId = calc.DiscountGroupId
    FROM ProductItems AS item
    INNER JOIN Products AS product ON item.ProductId = product.Id
    INNER JOIN Calc AS calc ON item.Id = calc.ProductItemId
    WHERE
        (@FilterType = N'Product' AND item.ProductId IN (SELECT Id FROM @IdTable))
        OR (@FilterType = N'Category' AND product.CategoryId IN (SELECT Id FROM CategoryCTE))
        OR (@FilterType = N'Brand' AND product.BrandId IN (SELECT Id FROM @IdTable))
        OR (@FilterType = N'Store' AND item.StoreId IN (SELECT Id FROM @IdTable))
        OR (@FilterType = N'ProductItem' AND item.Id IN (SELECT Id FROM @IdTable))
        OR (@FilterType = N'DiscountGroup' AND item.DiscountGroupId IN (SELECT Id FROM @IdTable));

    EXEC [dbo].[UpdateProductPrice]
        @FilterType = @FilterType,
        @FilterIds = @FilterIds;
END;
GO
-- بررسی:
-- SELECT COL_LENGTH('Discounts','FundedByPastil') d, COL_LENGTH('ProductItems','PastilDiscountPercent') i, COL_LENGTH('ProductOrderItems','PastilDiscountUnit') oi, COL_LENGTH('ProductOrders','PastilFundedAmount') o;
-- SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.UpdateProductItemDiscount')) LIKE '%PastilEffective%' AS NewProcedureInstalled;
--
-- ===== بازگشت (فقط اگر لازم شد): ابتدا procedure قبلی، بعد (اختیاری) حذف ستون‌ها =====
-- (ردیف‌های FundedByPastil=1 را قبلش غیرفعال کنید وگرنه در procedure قبلی مثل تخفیف عادی حساب می‌شوند)
/*
CREATE OR ALTER PROCEDURE [dbo].[UpdateProductItemDiscount]
    @FilterType nvarchar(50),
    @FilterIds nvarchar(max)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdTable TABLE (Id bigint PRIMARY KEY);
    INSERT INTO @IdTable (Id)
    SELECT DISTINCT TRY_CAST(value AS bigint)
    FROM STRING_SPLIT(@FilterIds, N',')
    WHERE TRY_CAST(value AS bigint) IS NOT NULL;

    ;WITH CategoryCTE AS
    (
        SELECT Id
        FROM Categories
        WHERE Id IN (SELECT Id FROM @IdTable)

        UNION ALL

        SELECT c.Id
        FROM Categories AS c
        INNER JOIN CategoryCTE AS parent ON c.ParentId = parent.Id
    ),
    MaxDiscounts AS
    (
        SELECT
            item.Id AS ProductItemId,
            discount.[Percent],
            discount.EndDate,
            discount.DiscountGroupId,
            ROW_NUMBER() OVER
            (
                PARTITION BY item.Id
                ORDER BY discount.[Percent] DESC, discount.Id ASC
            ) AS RowNumber
        FROM ProductItems AS item
        INNER JOIN Products AS product ON item.ProductId = product.Id
        LEFT JOIN CategoryProduct AS cp ON cp.ProductsId = product.Id
        LEFT JOIN Discounts AS discount
            ON
            (
                item.Id = discount.ProductItemId
                OR product.Id = discount.ProductId
                OR cp.CategoriesId = discount.CategoryId
                OR product.BrandId = discount.BrandId
            )
            AND discount.Active = 1
            AND discount.Deleted = 0
            AND CAST(discount.EndDate AS date) >= CAST(GETDATE() AS date)
            AND discount.StoreId = item.StoreId
        WHERE item.Active = 1
          AND
          (
              (@FilterType = N'Product' AND product.Id IN (SELECT Id FROM @IdTable))
              OR (@FilterType = N'Category' AND product.CategoryId IN (SELECT Id FROM CategoryCTE))
              OR (@FilterType = N'Brand' AND product.BrandId IN (SELECT Id FROM @IdTable))
              OR (@FilterType = N'Store' AND item.StoreId IN (SELECT Id FROM @IdTable))
              OR (@FilterType = N'ProductItem' AND item.Id IN (SELECT Id FROM @IdTable))
              OR (@FilterType = N'DiscountGroup' AND item.DiscountGroupId IN (SELECT Id FROM @IdTable))
          )
    ),
    FilteredDiscounts AS
    (
        SELECT ProductItemId, [Percent], EndDate, DiscountGroupId
        FROM MaxDiscounts
        WHERE RowNumber = 1
    )
    UPDATE item
    SET DiscountPercent = ISNULL(discount.[Percent], 0),
        Price = item.BasePrice
            - (ISNULL(item.BasePrice, 0) * (ISNULL(discount.[Percent], 0) / 100.0)),
        DiscountEndDate = discount.EndDate,
        DiscountGroupId = discount.DiscountGroupId
    FROM ProductItems AS item
    INNER JOIN Products AS product ON item.ProductId = product.Id
    INNER JOIN FilteredDiscounts AS discount ON item.Id = discount.ProductItemId
    WHERE
        (@FilterType = N'Product' AND item.ProductId IN (SELECT Id FROM @IdTable))
        OR (@FilterType = N'Category' AND product.CategoryId IN (SELECT Id FROM CategoryCTE))
        OR (@FilterType = N'Brand' AND product.BrandId IN (SELECT Id FROM @IdTable))
        OR (@FilterType = N'Store' AND item.StoreId IN (SELECT Id FROM @IdTable))
        OR (@FilterType = N'ProductItem' AND item.Id IN (SELECT Id FROM @IdTable))
        OR (@FilterType = N'DiscountGroup' AND item.DiscountGroupId IN (SELECT Id FROM @IdTable));

    EXEC [dbo].[UpdateProductPrice]
        @FilterType = @FilterType,
        @FilterIds = @FilterIds;
END;
*/
