# قرارداد جامع API پانسیون و مهد پاستیل

این صفحه از route، کنترلر، سرویس و DTOهای سورس فعلی تهیه شده است. مسیرها، شکل ورودی و خروجی و قواعد اجرایی لازم در همین صفحه آمده‌اند. این قرارداد snapshot کد است و به‌تنهایی استقرار Production را اثبات نمی‌کند. نام JSON فیلدها در مثال‌ها camelCase است.

**پوشش:** 72 عملیات در چهار سطح؛ 106 تعریف DTO با 1054 فیلد. 58 عملیات پاسخ envelope دارند و بقیه پاسخ مستقیم می‌دهند.

## قواعد مشترک دسترسی، مدل خطا و پاسخ

- `BaseResultDto<T>` چهار بخش `isSuccess`، `messages`، `code` و `data` دارد؛ نوع بدون `<T>` فیلد `data` ندارد. `messages` آرایهٔ جفت پیام است. برای تصمیم موفقیت صرفاً HTTP 200 کافی نیست: `isSuccess` را بررسی کنید. پاسخ‌های `SearchDto` مستقیم‌اند و envelope ندارند.
- مسیرهای عمومی `/api/Pansion`، `/api/PansionReserve/{PansionId}`، `/api/PansionSiteMap` و `/api/Site/Pansion` بدون `[Authorize]` یا دارای `[AllowAnonymous]` هستند. `GET /api/EndUser/PansionComment` نیز استثنائاً آزاد است. باقی EndUser، Companion و Admin به JWT معتبر نیاز دارند.
- دسترسی Admin با policy مرکزی ناحیه کنترل می‌شود: نقش مدیر یا permission معتبر همان controller/action. در Companion، policy عضویت ناحیه به تنظیم `Security:AreaPolicyMode` وابسته است؛ مالکیت در سرویس/کنترلر باید جداگانه بررسی شود. route جستجو و جزئیات رزرو نماینده به علت غلط‌املایی `[Area]` با `/api/Comapnion/` شروع می‌شود.
- **شکاف دسترسی فعلی:** `[Area("Comapnion")]` با فهرست convention (`Seller/Companion/Driver`) تطبیق ندارد؛ پس دو GET رزرو `/api/Comapnion/PansionReserve` فقط `[Authorize]` دارند و policy عضویت `AreaMember:Companion` نمی‌گیرند. وقتی `CurrentUser.CompanionId` تهی است، سرویس فیلتر مالکیت را هم حذف می‌کند؛ جستجو و جزئیات ممکن است رزرو دیگران را افشا کنند. تا اصلاح بک‌اند، هیچ کلاینت مشتری نباید این مسیر را proxy کند؛ مسیر مشتری `GET /api/EndUser/PansionReserve/{id}` است.
- **شکاف‌های هم‌خانوادهٔ Companion:** `PUT /api/Companion/PansionActive` و `PUT /api/Companion/PansionReserveChangeStatus` نیز `CompanionId` nullable را به سرویسی می‌دهند که فقط وقتی مقدار داشته باشد مالکیت را بررسی می‌کند؛ در حالت Audit کاربر صرفاً JWTدار و در حالت Enforce عضو دارای `IsCompanionUser` ولی بدون CompanionId ممکن است این actionها را بدون مالکیت اجرا کند. جستجوی Pansion/PansionPet/PansionPicture با CompanionId تهی نیز بی‌فیلتر و جزئیات Pet/Picture بدون کنترل مالکیت‌اند. این موارد نیاز به اصلاح بک‌اند دارند.
- `[ApiController]` برای بدنه/پارامتر نامعتبر می‌تواند HTTP 400، نبود توکن 401 و نبود مجوز 403 بدهد. بعضی جزئیات غیرمجاز HTTP 404 دارند؛ بیشتر خطاهای دامنه در HTTP 200 با `isSuccess=false` هستند. پیام متنی را قرارداد پایدار تصمیم‌گیری فرض نکنید.

```json
{"isSuccess":true,"messages":[],"code":0,"data":{"id":123}}
```

## ترتیب امن فراخوانی از دید کلاینت

1. فهرست عمومی سایت را از `GET /api/Site/Pansion` بگیرید؛ `isDaycare=false` پانسیون شبانه و `isDaycare=true` مهد ساعتی را جدا می‌کند. از جزئیات همین مسیر برای صفحهٔ معرفی استفاده کنید.
2. پس از ورود، `POST /api/EndUser/PansionReserve` با شناسهٔ مرکز و پت و تاریخ‌های اقامت یا تاریخ/ساعت مهد بفرستید. مبلغ و تعداد شب/ساعت را از محاسبهٔ سرور بخوانید؛ تداخل رزرو در زمان ثبت دوباره بررسی می‌شود. ارسال قیمت یا وضعیت از کلاینت قابل اتکا نیست.
3. اگر تخفیف یا کیف پول لازم است، فقط روی رزرو پرداخت‌نشدهٔ خودتان به‌ترتیب موردنظر اعمال کنید و جزئیات را دوباره بخوانید. سپس `POST /api/EndUser/PansionReservePayment` را با `pansionReserveId` اجرا کنید. بعد از callback معتبر، رزرو پرداخت‌شده در انتظار تأیید مرکز قرار می‌گیرد.
4. برای نمایش سفارش بعد از پرداخت، `statusId`، `isReserved`، `isCancel` و `ownerDecision` را با هم بخوانید. مرکز/مدیر تا مهلت پاسخ تأیید یا رد می‌کند؛ رد یا انقضای مهلت به لغو و بازگشت وجه به کیف پول منجر می‌شود. سفر اختیاری پت‌رسان با `TripPansionReservation` جدا ساخته و جدا پرداخت می‌شود.

## قواعد وضعیت، زمان، قیمت و مالکیت فیلد

| موضوع | قرارداد اجرایی |
|---|---|
| وضعیت رزرو `statusId` | ۴۶ = Registered، ۴۷ = Paid، ۴۸ = Complete؛ `isReserved` و `isCancel` نیز مستقل‌اند. |
| تصمیم مرکز `ownerDecision` | ۰ = NotRequired، ۱ = Pending، ۲ = Approved، ۳ = Rejected، ۴ = Expired. تکمیل به Approved یا NotRequired نیاز دارد. |
| مهلت تصمیم | حداکثر ۱۲ ساعت از پرداخت و حداکثر تا یک ساعت پیش از شروع؛ کف مهلت ۳۰ دقیقه از اکنون. این فرمول در کد اجرا می‌شود، حتی اگر شروع نزدیک باشد. |
| پانسیون شبانه | `nightCount = (toDate.Date - fromDate.Date).Days`؛ ورود ۲۰ و خروج ۲۱ یعنی یک شب. اقامت یک‌روزه/منفی رد می‌شود؛ روز خروج می‌تواند روز ورود رزرو بعدی باشد. |
| مهد روزانه | ساعت ناقص رو به بالا گرد می‌شود: ۰۸:۰۰ تا ۰۹:۳۰ برابر ۲ ساعت. ساعت باید معتبر و پایان پس از شروع باشد؛ `openHour/closeHour` قابل‌خواندن محدودهٔ مجازند. |
| نوع مرکز | `isDaycare=true` فقط تاریخ مهد + `startTime/endTime` را می‌پذیرد؛ `false` فقط `fromDate/toDate` را. مقدار `null` هر دو روش رزرو را با `PansionActivityTypeNotSpecified` رد می‌کند. ارسال هر دو مجموعهٔ زمان نیز معتبر نیست. |
| پت و بیماری | اگر `PansionPets` غیرخالی باشد، گونهٔ پت باید در آن باشد. `specificDisease` غیرتهی فقط به پانسیون عفونی و پت بدون آن فقط به پانسیون غیرعفونی می‌تواند رزرو دهد. |
| قیمت | `pansionPrice × nights` یا `schoolPrice × ceil(hours)` در سرور. در شروع پرداخت، رزرو پرداخت‌نشده با تعرفهٔ فعلی مرکز دوباره قیمت‌گذاری می‌شود؛ `paymentPrice=max(0,price-rebatePrice)`. |
| ساخت رزرو مشتری | `bookerId` از JWT است. قیمت، کد رزرو، تاریخ ثبت، وضعیت، تخفیف نهایی و سهم سایت/مرکز را از بدنهٔ ارسالی ملاک نگیرید. |
| جستجوی مشتری/نماینده | در EndUser `bookerId` و در فهرست نماینده `companionId` از JWT روی فیلتر بازنویسی می‌شود. |
| سایت معرفی | برای `api/Site/Pansion` سه شرط `active`، `approve` و `showToSite` با هم لازم‌اند؛ مسیر قدیمی `/api/Pansion/{id}` چنین کنترل نمایشی ندارد. |
| سفر پت‌رسان | `petResanTrip` و `totalPrice` در جزئیات رزرو قابل نمایش‌اند، ولی قیمت و پرداخت سفر از رزرو پانسیون جداست. |
| DTO پرداخت مشترک | `PaymentStartDto` برای چند نوع سفارش مشترک است؛ در پرداخت این مسیر فقط `pansionReserveId` مربوط به رزرو پانسیون است. شناسه‌های Shop/Groomer/Clinic/Trip در فرهنگ DTO صرفاً به خاطر اشتراک نوع دیده می‌شوند. |

## نمونهٔ درخواست‌های کلیدی

```http
GET /api/Site/Pansion?isDaycare=false&pageIndex=1&pageSize=12
GET /api/Site/Pansion/31
POST /api/EndUser/PansionReserve
PUT /api/EndUser/PansionReserveSetRebate
POST /api/EndUser/PansionReservePayment
```

```json
{"pansionId":31,"userPetId":208,"fromDate":"2026-11-01T00:00:00","toDate":"2026-11-03T00:00:00"}
```

شناسه‌ها و تاریخ نمونه فقط شکل داده را نشان می‌دهند؛ شناسهٔ واقعی را از همان محیط بگیرید. برای مهد، فیلدهای `schoolCreateDate`، `startTime` و `endTime` به جای `fromDate/toDate` استفاده می‌شوند.

## عمومی و سایت — 6 عملیات

| روش | مسیر | ورودی | پاسخ موفق | شرح |
|---|---|---|---|---|
| `GET` | `/api/Pansion` | `dto` (query) | `PansionSearchDto` | جستجو |
| `GET` | `/api/Pansion/{id}` | `id` (مسیر) | `BaseResultDto<PansionVDto>` | اطلاعات آیتم شناسه پانسیون |
| `GET` | `/api/PansionReserve/{PansionId}` | `PansionId` (مسیر) | `BaseResultDto<int>` | تعداد رزروهای آیتم شناسه نماینده تعداد رزروها |
| `GET` | `/api/PansionSiteMap` | — | `BaseResultDto<List<PansionSiteMapDto>>` | سایت مپ پانسیون‌ها |
| `GET` | `/api/Site/Pansion` | `dto` (query) | `PansionSearchDto` | فهرست/مشاهده Pansion Site |
| `GET` | `/api/Site/Pansion/{id}` | `id` (مسیر) | `BaseResultDto<PansionVDto>` | فهرست/مشاهده Pansion Site |

### `GET /api/Pansion`

**کار:** جستجو  
**دسترسی:** بدون ورود  
**ورودی:** `dto: PansionInputDto` در query  
**شکل پاسخ 200:** `PansionSearchDto`

**قاعدهٔ اختصاصی:** کنترلر `approve=true` و `available=true` را روی فیلتر بازنویسی می‌کند. برای دادهٔ صفحهٔ معرفی سایت، فیلتر `showToSite=true` در مسیر `/api/Site/Pansion` نیز اعمال می‌شود.

### `GET /api/Pansion/{id}`

**کار:** اطلاعات آیتم شناسه پانسیون  
**دسترسی:** بدون ورود  
**ورودی:** `id: long` در مسیر  
**شکل پاسخ 200:** `BaseResultDto<PansionVDto>`

**قاعدهٔ اختصاصی:** این جزئیات فقط حذف نرم را فیلتر می‌کند و در خود کنترلر `active/approve/showToSite` بررسی نمی‌شود؛ برای نمایش عمومی سایت از `/api/Site/Pansion/{id}` استفاده کنید. نوع runtime `BaseResultDto<PansionVDto>` است؛ annotation قدیمی `PansionDto` نوشته است.

### `GET /api/PansionReserve/{PansionId}`

**کار:** تعداد رزروهای آیتم شناسه نماینده تعداد رزروها  
**دسترسی:** بدون ورود  
**ورودی:** `PansionId: long` در مسیر  
**شکل پاسخ 200:** `BaseResultDto<int>`

**قاعدهٔ اختصاصی:** تعداد رزروها در `data` یک `BaseResultDto<int>` است؛ annotation کنترلر به‌اشتباه `int` نوشته است.

