namespace Persistence.Migrations
{
    /// <summary>
    /// متن procedure «UpdateProductItemDiscount» بعد از معرفی تخفیفِ دو‌لایه (سهم فروشگاه + سهم پاستیل).
    /// نسخه‌ی قبلی فقط «بیشترین» تخفیف را می‌گرفت؛ حالا بیشترین تخفیفِ فروشگاه و بیشترین تخفیفِ پاستیل جدا انتخاب می‌شوند:
    /// قیمت نهایی = پایه × (۱ − فروشگاه٪) × (۱ − پاستیل٪) (پاستیل روی قیمتِ بعد از تخفیف فروشگاه). سهم پاستیل در
    /// ProductItems.PastilDiscountPercent و مبلغش (به‌ازای هر واحد) در PastilDiscountAmount ثبت می‌شود.
    /// بدون ردیف تخفیفِ پاستیل رفتار دقیقاً مثل قبل است. (همین متن در scripts/PastilFundedDiscount.sql هم هست.)
    /// </summary>
    public static class PastilDiscountProcedure
    {
        public const string Sql = @"
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
END;";

        /// <summary>نسخه‌ی قبلی procedure (فقط برای Down).</summary>
        public const string PreviousSql = @"
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
END;";
    }
}
