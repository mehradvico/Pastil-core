# طرح پیاده‌سازی بک‌اند «تطبیق هوشمند محصولات» (AiProductMatch)

**رابطه با سند اصلی:** این سند مکمل `PRODUCT_IMPORT_BACKEND_FA.md` (سند قرارداد/دیدگاه محصولی، دریافت‌شده از تیم فرانت) است، نه جایگزین آن. آن سند **چه چیزی** باید ساخته شود را از دید مصرف‌کننده (فرانت) توضیح می‌دهد؛ این سند بعد از تطبیق دقیق با کد فعلی بک‌اند، **چطور** و **دقیقاً کجا** ساخته شود را مشخص می‌کند — شامل چند اصلاح ضروری نسبت به سند اصلی، و تصمیماتی که با صاحب محصول نهایی شده‌اند (بخش ۰).

> این سند از خواندن مستقیم کد فعلی (`Application/Services/PastilAISrv/Provider/*`, `Application/Services/CompanionSrvs`, `Api/Areas/Seller/Controllers/*`, `Entities/Entities/Product*`, `File/Controllers/PictureUploadController.cs`) نتیجه‌گیری شده است.

---

## ۰. اصلاحات و تصمیمات نهایی‌شده (قبل از شروع حتماً بخوانید)

### ۰.۱ کلید Gemini از قبل وجود دارد — دوباره نسازید

سند اصلی پیشنهاد می‌دهد کلید جدید با نام‌های `Gemini__ApiKey` / `Gemini__Model` / `Gemini__BaseUrl` ساخته شود. **این اشتباه است.** پروژه از قبل یک زیرساخت کامل چندمدلی (PastilAI) دارد که Gemini در آن پیکربندی‌شده و در Production فعال است:

- بخش `PastilAI:Providers` در `backend/Api/appsettings.json` (و `appsettings.Development.json`) از قبل یک ورودی با `"Name": "Gemini"`, `"Kind": "Gemini"`, `"BaseUrl": "https://generativelanguage.googleapis.com/v1beta"`, `"TextModel"/"VisionModel": "gemini-2.5-flash"`, `"SupportsImage": true` دارد.
- مقدار واقعی کلید از متغیر محیطی **`PASTIL_AI_GEMINI_API_KEY`** خوانده می‌شود (دقیقاً همان چیزی که در `backend/Docs/ENV_AND_GIT_SECURITY_FA.md` به‌عنوان یکی از متغیرهای موجود در `.env.example` مستند شده).

**تصمیم:** فیچر جدید از همین کلید/Provider موجود استفاده می‌کند. هیچ متغیر محیطی یا appsetting جدیدی برای خودِ کلید ساخته نمی‌شود — فقط یک بخش تنظیمات کوچک برای انتخاب مدل/محدودیت‌های حجم فایل مخصوص این فیچر اضافه می‌شود (بخش ۲).

### ۰.۲ کدهای HTTP سند اصلی با قرارداد فعلی پروژه هم‌خوانی ندارد

سند اصلی جدولی از کدهای HTTP (400/401/403/413/422/429/502/503) پیشنهاد داده. اما **قرارداد کل پروژه** (تایید‌شده در `docs/ai/DEVELOPMENT_RULES.md` و دیده‌شده در تمام کنترلرهای بررسی‌شده، از جمله همین `PictureUploadController` که خودش یک Endpoint آپلود فایل است) این است:

> تقریباً همیشه HTTP 200 برگردانده می‌شود؛ نتیجه فقط از `isSuccess` و فیلد عددی `code` داخل `BaseResultDto` خوانده می‌شود.

حتی خودِ Endpoint آپلود تصویر فعلی (`File/Controllers/PictureUploadController.cs`) برای فایل نامعتبر، حجم زیاد یا فرمت اشتباه، `return Ok(new BaseResultDto(false, Resource.Notification.FileNotAllow))` برمی‌گرداند — **نه** یک وضعیت 400 یا 422 واقعی.

