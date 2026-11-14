# قرارداد پیشنهادی بک‌اند: تأیید یا رد رزرو پانسیون و مهد توسط نماینده

**وضعیت سند: پیشنهاد برای توسعه؛ این endpointها و وضعیت‌های جدید در API فعلی پیاده‌سازی نشده‌اند.** این سند برای تیم بک‌اند و تیم اپ نوشته شده تا جریان «نماینده ابتدا قبول/رد می‌کند، کاربر سپس پرداخت می‌کند» قرارداد مشخصی داشته باشد. دامنه، رزرو پانسیون اقامتی و مهد پت، در هر دو حالت عفونی و غیرعفونی است. این سند درخواست تغییر کد نیست.

## ۱. مسئله و مرز API فعلی

در وضعیت فعلی، رزرو با `statusId=46` (`Registered`) و `isReserved=false` ثبت می‌شود؛ callback پرداخت موفق آن را `47` (`Paid`) و `isReserved=true` می‌کند؛ نماینده از `PansionReserveChangeStatus` می‌تواند `48` (`Complete`) را ثبت کند. endpoint مجزای «قبول/رد رزرو توسط پانسیون»، زمان تصمیم، دلیل رد و مهلت تصمیم وجود ندارد. `PUT /api/Companion/PansionReserveChangeStatus` برای تأیید مناسب نیست: فرستادن `47` فقط فیلد وضعیت را تغییر می‌دهد و پرداخت را انجام نمی‌دهد. لغو موجود نیز برای نمایندهٔ عادی مجاز نیست.

فهرست و جزئیات فعلی پنل نماینده از مسیرهای `GET /api/Comapnion/PansionReserve` و `GET /api/Comapnion/PansionReserve/{id}` می‌آیند. **املای `Comapnion` در این دو route موجود است** و تا زمان مهاجرت هماهنگ نباید بی‌خبر تغییر کند. مسیرهای جدید این سند با املای درست `Companion` پیشنهاد شده‌اند.

## ۲. نتیجهٔ مورد انتظار

1. کاربر رزرو را بدون پرداخت ثبت کند؛ رزرو در تب «در انتظار تأیید پانسیون» نماینده ظاهر شود.
2. نمایندهٔ مالک پانسیون در مهلت مشخص، رزرو را قبول یا با دلیل رد کند.
3. فقط پس از قبول، پرداخت برای کاربر باز شود. قبول رزرو **به معنی پرداخت یا قطعی شدن** نیست.
4. پس از callback موفق و کنترل تداخل، رزرو `Paid` و قطعی شود؛ پس از انجام خدمت، نماینده آن را `Complete` کند.
5. تصمیم‌ها، مهلت‌ها، اعلان‌ها و خطاها در API قابل مشاهده و قابل بازیابی باشند؛ تکرار درخواست یا رقابت هم‌زمان وضعیت نامعتبر ایجاد نکند.

## ۳. ماشین وضعیت پیشنهادی

شناسهٔ عددی وضعیت‌های جدید باید هنگام migration در کاتالوگ `Code` تخصیص داده شود. **اعداد ۴۶، ۴۷ و ۴۸ فعلی حفظ شوند؛ کلاینت مقدار وضعیت‌های جدید را از پاسخ API/کاتالوگ بخواند، نه از عدد حدسی.**

| Label پیشنهادی در `PansionReserveStatusEnum` | ID | معنی | `isReserved` |
|---|---:|---|---:|
| `PansionReserveState_AwaitingPansionConfirmation` | جدید؛ تعیین در migration | ثبت اولیه، منتظر تصمیم نماینده | `false` |
| `PansionReserveState_Registered` | `46` | نماینده قبول کرده؛ منتظر پرداخت کاربر | `false` |
| `PansionReserveState_RejectedByPansion` | جدید؛ تعیین در migration | نماینده با دلیل رد کرده | `false` |
| `PansionReserveState_PansionConfirmationExpired` | جدید؛ تعیین در migration | مهلت تصمیم نماینده تمام شده | `false` |
| `PansionReserveState_PaymentExpired` | جدید؛ تعیین در migration | نماینده قبول کرده ولی مهلت پرداخت تمام شده | `false` |
| `PansionReserveState_Paid` | `47` | پرداخت موفق و رزرو قطعی | `true` |
| `PansionReserveState_Complete` | `48` | خدمت پس از پرداخت انجام شده | `true` |

