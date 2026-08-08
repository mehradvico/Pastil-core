# مستند گروه‌بندی خدمات برای سایت

## هدف

خدمات (`Assistance`) می‌توانند به یک گروه خدمات (`AssistanceGroup`) متصل شوند.
برای دریافت خدمات هر گروه، مقدار `assistanceGroupId` را در Search خدمات ارسال کنید.

ارتباط با گروه فعلاً nullable است تا خدمات قدیمی که هنوز گروه‌بندی نشده‌اند همچنان قابل نمایش باشند.

## دریافت گروه‌های فعال

```http
GET /api/AssistanceGroup?pageIndex=1&pageSize=100&sortBy=6
```

این endpoint عمومی است و بک‌اند فقط گروه‌های فعال و حذف‌نشده را برمی‌گرداند.

پارامترهای قابل استفاده:

| پارامتر | نوع | توضیح |
|---|---|---|
| `pageIndex` | number | شماره صفحه، پیش‌فرض `1` |
| `pageSize` | number | تعداد در صفحه، پیش‌فرض `20` |
| `q` | string | جستجو در نام گروه |
| `sortBy` | number | نحوه مرتب‌سازی |

مقادیر مهم `sortBy`:

| مقدار | ترتیب |
|---|---|
| `1` | جدیدترین |
| `2` | قدیمی‌ترین |
| `3` | نام |
| `6` | اولویت بیشتر |
| `7` | اولویت کمتر |

نمونه پاسخ:

```json
{
  "pageIndex": 1,
  "pageSize": 100,
  "q": null,
  "sortBy": 6,
  "available": true,
  "totalCount": 2,
  "list": [
    {
      "id": 1,
      "name": "خدمات مراقبتی",
      "priority": 100,
      "active": true
    }
  ]
}
```

## دریافت خدمات یک گروه

```http
GET /api/Assistance?assistanceGroupId=1&pageIndex=1&pageSize=20
```

پارامتر جدید:

| پارامتر | نوع | اجباری | توضیح |
|---|---|---|---|
| `assistanceGroupId` | number | خیر | شناسه گروه برای فیلتر خدمات |

فیلترهای قبلی مانند `isPersonal`، `q`، صفحه‌بندی و مرتب‌سازی بدون تغییر هستند.

نمونه پاسخ هر خدمت:

```json
{
  "id": 10,
  "name": "پت‌سیتر در منزل",
  "summary": "مراقبت از پت در منزل",
  "description": "...",
  "isPersonal": true,
  "assistanceGroupId": 1,
  "pictureId": 25,
  "active": true,
  "assistanceGroup": {
    "id": 1,
    "name": "خدمات مراقبتی",
    "priority": 100,
    "active": true
  },
  "picture": {}
}
```

## روند پیشنهادی در سایت

1. گروه‌ها را از `GET /api/AssistanceGroup` دریافت کنید.
2. گروه‌ها را بر اساس خروجی API نمایش دهید.
3. بعد از انتخاب گروه، شناسه آن را با `assistanceGroupId` به `GET /api/Assistance` بدهید.
4. برای گزینه «همه خدمات»، پارامتر `assistanceGroupId` را ارسال نکنید.

اگر یک گروه غیرفعال یا حذف شود، خدمات آن گروه در endpoint عمومی سایت نمایش داده نمی‌شوند.
خدمات قدیمی که هنوز گروه ندارند در حالت «همه خدمات» قابل نمایش باقی می‌مانند.
