# قرارداد جامع API آرایشگاه پاستیل

این سند از کنترلرها و DTOهای فعلی `backend` تولید شده و برای پیاده‌سازی کلاینت به فایل یا OpenAPI دیگری نیاز ندارد. قرارداد کد فعلی را توصیف می‌کند و دربارهٔ استقرار نسخه‌ای تازه در Production ادعا نمی‌کند. نام فیلدهای JSON در مثال‌ها به صورت camelCase نوشته شده‌اند؛ نام کلاس‌های C# در جدول‌ها عین کد است.

**پوشش:** 121 عملیات در چهار سطح عمومی، مشتری، مالک/نماینده و مدیر؛ 86 تعریف DTO با 601 فیلد تعریف‌شده. مسیرها از attributeهای `Http*` و نوع پاسخ از `ProducesResponseType(200)` خود کنترلر خوانده شده‌اند.

## قواعد مشترک خواندن پاسخ و دسترسی

- `BaseResultDto<T>` حاوی `isSuccess`، `messages`، `code` و `data` است؛ `BaseResultDto` بدون داده سه فیلد اول را دارد. دریافت HTTP 200 به معنی موفقیت عملیات نیست: ابتدا `isSuccess` را بررسی کنید. برخی جستجوها و فهرست‌ها شیء صفحه‌بندی را مستقیماً برمی‌گردانند و envelope ندارند.
- مسیرهای عمومی `AllowAnonymous` دارند. مسیرهای `EndUser` به JWT معتبر نیاز دارند. مسیرهای `Companion` علاوه بر JWT با policy عضویت ناحیه کنترل می‌شوند؛ حالت عضویت با `Security:AreaPolicyMode` می‌تواند Audit یا Enforce باشد. عملیات تغییر و خواندن رزرو مالکیت را در سرویس نیز بررسی می‌کنند. بعضی GETهای ساعت/محدوده/تصویر فقط وجود آرایشگاه را بررسی می‌کنند و نباید برای دادهٔ محرمانه به کار روند.
- مسیرهای `Admin` با policy مرکزی `AdminArea` محافظت می‌شوند: نقش Admin یا مجوز تفویض‌شدهٔ معتبر برای همان controller/action. وجود `[Authorize]` در فایل کنترلر به تنهایی تعریف کامل این policy نیست.
- خطای مدل ورودی ممکن است HTTP 400 بدهد؛ نبود JWT خطای 401 و نداشتن مجوز خطای 403 است. خطاهای کسب‌وکار عمدتاً به شکل پاسخ 200 با `isSuccess=false` و `messages` برمی‌گردند. `GET /api/Companion/Groomer/{id}` برای آرایشگاه غیرمتعلق، و سه مسیر عمومی `GET /api/Groomer/{id}/Times`، `Zones` و `Pictures` برای آرایشگاه ناموجود/غیرقابل نمایش، HTTP 404 با `BaseResultDto(false, NothingFound)` برمی‌گردانند. به متن پیام برای تصمیم‌گیری برنامه‌ای تکیه نکنید؛ وضعیت دامنه و `isSuccess` را مبنا بگیرید.

```json
{"isSuccess":true,"messages":[],"code":0,"data":{"id":125}}
```

## عمومی و جستجو — 10 عملیات

| روش | مسیر | ورودی | پاسخ 200 | شرح |
|---|---|---|---|---|
| `GET` | `/api/Groomer` | `dto` (query) | `GroomerSearchDto` | جستجو (شهر، محله، حیوان، خدمت، خدمات در محل، ...) |
| `GET` | `/api/Groomer/{id}` | `id` (مسیر) | `BaseResultDto<GroomerVDto>` | جزئیات آرایشگاه |
| `GET` | `/api/Groomer/{id}/Services` | `id` (مسیر), `petSize` (query) | `BaseResultDto<List<GroomerServiceVDto>>` | خدمات و بسته‌های قیمتی قابل رزرو؛ petSize (Small/Medium/Large) بسته‌ها را فیلتر می‌کند |
| `GET` | `/api/Groomer/{id}/Times` | `id` (مسیر) | `BaseResultDto<List<GroomerTimeVDto>>` | فهرست/مشاهده ساعت‌های کاری آرایشگاه |
| `GET` | `/api/Groomer/{id}/Zones` | `id` (مسیر) | `BaseResultDto<List<GroomerZoneVDto>>` | فهرست/مشاهده محدوده‌های پوشش آرایشگاه |
| `GET` | `/api/Groomer/{id}/Pictures` | `id` (مسیر) | `BaseResultDto<List<GroomerPictureVDto>>` | فهرست/مشاهده تصاویر گالری آرایشگاه |
| `GET` | `/api/GroomerPortfolio` | `dto` (query) | `GroomerPortfolioPage<GroomerPortfolioPostVDto>` | فید نمونه‌کارها؛ با groomerId فقط نمونه‌کارهای همان آرایشگاه |
| `GET` | `/api/GroomerPortfolio/{id}` | `id` (مسیر) | `BaseResultDto<GroomerPortfolioPostVDto>` | جزئیات نمونه‌کار آرایشگاه |
| `GET` | `/api/GroomerPortfolio/Comments` | `dto` (query) | `GroomerPortfolioPage<GroomerPortfolioCommentVDto>` | کامنت‌های یک نمونه‌کار؛ ParentId خالی = نظرهای اصلی، پر = پاسخ‌های آن نظر |
| `GET` | `/api/GroomerReview` | `dto` (query) | `GroomerReviewPage` | فهرست/مشاهده نظر آرایشگاه |

### `GET /api/Groomer`

**کار:** جستجو (شهر، محله، حیوان، خدمت، خدمات در محل، ...)  
**دسترسی:** بدون ورود  
**ورودی:** `dto: GroomerInputDto` در query  
**پاسخ 200:** `GroomerSearchDto`

### `GET /api/Groomer/{id}`

**کار:** جزئیات آرایشگاه  
**دسترسی:** بدون ورود  
**ورودی:** `id: long` در مسیر  
**پاسخ 200:** `BaseResultDto<GroomerVDto>`

### `GET /api/Groomer/{id}/Services`

**کار:** خدمات و بسته‌های قیمتی قابل رزرو؛ petSize (Small/Medium/Large) بسته‌ها را فیلتر می‌کند  
**دسترسی:** بدون ورود  
**ورودی:** `id: long` در مسیر؛ `petSize: string` در query (پیش‌فرض `null`)  
**پاسخ 200:** `BaseResultDto<List<GroomerServiceVDto>>`

### `GET /api/Groomer/{id}/Times`

**کار:** فهرست/مشاهده ساعت‌های کاری آرایشگاه  
**دسترسی:** بدون ورود  
**ورودی:** `id: long` در مسیر  
**پاسخ 200:** `BaseResultDto<List<GroomerTimeVDto>>`

**قاعدهٔ اختصاصی:** اگر آرایشگاه در جستجوی عمومی وجود نداشته باشد یا فعال/تأییدشده نباشد، 404 با `BaseResultDto(false, NothingFound)` برمی‌گردد.

### `GET /api/Groomer/{id}/Zones`

**کار:** فهرست/مشاهده محدوده‌های پوشش آرایشگاه  
**دسترسی:** بدون ورود  
**ورودی:** `id: long` در مسیر  
**پاسخ 200:** `BaseResultDto<List<GroomerZoneVDto>>`

**قاعدهٔ اختصاصی:** اگر آرایشگاه در جستجوی عمومی وجود نداشته باشد یا فعال/تأییدشده نباشد، 404 با `BaseResultDto(false, NothingFound)` برمی‌گردد.

### `GET /api/Groomer/{id}/Pictures`

**کار:** فهرست/مشاهده تصاویر گالری آرایشگاه  
**دسترسی:** بدون ورود  
**ورودی:** `id: long` در مسیر  
**پاسخ 200:** `BaseResultDto<List<GroomerPictureVDto>>`

**قاعدهٔ اختصاصی:** اگر آرایشگاه در جستجوی عمومی وجود نداشته باشد یا فعال/تأییدشده نباشد، 404 با `BaseResultDto(false, NothingFound)` برمی‌گردد.

### `GET /api/GroomerPortfolio`

**کار:** فید نمونه‌کارها؛ با groomerId فقط نمونه‌کارهای همان آرایشگاه  
**دسترسی:** بدون ورود  
**ورودی:** `dto: GroomerPortfolioPostSearchInputDto` در query  
**پاسخ 200:** `GroomerPortfolioPage<GroomerPortfolioPostVDto>`

### `GET /api/GroomerPortfolio/{id}`

**کار:** جزئیات نمونه‌کار آرایشگاه  
**دسترسی:** بدون ورود  
**ورودی:** `id: long` در مسیر  
**پاسخ 200:** `BaseResultDto<GroomerPortfolioPostVDto>`

### `GET /api/GroomerPortfolio/Comments`

**کار:** کامنت‌های یک نمونه‌کار؛ ParentId خالی = نظرهای اصلی، پر = پاسخ‌های آن نظر  
**دسترسی:** بدون ورود  
**ورودی:** `dto: GroomerPortfolioCommentSearchInputDto` در query  
**پاسخ 200:** `GroomerPortfolioPage<GroomerPortfolioCommentVDto>`

### `GET /api/GroomerReview`

**کار:** فهرست/مشاهده نظر آرایشگاه  
**دسترسی:** بدون ورود  
**ورودی:** `dto: GroomerReviewSearchInputDto` در query  
**پاسخ 200:** `GroomerReviewPage`

## مشتری — 17 عملیات

| روش | مسیر | ورودی | پاسخ 200 | شرح |
|---|---|---|---|---|
| `POST` | `/api/EndUser/GroomerPortfolioComment` | `dto` (بدنه JSON) | `BaseResultDto<GroomerPortfolioCommentVDto>` | ثبت کامنت (یا پاسخ با ParentId) |
| `DELETE` | `/api/EndUser/GroomerPortfolioComment` | `id` (query) | `BaseResultDto` | حذف کامنت خودم |
| `POST` | `/api/EndUser/GroomerPortfolioLike` | `postId` (query) | `BaseResultDto<GroomerPortfolioLikeResultDto>` | لایک (تکراری بودن مشکلی ندارد؛ همیشه وضعیت نهایی و تعداد را برمی‌گرداند) |
| `DELETE` | `/api/EndUser/GroomerPortfolioLike` | `postId` (query) | `BaseResultDto<GroomerPortfolioLikeResultDto>` | برداشتن لایک |
| `GET` | `/api/EndUser/GroomerReserve` | `dto` (query) | `GroomerReservePage` | رزروهای من |
| `GET` | `/api/EndUser/GroomerReserve/{id}` | `id` (مسیر) | `BaseResultDto<GroomerReserveVDto>` | جزئیات رزرو آرایشگاه |
| `GET` | `/api/EndUser/GroomerReserve/Quote` | `packageIds` (query), `packageId` (query), `pickupRequested` (query) | `BaseResultDto<GroomerReserveQuoteDto>` | قیمت بسته (پیش‌پرداخت و باقی‌مانده‌ی حضوری) قبل از رزرو یک یا چند بسته (رزرو چندخدمتی؛ حداکثر ۵، هر خدمت یک‌بار). packageId قدیمی هم پذیرفته می‌شود. |
| `GET` | `/api/EndUser/GroomerReserve/Availability` | `groomerId` (query), `date` (query), `packageId` (query), `assigneeUserId` (query), `extraPackageIds` (query) | `BaseResultDto<GroomerAvailabilityDto>` | زمان‌های آزاد یک روز برای بسته‌ی انتخابی |
| `POST` | `/api/EndUser/GroomerReserve` | `dto` (بدنه JSON) | `BaseResultDto<GroomerReserveVDto>` | ثبت رزرو (پرداخت‌نشده)؛ بعد از آن کد تخفیف/کیف پول و پرداخت |
| `PUT` | `/api/EndUser/GroomerReserve/Cancel` | `id` (query) | `BaseResultDto` | لغو رزرو: تا ۱۲ ساعت قبل از نوبت (یا تا قبل از تأیید آرایشگاه) با برگشت کل مبلغ به کیف پول |
| `POST` | `/api/EndUser/GroomerReservePayment` | `payment` (بدنه JSON) | `BaseResultDto<PaymentStartDto>` | شروع پرداخت رزرو (GroomerReserveId در بدنه) |
| `PUT` | `/api/EndUser/GroomerReservePayment/Rebate` | `dto` (بدنه JSON) | `BaseResultDto` | اعمال کد تخفیف روی مبلغ آنلاین رزرو |
| `DELETE` | `/api/EndUser/GroomerReservePayment/Rebate` | `id` (query) | `BaseResultDto` | حذف کد تخفیف پرداخت رزرو آرایشگاه |
| `PUT` | `/api/EndUser/GroomerReservePayment/Wallet` | `dto` (بدنه JSON) | `BaseResultDto` | پرداخت از کیف پول (کل یا بخشی از مبلغ) |
| `POST` | `/api/EndUser/GroomerReview` | `dto` (بدنه JSON) | `BaseResultDto<GroomerReviewVDto>` | نظر تا تأیید ادمین نمایش داده نمی‌شود |
| `GET` | `/api/EndUser/GroomerStaffInvite` | — | `List<GroomerStaffInviteVDto>` | دعوت‌نامه‌های منتظر پاسخ من |
| `PUT` | `/api/EndUser/GroomerStaffInvite` | `id` (query), `accept` (query) | `BaseResultDto` | پذیرفتن (accept=true) یا رد دعوت |

