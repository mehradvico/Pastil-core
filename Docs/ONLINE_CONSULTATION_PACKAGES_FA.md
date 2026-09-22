# طراحی «پکیج مشاوره آنلاین» (کانال × مدت) — سند طراحی و برنامه‌ی اجرا

> وضعیت: **طراحی تأییدشده — فاز ۱ (پکیج‌ها) ، فاز ۲ (خرید/پرداخت/بازپرداخت) ، فاز ۳ (شروع نماینده/پنجره‌ی زمانی) و فاز ۴ (اعلان‌ها و نوار بازگشت) انجام شد؛ فازهای ۵ و ۶ مانده** · تاریخ: ۲۹ شهریور ۱۴۰۵  
> هدف: نماینده/کلینیک برای «مشاوره آنلاین» (خدمت کاتالوگ `Assistance` شناسه‌ی ۱۵) پکیج‌هایی به‌تفکیک **کانال** (چت، تماس درون‌برنامه، تماس تصویری، تماس تلفنی) و **مدت** (۳۰ دقیقه / ۱ ساعت) تعریف می‌کند؛ کاربر آن را می‌خرد؛ نماینده با کاربر ارتباط را برقرار و «شروع» می‌کند؛ تا پایان پنجره‌ی زمانی، کاربر می‌تواند به گفتگو/تماس برگردد؛ بعد از پایان، ورود بسته می‌شود. الگو: دکتر اسنپ.

## ۱) تصمیم‌های گرفته‌شده (تأییدشده توسط مالک محصول)

| موضوع | تصمیم |
| --- | --- |
| زیرساخت | **جدول‌های کاملاً مستقل** از `CompanionAssistancePackage` / `CompanionReserve` (تعریف پکیج، خرید و جلسه جدا). پرداخت با اتصال جدید به `PaymentService` (الگو: `PaymentCallbackTypeEnum.PastilAI`) |
| زمان‌بندی | خرید **فوری** (بدون انتخاب ساعت)؛ پنجره‌ی ۳۰/۶۰ دقیقه‌ای از **لحظه‌ی «شروع» توسط نماینده** حساب می‌شود |
| مقدار اولیه‌ی پکیج‌ها | برای همه‌ی کلینیک‌های فعال ۸ پکیج (۴ کانال × ۲ مدت) **غیرفعال با قیمت ۰**؛ کلینیک قیمت می‌گذارد و فعال می‌کند |
| بعد از پایان پنجره | چت فقط‌خواندنی (تاریخچه برای هر دو)، تماس بسته؛ عدم شروع نماینده ⇒ لغو خودکار + بازپرداخت به کیف پول |
| اعلان به نماینده | با خرید موفق برای نماینده(ها) نوتیف «رزرو مشاوره داری؛ بیا ارتباط را برقرار کن» می‌رود |
| کارمزد پاستیل | **۰٪** (تمام مبلغ سهم کلینیک؛ `CommissionPercent` فعلاً نادیده گرفته می‌شود و بعداً قابل فعال‌سازی است) |
| تخفیف | **بله، مثل رزرو فعلی** (Rebate: نوع جدید `RebateType`، اعتبارسنجی، SetRebate/RemoveRebate) |
| مهلت شروع / لغو | ۲۴ ساعت بعد از پرداخت؛ لغو آزاد کاربر قبل از «شروع» با بازپرداخت کامل |
| نمایندگان مجاز | مالک کلینیک + کاربران تخصیص‌یافته روی خدمت ۱۵ (`CompanionAssistanceUsers`) |
| مدت‌ها | ثابت ۳۰ و ۶۰ دقیقه |

## ۲) مفاهیم و نقش‌ها

- **پکیج مشاوره** (`ConsultationPackage`): تعریف کلینیک: کانال + مدت + قیمت + فعال/غیرفعال. هر کلینیک حداکثر یک پکیج فعال برای هر جفت (کانال، مدت) (۸ ترکیب).
- **خرید مشاوره** (`ConsultationPurchase`): سفارش کاربر برای یک پکیج؛ وضعیت، پرداخت، زمان‌ها، نماینده‌ی شروع‌کننده.
- **نماینده**: مالک کلینیک یا کاربر عضو فعال کلینیک (`CompanionUser` فعال و تأییدشده) که روی خدمت «مشاوره آنلاین» همان کلینیک مجاز است (`CompanionAssistanceUser`). اولین نفری که «شروع» را بزند نماینده‌ی آن خرید می‌شود.
- **جلسه‌ی آنلاین** (`OnlineSession` موجود): با این کار فیلدهای `ConsultationPurchaseId` و `StartDate/ExpireDate` می‌گیرد؛ چت (`OnlineSessionMessage`)، تماس (`CallHub.JoinSessionCall`) و پوش‌ها بدون تغییر ساختاری از همین جلسه استفاده می‌کنند.

