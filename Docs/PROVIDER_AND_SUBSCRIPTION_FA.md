# ارائه‌دهنده (Provider) و اشتراک — طراحی و وضعیت (۱۴۰۵/۰۷ — فاز ۱)

> وضعیت: کد backend و پنل نوشته شده و build/تست‌ها سبز هستند. **مایگریشن `AddProviders` اعمال نشده است** (قبل از Deploy باید `scripts/AddProviders.sql` روی دیتابیس اجرا شود).

## ایده

- **کاربر (User)** همان حساب سایت است. «نقش‌ها» (Role) فقط برای کنترل ادمین/پنل است.
- **ارائه‌دهنده (Provider)** = کاربری که صاحب یک یا چند کسب‌وکار است (قبلاً «نماینده»). `User.IsProvider` فلگ سریع است و جزئیات در جدول `Providers` (۱ به ۱ با User).
- هر ارائه‌دهنده می‌تواند چند **نقش** داشته باشد. نقش‌ها یک کاتالوگ هم‌شکل جدول `Role` هستند (`ProviderRole`: فروشگاه `Store`، کلینیک `Clinic`، مربی `Coach`، آرایشگاه `Groomer`، پانسیون `Pansion`، مدرسه `School`) و هر نقشِ داده‌شده یک `ProviderRoleAssignment` است که تأیید جدا دارد.
- هر ارائه‌دهنده چند **نمایندگی** دارد: `Store` (فروشگاه) و `Companion` (کلینیک/آرایشگاه/مربی؛ `Pansion` و `School` از طریق Companion). هر دو یک `ProviderId` دارند.
- **پرسنل** همان `CompanionUser` (برای Companion) و جدول `StoreUser` (برای Store) هستند؛ صاحب = Provider، بقیه پرسنلِ آن نمایندگی.
- **اشتراک** (نقره‌ای ‹ طلایی ‹ الماس) روی سطح Provider است، نه Companion. فعلاً فقط وجود دارد؛ مزایا بعداً تعریف می‌شود.

## جداول

| جدول | توضیح |
|---|---|
| `Providers` | `UserId` (یکتا بین غیرحذف‌شده‌ها)، `Active`، `Approved`، `Note`، `Deleted` |
| `ProviderRoles` | کاتالوگ نقش‌ها: `Name` (فارسی، قابل ویرایش)، `Label` (کلید ثابت کد، یکتا)، `Active` — seed شش نقش در مایگریشن `AddProviderRoles` |
| `ProviderRoleAssignments` | `ProviderId`، `ProviderRoleId`، `Approved`؛ یکتا روی (ProviderId, ProviderRoleId) بین غیرحذف‌شده‌ها |
| `SubscriptionPlans` | `Name`، `Level`، `Active` — seed: نقره‌ای=۱، طلایی=۲، الماس=۳ |
| `ProviderSubscriptions` | تاریخچه: `ProviderId`، `PlanId`، `StartDate`، `EndDate`، `CreatedByUserId`، `Note` |
| `Companions.ProviderId` / `Stores.ProviderId` | ستون‌های جدید (nullable) |
| `Users.IsProvider` | ستون جدید |

اشتراک جاری = دوره‌ای که `StartDate <= الان < EndDate`؛ اگر چند تا باشد، بالاترین `Level`. هنگام اختصاص اشتراک جدید، دوره‌ی هم‌پوشان قبلی در لحظه‌ی شروع جدید بسته می‌شود (یا اگر کامل بعد از آن شروع می‌شده، حذف نرم می‌شود).

## API (ناحیه Admin، دسته «مدیریت نمایندگان»)

- `GET/POST/PUT/DELETE /api/Admin/Provider` — `GET /{id}` خروجی کامل (کاربر، نقش‌ها `roleAssignments`، اشتراک جاری، تعداد نمایندگی‌ها).
- `.../ProviderRole` — CRUD کاتالوگ نقش‌ها (ویرایش فقط نام و فعال بودن؛ نقشِ داده‌شده حذف نمی‌شود).
- `.../ProviderRoleAssignment` — دادن/تأیید/برداشتن یک نقش به ارائه‌دهنده.
- `.../SubscriptionPlan` — CRUD پلن‌ها.
- `.../ProviderSubscription` — `GET` تاریخچه، `GET current/{providerId}`، `POST` اختصاص، `PUT cancel/{id}` پایان فوری، `DELETE`.
- `.../ProviderOverview` — `GET {providerId}`: خلاصه‌ی صفحه‌ی جزئیات: کیف پول، حسابداری هر نمایندگی (سهم ارائه‌دهنده/سایت، تسویه‌شده/مانده، پرداخت‌شده)، کارت‌های بانکی، ۱۰ تسویه‌ی آخر و آمار نمایندگی‌ها. فقط می‌خواند؛ عددها از همان `FinanceCompanion`/`FinanceStore` موجود می‌آیند.
- `.../Provider?ProviderRoleId=` — فیلتر ارائه‌دهندگان بر اساس نقش (کنار `PlanId`, `Active`, `Approved`).
- `.../ProviderVenue` — `GET {providerId}` فهرست نمایندگی‌ها، `PUT companion` و `PUT store` اتصال دستی.

