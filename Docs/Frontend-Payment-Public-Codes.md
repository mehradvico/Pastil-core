# مستند اتصال فرانت به کدهای عمومی پرداخت، سفارش و رزرو

مخاطب: تیم فرانت اپ پاستیل (امیرمحسن)  
تاریخ آخرین بروزرسانی: ۱۴۰۵/۰۵/۳۰

## خلاصه تغییر

از این نسخه به بعد هیچ شناسه داخلی دیتابیس مانند `id`، `paymentId`، `productOrderId`، `companionReserveId` یا `pansionReserveId` نباید به کاربر نمایش داده شود.

شناسه‌های داخلی همچنان برای Routeها، درخواست‌های API و ارتباط داخلی موجودیت‌ها استفاده می‌شوند. برای نمایش به کاربر باید از فیلدهای عمومی زیر استفاده شود:

| موضوع | فیلد قابل نمایش | نمونه |
|---|---|---|
| پرداخت | `paymentCode` | `H7K9W4P2M8X6Q3R5T9Y2N4BC` |
| سفارش محصول | `orderCode` | `ORD-14050530-1426-4387` |
| رزرو خدمات همراه | `reserveCode` | `RSV-14050530-1425-8642` |
| رزرو پانسیون | `reserveCode` | `PAN-14050530-1428-1436` |
| کد مرتبط با تراکنش کیف پول | `referenceCode` | کد سفارش یا رزرو مرتبط |

> `paymentCode` کد یک تراکنش پرداخت است؛ `orderCode` یا `reserveCode` کد خود سفارش/رزرو است. این کدها مستقل‌اند و نباید به‌جای یکدیگر ذخیره شوند.

## قانون اصلی نمایش

- در UI پرداخت همیشه `paymentCode` نمایش داده شود.
- در UI سفارش محصول همیشه `orderCode` نمایش داده شود.
- در UI رزرو خدمات و پانسیون همیشه `reserveCode` نمایش داده شود.
- در تاریخچه کیف پول ابتدا `paymentCode` و در صورت وجود، `referenceCode` نیز جداگانه نمایش داده شود.
- اگر کد عمومی تهی بود، عبارت «کد ثبت نشده» نمایش داده شود؛ هرگز به‌عنوان fallback شناسه عددی نشان داده نشود.
- شناسه داخلی فقط برای فراخوانی API یا ساخت Route استفاده شود.

نمونه صحیح:

```vue
<span>{{ order.orderCode || 'کد ثبت نشده' }}</span>

<button @click="openOrder(order.id)">مشاهده جزئیات</button>
```

نمونه ممنوع:

```vue
<span>#{{ order.id }}</span>
<span>{{ order.orderCode || order.id }}</span>
```

## فرمت کدها

فرمت کلی کد عمومی:

```text
PREFIX-JALALI_DATE-TIME-4_DIGIT_SUFFIX
```

مثال:

```text
H7K9W4P2M8X6Q3R5T9Y2N4BC
```

پیشوندهای پرداخت:

| عملیات | پیشوند |
|---|---|
| سفارش محصول | `ORD` |
| شارژ کیف پول | `WLT` |
| PastilAI | `PAI` |
| رزرو خدمات همراه | `RSV` |
| رزرو پانسیون | `PAN` |
| سفر | `TRP` |
| کارگو | `CRG` |
| بیمه | `INS` |

تمام مبالغ API در این جریان بر حسب **تومان** هستند.

## قرارداد شروع پرداخت

Endpointهای مستقیم بک‌اند:

| عملیات | Method و Endpoint |
|---|---|
| Checkout سبد خرید | `POST /api/EndUser/Cart` |
| شارژ کیف پول | `POST /api/EndUser/Wallet` |
| پرداخت رزرو همراه | `POST /api/EndUser/CompanionReservePayment` |
| پرداخت رزرو پانسیون | `POST /api/EndUser/PansionReservePayment` |
| خرید اشتراک PastilAI | `POST /api/EndUser/PastilAI/purchase` |
| پرداخت سفر | `POST /api/EndUser/TripPayment` |
| پرداخت کارگو | `POST /api/EndUser/CargoPayment` |
| پرداخت بیمه | `POST /api/EndUser/CompanionInsurancePackageSalePayment` |

در Nuxt فعلی بعضی از این مسیرها از Proxy داخلی اپ، مانند `/api/wallet/wallet` یا `/api/pansion/payment` عبور می‌کنند. قانون Header و پاسخ در هر دو حالت یکسان است.

### هدر Idempotency-Key

هر Checkout باید یک UUID نسخه ۴ با فرمت استاندارد در هدر زیر ارسال کند:

```http
Idempotency-Key: 550e8400-e29b-41d4-a716-446655440000
```

قوانین:

1. برای شروع یک Checkout جدید، UUID جدید ساخته شود.
2. اگر پاسخ به‌دلیل قطع اینترنت، Timeout یا وضعیت نامشخص دریافت نشد، Retry همان Checkout باید با همان UUID انجام شود.
3. UUID فقط بعد از دریافت نتیجه قطعی Checkout پاک شود.
4. UUID یک Checkout نباید برای مبلغ، کاربر، درگاه یا موجودیت دیگری استفاده شود.
5. هدر باید دقیقاً UUID معتبر با فرمت `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` باشد.

اگر یک کلید برای Checkout دیگری استفاده شود، پاسخ ناموفق زیر برمی‌گردد:

```text
این Idempotency-Key قبلاً برای Checkout دیگری استفاده شده است.
```

اگر فرمت کلید معتبر نباشد:

```text
هدر Idempotency-Key باید یک UUID معتبر باشد.
```

### نمونه پیاده‌سازی فرانت