لغو اداری فعلی با `isCancel=true`, `cancelDetail`, `cancelDate` باقی می‌ماند؛ از `RejectedByPansion` برای لغو پس از پرداخت یا رد بدون دلیل استفاده نشود. وضعیت‌های رد، انقضای تصمیم و انقضای پرداخت پایانی‌اند و دیگر پرداخت‌پذیر یا قابل‌تأیید نیستند.

```text
POST رزرو
  └─ AwaitingPansionConfirmation
       ├─ Accept → Registered (46؛ منتظر پرداخت)
       │            ├─ پرداخت موفق → Paid (47) → پایان خدمت → Complete (48)
       │            └─ پایان مهلت پرداخت → PaymentExpired
       ├─ Reject → RejectedByPansion
       └─ پایان مهلت تصمیم → PansionConfirmationExpired
```

تغییر `Paid→Complete` فقط برای رزرو `isReserved=true && isCancel=false` مجاز باشد. `ChangeStatus` موجود باید اجازهٔ فرستادن دلخواه `46`, `47`, `48` یا شناسه‌های جدید را از پنل نماینده ندهد؛ ترجیحاً فقط عمل «ثبت پایان خدمت» را روی انتقال `Paid→Complete` اجرا کند. ساخت `Paid` فقط در مسیر تسویه/پرداخت معتبر مجاز است.

## ۴. داده‌های جدید و معنای دقیق فیلدها

به `PansionReserve` و پاسخ `PansionReserveVDto` حداقل این فیلدها اضافه شوند. همهٔ timestampها در سرور به UTC ذخیره و در JSON با ISO 8601 و پسوند `Z` ارسال شوند.

| فیلد پیشنهادی | نوع | زمان مقداردهی و کاربرد |
|---|---|---|
| `confirmationDeadlineAtUtc` | `DateTimeOffset` | هنگام ثبت؛ آخرین لحظهٔ مجاز تصمیم نماینده |
| `decisionAtUtc` | `DateTimeOffset?` | زمان قبول یا رد واقعی |
| `decisionByUserId` | `long?` | کاربر نماینده‌ای که تصمیم گرفت؛ از token، نه body |
| `rejectionReason` | `string?` | فقط در رد؛ غیرخالی پس از trim و با سقف طول مشخص در اعتبارسنجی |
| `paymentDeadlineAtUtc` | `DateTimeOffset?` | فقط پس از قبول؛ آخرین مهلت آغاز پرداخت معتبر |
| `confirmationSource` | enum/string | `Companion` برای رزرو جدید؛ `Legacy` برای رزروهای قدیمیِ منتقل‌شده، در صورت نیاز به سازگاری |
| `rowVersion` | token رشته‌ای/`rowversion` | جلوگیری از نوشتن تصمیم روی دادهٔ قدیمی و رقابت accept/reject/expiry |
| `canAccept`, `canReject`, `canPay`, `canComplete` | bool در VDto | امکان اقدام براساس وضعیت، مهلت، مالکیت و پرداخت؛ **محاسبهٔ سرور** برای UI |

`isReserved` **فقط پس از پرداخت موفق** `true` شود؛ قبول نماینده آن را تغییر ندهد. `reserveCode`, `price`, `paymentPrice`, `pansionId`, بازهٔ تاریخ/ساعت و پت در پاسخ‌های موجود حفظ شوند. قیمت خدمت در زمان ساخت رزرو از قیمت مرکز محاسبه و برای همان رزرو ثابت شود؛ ویرایش قیمت مرکز، مبلغ رزرو در انتظار تصمیم را تغییر ندهد. تغییر تخفیف/کیف پول و مبلغ پرداخت در مرحلهٔ پس از قبول و پیش از شروع پرداخت، طبق قواعد مالی سرور باشد.