### `POST /api/EndUser/GroomerPortfolioComment`

**کار:** ثبت کامنت (یا پاسخ با ParentId)  
**دسترسی:** مشتری با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerPortfolioCommentInputDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerPortfolioCommentVDto>`

### `DELETE /api/EndUser/GroomerPortfolioComment`

**کار:** حذف کامنت خودم  
**دسترسی:** مشتری با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `POST /api/EndUser/GroomerPortfolioLike`

**کار:** لایک (تکراری بودن مشکلی ندارد؛ همیشه وضعیت نهایی و تعداد را برمی‌گرداند)  
**دسترسی:** مشتری با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `postId: long` در query  
**پاسخ 200:** `BaseResultDto<GroomerPortfolioLikeResultDto>`

### `DELETE /api/EndUser/GroomerPortfolioLike`

**کار:** برداشتن لایک  
**دسترسی:** مشتری با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `postId: long` در query  
**پاسخ 200:** `BaseResultDto<GroomerPortfolioLikeResultDto>`

### `GET /api/EndUser/GroomerReserve`

**کار:** رزروهای من  
**دسترسی:** مشتری با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerReserveSearchInputDto` در query  
**پاسخ 200:** `GroomerReservePage`

### `GET /api/EndUser/GroomerReserve/{id}`

**کار:** جزئیات رزرو آرایشگاه  
**دسترسی:** مشتری با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در مسیر  
**پاسخ 200:** `BaseResultDto<GroomerReserveVDto>`

### `GET /api/EndUser/GroomerReserve/Quote`

**کار:** قیمت بسته (پیش‌پرداخت و باقی‌مانده‌ی حضوری) قبل از رزرو یک یا چند بسته (رزرو چندخدمتی؛ حداکثر ۵، هر خدمت یک‌بار). packageId قدیمی هم پذیرفته می‌شود.  
**دسترسی:** مشتری با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `packageIds: List<long>` در query؛ `packageId: long` در query (پیش‌فرض `0`)؛ `pickupRequested: bool` در query (پیش‌فرض `false`)  
**پاسخ 200:** `BaseResultDto<GroomerReserveQuoteDto>`

**قاعدهٔ اختصاصی:** `packageIds` در query می‌تواند تکرار شود؛ `packageId` تک‌مقداری قدیمی هم پذیرفته می‌شود. ترتیب بستهٔ نخست، بستهٔ اصلی است. مبالغ سمت سرور محاسبه می‌شوند.

### `GET /api/EndUser/GroomerReserve/Availability`

**کار:** زمان‌های آزاد یک روز برای بسته‌ی انتخابی  
**دسترسی:** مشتری با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `groomerId: long` در query؛ `date: DateTime` در query؛ `packageId: long` در query؛ `assigneeUserId: long?` در query (پیش‌فرض `null`)؛ `extraPackageIds: List<long>` در query (پیش‌فرض `null`)  
**پاسخ 200:** `BaseResultDto<GroomerAvailabilityDto>`

**قاعدهٔ اختصاصی:** `date` یک روز، `packageId` بستهٔ اصلی و `extraPackageIds` بسته‌های افزوده است؛ خروجی slotهای ۳۰ دقیقه‌ای با `capacity` و `available` است. ظرفیت در POST دوباره بررسی می‌شود.

### `POST /api/EndUser/GroomerReserve`

**کار:** ثبت رزرو (پرداخت‌نشده)؛ بعد از آن کد تخفیف/کیف پول و پرداخت  
**دسترسی:** مشتری با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerReserveInputDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerReserveVDto>`

**قاعدهٔ اختصاصی:** `groomerServicePackageId` بستهٔ اصلی است؛ `extraPackageIds` حداکثر چهار بستهٔ افزوده تا سقف کل پنج بسته. مبلغ، کمیسیون و مالک رزرو از سرور تعیین می‌شوند.

### `PUT /api/EndUser/GroomerReserve/Cancel`

**کار:** لغو رزرو: تا ۱۲ ساعت قبل از نوبت (یا تا قبل از تأیید آرایشگاه) با برگشت کل مبلغ به کیف پول  
**دسترسی:** مشتری با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** تا پیش از شروع و قبل از تکمیل قابل لغو است؛ فقط در حالت Pending یا دست‌کم ۱۲ ساعت پیش از نوبت، مبلغ پرداخت‌شده به کیف پول مسترد می‌شود. در بازهٔ دیرتر پس از تأیید، لغو ممکن است بدون استرداد باشد.

### `POST /api/EndUser/GroomerReservePayment`

**کار:** شروع پرداخت رزرو (GroomerReserveId در بدنه)  
**دسترسی:** مشتری با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `payment: PaymentStartDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<PaymentStartDto>`

**قاعدهٔ اختصاصی:** در بدنهٔ `PaymentStartDto` شناسهٔ `groomerReserveId` رزرو خود کاربر را بفرستید؛ هرگاه مبلغ قابل پرداخت در درگاه مثبت باشد، `merchantId` معتبر لازم است، حتی اگر بخشی از مبلغ از کیف پول کسر شده باشد. `userId`/`user` در کنترلر و مبلغ/هدف/توکن درگاه در سرویس بازنویسی می‌شوند. هدر اختیاری `Idempotency-Key` اگر ارسال شود باید UUID معتبر با قالب D باشد. annotation پاسخ `BaseResultDto` است، اما در مسیر موفق StartPayment نمونهٔ runtime از `BaseResultDto<PaymentStartDto>` می‌آید؛ `data.paymentUrl` و `data.paymentIsLink` را فقط پس از `isSuccess=true` بخوانید.

### `PUT /api/EndUser/GroomerReservePayment/Rebate`

**کار:** اعمال کد تخفیف روی مبلغ آنلاین رزرو  
**دسترسی:** مشتری با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerReserveRebateCodeDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `DELETE /api/EndUser/GroomerReservePayment/Rebate`

**کار:** حذف کد تخفیف پرداخت رزرو آرایشگاه  
**دسترسی:** مشتری با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/EndUser/GroomerReservePayment/Wallet`

**کار:** پرداخت از کیف پول (کل یا بخشی از مبلغ)  
**دسترسی:** مشتری با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerReserveWalletDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `POST /api/EndUser/GroomerReview`

**کار:** نظر تا تأیید ادمین نمایش داده نمی‌شود  
**دسترسی:** مشتری با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerReviewInputDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerReviewVDto>`

### `GET /api/EndUser/GroomerStaffInvite`

**کار:** دعوت‌نامه‌های منتظر پاسخ من  
**دسترسی:** مشتری با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** ورودی ندارد  
**پاسخ 200:** `List<GroomerStaffInviteVDto>`

### `PUT /api/EndUser/GroomerStaffInvite`

**کار:** پذیرفتن (accept=true) یا رد دعوت  
**دسترسی:** مشتری با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query؛ `accept: bool` در query  
**پاسخ 200:** `BaseResultDto`

## مالک/نمایندهٔ آرایشگاه — 44 عملیات

| روش | مسیر | ورودی | پاسخ 200 | شرح |
|---|---|---|---|---|
| `GET` | `/api/Companion/Groomer` | `dto` (query) | `GroomerSearchDto` | لیست آرایشگاه‌های من |
| `GET` | `/api/Companion/Groomer/{id}` | `id` (مسیر) | `BaseResultDto<GroomerVDto>` | جزئیات آرایشگاه |
| `POST` | `/api/Companion/Groomer` | `dto` (بدنه JSON) | `BaseResultDto<GroomerVDto>` | ثبت آرایشگاه جدید زیر ارائه‌دهنده‌ی خودم (منتظر تأیید ادمین می‌ماند) |
| `PUT` | `/api/Companion/Groomer` | `dto` (بدنه JSON) | `BaseResultDto` | ویرایش (دوباره منتظر تأیید ادمین می‌شود) |
| `PUT` | `/api/Companion/Groomer/Active` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر وضعیت فعالیت آرایشگاه |
| `PUT` | `/api/Companion/Groomer/Pets` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر گونه‌های حیوان قابل پذیرش آرایشگاه |
| `DELETE` | `/api/Companion/Groomer` | `id` (query) | `BaseResultDto` | حذف آرایشگاه |
| `GET` | `/api/Companion/GroomerPicture` | `groomerId` (query) | `BaseResultDto<List<GroomerPictureVDto>>` | فهرست/مشاهده تصویر گالری آرایشگاه |
| `POST` | `/api/Companion/GroomerPicture` | `dto` (بدنه JSON) | `BaseResultDto<GroomerPictureVDto>` | ثبت تصویر گالری آرایشگاه |
| `DELETE` | `/api/Companion/GroomerPicture` | `id` (query) | `BaseResultDto` | حذف تصویر گالری آرایشگاه |
| `GET` | `/api/Companion/GroomerPortfolio` | `dto` (query) | `GroomerPortfolioPage<GroomerPortfolioPostVDto>` | پست‌های آرایشگاه‌های من (شامل پنهان‌شده‌ها با دلیل) |
| `POST` | `/api/Companion/GroomerPortfolio` | `dto` (بدنه JSON) | `BaseResultDto<GroomerPortfolioPostVDto>` | ثبت نمونه‌کار آرایشگاه |
| `PUT` | `/api/Companion/GroomerPortfolio` | `dto` (بدنه JSON) | `BaseResultDto<GroomerPortfolioPostVDto>` | تغییر نمونه‌کار آرایشگاه |
| `DELETE` | `/api/Companion/GroomerPortfolio` | `id` (query) | `BaseResultDto` | حذف نمونه‌کار آرایشگاه |
| `DELETE` | `/api/Companion/GroomerPortfolio/Comment` | `id` (query) | `BaseResultDto` | حذف کامنت زیر نمونه‌کار خودم |
| `GET` | `/api/Companion/GroomerReserve` | `dto` (query) | `GroomerReservePage` | فقط رزروهای پرداخت‌شده‌ی آرایشگاه‌های من |
| `GET` | `/api/Companion/GroomerReserve/{id}` | `id` (مسیر) | `BaseResultDto<GroomerReserveVDto>` | جزئیات رزرو آرایشگاه |
| `PUT` | `/api/Companion/GroomerReserve/Approve` | `id` (query) | `BaseResultDto` | تغییر تأیید رزرو آرایشگاه |
| `PUT` | `/api/Companion/GroomerReserve/Reject` | `dto` (بدنه JSON) | `BaseResultDto` | رد رزرو (دلیل الزامی)؛ کل مبلغ به کیف پول مشتری برمی‌گردد |
| `PUT` | `/api/Companion/GroomerReserve/Reschedule` | `dto` (بدنه JSON) | `BaseResultDto<GroomerReserveVDto>` | تغییر زمان نوبت (به مشتری پوش و پیامک می‌رود)؛ فقط روی زمان آزاد آرایشگاه |
| `PUT` | `/api/Companion/GroomerReserve/Complete` | `id` (query) | `BaseResultDto` | تکمیل رزرو بعد از انجام خدمت |
| `GET` | `/api/Companion/GroomerReview` | `dto` (query) | `GroomerReviewPage` | فهرست/مشاهده نظر آرایشگاه |
| `PUT` | `/api/Companion/GroomerReview/Answer` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر پاسخ نظر آرایشگاه |
| `GET` | `/api/Companion/GroomerSchedule/Times` | `groomerId` (query) | `BaseResultDto<List<GroomerTimeVDto>>` | فهرست/مشاهده ساعت‌های کاری برنامهٔ کاری آرایشگاه |
| `PUT` | `/api/Companion/GroomerSchedule/Times` | `dto` (بدنه JSON) | `BaseResultDto<List<GroomerTimeVDto>>` | تغییر ساعت‌های کاری برنامهٔ کاری آرایشگاه |
| `GET` | `/api/Companion/GroomerSchedule/Zones` | `groomerId` (query) | `BaseResultDto<List<GroomerZoneVDto>>` | فهرست/مشاهده محدوده‌های پوشش برنامهٔ کاری آرایشگاه |
| `PUT` | `/api/Companion/GroomerSchedule/Zones` | `dto` (بدنه JSON) | `BaseResultDto<List<GroomerZoneVDto>>` | تغییر محدوده‌های پوشش برنامهٔ کاری آرایشگاه |
| `GET` | `/api/Companion/GroomerService` | `groomerId` (query) | `BaseResultDto<List<GroomerServiceVDto>>` | فهرست/مشاهده خدمت آرایشگاه |
| `POST` | `/api/Companion/GroomerService` | `dto` (بدنه JSON) | `BaseResultDto<GroomerServiceVDto>` | افزودن خدمت (تا تأیید ادمین قابل رزرو نیست) |
| `PUT` | `/api/Companion/GroomerService` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر خدمت آرایشگاه |
| `DELETE` | `/api/Companion/GroomerService` | `id` (query) | `BaseResultDto` | حذف خدمت آرایشگاه |
| `PUT` | `/api/Companion/GroomerService/Active` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر وضعیت فعالیت خدمت آرایشگاه |
| `PUT` | `/api/Companion/GroomerService/Users` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر پرسنل خدمت خدمت آرایشگاه |
| `POST` | `/api/Companion/GroomerService/Package` | `dto` (بدنه JSON) | `BaseResultDto<GroomerServicePackageVDto>` | ثبت بستهٔ خدمت خدمت آرایشگاه |
| `PUT` | `/api/Companion/GroomerService/Package` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر بستهٔ خدمت خدمت آرایشگاه |
| `PUT` | `/api/Companion/GroomerService/Package/Active` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر بستهٔ خدمت / وضعیت فعالیت خدمت آرایشگاه |
| `DELETE` | `/api/Companion/GroomerService/Package` | `id` (query) | `BaseResultDto` | حذف بستهٔ خدمت خدمت آرایشگاه |
| `GET` | `/api/Companion/GroomerSettlement` | `dto` (query) | `SettlementSearchDto` | فهرست/مشاهده تسویهٔ آرایشگاه |
| `GET` | `/api/Companion/GroomerSettlement/{id}/Items` | `id` (مسیر) | `BaseResultDto<List<SettlementItemVDto>>` | ریز تسویه‌ی خودم |
| `GET` | `/api/Companion/GroomerSettlement/{id}` | `id` (مسیر) | `BaseResultDto<SettlementVDto>` | جزئیات تسویهٔ آرایشگاه |
| `GET` | `/api/Companion/GroomerUser` | `groomerId` (query) | `BaseResultDto<List<GroomerStaffVDto>>` | فهرست/مشاهده پرسنل آرایشگاه |
| `POST` | `/api/Companion/GroomerUser` | `dto` (بدنه JSON) | `BaseResultDto<GroomerStaffVDto>` | ثبت پرسنل آرایشگاه |
| `PUT` | `/api/Companion/GroomerUser/Active` | `id` (query), `active` (query) | `BaseResultDto` | تغییر وضعیت فعالیت پرسنل آرایشگاه |
| `DELETE` | `/api/Companion/GroomerUser` | `id` (query) | `BaseResultDto` | حذف پرسنل آرایشگاه |

### `GET /api/Companion/Groomer`

**کار:** لیست آرایشگاه‌های من  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerInputDto` در query  
**پاسخ 200:** `GroomerSearchDto`

