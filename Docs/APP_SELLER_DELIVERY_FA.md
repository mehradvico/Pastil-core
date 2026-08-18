# قرارداد فرانت روش‌های ارسال فروشگاه

این مستند برای پیاده‌سازی بخش «روش‌های ارسال» در اپ پاستیل تهیه شده است.

## علت خطای قبلی

فرانت شناسه‌های `35` تا `38` را برای نوع ارسال به‌صورت ثابت ارسال می‌کرد، درحالی‌که شناسه Codeها بعد از بازسازی دیتابیس تغییر کرده‌اند. شناسه Code داده‌ی دیتابیس است و نباید در فرانت ثابت نوشته شود.

## دریافت داینامیک انواع ارسال

```http
GET /api/Seller/Delivery/types
Authorization: Bearer {accessToken}
```

نمونه پاسخ:

```json
{
  "isSuccess": true,
  "messages": [],
  "code": 0,
  "data": [
    {
      "id": 88,
      "name": "پیک فروشگاه",
      "label": "DeliveryType_Courier",
      "priority": 1
    }
  ]
}
```

مقدار `id` پاسخ باید به‌عنوان `deliveryTypeId` ارسال شود. IDهای نمونه را در کد فرانت هاردکد نکنید.

Labelهای شناخته‌شده بک:

- `DeliveryType_Courier`: پیک
- `DeliveryType_Post`: پست
- `DeliveryType_Tipax`: تیپاکس
- `DeliveryType_InStore`: تحویل حضوری
- `DeliveryType_AloPeyk`: الوپیک
- `DeliveryType_SnappBox`: اسنپ‌باکس

عنوان قابل نمایش را از `name` بگیرید تا با تغییر نام Code در پنل، متن اپ نیز خودکار تغییر کند.

## ثبت روش ارسال

```http
POST /api/Seller/Delivery
Authorization: Bearer {accessToken}
Content-Type: application/json
```

```json
{
  "deliveryTypeId": 88,
  "basePrice": 80000,
  "minPriceForFree": 1000000,
  "minCountForFree": 0,
  "maxDays": 1,
  "stateId": null,
  "cityId": null,
  "active": true,
  "afterRent": false
}
```

نکات:

- `storeId` را ارسال نکنید؛ بک آن را فقط از کاربر لاگین‌شده استخراج می‌کند.
- تمام قیمت‌ها و تعدادها باید صفر یا بیشتر باشند.
- اگر `cityId` و `stateId` با هم ارسال شوند، شهر باید متعلق به همان استان باشد.
- برای هزینه پس‌کرایه، `afterRent` برابر `true` باشد؛ بک `basePrice` را صفر ذخیره می‌کند.
- نتیجه موفق فقط زمانی است که HTTP موفق و `isSuccess === true` باشد.

## ویرایش و حذف

ویرایش:

```http
PUT /api/Seller/Delivery
```

بدنه مشابه ثبت است و باید `id` روش ارسال داخل آن باشد.

حذف:

```http
DELETE /api/Seller/Delivery?id={deliveryId}
```

بک مالکیت روش ارسال را کنترل می‌کند؛ فروشنده نمی‌تواند روش ارسال فروشگاه دیگری را مشاهده، ویرایش یا حذف کند.

## نمایش خطای واقعی بک

پاسخ‌های اعتبارسنجی بک معمولاً HTTP 200 با `isSuccess: false` هستند. متن خطا از مسیر زیر خوانده شود:

```ts
const message = response?.messages?.[0]?.item1 || 'عملیات ناموفق بود'
```

در Server APIهای Nuxt فایل‌های `delivery.post.js` و `delivery.put.js`، شیء خام `error` را `return` نکنید. پاسخ بک را حفظ کنید یا با `createError` منتقل کنید:

```ts
catch (error) {
  throw createError({
    statusCode: error?.response?.status || error?.statusCode || 500,
    statusMessage: error?.data?.messages?.[0]?.item1 || 'خطا در ارتباط با سرور',
    data: error?.data
  })
}
```

## ترتیب پیشنهادی صفحه

1. هنگام باز شدن فرم، `GET /api/Seller/Delivery/types` اجرا شود.
2. DropDown از `data` ساخته شود و مقدار انتخاب‌شده `id` باشد.
3. قبل از Submit، انتخاب نوع ارسال و منفی نبودن مقادیر کنترل شود.
4. پس از Submit، علاوه بر HTTP Status حتماً `isSuccess` بررسی شود.
5. در خطا، پیام `messages[0].item1` به کاربر نمایش داده شود.