مهلت تصمیم و مهلت پرداخت باید از تنظیمات محصول/ادمین تعیین و در هر رزرو snapshot شوند. مقدار عددی این دو مهلت هنوز تصمیم محصول است؛ قرارداد API نباید یک مدت حدسی را hardcode کند. رابط از `confirmationDeadlineAtUtc` و `paymentDeadlineAtUtc` پاسخ برای شمارش معکوس استفاده کند؛ سرور مالک اجرای deadline است.

## ۵. endpointهای جدید پنل نماینده

هر دو endpoint با Bearer token، `CompanionId` غیرnull و مجوز تصمیم برای همان Companion کار کنند. `Pansion.CompanionId` رزرو باید با Companion token برابر باشد؛ صرف `[Authorize]` کافی نیست. `id`، دلیل رد و `rowVersion` تنها داده‌های client هستند؛ `decisionByUserId` و زمان تصمیم از سرور گرفته شوند.

### ۵.۱. قبول رزرو

```http
PUT /api/Companion/PansionReserveAccept
Authorization: Bearer <token>
Content-Type: application/json

{"id":501,"rowVersion":"AAAAAAAAB9E="}
```

**شرایط لازم:** رزرو وجود داشته باشد و متعلق به پانسیون همین نماینده باشد؛ `status=AwaitingPansionConfirmation`, `isReserved=false`, `isCancel=false`؛ زمان سرور قبل از `confirmationDeadlineAtUtc` باشد؛ پانسیون هنوز فعال، تأییدشده و حذف‌نشده باشد؛ بازهٔ رزرو در گذشته نرفته باشد؛ و تعارض رزرو پذیرفته‌شده/پرداخت‌شدهٔ همان پت در همان پانسیون وجود نداشته باشد. بررسی تداخل و تغییر وضعیت باید در تراکنش با کنترل هم‌زمانی انجام شود.

**اثر موفق:** `statusId=46`, `decisionAtUtc=now`, `decisionByUserId=currentUser`, `paymentDeadlineAtUtc=now+configuredDuration`, `rejectionReason=null`, `isReserved=false`. پاسخ `BaseResultDto<PansionReserveVDto>` با وضعیت تازه برگردد. `canPay` در پاسخ صاحب رزروِ کاربر نهایی `true` و در پاسخ نماینده `false` باشد؛ action flagها نسبت به نقش درخواست‌کننده محاسبه شوند. به کاربر اعلان «پانسیون رزرو را پذیرفت؛ تا مهلت مشخص پرداخت کنید» ارسال شود. ارسال اعلان پس از commit و idempotent باشد.

### ۵.۲. رد رزرو

```http
PUT /api/Companion/PansionReserveReject
Authorization: Bearer <token>
Content-Type: application/json

{"id":501,"rejectionReason":"در این بازه امکان پذیرش نداریم.","rowVersion":"AAAAAAAAB9E="}
```

**شرایط لازم:** همان کنترل مالکیت، pending بودن، عدم لغو/پرداخت و باز بودن مهلت تصمیم. `rejectionReason` پس از trim الزامی است و سقف طول آن باید در قرارداد نهایی تعیین و enforce شود؛ متن برای خروجی کاربر sanitize شود. **اثر موفق:** status به `RejectedByPansion`، `decisionAtUtc`, `decisionByUserId`, `rejectionReason` ثبت، `paymentDeadlineAtUtc=null` و پرداخت برای این رزرو بسته شود. دلیل رد به کاربر در detail و اعلان قابل مشاهده باشد. رد هیچ پرداخت یا استردادی انجام نمی‌دهد، چون این جریان پرداخت را تا پیش از قبول مسدود کرده است.

### ۵.۳. پاسخ و خطاها