### `GET /api/PansionSiteMap`

**کار:** سایت مپ پانسیون‌ها  
**دسترسی:** بدون ورود  
**ورودی:** ورودی ندارد  
**شکل پاسخ 200:** `BaseResultDto<List<PansionSiteMapDto>>`

**قاعدهٔ اختصاصی:** runtime از `GetSiteMap` یک `BaseResultDto<List<PansionSiteMapDto>>` می‌دهد، هرچند annotation فقط یک `PansionSiteMapDto` ذکر کرده است.

### `GET /api/Site/Pansion`

**کار:** فهرست/مشاهده Pansion Site  
**دسترسی:** بدون ورود  
**ورودی:** `dto: PansionInputDto` در query  
**شکل پاسخ 200:** `PansionSearchDto`

**قاعدهٔ اختصاصی:** جستجوی سایت فقط پانسیون فعال، تأییدشده و منتشرشده را برمی‌گرداند؛ `available/approve/showToSite` ارسالی کلاینت بازنویسی می‌شود.

### `GET /api/Site/Pansion/{id}`

**کار:** فهرست/مشاهده Pansion Site  
**دسترسی:** بدون ورود  
**ورودی:** `id: long` در مسیر  
**شکل پاسخ 200:** `BaseResultDto<PansionVDto>`

**قاعدهٔ اختصاصی:** اگر پانسیون وجود نداشته باشد یا `active`، `approve` یا `showToSite` false باشد، HTTP 200 با `isSuccess=false` و `NothingFound` برمی‌گردد.

## مشتری — 11 عملیات

| روش | مسیر | ورودی | پاسخ موفق | شرح |
|---|---|---|---|---|
| `GET` | `/api/EndUser/PansionComment` | `dto` (query) | `PansionCommentSearchDto` | جستجو |
| `POST` | `/api/EndUser/PansionComment` | `PansionComment` (بدنه JSON) | `BaseResultDto<PansionCommentDto>` | آیتم جدید |
| `GET` | `/api/EndUser/PansionReserve` | `dto` (query) | `PansionReserveSearchDto` | جستجو |
| `GET` | `/api/EndUser/PansionReserve/{id}` | `id` (مسیر) | `BaseResultDto<PansionReserveVDto>` | اطلاعات آیتم شناسه رزرو نماینده |
| `POST` | `/api/EndUser/PansionReserve` | `dto` (بدنه JSON) | `BaseResultDto<PansionReserveDto>` | آیتم جدید |
| `POST` | `/api/EndUser/PansionReservePayment` | `payment` (بدنه JSON) | `BaseResultDto<PaymentStartDto>` | آیتم جدید پرداختی |
| `PUT` | `/api/EndUser/PansionReserveRemoveRebate` | `id` (query) | `BaseResultDto` | ویرایش آیتم |
| `PUT` | `/api/EndUser/PansionReserveSetRebate` | `dto` (بدنه JSON) | `BaseResultDto` | ویرایش آیتم |
| `PUT` | `/api/EndUser/PansionReserveSetWallet` | `dto` (بدنه JSON) | `BaseResultDto` | ویرایش آیتم |
| `POST` | `/api/EndUser/TripPansionReservation` | `dto` (بدنه JSON) | `BaseResultDto<TripDto>` | آیتم جدید |
| `GET` | `/api/EndUser/TripPansionReservation/{pansionReserveId}` | `pansionReserveId` (مسیر) | `BaseResultDto<TripVDto>` | سفرِ پت‌رسانِ متصل به یک رزرو پانسیون مشخص — برای نمایش وضعیت («فلان راننده تایید کرد» / «هنوز کسی قبول نکرده») |

### `GET /api/EndUser/PansionComment`

**کار:** جستجو  
**دسترسی:** بدون ورود  
**ورودی:** `dto: PansionCommentInputDto` در query  
**شکل پاسخ 200:** `PansionCommentSearchDto`

**قاعدهٔ اختصاصی:** این GET با `[AllowAnonymous]` آزاد است. کنترلر `available=true` و `allStatus=false` را اجباری می‌کند و اگر هویت کاربر موجود باشد، `userId` فیلتر را به کاربر جاری تغییر می‌دهد.

### `POST /api/EndUser/PansionComment`

**کار:** آیتم جدید  
**دسترسی:** مشتری با JWT  
**ورودی:** `PansionComment: PansionCommentDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto<PansionCommentDto>`

**قاعدهٔ اختصاصی:** `userId` از JWT بازنویسی می‌شود؛ شرایط ثبت دیدگاه و ارتباط با رزرو را باید از پاسخ موفق/خطای سرویس و وضعیت رزرو کنترل کرد.

### `GET /api/EndUser/PansionReserve`

**کار:** جستجو  
**دسترسی:** مشتری با JWT  
**ورودی:** `dto: PansionReserveInputDto` در query  
**شکل پاسخ 200:** `PansionReserveSearchDto`

**قاعدهٔ اختصاصی:** `bookerId` ارسالی در query با شناسهٔ JWT بازنویسی می‌شود؛ فقط رزروهای خود کاربر.

### `GET /api/EndUser/PansionReserve/{id}`

**کار:** اطلاعات آیتم شناسه رزرو نماینده  
**دسترسی:** مشتری با JWT  
**ورودی:** `id: long` در مسیر  
**شکل پاسخ 200:** `BaseResultDto<PansionReserveVDto>`

**قاعدهٔ اختصاصی:** سرویس `FindAsyncVDto` با `bookerId` کاربر جاری صدا زده می‌شود؛ شناسهٔ متعلق به دیگری نتیجهٔ موفق ندارد.

### `POST /api/EndUser/PansionReserve`

**کار:** آیتم جدید  
**دسترسی:** مشتری با JWT  
**ورودی:** `dto: PansionReserveDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto<PansionReserveDto>`

**قاعدهٔ اختصاصی:** `bookerId` از JWT جای‌گذاری می‌شود؛ `price`، `paymentPrice`، روز/ساعت قابل محاسبه و وضعیت را از بدنهٔ ارسالی مبنا نگیرید. `isDaycare=null` در مرکز خطای `PansionActivityTypeNotSpecified` می‌دهد. پت بیمار فقط در مرکز `isInfectious=true` و پت سالم فقط در مرکز غیرعفونی پذیرفته می‌شود؛ `PansionPets` اگر غیرخالی باشد گونه را محدود می‌کند. فیلتر `IdempotentRequest` روی عملیات نصب است.

### `POST /api/EndUser/PansionReservePayment`

**کار:** آیتم جدید پرداختی  
**دسترسی:** مشتری با JWT  
**ورودی:** `payment: PaymentStartDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto<PaymentStartDto>`

**قاعدهٔ اختصاصی:** `pansionReserveId` و در صورت پرداخت درگاه `merchantId` را بفرستید. `userId` و `user` در کنترلر از JWT بازنویسی می‌شوند؛ مبلغ و کد callback باید از سرور بیاید. موفقیت بازگشت مرورگر جایگزین callback معتبر نیست.

### `PUT /api/EndUser/PansionReserveRemoveRebate`

**کار:** ویرایش آیتم  
**دسترسی:** مشتری با JWT  
**ورودی:** `id: long` در query  
**شکل پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** `id` یک پارامتر ساده در query است، نه بدنهٔ JSON. فقط رزرو پرداخت‌نشدهٔ متعلق به کاربر.

### `PUT /api/EndUser/PansionReserveSetRebate`

**کار:** ویرایش آیتم  
**دسترسی:** مشتری با JWT  
**ورودی:** `dto: PansionReserveRebateCodeDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** `id` و `rebateCode` ارسال می‌شود؛ فقط رزرو پرداخت‌نشدهٔ متعلق به کاربر. محاسبهٔ تخفیف بر پایهٔ قیمت سرور است.

### `PUT /api/EndUser/PansionReserveSetWallet`

**کار:** ویرایش آیتم  
**دسترسی:** مشتری با JWT  
**ورودی:** `dto: PansionReserveWalletDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** `id` رزرو و `fromWallet` ارسال می‌شود؛ سرویس مالکیت، وضعیت پرداخت و قفل تلاش مالی را بررسی می‌کند.

### `POST /api/EndUser/TripPansionReservation`

**کار:** آیتم جدید  
**دسترسی:** مشتری با JWT  
**ورودی:** `dto: TripPansionReservationCreateDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto<TripDto>`

**قاعدهٔ اختصاصی:** سفر پت‌رسان به رزرو موجود وصل می‌شود؛ پرداخت سفر مستقل از پرداخت پانسیون است. `scheduledLeadMinutes` فقط ۶۰ یا ۱۲۰ و `dropOffTime` برای پانسیون شبانه با قالب `HH:mm` لازم است.

### `GET /api/EndUser/TripPansionReservation/{pansionReserveId}`

**کار:** سفرِ پت‌رسانِ متصل به یک رزرو پانسیون مشخص — برای نمایش وضعیت («فلان راننده تایید کرد» / «هنوز کسی قبول نکرده»)  
**دسترسی:** مشتری با JWT  
**ورودی:** `pansionReserveId: long` در مسیر  
**شکل پاسخ 200:** `BaseResultDto<TripVDto>`

**قاعدهٔ اختصاصی:** سفر متصل به رزرو خود مشتری را برای نمایش راننده و وضعیت بخوانید؛ راننده هنگام ساخت سفر انتخاب نمی‌شود.

## مالک/نمایندهٔ مرکز — 22 عملیات

| روش | مسیر | ورودی | پاسخ موفق | شرح |
|---|---|---|---|---|
| `PUT` | `/api/Companion/PansionActive` | `dto` (بدنه JSON) | `BaseResultDto` | فعال سازی آیتم |
| `GET` | `/api/Companion/Pansion` | `dto` (query) | `PansionSearchDto` | جستجو |
| `GET` | `/api/Companion/Pansion/{id}` | `id` (مسیر) | `BaseResultDto<PansionVDto>` | اطلاعات آیتم شناسه پانسیون |
| `POST` | `/api/Companion/Pansion` | `dto` (بدنه JSON) | `BaseResultDto<PansionDto>` | آیتم جدید |
| `PUT` | `/api/Companion/Pansion` | `dto` (بدنه JSON) | `BaseResultDto` | ویرایش آیتم |
| `DELETE` | `/api/Companion/Pansion` | `id` (query) | `BaseResultDto` | حذف نرم پانسیون/مهد پت من (فقط مالک کلینیک). رزروها و گزارش مالی می‌مانند؛ با رزرو باز پرداخت‌شده حذف نمی‌شود. بازگردانی فقط توسط ادمین. شناسه پانسیون |
| `GET` | `/api/Companion/PansionPet` | `dto` (query) | `PansionPetSearchDto` | جستجو |
| `GET` | `/api/Companion/PansionPet/{id}` | `id` (مسیر) | `BaseResultDto<PansionPetDto>` | اطلاعات آیتم شناسه خدمات |
| `POST` | `/api/Companion/PansionPet` | `dto` (بدنه JSON) | `BaseResultDto<PansionPetDto>` | آیتم جدید |
| `PUT` | `/api/Companion/PansionPet` | `dto` (بدنه JSON) | `BaseResultDto` | ویرایش آیتم |
| `DELETE` | `/api/Companion/PansionPet` | `id` (query) | `BaseResultDto` | حذف آیتم |
| `GET` | `/api/Companion/PansionPicture/{id}` | `id` (مسیر) | `BaseResultDto<PansionPictureVDto>` | اطلاعات آیتم |
| `GET` | `/api/Companion/PansionPicture` | `dto` (query) | `PansionPictureSearchDto` | جستجو |
| `POST` | `/api/Companion/PansionPicture` | `PansionPictureDto` (بدنه JSON) | `BaseResultDto<PansionPictureDto>` | آیتم جدید |
| `PUT` | `/api/Companion/PansionPicture` | `PansionPictureDto` (بدنه JSON) | `BaseResultDto<PansionPictureDto>` | ویرایش آیتم |
| `DELETE` | `/api/Companion/PansionPicture` | `id` (query) | `BaseResultDto<PansionPictureDto>` | حذف آیتم |
| `PUT` | `/api/Companion/PansionReserveApprove` | `dto` (بدنه JSON) | `BaseResultDto` | تأیید رزرو (فقط مرکزِ مالک) |
| `PUT` | `/api/Companion/PansionReserveCancel` | `dto` (بدنه JSON) | `BaseResultDto` | ویرایش آیتم |
| `PUT` | `/api/Companion/PansionReserveChangeStatus` | `dto` (بدنه JSON) | `BaseResultDto` | ویرایش آیتم |
| `GET` | `/api/Comapnion/PansionReserve` | `dto` (query) | `PansionReserveSearchDto` | جستجو |
| `GET` | `/api/Comapnion/PansionReserve/{id}` | `id` (مسیر) | `BaseResultDto<PansionReserveVDto>` | اطلاعات آیتم شناسه نماینده |
| `PUT` | `/api/Companion/PansionReserveReject` | `dto` (بدنه JSON) | `BaseResultDto` | رد رزرو با دلیل (الزامی؛ برای کاربر ارسال می‌شود) |

