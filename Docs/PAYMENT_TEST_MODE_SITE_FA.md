# راهنمای پرداخت، کیف پول و کد تخفیف برای اپ و پنل

## وضعیت فعلی پرداخت

هر چهار Merchant فعلاً در حالت تست مرکزی هستند و هیچ درخواست واقعی به بانک ارسال نمی‌شود. تنظیم زیر باید در هر دو سرویس `Api` و `Payment` یکسان بماند:

```json
"PaymentTestMode": {
  "Enabled": true,
  "AllowResultOverride": false,
  "DefaultResult": "Success"
}
```

در این تنظیم، بک‌اند فقط یک نتیجه موفق معتبر تولید می‌کند. کاربر یا فرانت نمی‌تواند با تغییر `testResult` نتیجه Callback را عوض کند.

این رفتار برای سفارش محصول، رزرو نماینده، رزرو پانسیون، سفر، کارگو، بیمه، شارژ کیف پول و خرید پلن PastilAI مشترک است.

## سه حالت تسویه

### فقط درگاه تست

اگر کیف پول انتخاب نشده باشد، کل مبلغ باقی‌مانده وارد پرداخت تستی می‌شود. فرانت باید فقط `paymentUrl` برگشتی را باز کند.

### کیف پول و درگاه تست

اگر موجودی قابل استفاده کمتر از مبلغ باشد، بک‌اند سهم کیف پول را محاسبه می‌کند و فقط باقیمانده را به پرداخت تستی می‌فرستد. برداشت کیف پول فقط پس از Callback موفق قطعی می‌شود.

### فقط کیف پول یا تخفیف صددرصدی

اگر کیف پول و/یا کد تخفیف کل مبلغ را پوشش دهد، تسویه داخل همان درخواست نهایی می‌شود:

- `paymentIsLink=false`
- `paymentUrl` خالی است
- فرانت نباید Redirect انجام دهد
- سفارش، رزرو یا اشتراک همان لحظه نهایی می‌شود

## پاسخ شروع پرداخت

```json
{
  "isSuccess": true,
  "data": {
    "paymentId": 4501,
    "amount": 125000,
    "grossAmount": 300000,
    "rebateAmount": 75000,
    "walletAmount": 100000,
    "paymentIsLink": true,
    "isTestMode": true,
    "paymentUrl": "https://payment.pastil.pet/callback/4501?callbackToken=...&testResult=success",
    "testSuccessUrl": null,
    "testFailureUrl": null
  }
}
```

`grossAmount` مبلغ اولیه، `rebateAmount` سهم تخفیف، `walletAmount` سهم کیف پول و `amount` مبلغ باقیمانده برای پرداخت تستی است.

فرانت نباید Callback URL یا `callbackToken` را بسازد، ذخیره دائمی کند، تغییر دهد یا چند بار فراخوانی کند. فقط مقدار `paymentUrl` پاسخ بک‌اند باید با Redirect کامل صفحه باز شود.

```ts
function continuePayment(response: any) {
  if (!response?.isSuccess || !response?.data) {
    showError(response?.messages?.[0]?.item1 ?? "شروع پرداخت ناموفق بود");
    return;
  }

  const payment = response.data;
  if (!payment.paymentIsLink || !payment.paymentUrl) {
    showPaymentSuccess();
    return;
  }

  window.location.assign(payment.paymentUrl);
}
```

## Endpointهای پرداخت

| فرایند | Endpoint |
| --- | --- |
| رزرو نماینده | `POST /api/EndUser/CompanionReservePayment` |
| رزرو پانسیون | `POST /api/EndUser/PansionReservePayment` |
| سفر | `POST /api/EndUser/TripPayment` |
| کارگو | `POST /api/EndUser/CargoPayment` |
| بیمه | `POST /api/EndUser/CompanionInsurancePackageSalePayment` |
| شارژ کیف پول | `POST /api/EndUser/Wallet` |
| PastilAI | `POST /api/EndUser/PastilAI/purchase` |
| سفارش محصول | عملیات `SetOrder` در `POST /api/EndUser/Cart` |