نمونهٔ شکل پاسخ موفق؛ status ID جدید در این نمونه عمداً عدد ندارد:

```json
{
  "isSuccess": true,
  "code": 0,
  "messages": [],
  "data": {
    "id": 501,
    "reserveCode": "...",
    "statusId": 46,
    "isReserved": false,
    "isCancel": false,
    "confirmationDeadlineAtUtc": "2026-10-08T09:00:00Z",
    "decisionAtUtc": "2026-10-08T08:20:00Z",
    "decisionByUserId": 77,
    "rejectionReason": null,
    "paymentDeadlineAtUtc": "2026-10-08T08:50:00Z",
    "rowVersion": "AAAAAAAAB9Y=",
    "canAccept": false,
    "canReject": false,
    "canPay": false,
    "canComplete": false
  }
}
```

زمان‌های نمونه فقط نمونهٔ قالب‌اند؛ مدت ۳۰ دقیقه در آن قانون قطعی محصول نیست. خطاهای دامنه باید شناسه/`code` پایدار داشته باشند، با این معنی‌ها: `PansionReserveNotFound`, `CompanionContextRequired`, `AccessDenied`, `DecisionDeadlineExpired`, `InvalidDecisionState`, `AlreadyPaidOrCancelled`, `RejectionReasonRequired`, `ReservationConflict`, `PansionUnavailable`, `StaleRowVersion`. اگر قرارداد فعلی همهٔ خطاها را با HTTP 200 برمی‌گرداند، `isSuccess=false` و `code` پایدار الزامی است؛ استفاده از 403/404/409/422 هم باید با اپ هماهنگ شود. پیام فارسی تنها کلید منطق برنامه نباشد.

تکرار **همان تصمیم موفق** با همان `id` باید idempotent باشد: وضعیت را عوض نکند و اعلان/رویداد دوباره نسازد. درخواست تصمیم مخالف یا `rowVersion` قدیمی باید conflict بدهد. اگر مهلت گذشته، حتی قبل از اجرای job انقضا، accept/reject باید رد شود و وضعیت در تراکنش به Expired برسد.

## ۶. تغییر endpointهای موجود

| بخش موجود | تغییر لازم |
|---|---|
| ساخت رزرو | پس از اعتبارسنجی فعلیِ نوع مرکز، بیماری پت، زمان، قیمت و مالکیت پت، به‌جای `Registered=46` وضعیت جدید `AwaitingPansionConfirmation` و `confirmationDeadlineAtUtc` ثبت شود. `isReserved=false` بماند. اعلان نماینده به تب pending لینک دهد. |
| `GET /api/Comapnion/PansionReserve` | فیلتر `statusId` وضعیت‌های جدید را بپذیرد؛ `list` فیلدهای تصمیم، deadline و action flags را برگرداند. برای تب «در انتظار تأیید»، `statusId` وضعیت جدید ارسال شود. فیلتر `isCancel` نیز برای تب‌ها پیشنهاد می‌شود. |
| `GET /api/Comapnion/PansionReserve/{id}` | همین فیلدها، دلیل رد و timestampهای تصمیم را برگرداند؛ مالکیت Companion را با `CompanionId` غیرnull enforce کند. |
| پرداخت کاربر | شروع پرداخت، اعمال تخفیف/کیف پول و callback فقط در مرحلهٔ پذیرفته‌شدهٔ `Registered=46` و قبل از مهلت پرداخت مجاز شوند؛ owner رزرو هم چک شود. callback با تراکنش و کنترل دوبارهٔ تداخل، `Paid=47`, `isReserved=true` کند. |
| `PansionReserveChangeStatus` | برای نماینده فقط `Paid→Complete` روی رزرو قطعی لغونشده مجاز باشد؛ `statusId=47` یا وضعیت‌های تصمیم از این endpoint ممنوع شوند. شرط `paymentPrice==0` به‌تنهایی مانع تکمیل رزرو معتبر رایگان نشود. |
| لغو | لغو ادمین موجود از تصمیم نماینده جدا بماند. اگر لغو پیش از پرداخت برای کاربر لازم است، endpoint و قواعد مستقل آن طراحی شوند؛ رد نماینده جای آن نیست. |

