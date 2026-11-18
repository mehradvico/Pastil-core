-- Consolidate the two obsolete جوندگان roots into جونده (Id 26).
-- Safe by default: set @Apply=1 only after reviewing a snapshot.
SET NOCOUNT ON;
SET XACT_ABORT ON;
DECLARE @Apply bit = 0;
BEGIN TRANSACTION;
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Id=26 AND Name=N'جونده' AND Deleted=0)
        THROW 51000, 'Target category changed', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Id=10109 AND Name=N'جوندگان' AND Deleted=0)
        THROW 51001, 'Source category changed', 1;

    CREATE TABLE #Map (SourceId bigint PRIMARY KEY, TargetId bigint NOT NULL);
    INSERT #Map VALUES (10069,26),(10109,26),(10070,10110),(10077,10111),(10117,10124);

    -- Snapshot category links before changing their identifiers. All inserts avoid duplicates.
    INSERT dbo.CategoryProduct (CategoriesId,ProductsId)
    SELECT DISTINCT m.TargetId,cp.ProductsId FROM dbo.CategoryProduct cp
    JOIN #Map m ON m.SourceId=cp.CategoriesId
    WHERE NOT EXISTS (SELECT 1 FROM dbo.CategoryProduct existing WHERE existing.CategoriesId=m.TargetId AND existing.ProductsId=cp.ProductsId);
    DELETE cp FROM dbo.CategoryProduct cp JOIN #Map m ON m.SourceId=cp.CategoriesId;

    INSERT dbo.CategoryFeature (CategoriesId,FeaturesId)
    SELECT DISTINCT m.TargetId,cf.FeaturesId FROM dbo.CategoryFeature cf
    JOIN #Map m ON m.SourceId=cf.CategoriesId
    WHERE NOT EXISTS (SELECT 1 FROM dbo.CategoryFeature existing WHERE existing.CategoriesId=m.TargetId AND existing.FeaturesId=cf.FeaturesId);
    DELETE cf FROM dbo.CategoryFeature cf JOIN #Map m ON m.SourceId=cf.CategoriesId;

    UPDATE p SET CategoryId=m.TargetId
    FROM dbo.Products p JOIN #Map m ON m.SourceId=p.CategoryId;

    -- Keep unique sibling slugs: merge identical leaves before moving the survivors.
    UPDATE dbo.Categories SET Deleted=1,Active=0,UpdateDate=GETDATE()
    WHERE Id IN (10070,10077,10117);
    UPDATE dbo.Categories SET ParentId=26,UpdateDate=GETDATE()
    WHERE Id IN (10070,10073,10077,10078,10110,10111,10114,10117);
    UPDATE dbo.Categories SET Deleted=1,Active=0,UpdateDate=GETDATE()
    WHERE Id IN (10069,10109);

    IF EXISTS (SELECT 1 FROM dbo.Products WHERE CategoryId IN (10069,10109,10070,10077,10117))
       OR EXISTS (SELECT 1 FROM dbo.CategoryProduct WHERE CategoriesId IN (10069,10109,10070,10077,10117))
       OR EXISTS (SELECT 1 FROM dbo.CategoryFeature WHERE CategoriesId IN (10069,10109,10070,10077,10117))
       OR EXISTS (SELECT 1 FROM dbo.Categories WHERE ParentId IN (10069,10109))
       OR EXISTS (SELECT 1 FROM dbo.Categories WHERE Id IN (10069,10109) AND Deleted=0)
        THROW 51002, 'Obsolete category references remain', 1;

    SELECT Id,Name,ParentId,Active,Deleted FROM dbo.Categories
    WHERE Id IN (26,10069,10109,10070,10077,10117,10073,10078,10110,10111,10114,10124)
    ORDER BY Id;
    SELECT CategoryId,COUNT(*) ProductCount FROM dbo.Products
    WHERE CategoryId IN (26,10069,10109,10070,10077,10117,10073,10078,10110,10111,10114,10124)
    GROUP BY CategoryId ORDER BY CategoryId;
    IF @Apply=1 COMMIT; ELSE ROLLBACK;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK;
    THROW;
END CATCH;
