-- لغو گروهی سفرهای مانده در «درخواست شده» (TripStatusId 75 → 77)، همراه با برگشت پول سفرهای پرداخت‌شده به کیف پول.
-- قبل از دیپلوی job لغو خودکار اجرا شود (وگرنه اولین اجرای AutoCancelUnassignedTrips همه را لغو و به هر کاربر پوش «لغو شد» می‌فرستد).
-- برگشت پول با همان کلید دفتر بک‌اند (TripCancelRefund:{tripId}) ثبت می‌شود، پس بعداً دوباره برنمی‌گردد.
-- نوبت‌های سرویس هفتگی که این‌جا لغو شوند، نوبت بعدی‌شان با ری‌استارت/دیپلوی Api دوباره ساخته می‌شود.

-- ۰) اگر نسخه‌ی قبلی این اسکریپت را در همین تب اجرا کرده‌اید و COMMIT نزده‌اید، تراکنش هنوز باز است:
SELECT @@TRANCOUNT AS OpenTransactions;   -- اگر 1 یا بیشتر بود، اول در همین تب ROLLBACK; را اجرا کنید

-- ۱) پیش‌نمایش (فقط خواندن)
SELECT t.Id, t.UserId, t.CreateDate, t.TripStartDateTime, t.PetResanServiceScheduleId, t.IsPaid,
       ISNULL(w.WalletPaid, 0) AS WalletPaid, ISNULL(p.GatewayPaid, 0) AS GatewayPaid,
       ISNULL(w.WalletPaid, 0) + ISNULL(p.GatewayPaid, 0) AS RefundAmount
FROM Trips t
OUTER APPLY (SELECT SUM(x.Amount) AS WalletPaid FROM Wallets x
             WHERE x.TripId = t.Id AND x.UserId = t.UserId AND x.IsIncrease = 0 AND x.Deleted = 0 AND x.Painding = 0) w
OUTER APPLY (SELECT SUM(y.Amount) AS GatewayPaid FROM Payments y
             WHERE y.UserId = t.UserId AND y.IsSuccess = 1 AND y.IsOnline = 1 AND y.AppliedDate IS NOT NULL
               AND (y.GatewayStatus IS NULL OR y.GatewayStatus NOT LIKE 'TEST[_]%')
               AND (y.TripId = t.Id OR (y.CallBackTypeLabel = N'Trip' AND y.CallBackId = CAST(t.Id AS nvarchar(50))))) p
WHERE t.TripStatusId = 75
ORDER BY t.Id;

-- ۲) برگشت پول + لغو، در یک تراکنش که خودش COMMIT می‌شود (و با هر خطا کامل برمی‌گردد)
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRAN;

    DECLARE @now datetime2 = GETDATE();

    INSERT INTO Wallets (Name, IsIncrease, Amount, CreateDate, UserId, Painding, Deleted)
    SELECT N'TripCancelRefund:' + CAST(t.Id AS nvarchar(50)), 1,
           ISNULL(w.WalletPaid, 0) + ISNULL(p.GatewayPaid, 0), @now, t.UserId, 0, 0
    FROM Trips t
    OUTER APPLY (SELECT SUM(x.Amount) AS WalletPaid FROM Wallets x
                 WHERE x.TripId = t.Id AND x.UserId = t.UserId AND x.IsIncrease = 0 AND x.Deleted = 0 AND x.Painding = 0) w
    OUTER APPLY (SELECT SUM(y.Amount) AS GatewayPaid FROM Payments y
                 WHERE y.UserId = t.UserId AND y.IsSuccess = 1 AND y.IsOnline = 1 AND y.AppliedDate IS NOT NULL
                   AND (y.GatewayStatus IS NULL OR y.GatewayStatus NOT LIKE 'TEST[_]%')
                   AND (y.TripId = t.Id OR (y.CallBackTypeLabel = N'Trip' AND y.CallBackId = CAST(t.Id AS nvarchar(50))))) p
    WHERE t.TripStatusId = 75
      AND ISNULL(w.WalletPaid, 0) + ISNULL(p.GatewayPaid, 0) > 0
      AND NOT EXISTS (SELECT 1 FROM Wallets r
                      WHERE r.Name = N'TripCancelRefund:' + CAST(t.Id AS nvarchar(50)) AND r.UserId = t.UserId AND r.Deleted = 0);
    DECLARE @refunds int = @@ROWCOUNT;

    UPDATE Trips
    SET TripStatusId = 77,
        CancelInitiatorId = 4,               -- Admin
        CancelReasonCodeId = NULL,
        CancelReasonDetail = N'لغو دستی گروهی سفرهای درخواست‌شده',
        ProgressUpdateDate = @now
    WHERE TripStatusId = 75;
    DECLARE @canceled int = @@ROWCOUNT;

    COMMIT;
    SELECT @canceled AS CanceledTrips, @refunds AS WalletRefunds;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    SELECT ERROR_MESSAGE() AS Error;
END CATCH;
