# مستند پرداخت آزمایشی برای فرانت سایت

## وضعیت فعلی

حالت تست پرداخت در API و سرویس Callback فعال است. انتخاب هر چهار درگاه زیر در این حالت رفتار یکسان دارد و هیچ درخواستی به بانک ارسال نمی‌شود:

- زرین‌پال
- پارسیان
- ملت
- سامان‌کیش

این حالت برای پرداخت سفارش محصول، رزرو نماینده، رزرو پانسیون، سفر، کارگو، بیمه، شارژ کیف پول و خرید پلن PastilAI اعمال می‌شود.

> این قابلیت فقط برای تست است. قبل از فعال‌کردن پرداخت واقعی باید مقدار `PaymentTestMode:Enabled` در تنظیمات هر دو پروژه `Api` و `Payment` برابر `false` شود.

## دریافت درگاه‌ها

```http
GET /api/Merchant
```

فقط درگاه‌های فعال برگردانده می‌شوند. مقدار `id` درگاه انتخاب‌شده را در فیلد `merchantId` درخواست پرداخت ارسال کنید. در حالت تست، اطلاعات واقعی ترمینال و کلید درگاه استفاده نمی‌شود؛ با این حال خود Merchant باید در دیتابیس فعال باشد.

## شروع پرداخت‌ها

تمام درخواست‌ها به Bearer Token کاربر نیاز دارند، به‌جز endpointهایی که قبلاً در پروژه عمومی تعریف شده‌اند.

| فرایند | Endpoint | بدنه‌ی اصلی |
| --- | --- | --- |
| رزرو نماینده | `POST /api/EndUser/CompanionReservePayment` | `merchantId`, `companionReserveId` |
| رزرو پانسیون | `POST /api/EndUser/PansionReservePayment` | `merchantId`, `pansionReserveId` |
| سفر | `POST /api/EndUser/TripPayment` | `merchantId`, `tripId` |
| کارگو | `POST /api/EndUser/CargoPayment` | `merchantId`, `cargoId` |
| بیمه | `POST /api/EndUser/CompanionInsurancePackageSalePayment` | `merchantId`, `companionInsurancePackageSaleId` |
| شارژ کیف پول | `POST /api/EndUser/Wallet` | `merchantId`, `amount` |
| خرید PastilAI | `POST /api/EndUser/PastilAI/purchase` | `planId`, `merchantId`, `rebateCode`, `fromWallet` |
| سفارش محصول | `POST /api/EndUser/Cart` با عملیات ثبت سفارش موجود | قرارداد فعلی `CartUpdateDto` شامل `merchantId`, `fromWallet`, `rebateCode` و سایر اطلاعات سفارش |

نمونه‌ی پرداخت رزرو پانسیون:

```json
{
  "merchantId": 1,
  "pansionReserveId": 120
}
```

نمونه‌ی شارژ کیف پول:

```json
{
  "merchantId": 1,
  "amount": 100000
}
```

نمونه‌ی خرید PastilAI:

```json
{
  "planId": 2,
  "merchantId": 1,
  "rebateCode": null,
  "fromWallet": false
}
```

مبلغ و شناسه‌ی کاربر برای رزروها، سفارش و PastilAI در بک‌اند محاسبه می‌شود. فرانت نباید مبلغ محاسبه‌شده یا `userId` را جایگزین کند.

## پاسخ شروع پرداخت در حالت تست

```json
{
  "isSuccess": true,
  "messages": [],
  "code": 0,
  "data": {
    "paymentId": 4501,
    "merchantId": 1,
    "amount": 250000,
    "paymentIsLink": true,
    "isTestMode": true,
    "paymentUrl": "https://payment.pastil.pet/callback/4501?testResult=success",
    "testSuccessUrl": "https://payment.pastil.pet/callback/4501?testResult=success",
    "testFailureUrl": "https://payment.pastil.pet/callback/4501?testResult=failed"
  }
}
```