پیام‌های جدید در `Resource/Notification*.resx`: `ProviderNotFound`، `ProviderAlreadyExistsForUser`، `SubscriptionPlanNotFound`، `SubscriptionPeriodInvalid`.

## ساخت خودکار Provider

- ثبت Companion (`CompanionService.InsertAsyncDto`) و درخواست فروشگاه توسط کاربر (`StoreService`) → `IProviderService.EnsureForUserAsync(userId, approved:false)`؛ Provider در همان `SaveChanges` ساخته و وصل می‌شود.
- ساخت فروشگاه توسط ادمین مالک ندارد (`ProviderId = null`)؛ از پنل «ارائه‌دهندگان ← نمایندگی‌ها» وصل می‌شود.

## مهاجرت داده (داخل مایگریشن)

1. هر `Companion.OwnerId` (غیرحذف‌شده) و هر «صاحب» فروشگاه ← یک Provider.
2. صاحب فروشگاه = **کوچک‌ترین `UsersId`** در `StoreUser` (فروشگاه چندکاربره را بعد از اجرا بازبینی کنید؛ کوئری انتهای `scripts/AddProviders.sql`).
3. نقش‌ها (مایگریشن دوم `AddProviderRoles`): تخصص‌های قبلی تبدیل شدند — فروشنده→Store، آرایشگر→Groomer، مربی→Coach، دامپزشک→Clinic، پانسیون‌دار→Pansion — و ارائه‌دهندگانِ دارای مدرسه نقش School گرفتند. نقش Clinic/Coach/Groomer برای ارائه‌دهندگانِ موجود از روی `CompanionType` **خودکار نگاشت نشده**؛ ادمین از صفحه ارائه‌دهنده می‌دهد.
4. اشتراک قدیمی Gold/Silver هر Companion ← اشتراک مالک: بالاترین سطحِ هنوز-معتبر با دیرترین انقضا.

## فاز ۲ (انجام‌شده)

- **قید «یک Companion برای هر مالک» برداشته شد** (ثبت و ویرایش در `CompanionService`). یک Provider می‌تواند چند کلینیک/آرایشگاه داشته باشد. جاهایی که مالک را با `OwnerId` پیدا می‌کنند (`FindClinicAsync` در OnlineSession، `AllMap` برای `CompanionId` کاربر و ...) اکنون اولین نمایندگی فعال را برمی‌دارند؛ اپ‌های فعلی همان رفتار قبلی را برای مالکِ تک‌نمایندگیه می‌بینند و برای چندنمایندگیه باید در اپ انتخاب نمایندگی اضافه شود.
- **اشتراک تنها منبع حقیقت روی Provider است.** `IProviderSubscriptionSyncService.SyncProviderAsync` سطح جاری را (طلایی و الماس ← `GoldAccountDate`، نقره‌ای ← `SilverAccountDate`) روی همه‌ی Companionهای Provider می‌نویسد؛ پس جستجو، مرتب‌سازی، badge و اپ‌ها بدون تغییر کار می‌کنند. فراخوانی: اختصاص/پایان/حذف اشتراک، اتصال Companion به Provider، ثبت Companion جدید. قاعده‌ی خالص: `ProviderSubscriptionRules.ToLegacyAccountDates` (تست دارد).
- endpointهای قدیمی `CompanionUpdateGoldAccount/SilverAccount` فقط پیام «از بخش اشتراک ارائه‌دهنده مدیریت می‌شود» برمی‌گردانند. محدودیت قدیمیِ «فقط یک Companion طلایی» دیگر وجود ندارد.
- پنل: دکمه‌های طلایی/نقره‌ای لیست نماینده‌ها به صفحه‌ی ارائه‌دهنده می‌روند؛ `CompanionAccountModal` حذف شد. `providerId` در خروجی Companion آمده است.

## هنوز انجام نشده (فاز ۳)

