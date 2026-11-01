-- کد اختصاصی هر تراکنش کیف پول (WalletTransactionCode، ۲۰۲۶-۱۱-۰۱). تکرارپذیر: فقط موارد ناموجود ساخته/پر می‌شوند.
-- قالب: WLT-<تاریخ شمسی yyyyMMdd>-<HHmm>-<پسوند ۴ رقمی> از روی CreateDate همان ردیف (هم‌شکل کدهای ORD-).
-- ترتیب: این اسکریپت را قبل از دیپلوی Api/Payment جدید اجرا کنید؛ ردیف‌های جدید را خود برنامه کد می‌دهد.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('Wallets','TransactionCode') IS NULL
    EXEC(N'ALTER TABLE [Wallets] ADD [TransactionCode] nvarchar(32) NULL');

COMMIT;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- پر کردن کد ردیف‌های قبلی. پسوند در هر دقیقه با ROW_NUMBER یکتا می‌شود (حداکثر ۹۰۰۰ ردیف در یک دقیقه).
;WITH Source AS (
    SELECT
        w.Id,
        w.TransactionCode,
        DATEPART(HOUR, w.CreateDate) AS Hh,
        DATEPART(MINUTE, w.CreateDate) AS Mi,
        DATEPART(YEAR, w.CreateDate) AS Gy,
        DATEPART(MONTH, w.CreateDate) AS Gm,
        DATEPART(DAY, w.CreateDate) AS Gd
    FROM [Wallets] w
    WHERE w.TransactionCode IS NULL
),
DayCount AS (
    -- میلادی ← شمسی (الگوریتم jdf)
    SELECT s.*,
        355666 + 365 * s.Gy
        + (CASE WHEN s.Gm > 2 THEN s.Gy + 1 ELSE s.Gy END + 3) / 4
        - (CASE WHEN s.Gm > 2 THEN s.Gy + 1 ELSE s.Gy END + 99) / 100
        + (CASE WHEN s.Gm > 2 THEN s.Gy + 1 ELSE s.Gy END + 399) / 400
        + s.Gd
        + CHOOSE(s.Gm, 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334) AS D0
    FROM Source s
),
Step1 AS (
    SELECT d.*, -1595 + 33 * (d.D0 / 12053) + 4 * ((d.D0 % 12053) / 1461) AS Jy1, (d.D0 % 12053) % 1461 AS D1
    FROM DayCount d
),
Step2 AS (
    SELECT s.*,
        s.Jy1 + CASE WHEN s.D1 > 365 THEN (s.D1 - 1) / 365 ELSE 0 END AS Jy,
        CASE WHEN s.D1 > 365 THEN (s.D1 - 1) % 365 ELSE s.D1 END AS D2
    FROM Step1 s
),
Jalali AS (
    SELECT s.Id, s.TransactionCode, s.Hh, s.Mi, s.Jy,
        CASE WHEN s.D2 < 186 THEN 1 + s.D2 / 31 ELSE 7 + (s.D2 - 186) / 30 END AS Jm,
        CASE WHEN s.D2 < 186 THEN 1 + s.D2 % 31 ELSE 1 + (s.D2 - 186) % 30 END AS Jd
    FROM Step2 s
),
Coded AS (
    SELECT j.*,
        ROW_NUMBER() OVER (PARTITION BY j.Jy, j.Jm, j.Jd, j.Hh, j.Mi ORDER BY j.Id) AS Rn,
        (CAST(j.Jy AS bigint) * 100000000 + j.Jm * 1000000 + j.Jd * 10000 + j.Hh * 100 + j.Mi) AS MinuteKey
    FROM Jalali j
)
UPDATE c
SET c.TransactionCode =
    'WLT-'
    + RIGHT('0000' + CAST(c.Jy AS varchar(4)), 4)
    + RIGHT('00' + CAST(c.Jm AS varchar(2)), 2)
    + RIGHT('00' + CAST(c.Jd AS varchar(2)), 2)
    + '-'
    + RIGHT('00' + CAST(c.Hh AS varchar(2)), 2)
    + RIGHT('00' + CAST(c.Mi AS varchar(2)), 2)
    + '-'
    + CAST(1000 + ((c.Rn - 1) + ((c.MinuteKey * 3571 + 1877) % 9000)) % 9000 AS varchar(4))
FROM Coded c;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Wallets_TransactionCode' AND object_id = OBJECT_ID(N'[Wallets]'))
    EXEC(N'CREATE UNIQUE INDEX [IX_Wallets_TransactionCode] ON [Wallets] ([TransactionCode]) WHERE [TransactionCode] IS NOT NULL');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261031230658_WalletTransactionCode')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261031230658_WalletTransactionCode', N'9.0.0');

COMMIT;
GO

-- بررسی: باید ۰ برگرداند
SELECT COUNT(*) AS WalletsWithoutCode FROM [Wallets] WHERE [TransactionCode] IS NULL;