### `PUT /api/Companion/PansionActive`

**کار:** فعال سازی آیتم  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `dto: PansionActiveDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** هشدار دسترسی: `CurrentUser.CompanionId` nullable به سرویس می‌رود؛ در حالت تهی، `UpdatePansionActiveDto` شرط مالکیت را حذف می‌کند و یک کاربر JWT مجاز به ورود به action می‌تواند `active` هر مرکز را تغییر دهد. تا اصلاح بک‌اند، برای هویت بدون CompanionId از این مسیر استفاده نشود.

### `GET /api/Companion/Pansion`

**کار:** جستجو  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `dto: PansionInputDto` در query  
**شکل پاسخ 200:** `PansionSearchDto`

**قاعدهٔ اختصاصی:** `companionId` query با مقدار nullable هویت جاری بازنویسی می‌شود. اگر هویت `CompanionId` نداشته باشد، سرویس جستجو شرط مالکیت نمی‌گذارد؛ این مسیر برای چنین توکنی امن نیست.

### `GET /api/Companion/Pansion/{id}`

**کار:** اطلاعات آیتم شناسه پانسیون  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `id: long` در مسیر  
**شکل پاسخ 200:** `BaseResultDto<PansionVDto>`

### `POST /api/Companion/Pansion`

**کار:** آیتم جدید  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `dto: PansionDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto<PansionDto>`

### `PUT /api/Companion/Pansion`

**کار:** ویرایش آیتم  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `dto: PansionDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** ویرایش نماینده از `ResubmitAsyncDto` می‌گذرد و مرکز را دوباره در مسیر تأیید قرار می‌دهد؛ `companionId` ارسالی بازنویسی می‌شود.

### `DELETE /api/Companion/Pansion`

**کار:** حذف نرم پانسیون/مهد پت من (فقط مالک کلینیک). رزروها و گزارش مالی می‌مانند؛ با رزرو باز پرداخت‌شده حذف نمی‌شود. بازگردانی فقط توسط ادمین. شناسه پانسیون  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `id: long` در query  
**شکل پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** حذف نرم فقط برای مالک؛ رزرو باز پرداخت‌شده مانع حذف است. بازگردانی فقط در Admin انجام می‌شود.

### `GET /api/Companion/PansionPet`

**کار:** جستجو  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `dto: PansionPetInputDto` در query  
**شکل پاسخ 200:** `PansionPetSearchDto`

**قاعدهٔ اختصاصی:** فیلتر `companionId` از JWT nullable است؛ مقدار تهی جستجو را از نظر مالکیت بی‌فیلتر می‌گذارد.

### `GET /api/Companion/PansionPet/{id}`

**کار:** اطلاعات آیتم شناسه خدمات  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `id: long` در مسیر  
**شکل پاسخ 200:** `BaseResultDto<PansionPetDto>`

**قاعدهٔ اختصاصی:** کنترلر پیش از `FindAsyncDto(id)` مالکیت مرکز را بررسی نمی‌کند. جزئیات گونهٔ مرکز دیگر در این مسیر می‌تواند خوانده شود.

### `POST /api/Companion/PansionPet`

**کار:** آیتم جدید  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `dto: PansionPetDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto<PansionPetDto>`

### `PUT /api/Companion/PansionPet`

**کار:** ویرایش آیتم  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `dto: PansionPetDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

### `DELETE /api/Companion/PansionPet`

**کار:** حذف آیتم  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `id: long` در query  
**شکل پاسخ 200:** `BaseResultDto`

### `GET /api/Companion/PansionPicture/{id}`

**کار:** اطلاعات آیتم  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `id: long` در مسیر  
**شکل پاسخ 200:** `BaseResultDto<PansionPictureVDto>`

**قاعدهٔ اختصاصی:** کنترلر پیش از `FindAsyncVDto(id)` مالکیت مرکز را بررسی نمی‌کند. جزئیات تصویر مرکز دیگر در این مسیر می‌تواند خوانده شود.

### `GET /api/Companion/PansionPicture`

**کار:** جستجو  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `dto: PansionPictureInputDto` در query  
**شکل پاسخ 200:** `PansionPictureSearchDto`

**قاعدهٔ اختصاصی:** فیلتر `companionId` از JWT nullable است؛ مقدار تهی جستجو را از نظر مالکیت بی‌فیلتر می‌گذارد.

### `POST /api/Companion/PansionPicture`

**کار:** آیتم جدید  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `PansionPictureDto: PansionPictureDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto<PansionPictureDto>`

### `PUT /api/Companion/PansionPicture`

**کار:** ویرایش آیتم  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `PansionPictureDto: PansionPictureDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto<PansionPictureDto>`

### `DELETE /api/Companion/PansionPicture`

**کار:** حذف آیتم  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `id: long` در query  
**شکل پاسخ 200:** `BaseResultDto<PansionPictureDto>`

### `PUT /api/Companion/PansionReserveApprove`

**کار:** تأیید رزرو (فقط مرکزِ مالک)  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `dto: PansionReserveDecisionDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** فقط نمایندهٔ مالک رزروِ پرداخت‌شده و منتظر پاسخ؛ نبود `companionId` در JWT به HTTP 403 می‌رسد.

### `PUT /api/Companion/PansionReserveCancel`

**کار:** ویرایش آیتم  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `dto: PansionReserveCancelDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** اگرچه route موجود است، `UpdatePansionReserveCancelDto` قبل از لغو مجوز Admin را می‌خواهد؛ نمایندهٔ معمولی `AccessDenied` می‌گیرد. این عملیات را به‌عنوان لغو قابل استفادهٔ نماینده طراحی نکنید.

### `PUT /api/Companion/PansionReserveChangeStatus`

**کار:** ویرایش آیتم  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `dto: PansionReserveStatusDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** `companionId` nullable از JWT به سرویس می‌رود؛ اگر تهی باشد شرط مالکیت اجرا نمی‌شود و تغییر وضعیت رزرو دیگران ممکن است. تکمیل (`statusId=48`) فقط پس از تصمیم Approved یا NotRequired مجاز است، ولی این جایگزین مالکیت نیست.

### `GET /api/Comapnion/PansionReserve`

**کار:** جستجو  
**دسترسی:** JWT معتبر؛ policy عضویت Companion به علت غلط‌املایی Area اعمال نمی‌شود و وقتی CompanionId تهی است مالکیت هم بررسی نمی‌شود  
**ورودی:** `dto: PansionReserveInputDto` در query  
**شکل پاسخ 200:** `PansionReserveSearchDto`

**قاعدهٔ اختصاصی:** در کد `[Area("Comapnion")]` غلط‌املایی دارد؛ route واقعی همین `/api/Comapnion/...` است. چون convention مرکزی فقط `Companion` را می‌شناسد، policy `AreaMember` به این کنترلر وصل نمی‌شود و فقط `[Authorize]` می‌ماند. اگر `CurrentUser.CompanionId` تهی باشد، فیلتر مالکیت به سرویس نمی‌رسد و جستجو ممکن است همهٔ رزروها را برگرداند.

### `GET /api/Comapnion/PansionReserve/{id}`

**کار:** اطلاعات آیتم شناسه نماینده  
**دسترسی:** JWT معتبر؛ policy عضویت Companion به علت غلط‌املایی Area اعمال نمی‌شود و وقتی CompanionId تهی است مالکیت هم بررسی نمی‌شود  
**ورودی:** `id: long` در مسیر  
**شکل پاسخ 200:** `BaseResultDto<PansionReserveVDto>`

**قاعدهٔ اختصاصی:** همان غلط‌املایی، نبود policy عضویت و شکاف مالکیت. اگر `CurrentUser.CompanionId` تهی باشد، `FindAsyncVDto` شرط مالکیت اضافه نمی‌کند و رزرو شناسهٔ دلخواه ممکن است دیده شود. کلاینت مشتری باید `/api/EndUser/PansionReserve/{id}` را استفاده کند؛ رفع این نقص در بک‌اند ضروری است.

### `PUT /api/Companion/PansionReserveReject`

**کار:** رد رزرو با دلیل (الزامی؛ برای کاربر ارسال می‌شود)  
**دسترسی:** مالک/نمایندهٔ مرکز با JWT  
**ورودی:** `dto: PansionReserveDecisionDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** `reason` برای رد لازم است؛ رد موفق رزرو را لغو و مبلغ پرداختی را به کیف پول کاربر مسترد می‌کند.

## مدیریت — 33 عملیات

| روش | مسیر | ورودی | پاسخ موفق | شرح |
|---|---|---|---|---|
| `PUT` | `/api/Admin/FinancePansion` | `dto` (بدنه JSON) | `BaseResultDto` | ویرایش آیتم |
| `PUT` | `/api/Admin/PansionActive` | `dto` (بدنه JSON) | `BaseResultDto` | فعال سازی آیتم |
| `PUT` | `/api/Admin/PansionApprove` | `dto` (بدنه JSON) | `BaseResultDto` | احراز هویت آیتم |
| `GET` | `/api/Admin/PansionComment/{id}` | `id` (مسیر) | `BaseResultDto<PansionCommentDto>` | اطلاعات آیتم |
| `GET` | `/api/Admin/PansionComment` | `dto` (query) | `PansionCommentSearchDto` | جستجو |
| `PUT` | `/api/Admin/PansionComment` | `PansionCommentDto` (بدنه JSON) | `BaseResultDto<PansionCommentDto>` | ویرایش آیتم |
| `GET` | `/api/Admin/Pansion` | `dto` (query), `deleted` (query) | `PansionSearchDto` | جستجو |
| `GET` | `/api/Admin/Pansion/{id}` | `id` (مسیر) | `BaseResultDto<PansionVDto>` | اطلاعات آیتم شناسه پانسیون |
| `POST` | `/api/Admin/Pansion` | `dto` (بدنه JSON) | `BaseResultDto<PansionDto>` | آیتم جدید |
| `PUT` | `/api/Admin/Pansion` | `dto` (بدنه JSON) | `BaseResultDto` | ویرایش آیتم |
| `DELETE` | `/api/Admin/Pansion` | `id` (query) | `BaseResultDto` | حذف نرم پانسیون توسط ادمین (رزروها و گزارش مالی می‌مانند؛ با رزرو باز پرداخت‌شده حذف نمی‌شود) |
| `PUT` | `/api/Admin/Pansion/Restore` | `id` (query) | `BaseResultDto` | بازگردانی پانسیون حذف‌شده (غیرفعال و منتشرنشده برمی‌گردد) |
| `GET` | `/api/Admin/PansionPet` | `dto` (query) | `PansionPetSearchDto` | جستجو |
| `GET` | `/api/Admin/PansionPet/{id}` | `id` (مسیر) | `BaseResultDto<PansionPetDto>` | اطلاعات آیتم شناسه خدمات |
| `POST` | `/api/Admin/PansionPet` | `dto` (بدنه JSON) | `BaseResultDto<PansionPetDto>` | آیتم جدید |
| `DELETE` | `/api/Admin/PansionPet` | `id` (query) | `BaseResultDto` | حذف آیتم |
| `GET` | `/api/Admin/PansionPicture/{id}` | `id` (مسیر) | `BaseResultDto<PansionPictureVDto>` | اطلاعات آیتم |
| `GET` | `/api/Admin/PansionPicture` | `dto` (query) | `PansionPictureSearchDto` | جستجو |
| `POST` | `/api/Admin/PansionPicture` | `PansionPictureDto` (بدنه JSON) | `BaseResultDto<PansionPictureDto>` | آیتم جدید |
| `DELETE` | `/api/Admin/PansionPicture` | `id` (query) | `BaseResultDto<PansionPictureDto>` | حذف آیتم |
| `PUT` | `/api/Admin/PansionReserveApprove` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر تأیید رزرو |
| `PUT` | `/api/Admin/PansionReserveCancel` | `dto` (بدنه JSON) | `BaseResultDto` | کنسل کردن آیتم |
| `PUT` | `/api/Admin/PansionReserveChangeStatus` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر وصعیت آیتم |
| `GET` | `/api/Admin/PansionReserve` | `dto` (query) | `PansionReserveSearchDto` | جستجو |
| `GET` | `/api/Admin/PansionReserve/{id}` | `id` (مسیر) | `BaseResultDto<PansionReserveVDto>` | اطلاعات آیتم شناسه نماینده |
| `POST` | `/api/Admin/PansionReserve` | `dto` (بدنه JSON) | `BaseResultDto<PansionReserveDto>` | آیتم جدید |
| `PUT` | `/api/Admin/PansionReserve` | `dto` (بدنه JSON) | `BaseResultDto` | ویرایش آیتم |
| `PUT` | `/api/Admin/PansionReserveReject` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر رد رزرو |
| `GET` | `/api/Admin/SitePansion` | `dto` (query) | `PansionSearchDto` | فهرست/مشاهده صفحهٔ عمومی پانسیون |
| `GET` | `/api/Admin/SitePansion/{id}` | `id` (مسیر) | `BaseResultDto<PansionVDto>` | جزئیات صفحهٔ عمومی پانسیون |
| `POST` | `/api/Admin/SitePansion` | `dto` (بدنه JSON) | `BaseResultDto<PansionDto>` | ثبت صفحهٔ عمومی پانسیون |
| `PUT` | `/api/Admin/SitePansion` | `dto` (بدنه JSON) | `BaseResultDto` | تغییر صفحهٔ عمومی پانسیون |
| `PATCH` | `/api/Admin/SitePansion/{id}/visibility` | `id` (مسیر), `dto` (بدنه JSON) | `BaseResultDto` | تغییر نمایش در سایت صفحهٔ عمومی پانسیون |

### `PUT /api/Admin/FinancePansion`

**کار:** ویرایش آیتم  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: FinancePansionDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

### `PUT /api/Admin/PansionActive`

**کار:** فعال سازی آیتم  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionActiveDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

### `PUT /api/Admin/PansionApprove`

**کار:** احراز هویت آیتم  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionApproveDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

### `GET /api/Admin/PansionComment/{id}`

**کار:** اطلاعات آیتم  
**دسترسی:** مدیریت با JWT  
**ورودی:** `id: long` در مسیر  
**شکل پاسخ 200:** `BaseResultDto<PansionCommentDto>`

### `GET /api/Admin/PansionComment`

**کار:** جستجو  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionCommentInputDto` در query  
**شکل پاسخ 200:** `PansionCommentSearchDto`

