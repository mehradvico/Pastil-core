# مستند بخش خاطرات برای اپ پاستیل

این بخش برای ثبت خاطره روزانه کاربر همراه با یکی از پت‌های خودش است. تمام endpointهای اپ نیازمند Bearer Token هستند و کاربر فقط به خاطرات متعلق به خودش دسترسی دارد.

## مدل کلی

هر خاطره شامل موارد زیر است:

| فیلد | نوع | توضیح |
|---|---|---|
| `id` | number | شناسه خاطره |
| `text` | string | متن خاطره، اجباری و حداکثر ۴۰۰۰ کاراکتر |
| `memoryDate` | ISO datetime | تاریخ و ساعت وقوع خاطره |
| `createDate` | ISO datetime | زمان ثبت در سرور |
| `updateDate` | ISO datetime/null | آخرین زمان ویرایش |
| `pictureId` | number/null | شناسه تصویر آپلودشده |
| `picture` | object/null | اطلاعات کامل تصویر خاطره |
| `userPetId` | number | شناسه پت کاربر |
| `userPetName` | string | نام پت |
| `userPetPicture` | object/null | تصویر پت |
| `userId` | number | شناسه مالک خاطره |

`memoryDate` با فرمت ISO ارسال شود. تقویم فارسی فقط در UI استفاده شود و قبل از ارسال به ISO تبدیل گردد:

```text
2026-08-09T21:35:00+03:30
```

تاریخ آینده پذیرفته نمی‌شود، ولی کاربر می‌تواند خاطرات روزهای گذشته را ثبت کند.

## ۱. آپلود تصویر خاطره

ابتدا تصویر روی سرویس File آپلود شود:

```http
POST https://file.pastil.pet/api/PictureUpload
Content-Type: multipart/form-data
```

نام فیلد فایل:

```text
PictureFile
```

فرمت‌های تصویری مجاز `jpg`، `jpeg`، `png` و `webp` و حداکثر حجم ۵ مگابایت است. بعد از پاسخ موفق، مقدار `data.id` در `pictureId` خاطره قرار داده شود.

تصویر اختیاری است؛ برای خاطره بدون تصویر مقدار `pictureId` برابر `null` ارسال شود.

## ۲. ثبت خاطره

```http
POST /api/EndUser/Memory
Authorization: Bearer USER_TOKEN
Content-Type: application/json
```

```json
{
  "text": "امروز برای اولین بار با پاستیل به پارک رفتیم.",
  "memoryDate": "2026-08-09T19:20:00+03:30",
  "pictureId": 108,
  "userPetId": 12
}
```

کاربر نمی‌تواند با `userPetId` متعلق به کاربر دیگر خاطره ثبت کند. پت باید فعال و حذف‌نشده باشد.

نمونه پاسخ موفق:

```json
{
  "isSuccess": true,
  "messages": [],
  "data": {
    "id": 31,
    "text": "امروز برای اولین بار با پاستیل به پارک رفتیم.",
    "memoryDate": "2026-08-09T19:20:00+03:30",
    "createDate": "2026-08-09T20:01:12+03:30",
    "updateDate": null,
    "pictureId": 108,
    "picture": {
      "id": 108,
      "url": "/Media/2026/8/9",
      "guidName": "...",
      "extension": ".webp"
    },
    "userPetId": 12,
    "userPetName": "پاستیل"
  }
}
```

## ۳. دریافت لیست و جستجوی خاطرات

```http
GET /api/EndUser/Memory
Authorization: Bearer USER_TOKEN
```

پارامترهای Query:

| پارامتر | نمونه | توضیح |
|---|---|---|
| `pageIndex` | `1` | شماره صفحه، از ۱ |
| `pageSize` | `20` | تعداد هر صفحه، حداکثر ۱۰۰ |
| `sortBy` | `1` | `1` جدیدترین و `2` قدیمی‌ترین |
| `q` | `پارک` | جستجو در متن خاطره و نام پت |
| `userPetId` | `12` | فقط خاطرات یک پت |
| `date` | `2026-08-09` | خاطرات دقیقاً یک روز |
| `fromDate` | `2026-08-01` | شروع بازه تاریخ |
| `toDate` | `2026-08-31` | پایان بازه تاریخ به‌صورت شامل |

اگر `date` ارسال شود، `fromDate` و `toDate` نادیده گرفته می‌شوند.

نمونه‌ها:

```http
GET /api/EndUser/Memory?date=2026-08-09&pageIndex=1&pageSize=20
```

```http
GET /api/EndUser/Memory?userPetId=12&fromDate=2026-08-01&toDate=2026-08-31&sortBy=1
```

```http
GET /api/EndUser/Memory?q=پارک&pageIndex=1&pageSize=20
```

