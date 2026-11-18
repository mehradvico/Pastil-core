-- Read-only aggregate audit. No customer data or mutations.
SET NOCOUNT ON;
;WITH Amounts AS (
    SELECT os.ProductOrderId, COUNT(*) AS StoreCount,
           SUM(os.DeliveryPrice) AS DeliveryPrice,
           SUM((os.PaymentPrice + COALESCE(f.Funded,0)) * (1 - CAST(s.CommissionPercent AS float)/100)) AS OldStore,
           SUM((os.PaymentPrice - os.DeliveryPrice + COALESCE(f.Funded,0)) * (1 - CAST(s.CommissionPercent AS float)/100)) AS NewStore
    FROM dbo.ProductOrderStores os JOIN dbo.Stores s ON s.Id = os.StoreId
    OUTER APPLY (
        SELECT SUM(i.PastilDiscountUnit * i.Count) AS Funded
        FROM dbo.ProductOrderItems i WHERE i.ProductOrderStoreId = os.Id
        AND i.Deleted = 0 AND i.Count > 0 AND i.PastilDiscountUnit > 0
    ) f
    WHERE os.Deleted = 0
    GROUP BY os.ProductOrderId
)
SELECT COUNT(*) AS PaidOrdersWithPrepaidDelivery,
       COALESCE(SUM(CASE WHEN ABS(o.StoreShare - a.OldStore) < 0.01 AND ABS(o.StoreShare - a.NewStore) >= 0.01 THEN 1 ELSE 0 END),0) AS MatchesOldRule,
       COALESCE(SUM(CASE WHEN ABS(o.StoreShare - a.NewStore) < 0.01 THEN 1 ELSE 0 END),0) AS MatchesNewRule,
       COALESCE(SUM(CASE WHEN ABS(o.StoreShare - a.OldStore) >= 0.01 AND ABS(o.StoreShare - a.NewStore) >= 0.01 THEN 1 ELSE 0 END),0) AS RequiresManualReview,
       COALESCE(SUM(CASE WHEN o.Permitted = 1 THEN 1 ELSE 0 END),0) AS FinanciallyPermitted,
       COALESCE(SUM(CASE WHEN a.StoreCount > 1 THEN 1 ELSE 0 END),0) AS SharedOrders,
       COALESCE(SUM(CASE WHEN o.Permitted = 0 AND ABS(o.StoreShare - a.OldStore) < 0.01 THEN a.OldStore-a.NewStore ELSE 0 END),0) AS EstimatedUnsettledShiftAtCurrentRates
FROM dbo.ProductOrders o JOIN Amounts a ON a.ProductOrderId = o.Id
JOIN dbo.Codes state ON state.Id = o.ProductOrderStateId
WHERE o.IsPaid = 1 AND o.Deleted = 0 AND a.DeliveryPrice > 0
  AND state.Label <> N'ProductOrderState_Canceled';
