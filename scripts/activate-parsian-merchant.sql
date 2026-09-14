-- فعال‌سازی درگاه واقعی پارسیان برای پاستیل
-- فقط Username واقعاً توسط ParsianGateway.cs استفاده می‌شود (به‌عنوان LoginAccount در
-- درخواست SOAP)؛ بقیه‌ی فیلدها را عمداً NULL می‌کنیم تا پلیس‌هولدر قدیمی TEST_MODE گیج‌کننده نماند.
-- مقدار را خام (plain text) می‌گذاریم — MerchantService خودش در اولین پرداخت واقعی رمزنگاری می‌کند.

UPDATE dbo.Merchants
SET
    Username = N'B4icn0io832s13qghNHw',
    Password = NULL,
    PrivateKey = NULL,
    TerminalKey = NULL,
    MerchantNo = NULL,
    Active = 1
WHERE Id = 2 AND BankId = 2; -- ردیف parsian

-- تایید نتیجه
SELECT Id, BankId, Username, Active FROM dbo.Merchants WHERE Id = 2;
