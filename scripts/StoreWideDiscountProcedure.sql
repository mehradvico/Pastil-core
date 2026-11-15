-- تخفیفِ «کل فروشگاه» در procedure اعمال نمی‌شد (۲۰۲۶-۱۱-۱۴)
-- علت: join تخفیف‌ها فقط روی ProductItemId/ProductId/CategoryId/BrandId بود؛ تخفیفِ نوع Store هیچ‌کدام را ندارد پس به هیچ کالایی نمی‌رسید.
-- اصلاح: ردیف تخفیفِ بدون هدف به همه‌ی کالاهای همان فروشگاه (discount.StoreId = item.StoreId) وصل می‌شود.
-- بی‌خطر و قابل تکرار (CREATE OR ALTER). بعد از اجرا باید یک‌بار برای فروشگاه‌هایی که تخفیف کل‌فروشگاه دارند اجرا شود (پایین).
-- نیازی به migration نیست.

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
                -- تخفیفِ «کل فروشگاه»: هیچ هدفی (محصول/تنوع/دسته/برند) ندارد و روی همه‌ی کالاهای همان فروشگاه اعمال می‌شود
                OR (discount.ProductItemId IS NULL AND discount.ProductId IS NULL AND discount.CategoryId IS NULL AND discount.BrandId IS NULL)
            )
            AND discount.Active = 1
            AND discount.Deleted = 0
            AND (discount.EndDate IS NULL OR CAST(discount.EndDate AS date) >= CAST(GETDATE() AS date))
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
    SET DiscountPercent = CAST(ROUND(CASE WHEN calc.StorePercent + calc.PastilPercent > 100 THEN 100.0 ELSE calc.StorePercent + calc.PastilPercent END, 0) AS int),
        PastilDiscountPercent = calc.PastilPercent,
        PastilDiscountAmount =
            CASE WHEN calc.PastilPercent > 0
                 THEN CAST(ISNULL(item.BasePrice, 0) * (1.0 - calc.StorePercent / 100.0) AS float)
                      - CAST(CAST(ISNULL(item.BasePrice, 0) * (1.0 - (CASE WHEN calc.StorePercent + calc.PastilPercent > 100 THEN 100.0 ELSE calc.StorePercent + calc.PastilPercent END) / 100.0) AS bigint) AS float)
                 ELSE 0 END,
        Price = CAST(ISNULL(item.BasePrice, 0) * (1.0 - (CASE WHEN calc.StorePercent + calc.PastilPercent > 100 THEN 100.0 ELSE calc.StorePercent + calc.PastilPercent END) / 100.0) AS bigint),
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
-- بازمحاسبه‌ی قیمت برای همه‌ی فروشگاه‌هایی که تخفیف فعالِ کل‌فروشگاه دارند
DECLARE @ids nvarchar(max) = (
    SELECT STRING_AGG(CAST(StoreId AS nvarchar(20)), N',') FROM (
        SELECT DISTINCT d.StoreId FROM dbo.Discounts d
        WHERE d.Active = 1 AND d.Deleted = 0 AND d.ProductItemId IS NULL AND d.ProductId IS NULL AND d.CategoryId IS NULL AND d.BrandId IS NULL
          AND (d.EndDate IS NULL OR CAST(d.EndDate AS date) >= CAST(GETDATE() AS date))
    ) x);
SELECT @ids AS StoresToRecompute;
IF @ids IS NOT NULL EXEC dbo.UpdateProductItemDiscount @FilterType = N'Store', @FilterIds = @ids;