**تصمیم:** این فیچر هم از همین قرارداد پیروی می‌کند، با دو استثنای ساختاری که در خودِ ASP.NET Core Middleware اتفاق می‌افتند و کنترل ما نیستند:

| کد HTTP سند اصلی | رفتار واقعی در این پیاده‌سازی |
|---|---|
| 401 | همان‌طور می‌ماند — از `[Authorize]` می‌آید (قبل از رسیدن به اکشن، توسط Middleware) |
| 413 | همان‌طور می‌ماند — از `[RequestFormLimits]`/`[RequestSizeLimit]` می‌آید (دقیقاً مثل الگوی `PictureUploadController`) |
| 400, 403, 422, 429, 502, 503 | **همه HTTP 200 با `isSuccess:false`**؛ تمایز با فیلد عددی `code` (جدول کامل در بخش ۸) |

فرانت (Flutter) باید طبق این جدول، منطق تشخیص خطا را روی `code` بسازد نه روی وضعیت HTTP — دقیقاً مثل تمام فلوهای دیگر پروژه (مثل رزرو، مستند‌شده در `reservation.md`).

### ۰.۳ منبع «تنوع‌های پیشنهادی» — نهایی شد: کل کاتالوگ

اسکیمای واقعی محصول (`Product`/`ProductItem`/`Variety`/`VarietyItem`) دو سطحی و **سراسری** است: `Variety`/`VarietyItem` مقادیر عمومی کاتالوگ‌اند (نه مخصوص یک فروشگاه)، ولی `ProductItem` (قیمت/موجودی واقعی) به‌ازای هر `StoreId` جداست.

**تصمیم نهایی (تایید صاحب محصول): گزینه‌ی «کل کاتالوگ».** `matches[].productItems` شامل همه‌ی `VarietyItem`/`VarietyItem2` معتبر برای `Product.VarietyId`/`Variety2Id` است، حتی اگر فروشگاه فعلی هنوز آن مورد خاص را ندارد. اگر فروشگاه آن تنوع را ندارد، فرانت یک `ProductItem` تازه برایش می‌سازد (از طریق همان `POST /api/Seller/ProductItem` موجود؛ نگاه کنید به ۰.۴).

### ۰.۴ خروجی می‌تواند شامل ساخت `ProductItem` جدید باشد، نه فقط تطبیق

نتیجه‌ی مستقیم تصمیم ۰.۳: بعد از تایید فروشنده، اگر تنوع پیشنهادی برای فروشگاهش قبلاً وجود ندارد، فرانت باید یک آیتم جدید در `ProductItemListUpdateDto.ProductItems` (با `Id=0`) به همان `POST /api/Seller/ProductItem` بفرستد. این تغییری در کد فعلی آن Endpoint نیاز ندارد (از قبل از این الگو پشتیبانی می‌کند)، فقط باید در مستند فرانت (بخش ۴ سند اصلی) توضیح داده شود.

### ۰.۵ بدون سقف مصرف روزانه — تصمیم نهایی

تایید صاحب محصول: **هیچ سقف مصرف روزانه‌ای وجود ندارد؛ همه‌ی فروشندگان بدون محدودیت به این قابلیت دسترسی دارند.** بنابراین هیچ زیرسیستم سهمیه/شمارنده‌ای ساخته نمی‌شود — نه Entity جدید، نه Migration، نه بررسی سهمیه در سرویس. طبق قاعده‌ی «برای سناریویی که رخ نمی‌دهد اعتبارسنجی ننویس»، از پیاده‌سازی آن صرف‌نظر می‌شود. (اگر در آینده نیاز به کنترل هزینه‌ی API پیش آمد، الگوی آماده‌اش در کد فعلی پروژه هست: `PastilAiDailyUsage` + `PastilAiQuotaPolicy` — می‌توان بعداً و جدا اضافه کرد.)