### `PUT /api/Admin/PansionComment`

**کار:** ویرایش آیتم  
**دسترسی:** مدیریت با JWT  
**ورودی:** `PansionCommentDto: PansionCommentDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto<PansionCommentDto>`

### `GET /api/Admin/Pansion`

**کار:** جستجو  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionInputDto` در query؛ `deleted: bool?` در query (پیش‌فرض `null`)  
**شکل پاسخ 200:** `PansionSearchDto`

**قاعدهٔ اختصاصی:** query اختیاری `deleted=true` فقط حذف‌شده‌ها را می‌آورد؛ پیش‌فرض حذف‌شده‌ها نمایش داده نمی‌شوند.

### `GET /api/Admin/Pansion/{id}`

**کار:** اطلاعات آیتم شناسه پانسیون  
**دسترسی:** مدیریت با JWT  
**ورودی:** `id: long` در مسیر  
**شکل پاسخ 200:** `BaseResultDto<PansionVDto>`

**قاعدهٔ اختصاصی:** Admin با `includeDeleted=true` جزئیات حذف‌شده را نیز می‌بیند؛ runtime `PansionVDto` است، نه `PansionDto` annotation.

### `POST /api/Admin/Pansion`

**کار:** آیتم جدید  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto<PansionDto>`

### `PUT /api/Admin/Pansion`

**کار:** ویرایش آیتم  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

### `DELETE /api/Admin/Pansion`

**کار:** حذف نرم پانسیون توسط ادمین (رزروها و گزارش مالی می‌مانند؛ با رزرو باز پرداخت‌شده حذف نمی‌شود)  
**دسترسی:** مدیریت با JWT  
**ورودی:** `id: long` در query  
**شکل پاسخ 200:** `BaseResultDto`

### `PUT /api/Admin/Pansion/Restore`

**کار:** بازگردانی پانسیون حذف‌شده (غیرفعال و منتشرنشده برمی‌گردد)  
**دسترسی:** مدیریت با JWT  
**ورودی:** `id: long` در query  
**شکل پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** `id` در query؛ بازگردانی مرکز را غیرفعال و منتشرنشده برمی‌گرداند. پس از آن باید تأیید و نمایش بررسی شود.

### `GET /api/Admin/PansionPet`

**کار:** جستجو  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionPetInputDto` در query  
**شکل پاسخ 200:** `PansionPetSearchDto`

### `GET /api/Admin/PansionPet/{id}`

**کار:** اطلاعات آیتم شناسه خدمات  
**دسترسی:** مدیریت با JWT  
**ورودی:** `id: long` در مسیر  
**شکل پاسخ 200:** `BaseResultDto<PansionPetDto>`

### `POST /api/Admin/PansionPet`

**کار:** آیتم جدید  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionPetDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto<PansionPetDto>`

### `DELETE /api/Admin/PansionPet`

**کار:** حذف آیتم  
**دسترسی:** مدیریت با JWT  
**ورودی:** `id: long` در query  
**شکل پاسخ 200:** `BaseResultDto`

### `GET /api/Admin/PansionPicture/{id}`

**کار:** اطلاعات آیتم  
**دسترسی:** مدیریت با JWT  
**ورودی:** `id: long` در مسیر  
**شکل پاسخ 200:** `BaseResultDto<PansionPictureVDto>`

### `GET /api/Admin/PansionPicture`

**کار:** جستجو  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionPictureInputDto` در query  
**شکل پاسخ 200:** `PansionPictureSearchDto`

### `POST /api/Admin/PansionPicture`

**کار:** آیتم جدید  
**دسترسی:** مدیریت با JWT  
**ورودی:** `PansionPictureDto: PansionPictureDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto<PansionPictureDto>`

### `DELETE /api/Admin/PansionPicture`

**کار:** حذف آیتم  
**دسترسی:** مدیریت با JWT  
**ورودی:** `id: long` در query  
**شکل پاسخ 200:** `BaseResultDto<PansionPictureDto>`

### `PUT /api/Admin/PansionReserveApprove`

**کار:** تغییر تأیید رزرو  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionReserveDecisionDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

### `PUT /api/Admin/PansionReserveCancel`

**کار:** کنسل کردن آیتم  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionReserveCancelDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** `isCancel=true` و `cancelDetail` لازم است. این مسیر رزرو را لغو می‌کند و سفر متصل/امتیاز را برمی‌گرداند، اما در همین متد استرداد وجه به کیف پول انجام نمی‌دهد؛ آن را معادل Reject ندانید. برای رزرو Paid/Complete، سرویس `RoleId=Admin` اصلی را می‌خواهد و نقش تفویض‌شده ممکن است رد شود.

### `PUT /api/Admin/PansionReserveChangeStatus`

**کار:** تغییر وصعیت آیتم  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionReserveStatusDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** `statusId=48` قبل از تأیید مالک یا برای رزرو لغوشده نباید به‌عنوان مسیر عادی استفاده شود؛ سرویس شرط تصمیم مالک برای تکمیل را اجرا می‌کند.

### `GET /api/Admin/PansionReserve`

**کار:** جستجو  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionReserveInputDto` در query  
**شکل پاسخ 200:** `PansionReserveSearchDto`

### `GET /api/Admin/PansionReserve/{id}`

**کار:** اطلاعات آیتم شناسه نماینده  
**دسترسی:** مدیریت با JWT  
**ورودی:** `id: long` در مسیر  
**شکل پاسخ 200:** `BaseResultDto<PansionReserveVDto>`

### `POST /api/Admin/PansionReserve`

**کار:** آیتم جدید  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionReserveDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto<PansionReserveDto>`

### `PUT /api/Admin/PansionReserve`

**کار:** ویرایش آیتم  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionReserveDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

### `PUT /api/Admin/PansionReserveReject`

**کار:** تغییر رد رزرو  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionReserveDecisionDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** ادمین می‌تواند به جای مرکز رد کند؛ `reason` لازم و استرداد به کیف پول انجام می‌شود.

### `GET /api/Admin/SitePansion`

**کار:** فهرست/مشاهده صفحهٔ عمومی پانسیون  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionInputDto` در query  
**شکل پاسخ 200:** `PansionSearchDto`

### `GET /api/Admin/SitePansion/{id}`

**کار:** جزئیات صفحهٔ عمومی پانسیون  
**دسترسی:** مدیریت با JWT  
**ورودی:** `id: long` در مسیر  
**شکل پاسخ 200:** `BaseResultDto<PansionVDto>`

### `POST /api/Admin/SitePansion`

**کار:** ثبت صفحهٔ عمومی پانسیون  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto<PansionDto>`

### `PUT /api/Admin/SitePansion`

**کار:** تغییر صفحهٔ عمومی پانسیون  
**دسترسی:** مدیریت با JWT  
**ورودی:** `dto: PansionDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

### `PATCH /api/Admin/SitePansion/{id}/visibility`

**کار:** تغییر نمایش در سایت صفحهٔ عمومی پانسیون  
**دسترسی:** مدیریت با JWT  
**ورودی:** `id: long` در مسیر؛ `dto: SiteVisibilityDto` در بدنه JSON  
**شکل پاسخ 200:** `BaseResultDto`

**قاعدهٔ اختصاصی:** `id` در مسیر و `{"showToSite":true|false}` در body؛ روی مرکز حذف‌شده تغییر اعمال نمی‌شود.

## پاسخ‌های مستقیم بدون envelope

در مسیرهای زیر JSON مستقیماً شیء جستجوست؛ `data` و `isSuccess` در ریشه وجود ندارد. `pageIndex/pageSize/totalCount/list` را از خود DTO پاسخ بخوانید.

| مسیر | نوع پاسخ |
|---|---|
| `GET /api/Pansion` | `PansionSearchDto` |
| `GET /api/Site/Pansion` | `PansionSearchDto` |
| `GET /api/EndUser/PansionComment` | `PansionCommentSearchDto` |
| `GET /api/EndUser/PansionReserve` | `PansionReserveSearchDto` |
| `GET /api/Companion/Pansion` | `PansionSearchDto` |
| `GET /api/Companion/PansionPet` | `PansionPetSearchDto` |
| `GET /api/Companion/PansionPicture` | `PansionPictureSearchDto` |
| `GET /api/Comapnion/PansionReserve` | `PansionReserveSearchDto` |
| `GET /api/Admin/PansionComment` | `PansionCommentSearchDto` |
| `GET /api/Admin/Pansion` | `PansionSearchDto` |
| `GET /api/Admin/PansionPet` | `PansionPetSearchDto` |
| `GET /api/Admin/PansionPicture` | `PansionPictureSearchDto` |
| `GET /api/Admin/PansionReserve` | `PansionReserveSearchDto` |
| `GET /api/Admin/SitePansion` | `PansionSearchDto` |

## اتصال پنل فعلی به API

این جدول مسیرهای واقعاً موجود در پنل Nuxt را به قرارداد بک‌اند وصل می‌کند. مسیر پنل، route مرورگر است و `/api/...` route سرور؛ یکی را جای دیگری نگذارید.

| صفحهٔ پنل | کار/Endpoint اصلی | نکتهٔ پیاده‌سازی |
|---|---|---|
| `/admin/companion` و `/admin/pansion` | فهرست و عملیات `Admin/Pansion` | فهرست یکپارچهٔ نمایندگان به ساخت/ویرایش/جزئیات پانسیون هدایت می‌کند. |
| `/admin/pansion/create`، `/admin/pansion/edit-[id]`، `/admin/pansion/detail-[id]` | `GET/POST/PUT /api/Admin/Pansion` | جزئیات شامل نوع مرکز، تعرفه، ساعت، بیماری، نشانی، مجوز، گالری و گونه‌هاست. |
| `/admin/pansion-pet`، `/admin/pansion-picture`، `/admin/pansion-comment` | `Admin/PansionPet`، `Admin/PansionPicture`، `Admin/PansionComment` | از query `pansionId` برای زمینهٔ مرکز استفاده می‌شود؛ مجوز را سرور بررسی کند. |
| `/admin/pansionreserve` و `/admin/pansionreserve/detail-[id]` | `Admin/PansionReserve`، `Admin/PansionReserveApprove/Reject/ChangeStatus/Cancel` | صفحهٔ جزئیات مهلت تأیید، دکمهٔ تأیید/رد و فرم وضعیت/لغو دارد؛ رد با refund است ولی Cancel عمومی همان معنا را ندارد. |
| `/admin/finance/companion-[id]` | `PUT /api/Admin/FinancePansion` | درصد کمیسیون روزانه و ساعتی جداگانه تنظیم می‌شود. |
| `/site-admin/pansions` | `Admin/SitePansion` | کارت معرفی و `showToSite` با مدیریت سایت کنترل می‌شود؛ در تنظیمات صفحه حذف غیرفعال است. |

## خطاها و رفتارهای ویژه‌ای که کلاینت باید پوشش دهد

