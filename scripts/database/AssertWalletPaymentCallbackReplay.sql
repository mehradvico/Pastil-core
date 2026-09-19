/*
   Read-only staging assertion for verify-payment-callback-replay.ps1.
   Set @PaymentId to the disposable Wallet payment created for the test.
*/
DECLARE @PaymentId BIGINT = 0; -- replace before running

IF @PaymentId <= 0
    THROW 51000, 'Set @PaymentId to the staging test payment ID before running this assertion.', 1;

SELECT
    p.Id,
    p.PaymentCode,
    p.IdempotencyKey,
    p.CallBackTypeLabel,
    p.IsSuccess,
    p.AppliedDate,
    p.GatewayStatus,
    p.RefNumber,
    p.Amount,
    p.CreateDate
FROM dbo.Payments AS p
WHERE p.Id = @PaymentId;

SELECT
    w.Id,
    w.PaymentId,
    w.UserId,
    w.Amount,
    w.IsIncrease,
    w.Painding,
    w.Deleted,
    w.CreateDate
FROM dbo.Wallets AS w
WHERE w.PaymentId = @PaymentId;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Payments AS p
    WHERE p.Id = @PaymentId
      AND p.CallBackTypeLabel = 'Wallet'
      AND p.IsSuccess = 1
      AND p.AppliedDate IS NOT NULL
      AND p.GatewayStatus IN ('TEST_APPLIED', 'APPLIED'))
    THROW 51001, 'Payment was not successfully applied as a Wallet payment.', 1;

IF (SELECT COUNT(*) FROM dbo.Wallets AS w WHERE w.PaymentId = @PaymentId AND w.Deleted = 0) <> 1
    THROW 51002, 'Callback replay created zero or more than one active wallet credit.', 1;

PRINT 'PASS: exactly one active wallet credit exists and the payment is applied.';
