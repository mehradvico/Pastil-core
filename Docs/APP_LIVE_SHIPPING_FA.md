# ارسال لحظه‌ای سفارش فروشگاه — مستند اپ

این قابلیت برای الوپیک، تیپاکس و اسنپ‌باکس طراحی شده و قیمت و مبلغ قابل پرداخت را فقط در بک محاسبه می‌کند.

## پیش‌نیازها

- کاربر باید لاگین باشد.
- آدرس مقصد باید قبل از قیمت‌گیری روی سبد انتخاب شده باشد.
- آدرس مقصد و فروشگاه باید مختصات `Location` داشته باشند.
- روش ارسال فروشگاه باید فعال باشد.
- برای دقت قیمت، وزن و ابعاد محصولات در پنل تکمیل شود. تا قبل از تکمیل، بک از مقادیر پیش‌فرض استفاده می‌کند.

## مرحله اول: انتخاب آدرس

همان API فعلی Cart با `CartUpdateType = SetAddress` استفاده شود. بک از این پس مالکیت آدرس را نیز کنترل می‌کند.

تغییر آدرس، Quote قبلی و هزینه ارسال انتخاب‌شده را باطل می‌کند.

## مرحله دوم: دریافت قیمت‌ها

```http
POST /api/EndUser/ShippingQuote
Authorization: Bearer {token}
Content-Type: application/json
```

```json
{
  "storeId": 12
}
```

نمونه پاسخ:

```json
{
  "isSuccess": true,
  "messages": [],
  "code": 0,
  "data": [
    {
      "quoteToken": "fbd972c7-430d-4bd8-8d99-d76a19885580",
      "deliveryId": 7,
      "deliveryName": "الوپیک",
      "provider": 1,
      "paymentMode": 1,
      "quotedPrice": 145000,
      "payableDeliveryPrice": 145000,
      "payAtDestination": false,
      "currency": "IRR",
      "expiresAtUtc": "2026-08-16T07:30:00Z"
    }
  ]
}
```

### Provider

- `0`: روش داخلی/ثابت
- `1`: الوپیک
- `2`: تیپاکس
- `3`: اسنپ‌باکس

### PaymentMode

- `1`: کرایه همراه سفارش پرداخت می‌شود (`Prepaid`)
- `2`: کرایه در مقصد پرداخت می‌شود (`ReceiverPays`)

در حالت `ReceiverPays`:

- `quotedPrice` برآورد کرایه برای نمایش است.
- `payableDeliveryPrice` صفر است.
- در UI عبارت «پرداخت کرایه در مقصد» نمایش داده شود.
- این حالت را «رایگان» نمایش ندهید.

تایمر انقضا از `expiresAtUtc` نمایش داده شود. زمان‌ها UTC هستند.

## مرحله سوم: انتخاب Quote

```http
POST /api/EndUser/ShippingSelection
Authorization: Bearer {token}
Content-Type: application/json
```

```json
{
  "quoteToken": "fbd972c7-430d-4bd8-8d99-d76a19885580"
}
```

مبلغ یا `deliveryId` را همراه این درخواست ارسال نکنید. بک Quote را با کاربر، سبد، فروشگاه، آدرس، قیمت و زمان انقضا تطبیق می‌دهد.

پس از انتخاب موفق، یک‌بار `GetCart` اجرا شود تا جمع پرداخت و هزینه ارسال به‌روز نمایش داده شود.

## مرحله چهارم: ثبت سفارش

فرایند فعلی `SetOrder` بدون تغییر فراخوانی می‌شود. بک قبل از ایجاد سفارش موارد زیر را مجدداً کنترل می‌کند:

- Quote متعلق به کاربر جاری باشد.
- Quote برای همان `CartStore` و همان آدرس باشد.
- روش ارسال هنوز فعال باشد.
- Quote منقضی نشده باشد.
- محتویات و مبلغ سبد بعد از Quote تغییر نکرده باشد.
- مبلغ ارسال روی سبد با مبلغ امن بک برابر باشد.

اگر Quote منقضی شده باشد، دوباره مرحله دریافت و انتخاب قیمت انجام شود.

## تغییر محتویات سبد

بعد از افزایش، کاهش یا حذف محصول، قیمت ارسال را مجدداً دریافت کنید. بک اجازه ثبت سفارش با Quote مربوط به محتویات قبلی را نمی‌دهد.

## نمایش Cart

روی هر `cartStore` فیلدهای زیر اضافه شده است:

```json
{
  "shippingQuoteId": 120,
  "shippingProvider": 1,
  "shippingPaymentMode": 1,
  "shippingQuotedPrice": 145000,
  "deliveryPrice": 145000
}
```

در حالت پس‌کرایه مقدار `deliveryPrice` صفر و `shippingQuotedPrice` مبلغ برآوردی است.

## مدیریت روش ارسال فروشنده

DTO ثبت و ویرایش `/api/Seller/Delivery` فیلدهای زیر را دارد:

```json
{
  "shippingProvider": 1,
  "livePricing": true,
  "allowPrepaid": true,
  "allowReceiverPay": false
}
```

انواع Code را از API زیر دریافت کنید و ID ثابت در فرانت ننویسید:

```http
GET /api/Seller/Delivery/types
```

Labelهای جدید:

- `DeliveryType_AloPeyk`
- `DeliveryType_Tipax`
- `DeliveryType_SnappBox`

## وزن و ابعاد محصول

DTO افزودن/ویرایش محصول فیلدهای زیر را دارد:

```json
{
  "shippingWeightGrams": 1500,
  "shippingLengthCm": 30,
  "shippingWidthCm": 20,
  "shippingHeightCm": 15
}
```

مقادیر ابعاد سانتی‌متر و وزن گرم هستند.

## نمایش خطا

HTTP موفق به‌تنهایی به معنی موفقیت عملیات نیست. همیشه `isSuccess` بررسی و پیام زیر نمایش داده شود:

```ts
const message = response?.messages?.[0]?.item1 || 'عملیات ناموفق بود'
```

خطاهای مهم:

- آدرس انتخاب نشده است.
- فروشگاه یا آدرس مختصات ندارد.
- Quote منقضی شده است.
- محتویات سبد تغییر کرده است.
- سرویس Provider موقتاً در دسترس نیست.

## Test Mode

در وضعیت فعلی `Shipping:TestMode` فعال است. قیمت تستی توسط بک تولید و کل جریان Quote، انتخاب، پرداخت و ایجاد Shipment آزمایش می‌شود. برای اتصال عملیاتی هر شرکت باید مستند رسمی قرارداد آن شرکت روی Adapter مربوطه اعمال و سپس Test Mode خاموش شود.