- بعد از اطمینان، حذف ستون‌های `GoldAccountDate/SilverAccountDate/SilverAccountCreateDate` و فیلدهای مرتبط DTO (فعلاً cache مشتق‌شده‌اند).
- مهاجرت چک‌های `OwnerId` به guard مرکزی مبتنی بر `ProviderId`، و انتخاب نمایندگی در اپ‌ها برای مالک چندنمایندگیه.
- تغییر نام «نماینده» → «ارائه‌دهنده» در متن‌های UI و webapp؛ مزایای هر پلن؛ نگاشت `CompanionType` به نقش‌ها.
- انقضای طبیعی اشتراک نیازی به job ندارد (تاریخ پایان خودش مقایسه می‌شود)، اما `ProviderSubscription` منقضی‌شده تا اولین Sync بعدی روی Companion باقی می‌ماند و در مقایسه‌ها اثری ندارد.

## پنل

- `/admin/provider` فهرست + افزودن (جستجوی کاربر)، `/admin/provider/detail-{id}` وضعیت، نقش‌ها، اشتراک (اختصاص/پایان/تاریخچه) و نمایندگی‌ها، `/admin/subscriptionplan` پلن‌ها، `/admin/providerrole` کاتالوگ نقش‌ها (همان UI صفحه نقش‌های کاربری). صفحه‌ی جزئیات چهار تب دارد: نمای کلی، حسابداری و کارت‌ها، نمایندگی‌ها، اشتراک. فروشگاه‌ها در منو زیر «مدیریت نمایندگان» هستند.
- store‌ها: `useProviderStore`، `useSubscriptionPlanStore`.
- منوی کناری از `AdminPermissionCatalog` می‌آید؛ بعد از Deploy، دسترسی بخش‌های جدید باید به نقش‌ها داده شود.

## حسابداری و تسویه‌ی ارائه‌دهنده‌محور (۱۴۰۵/۰۷/۲۷)

> **مایگریشن `ProviderSettlements` + `scripts/ProviderSettlements.sql` باید قبل از Deploy این Api روی دیتابیس اجرا شود** (ستون‌های جدید `Settlements.ProviderId/Kind`).

**اصل:** درصدها همچنان روی خدمت، نمایندگی، پانسیون، مشاوره و فروشگاه تنظیم می‌شوند؛ اما **حسابداری و تسویه برای ارائه‌دهنده** است.

- **دو استخر جدا:** هر ارائه‌دهنده یک مانده برای «نمایندگی‌ها» (`Kind=1`: رزرو خدمت + پانسیون + مشاوره‌ی تکمیل‌شده + دوره‌ی مدرسه‌ی همه‌ی نمایندگی‌هایش) و یک مانده برای «فروشگاه‌ها» (`Kind=2`: سفارش‌های همه‌ی فروشگاه‌هایش) دارد. هر تسویه فقط یک استخر را می‌پردازد، با یک مبلغ و یک کارت.
- **تعریف یگانه‌ی ردیف قابل تسویه:** `SettlementSources` (هم ثبت تسویه هم محاسبه‌ی مانده از آن می‌خوانند، پس عدد پنل = عدد پرداخت‌شده).
- **ثبت تسویه** (`POST /api/Admin/Settlement` با `{providerId, kind, userBankCardId, trackingCode}`):
  - ثبت با `storeId/companionId` رد می‌شود (پیام «تسویه فقط برای ارائه‌دهنده ثبت می‌شود»).
  - کارت باید **تأییدشده و متعلق به خودِ ارائه‌دهنده** باشد.
  - همه‌چیز در یک تراکنش Serializable: هر ردیف با یک `UPDATE ... WHERE Permitted=0` اتمی علامت می‌خورد؛ اگر حتی یک ردیف بین خواندن و نوشتن تغییر کند (لغو یا تسویه‌ی هم‌زمان) کل تسویه برمی‌گردد و چیزی ثبت نمی‌شود. (قبلاً فقط مدرسه و مشاوره اتمی بودند و بازگردانی ناقص بود.)
  - تسویه‌ی بدون هیچ ردیف رد می‌شود (قبلاً تسویه‌ی ۰ تومانی ثبت می‌شد).
  - نمایندگیِ حذف‌شده هم در استخر هست: پول ثبت‌شده‌اش هنوز بدهی به ارائه‌دهنده است.
