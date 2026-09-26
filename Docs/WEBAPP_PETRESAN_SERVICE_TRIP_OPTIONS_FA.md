# راهنمای وب‌اپ: گزینه‌های سفر در سرویس پترسان

این راهنما مربوط به فرم «سرویس» در صفحه‌ی پترسان وب‌اپ است. کاربر برای یک سرویس رفت‌وبرگشت هفتگی، گزینه‌هایی مانند باکس حمل، تهویه یا خدمات جانبی را انتخاب می‌کند. انتخاب‌ها باید در همه‌ی سفرهای بعدی همان سرویس حفظ شوند.

## فایل‌های مرتبط

| فایل | مسئولیت |
| --- | --- |
| `webapp/app/pages/transportation/index.vue` | نمایش فرم سرویس، انتخاب گزینه‌ها، preview و ثبت |
| `webapp/app/server/api/trip/tripOption.get.js` | BFF دریافت کاتالوگ گزینه‌ها |
| `webapp/app/server/api/petResanService/previewPrice.post.ts` | BFF پیش‌نمایش قیمت سرویس |
| `webapp/app/server/api/petResanService/petResanService.post.ts` | BFF ثبت سرویس با idempotency key |

## دریافت گزینه‌های فعال

در `onMounted` صفحه، گزینه‌ها از BFF زیر دریافت می‌شوند:

```ts
const response = await $fetch('/api/trip/tripOption', {
  query: { PageIndex: 1, PageSize: 100, Available: true }
})
options.value = list(response)
```

فقط گزینه‌های برگشتی را قابل انتخاب کنید. فیلدهای اصلی هر گزینه `id`، `name` و `price` هستند. قیمت را صرفاً برای توضیح UI نشان دهید؛ مبلغ نهایی باید از پاسخ preview سرور خوانده شود.

## وضعیت انتخاب در فرم

وضعیت انتخاب‌ها در همان صفحه نگه‌داری می‌شود:

```ts
const tripOptionIds = ref<number[]>([])

const toggleOption = (id: number) => {
  tripOptionIds.value = tripOptionIds.value.includes(id)
    ? tripOptionIds.value.filter(item => item !== id)
    : [...tripOptionIds.value, id]
}
```

در حالت `tripMode === 'subscription'`، کارت‌های گزینه‌ها باید در فرم اختصاصی سرویس نمایش داده شوند؛ اما کنترل‌های «رفت‌وبرگشت» و «همراه پت می‌آیم» نمایش داده نمی‌شوند، چون رفت‌وبرگشت برای سرویس در سرور اجباری است.

## پیش‌نمایش قیمت

تابع ساخت payload سرویس باید `tripOptionIds` را حفظ کند:

```ts
const subscriptionBuildPayload = () => ({
  userPetId: subscriptionPetId.value,
  origin: { x: origin.value!.x, y: origin.value!.y },
  destination: { x: destination.value!.x, y: destination.value!.y },
  fromAddress: subscriptionOriginAddress.value.trim(),
  toAddress: subscriptionDestinationAddress.value.trim(),
  totalWeeks: isOpenEnded.value ? null : totalWeeks.value,
  tripOptionIds: [...tripOptionIds.value],
  schedules: subscriptionScheduleList.value,
})
```

هر تغییر در مبدا، مقصد، برنامه‌ی هفتگی یا `tripOptionIds` باید preview را با debounce اجرا کند:

```ts
watch(
  [origin, destination, subscriptionScheduleList, tripOptionIds],
  debouncePreview,
  { deep: true }
)
```

درخواست preview:

```http
POST /api/petResanService/previewPrice
```

مبلغ معتبر فقط `response.data` است. اگر پاسخ ناموفق بود یا قیمت صفر/منفی بود، دکمه‌ی فعال‌سازی سرویس باید غیرفعال بماند.

## ثبت سرویس

همان payload preview برای ثبت به کار می‌رود:

```ts
await requestCheckout(
  '/api/petResanService/petResanService',
  'petresan-weekly-service',
  payload,
  payload,
)
```

`requestCheckout` باید `Idempotency-Key` یکتای UUID را روی درخواست بگذارد. در retry همان عملیات، همان کلید حفظ می‌شود تا سرویس تکراری ساخته نشود.

## پاسخ و نمایش سرویس‌های ثبت‌شده

پاسخ `GET /api/petResanService/petResanService` برای هر سرویس فیلد زیر را برمی‌گرداند:

```json
{
  "tripOptions": [
    { "id": 3, "name": "باکس حمل", "price": 50000, "active": true }
  ]
}
```

در کارت سرویس‌های ثبت‌شده و صفحه‌ی جزئیات، نام گزینه‌های انتخاب‌شده را نمایش دهید. گزینه‌ها در بک‌اند به سرویس متصل می‌شوند و در هر سفر هفتگیِ ایجادشده، به همان سفر منتقل و در قیمت آن لحاظ می‌شوند.

## خطاها و حالت‌ها

- اگر گزینه تکراری ارسال شود، سرور پاسخ ناموفق می‌دهد؛ UI باید انتخاب‌ها را یکتا نگه دارد.
- اگر گزینه پس از بارگذاری غیرفعال یا حذف شده باشد، ثبت سرویس ناموفق است؛ کاتالوگ را دوباره دریافت و پیام قابل فهم نشان دهید.
- در خطای preview، قیمت قبلی را نمایش ندهید و اجازه‌ی ثبت ندهید.
- پس از ثبت موفق، `tripOptionIds` را reset کنید و فهرست سرویس‌ها را دوباره بگیرید.

## معیار پذیرش

1. در حالت سرویس هفتگی، گزینه‌های فعال سفر قابل انتخاب‌اند.
2. با انتخاب/حذف گزینه، preview از سرور تازه می‌شود و مبلغ جدید نمایش داده می‌شود.
3. payload ثبت شامل `tripOptionIds` است.
4. پس از refresh، گزینه‌ها در `service.tripOptions` باقی مانده‌اند.
5. قیمت نهایی هر سفر فقط از سرور تعیین می‌شود، نه با جمع‌زدن قیمت گزینه‌ها در کلاینت.