## ۳) مدل داده (مایگریشن جدید؛ نیازمند تأیید قبل از اجرا روی سرور)

```
ConsultationPackage
  Id, CompanionId (کلینیک), AssistanceId=15 (کاتالوگ), ChannelId (1..4 = OnlineSessionChannelEnum),
  DurationMinutes (30 | 60), Price (تومان), Active, Deleted, CreateDate, UpdateDate
  یکتا: (CompanionId, ChannelId, DurationMinutes) روی ردیف‌های Deleted=0

ConsultationPurchase
  Id, PurchaseCode, UserId, ConsultationPackageId, CompanionId,
  ChannelId, DurationMinutes, PriceSnapshot           ← تصویر لحظه‌ی خرید (تغییر بعدی پکیج روی خرید اثر ندارد)
  Status: PendingPayment | Paid | Active | Completed | Expired | Cancelled | Refunded
  PaymentId?, PaymentPrice, WalletPrice, FromWallet, PaidDate
  CommissionPercent, CompanionShare, SiteShare        ← تسهیم درآمد در لحظه‌ی پرداخت (مثل رزرو)
  AgentUserId? (نماینده‌ی شروع‌کننده), OnlineSessionId?
  StartDate?, ExpireDate?                             ← StartDate = لحظه‌ی «شروع»؛ ExpireDate = StartDate + DurationMinutes
  StartDeadline                                       ← PaidDate + ۲۴ ساعت (قابل تنظیم)؛ بعد از آن لغو خودکار
  CancelDate?, CancelReason?, RefundDate?
  CreateDate

OnlineSession (فیلدهای جدید)
  ConsultationPurchaseId?, ExpireDate?   ← نال = جلسه‌ی آزاد قبلی (بدون مدت)؛ غیرنال = جلسه‌ی مدت‌دار
```

- خدمت `Assistance` شناسه‌ی ۱۵ باید برای هر کلینیک یک `CompanionAssistance` فعال داشته باشد (برای مالکیت/کارمزد `CommissionPercent`، و فهرست نمایندگان `CompanionAssistanceUser`). اسکریپت seed آن را در صورت نبود می‌سازد (بند ۹).
- نوع پرداخت جدید: کد `PaymentType_ConsultationPurchase` (ردیف جدول `Codes`)، مقدار جدید `PaymentCallbackTypeEnum.ConsultationPurchase`، بدون ستون FK جدید روی `Payments` (از `CallBackId` استفاده می‌شود، مثل PastilAI).

## ۴) ماشین حالت خرید

```
PendingPayment ──(پرداخت موفق)──▶ Paid ──(نماینده «شروع» را می‌زند)──▶ Active ──(ExpireDate می‌گذرد)──▶ Completed
      │                              │                                        │
      │(پرداخت ناموفق/۳۰ دقیقه)      │(StartDeadline می‌گذرد، شروع نشد)        └─(قطع ارتباط زودتر: پنجره همچنان باز می‌ماند)
      ▼                              ▼
  Cancelled                     Expired ─▶ Refunded (کیف پول)
                        Paid ──(لغو کاربر قبل از شروع)──▶ Cancelled ─▶ Refunded
```

قواعد:
1. `Active` فقط یک‌بار و فقط از `Paid` ساخته می‌شود (شروع مجدد ممکن نیست؛ پنجره یک‌بار شروع می‌شود).
2. لغو کاربر فقط قبل از «شروع»؛ مبلغ کامل به کیف پول.
3. کاربر همزمان بیش از یک خرید `Paid/Active` برای یک کلینیک+کانال نداشته باشد (جلوگیری از خرید تکراری اشتباهی).
4. همه‌ی گذارها با `UPDATE ... WHERE Status = <قبلی>` (مقایسه‌ی اتمی) تا دو نماینده هم‌زمان «شروع» را نزنند و پرداخت دوباره اعمال نشود.

## ۵) جریان‌ها