```ts
const scope = `companion-reserve:${reserveId}`
const storageKey = `pastil:checkout:idempotency:${scope}`

let idempotencyKey = sessionStorage.getItem(storageKey)
if (!idempotencyKey) {
  idempotencyKey = crypto.randomUUID()
  sessionStorage.setItem(storageKey, idempotencyKey)
}

const response = await $fetch('/api/companionReserve/ReservePayment', {
  method: 'POST',
  headers: {
    'Idempotency-Key': idempotencyKey,
  },
  body: {
    companionReserveId: reserveId,
    merchantId: selectedMerchantId,
  },
})

// فقط بعد از دریافت نتیجه قطعی Checkout
if (response?.isSuccess) {
  sessionStorage.removeItem(storageKey)
}
```

### پاسخ موفق شروع پرداخت

ساختار عمومی پاسخ:

```json
{
  "isSuccess": true,
  "messages": [],
  "data": {
    "paymentId": 2481,
    "paymentCode": "H7K9W4P2M8X6Q3R5T9Y2N4BC",
    "paymentUrl": "https://gateway.example/start/...",
    "paymentIsLink": true,
    "isOnline": true,
    "amount": 250000,
    "grossAmount": 300000,
    "rebateAmount": 0,
    "walletAmount": 50000,
    "merchantId": 2,
    "companionReserveId": 321
  }
}
```

رفتار فرانت بعد از پاسخ:

```ts
if (response.isSuccess && response.data?.paymentIsLink) {
  showMessage('در حال انتقال به درگاه پرداخت')
  await navigateTo(response.data.paymentUrl, { external: true })
}
```

- `paymentCode` باید قبل از انتقال به درگاه به کاربر نمایش داده شود.
- مقدار `paymentId` فقط داخلی است و نباید در Toast، فاکتور یا صفحه نتیجه نمایش داده شود.
- URL درگاه فقط از `paymentUrl` خوانده شود و در فرانت ساخته نشود.
- اگر `paymentIsLink` برابر `false` بود، پرداخت از کیف پول/تخفیف به‌صورت داخلی نهایی شده و نباید کاربر به درگاه منتقل شود.

## پاسخ Retry با Idempotency-Key یکسان

ارسال مجدد همان Checkout با همان UUID، پرداخت جدید ایجاد نمی‌کند و اطلاعات همان پرداخت را برمی‌گرداند:

```json
{
  "isSuccess": true,
  "data": {
    "paymentId": 2481,
    "paymentCode": "H7K9W4P2M8X6Q3R5T9Y2N4BC",
    "paymentUrl": "https://gateway.example/start/...",
    "paymentIsLink": true
  }
}
```

در Retry نباید انتظار `paymentCode` یا `paymentId` جدید وجود داشته باشد.

## فیلدهای لیست و جزئیات

### سفارش محصول

پاسخ لیست و جزئیات سفارش شامل موارد زیر است:

```json
{
  "id": "internal-order-id",
  "orderCode": "ORD-14050530-1426-4387"
}
```

- Route و درخواست جزئیات: `id`
- متن قابل نمایش و دکمه کپی: `orderCode`

### رزرو خدمات همراه

```json
{
  "id": 321,
  "reserveCode": "RSV-14050530-1425-8642"
}
```

- Route و عملیات لغو/ویرایش: `id`
- متن قابل نمایش و دکمه کپی: `reserveCode`

### رزرو پانسیون

```json
{
  "id": 781,
  "reserveCode": "PAN-14050530-1428-1436"
}
```

- Route و عملیات جزئیات/لغو: `id`
- متن قابل نمایش و دکمه کپی: `reserveCode`

### تاریخچه کیف پول

هر ردیف ممکن است دارای دو کد باشد:

```json
{
  "paymentCode": "H7K9W4P2M8X6Q3R5T9Y2N4BC",
  "referenceCode": "ORD-14050530-1426-4387"
}
```

- `paymentCode`: کد تراکنش پرداخت/کیف پول
- `referenceCode`: کد عمومی سفارش یا رزروی که تراکنش به آن مربوط است

هیچ‌کدام جایگزین دیگری نیستند.

## Callback پرداخت

Callback توسط بک‌اند مدیریت می‌شود:

```text
GET /callback/{paymentId}?callbackToken={token}
```

فرانت نباید این URL یا `callbackToken` را بسازد، تغییر دهد یا ذخیره دائمی کند. در صفحه نتیجهٔ پرداخت بک‌اند موارد زیر نمایش داده می‌شوند:

- `paymentCode` یک توکن داخلی حروف‌وعدد است و در رابط کاربری به کاربر نمایش داده نشود.
- `referenceCode` با عنوان «کد سفارش» یا «کد رزرو»
- شماره پیگیری بانکی با عنوان «پیگیری پرداخت»
- مبلغ به تومان

شناسه داخلی callback یا `paymentId` نباید به کاربر نمایش داده شود.

## چک‌لیست تحویل فرانت

- [ ] همه Checkoutها هدر `Idempotency-Key` معتبر دارند.
- [ ] Retry یک Checkout با همان UUID انجام می‌شود.
- [ ] `paymentCode` قبل از رفتن به درگاه نمایش داده می‌شود.
- [ ] در لیست و جزئیات سفارش از `orderCode` استفاده می‌شود.
- [ ] در لیست و جزئیات رزرو از `reserveCode` استفاده می‌شود.
- [ ] در کیف پول `paymentCode` و `referenceCode` با عنوان درست نمایش داده می‌شوند.
- [ ] هیچ ID داخلی به‌عنوان fallback در UI نمایش داده نمی‌شود.
- [ ] IDهای داخلی فقط برای Route و API باقی مانده‌اند.
- [ ] مبلغ‌ها همه با واحد تومان نمایش داده می‌شوند.