### ۰.۶ عکس‌های قفسه نگه‌داشته می‌شوند — از طریق سرویس جدای File آپلود می‌شوند، نه در‌جا در Api

تایید صاحب محصول: عکس‌های ارسالی برای تحلیل باید بعداً هم قابل بازبینی باشند (برای رفع اختلاف/عیب‌یابی).

**نکته‌ی معماری مهم:** با این‌که `IPictureService` در پروژه‌ی مشترک `Application` تعریف شده (که هم `Api` و هم `File` به آن ارجاع می‌دهند)، خودِ `IPictureService.InsertAsyncDto(PictureDto)` فقط **متادیتا** را در دیتابیس ثبت می‌کند؛ پردازش واقعی تصویر (رمزگذاری مجدد با ImageSharp به WebP، ساخت تصویر بندانگشتی، نوشتن بایت‌ها روی دیسک زیر `wwwroot/Media/...`) مستقیماً داخل `File/Controllers/PictureUploadController.cs` انجام می‌شود. چون `Api` و `File` در Production **دو Container/دامنه‌ی کاملاً جدا** هستند (`Urls:ApiBaseUrl = https://api.pastil.pet` در برابر `Urls:FileBaseUrl = https://file.pastil.pet`، هر کدام `wwwroot` مستقل خودشان)، اگر این منطق نوشتن روی دیسک داخل `Api` هم تکرار شود، فایل روی دیسک Container اشتباه نوشته می‌شود و آدرس نهایی (`https://file.pastil.pet/Media/...`) صفحه‌ی ۴۰۴ می‌دهد.

**تصمیم:** `AiProductMatchService` هر تصویر ورودی را با یک فراخوانی HTTP واقعی (نه فراخوانی درون‌فرایندی) به همان Endpoint موجود می‌فرستد:

```
POST {Urls:FileBaseUrl}/api/PictureUpload
Authorization: Bearer <همان توکن فروشنده که در درخواست اصلی آمده>
Content-Type: multipart/form-data
```

توکن کاربر جاری از `HttpContext` عبور داده می‌شود (Forward می‌شود)، دقیقاً چون این Endpoint خودش `[Authorize]` است و باید بداند همین فروشنده مالک آپلود است. پاسخ (`PictureDto`، شامل `id`) به‌عنوان `SourcePictureId` روی هر ردیف نتیجه ذخیره می‌شود (بخش ۶) — چون خودِ رکورد `Picture` برای همیشه در دیتابیس می‌ماند، **هیچ Entity یا جدول جدیدی برای «نگه‌داشتن عکس‌ها» لازم نیست**؛ فقط ارجاع به همان `PictureId` در پاسخ کافی است تا بعداً قابل بازبینی باشد.

---

## ۱. نمای کلی معماری فیچر

```
Seller App (Flutter)
    │
    │  POST /api/Seller/AiProductMatch/analyze  (multipart)
    ▼
AiProductMatchController  (Api/Areas/Seller/Controllers، جدید)
    │
    ▼
AiProductMatchService  (Application/Services/ProductSrvs/AiProductMatchSrv، جدید)
    ├─ ۱. اعتبارسنجی ورودی (sourceType، حداقل یکی از images/rowsJson)
    ├─ ۲. اگر images دارد → آپلود هرکدام به File Service (POST {FileBaseUrl}/api/PictureUpload) → PictureId
    ├─ ۳. اگر rowsJson دارد → پارس و اعتبارسنجی ساختاری (بدون تغییر قیمت/تعداد توسط AI)
    ├─ ۴. ساخت لیست کاندید از کاتالوگ  (ProductService.SearchMinAsync موجود، بازاستفاده)
    ├─ ۵. ساخت Prompt + فراخوانی Gemini  (PastilAiCompletionRouter، با یک متد سطح‌پایین تازه)
    ├─ ۶. پارس پاسخ ساختاریافته‌ی Gemini  (Parser جدید، جدا از ParseModelOutput فعلی چت)
    └─ ۷. نگاشت نتیجه به ProductId/ProductItemId واقعی پاستیل + ساخت پاسخ نهایی
```

