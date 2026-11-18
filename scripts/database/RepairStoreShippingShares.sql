-- 2026-11-18. Default is PREVIEW ONLY. No schema changes.
-- Apply only after reviewing the preview, backing up, and explicit approval.
-- Deploy corrected Api + Payment together. Settled / shared / cancelled orders are excluded.
SET NOCOUNT ON;
SET XACT_ABORT ON;
DECLARE @Apply bit = 0;
DECLARE @ExpectedCount int = NULL;
DECLARE @ExpectedShift decimal(28,6) = NULL;
DROP TABLE IF EXISTS #Candidates;

BEGIN TRANSACTION;
BEGIN TRY
    ;WITH StoreAmounts AS
    (
        SELECT os.ProductOrderId, COUNT(*) AS StoreCount,
               SUM(CAST(os.PaymentPrice AS decimal(28,6))) AS Payment,
               SUM(CAST(os.DeliveryPrice AS decimal(28,6))) AS Delivery,
               SUM(COALESCE(f.Funded, 0)) AS Funded,
               SUM((CAST(os.PaymentPrice AS decimal(28,6)) + COALESCE(f.Funded,0))
                   * (100 - s.CommissionPercent) / 100) AS OldStore,
               SUM((CAST(os.PaymentPrice AS decimal(28,6)) - CAST(os.DeliveryPrice AS decimal(28,6)) + COALESCE(f.Funded,0))
                   * (100 - s.CommissionPercent) / 100) AS NewStore,
               MIN(s.CommissionPercent) AS MinPercent, MAX(s.CommissionPercent) AS MaxPercent
        FROM dbo.ProductOrderStores os
        JOIN dbo.Stores s ON s.Id = os.StoreId
        OUTER APPLY (
            SELECT SUM(CAST(i.PastilDiscountUnit AS decimal(28,6)) * i.Count) AS Funded
            FROM dbo.ProductOrderItems i
            WHERE i.ProductOrderStoreId = os.Id AND i.Deleted = 0 AND i.Count > 0 AND i.PastilDiscountUnit > 0
        ) f
        WHERE os.Deleted = 0
        GROUP BY os.ProductOrderId
    )
    SELECT o.Id, o.OrderCode,
           o.StoreShare AS PreviousStoreShare, o.SiteShare AS PreviousSiteShare,
           a.NewStore AS NewStoreShare, a.Payment - a.NewStore AS NewSiteShare,
           a.Delivery AS DeliveryPrice,
           CAST(a.OldStore - a.NewStore AS decimal(28,6)) AS MovedFromStoreToPastil
    INTO #Candidates
    FROM dbo.ProductOrders o WITH (UPDLOCK, HOLDLOCK)
    JOIN StoreAmounts a ON a.ProductOrderId = o.Id
    JOIN dbo.Codes state ON state.Id = o.ProductOrderStateId
    WHERE o.Deleted = 0 AND o.IsPaid = 1 AND o.Permitted = 0
      AND state.Label <> N'ProductOrderState_Canceled'
      AND NOT EXISTS (SELECT 1 FROM dbo.SettlementStores ss WHERE ss.ProductOrderId = o.Id)
      AND a.StoreCount = 1 AND a.Delivery > 0 AND a.MinPercent >= 0 AND a.MaxPercent <= 100
      -- No guessing a historical commission: only rows exactly matching the old rule qualify.
      AND ABS(o.StoreShare - a.OldStore) < 0.01
      AND ABS(o.SiteShare - (a.Payment - a.OldStore)) < 0.01
      AND ABS(o.PastilFundedAmount - a.Funded) < 0.01
      -- Already corrected rows do not qualify: reruns are idempotent.
      AND ABS(o.StoreShare - a.NewStore) >= 0.01;

    SELECT COUNT(*) AS CandidateCount, COALESCE(SUM(MovedFromStoreToPastil),0) AS TotalMovedFromStoreToPastil
    FROM #Candidates;
    SELECT * FROM #Candidates ORDER BY Id;

    IF @Apply = 1
    BEGIN
        IF @ExpectedCount IS NOT NULL AND @ExpectedCount <> (SELECT COUNT(*) FROM #Candidates)
            THROW 51000, 'Candidate count changed since review; no shares updated.', 1;
        IF @ExpectedShift IS NOT NULL AND ABS(@ExpectedShift - COALESCE((SELECT SUM(MovedFromStoreToPastil) FROM #Candidates),0)) >= 0.01
            THROW 51001, 'Candidate total changed since review; no shares updated.', 1;
        UPDATE o SET StoreShare = c.NewStoreShare, SiteShare = c.NewSiteShare
        OUTPUT inserted.Id, deleted.StoreShare AS PreviousStoreShare, inserted.StoreShare AS NewStoreShare,
               deleted.SiteShare AS PreviousSiteShare, inserted.SiteShare AS NewSiteShare
        FROM dbo.ProductOrders o JOIN #Candidates c ON c.Id = o.Id;
        COMMIT;
    END
    ELSE
        ROLLBACK;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    THROW;
END CATCH;
