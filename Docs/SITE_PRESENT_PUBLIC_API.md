# API عمومی سایت Present پاستیل

Base URL:

```text
https://api.pastil.pet
```

این APIها Public هستند و فقط داده‌های مجاز برای نمایش در سایت را برمی‌گردانند. فرانت نباید از APIهای Admin یا APIهای عمومی App برای سایت Present استفاده کند.

## Endpointها

```text
GET /api/Site/Banner
GET /api/Site/Banner/{id}

GET /api/Site/Companion
GET /api/Site/Companion/{id}

GET /api/Site/Assistance
GET /api/Site/Assistance/{id}

GET /api/Site/Pansion
GET /api/Site/Pansion/{id}

GET /api/Site/Store
GET /api/Site/Store/{id}

GET /api/Site/Post
GET /api/Site/Post/{id}

GET /api/Site/Gallery
GET /api/Site/Gallery/label/{label}
```

تمام endpointهای لیست از Queryهای Paging فعلی پشتیبانی می‌کنند:

```text
pageIndex=1
pageSize=20
q=
sortBy=0
```

فیلترهای امنیتی نمایش مانند `ShowToSite`, `Active` و `Approved` داخل بک اعمال می‌شوند و فرانت نیازی به ارسال آن‌ها ندارد.

## جداسازی Banner

- سایت Present فقط از `/api/Site/Banner` استفاده کند.
- App فعلی از `/api/Banner` استفاده می‌کند.
- `/api/Banner` فقط بنرهای `ShowToApp=true` را نمایش می‌دهد.
- `/api/Site/Banner` فقط بنرهای `ShowToSite=true` را نمایش می‌دهد.

## آدرس تصاویر

مقادیر `picture.url` و `picture.baseUrl` ممکن است Relative باشند. برای نمایش، Base URL فایل اضافه شود:

```text
https://file.pastil.pet
```

نمونه:

```text
https://file.pastil.pet/Media/2026/8/1/example.webp
```

