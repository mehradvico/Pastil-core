# راهنمای Flutter: گزینه‌های سفر در سرویس پت‌رسان

در فرم ساخت سرویس پت‌رسان تکرارشونده، گزینه‌های سفر فعال را نمایش دهید و شناسه‌ی گزینه‌های انتخاب‌شده را در `tripOptionIds` ارسال کنید. سرور قیمت را محاسبه می‌کند؛ قیمت را در Flutter جمع نزنید.

## دریافت گزینه‌ها

```http
GET /api/EndUser/TripOption?Available=true&PageIndex=1&PageSize=100
Authorization: Bearer <access-token>
```

برای هر آیتم از `id`، `name` و `price` استفاده کنید. فقط گزینه‌های بازگشتی این endpoint را قابل انتخاب کنید.

## پیش‌نمایش قیمت

```http
POST /api/EndUser/PetResanService/preview-price
```

```json
{
  "userPetId": 55,
  "origin": { "x": 51.389, "y": 35.689 },
  "destination": { "x": 51.425, "y": 35.710 },
  "fromAddress": "...",
  "toAddress": "...",
  "tripOptionIds": [3, 8]
}
```

هر بار که مبدا، مقصد یا گزینه‌های سفر تغییر می‌کند، preview را با debounce کوتاه دوباره بگیرید. مبلغ `data` پاسخ، قیمت یک رفت‌وبرگشت است.

## ثبت سرویس

در همان payload ساخت سرویس، `tripOptionIds` را بفرستید:

```json
{
  "userPetId": 55,
  "origin": { "x": 51.389, "y": 35.689 },
  "destination": { "x": 51.425, "y": 35.710 },
  "fromAddress": "...",
  "toAddress": "...",
  "totalWeeks": 8,
  "tripOptionIds": [3, 8],
  "schedules": [
    { "weekDayId": 2, "time": "08:30" },
    { "weekDayId": 4, "time": "08:30" }
  ]
}
```

```http
POST /api/EndUser/PetResanService
Idempotency-Key: <uuid-v4>
Authorization: Bearer <access-token>
```

- `tripOptionIds` اختیاری است؛ برای نداشتن گزینه، `[]` بفرستید.
- شناسه‌های تکراری یا گزینه‌ی غیرفعال/حذف‌شده رد می‌شوند.
- کلید idempotency را در retry همان درخواست ثابت نگه دارید.

## پاسخ و نمایش جزئیات

پاسخ سرویس شامل `data.tripOptions` است. گزینه‌ها را در کارت سرویس و صفحه‌ی جزئیات نمایش دهید. سرور این انتخاب‌ها را نگه می‌دارد و هنگام ساخت هر سفر هفتگی، همان گزینه‌ها را به سفر منتقل و در قیمت آن لحاظ می‌کند.

## معیار پذیرش

1. گزینه‌های فعال از `TripOption` بارگذاری و قابل انتخاب باشند.
2. با انتخاب گزینه، مبلغ preview فقط از پاسخ سرور تغییر کند.
3. گزینه‌های ارسال‌شده پس از refresh در `data.tripOptions` باقی بمانند.
4. سفرهای ساخته‌شده از این سرویس، همان گزینه‌ها و قیمتِ شامل آن‌ها را داشته باشند.