---

## ۲. تنظیمات (Options + appsettings)

### ۲.۱ کلاس تنظیمات جدید

```csharp
// backend/Application/Common/Configuration/AiProductMatchOptions.cs
namespace Application.Common.Configuration
{
    public class AiProductMatchOptions
    {
        public const string SectionName = "AiProductMatch";

        public bool Enabled { get; set; } = true;
        public string ProviderName { get; set; } = "Gemini";          // به کدام Provider از PastilAI:Providers ارجاع می‌دهد
        public int MaxImagesPerRequest { get; set; } = 8;
        public int MaxImageSizeBytes { get; set; } = 8 * 1024 * 1024;  // 8MB به‌ازای هر تصویر (پیش از Forward به File Service)
        public int CandidateShortlistSize { get; set; } = 25;         // چند محصول به‌عنوان کاندید به Gemini فرستاده شود
        public int RequestTimeoutSeconds { get; set; } = 45;
    }
}
```

### ۲.۲ افزودن به `appsettings.json` (هر دو محیط)

```json
"AiProductMatch": {
  "Enabled": true,
  "ProviderName": "Gemini",
  "MaxImagesPerRequest": 8,
  "MaxImageSizeBytes": 8388608,
  "CandidateShortlistSize": 25,
  "RequestTimeoutSeconds": 45
}
```

هیچ کلید مخفی‌ای اینجا نیست (مطابق ۰.۱) — پس این بخش نیازی به Sanitize شدن در `Sanitize-AppSettings.ps1` ندارد.

### ۲.۳ ثبت در DI

`backend/Application/Configures/ConfigureServices.cs`:
```csharp
services.Configure<AiProductMatchOptions>(configuration.GetSection(AiProductMatchOptions.SectionName));
services.AddHttpClient<IAiProductMatchFileClient, AiProductMatchFileClient>();
services.AddScoped<IAiProductMatchService, AiProductMatchService>();
```

---

## ۳. آپلود و نگهداری تصاویر (فراخوانی سرویس File)

```csharp
// backend/Application/Services/ProductSrvs/AiProductMatchSrv/IAiProductMatchFileClient.cs
public interface IAiProductMatchFileClient
{
    // authorizationHeaderValue = دقیقاً همان مقداری که در هدر Authorization درخواست اصلی آمده
    // ("Bearer xxx")، بدون تغییر Forward می‌شود.
    Task<(bool Ok, long? PictureId, string Error)> UploadAsync(
        IFormFile image, string authorizationHeaderValue, CancellationToken cancellationToken);
}
```

پیاده‌سازی: یک `multipart/form-data` واقعی به `{Urls:FileBaseUrl}/api/PictureUpload` می‌سازد (همان فیلد فرمی که خودِ `PictureUploadController` انتظار دارد)، هدر `Authorization` را ست می‌کند، و از پاسخ فقط `data.id` (شناسه‌ی `Picture` تازه‌ساخته‌شده) را برمی‌گرداند. اگر پاسخ `isSuccess:false` بود (مثلاً فرمت فایل رد شد)، همان پیام را به بالادست پاس می‌دهد تا در `issues[]` همان ردیف قرار بگیرد — یک تصویر رد‌شده کل درخواست را متوقف نمی‌کند.

---

## ۴. لایه‌ی فراخوانی Gemini — استخراج بخش مشترک بدون تغییر رفتار PastilAI فعلی

### ۴.۱ مشکل فعلی