### `GET /api/Companion/Groomer/{id}`

**کار:** جزئیات آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در مسیر  
**پاسخ 200:** `BaseResultDto<GroomerVDto>`

### `POST /api/Companion/Groomer`

**کار:** ثبت آرایشگاه جدید زیر ارائه‌دهنده‌ی خودم (منتظر تأیید ادمین می‌ماند)  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerVDto>`

### `PUT /api/Companion/Groomer`

**کار:** ویرایش (دوباره منتظر تأیید ادمین می‌شود)  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/Companion/Groomer/Active`

**کار:** تغییر وضعیت فعالیت آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerActiveDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/Companion/Groomer/Pets`

**کار:** تغییر گونه‌های حیوان قابل پذیرش آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerPetsRequestDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `DELETE /api/Companion/Groomer`

**کار:** حذف آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `GET /api/Companion/GroomerPicture`

**کار:** فهرست/مشاهده تصویر گالری آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `groomerId: long` در query  
**پاسخ 200:** `BaseResultDto<List<GroomerPictureVDto>>`

### `POST /api/Companion/GroomerPicture`

**کار:** ثبت تصویر گالری آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerPictureAddDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerPictureVDto>`

### `DELETE /api/Companion/GroomerPicture`

**کار:** حذف تصویر گالری آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `GET /api/Companion/GroomerPortfolio`

**کار:** پست‌های آرایشگاه‌های من (شامل پنهان‌شده‌ها با دلیل)  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerPortfolioPostSearchInputDto` در query  
**پاسخ 200:** `GroomerPortfolioPage<GroomerPortfolioPostVDto>`

### `POST /api/Companion/GroomerPortfolio`

**کار:** ثبت نمونه‌کار آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerPortfolioPostInputDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerPortfolioPostVDto>`

### `PUT /api/Companion/GroomerPortfolio`

**کار:** تغییر نمونه‌کار آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerPortfolioPostInputDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerPortfolioPostVDto>`

### `DELETE /api/Companion/GroomerPortfolio`

**کار:** حذف نمونه‌کار آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `DELETE /api/Companion/GroomerPortfolio/Comment`

**کار:** حذف کامنت زیر نمونه‌کار خودم  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `GET /api/Companion/GroomerReserve`

**کار:** فقط رزروهای پرداخت‌شده‌ی آرایشگاه‌های من  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerReserveSearchInputDto` در query  
**پاسخ 200:** `GroomerReservePage`

### `GET /api/Companion/GroomerReserve/{id}`

**کار:** جزئیات رزرو آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در مسیر  
**پاسخ 200:** `BaseResultDto<GroomerReserveVDto>`

### `PUT /api/Companion/GroomerReserve/Approve`

**کار:** تغییر تأیید رزرو آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/Companion/GroomerReserve/Reject`

**کار:** رد رزرو (دلیل الزامی)؛ کل مبلغ به کیف پول مشتری برمی‌گردد  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerReserveReasonDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** `reason` لازم است؛ رد، رزرو را لغو و وجه پرداخت‌شده را به کیف پول برمی‌گرداند.

### `PUT /api/Companion/GroomerReserve/Reschedule`

**کار:** تغییر زمان نوبت (به مشتری پوش و پیامک می‌رود)؛ فقط روی زمان آزاد آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerReserveRescheduleDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerReserveVDto>`

**قاعدهٔ اختصاصی:** فقط رزرو پرداخت‌شدهٔ مجاز؛ زمان جدید باید آینده، در ساعت کاری و دارای ظرفیت باشد؛ `rescheduleCount` و `originalDoDate` ثبت می‌شوند.

### `PUT /api/Companion/GroomerReserve/Complete`

**کار:** تکمیل رزرو بعد از انجام خدمت  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `GET /api/Companion/GroomerReview`

**کار:** فهرست/مشاهده نظر آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerReviewSearchInputDto` در query  
**پاسخ 200:** `GroomerReviewPage`

### `PUT /api/Companion/GroomerReview/Answer`

**کار:** تغییر پاسخ نظر آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerReviewOwnerAnswerDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `GET /api/Companion/GroomerSchedule/Times`

**کار:** فهرست/مشاهده ساعت‌های کاری برنامهٔ کاری آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `groomerId: long` در query  
**پاسخ 200:** `BaseResultDto<List<GroomerTimeVDto>>`

### `PUT /api/Companion/GroomerSchedule/Times`

**کار:** تغییر ساعت‌های کاری برنامهٔ کاری آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerProfileSetDto<GroomerTimeInputDto>` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<List<GroomerTimeVDto>>`

**قاعدهٔ اختصاصی:** بدنه جایگزین کل بازه‌های ساعت کاری آرایشگاه می‌شود؛ هر ردیف `weekDayId,startTime,endTime,capacity,active` دارد.

### `GET /api/Companion/GroomerSchedule/Zones`

**کار:** فهرست/مشاهده محدوده‌های پوشش برنامهٔ کاری آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `groomerId: long` در query  
**پاسخ 200:** `BaseResultDto<List<GroomerZoneVDto>>`

### `PUT /api/Companion/GroomerSchedule/Zones`

**کار:** تغییر محدوده‌های پوشش برنامهٔ کاری آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerProfileSetDto<GroomerZoneInputDto>` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<List<GroomerZoneVDto>>`

**قاعدهٔ اختصاصی:** بدنه جایگزین کل محدوده‌های پوشش است؛ هر ردیف `stateId,cityId,neighborhoodId` دارد.

### `GET /api/Companion/GroomerService`

**کار:** فهرست/مشاهده خدمت آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `groomerId: long` در query  
**پاسخ 200:** `BaseResultDto<List<GroomerServiceVDto>>`

### `POST /api/Companion/GroomerService`

**کار:** افزودن خدمت (تا تأیید ادمین قابل رزرو نیست)  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerServiceInputDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerServiceVDto>`

### `PUT /api/Companion/GroomerService`

**کار:** تغییر خدمت آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerServiceInputDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `DELETE /api/Companion/GroomerService`

**کار:** حذف خدمت آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/Companion/GroomerService/Active`

**کار:** تغییر وضعیت فعالیت خدمت آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerServiceActiveRequestDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/Companion/GroomerService/Users`

**کار:** تغییر پرسنل خدمت خدمت آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerServiceUsersRequestDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `POST /api/Companion/GroomerService/Package`

**کار:** ثبت بستهٔ خدمت خدمت آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerServicePackageInputDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerServicePackageVDto>`

### `PUT /api/Companion/GroomerService/Package`

**کار:** تغییر بستهٔ خدمت خدمت آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerServicePackageInputDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/Companion/GroomerService/Package/Active`

**کار:** تغییر بستهٔ خدمت / وضعیت فعالیت خدمت آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerServiceActiveRequestDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `DELETE /api/Companion/GroomerService/Package`

**کار:** حذف بستهٔ خدمت خدمت آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `GET /api/Companion/GroomerSettlement`

**کار:** فهرست/مشاهده تسویهٔ آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: SettlementInputDto` در query  
**پاسخ 200:** `SettlementSearchDto`

**قاعدهٔ اختصاصی:** فقط تسویه‌های استخر Groomer ارائه‌دهندهٔ کاربر جاری برمی‌گردند.

### `GET /api/Companion/GroomerSettlement/{id}/Items`

**کار:** ریز تسویه‌ی خودم  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در مسیر  
**پاسخ 200:** `BaseResultDto<List<SettlementItemVDto>>`

**قاعدهٔ اختصاصی:** مالکیت تسویه برای ارائه‌دهندهٔ کاربر بررسی می‌شود؛ در عدم مالکیت `isSuccess=false/AccessDenied` برمی‌گردد.

### `GET /api/Companion/GroomerSettlement/{id}`

**کار:** جزئیات تسویهٔ آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در مسیر  
**پاسخ 200:** `BaseResultDto<SettlementVDto>`

**قاعدهٔ اختصاصی:** مالکیت تسویه برای ارائه‌دهندهٔ کاربر بررسی می‌شود؛ در عدم مالکیت `isSuccess=false/AccessDenied` برمی‌گردد.

### `GET /api/Companion/GroomerUser`

**کار:** فهرست/مشاهده پرسنل آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `groomerId: long` در query  
**پاسخ 200:** `BaseResultDto<List<GroomerStaffVDto>>`

### `POST /api/Companion/GroomerUser`

**کار:** ثبت پرسنل آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerStaffAddDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerStaffVDto>`

### `PUT /api/Companion/GroomerUser/Active`

**کار:** تغییر وضعیت فعالیت پرسنل آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query؛ `active: bool` در query  
**پاسخ 200:** `BaseResultDto`

### `DELETE /api/Companion/GroomerUser`

**کار:** حذف پرسنل آرایشگاه  
**دسترسی:** مالک/نمایندهٔ آرایشگاه با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

## مدیریت — 50 عملیات