قیمت صفر بر اثر تخفیف کامل یا پرداخت کامل با کیف پول نیاز به مسیر تسویهٔ معتبر بدون PSP دارد: پس از قبول و اعتبارسنجی مالی، همان جریان اتمیِ پرداخت باید `Paid=47` و `isReserved=true` ثبت کند. تغییر status دستی جای تسویهٔ صفرمبلغ نیست.

## ۷. مهلت‌ها، رقابت هم‌زمان و پرداخت دیررس

- job انقضا رزرو pending را پس از `confirmationDeadlineAtUtc` به `PansionConfirmationExpired` و رزرو پذیرفته‌شدهٔ پرداخت‌نشده را پس از `paymentDeadlineAtUtc` به `PaymentExpired` ببرد. actionها در لحظهٔ درخواست هم deadline را چک کنند؛ صحت به اجرای به‌موقع job وابسته نباشد.
- قبول، رد، expiry و callback پرداخت باید بر یک رزرو با تراکنش/`rowVersion` رقابت کنند؛ فقط یک انتقال برنده شود. دو درخواست هم‌زمان قبول و رد نباید هر دو موفق شوند.
- هنگام قبول، همان پت در همان پانسیون و بازهٔ هم‌پوشان نباید دو رزرو هم‌زمانِ پذیرفته‌شده و قابل‌پرداخت داشته باشد. نگه‌داشتن این نوبت تا پایان مهلت پرداخت لازم است؛ قاعدهٔ ظرفیت کل همچنان جداگانه و در مدل فعلی ناموجود است.
- پرداختی که پیش از deadline شروع شده ولی callback آن دیر می‌رسد، باید طبق وضعیت attempt و پاسخ درگاه reconcile شود. job نباید attempt فعال را بدون تعیین تکلیف منقضی و سپس پرداخت موفق را بی‌رزرو باقی بگذارد. در صورت دریافت وجه برای رزروی که دیگر قابل قطعی‌شدن نیست، مسیر برگشت وجه/پیگیری مالی باید ثبت شود؛ callback نباید صرفاً `Paid` را به زور بنویسد.
- کلید idempotency پرداخت و اعلان‌ها به `reserveId`/نوع رویداد وابسته باشد؛ retry شبکه، دو بار سهم، امتیاز یا پیام ایجاد نکند.

## ۸. سازگاری داده‌های قدیمی و انتشار

رزروهای قدیمی `Registered=46` قبل از این تغییر، تأیید نماینده نداشته‌اند. migration باید آن‌ها را صریحاً `confirmationSource=Legacy` یا معادل آن علامت بزند و سیاست روشن داشته باشد: **پیشنهاد سازگار** این است که همان رزروهای ۴۶ قبلی همچنان «مجاز به پرداخت طبق جریان قدیم» تلقی شوند، بدون ساخت تصمیم جعلی به نام نماینده. رزروهای جدید فقط از مسیر pending/accept به ۴۶ برسند. رزروهای ۴۷/۴۸ و سوابق لغو بدون تغییر وضعیت باقی بمانند.

برای جلوگیری از شکست نسخهٔ قدیمی اپ، قابلیت جدید با rollout کنترل‌شده یا حداقل نسخهٔ پشتیبانی‌شده فعال شود. فهرست و detail قدیمی حفظ شوند؛ endpointهای جدید روی مسیر درست `Companion` اضافه شوند. دربارهٔ اصلاح typo `Comapnion`، ابتدا alias/backward compatibility و سپس مهاجرت client انجام شود.

## ۹. قرارداد لازم برای ساخت UI نماینده

