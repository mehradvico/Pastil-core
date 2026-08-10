# مستند Search V2 پاستیل

این نسخه با قرارداد قبلی سازگار است؛ فیلدهای گروهی قدیمی حذف نشده‌اند و خروجی یکپارچه‌ی `items` به آن اضافه شده است. جستجو میان محصول، دسته‌بندی، برند، ویژگی، نماینده، خدمت، فروشگاه، پانسیون و پکیج خدمات نماینده انجام می‌شود.

## API عمومی

```http
POST /api/Search
Content-Type: application/json
```

نمونه درخواست پیشنهادی برای اپ:

```json
{
  "q": "آرایش سگ",
  "totalCount": 20,
  "productCount": 5,
  "brandCount": 3,
  "categoryCount": 3,
  "featureCount": 3,
  "companionCount": 5,
  "assistanceCount": 5,
  "storeCount": 5,
  "pansionCount": 5,
  "packageCount": 5,
  "enableFuzzy": true
}
```

- `q`: اجباری، حداقل ۲ و حداکثر ۱۰۰ کاراکتر.
- تعداد هر نوع بین ۰ تا ۲۰ و `totalCount` بین ۱ تا ۵۰ محدود می‌شود.
- `productNotId`: در صفحات جزئیات برای حذف یک محصول مشخص از نتیجه قابل استفاده است.
- `enableFuzzy`: اصلاح جستجوی تقریبی، مترادف‌ها و خطای تایپ را فعال می‌کند و بهتر است روشن بماند.

نمونه ساختار پاسخ:

```json
{
  "isSuccess": true,
  "data": {
    "query": "آرایش سگ",
    "normalizedQuery": "آرایش سگ",
    "totalCount": 12,
    "tookMilliseconds": 84,
    "suggestions": ["اصلاح", "گرومینگ"],
    "items": [
      {
        "type": 9,
        "id": 42,
        "title": "پکیج اصلاح کامل",
        "subTitle": "نماینده مهراد - آرایش حیوانات - 500000",
        "picture": {},
        "score": 93.5,
        "url": "/companion-assistance-package/42",
        "matchedBy": "title,semantic"
      }
    ],
    "packages": [
      {
        "id": 42,
        "name": "پکیج اصلاح کامل",
        "price": 500000,
        "prePaymentPrice": 100000,
        "companionAssistanceId": 18,
        "companionId": 7,
        "companionName": "نماینده مهراد",
        "assistanceId": 3,
        "assistanceName": "آرایش حیوانات",
        "description": "...",
        "picture": {}
      }
    ]
  }
}
```

مقادیر `type` در `items`:

| مقدار | نوع |
|---:|---|
| 1 | Product |
| 2 | Category |
| 3 | Brand |
| 4 | FeatureItem |
| 5 | Companion |
| 6 | Assistance |
| 7 | Store |
| 8 | Pansion |
| 9 | CompanionAssistancePackage |

برای پیاده‌سازی جدید، ترتیب نمایش را از `items` بگیرید؛ این آرایه از قبل براساس ارتباط با عبارت، کیفیت تطبیق و اطلاعاتی مانند امتیاز کسب‌وکار مرتب شده است. آرایه‌های `products`، `companions`، `packages` و سایر گروه‌ها برای سازگاری و نمایش سکشن‌های مستقل باقی مانده‌اند.

## رفتار هوشمند جستجو

- یکسان‌سازی حروف فارسی/عربی، نیم‌فاصله، اعراب و ارقام فارسی و انگلیسی.
- جستجو در چند فیلد مرتبط؛ برای نمونه محصول در نام، نام دوم، لیبل، برند، دسته‌بندی و ویژگی‌ها.
- مترادف‌های رایج مانند پت‌شاپ/فروشگاه، پانسیون/اقامتگاه و اصلاح/آرایش/گرومینگ.
- تبدیل متن تایپ‌شده با صفحه‌کلید انگلیسی به فارسی.
- تحمل خطای تایپی و رتبه‌بندی تقریبی.
- حذف رکوردهای غیرفعال، حذف‌شده یا تأییدنشده. این API مخصوص اپ است و به `ShowToSite` وابسته نیست.
- محدودیت نرخ برای هر IP: سی درخواست در دقیقه.

## جستجوی Hybrid اختیاری

جستجوی واژگانی همیشه فعال است. در صورت داشتن سرویس embedding/reranker، این متغیرها در فایل env سرویس API تنظیم می‌شوند:

```env
PASTIL_SEARCH_HYBRID_ENABLED=true
PASTIL_SEARCH_HYBRID_ENDPOINT=https://your-service.example/rerank
PASTIL_SEARCH_HYBRID_API_KEY=your-secret
PASTIL_SEARCH_HYBRID_WEIGHT=0.25
```

درخواست ارسالی به reranker:

```json
{
  "query": "آرایش سگ",
  "items": [
    { "type": "CompanionAssistancePackage", "id": 42, "title": "...", "subTitle": "..." }
  ]
}
```

پاسخ مورد انتظار:

```json
{
  "scores": [
    { "type": "CompanionAssistancePackage", "id": 42, "score": 0.94 }
  ]
}
```

امتیاز باید بین صفر و یک باشد. timeout یا خرابی این سرویس باعث خرابی Search نمی‌شود و نتیجه واژگانی به‌صورت خودکار برگردانده می‌شود. تا زمان آماده بودن سرویس خارجی مقدار `Enabled` باید `false` بماند.

## آمار جستجو برای پنل

```http
GET /api/Admin/SearchAnalytics?days=30&take=50&zeroResultOnly=false
Authorization: Bearer ADMIN_TOKEN
```

- `days`: از ۱ تا ۳۶۵.
- `take`: از ۱ تا ۲۰۰.
- `zeroResultOnly=true`: فقط عبارت‌های بدون نتیجه را نمایش می‌دهد.
خروجی شامل عبارت، تعداد جستجو، تعداد دفعات بدون نتیجه، میانگین تعداد نتیجه، میانگین زمان پاسخ و آخرین زمان جستجو است. Permission این کنترلر با نام `SearchAnalytics` و `IsMenu=false` در گروه تنظیمات ساخته می‌شود.

## دیتابیس و استقرار

Migration زیر جدول سبک `SearchQueryLogs` و indexهای موردنیاز آمار را می‌سازد:

```text
20260809113327_AddSearchV2Analytics
```

بعد از آپلود بک‌اند اجرا شود:

```powershell
Update-Database
```

یا در CLI:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project Persistence --startup-project Api
```

اگر API قبل از اجرای migration بالا بیاید، اصل جستجو همچنان کار می‌کند و فقط ثبت آمار تا زمان اجرای migration انجام نمی‌شود.