| روش | مسیر | ورودی | پاسخ 200 | شرح |
|---|---|---|---|---|
| `PUT` | `/api/Admin/GroomerActive` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر فعال‌سازی آرایشگاه |
| `PUT` | `/api/Admin/GroomerApprove` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر تأیید آرایشگاه |
| `GET` | `/api/Admin/Groomer` | `dto` (query), `deleted` (query) | `GroomerSearchDto` | جستجو true = فقط آرایشگاه‌های حذف‌شده (حذف نرم) |
| `GET` | `/api/Admin/Groomer/{id}` | `id` (مسیر) | `BaseResultDto<GroomerVDto>` | اطلاعات کامل یک آرایشگاه (حذف‌شده هم برای ادمین دیده می‌شود) |
| `POST` | `/api/Admin/Groomer` | `dto` (بدنه JSON) | `BaseResultDto<GroomerVDto>` | ساخت آرایشگاه (مستقیم تأییدشده و فعال) |
| `PUT` | `/api/Admin/Groomer` | `dto` (بدنه JSON) | `BaseResultDto` | ویرایش اطلاعات آرایشگاه (تأیید، کمیسیون و ... تغییر نمی‌کند) |
| `DELETE` | `/api/Admin/Groomer` | `id` (query) | `BaseResultDto` | حذف نرم (رزروها و گزارش مالی می‌مانند) |
| `PUT` | `/api/Admin/Groomer/Restore` | `id` (query) | `BaseResultDto` | بازگردانی آرایشگاه حذف‌شده (غیرفعال و منتشرنشده برمی‌گردد) |
| `PUT` | `/api/Admin/Groomer/Pets` | `dto` (بدنه JSON) | `BaseResultDto` | گونه‌های پذیرش‌شده (جایگزین کل لیست) |
| `GET` | `/api/Admin/GroomerPicture` | `groomerId` (query) | `BaseResultDto<List<GroomerPictureVDto>>` | فهرست/مشاهده تصویر گالری آرایشگاه |
| `POST` | `/api/Admin/GroomerPicture` | `dto` (بدنه JSON) | `BaseResultDto<GroomerPictureVDto>` | ثبت تصویر گالری آرایشگاه |
| `DELETE` | `/api/Admin/GroomerPicture` | `id` (query) | `BaseResultDto` | حذف تصویر گالری آرایشگاه |
| `GET` | `/api/Admin/GroomerPortfolioComment` | `dto` (query) | `GroomerPortfolioPage<GroomerPortfolioCommentVDto>` | فهرست/مشاهده دیدگاه نمونه‌کار |
| `DELETE` | `/api/Admin/GroomerPortfolioComment` | `id` (query) | `BaseResultDto` | حذف دیدگاه نمونه‌کار |
| `GET` | `/api/Admin/GroomerPortfolio` | `dto` (query) | `GroomerPortfolioPage<GroomerPortfolioPostVDto>` | فهرست/مشاهده نمونه‌کار آرایشگاه |
| `POST` | `/api/Admin/GroomerPortfolio` | `dto` (بدنه JSON) | `BaseResultDto<GroomerPortfolioPostVDto>` | ثبت نمونه‌کار آرایشگاه |
| `PUT` | `/api/Admin/GroomerPortfolio` | `dto` (بدنه JSON) | `BaseResultDto<GroomerPortfolioPostVDto>` | تغییر نمونه‌کار آرایشگاه |
| `DELETE` | `/api/Admin/GroomerPortfolio` | `id` (query) | `BaseResultDto` | حذف نمونه‌کار آرایشگاه |
| `PUT` | `/api/Admin/GroomerPortfolio/Moderate` | `dto` (بدنه JSON) | `BaseResultDto` | پنهان/نمایش یک پست (پنهان‌کردن نیاز به دلیل دارد) |
| `GET` | `/api/Admin/GroomerReserve` | `dto` (query) | `GroomerReservePage` | فهرست/مشاهده رزرو آرایشگاه |
| `GET` | `/api/Admin/GroomerReserve/{id}` | `id` (مسیر) | `BaseResultDto<GroomerReserveVDto>` | جزئیات رزرو آرایشگاه |
| `PUT` | `/api/Admin/GroomerReserve/Approve` | `id` (query) | `BaseResultDto` | تغییر تأیید رزرو آرایشگاه |
| `PUT` | `/api/Admin/GroomerReserve/Reject` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر رد رزرو آرایشگاه |
| `PUT` | `/api/Admin/GroomerReserve/Complete` | `id` (query) | `BaseResultDto` | تغییر تکمیل رزرو آرایشگاه |
| `PUT` | `/api/Admin/GroomerReserve/Reschedule` | `dto` (بدنه JSON) | `BaseResultDto<GroomerReserveVDto>` | تغییر زمان نوبت (به کاربر پوش و پیامک می‌رود) |
| `PUT` | `/api/Admin/GroomerReserve/Cancel` | `dto` (بدنه JSON), `refund` (query) | `BaseResultDto` | لغو توسط ادمین؛ refund=true کل مبلغ پرداخت‌شده را به کیف پول کاربر برمی‌گرداند |
| `GET` | `/api/Admin/GroomerReview` | `dto` (query) | `GroomerReviewPage` | فهرست/مشاهده نظر آرایشگاه |
| `PUT` | `/api/Admin/GroomerReview/Moderate` | `id` (query), `accept` (query) | `BaseResultDto` | تأیید (accept=true) یا رد نظر |
| `PUT` | `/api/Admin/GroomerReview/Answer` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر پاسخ نظر آرایشگاه |
| `GET` | `/api/Admin/GroomerSchedule/Times` | `groomerId` (query) | `BaseResultDto<List<GroomerTimeVDto>>` | فهرست/مشاهده ساعت‌های کاری برنامهٔ کاری آرایشگاه |
| `PUT` | `/api/Admin/GroomerSchedule/Times` | `dto` (بدنه JSON) | `BaseResultDto<List<GroomerTimeVDto>>` | کل ساعت‌های کاری را جایگزین می‌کند |
| `GET` | `/api/Admin/GroomerSchedule/Zones` | `groomerId` (query) | `BaseResultDto<List<GroomerZoneVDto>>` | فهرست/مشاهده محدوده‌های پوشش برنامهٔ کاری آرایشگاه |
| `PUT` | `/api/Admin/GroomerSchedule/Zones` | `dto` (بدنه JSON) | `BaseResultDto<List<GroomerZoneVDto>>` | کل محدوده‌های پوشش را جایگزین می‌کند |
| `GET` | `/api/Admin/GroomerService` | `groomerId` (query) | `BaseResultDto<List<GroomerServiceVDto>>` | فهرست/مشاهده خدمت آرایشگاه |
| `POST` | `/api/Admin/GroomerService` | `dto` (بدنه JSON) | `BaseResultDto<GroomerServiceVDto>` | ثبت خدمت آرایشگاه |
| `PUT` | `/api/Admin/GroomerService` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر خدمت آرایشگاه |
| `DELETE` | `/api/Admin/GroomerService` | `id` (query) | `BaseResultDto` | حذف خدمت آرایشگاه |
| `PUT` | `/api/Admin/GroomerService/Active` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر وضعیت فعالیت خدمت آرایشگاه |
| `PUT` | `/api/Admin/GroomerService/Approve` | `dto` (بدنه JSON) | `BaseResultDto` | تأیید/رد خدمت و تعیین درصد کمیسیون |
| `PUT` | `/api/Admin/GroomerService/Users` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر پرسنل خدمت خدمت آرایشگاه |
| `POST` | `/api/Admin/GroomerService/Package` | `dto` (بدنه JSON) | `BaseResultDto<GroomerServicePackageVDto>` | ثبت بستهٔ خدمت خدمت آرایشگاه |
| `PUT` | `/api/Admin/GroomerService/Package` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر بستهٔ خدمت خدمت آرایشگاه |
| `PUT` | `/api/Admin/GroomerService/Package/Active` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر بستهٔ خدمت / وضعیت فعالیت خدمت آرایشگاه |
| `DELETE` | `/api/Admin/GroomerService/Package` | `id` (query) | `BaseResultDto` | حذف بستهٔ خدمت خدمت آرایشگاه |
| `PUT` | `/api/Admin/GroomerSetting` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر تنظیمات آرایشگاه |
| `PATCH` | `/api/Admin/GroomerSetting/{id}/visibility` | `id` (مسیر), `dto` (بدنه JSON) | `BaseResultDto` | نمایش در سایت (فقط آرایشگاه تأییدشده و فعال) |
| `GET` | `/api/Admin/GroomerUser` | `groomerId` (query) | `BaseResultDto<List<GroomerStaffVDto>>` | فهرست/مشاهده پرسنل آرایشگاه |
| `POST` | `/api/Admin/GroomerUser` | `dto` (بدنه JSON) | `BaseResultDto<GroomerStaffVDto>` | افزودن پرسنل با شماره موبایل (کاربر باید قبلاً ثبت‌نام کرده باشد) |
| `PUT` | `/api/Admin/GroomerUser/Active` | `id` (query), `active` (query) | `BaseResultDto` | تغییر وضعیت فعالیت پرسنل آرایشگاه |
| `DELETE` | `/api/Admin/GroomerUser` | `id` (query) | `BaseResultDto` | حذف پرسنل آرایشگاه |

### `PUT /api/Admin/GroomerActive`

**کار:** تغییر فعال‌سازی آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerActiveDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/Admin/GroomerApprove`

**کار:** تغییر تأیید آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerApproveDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `GET /api/Admin/Groomer`

**کار:** جستجو true = فقط آرایشگاه‌های حذف‌شده (حذف نرم)  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerInputDto` در query؛ `deleted: bool?` در query (پیش‌فرض `null`)  
**پاسخ 200:** `GroomerSearchDto`

### `GET /api/Admin/Groomer/{id}`

**کار:** اطلاعات کامل یک آرایشگاه (حذف‌شده هم برای ادمین دیده می‌شود)  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در مسیر  
**پاسخ 200:** `BaseResultDto<GroomerVDto>`

### `POST /api/Admin/Groomer`

**کار:** ساخت آرایشگاه (مستقیم تأییدشده و فعال)  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerVDto>`

### `PUT /api/Admin/Groomer`

**کار:** ویرایش اطلاعات آرایشگاه (تأیید، کمیسیون و ... تغییر نمی‌کند)  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `DELETE /api/Admin/Groomer`

**کار:** حذف نرم (رزروها و گزارش مالی می‌مانند)  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/Admin/Groomer/Restore`

**کار:** بازگردانی آرایشگاه حذف‌شده (غیرفعال و منتشرنشده برمی‌گردد)  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/Admin/Groomer/Pets`

**کار:** گونه‌های پذیرش‌شده (جایگزین کل لیست)  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerPetsDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `GET /api/Admin/GroomerPicture`

**کار:** فهرست/مشاهده تصویر گالری آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `groomerId: long` در query  
**پاسخ 200:** `BaseResultDto<List<GroomerPictureVDto>>`

### `POST /api/Admin/GroomerPicture`

**کار:** ثبت تصویر گالری آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerPictureAddDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerPictureVDto>`

### `DELETE /api/Admin/GroomerPicture`

**کار:** حذف تصویر گالری آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `GET /api/Admin/GroomerPortfolioComment`

**کار:** فهرست/مشاهده دیدگاه نمونه‌کار  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerPortfolioCommentSearchInputDto` در query  
**پاسخ 200:** `GroomerPortfolioPage<GroomerPortfolioCommentVDto>`

### `DELETE /api/Admin/GroomerPortfolioComment`

**کار:** حذف دیدگاه نمونه‌کار  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `GET /api/Admin/GroomerPortfolio`

**کار:** فهرست/مشاهده نمونه‌کار آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerPortfolioPostSearchInputDto` در query  
**پاسخ 200:** `GroomerPortfolioPage<GroomerPortfolioPostVDto>`

### `POST /api/Admin/GroomerPortfolio`

**کار:** ثبت نمونه‌کار آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerPortfolioPostInputDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerPortfolioPostVDto>`

### `PUT /api/Admin/GroomerPortfolio`

**کار:** تغییر نمونه‌کار آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerPortfolioPostInputDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerPortfolioPostVDto>`

### `DELETE /api/Admin/GroomerPortfolio`

**کار:** حذف نمونه‌کار آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/Admin/GroomerPortfolio/Moderate`

**کار:** پنهان/نمایش یک پست (پنهان‌کردن نیاز به دلیل دارد)  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerPortfolioModerationDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `GET /api/Admin/GroomerReserve`

**کار:** فهرست/مشاهده رزرو آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerReserveSearchInputDto` در query  
**پاسخ 200:** `GroomerReservePage`

### `GET /api/Admin/GroomerReserve/{id}`

**کار:** جزئیات رزرو آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در مسیر  
**پاسخ 200:** `BaseResultDto<GroomerReserveVDto>`

### `PUT /api/Admin/GroomerReserve/Approve`

