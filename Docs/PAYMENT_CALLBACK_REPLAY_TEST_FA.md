# اثبات idempotency پرداخت در staging

این تست فقط برای staging و یک شارژ کیف‌پولِ دورریختنی است. هرگز URL callback، مخصوصاً `callbackToken`، را در تیکت، چت یا لاگ عمومی قرار ندهید.

## پیش‌نیاز

در هر دو سرویس API و Payment حالت تست فعال باشد:

```json
"PaymentTestMode": {
  "Enabled": true,
  "AllowResultOverride": false,
  "DefaultResult": "Success"
}
```

## ۱. retry هم‌زمان شروع پرداخت

یک target دورریختنی در staging و payload معتبرِ همان endpoint پرداخت آماده کنید. با یک access token کاربر آزمایشی، همان درخواست را ۱۰ بار هم‌زمان و با یک کلید مشترک بفرستید:

```powershell
pwsh -File .\scripts\verify-payment-start-idempotency.ps1 `
  -StartUrl 'https://api-staging.example/api/EndUser/Wallet' `
  -BearerToken 'TOKEN_ONLY_IN_LOCAL_TERMINAL' `
  -PayloadJson '{"amount":10000,"merchantId":1}' `
  -Concurrency 10 `
  -ConfirmStaging
```

همهٔ پاسخ‌ها باید یک `PaymentId` واحد داشته باشند. از همان payment ساخته‌شده در مرحلهٔ بعد استفاده کنید.

## ۲. callback تکراری هم‌زمان

1. با یک کاربر آزمایشی، از UI staging یک شارژ کیف‌پول کوچک ایجاد کنید و URL پرداخت برگشتی را فقط در همان ترمینال نگه دارید.
2. یک بار URL را باز کنید تا نتیجهٔ موفق اولیه ثبت شود؛ سپس همان URL را با ۲۰ درخواست هم‌زمان replay کنید:

```powershell
pwsh -File .\scripts\verify-payment-callback-replay.ps1 `
  -CallbackUrl 'https://payment-staging.example/callback/123?callbackToken=REDACTED&testResult=success' `
  -Concurrency 20 `
  -ConfirmStaging
```

3. شناسهٔ پرداخت را در `scripts/database/AssertWalletPaymentCallbackReplay.sql` قرار دهید و query را روی دیتابیس staging اجرا کنید.

## معیار قبولی

- هر ۱۰ درخواست شروع پرداخت، موفق و دارای یک `PaymentId` مشترک باشند.
- هر ۲۰ پاسخ، صفحهٔ موفق callback را برگردانند.
- `Payments.IsSuccess = 1`، `AppliedDate` پر و `GatewayStatus = TEST_APPLIED` باشد.
- فقط دقیقاً یک `Wallets` فعال با همان `PaymentId` وجود داشته باشد.

در صورت شکست، آن پرداخت آزمایشی را دوباره replay نکنید؛ لاگ‌های Payment و API و خروجی query را بررسی کنید.