`PastilAiCompletionRouter.CallGeminiAsync` (`Provider/PastilAiCompletionRouter.cs:256-336`) هم ساخت درخواست HTTP و هم پارس پاسخ را با هم انجام می‌دهد، و `ParseModelOutput` انتهای آن فقط شکل `{answer, scope, isEmergency}` چت پاستیل‌AI را می‌فهمد. این فیچر به یک JSON کاملاً متفاوت (لیست کالاهای تطبیق‌یافته) نیاز دارد.

### ۴.۲ راه‌حل: جدا کردن HTTP Call از Parse (بدون دست‌زدن به مسیر فعلی چت)

داخل همان کلاس `PastilAiCompletionRouter`، منطق ساخت + ارسال درخواست HTTP به Gemini (خطوط تقریبی ۲۵۶ تا قبل از پارس پاسخ) به یک متد عمومی جدید استخراج شود:

```csharp
// امضای پیشنهادی — فقط ساخت/ارسال HTTP و برگرداندن متن خام JSON پاسخ Gemini،
// بدون هیچ فرضی درباره‌ی شکل داخلی آن JSON (پارس بر عهده‌ی فراخوان‌کننده است)
public async Task<string> CallGeminiRawAsync(
    PastilAiProviderDefinition provider,
    string systemInstruction,
    string userText,
    IReadOnlyList<string> mediaDataUrls,   // هر کدام data:<mime>;base64,<data> — می‌تواند چند تصویر باشد
    string model,                          // provider.VisionModel اگر تصویر دارد، وگرنه provider.TextModel
    bool requestJsonResponse,
    CancellationToken cancellationToken)
```

مسیر فعلی `CompleteAsync` (چت پاستیل‌AI) بدون تغییر رفتار، فقط از همین متد جدید به‌جای کد تکراری داخلی استفاده می‌کند (Refactor خالص، بدون تغییر ورودی/خروجی عمومی آن؛ چت همیشه حداکثر یک رسانه می‌فرستد، پس با `mediaDataUrls` تک‌عضوی فراخوانی می‌شود).

### ۴.۳ Parser جدید مخصوص این فیچر (در سرویس جدید، نه در Router)

```csharp
// backend/Application/Services/ProductSrvs/AiProductMatchSrv/AiProductMatchGeminiResponseParser.cs
public static class AiProductMatchGeminiResponseParser
{
    // ورودی: متن خام JSON برگشتی از CallGeminiRawAsync
    // خروجی: لیست تطبیق‌های خام (rowId, detectedName, externalCode, price, quantity, unit,
    //         candidateIndex با confidence) — هنوز به ProductId/ProductItemId واقعی نگاشت نشده
    public static List<AiProductMatchRawResult> Parse(string rawJson);
}
```

طراحی Prompt باید صراحتاً از Gemini بخواهد فقط بین **اندیس‌های لیست کاندید ارسالی** (نه نام آزاد محصول) انتخاب کند — همان تکنیکی که سند اصلی در بخش «کل کاتالوگ نباید ارسال شود» اشاره کرده. یعنی به هر کاندید یک `candidateIndex` عددی کوچک (0، 1، 2...) نسبت می‌دهیم، از Gemini می‌خواهیم فقط این اندیس یا `null` (اگر هیچ‌کدام مطابقت ندارد) را برگرداند، و بعد خودمان اندیس را به `ProductId`/`ProductItemId` واقعی نگاشت می‌کنیم. این کار احتمال Hallucination شناسه‌های ساختگی را عملاً به صفر می‌رساند (بند ۷ چک‌لیست پذیرش سند اصلی: «فقط ProductId و ProductItemId معتبر پاستیل برگردند»).

---

## ۵. ساخت لیست کاندید از کاتالوگ (بازاستفاده از `SearchMinAsync`)

### ۵.۱ چرا این و نه جست‌وجوی جدید

`ProductService.SearchMinAsync(SearchRequestDto, CancellationToken)` از قبل یک کوئری SQL خام با `STRING_SPLIT` روی کلیدواژه‌ها و رتبه‌بندی بر اساس تعداد کلیدواژه‌ی مطابق دارد — دقیقاً همان «جست‌وجوی سریع و تقریبی روی نام» که برای کوتاه‌کردن فهرست قبل از فرستادن به AI لازم است. نیازی به نوشتن SQL جدید نیست.