**کار:** تغییر تأیید رزرو آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/Admin/GroomerReserve/Reject`

**کار:** تغییر رد رزرو آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerReserveReasonDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** `reason` لازم است؛ رد، رزرو را لغو و وجه پرداخت‌شده را به کیف پول برمی‌گرداند.

### `PUT /api/Admin/GroomerReserve/Complete`

**کار:** تغییر تکمیل رزرو آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/Admin/GroomerReserve/Reschedule`

**کار:** تغییر زمان نوبت (به کاربر پوش و پیامک می‌رود)  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerReserveRescheduleDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerReserveVDto>`

**قاعدهٔ اختصاصی:** فقط رزرو پرداخت‌شدهٔ مجاز؛ زمان جدید باید آینده، در ساعت کاری و دارای ظرفیت باشد؛ `rescheduleCount` و `originalDoDate` ثبت می‌شوند.

### `PUT /api/Admin/GroomerReserve/Cancel`

**کار:** لغو توسط ادمین؛ refund=true کل مبلغ پرداخت‌شده را به کیف پول کاربر برمی‌گرداند  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerReserveReasonDto` در بدنه JSON؛ `refund: bool` در query (پیش‌فرض `true`)  
**پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** `refund` در query پیش‌فرض true است؛ `reason` در بدنه لازم است. false یعنی لغو بدون استرداد و باید تصمیم ادمین آگاهانه باشد.

### `GET /api/Admin/GroomerReview`

**کار:** فهرست/مشاهده نظر آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerReviewSearchInputDto` در query  
**پاسخ 200:** `GroomerReviewPage`

### `PUT /api/Admin/GroomerReview/Moderate`

**کار:** تأیید (accept=true) یا رد نظر  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query؛ `accept: bool` در query  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/Admin/GroomerReview/Answer`

**کار:** تغییر پاسخ نظر آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerReviewAnswerDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `GET /api/Admin/GroomerSchedule/Times`

**کار:** فهرست/مشاهده ساعت‌های کاری برنامهٔ کاری آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `groomerId: long` در query  
**پاسخ 200:** `BaseResultDto<List<GroomerTimeVDto>>`

### `PUT /api/Admin/GroomerSchedule/Times`

**کار:** کل ساعت‌های کاری را جایگزین می‌کند  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerProfileSetDto<GroomerTimeInputDto>` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<List<GroomerTimeVDto>>`

**قاعدهٔ اختصاصی:** بدنه جایگزین کل بازه‌های ساعت کاری آرایشگاه می‌شود؛ هر ردیف `weekDayId,startTime,endTime,capacity,active` دارد.

### `GET /api/Admin/GroomerSchedule/Zones`

**کار:** فهرست/مشاهده محدوده‌های پوشش برنامهٔ کاری آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `groomerId: long` در query  
**پاسخ 200:** `BaseResultDto<List<GroomerZoneVDto>>`

### `PUT /api/Admin/GroomerSchedule/Zones`

**کار:** کل محدوده‌های پوشش را جایگزین می‌کند  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerProfileSetDto<GroomerZoneInputDto>` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<List<GroomerZoneVDto>>`

**قاعدهٔ اختصاصی:** بدنه جایگزین کل محدوده‌های پوشش است؛ هر ردیف `stateId,cityId,neighborhoodId` دارد.

### `GET /api/Admin/GroomerService`

**کار:** فهرست/مشاهده خدمت آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `groomerId: long` در query  
**پاسخ 200:** `BaseResultDto<List<GroomerServiceVDto>>`

### `POST /api/Admin/GroomerService`

**کار:** ثبت خدمت آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerServiceInputDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerServiceVDto>`

### `PUT /api/Admin/GroomerService`

**کار:** تغییر خدمت آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerServiceInputDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `DELETE /api/Admin/GroomerService`

**کار:** حذف خدمت آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/Admin/GroomerService/Active`

**کار:** تغییر وضعیت فعالیت خدمت آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerServiceActiveDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/Admin/GroomerService/Approve`

**کار:** تأیید/رد خدمت و تعیین درصد کمیسیون  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerServiceApproveDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/Admin/GroomerService/Users`

**کار:** تغییر پرسنل خدمت خدمت آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerServiceUsersDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `POST /api/Admin/GroomerService/Package`

**کار:** ثبت بستهٔ خدمت خدمت آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerServicePackageInputDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerServicePackageVDto>`

### `PUT /api/Admin/GroomerService/Package`

**کار:** تغییر بستهٔ خدمت خدمت آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerServicePackageInputDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/Admin/GroomerService/Package/Active`

**کار:** تغییر بستهٔ خدمت / وضعیت فعالیت خدمت آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerServiceActiveDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

### `DELETE /api/Admin/GroomerService/Package`

**کار:** حذف بستهٔ خدمت خدمت آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

### `PUT /api/Admin/GroomerSetting`

**کار:** تغییر تنظیمات آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerAdminSettingsDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** `suggested`، `licenseApproved` و `commissionPercent` تنظیم مدیریتی‌اند؛ درصد در بازهٔ ۰ تا ۱۰۰ است.

### `PATCH /api/Admin/GroomerSetting/{id}/visibility`

**کار:** نمایش در سایت (فقط آرایشگاه تأییدشده و فعال)  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در مسیر؛ `dto: SiteVisibilityDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** انتشار در سایت با `showToSite` جدا از فعال/تأیید بودن آرایشگاه مدیریت می‌شود.

### `GET /api/Admin/GroomerUser`

**کار:** فهرست/مشاهده پرسنل آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `groomerId: long` در query  
**پاسخ 200:** `BaseResultDto<List<GroomerStaffVDto>>`

### `POST /api/Admin/GroomerUser`

**کار:** افزودن پرسنل با شماره موبایل (کاربر باید قبلاً ثبت‌نام کرده باشد)  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `dto: GroomerStaffAddDto` در بدنه JSON  
**پاسخ 200:** `BaseResultDto<GroomerStaffVDto>`

### `PUT /api/Admin/GroomerUser/Active`

**کار:** تغییر وضعیت فعالیت پرسنل آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query؛ `active: bool` در query  
**پاسخ 200:** `BaseResultDto`

### `DELETE /api/Admin/GroomerUser`

**کار:** حذف پرسنل آرایشگاه  
**دسترسی:** مدیریت با JWT و کنترل دسترسی همان ناحیه  
**ورودی:** `id: long` در query  
**پاسخ 200:** `BaseResultDto`

## پاسخ‌های مستقیمِ بدون envelope

این مسیرها شیء جستجو/صفحه را مستقیماً در body پاسخ 200 می‌دهند؛ در آن‌ها `isSuccess` وجود ندارد. فیلدهای صفحه‌بندی (`pageIndex`, `pageSize`, `totalCount`, `list`) و فیلترهای برگشتی در DTO همان پاسخ آمده‌اند. کلاینت باید شکل پاسخ را برای هر عملیات از ستون نوع پاسخ بخواند، نه اینکه همهٔ پاسخ‌ها را `data` فرض کند.

| مسیر | نوع مستقیم |
|---|---|
| `GET /api/Groomer` | `GroomerSearchDto` |
| `GET /api/GroomerPortfolio` | `GroomerPortfolioPage<GroomerPortfolioPostVDto>` |
| `GET /api/GroomerPortfolio/Comments` | `GroomerPortfolioPage<GroomerPortfolioCommentVDto>` |
| `GET /api/GroomerReview` | `GroomerReviewPage` |
| `GET /api/EndUser/GroomerReserve` | `GroomerReservePage` |
| `GET /api/EndUser/GroomerStaffInvite` | `List<GroomerStaffInviteVDto>` |
| `GET /api/Companion/Groomer` | `GroomerSearchDto` |
| `GET /api/Companion/GroomerPortfolio` | `GroomerPortfolioPage<GroomerPortfolioPostVDto>` |
| `GET /api/Companion/GroomerReserve` | `GroomerReservePage` |
| `GET /api/Companion/GroomerReview` | `GroomerReviewPage` |
| `GET /api/Companion/GroomerSettlement` | `SettlementSearchDto` |
| `GET /api/Admin/Groomer` | `GroomerSearchDto` |
| `GET /api/Admin/GroomerPortfolioComment` | `GroomerPortfolioPage<GroomerPortfolioCommentVDto>` |
| `GET /api/Admin/GroomerPortfolio` | `GroomerPortfolioPage<GroomerPortfolioPostVDto>` |
| `GET /api/Admin/GroomerReserve` | `GroomerReservePage` |
| `GET /api/Admin/GroomerReview` | `GroomerReviewPage` |

## مالکیت فیلدهای ورودی و خروجی

| بخش | قابل ارسال از کلاینت | تعیین‌شده/بازنویسی‌شده در سرور |
|---|---|---|
| ساخت/ویرایش آرایشگاه | مشخصات `GroomerDto` مانند نام، تماس، نشانی، تصویر، نوع خدمت، امکانات و SEO؛ در ساخت ادمین `providerId` باید ارائه‌دهندهٔ معتبر باشد. | در ساخت `ownerId` از کاربر ارائه‌دهنده و `providerId` از ارائه‌دهندهٔ معتبر تعیین می‌شود؛ `approved`، `active`، `showToSite`، `suggested`، `licenseApproved`، `commissionPercent` و شمارنده‌ها از DTO عمومی گرفته نمی‌شوند. در PUT، مالک و ارائه‌دهنده جابه‌جا نمی‌شوند. |
| ساخت آرایشگاه توسط Companion | مشخصات پروفایل؛ `providerId` فرستاده‌شده در کنترلر null می‌شود. | ارائه‌دهنده و مالک از هویت کاربر جاری حل می‌شوند؛ ایجاد/ویرایش صاحب آرایشگاه را در وضعیت نیازمند تأیید مدیر و خارج از رزرو جدید قرار می‌دهد. |
| ساخت رزرو | `groomerId`، بستهٔ اصلی/افزوده، `userPetId`، `doDate`، نوع خدمت، `addressId`، pickup، شخص انجام‌دهنده و یادداشت. | `bookerId` از JWT؛ قیمت، مبلغ آنلاین/حضوری، کمیسیون، سهم‌ها، وضعیت، مهلت پاسخ و کد رزرو از دیتابیس/قواعد سرور. |
| شروع پرداخت | `groomerReserveId` مربوط به رزرو خود کاربر و، برای مبلغ درگاه مثبت بدون پوشش کامل کیف پول، `merchantId` در `PaymentStartDto`. | `userId` و `user` در کنترلر، و مبلغ/هدف پرداخت/توکن درگاه در PaymentService تعیین یا بازنویسی می‌شوند. |
| جستجوی مشتری/مالک | فیلترهای مجاز مانند صفحه، متن، تاریخ، وضعیت. | `bookerId` در مسیر مشتری، `ownerUserId` و `onlyPaid=true` در مسیر Companion از هویت جاری بازنویسی می‌شوند؛ فیلتر مالکیت ارسالی از کلاینت ملاک نیست. |
| پاسخ عمومی | فقط پارامترهای query جستجو. | `ownerId` و `providerId` در خروجی عمومی پاک/خنثی می‌شوند؛ نمایش عمومی فقط برای آرایشگاه فعال و تأییدشده است. |

## سناریوی پایهٔ رزرو و پرداخت

1. از `GET /api/Groomer` و جزئیات/خدمات عمومی، آرایشگاه فعال و بستهٔ قابل رزرو را انتخاب کنید. برای `Quote`، `packageIds` را به صورت query تکراری بفرستید؛ `packageId` تکی نیز برای سازگاری پذیرفته می‌شود. خروجی مبلغ کل، آنلاین، حضوری، زمان کل و هزینهٔ pickup را تعیین می‌کند؛ قیمت‌های ارسالی کلاینت پذیرفته نمی‌شوند.
2. `Availability` را با همان بستهٔ اصلی و `extraPackageIds`، تاریخ و در صورت نیاز `assigneeUserId` بخوانید. `startDate` یکی از `slots` را به عنوان `doDate` در POST رزرو بفرستید؛ ظرفیت در زمان ثبت مجدداً اتمیک بررسی می‌شود، پس زمان مشاهده‌شده تضمین قطعی نیست.
3. `POST /api/EndUser/GroomerReserve` رزرو پرداخت‌نشده می‌سازد. در صورت نیاز `PUT .../Rebate` و `PUT .../Wallet` را روی شناسهٔ رزرو اعمال کنید؛ سپس `POST /api/EndUser/GroomerReservePayment` با `groomerReserveId` همان رزرو و `merchantId` هنگام نیاز به درگاه، پرداخت را آغاز می‌کند. `userId` و اطلاعات کاربر در سرور جای‌گذاری می‌شوند. پرداخت موفق از callback معتبر در سرور ثبت می‌شود و رزرو وارد انتظار تصمیم آرایشگاه می‌گردد؛ صرف بازگشت کاربر از درگاه اثبات پرداخت نیست.
4. مالک/نماینده یا مدیر، رزرو پرداخت‌شده را در مهلت پاسخ تأیید/رد می‌کند. رد و انقضای مهلت، مسیر لغو و استرداد به کیف پول را اجرا می‌کنند. پس از شروع و انجام خدمت، `Complete` مجاز می‌شود. برای نمایش، هم `status` و هم `ownerDecision` و `isCancel` را بخوانید.

```http
GET /api/EndUser/GroomerReserve/Quote?packageIds=701&packageIds=702&pickupRequested=false
GET /api/EndUser/GroomerReserve/Availability?groomerId=51&date=2026-11-01&packageId=701&extraPackageIds=702
```

```json
{"groomerId":51,"groomerServicePackageId":701,"extraPackageIds":[702],"userPetId":208,"doDate":"2026-11-01T10:00:00","isHomeService":false,"pickupRequested":false,"assigneeUserId":null,"bookerNote":""}
```

نمونهٔ شناسه‌ها و زمان بالا صرفاً شکل قرارداد را نشان می‌دهد؛ باید از پاسخ واقعی فهرست/availability همان محیط گرفته شوند.

## وضعیت‌ها، زمان‌بندی، ظرفیت و پول

| فیلد/قاعده | مقدار و اثر |
|---|---|
| `status` | `0=Registered` (پرداخت‌نشده)، `1=Paid`، `2=Complete` |
| `ownerDecision` | `0=NotRequired`، `1=Pending`، `2=Approved`، `3=Rejected`، `4=Expired` |
| سقف بسته‌ها | حداکثر ۵ بسته در هر رزرو؛ همه از یک آرایشگاه و هر خدمت فقط یک بار |
| مدت | مجموع حداکثر ۷۲۰ دقیقه؛ خدمت بدون مدت معتبر ۶۰ دقیقه؛ گام نمایش slot برابر ۳۰ دقیقه |
| پیش‌زمان و hold | شروع نوبت دست‌کم یک ساعت پس از اکنون؛ رزرو پرداخت‌نشده تا ۱۵ دقیقه ظرفیت را نگه می‌دارد |
| ساعت و ظرفیت | `startTime`/`endTime` قالب `HH:mm`، شروع کوچک‌تر از پایان، ظرفیت ردیف ۱ تا ۱۰۰۰، حداکثر ۱۰۰ ردیف و بدون هم‌پوشانی فعال در یک روز. ظرفیت مؤثر کمینهٔ ردیف و `maxConcurrentPets` مثبت آرایشگاه؛ هم‌پوشانی `[start,end)` و اشغال فرد انتخابی/حیوان بررسی می‌شود. |
| محدوده و تصویر | حداکثر ۵۰۰ محدودهٔ یکتا و ۳۰ تصویر گالری؛ شهر باید در استان و محله باید در شهر انتخابی باشد. |
| مهلت تصمیم | ۱۲ ساعت پس از پرداخت، محدود به ۳۰ دقیقه قبل از شروع، با کف ۳۰ دقیقه پس از اکنون |
| لغو مشتری | تا پیش از شروع نوبت ممکن است؛ اگر تصمیم مالک هنوز Pending باشد یا دست‌کم ۱۲ ساعت تا نوبت مانده باشد، مبلغ پرداخت‌شده به کیف پول برمی‌گردد. پس از تأیید و در کمتر از ۱۲ ساعت مانده به شروع، لغو می‌تواند بدون استرداد انجام شود. رزرو کامل‌شده/شروع‌شده لغو نمی‌شود. |
| قیمت | مجموع قیمت بسته‌ها + هزینهٔ pickup؛ آنلاین = مجموع پیش‌پرداخت معتبر بسته‌ها + pickup؛ باقی‌مانده = `price-onlineAmount` |
| کمیسیون | روی مبلغ پرداخت‌شده، هر خدمت با درصد خودش و در نبود آن درصد پیش‌فرض آرایشگاه؛ سهم‌ها به تومان گرد می‌شوند |

برای خدمت در محل، `addressId` متعلق به مشتری و داخل محدودهٔ پوشش لازم است؛ اگر محدوده‌ای تعریف نشده باشد شهر آرایشگاه مبناست. `pickupRequested` با خدمت در محل هم‌زمان مجاز نیست. `assigneeUserId` باید مالک یا پرسنل پذیرفته‌شده و فعال باشد و اگر برای خدمت افراد مشخص شده‌اند در همان لیست خدمت باشد.

## خطاهای کسب‌وکار مهم

| شناسهٔ پیام/شرط | برخورد کلاینت |
|---|---|
| `GroomerReservePackageInvalid`، `GroomerReserveTooManyPackages`، `GroomerReserveDuplicateService`، `GroomerReserveTooLong` | انتخاب بسته‌ها را بازسازی و دوباره Quote/Availability بگیرید. |
| `GroomerReserveTimeInPast`، `GroomerReserveOutsideWorkingHours`، `GroomerReserveSlotFull`، `GroomerReservePetBusy` | زمان‌های آزاد را دوباره واکشی کنید و انتخاب تازه بگیرید. |
| `GroomerReserveHomeServiceUnavailable`، `GroomerReserveSalonServiceUnavailable`، `GroomerReservePickupUnavailable`، `GroomerReserveAddressRequired`، `GroomerReserveAddressOutOfZone` | نوع خدمت، نشانی و pickup را با قابلیت آرایشگاه تطبیق دهید. |
| `GroomerReservePetInvalid`، `GroomerReservePetTypeNotAccepted`، `GroomerReservePetSizeMismatch`، `GroomerReserveAssigneeInvalid` | حیوان یا پرسنل مجاز را دوباره انتخاب کنید. |
| `GroomerReserveApprovalNotPending`، `GroomerReserveAwaitingOwnerApproval`، `GroomerReserveRescheduleNotAllowed`، `GroomerReserveNotPaid` | عملیات را متوقف کنید و وضعیت تازهٔ رزرو را بخوانید. |
| `GroomerReserveRefundFailed` | لغو/رد را موفق نشان ندهید؛ وضعیت رزرو و کیف پول را دوباره بخوانید. |
| `AccessDenied`، `NothingFound` | دسترسی/مالکیت و شناسه را بازبینی کنید؛ جزئیات رزرو شخص دیگر را نمایش ندهید. |

## فرهنگ DTO و فیلدها

نوع `?` در C# یعنی nullable. برای `string`های بدون `?`، چون مدل‌های فعلی الزام صریح یکسانی ندارند، صرف نوع را نشانهٔ الزامی بودن ندانید. قوانین الزامی و دامنهٔ مقادیر در سناریو و سرویس توضیح داده شده‌اند. فیلدهای خروجی که در DTO ورودی مشترک وجود دارند به معنی مجازبودن تغییر سمت کلاینت نیستند.

### `BankCardVDto`

پایه: `Id_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `bankName` | `string` | — |
| `cardPrefix` | `string` | — |

