# مستند گروه‌بندی خدمات برای پنل

## تغییرات مدل

مدل جدید `AssistanceGroup`:

| فیلد | نوع | توضیح |
|---|---|---|
| `id` | number | شناسه |
| `name` | string | نام گروه، اجباری |
| `priority` | number | اولویت نمایش |
| `active` | boolean | وضعیت فعال |

فیلد جدید در `Assistance`:

| فیلد | نوع | توضیح |
|---|---|---|
| `assistanceGroupId` | number/null | شناسه گروه خدمات |
| `assistanceGroup` | object/null | اطلاعات گروه در پاسخ‌های نمایشی |

`assistanceGroupId` در بک‌اند nullable است تا خدمات قدیمی بدون مشکل باقی بمانند.
در فرم ثبت و ویرایش پنل بهتر است انتخاب گروه را برای کاربر پنل الزامی کنید.

## API مدیریت گروه‌ها

تمام endpointهای این بخش نیازمند توکن پنل هستند.

### جستجو

```http
GET /api/Admin/AssistanceGroup?pageIndex=1&pageSize=20&q=&available=true&sortBy=6
Authorization: Bearer {token}
```

پارامترها:

| پارامتر | نوع | توضیح |
|---|---|---|
| `pageIndex` | number | شماره صفحه |
| `pageSize` | number | تعداد در صفحه |
| `q` | string | جستجو در نام |
| `available` | boolean/null | فیلتر فعال یا غیرفعال |
| `sortBy` | number | `1` جدید، `2` قدیم، `3` نام، `6` اولویت بیشتر، `7` اولویت کمتر |

### دریافت یک گروه

```http
GET /api/Admin/AssistanceGroup/1
Authorization: Bearer {token}
```

### ثبت گروه

```http
POST /api/Admin/AssistanceGroup
Authorization: Bearer {token}
Content-Type: application/json
```

```json
{
  "name": "خدمات مراقبتی",
  "priority": 100,
  "active": true
}
```

### ویرایش گروه

```http
PUT /api/Admin/AssistanceGroup
Authorization: Bearer {token}
Content-Type: application/json
```

```json
{
  "id": 1,
  "name": "خدمات مراقبت از پت",
  "priority": 100,
  "active": true
}
```

### حذف گروه

```http
DELETE /api/Admin/AssistanceGroup?id=1
Authorization: Bearer {token}
```

حذف طبق ساختار پروژه Soft Delete است.

## تغییر فرم Assistance

در فرم ثبت و ویرایش خدمات، ابتدا گروه‌ها را دریافت کنید:

```http
GET /api/Admin/AssistanceGroup?pageIndex=1&pageSize=100&available=true&sortBy=6
```

سپس مقدار انتخاب‌شده را در `assistanceGroupId` ارسال کنید:

```http
POST /api/Admin/Assistance
Authorization: Bearer {token}
Content-Type: application/json
```

```json
{
  "name": "پت‌سیتر در منزل",
  "summary": "مراقبت از پت در منزل",
  "description": "...",
  "isPersonal": true,
  "assistanceGroupId": 1,
  "pictureId": 25,
  "active": true
}
```

برای ویرایش نیز همین فیلد همراه `id` به endpoint قبلی ارسال می‌شود:

```http
PUT /api/Admin/Assistance
```

## فیلتر خدمات در پنل

```http
GET /api/Admin/Assistance?assistanceGroupId=1&pageIndex=1&pageSize=20
Authorization: Bearer {token}
```

فیلترهای قبلی Assistance بدون تغییر هستند:

- `isPersonal`
- `available`
- `q`
- `sortBy`
- `pageIndex`
- `pageSize`

## Permission

کنترلر `AssistanceGroup` در گروه دسترسی `CompanionManagement` قرار گرفته است.
Permission اصلی این کنترلر با `IsMenu = false` ساخته می‌شود؛ بنابراین از آن برای کنترل دسترسی استفاده کنید ولی منوی مستقلی از روی Permission نسازید.

بعد از استقرار نسخه جدید و اجرای Migration، یک بار همگام‌سازی Permissionها را اجرا کنید:

```http
POST /api/Admin/PermissionSync
Authorization: Bearer {admin-token}
```

Actionهای ایجادشده شامل دسترسی‌های `Get`، `Post`، `Put` و `Delete` برای `AssistanceGroup` هستند.