### ۵.۲ گام‌ها در `AiProductMatchService.AnalyzeAsync`

1. برای هر ردیف ورودی (چه از `rowsJson.name` چه از نامی که در تصویر تشخیص داده می‌شود — نکته: برای منبع `shelf`، تشخیص اولیه‌ی نام هم توسط همین فراخوانی Gemini انجام می‌شود، پس ترتیب واقعی این است: تصویر خام → Gemini یک‌بار برای استخراج نام‌های خام از عکس → سپس برای هر نام خام، `SearchMinAsync` برای کاندید → سپس یک فراخوانی دومِ Gemini برای انتخاب نهایی بین کاندیدها. برای منبع‌های `excel`/`sepidar`/`veterinary` که نام از قبل در `rowsJson` مشخص است، مرحله‌ی اول Gemini لازم نیست و مستقیم از `rowsJson.name` شروع می‌شود).
2. `SearchMinAsync` با `request.SearchTerms = نام خام`, محدود به `_options.Value.CandidateShortlistSize` نتیجه.
3. برای هر کاندید، `Product.Name`, `Product.BrandId → Brand.Name`, `Product.CodeValue`، و فهرست تنوع‌های معتبر (طبق تصمیم ۰.۳، از `Variety`/`VarietyItem` مرتبط با `Product.VarietyId`/`Variety2Id`، نه فقط `ProductItem`های همین فروشگاه) بارگذاری شود.
4. این فهرست کوتاه (نه کل کاتالوگ) به‌همراه نام خام، به مرحله‌ی انتخاب نهایی Gemini فرستاده می‌شود.

### ۵.۳ نکته درباره‌ی `externalCode`

اسکیمای فعلی `Product` هیچ فیلد اختصاصی بارکد/GTIN ندارد — نزدیک‌ترین فیلد `Product.CodeValue` (رشته) است. اگر `externalCode` ورودی (کد انبار سپیدار/دامپزشکیار) با `CodeValue` پاستیل یکی نبود (که در عمل اغلب همین‌طور خواهد بود، چون این کد داخلیِ نرم‌افزار انبار فروشنده است، نه بارکد استاندارد)، `externalCode` فقط برای **نمایش در پاسخ به فرانت** استفاده می‌شود (کاربر ببیند کد داخلی خودش هم منعکس شده)، نه به‌عنوان کلید جست‌وجو. جست‌وجوی کاندید همیشه بر پایه‌ی نام است.

---

## ۶. DTOهای دقیق

```csharp
// backend/Application/Services/ProductSrvs/AiProductMatchSrv/Dto/AiProductMatchStatusDto.cs
public class AiProductMatchStatusDto
{
    public bool Available { get; set; }
    public string Provider { get; set; }
}
```

```csharp
// AiProductMatchAnalyzeInputDto.cs  — از multipart/form-data بایند می‌شود
public class AiProductMatchAnalyzeInputDto
{
    public string SourceType { get; set; }         // "shelf" | "excel" | "sepidar" | "veterinary"
    public List<IFormFile> Images { get; set; }     // اختیاری
    public string RowsJson { get; set; }            // اختیاری، رشته‌ی JSON خام
    public string Currency { get; set; } = "IRT";
}
```

```csharp
// AiProductMatchRowInputDto.cs — نتیجه‌ی پارس RowsJson
public class AiProductMatchRowInputDto
{
    public string RowId { get; set; }
    public string Name { get; set; }
    public string ExternalCode { get; set; }
    public double? Price { get; set; }
    public int? Quantity { get; set; }
    public string Unit { get; set; }
}
```