- `paymentUrl`: نتیجه‌ی پیش‌فرض تست است و فعلاً به callback موفق اشاره می‌کند.
- `testSuccessUrl`: پرداخت را موفق ثبت می‌کند و منطق نهایی همان سفارش، رزرو، کیف پول یا اشتراک را اجرا می‌کند.
- `testFailureUrl`: پرداخت را ناموفق ثبت می‌کند و منطق شکست همان فرایند را اجرا می‌کند.
- `isTestMode`: به فرانت اعلام می‌کند که پاسخ متعلق به حالت تست است.
- `paymentIsLink`: اگر `true` باشد مقدار URL را با تغییر کامل صفحه باز کنید.

## پیاده‌سازی پیشنهادی فرانت

```ts
type PaymentStartData = {
  paymentId: number;
  paymentIsLink: boolean;
  paymentUrl?: string;
  isTestMode: boolean;
  testSuccessUrl?: string;
  testFailureUrl?: string;
};

function continuePayment(data: PaymentStartData) {
  if (!data.paymentIsLink || !data.paymentUrl) {
    // پرداخت کامل با کیف پول یا فرایندی که همان لحظه در بک‌اند نهایی شده است.
    showPaymentSuccess();
    return;
  }

  window.location.assign(data.paymentUrl);
}

function simulateFailedPayment(data: PaymentStartData) {
  if (!data.isTestMode || !data.testFailureUrl) {
    throw new Error("Failed callback is only available in payment test mode.");
  }

  window.location.assign(data.testFailureUrl);
}
```

برای مسیر عادی، فقط `paymentUrl` را باز کنید. دکمه یا انتخاب نتیجه‌ی موفق/ناموفق صرفاً برای QA است و نباید در UI نهایی محصول نمایش داده شود.

## نتیجه‌ی Callback

بازکردن هرکدام از URLهای تست، صفحه‌ی Callback سرویس Payment را نمایش می‌دهد:

- در موفقیت، `isSuccess=true` می‌شود، شماره پیگیری تستی با قالب `TEST-{paymentId}` ثبت می‌شود و فرایند مقصد نهایی می‌شود.
- در شکست، `isSuccess=false` و وضعیت `TEST_FAILED` ثبت می‌شود؛ مقصد نباید پرداخت‌شده تلقی شود.

اولین Callback نتیجه‌ی نهایی آن Payment است. فرانت نباید هر دو URL موفق و ناموفق را برای یک `paymentId` فراخوانی کند و نباید Callback را از طریق درخواست پس‌زمینه چندبار اجرا کند.

## پرداخت کامل با کیف پول

اگر کل مبلغ از کیف پول پوشش داده شود، بک‌اند همان لحظه فرایند را نهایی می‌کند. در این حالت معمولاً `paymentIsLink=false` است و نیازی به redirect یا Callback بانکی نیست. اگر فقط بخشی از مبلغ از کیف پول برداشته شود، باقی‌مانده مانند پرداخت عادی وارد همین فرایند تست می‌شود.

## مدیریت خطا

حتی اگر HTTP Status برابر `200` باشد، همیشه ابتدا `isSuccess` را بررسی کنید. در صورت `false`، متن قابل نمایش از `messages` خوانده شود و هیچ redirectی انجام نشود.

```ts
const response = await startPayment(payload);

if (!response.isSuccess || !response.data) {
  showError(response.messages?.[0]?.item1 ?? "شروع پرداخت ناموفق بود");
  return;
}

continuePayment(response.data);
```

حداقل مبلغ قابل ارسال به درگاه در منطق فعلی بک‌اند `10,000` تومان است. مبلغ‌های کمتر از این مقدار برای پرداخت آنلاین پذیرفته نمی‌شوند.

## تنظیمات بک‌اند

تنظیم مشترک حالت تست:

```json
"PaymentTestMode": {
  "Enabled": true,
  "AllowResultOverride": true,
  "DefaultResult": "Success"
}
```

- `Enabled`: تمام چهار درگاه را به‌صورت مرکزی وارد حالت تست می‌کند.
- `AllowResultOverride`: اجازه می‌دهد `testResult=success` یا `testResult=failed` نتیجه‌ی Callback را تعیین کند.
- `DefaultResult`: نتیجه‌ی `paymentUrl` را مشخص می‌کند؛ مقدار فعلی `Success` است.

تنظیم این بخش باید در پروژه‌های `Api` و `Payment` یکسان باشد. غیرفعال‌کردن آن فقط در یکی از دو پروژه باعث ناسازگاری شروع پرداخت و Callback می‌شود.