| وضعیت/شرط | رفتار رابط |
|---|---|
| مرکز غیرفعال، تأییدنشده، حذف‌شده یا خارج از سایت | رزرو جدید را آغاز نکنید؛ جزئیات عمومی سایت ممکن است `isSuccess=false` بدهد. |
| تاریخ نامعتبر، اقامت بدون شب، زمان مهد خارج از ساعت یا پت نامجاز | خطای رزرو را کنار همان ورودی نشان دهید و پیش از تلاش دوباره دادهٔ مرکز و پت را تازه‌سازی کنید. |
| `PansionActivityTypeNotSpecified` یا مغایرت نوع/ورودی | مرکز با `isDaycare=null` رزروپذیر نیست؛ برای `true` فقط فرم ساعتی و برای `false` فقط فرم شبانه نشان دهید. |
| `PansionRequiresInfectiousForSickPet` / `InfectiousPansionOnlyAcceptsSickPets` / `PansionPetTypeNotAccepted` | کاربر را به انتخاب مرکز سازگار با وضعیت و گونهٔ پت راهنمایی کنید. |
| مبلغ رزرو پیش از پرداخت تغییر کرد | مبلغ server را دوباره نشان دهید؛ قیمت محلی یا query string را منبع نهایی ندانید. |
| پرداخت ناموفق/نامعلوم | صرف redirect را موفقیت ندانید؛ رزرو و پرداخت را دوباره از API بخوانید. |
| `ownerDecision=Pending` | پیام انتظار و مهلت پاسخ مرکز را نشان دهید؛ تا تصمیم نهایی تحویل قطعی را وعده ندهید. |
| `Rejected` یا `Expired` | لغو و برگشت پول به کیف پول را از وضعیت تازه بخوانید؛ دوباره همان رزرو را پرداخت نکنید. |
| HTTP 401/403، یا `isSuccess=false` / `AccessDenied` | ورود/سطح دسترسی/مالکیت را بررسی کنید و دادهٔ دیگران را نمایش ندهید. |
| `/api/Companion/PansionReserve` نامعتبر | مسیر جستجو/جزئیات واقعی `/api/Comapnion/PansionReserve` است؛ خطای املا در attribute فعلی کد. |

## فرهنگ DTO و فیلدهای JSON

نوع `?` در C# یعنی nullable. برای `string` بدون `?`، صرف نوع نشانهٔ اعتبارسنجی الزامی یکسان نیست؛ شرط‌های اجرایی در بخش‌های بالا آمده‌اند. وجود فیلدی در DTO مشترک خروجی/ورودی مجوز تغییر آن توسط کلاینت نیست. انواع پایه نیز در همین فهرست آمده‌اند.

### `AddressVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `cityId` | `long` | — |
| `firstName` | `string` | — |
| `lastName` | `string` | — |
| `phone` | `string` | — |
| `mobile` | `string` | — |
| `addressValue` | `string` | — |
| `unit` | `string` | — |
| `floor` | `string` | — |
| `location` | `PointDto` | — |
| `postalCode` | `string` | — |
| `nationalCode` | `string` | — |
| `isSelected` | `bool` | — |
| `city` | `CityVDto` | — |

### `AssistanceGroupVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `priority` | `int` | — |
| `active` | `bool` | — |

### `AssistanceVDto`

پایه: `FullName_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `isPersonal` | `bool` | — |
| `assistanceGroupId` | `long?` | — |
| `pictureId` | `long?` | — |
| `active` | `bool` | — |
| `showToSite` | `bool` | — |
| `assistanceGroup` | `AssistanceGroupVDto` | — |
| `picture` | `PictureVDto` | — |

### `BankVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `pictureId` | `long?` | — |
| `label` | `string` | — |
| `picture` | `PictureVDto` | — |

### `BaseInputDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `pageIndex` | `int` | — |
| `pageSize` | `int` | تعداد در صفحه سقف ۱۰۰۰ (بزرگ‌ترین مقدار واقعی مصرف‌کننده‌ها: لیست برند/دسته در پنل). قبلاً بی‌سقف بود و هر endpoint عمومی را می‌شد با pageSize میلیونی یک‌جا خالی کرد. |
| `q` | `string` | متن قابل جستجو |
| `sortBy` | `SortEnum` | مرتب سازی بر اساس |
| `available` | `bool?` | — |

### `BaseSearchDto<Tdto>`

پایه: `BaseInputDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `totalCount` | `int` | — |
| `list` | `List<Tdto>` | — |

### `CargoVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `dateGone` | `DateTime` | — |
| `dateReturn` | `DateTime?` | — |
| `createDate` | `DateTime` | — |
| `fromStateId` | `long` | — |
| `toStateId` | `long` | — |
| `userPetId` | `long` | — |
| `accompany` | `bool` | — |
| `price` | `double` | — |
| `defaultPrice` | `double` | — |
| `notAccompanyPrice` | `double?` | — |
| `returnPrice` | `double?` | — |
| `isPaid` | `bool` | — |
| `statusId` | `long` | — |
| `statusDetail` | `string` | — |
| `discount` | `double` | — |
| `paymentPrice` | `double` | — |
| `rebateId` | `long?` | — |
| `rebatePrice` | `double` | — |
| `fromWallet` | `bool` | — |
| `walletPrice` | `double` | — |
| `userPet` | `UserPetVDto` | — |
| `fromState` | `StateVDto` | — |
| `toState` | `StateVDto` | — |
| `status` | `CodeVDto` | — |
| `rebate` | `RebateVDto` | — |

### `CityVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `stateId` | `long` | — |
| `state` | `StateVDto` | — |

### `CodeDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `label` | `string` | — |
| `value` | `string` | — |
| `codeGroupId` | `long` | — |
| `priority` | `int` | — |
| `active` | `bool` | — |

### `CodeVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `label` | `string` | — |
| `value` | `string` | — |

### `CommentDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `name` | `string` | — |
| `text` | `string` | — |
| `rate` | `int?` | — |
| `statusId` | `long` | — |
| `answer` | `string` | — |
| `userId` | `long?` | — |
| `likeCount` | `int` | — |
| `disLikeCount` | `int` | — |

### `CommentVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `text` | `string` | — |
| `rate` | `int?` | — |
| `statusId` | `long` | — |
| `createDate` | `DateTime` | — |
| `upOrDownThumb` | `bool?` | — |
| `answer` | `string` | — |
| `userId` | `long?` | — |
| `likeCount` | `int` | — |
| `disLikeCount` | `int` | — |
| `user` | `UserMinVDto` | — |
| `status` | `CodeDto` | — |

### `CompanionAssistancePackageOnlineSelectionVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `companionAssistancePackageId` | `long` | — |
| `companionAssistancePackageOnlineId` | `long` | — |
| `price` | `double` | — |
| `active` | `bool` | — |
| `activationValue` | `string` | — |
| `companionAssistancePackage` | `CompanionAssistancePackageVDto` | — |
| `companionAssistancePackageOnline` | `CompanionAssistancePackageOnlineVDto` | — |

### `CompanionAssistancePackageOnlineVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `price` | `double` | — |
| `active` | `bool` | — |
| `isInstant` | `bool` | — |

### `CompanionAssistancePackagePictureVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `companionAssistancePackageId` | `long` | — |
| `pictureId` | `long` | — |
| `picture` | `PictureVDto` | — |

### `CompanionAssistancePackageTypeDto`

قیمت و پیش‌پرداخت یک پکیج در یک «نحوه ارائه» (۳۷ آنلاین، ۳۸ حضوری در مرکز، ۳۹ در محل مشتری).

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `companionAssistanceTypeId` | `long` | — |
| `companionAssistanceTypeName` | `string` | — |
| `price` | `double` | — |
| `prePaymentPrice` | `double` | — |

### `CompanionAssistancePackageVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `price` | `double` | — |
| `prePaymentPrice` | `double` | — |
| `pictureId` | `long?` | — |
| `active` | `bool` | — |
| `activationValue` | `string` | — |
| `companionAssistanceId` | `long` | — |
| `discription` | `string` | — |
| `petSize` | `string` | — |
| `types` | `List<CompanionAssistancePackageTypeDto>` | — |
| `companionAssistance` | `CompanionAssistanceVDto` | — |
| `picture` | `PictureVDto` | — |
| `companionAssistancePackagePictures` | `List<CompanionAssistancePackagePictureVDto>` | — |

### `CompanionAssistanceTimeVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `startTime` | `string` | — |
| `endTime` | `string` | — |
| `weekDayId` | `long` | — |
| `companionAssistanceId` | `long` | — |
| `active` | `bool` | — |
| `weekDay` | `WeekDayVDto` | — |
| `companionAssistance` | `CompanionAssistanceVDto` | — |

### `CompanionAssistanceUserVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `active` | `bool` | — |
| `activationValue` | `string` | — |
| `companionAssistanceId` | `long` | — |
| `userId` | `long` | — |
| `user` | `UserVDto` | — |
| `companionAssistance` | `CompanionAssistanceVDto` | — |

### `CompanionAssistanceVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `companionId` | `long` | — |
| `assistanceId` | `long` | — |
| `companionTypeId` | `long` | — |
| `isSinglePackage` | `bool` | — |
| `active` | `bool` | — |
| `approved` | `bool` | — |
| `activationValue` | `string` | — |
| `commissionPercent` | `decimal` | — |
| `companion` | `CompanionVDto` | — |
| `assistance` | `AssistanceVDto` | — |
| `companionType` | `CodeVDto` | — |
| `codes` | `List<CodeVDto>` | — |

### `CompanionInsurancePackageSaleVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `companionInsurancePackageId` | `long` | — |
| `price` | `double` | — |
| `userPetId` | `long` | — |
| `addressId` | `long` | — |
| `isPaid` | `bool` | — |
| `discount` | `double` | — |
| `paymentPrice` | `double` | — |
| `rebateId` | `long?` | — |
| `rebatePrice` | `double` | — |
| `fromWallet` | `bool` | — |
| `walletPrice` | `double` | — |
| `manualPayDate` | `DateTime?` | — |
| `createDate` | `DateTime` | — |
| `startDate` | `DateTime` | — |
| `endDate` | `DateTime` | — |
| `userPet` | `UserPetVDto` | — |
| `address` | `AddressVDto` | — |
| `rebate` | `RebateVDto` | — |
| `companionInsurancePackage` | `CompanionInsurancePackageVDto` | — |

### `CompanionInsurancePackageVDto`

پایه: `FullName_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `dayCount` | `int` | — |
| `companionId` | `long` | — |
| `price` | `double` | — |
| `petId` | `long` | — |
| `priority` | `int` | — |
| `active` | `bool` | — |
| `activationValue` | `string` | — |
| `companion` | `CompanionMinVDto` | — |
| `pet` | `PetVDto` | — |

### `CompanionMinVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
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

### `CompanionPetVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `companionId` | `long` | — |
| `petId` | `long` | — |
| `companion` | `CompanionMinVDto` | — |
| `pet` | `PetVDto` | — |

### `CompanionReservePackageItemVDto`

یک پکیج داخل رزرو با وضعیتش. Status: 1 در انتظار تأیید، 2 تأییدشده، 3 لغوشده.

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | ۰ برای رزروهای قدیمی که هنوز ردیفی ندارند (مجازی، همیشه «در انتظار»/«تأییدشده») |
| `packageId` | `long` | — |
| `packageName` | `string` | — |
| `petCount` | `int` | — |
| `price` | `double` | — |
| `status` | `int` | — |
| `statusReason` | `string` | — |
| `statusChangedDate` | `DateTime?` | — |
| `statusChangedByAdmin` | `bool` | — |
| `refundAmount` | `double` | مبلغی که با لغو این پکیج به کیف پول کاربر برگشته |
| `refundDate` | `DateTime?` | — |
| `addedAfterPayment` | `bool` | — |
| `createDate` | `DateTime?` | — |

### `CompanionReservePackageItemsVDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `reserveId` | `long` | — |
| `canManage` | `bool` | true = نماینده/ادمین هنوز می‌تواند تأیید/لغو/افزودن کند (رزرو لغو/کامل/نهایی نشده) |
| `items` | `List<CompanionReservePackageItemVDto>` | — |
| `summary` | `CompanionReservePackageSummaryVDto` | — |

### `CompanionReservePackageSummaryVDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `total` | `int` | — |
| `pending` | `int` | — |
| `approved` | `int` | — |
| `cancelled` | `int` | — |
| `refundedTotal` | `double` | جمع مبالغ برگشت‌خورده به کیف پول از لغو پکیج‌ها |