```csharp
// AiProductMatchResultItemDto.cs — هر آیتم در data.items[]
public class AiProductMatchResultItemDto
{
    public string RowId { get; set; }
    public string DetectedName { get; set; }
    public string ExternalCode { get; set; }
    public double? Price { get; set; }
    public int? Quantity { get; set; }
    public long? ProductId { get; set; }
    public long? ProductItemId { get; set; }
    public string ProductName { get; set; }
    public double Confidence { get; set; }
    public long? SourcePictureId { get; set; }   // اگر منبع تصویر بود؛ همان Picture ثبت‌شده در سرویس File (بخش ۰.۶/۳)
    public List<string> Issues { get; set; } = new();
    public List<AiProductMatchCandidateDto> Matches { get; set; } = new();
}

public class AiProductMatchCandidateDto
{
    public long ProductId { get; set; }
    public string Name { get; set; }
    public double Confidence { get; set; }
    public List<AiProductMatchPackageDto> ProductItems { get; set; } = new();
}

public class AiProductMatchPackageDto
{
    public long? ProductItemId { get; set; }   // null یعنی این ترکیب تنوع هنوز برای این فروشگاه ساخته نشده (طبق تصمیم ۰.۳)
    public string Label { get; set; }
    public long? VarietyItemId { get; set; }
    public long? VarietyItem2Id { get; set; }
    public bool ExistsForCurrentStore { get; set; }
}
```

این ساختار دقیقاً همان چیزی‌ست که در نمونه‌ی پاسخ سند اصلی آمده، با دو فیلد اضافه: `SourcePictureId` (طبق ۰.۶) و `ExistsForCurrentStore` که مستقیماً به فرانت می‌گوید آیا باید مسیر «آپدیت موجودی» یا مسیر «ساخت آیتم جدید» را برای `POST /api/Seller/ProductItem` طی کند (طبق ۰.۴).

---

## ۷. کنترلر و Endpointها

```csharp
// backend/Api/Areas/Seller/Controllers/AiProductMatchController.cs
[Area("Seller")]
[Route("api/[area]/[controller]")]
[ApiController]
[Authorize]
public class AiProductMatchController : ControllerBase
{
    private readonly IAiProductMatchService _service;
    private readonly ICurrentUserHelper _currentUserHelper;

    [HttpGet("status")]
    [ProducesResponseType(typeof(BaseResultDto<AiProductMatchStatusDto>), 200)]
    public async Task<IActionResult> Status()
        => Ok(await _service.GetStatusAsync());

    [HttpPost("analyze")]
    [RequestSizeLimit(64 * 1024 * 1024)]                       // مجموع درخواست، مطابق الگوی PictureUploadController
    [RequestFormLimits(MultipartBodyLengthLimit = 64 * 1024 * 1024)]
    [ProducesResponseType(typeof(BaseResultDto<AiProductMatchAnalyzeResultDto>), 200)]
    public async Task<IActionResult> Analyze([FromForm] AiProductMatchAnalyzeInputDto dto)
    {
        var storeId = _currentUserHelper.CurrentUser.StoreId;
        var authHeader = Request.Headers.Authorization.ToString();
        var result = await _service.AnalyzeAsync(storeId, dto, authHeader, HttpContext.RequestAborted);
        return Ok(result);
    }
}
```

- `storeId` همیشه از توکن گرفته می‌شود، دقیقاً مثل الگوی `ProductItemController` — هیچ‌وقت از بدنه‌ی درخواست خوانده نمی‌شود.
- هدر `Authorization` خام همان درخواست، برای Forward به سرویس File (بخش ۳) به سرویس پاس داده می‌شود.
- سقف حجم کلی روی خودِ Attribute (که واقعاً HTTP 413 تولید می‌کند، طبق ۰.۲) گذاشته شده، مستقل از اعتبارسنجی تعداد/حجم تک‌تک تصاویر که داخل سرویس و با کد ۲۰۰+`code` انجام می‌شود.

---