### ۵.۱ تعریف پکیج (نماینده/کلینیک) — طراحی جدا از پکیج‌های عادی
- صفحه‌ی جدید در پنل نماینده: **«پکیج‌های مشاوره آنلاین»** (جدا از «خدمات/پکیج‌ها»)؛ یک ماتریس ۴×۲ (کانال × مدت) که هر خانه: قیمت + سوئیچ فعال.
- API: `GET/PUT /api/Companion/ConsultationPackage` (PUT کل ماتریس را ذخیره می‌کند؛ اعتبارسنجی: قیمت ≥ ۰، فعال کردن فقط با قیمت > ۰، مدت فقط ۳۰/۶۰، کانال فقط ۱..۴).
- این پکیج‌ها در هیچ لیست پکیج عادی دیده نمی‌شوند (جدول جدا).

### ۵.۲ خرید (کاربر)
1. کاربر «مشاوره آنلاین» را از خدمات آنلاین کلینیک انتخاب می‌کند ⇒ صفحه‌ی **انتخاب کانال و مدت** با فقط پکیج‌های فعال (`GET /api/EndUser/ConsultationPackage?companionId=`).
2. `POST /api/EndUser/ConsultationPurchase {packageId, fromWallet}` ⇒ ساخت خرید `PendingPayment` با `PriceSnapshot`.
3. پرداخت با درگاه/کیف پول: `POST /api/EndUser/ConsultationPurchasePayment` (بازپرداخت مبلغ با الگوی PaymentService).
4. callback موفق ⇒ `Paid` + `PaidDate` + `StartDeadline` + محاسبه‌ی تسهیم + **نوتیف به نماینده‌ها** (بند ۶) + نوتیف تأیید به کاربر.

### ۵.۳ شروع توسط نماینده
- صفحه‌ی **«مشاوره‌های من»** (پنل کاری نماینده، جدا از «خدمات تخصیص‌یافته»): فهرست خریدهای `Paid/Active` کلینیک (نمایندگان مجاز) با نام/عکس/پت‌های کاربر، کانال و مدت.
- دکمه‌ی «شروع» ⇒ `POST /api/Companion/ConsultationPurchase/{id}/Start`:
  - اتمی: `Paid → Active`، `AgentUserId`، `StartDate=Now`، `ExpireDate=Now+Duration`؛
  - ساخت/اتصال `OnlineSession` (کانال همان پکیج، `ExpireDate` جلسه) ؛
  - پوش به کاربر (بند ۶) و هدایت نماینده: چت ⇒ `/online-chat/{sessionId}`؛ تماس درون‌برنامه/تصویری ⇒ `/call/session/{sessionId}`؛ تلفنی ⇒ `tel:` (مثل امروز).
- بعد از `Active` نماینده هر چند بار بخواهد در پنجره تماس/چت را دوباره باز می‌کند (همان دکمه‌ی «ورود» در «مشاوره‌های من»).

### ۵.۴ بازگشت کاربر در پنجره (پاپ‌آپ/مودال ثابت)
- کامپوننت سراسری `ConsultationReturnBar` (زیر هدر/بالای نوار پایین در همه‌ی صفحات): وقتی کاربر خریدی `Active` و منقضی‌نشده دارد، نوار «مشاوره‌ی ۳۰ دقیقه‌ای با X در جریان است — ۱۸:۴۲ مانده [بازگشت به چت/تماس]» با شمارش معکوس نشان می‌دهد؛ با زدن، به `/online-chat/{id}` یا `/call/session/{id}` می‌رود.
- منبع داده: `GET /api/EndUser/ConsultationPurchase/Active` (پول‌مبتنی: هر ۱۵–۳۰ ثانیه + هنگام فوکوس)، ساعت مبنا **ساعت سرور** (`serverNow` در پاسخ) تا با ساعت دستگاه اختلاف نیفتد.
- با رسیدن `ExpireDate` نوار ناپدید می‌شود (سمت کلاینت با شمارش معکوس و سمت سرور با کنترل).

### ۵.۵ پایان پنجره
- کنترل در **سرور** (نه فقط UI):
  - `OnlineSessionService.SendMessageAsync`: اگر جلسه `ExpireDate` دارد و گذشته ⇒ رد (چت فقط‌خواندنی؛ خواندن پیام‌ها همچنان مجاز).
  - `CallHub.JoinSessionCall`: اگر `ExpireDate` گذشته ⇒ `callError`؛ در پنجره‌ی فعال برای هر دو طرف مجاز.
  - تماس در حال انجام هنگام رسیدن `ExpireDate`: هاب یک `callEnded` می‌فرستد (تایمر سمت سرور) و اتصال را می‌بندد.