### `CompanionReserveVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `petResanTrip` | `Application.Services.TripSrv.TripSrv.Dto.LinkedPetResanTripVDto` | — |
| `totalPrice` | `double` | — |
| `reserveCode` | `string` | — |
| `bookerId` | `long` | — |
| `batchId` | `long?` | — |
| `petCount` | `int` | — |
| `prePaymentPrice` | `double` | — |
| `operatorFinalPrice` | `double` | — |
| `operatorUnpaid` | `bool` | — |
| `operatorUnpaidAmount` | `double` | — |
| `operatorUnpaidDate` | `System.DateTime?` | — |
| `operatorDebtPaidDate` | `System.DateTime?` | — |
| `operatorDebtPaidByWallet` | `bool` | — |
| `operatorStuffPrice` | `double` | — |
| `operatorWagesPrice` | `double` | — |
| `paymentPrice` | `double` | — |
| `fromWallet` | `bool` | — |
| `walletPrice` | `double` | — |
| `packagePrice` | `double` | — |
| `addressId` | `long?` | — |
| `companionAssistanceId` | `long` | — |
| `companionAssistanceTimeId` | `long` | — |
| `companionTimeId` | `long?` | — |
| `companionAssistanceUserId` | `long?` | — |
| `companionAssistancePackageOnlineSelectionId` | `long?` | — |
| `callStartDate` | `DateTime?` | — |
| `callEndDate` | `DateTime?` | — |
| `isFemale` | `bool?` | — |
| `bookerDetail` | `string` | — |
| `assistanceDetail` | `string` | — |
| `createDate` | `DateTime` | — |
| `doDate` | `DateTime` | — |
| `doneDate` | `DateTime?` | — |
| `isReserved` | `bool` | — |
| `isCancel` | `bool` | — |
| `cancelDetail` | `string` | — |
| `stateId` | `long` | — |
| `companionAssistanceTypeId` | `long` | — |
| `cancelDate` | `DateTime?` | — |
| `operatorStateId` | `long` | — |
| `operatorChangeStateDate` | `DateTime?` | — |
| `operatorDetail` | `string` | — |
| `userResponse` | `bool?` | — |
| `discount` | `double` | — |
| `rebateId` | `long?` | — |
| `rebatePrice` | `double` | — |
| `companionShare` | `double` | — |
| `siteShare` | `double` | — |
| `packageItems` | `Application.Services.CompanionSrvs.CompanionReservePackageItemSrv.Dto.CompanionReservePackageItemsVDto` | — |
| `booker` | `UserMinVDto` | — |
| `companionAssistance` | `CompanionAssistanceVDto` | — |
| `companionAssistanceTime` | `CompanionAssistanceTimeVDto` | — |
| `companionTime` | `CompanionTimeVDto` | — |
| `companionAssistanceUser` | `CompanionAssistanceUserVDto` | — |
| `companionAssistancePackageOnlineSelection` | `CompanionAssistancePackageOnlineSelectionVDto` | — |
| `state` | `CodeVDto` | — |
| `operatorState` | `CodeVDto` | — |
| `companionAssistanceType` | `CodeVDto` | — |
| `address` | `AddressVDto` | — |
| `rebate` | `RebateVDto` | — |
| `userPets` | `List<UserPetVDto>` | — |
| `companionAssistancePackages` | `List<CompanionAssistancePackageVDto>` | — |

### `CompanionTimeVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `startTime` | `string` | — |
| `endTime` | `string` | — |
| `capacity` | `int` | — |
| `weekDayId` | `long` | — |
| `companionId` | `long` | — |
| `active` | `bool` | — |
| `weekDay` | `WeekDayVDto` | — |
| `companion` | `CompanionVDto` | — |
| `remainingCapacity` | `int?` | — |
| `isFull` | `bool` | — |

### `CompanionTypeVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `companionId` | `long` | — |
| `typeId` | `long` | — |
| `companion` | `CompanionMinVDto` | — |
| `type` | `CodeVDto` | — |

### `CompanionVDto`

پایه: `Seo_Full_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `isPersonal` | `bool` | — |
| `ownerId` | `long` | — |
| `providerId` | `long?` | ارائه‌دهنده‌ی صاحب این مکان؛ اشتراک از روی همین مدیریت می‌شود. |
| `referralCode` | `string` | — |
| `goldAccountDate` | `DateTime?` | — |
| `silverAccountDate` | `DateTime?` | — |
| `silverAccountCreateDate` | `DateTime?` | — |
| `pictureId` | `long?` | — |
| `backgroundPictureId` | `long?` | — |
| `iconId` | `long?` | — |
| `active` | `bool` | — |
| `showToSite` | `bool` | — |
| `approved` | `bool` | — |
| `activationValue` | `string` | — |
| `addressValue` | `string` | — |
| `phone` | `string` | — |
| `cityId` | `long` | — |
| `neighborhoodId` | `long?` | — |
| `commentCount` | `int` | — |
| `rateAvg` | `double` | — |
| `rateCount` | `int` | — |
| `reserveCount` | `int` | — |
| `isSilver` | `bool` | — |
| `isGold` | `bool` | — |
| `searchKey` | `string` | — |
| `hasPansion` | `bool` | — |
| `owner` | `UserMinVDto` | — |
| `city` | `CityVDto` | — |
| `neighborhood` | `NeighborhoodVDto` | — |
| `location` | `PointDto` | — |
| `picture` | `PictureVDto` | — |
| `backgroundPicture` | `PictureVDto` | — |
| `icon` | `PictureVDto` | — |
| `companionPets` | `List<CompanionPetVDto>` | — |
| `companionTypes` | `List<CompanionTypeVDto>` | — |
| `companionZones` | `List<CompanionZoneVDto>` | — |
| `pansions` | `List<PansionVDto>` | — |

### `CompanionZoneVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `companionId` | `long` | — |
| `stateId` | `long` | — |
| `cityId` | `long` | — |
| `neighborhoodId` | `long?` | — |
| `state` | `StateVDto` | — |
| `city` | `CityVDto` | — |
| `neighborhood` | `NeighborhoodVDto` | — |

### `CountryVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `enName` | `string` | — |

### `DeliveryDistanceVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `fromD` | `int` | — |
| `toD` | `int` | — |
| `price` | `double` | — |
| `deliveryId` | `long` | — |

### `DeliveryVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `deliveryTypeId` | `long` | — |
| `basePrice` | `double` | — |
| `minPriceForFree` | `double` | — |
| `minCountForFree` | `int` | — |
| `maxDays` | `int` | — |
| `cityId` | `long?` | — |
| `stateId` | `long?` | — |
| `active` | `bool` | — |
| `deleted` | `bool` | — |
| `storeId` | `long?` | — |
| `afterRent` | `bool` | — |
| `shippingProvider` | `Entities.Entities.ShippingField.ShippingProviderEnum` | — |
| `livePricing` | `bool` | — |
| `allowPrepaid` | `bool` | — |
| `allowReceiverPay` | `bool` | — |
| `store` | `StoreMinVDto` | — |
| `city` | `CityVDto` | — |
| `deliveryType` | `CodeVDto` | — |
| `state` | `StateVDto` | — |
| `deliveryDistance` | `List<DeliveryDistanceVDto>` | — |

### `DriverVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `ownerId` | `long` | — |
| `phone` | `string` | — |
| `vehicle` | `string` | — |
| `licensePlateNumber` | `string` | — |
| `ownerDetail` | `string` | — |
| `active` | `bool` | — |
| `rate` | `int` | — |
| `profilePictureId` | `long?` | — |
| `certificatePictureId` | `long?` | — |
| `vehicleCardPictureId` | `long?` | — |
| `cityId` | `long` | — |
| `neighborhoodId` | `long?` | — |
| `statusId` | `long` | — |
| `adminDetail` | `string` | — |
| `approved` | `bool` | — |
| `vehicleTypeId` | `long?` | — |
| `city` | `CityVDto` | — |
| `neighborhood` | `NeighborhoodVDto` | — |
| `profilePicture` | `PictureVDto` | — |
| `certificatePicture` | `PictureVDto` | — |
| `vehicleCardPicture` | `PictureVDto` | — |
| `owner` | `UserMinVDto` | — |
| `status` | `CodeVDto` | — |
| `vehicleType` | `CodeVDto` | — |

### `FileVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `url` | `string` | — |
| `orginalName` | `string` | — |

### `FinancePansionDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `pansionId` | `long` | — |
| `dailyCommissionPercent` | `decimal` | — |
| `hourlyCommissionPercent` | `decimal` | — |

### `FullName_FieldDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `summary` | `string` | — |
| `description` | `string` | — |

### `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |

### `LinkedPetResanTripVDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `tripId` | `long` | — |
| `price` | `double` | — |
| `paymentPrice` | `double` | — |
| `isPaid` | `bool` | — |
| `tripStatusLabel` | `string` | — |
| `driverStatusLabel` | `string` | — |
| `driverId` | `long?` | — |
| `driverName` | `string` | — |

### `MerchantVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `bankId` | `long` | — |
| `active` | `bool` | — |
| `bank` | `BankVDto` | — |

### `Name_FieldDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `name` | `string` | — |

### `NeighborhoodVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `regionNumber` | `int` | — |
| `cityId` | `long` | — |
| `city` | `CityVDto` | — |

### `PansionActiveDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `active` | `bool` | — |

### `PansionApproveDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `approve` | `bool` | — |
| `approvalValue` | `string` | — |

### `PansionCommentDto`

پایه: `CommentDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `pansionId` | `long` | — |
| `pansionReserveId` | `long` | — |

### `PansionCommentInputDto`

پایه: `BaseInputDto, IPansionCommentSearchFields`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `pansionId` | `long?` | — |
| `isReserved` | `bool?` | — |
| `userId` | `long?` | — |
| `allStatus` | `bool?` | — |

### `PansionCommentSearchDto`

پایه: `BaseSearchDto<PansionComment, PansionCommentVDto>, IPansionCommentSearchFields`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `pansionId` | `long?` | — |
| `isReserved` | `bool?` | — |
| `userId` | `long?` | — |
| `allStatus` | `bool?` | — |

### `PansionCommentVDto`

پایه: `CommentVDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `pansionId` | `long` | — |
| `pansionReserveId` | `long?` | — |
| `isReserved` | `bool` | — |
| `pansionName` | `string` | — |

### `PansionDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `isDaycare` | `bool?` | — |
| `companionId` | `long` | — |
| `active` | `bool` | — |
| `showToSite` | `bool` | — |
| `approve` | `bool` | — |
| `approvalValue` | `string` | — |
| `stateId` | `long` | — |
| `cityId` | `long` | — |
| `discription` | `string` | — |
| `addressValue` | `string` | — |
| `location` | `PointDto` | — |
| `commentCount` | `int` | — |
| `rateAvg` | `double` | — |
| `rateCount` | `int` | — |
| `pictureId` | `long?` | — |
| `suggested` | `bool` | — |
| `isInfectious` | `bool` | — |
| `pansionPrice` | `double` | — |
| `schoolPrice` | `double` | — |
| `regulations` | `string` | — |
| `openHour` | `string` | — |
| `closeHour` | `string` | — |

### `PansionInputDto`

پایه: `BaseInputDto, IPansionSearchFields`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `isDaycare` | `bool?` | — |
| `companionId` | `long?` | — |
| `approve` | `bool?` | — |
| `neighborhoodIds` | `List<long>` | — |
| `stateId` | `long?` | — |
| `cityId` | `long?` | — |
| `suggested` | `bool?` | — |
| `showToSite` | `bool?` | — |
| `petId` | `long?` | — |
| `isInfectious` | `bool?` | — |

### `PansionMinVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `isDaycare` | `bool?` | — |
| `companionId` | `long` | — |
| `active` | `bool` | — |
| `approve` | `bool` | — |
| `stateId` | `long` | — |
| `cityId` | `long` | — |
| `discription` | `string` | — |
| `addressValue` | `string` | — |
| `commentCount` | `int` | — |
| `rateAvg` | `double` | — |
| `rateCount` | `int` | — |
| `pictureId` | `long?` | — |
| `suggested` | `bool` | — |
| `pansionPrice` | `double` | — |
| `schoolPrice` | `double` | — |
| `regulations` | `string` | — |
| `openHour` | `string` | — |
| `closeHour` | `string` | — |

### `PansionPetDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `pansionId` | `long` | — |
| `petId` | `long` | — |

### `PansionPetInputDto`

پایه: `BaseInputDto, IPansionPetSearchFields`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `companionId` | `long?` | — |

### `PansionPetSearchDto`

پایه: `BaseSearchDto<PansionPet, PansionPetVDto>, IPansionPetSearchFields`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `companionId` | `long?` | — |

### `PansionPetVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `pansionId` | `long` | — |
| `petId` | `long` | — |
| `pet` | `PetVDto` | — |

### `PansionPictureDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `pansionId` | `long` | — |
| `pictureId` | `long` | — |

### `PansionPictureInputDto`

پایه: `BaseInputDto, IPansionPictureSearchFields`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `pansionId` | `long?` | — |
| `companionId` | `long?` | — |