### `BankVDto`

پایه: `Name_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `pictureId` | `long?` | — |
| `label` | `string` | — |
| `picture` | `PictureVDto` | — |

### `BaseInputDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `pageIndex` | `int` | — |
| `pageSize` | `int` | تعداد در صفحه سقف ۱۰۰۰ (بزرگ‌ترین مقدار واقعی مصرف‌کننده‌ها: لیست برند/دسته در پنل). قبلاً بی‌سقف بود و هر endpoint عمومی را می‌شد با pageSize میلیونی یک‌جا خالی کرد. |
| `q` | `string` | متن قابل جستجو |
| `sortBy` | `SortEnum` | مرتب سازی بر اساس |
| `available` | `bool?` | — |

### `BaseSearchDto<Tdto>`

پایه: `BaseInputDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `totalCount` | `int` | — |
| `list` | `List<Tdto>` | — |

### `CityVDto`

پایه: `Name_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `stateId` | `long` | — |
| `state` | `StateVDto` | — |

### `CompanionMinVDto`

پایه: `Name_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `isPersonal` | `bool` | — |
| `ownerId` | `long` | — |
| `providerId` | `long?` | ارائه‌دهنده‌ی صاحب این مکان؛ اشتراک از روی همین مدیریت می‌شود. |
| `pictureId` | `long?` | — |
| `backgroundPictureId` | `long?` | — |
| `iconId` | `long?` | — |
| `active` | `bool` | — |
| `activationValue` | `string` | — |
| `phone` | `string` | — |
| `goldAccountDate` | `DateTime?` | — |
| `silverAccountDate` | `DateTime?` | — |
| `commentCount` | `int` | — |
| `rateAvg` | `double` | — |
| `rateCount` | `int` | — |
| `isSilver` | `bool` | — |
| `isGold` | `bool` | — |
| `owner` | `UserMinVDto` | — |
| `picture` | `PictureVDto` | — |
| `backgroundPicture` | `PictureVDto` | — |
| `icon` | `PictureVDto` | — |

### `CountryVDto`

پایه: `Name_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `enName` | `string` | — |

### `GroomerActiveDto`

پایه: `Id_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `active` | `bool` | — |

### `GroomerAdminSettingsDto`

فیلدهایی که فقط ادمین تغییر می‌دهد.

پایه: `Id_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `suggested` | `bool` | — |
| `licenseApproved` | `bool` | — |
| `commissionPercent` | `decimal` | درصد کمیسیون پیش‌فرض (۰ تا ۱۰۰). |

### `GroomerApproveDto`

پایه: `Id_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `approve` | `bool` | — |
| `approvalValue` | `string` | — |

### `GroomerAvailabilityDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `date` | `DateTime` | — |
| `durationMinutes` | `int` | — |
| `slots` | `List<GroomerAvailabilitySlotDto>` | — |

### `GroomerAvailabilitySlotDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `time` | `string` | HH:mm |
| `startDate` | `DateTime` | — |
| `capacity` | `int` | — |
| `available` | `int` | — |

### `GroomerDto`

ورودی ساخت/ویرایش آرایشگاه (فیلدهای مدیریتی مثل تأیید و کمیسیون در DTOهای جدا هستند).

پایه: `Name_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `providerId` | `long?` | — |
| `ownerId` | `long?` | اگر خالی باشد مالک = کاربر ارائه‌دهنده. |
| `isPersonal` | `bool` | — |
| `pictureId` | `long?` | — |
| `backgroundPictureId` | `long?` | — |
| `iconId` | `long?` | — |
| `phone` | `string` | — |
| `addressValue` | `string` | — |
| `stateId` | `long` | — |
| `cityId` | `long` | — |
| `neighborhoodId` | `long?` | — |
| `location` | `PointDto` | — |
| `summary` | `string` | — |
| `description` | `string` | — |
| `regulations` | `string` | — |
| `establishedYear` | `int?` | — |
| `licenseNumber` | `string` | — |
| `instagramUrl` | `string` | — |
| `websiteUrl` | `string` | — |
| `serviceAtHome` | `bool` | — |
| `serviceInSalon` | `bool` | — |
| `pickupAvailable` | `bool` | — |
| `pickupFee` | `double` | — |
| `hasParking` | `bool` | — |
| `hasWaitingRoom` | `bool` | — |
| `hasCamera` | `bool` | — |
| `maxConcurrentPets` | `int` | — |
| `seoH1` | `string` | — |
| `seoMinDescription` | `string` | — |
| `seoDescription` | `string` | — |
| `seoTitle` | `string` | — |
| `seoPictureAlt` | `string` | — |
| `seoUrlText` | `string` | — |
| `seoNoIndex` | `bool` | — |
| `seoNoFollow` | `bool` | — |
| `seoCanonical` | `string` | — |

### `GroomerInputDto`

پایه: `BaseInputDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `providerId` | `long?` | — |
| `ownerId` | `long?` | — |
| `approve` | `bool?` | — |
| `suggested` | `bool?` | — |
| `showToSite` | `bool?` | — |
| `stateId` | `long?` | — |
| `cityId` | `long?` | — |
| `neighborhoodIds` | `List<long>` | — |
| `petId` | `long?` | — |
| `serviceAtHome` | `bool?` | — |
| `serviceInSalon` | `bool?` | — |
| `pickupAvailable` | `bool?` | — |
| `assistanceId` | `long?` | — |

### `GroomerMinVDto`

خلاصه‌ی آرایشگاه برای لیست‌ها.