- Hangfire (هر دقیقه): `Active` با `ExpireDate` گذشته ⇒ `Completed`؛ `Paid` با `StartDeadline` گذشته ⇒ `Expired` + بازپرداخت + نوتیف به کاربر.

### ۵.۶ کانال تماس تلفنی
- پنجره از «شروع» حساب می‌شود؛ تماس بیرون از پاستیل است. نوار بازگشت کاربر فقط شمارش معکوس و متن «نماینده با شما تماس می‌گیرد» را نشان می‌دهد (دکمه‌ی بازگشت ندارد).

## ۶) اعلان‌ها (Push + درون‌برنامه)

> مقصد کلیک پوش‌ها در وب‌اپ: کاربر `/consultations`، نماینده `/consultations/manage` (ساخته‌شده در فاز ۵)؛ پوش ۷۳ به گفتگو/تماس همان جلسه می‌رود.

| رویداد | گیرنده | متن | مقصد |
| --- | --- | --- | --- |
| خرید موفق | **نماینده‌ها** (مالک + اعضای فعال) | «رزرو مشاوره‌ی {کانال}/{مدت} از {کاربر} داری؛ بیا ارتباط را برقرار کن» | `/consultations` (مشاوره‌های من) |
| خرید موفق | کاربر | «مشاوره‌ی شما ثبت شد؛ منتظر شروع نماینده باشید» | `/consultation/{id}` |
| شروع | کاربر | «{نماینده} مشاوره‌ی شما را شروع کرد» (چت) / زنگ تماس (تماس/تصویری) | چت / صفحه‌ی تماس |
| ۵ دقیقه به پایان | هر دو | «۵ دقیقه‌ی دیگر پنجره‌ی مشاوره تمام می‌شود» | همان جلسه |
| لغو/انقضا و بازپرداخت | کاربر | «مشاوره‌ی شما شروع نشد؛ مبلغ به کیف پول برگشت» | کیف پول |

نوع‌های جدید `PushTypeEnum` + الگوی متنی (`PushPatterns`) + منبع فارسی `Pattern.resx/fa.resx` (طبق تست `EveryPushType_HasAPersianResource`).

## ۷) نقاط تماس با کد موجود

- `PaymentService`: افزودن `ConsultationPurchase` به: شروع پرداخت (مثل `InsertCompanionInsurancePackageSalePaymentAsyncDto`)، تطبیق snapshot، `ApplySuccessfulPaymentAsync`، `HandleFailedPaymentAsync`، `GetPaymentTypeLabel`، پرداخت دستی ادمین (اختیاری). **تغییر کد مالی ⇒ تست واحد اجباری.**
- `OnlineSessionService` / `CallHub`: گیت زمانی (`ExpireDate`) و اتصال به خرید.
- `IWalletService`: بازپرداخت (مثل لغو رزرو).
- `FinanceCompanionService`: درآمد کلینیک از خرید مشاوره (تسهیم با `CommissionPercent` خدمت).
- پنل ادمین (Nuxt `panel/`): فهرست/جزئیات خریدهای مشاوره + پکیج‌های کلینیک‌ها (فاز پایانی).
- وب‌اپ: صفحه‌ی انتخاب پکیج، صفحه‌ی وضعیت خرید، «مشاوره‌های من» (نماینده)، صفحه‌ی تعریف پکیج (نماینده)، `ConsultationReturnBar`.

## ۸) موارد لبه و امنیت

- قیمت همیشه از `PriceSnapshot`/سرور؛ کلاینت قیمت نمی‌فرستد. تغییر پکیج بعد از خرید اثری ندارد.
- خرید فقط توسط صاحبش خوانده/لغو می‌شود؛ «شروع» فقط توسط نمایندگان مجازِ همان کلینیک؛ ادمین خواندن.
- ضدتکرار: دو کلیک روی «شروع»/«پرداخت»؛ callback تکراری درگاه (idempotent).
- ساعت سرور مبنای همه‌ی محاسبه‌هاست؛ ذخیره‌ی `DateTime` سرور مثل بقیه‌ی پروژه (`DateTime.Now` محلی).
- بازپرداخت دوباره‌کاری نشود (`Status` اتمی + `RefundDate`).
- کاربر پکیج فعال را بدون کلینیکِ فعال نمی‌بیند؛ پکیج با قیمت ۰ هرگز به کاربر نشان داده نمی‌شود/قابل خرید نیست.
- حریم خصوصی: نماینده فقط برای خرید خودش عکس/پت‌های کاربر را می‌بیند (همان قاعده‌ی فعلی `TargetPets`).