### `PansionPictureSearchDto`

پایه: `BaseSearchDto<PansionPicture, PansionPictureVDto>, IPansionPictureSearchFields`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `pansionId` | `long?` | — |
| `companionId` | `long?` | — |

### `PansionPictureVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `pansionId` | `long` | — |
| `pictureId` | `long` | — |
| `picture` | `PictureVDto` | — |

### `PansionReserveCancelDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `isCancel` | `bool` | — |
| `cancelDetail` | `string` | — |

### `PansionReserveDecisionDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `id` | `long` | — |
| `reason` | `string` | — |

### `PansionReserveDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `reserveCode` | `string` | — |
| `pansionId` | `long` | — |
| `bookerId` | `long` | — |
| `userPetId` | `long` | — |
| `price` | `double` | — |
| `fromWallet` | `bool` | — |
| `walletPrice` | `double` | — |
| `paymentPrice` | `double` | — |
| `isReserved` | `bool` | — |
| `isCancel` | `bool` | — |
| `cancelDetail` | `string` | — |
| `startTime` | `string` | — |
| `endTime` | `string` | — |
| `schoolCreateDate` | `DateTime?` | — |
| `fromDate` | `DateTime?` | — |
| `toDate` | `DateTime?` | — |
| `createDate` | `DateTime` | — |
| `cancelDate` | `DateTime?` | — |
| `discount` | `double` | — |
| `rebateId` | `long?` | — |
| `rebatePrice` | `double` | — |
| `statusId` | `long` | — |

### `PansionReserveInputDto`

پایه: `BaseInputDto, IPansionReserveSearchFields`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `bookerId` | `long?` | — |
| `userPetId` | `long?` | — |
| `pansionId` | `long?` | — |
| `companionId` | `long?` | — |
| `statusId` | `long?` | — |
| `isDaycare` | `bool?` | — |

### `PansionReserveRebateCodeDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `rebateCode` | `string` | — |

### `PansionReserveSearchDto`

پایه: `BaseSearchDto<PansionReserve, PansionReserveVDto>, IPansionReserveSearchFields`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `bookerId` | `long?` | — |
| `userPetId` | `long?` | — |
| `pansionId` | `long?` | — |
| `companionId` | `long?` | — |
| `statusId` | `long?` | — |
| `isDaycare` | `bool?` | — |

### `PansionReserveStatusDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `statusId` | `long` | — |

### `PansionReserveVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `petResanTrip` | `Application.Services.TripSrv.TripSrv.Dto.LinkedPetResanTripVDto` | — |
| `totalPrice` | `double` | — |
| `reserveCode` | `string` | — |
| `pansionId` | `long` | — |
| `bookerId` | `long` | — |
| `userPetId` | `long` | — |
| `price` | `double` | — |
| `fromWallet` | `bool` | — |
| `walletPrice` | `double` | — |
| `paymentPrice` | `double` | — |
| `isReserved` | `bool` | — |
| `isCancel` | `bool` | — |
| `cancelDetail` | `string` | — |
| `startTime` | `string` | — |
| `endTime` | `string` | — |
| `schoolCreateDate` | `DateTime?` | — |
| `fromDate` | `DateTime?` | — |
| `toDate` | `DateTime?` | — |
| `createDate` | `DateTime` | — |
| `cancelDate` | `DateTime?` | — |
| `discount` | `double` | — |
| `rebateId` | `long?` | — |
| `rebatePrice` | `double` | — |
| `statusId` | `long` | — |
| `hourCount` | `int` | — |
| `dayCount` | `int` | — |
| `ownerDecision` | `int` | — |
| `ownerDecisionDate` | `DateTime?` | — |
| `ownerDecisionReason` | `string` | — |
| `ownerApprovalDeadline` | `DateTime?` | — |
| `companionShare` | `double` | — |
| `siteShare` | `double` | — |
| `pansion` | `PansionMinVDto` | — |
| `userPet` | `UserPetVDto` | — |
| `status` | `CodeVDto` | — |
| `booker` | `UserMinVDto` | — |
| `rebate` | `RebateVDto` | — |

### `PansionReserveWalletDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `fromWallet` | `bool` | — |

### `PansionSearchDto`

پایه: `BaseSearchDto<Pansion, PansionVDto>, IPansionSearchFields`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `isDaycare` | `bool?` | — |
| `companionId` | `long?` | — |
| `approve` | `bool?` | — |
| `neighborhoodIds` | `List<long>` | — |
| `stateId` | `long?` | — |
| `cityId` | `long?` | — |
| `suggested` | `bool?` | — |
| `showToSite` | `bool?` | — |
| `petId` | `long?` | — |

### `PansionSiteMapDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `name` | `string` | — |

### `PansionVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `isDaycare` | `bool?` | — |
| `companionId` | `long` | — |
| `active` | `bool` | — |
| `showToSite` | `bool` | — |
| `approve` | `bool` | — |
| `approvalValue` | `string` | — |
| `stateId` | `long` | — |
| `cityId` | `long` | — |
| `discription` | `string` | — |
| `addressValue` | `string` | — |
| `location` | `PointDto` | — |
| `commentCount` | `int` | — |
| `rateAvg` | `double` | — |
| `rateCount` | `int` | — |
| `pictureId` | `long?` | — |
| `suggested` | `bool` | — |
| `deleted` | `bool` | — |
| `deleteDate` | `System.DateTime?` | — |
| `isInfectious` | `bool` | — |
| `pansionPrice` | `double` | — |
| `schoolPrice` | `double` | — |
| `regulations` | `string` | — |
| `openHour` | `string` | — |
| `closeHour` | `string` | — |
| `dailyCommissionPercent` | `decimal` | — |
| `hourlyCommissionPercent` | `decimal` | — |
| `companion` | `CompanionMinVDto` | — |
| `state` | `StateVDto` | — |
| `city` | `CityVDto` | — |
| `picture` | `PictureVDto` | — |
| `pansionPets` | `List<PansionPetVDto>` | — |
| `pansionComments` | `List<PansionCommentVDto>` | — |
| `pansionPictures` | `List<PansionPictureVDto>` | — |

### `PaymentStartDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `paymentCode` | `string` | — |
| `isOnline` | `bool` | — |
| `productOrderId` | `string` | — |
| `companionReserveId` | `long?` | — |
| `pansionReserveId` | `long?` | — |
| `schoolReserveId` | `long?` | — |
| `groomerReserveId` | `long?` | — |
| `clinicReserveId` | `long?` | — |
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

### `PaymentVDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `id` | `string` | — |
| `paymentCode` | `string` | — |
| `merchantId` | `long?` | — |
| `productOrderId` | `string` | — |
| `companionReserveId` | `long?` | — |
| `pasnionReserveId` | `long?` | — |
| `tripId` | `long?` | — |
| `cargoId` | `long?` | — |
| `companionInsurancePackageSaleId` | `long?` | — |
| `refNumber` | `string` | — |
| `amount` | `double` | — |
| `bonusCode` | `string` | — |
| `datetime` | `System.DateTime` | — |
| `description` | `string` | — |
| `isSuccess` | `bool?` | — |
| `isOnline` | `bool` | — |
| `fileId` | `int?` | — |
| `callBackTypeLabel` | `string` | — |
| `callBackId` | `string` | — |
| `merchant` | `MerchantVDto` | — |
| `file` | `FileVDto` | — |

### `PetVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `label` | `string` | — |
| `slug` | `string` | — |
| `priority` | `int` | — |

### `PictureVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `url` | `string` | — |
| `orginalName` | `string` | — |
| `baseUrl` | `string` | — |
| `guidName` | `string` | — |
| `extension` | `string` | — |

### `PointDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `x` | `double` | — |
| `y` | `double` | — |
| `distanceMeter` | `double` | — |

### `ProductItemFullVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `basePrice` | `long` | — |
| `price` | `long` | — |
| `discountPercent` | `int` | — |
| `discountGroupId` | `long?` | — |
| `discountEndDate` | `DateTime?` | — |
| `varietyItemId` | `long?` | — |
| `varietyItem2Id` | `long?` | — |
| `productId` | `long` | — |
| `storeId` | `long` | — |
| `quantity` | `int` | — |
| `actice` | `bool` | — |
| `warranty` | `string` | — |
| `product` | `ProductMinVDto` | — |
| `varietyItem` | `VarietyItemVDto` | — |
| `varietyItem2` | `VarietyItemVDto` | — |

### `ProductMinVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `slug` | `string` | — |
| `secondName` | `string` | — |
| `productLabel` | `string` | — |
| `pictureId` | `long` | — |
| `variety` | `VarietyShowVDto` | — |
| `variety2` | `VarietyShowVDto` | — |
| `picture` | `PictureVDto` | — |

### `ProductOrderItemVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `productItemId` | `long` | — |
| `count` | `int` | — |
| `price` | `double` | — |
| `basePrice` | `double` | — |
| `discountPrice` | `double` | — |
| `discountPercent` | `int` | — |
| `productOrderStoreId` | `long` | — |
| `description` | `string` | — |
| `deleted` | `bool` | — |
| `edited` | `bool` | — |
| `productItem` | `ProductItemFullVDto` | — |

### `ProductOrderStoreVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `price` | `double` | — |
| `basePrice` | `double` | — |
| `discountPrice` | `double` | — |
| `storeId` | `long` | — |
| `productOrderId` | `string` | — |
| `description` | `string` | — |
| `deleted` | `bool` | — |
| `edited` | `bool` | — |
| `deliveryId` | `long?` | — |
| `deliveryPrice` | `double` | — |
| `shippingQuoteId` | `long?` | — |
| `shippingSlotId` | `long?` | — |
| `shippingSlotDate` | `System.DateTime?` | — |
| `shippingIsExpress` | `bool` | — |
| `shippingProvider` | `Entities.Entities.ShippingField.ShippingProviderEnum?` | — |
| `shippingPaymentMode` | `Entities.Entities.ShippingField.ShippingPaymentModeEnum?` | — |
| `shippingQuotedPrice` | `double` | — |
| `paymentPrice` | `double` | — |
| `delivery` | `DeliveryVDto` | — |
| `shipmentStatus` | `Entities.Entities.ShippingField.ShipmentStatusEnum?` | — |
| `shipmentSlotStartUtc` | `System.DateTime?` | — |
| `shipmentSlotEndUtc` | `System.DateTime?` | — |
| `shipmentPickupDeadlineUtc` | `System.DateTime?` | — |
| `shipmentTrackingCode` | `string` | — |
| `shipmentDeliveryCode` | `string` | کد ۵ رقمی تحویل؛ فقط در پاسخِ خودِ مشتری پر می‌شود و برای فروشنده/ادمین همیشه خالی است. |
| `store` | `StoreMinVDto` | — |
| `productOrderItems` | `List<ProductOrderItemVDto>` | — |

### `ProductOrderVDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `id` | `string` | — |
| `orderCode` | `string` | — |
| `userId` | `long` | — |
| `addressId` | `long?` | — |
| `paymentTypeId` | `long` | — |
| `productOrderStatusId` | `long` | — |
| `productOrderStateId` | `long` | — |
| `price` | `double` | — |
| `basePrice` | `double` | — |
| `discountPrice` | `double` | — |
| `paymentPrice` | `double` | — |
| `createDate` | `DateTime` | — |
| `bonusCode` | `string` | — |
| `userDescription` | `string` | — |
| `adminDescription` | `string` | — |
| `stateDescription` | `string` | — |
| `isPaid` | `bool` | — |
| `rebateId` | `long?` | — |
| `rebatePrice` | `double` | — |
| `deliveryTypeId` | `long?` | — |
| `deliveryPrice` | `double` | — |
| `clubFreeDeliveryBenefitId` | `long?` | — |
| `clubDeliveryDiscount` | `double` | — |
| `trackingCode` | `string` | — |
| `userReceived` | `bool?` | — |
| `userReceivedDate` | `System.DateTime?` | — |
| `userReceiveNote` | `string` | — |
| `userReceivedAuto` | `bool` | — |
| `autoConfirmDate` | `System.DateTime?` | — |
| `canConfirmDelivery` | `bool` | — |
| `hasCancelRequestDate` | `DateTime?` | — |
| `walletPrice` | `double` | — |
| `siteShare` | `double` | — |
| `storeShare` | `double` | — |
| `rebate` | `RebateVDto` | — |
| `user` | `UserVDto` | — |
| `address` | `AddressVDto` | — |
| `paymentType` | `CodeVDto` | — |
| `productOrderStatus` | `CodeVDto` | — |
| `productOrderState` | `CodeVDto` | — |
| `deliveryType` | `CodeVDto` | — |
| `productOrderStores` | `List<ProductOrderStoreVDto>` | — |

