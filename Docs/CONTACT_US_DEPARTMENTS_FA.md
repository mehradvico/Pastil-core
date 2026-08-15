# مستند دپارتمان‌ها و فرم ارتباط با ما

## دریافت دپارتمان‌ها

```http
GET /api/ContactUsGroup?PageIndex=1&PageSize=50&SortBy=7
```

هر عضو `list` علاوه بر `id`، `name` و `label` دارای آرایه `formFields` است. فرانت باید فیلدهای تکمیلی فرم را از همین آرایه بسازد و هیچ فهرست ثابتی داخل کد نگه ندارد.

دپارتمان‌های پیش‌فرض:

| نام | Label |
|---|---|
| ارتباط با پاستیل | `contact-pastil` |
| درخواست راننده | `driver-request` |
| درخواست نمایندگی | `companion-request` |
| درخواست پت شاپ | `pet-shop-request` |
| درخواست محصول خاص | `special-product-request` |
| درخواست تبلیغات | `advertising-request` |

ساختار هر `formFields`:

```json
{
  "key": "city",
  "label": "شهر محل فعالیت",
  "inputType": "text",
  "required": true,
  "priority": 1,
  "maxLength": 100,
  "minValue": null,
  "placeholder": "مثلاً تهران",
  "options": []
}
```

مقادیر فعلی `inputType` شامل `text`، `number`، `url` و `select` است. برای `select` گزینه‌ها از `options` خوانده شوند.

## ثبت درخواست

```http
POST /api/ContactUs
Content-Type: application/json
```

فیلدهای مشترک:

| فیلد | وضعیت | توضیح |
|---|---|---|
| `fullName` | الزامی | حداکثر ۱۲۰ کاراکتر |
| `mobile` | الزامی | موبایل ایران با قالب `09xxxxxxxxx`؛ اعداد فارسی و `+98` در بک نرمال می‌شوند |
| `email` | اختیاری | ایمیل معتبر |
| `title` | الزامی | حداکثر ۱۵۰ کاراکتر |
| `body` | الزامی | شرح درخواست، حداکثر ۴۰۰۰ کاراکتر |
| `contactUsGroupId` | الزامی | شناسه دپارتمان فعال |
| `fileId` | اختیاری | شناسه فایل آپلودشده و معتبر |
| `contactUsItems` | وابسته به دپارتمان | پاسخ فیلدهای `formFields` |

در `contactUsItems` مقدار `title` باید دقیقاً برابر `key` فیلد دریافتی از API باشد؛ متن فارسی نمایشی در `value` یا `title` ارسال نشود.

نمونه درخواست راننده:

```json
{
  "fullName": "علی رضایی",
  "mobile": "09121234567",
  "email": "ali@example.com",
  "title": "درخواست همکاری به عنوان راننده",
  "body": "امکان فعالیت تمام‌وقت در تهران را دارم.",
  "contactUsGroupId": 12,
  "fileId": null,
  "contactUsItems": [
    { "title": "city", "value": "تهران" },
    { "title": "vehicleType", "value": "motorcycle" },
    { "title": "hasDriverLicense", "value": "yes" },
    { "title": "experienceYears", "value": "2" }
  ]
}
```

بک موارد زیر را کنترل می‌کند:

- دپارتمان باید موجود و فعال باشد.
- فیلدهای `required` باید ارسال شوند.
- کلید ناشناخته یا تکراری پذیرفته نمی‌شود.
- مقدار `select` باید یکی از `options.value` باشد.
- مقدار عددی باید حداقل برابر `minValue` همان فیلد باشد؛ برای تعداد محصول این مقدار ۱ است.
- مقدار URL باید با `http` یا `https` معتبر باشد.
- طول مقادیر و وجود `fileId` بررسی می‌شود.

در صورت موفقیت پاسخ دارای `isSuccess: true` است. در خطای اعتبارسنجی، `isSuccess: false` و پیام قابل نمایش در `messages`/`val` برمی‌گردد.