پایه: `Name_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `providerId` | `long?` | — |
| `ownerId` | `long` | — |
| `active` | `bool` | — |
| `approved` | `bool` | — |
| `showToSite` | `bool` | — |
| `suggested` | `bool` | — |
| `summary` | `string` | — |
| `phone` | `string` | — |
| `addressValue` | `string` | — |
| `cityId` | `long` | — |
| `commentCount` | `int` | — |
| `rateAvg` | `double` | — |
| `rateCount` | `int` | — |
| `portfolioCount` | `int` | — |
| `portfolioLikeCount` | `int` | — |
| `serviceAtHome` | `bool` | — |
| `serviceInSalon` | `bool` | — |
| `goldAccountDate` | `DateTime?` | — |
| `silverAccountDate` | `DateTime?` | — |
| `picture` | `PictureVDto` | — |
| `icon` | `PictureVDto` | — |
| `city` | `CityVDto` | — |

### `GroomerOwnerVDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `firstName` | `string` | — |
| `lastName` | `string` | — |
| `mobile` | `string` | — |

### `GroomerPetsDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `groomerId` | `long` | — |
| `petIds` | `List<long>` | — |

### `GroomerPetsRequestDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `groomerId` | `long` | — |
| `petIds` | `List<long>` | — |

### `GroomerPictureAddDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `groomerId` | `long` | — |
| `pictureId` | `long` | — |
| `label` | `string` | — |

### `GroomerPictureVDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `groomerId` | `long` | — |
| `pictureId` | `long` | — |
| `label` | `string` | — |
| `picture` | `PictureVDto` | — |

### `GroomerPortfolioCommentInputDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `postId` | `long` | — |
| `text` | `string` | — |
| `parentId` | `long?` | اگر پر باشد پاسخ به یک نظر است (پاسخ به پاسخ به نظر اصلی وصل می‌شود). |

### `GroomerPortfolioCommentSearchInputDto`

پایه: `BaseInputDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `postId` | `long` | — |
| `parentId` | `long?` | خالی = نظرهای اصلی؛ پر = پاسخ‌های همان نظر. |

### `GroomerPortfolioCommentVDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `postId` | `long` | — |
| `parentId` | `long?` | — |
| `text` | `string` | — |
| `createDate` | `DateTime` | — |
| `userId` | `long?` | — |
| `userName` | `string` | — |
| `isMine` | `bool` | — |
| `replyCount` | `int` | — |

### `GroomerPortfolioLikeResultDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `liked` | `bool` | — |
| `likeCount` | `int` | — |

### `GroomerPortfolioModerationDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `approved` | `bool` | — |
| `approvalValue` | `string` | — |

### `GroomerPortfolioPage<T>`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `pageIndex` | `int` | — |
| `pageSize` | `int` | — |
| `totalCount` | `int` | — |
| `list` | `List<T>` | — |

### `GroomerPortfolioPictureInputDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `pictureId` | `long` | — |
| `kind` | `int` | — |
| `label` | `string` | — |

### `GroomerPortfolioPictureVDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `pictureId` | `long` | — |
| `kind` | `int` | — |
| `sortOrder` | `int` | — |
| `label` | `string` | — |
| `picture` | `PictureVDto` | — |

### `GroomerPortfolioPostInputDto`

ساخت/ویرایش پست نمونه‌کار. ترتیب Pictures همان ترتیب نمایش است.

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `groomerId` | `long` | — |
| `caption` | `string` | — |
| `petId` | `long?` | — |
| `groomerServiceId` | `long?` | — |
| `pictures` | `List<GroomerPortfolioPictureInputDto>` | — |

### `GroomerPortfolioPostSearchInputDto`

پایه: `BaseInputDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `groomerId` | `long?` | — |
| `cityId` | `long?` | — |
| `petId` | `long?` | — |
| `groomerServiceId` | `long?` | — |
| `approved` | `bool?` | فقط برای ادمین/مالک: فیلتر وضعیت تأیید (در فید عمومی نادیده گرفته می‌شود). |

### `GroomerPortfolioPostVDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `groomerId` | `long` | — |
| `groomerName` | `string` | — |
| `groomerIcon` | `PictureVDto` | — |
| `caption` | `string` | — |
| `petId` | `long?` | — |
| `petName` | `string` | — |
| `groomerServiceId` | `long?` | — |
| `serviceName` | `string` | — |
| `createDate` | `DateTime` | — |
| `likeCount` | `int` | — |
| `commentCount` | `int` | — |
| `likedByMe` | `bool` | کاربر جاری این پست را لایک کرده است (برای کاربر ناشناس همیشه false). |
| `approved` | `bool` | — |
| `active` | `bool` | — |
| `approvalValue` | `string` | — |
| `pictures` | `List<GroomerPortfolioPictureVDto>` | — |

### `GroomerProfileSetDto<T>`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `groomerId` | `long` | — |
| `items` | `List<T>` | — |

### `GroomerProviderVDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `userId` | `long` | — |
| `active` | `bool` | — |
| `approved` | `bool` | — |

### `GroomerReserveInputDto`

ورودی ساخت رزرو توسط کاربر. مبلغ‌ها از بدنه پذیرفته نمی‌شوند؛ همه از روی بسته‌ی قیمتی سرور محاسبه می‌شوند.

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `groomerId` | `long` | — |
| `groomerServicePackageId` | `long` | — |
| `userPetId` | `long` | — |
| `extraPackageIds` | `List<long>` | بسته‌های اضافه (رزرو چندخدمتی)؛ بسته‌ی اصلی در GroomerServicePackageId است. حداکثر ۵ بسته، هر خدمت یک‌بار. |
| `doDate` | `DateTime` | زمان شروع نوبت (باید روی یکی از زمان‌های آزاد آرایشگاه باشد). |
| `isHomeService` | `bool` | — |
| `addressId` | `long?` | — |
| `pickupRequested` | `bool` | — |
| `assigneeUserId` | `long?` | — |
| `bookerNote` | `string` | — |

### `GroomerReserveItemVDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `groomerServiceId` | `long` | — |
| `serviceName` | `string` | — |
| `groomerServicePackageId` | `long` | — |
| `packageName` | `string` | — |
| `packagePrice` | `double` | — |
| `onlineAmount` | `double` | — |
| `durationMinutes` | `int` | — |
| `commissionPercent` | `decimal` | — |
| `providerShare` | `double` | — |
| `siteShare` | `double` | — |

### `GroomerReservePage`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `pageIndex` | `int` | — |
| `pageSize` | `int` | — |
| `totalCount` | `int` | — |
| `list` | `List<GroomerReserveVDto>` | — |

### `GroomerReserveQuoteDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `groomerServicePackageId` | `long` | — |
| `packageIds` | `List<long>` | — |
| `packagePrice` | `double` | — |
| `pickupFee` | `double` | — |
| `price` | `double` | — |
| `onlineAmount` | `double` | مبلغی که همین حالا آنلاین پرداخت می‌شود (قبل از تخفیف). |
| `payAtVenue` | `double` | باقی‌مانده که حضوری در آرایشگاه پرداخت می‌شود. |
| `durationMinutes` | `int` | — |

### `GroomerReserveReasonDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `reason` | `string` | — |

### `GroomerReserveRebateCodeDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `rebateCode` | `string` | — |

### `GroomerReserveRescheduleDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `newDoDate` | `DateTime` | — |

### `GroomerReserveSearchInputDto`

پایه: `BaseInputDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `bookerId` | `long?` | — |
| `groomerId` | `long?` | — |
| `providerId` | `long?` | — |
| `ownerUserId` | `long?` | فقط رزروهای آرایشگاه‌های این مالک (اپ نماینده). |
| `userPetId` | `long?` | — |
| `status` | `int?` | — |
| `ownerDecision` | `int?` | — |
| `isCancel` | `bool?` | — |
| `onlyPaid` | `bool?` | true = فقط رزروهای پرداخت‌شده (پیش‌فرض ادمین/مالک)؛ false/خالی = همه. |
| `dateFrom` | `DateTime?` | — |
| `dateTo` | `DateTime?` | — |

### `GroomerReserveVDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `rescheduleCount` | `int` | — |
| `originalDoDate` | `DateTime?` | — |
| `items` | `List<GroomerReserveItemVDto>` | — |
| `id` | `long` | — |
| `reserveCode` | `string` | — |
| `groomerId` | `long` | — |
| `groomerName` | `string` | — |
| `groomerPhone` | `string` | — |
| `groomerAddress` | `string` | — |
| `groomerServiceId` | `long` | — |
| `serviceName` | `string` | — |
| `groomerServicePackageId` | `long` | — |
| `packageName` | `string` | — |
| `packagePetSize` | `string` | — |
| `bookerId` | `long` | — |
| `bookerName` | `string` | — |
| `bookerMobile` | `string` | — |
| `userPetId` | `long` | — |
| `petName` | `string` | — |
| `assigneeUserId` | `long?` | — |
| `assigneeName` | `string` | — |
| `doDate` | `DateTime` | — |
| `endDate` | `DateTime` | — |
| `durationMinutes` | `int` | — |
| `isHomeService` | `bool` | — |
| `addressId` | `long?` | — |
| `addressValue` | `string` | — |
| `pickupRequested` | `bool` | — |
| `bookerNote` | `string` | — |
| `packagePrice` | `double` | — |
| `pickupFee` | `double` | — |
| `price` | `double` | — |
| `onlineAmount` | `double` | — |
| `payAtVenue` | `double` | — |
| `fromWallet` | `bool` | — |
| `walletPrice` | `double` | — |
| `rebateId` | `long?` | — |
| `rebatePrice` | `double` | — |
| `paymentPrice` | `double` | — |
| `status` | `int` | — |
| `isReserved` | `bool` | — |
| `isCancel` | `bool` | — |
| `cancelDetail` | `string` | — |
| `cancelDate` | `DateTime?` | — |
| `cancelledByBooker` | `bool` | — |
| `createDate` | `DateTime` | — |
| `paidDate` | `DateTime?` | — |
| `doneDate` | `DateTime?` | — |
| `ownerDecision` | `int` | — |
| `ownerDecisionDate` | `DateTime?` | — |
| `ownerDecisionReason` | `string` | — |
| `ownerApprovalDeadline` | `DateTime?` | — |
| `providerShare` | `double` | فقط برای ادمین/مالک: سهم‌ها بعد از پرداخت. |
| `siteShare` | `double` | — |
| `commissionPercent` | `decimal` | — |
| `permitted` | `bool` | — |
| `freeCancelAvailable` | `bool` | کاربر هنوز می‌تواند با لغو رایگان کل مبلغ را پس بگیرد. |

### `GroomerReserveWalletDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `fromWallet` | `bool` | — |

### `GroomerReviewAnswerDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `answer` | `string` | — |

### `GroomerReviewInputDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `groomerReserveId` | `long` | — |
| `rate` | `int` | ۱ تا ۵ |
| `text` | `string` | — |

### `GroomerReviewOwnerAnswerDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `answer` | `string` | — |

### `GroomerReviewPage`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `pageIndex` | `int` | — |
| `pageSize` | `int` | — |
| `totalCount` | `int` | — |
| `list` | `List<GroomerReviewVDto>` | — |

### `GroomerReviewSearchInputDto`

پایه: `BaseInputDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `groomerId` | `long?` | — |
| `statusId` | `long?` | — |
| `ownerUserId` | `long?` | — |

### `GroomerReviewVDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `groomerId` | `long` | — |
| `groomerName` | `string` | — |
| `groomerReserveId` | `long?` | — |
| `rate` | `int?` | — |
| `text` | `string` | — |
| `answer` | `string` | — |
| `createDate` | `DateTime` | — |
| `userId` | `long?` | — |
| `userName` | `string` | — |
| `statusId` | `long` | ۱ منتظر بررسی، ۲ تأییدشده، ۳ ردشده (CommentEnum) |

### `GroomerSearchDto`

پایه: `BaseSearchDto<Groomer, GroomerMinVDto>`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `providerId` | `long?` | — |
| `ownerId` | `long?` | — |
| `approve` | `bool?` | — |
| `suggested` | `bool?` | — |
| `showToSite` | `bool?` | — |
| `stateId` | `long?` | — |
| `cityId` | `long?` | — |
| `petId` | `long?` | — |

### `GroomerServiceActiveDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `active` | `bool` | — |

### `GroomerServiceActiveRequestDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `active` | `bool` | — |

### `GroomerServiceApproveDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `approve` | `bool` | — |
| `approvalValue` | `string` | — |
| `commissionPercent` | `decimal?` | درصد کمیسیون سایت از این خدمت (۰ تا ۱۰۰)؛ خالی = بدون تغییر. |

### `GroomerServiceInputDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `groomerId` | `long` | — |
| `assistanceId` | `long` | خدمت از کاتالوگ سایت (Assistance)؛ بعد از ساخت قابل تغییر نیست. |
| `durationMinutes` | `int` | — |
| `description` | `string` | — |