## ۹) داده‌ی اولیه (اسکریپت SQL جدا، بدون اجرا روی سرور)

`backend/scripts/Seed-ConsultationPackages.sql` (idempotent):
1. برای هر کلینیک فعال (`Companions`: `Active=1, Approved=1, Deleted=0`) اگر `CompanionAssistance` برای `AssistanceId=15` نبود، ساخته می‌شود (`Active=1`، `Approved=1`، `CommissionPercent` = پیش‌فرض کلینیک/سایت).
2. برای هر کلینیک ۸ ردیف `ConsultationPackage` با `Price=0, Active=0` (فقط در صورت نبودن).
3. گزارش نهایی: تعداد کلینیک‌ها، تعداد `CompanionAssistance` ساخته‌شده، تعداد پکیج‌های ساخته‌شده.

## ۱۰) برنامه‌ی اجرا (فازها)

| فاز | خروجی | وضعیت |
| --- | --- | --- |
| ۱ | موجودیت‌ها (`ConsultationPackage`، `ConsultationPurchase`) + ستون‌های `OnlineSession.ExpireDate/ConsultationPurchaseId` + مایگریشن `AddConsultationPackages` + Seed SQL + API تعریف پکیج (`/api/Companion/ConsultationPackage`) و نمای کاربر (`/api/EndUser/ConsultationPackage`) + تست واحد | **انجام شد** |
| ۲ | خرید + پرداخت (PaymentService، `PaymentCallbackTypeEnum.ConsultationPurchase`، کد `PaymentType_ConsultationPurchase`) + کیف پول (`Wallet.ConsultationPurchaseId`) + تخفیف (`RebateType_ConsultationPurchase`) + لغو کاربر/بازپرداخت + job لغو خودکار (`ConsultationExpireOverdue`، هر دقیقه) + تست واحد | **انجام شد** (تست واقعی با کیف پول؛ درگاه بانکی و کد تخفیف تست نشده) |
| ۳ | شروع نماینده (`/api/Companion/ConsultationSession`: فهرست، `{id}/Start`، `{id}/Enter`) + جلسه‌ی مدت‌دار (`OnlineSession.ExpireDate`) + گیت زمانی سمت سرور (چت فقط‌خواندنی، `JoinSessionCall` رد می‌شود، تماس در جریان با `CallWindowScheduler` بسته می‌شود) + job `ConsultationCompleteExpired` + `GET /api/EndUser/ConsultationPurchase/Active` | **انجام شد** (تست واقعی روی دیتابیس لوکال) |
| ۴ | اعلان‌ها: خرید موفق (نماینده‌ها ۷۱ + کاربر ۷۲)، ۵ دقیقه به پایان (هر دو ۷۳)، لغو خودکار و بازپرداخت (کاربر ۷۴) با `ConsultationNotificationService` (dedupe بر اساس `PushNotifications`، مایگریشن `AddConsultationPushTypes`، jobهای `ConsultationNotifyPurchases` و `ConsultationEndingSoon`) + نوار سراسری `ConsultationReturnBar` و حالت فقط‌خواندنی/شمارش معکوس در `online-chat/[sessionId].vue` | **انجام شد** (اعلان‌ها تست واقعی؛ UI با داده‌ی ساختگی) |
| ۵ | UI وب‌اپ: تعریف پکیج (`/companionProfile/consultationPackages`)، خرید (`/consultations/buy?companionId=`، ورودی از صفحه‌ی کلینیک)، «مشاوره‌های من» (`/consultations`)، مدیریت نماینده (`/consultations/manage`)، نوار بازگشت؛ پروکسی `webapp/app/server/api/consultation/[...path].ts` | ✔ انجام شد |
| ۶ | پنل ادمین (فقط خواندن): `GET /api/Admin/ConsultationPurchase` (جستجو + `{id}`) و `GET /api/Admin/ConsultationPackage?q=` (همه‌ی کلینیک‌های فعال)، `GET /{companionId}` (ماتریس ۸ خانه) و `PUT /{companionId}` (تعریف/ویرایش/فعال‌وغیرفعال توسط ادمین؛ همان قوانین ذخیره‌ی نماینده)؛ ثبت در `AdminPermissionCatalog` (گروه «مدیریت نمایندگان»)؛ صفحه‌های پنل `/admin/consultationpurchase` و `/admin/consultationpackage` + مستندات + چک‌لیست دیپلوی (بند ۱۲) | ✔ انجام شد |