### `RebateVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `minCartPrice` | `double` | — |
| `codeValue` | `string` | — |
| `priceValue` | `double` | — |
| `isPriceRebate` | `bool` | — |
| `finalPrice` | `double` | — |
| `productId` | `long?` | — |
| `typeId` | `long` | — |
| `maxUsePerUser` | `int` | — |
| `clubRewardId` | `long?` | — |

### `Seo_FieldDto`

پایه: `FullName_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `seoH1` | `string` | — |
| `seoMinDescription` | `string` | — |
| `seoDescription` | `string` | — |
| `seoTitle` | `string` | — |
| `seoPictureAlt` | `string` | — |

### `Seo_Full_FieldDto`

پایه: `Seo_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `seoUrlText` | `string` | — |
| `seoNoIndex` | `bool` | — |
| `seoNoFollow` | `bool` | — |
| `seoCanonical` | `string` | — |

### `SiteVisibilityDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `showToSite` | `bool` | — |

### `StateVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `enName` | `string` | — |
| `countryId` | `long` | — |
| `country` | `CountryVDto` | — |

### `StoreMinVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `storeId` | `long` | — |
| `maxDiscountPercent` | `int` | — |
| `rateAvg` | `double` | — |
| `rateCount` | `int` | — |
| `expressShippingEnabled` | `bool` | این فروشگاه ارسال اکسپرس (۲ تا ۳ ساعت کاری) دارد؛ برای نشان «ارسال اکسپرس» روی محصول. |
| `picture` | `PictureVDto` | — |

### `TripDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `origin` | `PointDto` | — |
| `destination` | `PointDto` | — |
| `secondDestination` | `PointDto` | — |
| `routeLength` | `long` | — |
| `fromCityId` | `long?` | — |
| `roundTrip` | `bool` | — |
| `fromAddress` | `string` | — |
| `toAddress` | `string` | — |
| `driverId` | `long?` | — |
| `price` | `double` | — |
| `userDetail` | `string` | — |
| `userComment` | `string` | — |
| `userRate` | `int` | — |
| `createDate` | `DateTime` | — |
| `tripStartDateTime` | `DateTime?` | — |
| `isOnline` | `bool` | — |
| `driverStatusId` | `long` | — |
| `tripStatusId` | `long` | — |
| `isPaid` | `bool` | — |
| `connectionId` | `string` | — |
| `userToken` | `string` | — |
| `userPetId` | `long?` | — |
| `userId` | `long` | — |
| `tripStopId` | `long?` | — |
| `stopMinutes` | `int?` | — |
| `tripOptionIds` | `List<long>` | — |
| `driverShare` | `double` | — |
| `siteShare` | `double` | — |
| `rebateCode` | `string` | — |
| `discount` | `double` | — |
| `paymentPrice` | `double` | — |
| `rebateId` | `long?` | — |
| `rebatePrice` | `double` | — |
| `fromWallet` | `bool` | — |
| `walletPrice` | `double` | — |
| `progressStageId` | `int` | — |
| `isReturnLeg` | `bool` | — |
| `companionReserveId` | `long?` | — |
| `pansionReserveId` | `long?` | — |
| `schoolReserveId` | `long?` | — |
| `scheduledLeadMinutes` | `int?` | — |
| `scheduledDepartureAt` | `DateTime?` | — |
| `ownerRidesAlong` | `bool` | — |
| `userPetIds` | `List<long>` | — |
| `cancelInitiatorId` | `int?` | — |
| `cancelReasonCodeId` | `long?` | — |
| `cancelReasonDetail` | `string` | — |
| `previousTripId` | `long?` | — |
| `vehicleTypeId` | `long?` | — |
| `driverStatus` | `CodeVDto` | — |
| `user` | `UserVDto` | — |
| `tripStatus` | `CodeVDto` | — |
| `cancelReasonCode` | `CodeVDto` | — |
| `vehicleType` | `CodeVDto` | — |
| `userPet` | `UserPetVDto` | — |
| `driver` | `DriverVDto` | — |
| `fromCity` | `CityVDto` | — |
| `rebate` | `RebateVDto` | — |
| `tripStop` | `TripStopVDto` | — |
| `tripOptions` | `List<TripOptionVDto>` | — |

### `TripOptionVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `price` | `double` | — |
| `active` | `bool` | — |

### `TripPansionReservationCreateDto`

درخواست ساخت سفر پت‌رسانِ متصل به یک رزرو پانسیون (تحویل پت هنگام ورود به هتل/مهد). راننده در لحظه‌ی ثبت انتخاب نمی‌شود؛ دقیقاً مثل TripReservationCreateDto (حالت دو).

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `pansionReserveId` | `long` | — |
| `origin` | `PointDto` | — |
| `destination` | `PointDto` | — |
| `fromAddress` | `string` | — |
| `toAddress` | `string` | — |
| `scheduledLeadMinutes` | `int` | فاصله‌ی زمانی حرکت راننده قبل از لحظه‌ی تحویل — فقط ۶۰ یا ۱۲۰ دقیقه. |
| `ownerRidesAlong` | `bool` | — |
| `userPetIds` | `List<long>` | — |
| `tripOptionIds` | `List<long>` | — |
| `dropOffTime` | `string` | فقط برای پانسیون‌های شبانه/اقامتی (Pansion.IsDaycare == false) لازم است، چون FromDate آن‌ها فقط تاریخ دارد (بدون ساعت). قالب "HH:mm". برای پانسیون‌های روزانه/مهدی نادیده گرفته می‌شود چون StartTime واقعی رزرو خودش این نقش را دارد. |

### `TripServiceInfoVDto`

خلاصه‌ی «سرویس هفتگی»ِ یک نوبت برای راننده (فقط برنامه؛ بدون قیمت و اطلاعات شخصی کاربر). فقط وقتی Trip.PetResanServiceScheduleId پر باشد پر می‌شود.

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `serviceId` | `long` | — |
| `scheduleId` | `long` | ردیف برنامه‌ای که همین نوبت از آن ساخته شده |
| `active` | `bool` | — |
| `startDate` | `DateTime` | — |
| `totalWeeks` | `int?` | — |
| `endDate` | `DateTime?` | — |
| `schedules` | `List<TripServiceScheduleVDto>` | — |

### `TripServiceScheduleVDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `weekDayId` | `long` | — |
| `weekDayName` | `string` | — |
| `weekDayNumber` | `int` | — |
| `time` | `string` | ساعت رفت HH:mm |
| `returnTime` | `string` | ساعت برگشت HH:mm یا null |

### `TripStopVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `price` | `double` | — |
| `active` | `bool` | — |

### `TripVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `origin` | `PointDto` | — |
| `destination` | `PointDto` | — |
| `secondDestination` | `PointDto` | — |
| `routeLength` | `long` | — |
| `fromCityId` | `long?` | — |
| `stopTime` | `int` | — |
| `roundTrip` | `bool` | — |
| `fromAddress` | `string` | — |
| `toAddress` | `string` | — |
| `driverId` | `long?` | — |
| `passengerId` | `long` | — |
| `price` | `double` | — |
| `userDetail` | `string` | — |
| `userComment` | `string` | — |
| `userRate` | `int` | — |
| `createDate` | `DateTime` | — |
| `tripStartDateTime` | `DateTime?` | — |
| `isOnline` | `bool` | — |
| `driverStatusId` | `long` | — |
| `tripStatusId` | `long` | — |
| `isPaid` | `bool` | — |
| `manualPayDate` | `DateTime?` | — |
| `connectionId` | `string` | — |
| `userToken` | `string` | — |
| `userPetId` | `long` | — |
| `userId` | `long` | — |
| `driverShare` | `double` | — |
| `siteShare` | `double` | — |
| `discount` | `double` | — |
| `paymentPrice` | `double` | — |
| `rebateId` | `long?` | — |
| `rebatePrice` | `double` | — |
| `fromWallet` | `bool` | — |
| `walletPrice` | `double` | — |
| `progressStageId` | `int` | — |
| `isReturnLeg` | `bool` | — |
| `progressUpdateDate` | `DateTime?` | — |
| `companionReserveId` | `long?` | — |
| `pansionReserveId` | `long?` | — |
| `schoolReserveId` | `long?` | — |
| `petResanServiceScheduleId` | `long?` | — |
| `petResanService` | `TripServiceInfoVDto` | — |
| `scheduledLeadMinutes` | `int?` | — |
| `scheduledDepartureAt` | `DateTime?` | — |
| `ownerRidesAlong` | `bool` | — |
| `cancelInitiatorId` | `int?` | — |
| `cancelReasonCodeId` | `long?` | — |
| `cancelReasonDetail` | `string` | — |
| `previousTripId` | `long?` | — |
| `vehicleTypeId` | `long?` | — |
| `stopMinutes` | `int?` | — |
| `driverStatus` | `CodeVDto` | — |
| `tripStatus` | `CodeVDto` | — |
| `cancelReasonCode` | `CodeVDto` | — |
| `vehicleType` | `CodeVDto` | — |
| `userPet` | `UserPetVDto` | — |
| `userPets` | `List<UserPetVDto>` | — |
| `driver` | `DriverVDto` | — |
| `user` | `UserVDto` | — |
| `fromCity` | `CityVDto` | — |
| `rebate` | `RebateVDto` | — |
| `wallet` | `WalletVDto` | — |
| `tripStop` | `TripStopVDto` | — |
| `tripOptions` | `List<TripOptionVDto>` | — |

### `UserCurrentLocationVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
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

| فیلد JSON | نوع C# | توضیح صریح سورس |
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

### `UserPetPictureVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `userPetId` | `long` | — |
| `pictureId` | `long` | — |
| `picture` | `PictureVDto` | — |

### `UserPetRecordMinVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `userPetId` | `long` | — |
| `operatorId` | `long` | — |
| `createDate` | `DateTime` | — |
| `operator` | `UserMinVDto` | — |

### `UserPetVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `petId` | `long` | — |
| `userId` | `long` | — |
| `pictureId` | `long?` | — |
| `petBreedId` | `long?` | — |
| `petBreed2Id` | `long?` | — |
| `isMixBreed` | `bool` | — |
| `birthday` | `DateTime` | — |
| `microChipCode` | `string` | — |
| `size` | `string` | — |
| `weight` | `string` | — |
| `isMale` | `bool` | — |
| `isSterile` | `bool` | — |
| `specificDisease` | `string` | — |
| `specificMedicene` | `string` | — |
| `addressValue` | `string` | — |
| `user` | `UserMinVDto` | — |
| `pet` | `PetVDto` | — |
| `picture` | `PictureVDto` | — |
| `userPetRecords` | `List<UserPetRecordMinVDto>` | — |
| `userPetPictures` | `List<UserPetPictureVDto>` | — |

### `UserVDto`

پایه: `Id_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
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

### `VarietyItemVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `label` | `string` | — |
| `varietyId` | `long` | — |
| `varietyName` | `string` | — |

### `VarietyShowVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `description` | `string` | — |
| `label` | `string` | — |

### `WalletVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `isIncrease` | `bool` | — |
| `amount` | `double` | — |
| `createDate` | `DateTime` | — |
| `userId` | `long` | — |
| `paymentId` | `long?` | — |
| `paymentCode` | `string` | — |
| `referenceCode` | `string` | — |
| `productOrderId` | `string` | — |
| `companionReserveId` | `long?` | — |
| `pansionReserveId` | `long?` | — |
| `tripId` | `long?` | — |
| `cargoId` | `long?` | — |
| `companionInsurancePackageSaleId` | `long?` | — |
| `pastilAiSubscriptionId` | `long?` | — |
| `schoolReserveId` | `long?` | — |
| `groomerReserveId` | `long?` | — |
| `clinicReserveId` | `long?` | — |
| `consultationPurchaseId` | `long?` | — |
| `painding` | `bool` | — |
| `ledgerName` | `string` | — |
| `user` | `UserVDto` | — |
| `payment` | `PaymentVDto` | — |
| `productOrder` | `ProductOrderVDto` | — |
| `companionReserve` | `CompanionReserveVDto` | — |
| `pansionReserveVDto` | `PansionReserveVDto` | — |
| `trip` | `TripVDto` | — |
| `cargo` | `CargoVDto` | — |
| `companionInsurancePackageSale` | `CompanionInsurancePackageSaleVDto` | — |

### `WeekDayVDto`

پایه: `Name_FieldDto`

| فیلد JSON | نوع C# | توضیح صریح سورس |
|---|---|---|
| `label` | `string` | — |
| `number` | `int` | — |

## نگهداری قرارداد

این صفحه از سورس جاری قابل بازتولید است. هر تغییر route یا DTO باید با اجرای `python docs/pansion/build_api_reference.py` در جدول عملیات و فرهنگ فیلدها منعکس شود. سازگاری کلاینت را با نوع runtime و قواعد سرویس بررسی کنید، زیرا چند annotation قدیمی با پاسخ واقعی فرق دارند.