- **خواندن:** `GET /api/Admin/ProviderFinance` (لیست + هشدار نمایندگی/فروشگاه‌های بدون ارائه‌دهنده)، `GET .../{providerId}` (جمع هر استخر + تفکیک هر نمایندگی/فروشگاه + کارت‌ها)، `GET .../{providerId}/pending?kind=` (پیش‌نمایش دقیق ردیف‌هایی که تسویه می‌پردازد). سوابق: `GET /api/Admin/Settlement?ProviderId=&Kind=`.
- **هر نمایندگی/فروشگاه باید ارائه‌دهنده داشته باشد:** ساخت فروشگاه توسط ادمین بدون `providerId` رد می‌شود؛ نمایندگیِ بدون ارائه‌دهنده در ویرایش از مالک ساخته می‌شود؛ بقیه در لیست حسابداری با هشدار نشان داده می‌شوند و تا وصل نشوند تسویه نمی‌شوند. (ستون `ProviderId` عمداً هنوز nullable است؛ بعد از پاک‌شدن هشدارها می‌توان NOT NULL کرد.)
- **سوابق قدیمی:** مایگریشن `Kind` و `ProviderId` ردیف‌های قبلی را پر می‌کند؛ `StoreId/CompanionId` قدیمی دست‌نخورده می‌ماند.
- **اپ نماینده / فروشنده:** `GET /Companion/Settlement` و `GET /Seller/Settlement` اکنون تسویه‌های ارائه‌دهنده‌ی همان نمایندگی/فروشگاه و همان استخر را برمی‌گردانند (پارامتر `StoreId/CompanionId/ProviderId` کلاینت نادیده گرفته می‌شود). باگ قدیمیِ فروشنده (`CompanionId = StoreId`) برطرف شد. گزارش `FinanceCompanion` (ریز رزروها) و `FinanceStore` بدون تغییر و همچنان به‌ازای همان نمایندگی/فروشگاه است.
- **محدودیت شناخته‌شده‌ی قدیمی (دست نخورد):** سهم سفارش فروشگاه در سطح خودِ سفارش ذخیره می‌شود (`ProductOrder.StoreShare` جمع همه‌ی فروشگاه‌های سفارش). سفارشی که فروشگاه‌های چند ارائه‌دهنده دارد، کل سهمش به اولین تسویه می‌رسد؛ حسابداری آن را با «سفارش چندارائه‌دهنده‌ای» هشدار می‌دهد. تقسیم دقیق نیازمند ذخیره‌ی سهم و `Permitted` روی `ProductOrderStore` است.
- **پنل:** «حسابداری ارائه‌دهندگان» (`/admin/providerfinance`: لیست، `detail-{id}` با دو استخر، `settle-{id}?kind=` با پیش‌نمایش و تأیید). دکمه‌های «ثبت تسویه/سوابق» از صفحه‌های مالی نمایندگی و فروشگاه به این صفحه منتقل شدند؛ درصدها همان‌جا (`/admin/finance`) می‌مانند. `finance/settlement/new` فقط به صفحه‌ی ارائه‌دهنده هدایت می‌کند.

## لیست یکپارچه‌ی نمایندگی‌ها (Agency)

- `GET /api/Admin/Agency` (مجوز «Agency» در بخش Companion): فروشگاه‌ها و نمایندگی‌ها (Companion) را پیش‌فرض با هم برمی‌گرداند (UNION ALL، صفحه‌بندی و مرتب‌سازی سمت سرور). هر ردیف `kind` دارد: 1 = Companion، 2 = Store.
- فیلتر نقش ارائه‌دهنده در پنل نقش «نوع» را تعیین می‌کند: بدون نقش = همه، نقش Store = فقط فروشگاه‌ها، نقش دیگر = نمایندگی‌های ارائه‌دهنده‌هایی با آن نقش.
- فیلترهای مخصوص Companion (شخصی، طلایی، نقره‌ای) فروشگاه‌ها را از نتیجه حذف می‌کنند.
- ردیف فروشگاه: منوی سه‌نقطه‌ی `StoreRowActions` (جزئیات، ویرایش، پرسنل، نظرات، مالی، حذف)؛ ردیف نمایندگی: `CompanionActionsDropdown`.
- کد: `Application/Services/AgencySrvs/AgencySrv/*`، تست ترجمه‌ی SQL در `ProviderFinanceQueryTranslationTests`.
- بعد از دیپلوی Api باید Permission Sync زده شود و مجوز «Agency» به نقش‌ها داده شود.

### پانسیون و مدرسه در لیست نمایندگی‌ها
- `kind` 3 = پانسیون، 4 = مدرسه (از مسیر Companion صاحب‌شان ارائه‌دهنده و مالک می‌گیرند). منوی «پانسیون‌ها» و «مدرسه‌ها» از سایدبار برداشته شد (صفحه‌ها با URL باز می‌مانند).
- ردیف‌ها منوی همان صفحه‌ی خودشان را دارند (`PansionRowActions` / `SchoolRowActions`). دکمه‌ی «افزودن نمایندگی» مودال ۶ نوع را باز می‌کند؛ آرایشگاه/کلینیک/مربی فعلاً به فرم Companion می‌روند تا جدا شوند.
- پانسیون/مدرسه‌ی حذف‌شده در این لیست نیست (فیلتر حذف‌شده‌ها فقط در صفحه‌ی خودشان بود).