**فاز ۲ — API:** `POST /api/EndUser/ConsultationPurchase` (packageId, fromWallet, merchantId?, rebateCode? — قیمت فقط از سرور)، `GET /api/EndUser/ConsultationPurchase` و `/{id}`، `PUT /api/EndUser/ConsultationPurchase/{id}/Cancel`. مایگریشن `AddConsultationPurchasePayment` (ستون کیف پول + دو کد). اسکریپت مجموع: `backend/scripts/AddConsultationPackages.sql`.

**فاز ۱ — فایل‌های اجرایی:** مایگریشن `AddConsultationPackages` (اسکریپت SQL: `backend/scripts/AddConsultationPackages.sql`)، داده‌ی اولیه `backend/scripts/Seed-ConsultationPackages.sql` (بعد از مایگریشن اجرا شود؛ idempotent و تراکنشی).

## ۱۱) پرسش‌های باز
همه‌ی پرسش‌های قبلی پاسخ داده شد (بند ۱). موردی باز نیست؛ قیمت اولیه‌ی seed صفر و پکیج‌ها غیرفعال است.

## ۱۲) چک‌لیست دیپلوی (به‌ترتیب؛ هیچ‌کدام از طرف ابزار خودکار اجرا نمی‌شود)

۱. **پشتیبان‌گیری** از دیتابیس اصلی (`backend/scripts/Backup-Database.ps1`).
۲. **مایگریشن‌ها** (خودتان `update-database` یا اسکریپت مجموع): `backend/scripts/AddConsultationPackages.sql` (idempotent؛ از `AddOnlineSessionPhoneCallPush` تا `AddConsultationPushTypes`). اگر جلسه‌های چت/تماس قبلاً روی سرور نیستند، اول `AddOnlineSessionImageAndCallPush.sql`.
۳. **بررسی کدها**: در جدول `Codes` باید `PaymentType_ConsultationPurchase` (پرداخت) و `RebateType_ConsultationPurchase` (تخفیف) وجود داشته باشند و در `PushTypes` شناسه‌های ۶۶ تا ۷۴.
۴. **دیپلوی بک‌اند** (Api؛ Hangfire چهار job دقیقه‌ای را خودش ثبت می‌کند: `ConsultationExpireOverdue`، `ConsultationCompleteExpired`، `ConsultationNotifyPurchases`، `ConsultationEndingSoon`). سرویس Payment هم باید با همین نسخه‌ی `Application` به‌روز شود (کال‌بک پرداخت `ConsultationPurchase`). قبل از commit خروجی‌های `publish-*`: `backend/scripts/Test-NoTrackedSecrets.ps1`.
۵. **داده‌ی اولیه**: `backend/scripts/Seed-ConsultationPackages.sql` (بعد از مایگریشن؛ برای هر کلینیک فعال ۸ بسته‌ی غیرفعال با قیمت ۰ و در صورت نبودن، `CompanionAssistance` خدمت ۱۵).
۶. **همگام‌سازی دسترسی‌های ادمین**: در پنل «همگام‌سازی دسترسی‌ها» را بزنید تا `ConsultationPurchase` و `ConsultationPackage` (ناحیه‌ی Admin) ساخته شوند؛ سپس برای نقش‌های غیر ادمین (مثلاً پشتیبانی) در صورت نیاز دسترسی بدهید. منو فقط هنگام بارگذاری پنل خوانده می‌شود (خروج/ورود دوباره). اگر آیتم منو ظاهر نشد، مثل `scripts/database/EnsureMissingProductMenu.sql` ردیف «لنگر» را `IsMenu=1` کنید.
۷. **دیپلوی پنل و وب‌اپ** (`panel/output.zip`، `webapp/output.zip`).
۸. **دود‌آزمایی روی سرور** (بدون پول واقعی): کلینیک آزمایشی → در وب‌اپ بسته‌ای فعال با قیمت کم بسازید → با کاربر دیگر بخرید (کیف پول) → نماینده «شروع مشاوره» را بزند → پوش ۷۱/۷۲ و نوار بازگشت را ببینید → پایان پنجره: چت فقط‌خواندنی و تماس بسته شود → خرید دوم را لغو کنید و بازپرداخت را در کیف پول ببینید.

**تست‌نشده تا اینجا** (نیاز به آزمون روی سرور): پرداخت با درگاه بانکی و کال‌بک، کد تخفیف واقعی، رسانه‌ی واقعی صدا/تصویر (نیاز به TURN برای شبکه‌های متفاوت)، تحویل واقعی پوش (FCM/وب‌پوش).