### `GroomerServicePackageInputDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `groomerServiceId` | `long` | — |
| `name` | `string` | — |
| `price` | `double` | — |
| `prePaymentPrice` | `double` | — |
| `pictureId` | `long?` | — |
| `discription` | `string` | — |
| `petSize` | `string` | Small/Medium/Large؛ خالی = همه‌ی سایزها. |

### `GroomerServicePackageVDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `groomerServiceId` | `long` | — |
| `name` | `string` | — |
| `price` | `double` | — |
| `prePaymentPrice` | `double` | — |
| `pictureId` | `long?` | — |
| `picture` | `PictureVDto` | — |
| `active` | `bool` | — |
| `activationValue` | `string` | — |
| `discription` | `string` | — |
| `petSize` | `string` | — |

### `GroomerServiceUsersDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `groomerServiceId` | `long` | — |
| `userIds` | `List<long>` | — |

### `GroomerServiceUsersRequestDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `groomerServiceId` | `long` | — |
| `userIds` | `List<long>` | — |

### `GroomerServiceVDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `groomerId` | `long` | — |
| `assistanceId` | `long` | — |
| `assistanceName` | `string` | — |
| `assistancePicture` | `PictureVDto` | — |
| `active` | `bool` | — |
| `approved` | `bool` | — |
| `activationValue` | `string` | — |
| `commissionPercent` | `decimal` | — |
| `durationMinutes` | `int` | — |
| `description` | `string` | — |
| `packages` | `List<GroomerServicePackageVDto>` | — |
| `userIds` | `List<long>` | — |

### `GroomerStaffAddDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `groomerId` | `long` | — |
| `mobile` | `string` | کاربر باید قبلاً در پاستیل ثبت‌نام کرده باشد. |

### `GroomerStaffInviteVDto`

دعوت‌نامه‌ی عضویت در پرسنل یک آرایشگاه، از دید کاربرِ دعوت‌شده.

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `groomerId` | `long` | — |
| `groomerName` | `string` | — |
| `groomerPhone` | `string` | — |
| `inviteDate` | `System.DateTime?` | — |

### `GroomerStaffVDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `groomerId` | `long` | — |
| `userId` | `long` | — |
| `firstName` | `string` | — |
| `lastName` | `string` | — |
| `mobile` | `string` | — |
| `active` | `bool` | — |
| `activationValue` | `string` | — |
| `userAccept` | `bool?` | null = دعوت منتظر پاسخ، true = پذیرفته، false = ردشده. |
| `inviteDate` | `System.DateTime?` | — |

### `GroomerTimeInputDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `weekDayId` | `long` | — |
| `startTime` | `string` | HH:mm |
| `endTime` | `string` | HH:mm |
| `capacity` | `int` | — |
| `active` | `bool` | — |

### `GroomerTimeVDto`

پایه: `GroomerTimeInputDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `weekDayName` | `string` | — |

### `GroomerVDto`

پایه: `GroomerMinVDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `activationValue` | `string` | — |
| `legacyCompanionId` | `long?` | — |
| `isPersonal` | `bool` | — |
| `stateId` | `long?` | — |
| `neighborhoodId` | `long?` | — |
| `pictureId` | `long?` | — |
| `backgroundPictureId` | `long?` | — |
| `iconId` | `long?` | — |
| `location` | `PointDto` | — |
| `deleted` | `bool` | — |
| `deleteDate` | `DateTime?` | — |
| `createDate` | `DateTime` | — |
| `description` | `string` | — |
| `regulations` | `string` | — |
| `establishedYear` | `int?` | — |
| `licenseNumber` | `string` | — |
| `licenseApproved` | `bool` | — |
| `instagramUrl` | `string` | — |
| `websiteUrl` | `string` | — |
| `pickupAvailable` | `bool` | — |
| `pickupFee` | `double` | — |
| `hasParking` | `bool` | — |
| `hasWaitingRoom` | `bool` | — |
| `hasCamera` | `bool` | — |
| `maxConcurrentPets` | `int` | — |
| `staffCount` | `int` | — |
| `commissionPercent` | `decimal` | — |
| `seoH1` | `string` | — |
| `seoMinDescription` | `string` | — |
| `seoDescription` | `string` | — |
| `seoTitle` | `string` | — |
| `seoPictureAlt` | `string` | — |
| `seoUrlText` | `string` | — |
| `seoNoIndex` | `bool` | — |
| `seoNoFollow` | `bool` | — |
| `seoCanonical` | `string` | — |
| `owner` | `GroomerOwnerVDto` | — |
| `provider` | `GroomerProviderVDto` | — |
| `state` | `StateVDto` | — |
| `backgroundPicture` | `PictureVDto` | — |
| `petIds` | `List<long>` | — |

### `GroomerZoneInputDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `stateId` | `long` | — |
| `cityId` | `long` | — |
| `neighborhoodId` | `long?` | — |

### `GroomerZoneVDto`

پایه: `GroomerZoneInputDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `stateName` | `string` | — |
| `cityName` | `string` | — |
| `neighborhoodName` | `string` | — |

### `Id_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |

### `MerchantVDto`

پایه: `Id_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `bankId` | `long` | — |
| `active` | `bool` | — |
| `bank` | `BankVDto` | — |

### `Name_FieldDto`

پایه: `Id_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `name` | `string` | — |

### `NeighborhoodVDto`

پایه: `Name_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `regionNumber` | `int` | — |
| `cityId` | `long` | — |
| `city` | `CityVDto` | — |

### `PaymentStartDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `paymentCode` | `string` | — |
| `isOnline` | `bool` | — |
| `productOrderId` | `string` | — |
| `companionReserveId` | `long?` | — |
| `pansionReserveId` | `long?` | — |
| `schoolReserveId` | `long?` | — |
| `groomerReserveId` | `long?` | — |
| `companionReserveBatchId` | `long?` | — |
| `tripId` | `long?` | — |
| `cargoId` | `long?` | — |
| `companionInsurancePackageSaleId` | `long?` | — |
| `paymentId` | `long` | — |
| `merchantId` | `long?` | — |
| `rebateId` | `long?` | — |
| `amount` | `double` | — |
| `grossAmount` | `double` | — |
| `rebateAmount` | `double` | — |
| `walletAmount` | `double` | — |
| `merchant` | `MerchantVDto` | — |
| `paymentUrl` | `string` | — |
| `paymentIsLink` | `bool` | — |
| `isTestMode` | `bool` | — |
| `testSuccessUrl` | `string` | — |
| `testFailureUrl` | `string` | — |
| `userId` | `long?` | — |
| `typeId` | `long` | — |
| `callBackTypeLabel` | `string` | — |
| `callBackId` | `string` | — |
| `callbackUrl` | `string` | — |
| `gatewayToken` | `string` | — |
| `user` | `UserMinVDto` | — |

### `PictureVDto`

پایه: `Id_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `url` | `string` | — |
| `orginalName` | `string` | — |
| `baseUrl` | `string` | — |
| `guidName` | `string` | — |
| `extension` | `string` | — |

### `PointDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `x` | `double` | — |
| `y` | `double` | — |
| `distanceMeter` | `double` | — |

### `ProviderMinVDto`

نمای خلاصه‌ی ارائه‌دهنده برای لیست‌هایی مثل تسویه‌ها (نیازمند Include کاربر).

پایه: `Id_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `userId` | `long` | — |
| `active` | `bool` | — |
| `user` | `UserMinVDto` | — |

### `SettlementInputDto`

پایه: `BaseInputDto, ISettlementSearchFields`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `providerId` | `long?` | — |
| `kind` | `Entities.Entities.SettlementKindEnum?` | — |
| `storeId` | `long?` | — |
| `companionId` | `long?` | — |

### `SettlementItemVDto`

یک ردیف از ریز تسویه: چیزی که داخل این تسویه پرداخت شده است.

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `type` | `string` | Reserve \| Pansion \| Consultation \| School \| Order \| Groomer |
| `code` | `string` | — |
| `venueName` | `string` | — |
| `bookerFullName` | `string` | — |
| `paymentPrice` | `double` | — |
| `siteShare` | `double` | — |
| `providerShare` | `double` | — |

### `SettlementSearchDto`

پایه: `BaseSearchDto<Settlement, SettlementVDto>, ISettlementSearchFields`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `providerId` | `long?` | — |
| `kind` | `Entities.Entities.SettlementKindEnum?` | — |
| `storeId` | `long?` | — |
| `companionId` | `long?` | — |

### `SettlementVDto`

پایه: `Id_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `providerId` | `long?` | — |
| `kind` | `Entities.Entities.SettlementKindEnum` | — |
| `storeId` | `long?` | — |
| `companionId` | `long?` | — |
| `createDate` | `DateTime` | — |
| `userBankCardId` | `long` | — |
| `trackingCode` | `string` | — |
| `paidPrice` | `double` | — |
| `itemCount` | `long` | — |
| `userBankCard` | `UserBankCardVDto` | — |
| `provider` | `Application.Services.ProviderSrvs.ProviderSrv.Dto.ProviderMinVDto` | — |
| `companion` | `CompanionMinVDto` | — |
| `store` | `StoreMinVDto` | — |

### `SiteVisibilityDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `showToSite` | `bool` | — |

### `StateVDto`

پایه: `Name_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `enName` | `string` | — |
| `countryId` | `long` | — |
| `country` | `CountryVDto` | — |

### `StoreMinVDto`

پایه: `Name_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `storeId` | `long` | — |
| `maxDiscountPercent` | `int` | — |
| `rateAvg` | `double` | — |
| `rateCount` | `int` | — |
| `expressShippingEnabled` | `bool` | این فروشگاه ارسال اکسپرس (۲ تا ۳ ساعت کاری) دارد؛ برای نشان «ارسال اکسپرس» روی محصول. |
| `picture` | `PictureVDto` | — |

### `UserBankCardVDto`

پایه: `Id_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `userId` | `long` | — |
| `cardNumber` | `string` | — |
| `shebaNumber` | `string` | — |
| `bankCardId` | `long` | — |
| `cardHolderName` | `string` | — |
| `createDate` | `DateTime` | — |
| `lastUpdateDate` | `DateTime` | — |
| `approved` | `bool` | — |
| `adminDetail` | `string` | — |
| `bankCard` | `BankCardVDto` | — |
| `user` | `UserVDto` | — |

### `UserCurrentLocationVDto`

پایه: `Id_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `userId` | `long` | — |
| `location` | `PointDto` | — |
| `cityId` | `long` | — |
| `neighborhoodId` | `long?` | — |
| `lastUpdateDate` | `DateTime` | — |
| `city` | `CityVDto` | — |
| `neighborhood` | `NeighborhoodVDto` | — |

### `UserMinVDto`

پایه: `Id_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `mobile` | `string` | — |
| `email` | `string` | — |
| `firstName` | `string` | — |
| `lastName` | `string` | — |
| `fullName` | `string` | — |
| `referralCode` | `string` | — |
| `isFemale` | `bool` | — |
| `expertise` | `string` | — |
| `pictureId` | `long` | — |

### `UserVDto`

پایه: `Id_FieldDto`

| فیلد | نوع C# / JSON | توضیح صریح سورس |
|---|---|---|
| `mobile` | `string` | — |
| `email` | `string` | — |
| `firstName` | `string` | — |
| `lastName` | `string` | — |
| `fullName` | `string` | — |
| `referralCode` | `string` | — |
| `registrationReferralSource` | `Entities.Entities.Security.RegistrationReferralSource` | — |
| `usedReferralCode` | `string` | — |
| `referredByUserId` | `long?` | — |
| `referredByCompanionId` | `long?` | — |
| `referredByStoreId` | `long?` | — |
| `pictureId` | `long?` | — |
| `companionId` | `long?` | — |
| `driverId` | `long?` | — |
| `expertise` | `string` | — |
| `isCompanion` | `bool` | — |
| `createDate` | `DateTime` | — |
| `roleId` | `long` | — |
| `roleName` | `string` | — |
| `isFemale` | `bool` | — |
| `picture` | `PictureVDto` | — |
| `userCurrentLocation` | `UserCurrentLocationVDto` | — |

## قاعدهٔ نگهداری سند

این صفحه snapshot قابل بازتولید از کنترلر/DTO سورس است. با تغییر route یا DTO، اجرای `python docs/groomer/build_api_reference.py` جدول‌ها و فرهنگ فیلدها را هم‌زمان به‌روزرسانی می‌کند. مقدار واقعی پیکربندی و انتشار عملیاتی باید از محیط استقرار خوانده شود و از این سند استنتاج نمی‌شود.