| صفحه/دکمه | شرط سروری و دادهٔ مورد نیاز |
|---|---|
| تب «در انتظار تأیید» | فیلتر status جدید + `canAccept`/`canReject`, `confirmationDeadlineAtUtc`, `totalCount` |
| کارت رزرو pending | کد، مرکز، نام مشتری و پت، بیماری/نوع عفونی، نوع مهد/اقامتی، بازه، قیمت snapshot، زمان باقیمانده |
| دکمهٔ «قبول» | `canAccept=true`؛ call `PansionReserveAccept` با `id,rowVersion`؛ سپس refresh detail/list |
| دکمهٔ «رد» | `canReject=true`؛ دریافت دلیل غیرخالی؛ call `PansionReserveReject`؛ سپس refresh |
| تب «در انتظار پرداخت» | `statusId=46` فقط برای پذیرفته‌شده یا Legacy، نمایش `paymentDeadlineAtUtc` اگر موجود است |
| تب «پرداخت‌شده» | `statusId=47`, `isReserved=true`, `isCancel=false`؛ فقط دکمهٔ «اعلام پایان اقامت/مهد» |
| تب «تکمیل‌شده» | `statusId=48`؛ فقط مشاهدهٔ نتیجه/سوابق |
| رد/انقضا | statusهای جدید و `rejectionReason`/deadline؛ بدون دکمهٔ پرداخت یا تکمیل |

تا پیش از پیاده‌سازی این قرارداد در بک‌اند، اپ نباید دکمه‌های «قبول/رد» یا تب واقعی «در انتظار تأیید» را با تغییر دستی `statusId` شبیه‌سازی کند.

## ۱۰. معیار پذیرش بک‌اند

- [ ] رزرو جدید قبل از تصمیم نماینده پرداخت‌پذیر نیست و در فهرست scoped نماینده دیده می‌شود.
- [ ] نماینده فقط رزرو پانسیون متعلق به `CompanionId` خود را قبول/رد می‌کند؛ token بدون CompanionId رد می‌شود.
- [ ] رد بدون دلیل، تصمیم پس از deadline، تصمیم دوم مخالف و تصمیم روی رزرو پرداخت‌شده/لغوشده رد می‌شود.
- [ ] قبول، `isReserved` را تغییر نمی‌دهد؛ تنها تسویهٔ معتبر آن را `true` و status را ۴۷ می‌کند.
- [ ] دو تصمیم هم‌زمان و دو رزرو هم‌پوشان برای همان پت به وضعیت ناسازگار نمی‌رسند.
- [ ] expiry و callback دیررس، پرداخت و استرداد احتمالی را قابل پیگیری نگه می‌دارند.
- [ ] `Paid→Complete` تنها برای رزرو قطعی لغونشده انجام می‌شود؛ وضعیت ۴۷ از مسیر تغییر وضعیت نماینده قابل جعل نیست.
- [ ] لیست/detail فیلدهای مهلت، دلیل و action flags را می‌دهند؛ اپ می‌تواند همهٔ تب‌ها را بدون حدس از روی متن پیام بسازد.
- [ ] رزروهای قدیمی ۴۶/۴۷/۴۸ پس از migration رفتار مشخص و سازگار دارند.

## ۱۱. محل‌های اصلی تغییر در همین مخزن

`Entities/Entities/PansionField/PansionReserve.cs`؛ `Application/Common/Enumerable/Code/PansionReserveStatusEnum.cs` و داده‌های کاتالوگ Code؛ `Application/Services/PansionSrvs/PansionReserveSrv/Dto/`؛ `Application/Services/PansionSrvs/PansionReserveSrv/PansionReserveService.cs`؛ controllerهای `Api/Areas/Companion/Controllers/PansionReserve*.cs` و بخش آغاز/callback پرداخت پانسیون؛ migration پایگاه‌داده، اعلان‌ها و آزمون‌های انتقال/رقابت. این فهرست راهنمای توسعه است؛ هیچ‌یک از تغییرهای پیشنهادی این سند به‌عنوان قابلیت موجود معرفی نمی‌شود.