## ۸. جدول کامل `code` عددی (جایگزین جدول HTTP سند اصلی)

تمام این حالت‌ها (به‌جز ۴۰۱ و ۴۱۳ که واقعی می‌مانند) با HTTP 200 و `isSuccess:false` برمی‌گردند:

| `code` | معادل مفهومی سند اصلی | پیام فارسی (`Resource.Notification.*`، باید اضافه شود) |
|---:|---|---|
| 0 | (موفق) | — |
| 1 | 400 — ورودی خالی/نامعتبر | «حداقل یکی از تصاویر یا داده جدول باید ارسال شود.» / «نوع منبع نامعتبر است.» |
| 2 | 403 — فروشگاه غیرفعال/بدون دسترسی | «فروشگاه شما غیرفعال یا فاقد دسترسی به این قابلیت است.» |
| 3 | 422 — تصویر/جدول قابل تحلیل نیست | «امکان تحلیل تصویر یا جدول ارسالی وجود نداشت.» |
| 5 | 502 — خطای Gemini | «سرویس هوش مصنوعی موقتاً پاسخ نداد.» (بدون افشای متن خام خطای Gemini، طبق بند امنیتی سند اصلی) |
| 6 | 503 — سرویس تنظیم نشده | «قابلیت تطبیق هوشمند محصول در حال حاضر در دسترس نیست.» |

(کد ۴ عمداً حذف شد — طبق تصمیم ۰.۵ سقف مصرفی وجود ندارد که رد شود.)

413 (حجم/تعداد تصاویر بیش از حد) کاملاً توسط `[RequestFormLimits]` در لایه‌ی ASP.NET Core مدیریت می‌شود و اصلاً به کد سرویس نمی‌رسد — پاسخش یک صفحه‌ی خطای استاندارد Kestrel است، نه `BaseResultDto`؛ فرانت باید این حالت را جدا (بر اساس وضعیت واقعی HTTP 413) تشخیص دهد، نه از روی `code`.

---

## ۹. ترتیب فازهای پیاده‌سازی

1. **فاز ۱:** `AiProductMatchOptions` + appsettings + رشته‌های Resource جدید (بدون منطق) — قابل build و merge مستقل.
2. **فاز ۲:** `IAiProductMatchFileClient` (آپلود عکس به سرویس File) — قابل تست مستقل با یک عکس نمونه.
3. **فاز ۳:** Refactor غیرمخرب `PastilAiCompletionRouter` (استخراج `CallGeminiRawAsync`) — با اجرای کامل تست‌های موجود چت پاستیل‌AI برای اطمینان از عدم تغییر رفتار.
4. **فاز ۴:** `AiProductMatchGeminiResponseParser` + `AiProductMatchService` (منطق کامل، ابتدا با Provider Test Mode/Mock برای تست بدون هزینه‌ی واقعی API).
5. **فاز ۵:** کنترلر + Endpointها + تست End-to-End با یک عکس واقعی قفسه.
6. **فاز ۶:** هماهنگی نهایی با فرانت روی شکل دقیق پاسخ و رفع هر ابهام باقی‌مانده.

---

## ۱۰. سوال باز باقی‌مانده

برای منبع `shelf`، آیا مرحله‌ی اول (استخراج نام‌های خام از عکس) و مرحله‌ی دوم (انتخاب نهایی بین کاندیدها) باید دو فراخوانی جدای Gemini باشند (دقیق‌تر ولی دو برابر هزینه/تاخیر) یا یک فراخوانی واحد با کل کاتالوگ کاندید به‌ازای تمام آیتم‌های قابل‌مشاهده در عکس (سریع‌تر ولی پیچیده‌تر برای Prompt Engineering)؟ پیشنهاد این سند دو مرحله‌ای است (ساده‌تر برای پیاده‌سازی و عیب‌یابی اول کار) و همین رویکرد در فاز ۴ پیاده‌سازی می‌شود مگر خلاف آن اعلام شود.