هویت کاربر و مبلغ نهایی از سمت بک‌اند تعیین می‌شوند. فرانت نباید `userId` یا مبلغ محاسبه‌شده را قابل اعتماد فرض کند.

## افزودن، حذف و نوع کد تخفیف

هر کد تخفیف دقیقاً به یک `TypeId` متعلق است. `TypeId` باید از Codeهای داینامیک گروه RebateType گرفته شود و در پنل Hard-code نشود. Labelهای فعلی عبارت‌اند از:

- `RebateType_Cart`
- `RebateType_CompanionReserve`
- `RebateType_PansionReserve`
- `RebateType_Trip`
- `RebateType_Cargo`
- `RebateType_InsurancePackageSale`
- `RebateType_PastilAI`

کد مربوط به یک فرایند در هیچ فرایند دیگری پذیرفته نمی‌شود. محدودیت تاریخ، تعداد کل، تعداد هر کاربر، حداقل مبلغ، کاربر اختصاصی و Targetهای Pastil Club نیز در بک‌اند کنترل می‌شوند.

برای رزروها و خدمات، Endpointهای زیر با `PUT` استفاده می‌شوند:

```text
/api/EndUser/{Prefix}SetRebate
/api/EndUser/{Prefix}RemoveRebate?id={id}
/api/EndUser/{Prefix}SetWallet
```

`Prefix` یکی از مقادیر زیر است:

- `CompanionReserve`
- `PansionReserve`
- `Trip`
- `Cargo`
- `CompanionInsurancePackageSale`

نمونه افزودن کد:

```json
{
  "id": 120,
  "rebateCode": "pastil-club-20"
}
```

نمونه انتخاب یا لغو کیف پول:

```json
{
  "id": 120,
  "fromWallet": true
}
```

پس از ساخته‌شدن یک Payment فعال، تغییر کد تخفیف یا سهم کیف پول برای همان آیتم رد می‌شود. اگر شروع پرداخت ناموفق شد، کاربر می‌تواند پس از پایان وضعیت فعال دوباره اقدام کند.

در سبد خرید، عملیات‌های `SetRebate` و `RemoveRebate` همان قرارداد فعلی `CartUpdateDto` را دارند. در PastilAI نیز `rebateCode` و `fromWallet` داخل درخواست `purchase` ارسال می‌شوند.

## پنل مدیریت

Entity و API کد تخفیف حذف نشده‌اند، چون موتور پرداخت و جوایز Club به `Rebate` وابسته‌اند. اما Permission و منوی مستقل آن از مدیریت فروشگاه خارج شده و زیر `PastilClubManagement > Rebate` قرار گرفته است.

پنل باید:

- کدهای تخفیف را فقط در بخش Pastil Club نمایش دهد؛
- نوع کد را از Code API داینامیک بگیرد؛
- `usedCount` را قابل ویرایش نکند؛
- `typeId`، بازه تاریخ، `useCount`، `maxUsePerUser` و حداقل مبلغ را اجباری کنترل کند؛
- اطلاعات محرمانه Merchant را نمایش ندهد و مقدار خالی در ویرایش را به معنی «حفظ مقدار قبلی» در نظر بگیرد.

## نکات استقرار

قبل از اجرای بک‌اند، یک کلید Base64 با طول ۳۲ بایت بسازید و مقدار یکسان آن را در `.env` سرویس‌های API و Payment قرار دهید:

```bash
openssl rand -base64 32
```

```env
PASTIL_MERCHANT_ENCRYPTION_KEY=VALUE_FROM_COMMAND
```

کلید واقعی نباید وارد Git شود. اطلاعات Merchant قدیمی با اولین شروع پرداخت، در صورت وجود این کلید، خودکار رمز می‌شوند.

Migration `SecurePaymentWalletRebateAndFinancialIntegrity` باید قبل از اجرای نسخه جدید اعمال شود. این Migration پرداخت‌های قدیمی، کدهای تکراری، مصرف تکراری کد و داده‌های منفی را برای ایجاد محدودیت‌های جدید پاک‌سازی می‌کند.