ساختار پاسخ Search:

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "q": null,
  "sortBy": 1,
  "totalCount": 3,
  "list": []
}
```

## ۴. دریافت جزئیات خاطره

```http
GET /api/EndUser/Memory/{id}
Authorization: Bearer USER_TOKEN
```

مثال:

```http
GET /api/EndUser/Memory/31
```

اگر خاطره متعلق به کاربر دیگری باشد، بک آن را مانند رکورد پیدا‌نشده برمی‌گرداند.

## ۵. ویرایش خاطره

```http
PUT /api/EndUser/Memory
Authorization: Bearer USER_TOKEN
Content-Type: application/json
```

تمام فیلدها همراه با `id` ارسال شوند:

```json
{
  "id": 31,
  "text": "متن ویرایش‌شده خاطره",
  "memoryDate": "2026-08-09T19:20:00+03:30",
  "pictureId": 110,
  "userPetId": 12
}
```

برای حذف تصویر از خاطره، `pictureId: null` ارسال شود.

## ۶. حذف خاطره

```http
DELETE /api/EndUser/Memory/{id}
Authorization: Bearer USER_TOKEN
```

حذف به‌صورت Soft Delete انجام می‌شود و خاطره پس از آن در لیست و جزئیات نمایش داده نمی‌شود.

## ۷. یادآوری شبانه Push

هر روز ساعت ۲۲ به وقت تهران، Hangfire یادآوری ثبت خاطره را بررسی می‌کند.

ساعت از تنظیم `Memory:ReminderHour` خوانده می‌شود و مقدار پیش‌فرض آن `22` است. روی Docker در صورت نیاز می‌توان از متغیر `Memory__ReminderHour` استفاده کرد.

Push فقط برای کاربری ساخته می‌شود که:

- حداقل یک پت فعال داشته باشد؛
- حسابش حذف یا قفل نشده باشد؛
- برای همان روز خاطره‌ای ثبت نکرده باشد؛
- در همان روز قبلاً Push یادآوری خاطره دریافت نکرده باشد.

متن Push:

```text
{نام کاربر} عزیز، خاطره امروزت با پتت رو در پاستیل ثبت کن.
```

مسیر کلیک Push:

```text
/memories
```

فرانت باید route بالا را به صفحه لیست/ثبت خاطرات متصل کند. برای دریافت Push، ثبت `PushSubscription` کاربر طبق فرایند Push فعلی اپ همچنان الزامی است.

## ۸. پیشنهاد UI برای اپ

- صفحه اصلی خاطرات با گروه‌بندی براساس روز.
- فیلتر پت و بازه تاریخ در بالای صفحه.
- دکمه «ثبت خاطره امروز».
- استفاده از DatePicker و TimePicker فعلی اپ برای `memoryDate`.
- استفاده از Component فعلی آپلود/نمایش تصویر.
- نمایش Placeholder برای خاطره بدون تصویر.
- بعد از ثبت، ویرایش یا حذف موفق، صفحه اول لیست دوباره دریافت شود.

## ۹. خطاهای مهم

- `متن خاطره الزامی است.`
- `متن خاطره حداکثر می‌تواند ۴۰۰۰ کاراکتر باشد.`
- `تاریخ و ساعت خاطره الزامی است.`
- `تاریخ خاطره نمی‌تواند مربوط به آینده باشد.`
- `پت انتخاب‌شده متعلق به این کاربر نیست.`
- `تصویر انتخاب‌شده پیدا نشد.`
- `خاطره موردنظر پیدا نشد.`

همیشه `isSuccess` بررسی شود و متن `messages` برای نمایش خطا استفاده گردد.

## ۱۰. Endpoint مدیریتی و Permission

برای مدیر دارای Permission، مشاهده و جستجوی خاطرات کاربران فراهم است:

```http
GET /api/Admin/UserMemory
GET /api/Admin/UserMemory/{id}
Authorization: Bearer ADMIN_TOKEN
```

در Search مدیریتی، علاوه بر فیلترهای قبلی، `userId` نیز قابل ارسال است. Permission کنترلر با نام `UserMemory` در گروه مدیریت کاربران و با `IsMenu=true` ساخته می‌شود و صفحه آن در پنل از مسیر `/admin/usermemory` در دسترس است. برای رعایت حریم خصوصی، endpoint مدیریتی فقط خواندنی است.

## ۱۱. دیتابیس و استقرار

Migration این بخش:

```text
20260809130224_AddUserPetMemories
```

این migration جداول `Memories` و `UserMemories`، indexها، رابطه‌ها و Push Type/Pattern/Setting یادآوری را ایجاد می‌کند. بعد از آپلود بک اجرا شود:

```powershell
Update-Database
```
