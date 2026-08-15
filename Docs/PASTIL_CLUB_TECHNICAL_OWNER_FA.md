# راهنمای فنی مالک پروژه — پاستیل کلاب، پنل و سیستم معرف

این سند برای نگهداری پروژه نوشته شده و توضیح می‌دهد اجزای پاستیل کلاب در بک و پنل چه کاری انجام می‌دهند، داده‌ها کجا ذخیره می‌شوند و هنگام توسعه باید کدام مرزها حفظ شوند.

## 1. تصویر کلی معماری

پاستیل کلاب از سیستم قدیمی `ClubReward` مستقل است. مدل‌های جدید در Namespace زیر قرار دارند:

```text
Entities.Entities.PastilClubField
```

جریان اصلی:

```text
رویداد کسب‌وکار
  -> ClubPointIntegrationService
  -> ClubPointEventService
  -> ClubPointRule فعال
  -> ClubPointService
  -> ClubPointAccount + ClubPointTransaction

ادمین/اتوماسیون
  -> ClubRewardTemplate
  -> ClubRewardOffer
  -> تأیید
  -> Redeem توسط کاربر
  -> کسر امتیاز + ClubRewardRedemption
  -> ClubRewardBenefitFactory
  -> Coupon / FreeDelivery / PromotionalCredit / PastilAI
```

Controllerها فقط قرارداد HTTP را مدیریت می‌کنند. منطق امتیاز، صلاحیت، Redeem و ساخت مزیت در Serviceهاست.

## 2. محل فایل‌های اصلی بک

```text
Entities/Entities/PastilClubField/
Application/Services/PastilClubSrvs/
Api/Areas/Admin/Controllers/PastilClub*.cs
Api/Areas/EndUser/Controllers/PastilClub*.cs
Persistence/Context/DataBaseContext.cs
Persistence/Interface/IDataBaseContext.cs
Application/Configures/ConfigureServices.cs
```

تقسیم سرویس‌ها:

| پوشه | مسئولیت |
|---|---|
| `PointSrv` | حساب امتیاز، Ledger، افزایش/کاهش/برگشت و جستجو |
| `PointEventSrv` | تبدیل رویداد کسب‌وکار به Rule و تراکنش امتیاز |
| `PetProfileSrv` | تشخیص کامل شدن پروفایل پت |
| `RewardTemplateSrv` | CRUD قالب جایزه و Targetها |
| `RewardOfferSrv` | ساخت پیشنهاد دستی/خودکار، تأیید/رد و صلاحیت |
| `RewardRedemptionSrv` | دریافت جایزه و کسر اتمیک امتیاز |
| `BenefitSrv` | ساخت مزیت واقعی و نمایش کیف مزایا |

## 3. جدول‌های دیتابیس پاستیل کلاب

### حساب و Ledger

| جدول | نقش |
|---|---|
| `ClubPointAccounts` | مانده فعلی و آمار طول عمر هر User |
| `ClubPointRules` | مقدار امتیاز و محدودیت هر EventType |
| `ClubPointTransactions` | Ledger تغییرناپذیر تمام تغییرات امتیاز |

هر User حداکثر یک PointAccount دارد. مانده و بدهی منفی نمی‌شوند. `RowVersion` برای کنترل هم‌زمانی وجود دارد.

`ClubPointTransactions` نباید ویرایش یا حذف شود. اصلاح حساب همیشه با تراکنش جدید انجام می‌شود. `IdempotencyKey` Unique است و Retry را امن می‌کند.

### قالب و جایزه

| جدول | نقش |
|---|---|
| `ClubRewardTemplates` | تعریف قابل استفاده مجدد جایزه |
| `ClubRewardTargets` | دامنه مصرف قالب |
| `ClubRewardPetTypes` | محدودیت قالب به نوع پت |
| `ClubRewardPastilAITargets` | تنظیمات پلن/Upgrade مزیت AI |
| `ClubRewardOffers` | پیشنهاد Snapshot شده برای یک User |
| `ClubRewardRedemptions` | سابقه دریافت و کسر امتیاز |

Offer اطلاعات مهم مانند هزینه امتیاز و تاریخ انقضا را Snapshot می‌کند تا تغییر آینده Template قرارداد پیشنهاد قبلی را خراب نکند.

### مزیت‌های واقعی

| جدول | نقش |
|---|---|
| `ClubCoupons` | اتصال Redemption به `Rebate` شخصی |
| `ClubFreeDeliveryBenefits` | ارسال رایگان دارای Scope و تعداد مصرف |
| `ClubPromotionalWalletCredits` | اعتبار تشویقی غیرقابل برداشت |
| `ClubPromotionalCreditUsages` | Ledger مصرف اعتبار تشویقی |
| `ClubRewardCostTransactions` | هزینه تأمین‌شده توسط پاستیل |

مزیت PastilAI داخل ساختار Subscription موجود ساخته می‌شود و `ClubRewardRedemptionId` را نگه می‌دارد.

## 4. Migrationها

Migrationهای اصلی به ترتیب:

```text
20260810183310_AddPastilClubFoundation
20260810184856_AddPastilClubPointIntegrations
20260810193641_AddPastilClubRewardCore
20260810195933_AddPastilClubBenefitEngine
20260810205359_SecurePaymentWalletRebateAndFinancialIntegrity
20260811005513_AddRegistrationReferralAttribution
```

Migration امنیت مالی نیز روی موجودیت‌های Club اثر Snapshot دارد و قبل از Migration انتساب ثبت‌نام اجرا می‌شود.

در محیط مقصد همه Migrationها باید اعمال شده باشند:

```powershell
dotnet ef database update --project Persistence --startup-project Api
```

یا در Package Manager Console:

```powershell
Update-Database
```

قبل از انتشار:

```powershell
dotnet ef migrations has-pending-model-changes --project Persistence --startup-project Api
```

## 5. موتور امتیاز

### ClubPointRuleService

قانون هر EventType را ایجاد یا ویرایش می‌کند. مقدار امتیاز Hard-code نشده و از جدول Rule خوانده می‌شود. برای هر EventType فقط یک Rule قابل تعریف است.

فیلدهای مهم Rule:

- `PointAmount`: مقدار امتیاز.
- `DailyLimit`: تعداد مجاز در روز تهران.
- `MonthlyLimit`: تعداد مجاز در ماه تهران.
- `LifetimeLimit`: تعداد مجاز در کل عمر حساب.
- `StartDate` و `EndDate`: بازه فعالیت.
- `Active`: توقف یا فعال‌سازی بدون حذف.

### ClubPointEventService

رویداد را به Rule فعال وصل می‌کند، Source و IdempotencyKey را می‌سازد و Earn را به `ClubPointService` می‌فرستد. Eventهای Referral از نوع تراکنش `ReferralEarn` ثبت می‌شوند.

### ClubPointService

مرجع اصلی تغییر حساب است:

- تراکنش دیتابیس با IsolationLevel.Serializable.
- بررسی Idempotency قبل از تغییر.
- ایجاد PointAccount در اولین امتیاز.
- اعمال Limitهای Rule با timezone تهران.
- ثبت مانده قبل و بعد.
- افزایش آمار Lifetime.

منطق بدهی:

- ReverseEarn اگر موجودی کافی نباشد، موجودی را صفر و باقی‌مانده را Debt می‌کند.
- Earn جدید ابتدا Debt را کم می‌کند.
- Redeem و کاهش دستی با Debt یا موجودی ناکافی انجام نمی‌شوند.

## 6. نقاط اتصال امتیاز به پروژه

اتصالات فعلی:

| دامنه | نتیجه |
|---|---|
| سفارش محصول | تحویل + پرداخت = Earn؛ لغو معتبر = Reverse |
| رزرو نماینده | Complete = Earn؛ Cancel = Reverse |
| رزرو پانسیون | Complete = Earn؛ Cancel = Reverse |
| UserPet | اولین تکمیل پروفایل = Earn |
| Memory | ثبت روزانه = Earn؛ حذف/جابجایی روز = Reverse/Earn |

این اتصال‌ها از طریق `IClubPointIntegrationService` انجام می‌شوند تا سرویس‌های اصلی مستقیم به جزئیات Ledger وابسته نشوند.

خطای موقت پاستیل کلاب نباید خرید یا رزرو اصلی را 500 کند؛ خطا Log می‌شود. برای Production واقعی بهتر است Reconciliation Job برای Eventهای ثبت‌نشده اضافه شود.

## 7. قالب جایزه

`ClubRewardTemplateService` قالب، Targetها، PetTypeها و تنظیم PastilAI را مدیریت می‌کند.

قواعد مهم:

- `Name` نام داخلی و `Title` عنوان نمایشی است.
- `PointCost` باید مثبت باشد.
- `ApplicationMethod` محل مصرف تخفیف را مشخص می‌کند.
- کد تخفیف ساخته‌شده فقط در همان ApplicationMethod معتبر است.
- `Targets` دامنه واقعی مصرف را تعیین می‌کند و هنگام Checkout دوباره بررسی می‌شود.
- `PetTypeIds` در صورت خالی بودن یعنی همه انواع پت.
- `IsManualAllowed` مجوز ساخت Offer دستی است.
- `IsAutomationAllowed` برای موتور اتوماسیون آینده است.
- حذف Template نداریم؛ برای توقف `Active=false` استفاده می‌شود.

### Targetها

```text
1 Global
2 Store
3 Product
4 ProductCategory
5 Companion
6 Assistance
7 CompanionPackage
8 Pansion
9 PastilAI
10 PastilAIPlan
11 City
```

برای FreeDelivery فقط Global، Store و City منطقی و مجازند. برای PastilAI باید `PastilAITarget` نیز مقدار داشته باشد.

## 8. Offer و Approval

`ClubRewardOfferService` مسئول است:

- ساخت Offer دستی برای User.
- کنترل `IsManualAllowed` و فعال بودن Template.
- محاسبه تاریخ انقضا.
- Snapshot اطلاعات Template.
- تأیید و رد تکی یا گروهی.
- محدود کردن لیست EndUser به User داخل توکن.

Offer فقط وقتی برای دریافت معتبر است که:

- Status برابر Approved باشد.
- منقضی نشده باشد.
- Template فعال و داخل بازه باشد.
- نوع پت کاربر با Template سازگار باشد.
- User بدهی نداشته باشد.
- موجودی حداقل برابر PointCost باشد.

`CanRedeem` خروجی همین محاسبه است، ولی Redeem همه شروط را دوباره سمت سرور بررسی می‌کند.

## 9. Redemption و BenefitFactory

`ClubRewardRedemptionService.RedeemAsync` حساس‌ترین مسیر کلاب است:

1. UserId فقط از توکن گرفته می‌شود.
2. Offer با `UPDLOCK, ROWLOCK` و تراکنش Serializable قفل می‌شود.
3. کلید `club-reward-redeem:{offerId}` جلوی Redeem دوباره را می‌گیرد.
4. وضعیت، انقضا، Template، پت، بدهی و موجودی دوباره بررسی می‌شوند.
5. PointAccount قفل و امتیاز کم می‌شود.
6. PointTransaction و Redemption ساخته می‌شوند.
7. BenefitFactory مزیت واقعی را در همان تراکنش می‌سازد.
8. Offer به Redeemed تغییر می‌کند.
9. اگر Benefit شکست بخورد، کل عملیات Rollback می‌شود.

`ClubRewardBenefitFactory` بر اساس RewardType خروجی می‌سازد:

- Fixed/Percentage Discount: یک `Rebate` شخصی با UseCount=1.
- FreeDelivery: یک Benefit دارای Store/City/MaximumAmount.
- PromotionalWalletCredit: اعتبار مستقل از کیف پول نقدی.
- PastilAI Discount: کوپن مخصوص متد PastilAI.
- PastilAI Free/Upgrade: Subscription واقعی.

## 10. اتصال مزایا به پرداخت

چهار جریان مصرف پوشش داده شده‌اند:

```text
ProductOrder
CompanionReservation
PansionReservation
PastilAI
```

کوپن Club به Rebate فعلی تبدیل می‌شود؛ بنابراین کد Checkout موجود همچنان `rebateCode` می‌گیرد.

اعتبار تشویقی با Wallet نقدی یکی نیست. در `fromWallet=true` ترتیب مصرف:

1. اعتبار تشویقی معتبر و منطبق با Scope، با نزدیک‌ترین انقضا.
2. کیف پول نقدی.
3. درگاه برای باقی‌مانده.

مصرف اعتبار با ReferenceKey ثبت می‌شود تا Callback تکراری دوباره از آن کم نکند.

ارسال رایگان در Cart محاسبه و در سفارش Snapshot می‌شود. فرانت نباید قیمت نهایی را خودش بازسازی کند.

## 11. سیستم ثبت‌نام و معرف

### مدل داده User

فیلدهای افزوده‌شده:

```text
ReferralCode
RegistrationReferralSource
UsedReferralCode
ReferredByUserId
ReferredByCompanionId
ReferredByStoreId
```

سه FK به صورت Restrict تعریف شده‌اند تا حذف موجودیت معرف سابقه انتساب را ناخواسته خراب نکند. در هر ثبت‌نام فقط یکی از سه FK باید مقدار داشته باشد.

### SignUpDto

```text
ReferralSource : RegistrationReferralSource
ReferralCode   : string
```

`UserService.ResolveRegistrationReferralAsync` مسئول Normalize و اعتبارسنجی است:

- تبدیل ارقام فارسی به انگلیسی.
- اجباری بودن کد برای Clinic/PetShop/Acquaintances.
- کنترل عددی و طول.
- Query فقط روی موجودیت فعال و معتبر.
- جلوگیری از کد خود کاربر با Mobile/Email.
- جلوگیری از کد Companion مالک.
- جلوگیری از کد Store برای Userهای متصل به همان Store.
- ذخیره کد مصرف‌شده و FK صحیح.

SocialMedia و Other کد ارسالی را نادیده می‌گیرند.

### تولید کد

کلاس:

```text
Application/Common/Helpers/ReferralCodeGenerator.cs
```

قواعد:

- User: هفت رقم تصادفی امن.
- Companion: ده رقم با Prefix `1`.
- Store: ده رقم با Prefix `2`.
- بررسی هم‌زمان هر سه جدول قبل از بازگرداندن کد.
- حداکثر 50 تلاش و خطای صریح در صورت عدم امکان تولید.
- Unique Index دیتابیس لایه نهایی جلوگیری از تکرار است.

در Mappingهای Update، فیلد `ReferralCode` Ignore شده تا ادمین یا کلاینت نتواند آن را بازنویسی کند.

Migration برای Companion و Storeهای قبلی نیز Backfill انجام می‌دهد.

### محدودیت امنیتی قابل توجه

Self-referral بر اساس Mobile و Email کنترل می‌شود. اگر یک شخص با موبایل و ایمیل کاملاً متفاوت ثبت‌نام کند، بدون NaturalCode/KYC امکان تشخیص قطعی نیست. اگر امتیاز Referral ارزش مالی بالا پیدا کرد، قبل از فعال‌سازی Ruleهای 6 تا 8 باید KYC، محدودیت دستگاه/IP و Anti-Fraud اضافه شود.

### وضعیت فعلی امتیاز Referral

Entity، Enum و EventTypeهای زیر آماده‌اند:

```text
UserReferralReferrer = 6
UserReferralReferee = 7
BusinessReferralUser = 8
```

اما `UserService.SignUp` فعلاً فقط Attribution را ذخیره می‌کند و `IClubPointIntegrationService` را برای این سه Event صدا نمی‌زند. بنابراین Ruleهای 6 تا 8 را تا زمان اتصال کامل و تست Anti-Fraud فعال نکنید.

## 12. Companion، Store و Pansion

فیلد `ReferralCode` به این موجودیت‌ها اضافه شده است:

```text
Companion
Store
```

Pansion زیرمجموعه Companion است و کد مستقل ندارد.

هنگام Insert:

- `CompanionService` کد Companion را تولید می‌کند.
- `StoreService` کد Store را تولید می‌کند.
- مقدار ورودی DTO نادیده گرفته می‌شود.

DTOهای نمایش فعلی کد را برمی‌گردانند تا پنل بتواند آن را Read-only نشان دهد. اگر قرار نیست کد در API عمومی سایت دیده شود، در نسخه بعد بهتر است DTO عمومی Site از DTO Admin جدا شود و `ReferralCode` فقط در Admin/Owner DTO باقی بماند.

## 13. Permissionها

والد:

```text
PastilClubManagement
```

فرزندان اصلی:

```text
PastilClubPointRule
PastilClubPointTransaction
PastilClubPointIncrease
PastilClubPointDecrease
PastilClubRewardTemplate
PastilClubRewardOffer
PastilClubRewardApprove
PastilClubRewardReject
PastilClubRewardBulkApprove
PastilClubRewardBulkReject
PastilClubRewardRedemption
Rebate
```

`Rebate` همان مدیریت کدهای تخفیف موجود پروژه است که به دلیل استفاده موتور جوایز زیر والد پاستیل کلاب قرار گرفته و حذف نشده است. موارد عملیاتی مانند Increase/Decrease/Approve/Reject/Bulk برای Action هستند و نباید آیتم مستقل منو باشند. صفحات اصلی Rule، Transaction، Template، Offer، Redemption و Rebate منویی‌اند.

بعد از انتشار Controller یا Permission جدید:

```http
POST /api/Admin/PermissionSync
```

سپس Permissionها به Role ادمین موردنظر داده شوند.

## 14. معماری پنل

مسیر پروژه پنل:

```text
D:/WorkSpace/Projects/Pastil/admin-panel-site
```

فایل مرکزی Pinia:

```text
store/usePastilClubStore.ts
```

این Store پنج نوع لیست را مدیریت می‌کند:

```text
rules
transactions
templates
offers
redemptions
```

وظایف Store:

- نگهداری List، Total و Loading.
- حذف Queryهای خالی قبل از درخواست.
- Unwrap پاسخ جزئیات.
- POST/PUT مشترک برای Rule و Template.
- ساخت Offer دستی.
- متد تصمیم Offer برای Approve/Reject.

Utility enum و تاریخ:

```text
utils/pastilClub.ts
```

Componentهای مشترک:

```text
components/pastil-club/ClubPageHeader.vue
components/pastil-club/ClubPagination.vue
components/pastil-club/ClubDetailModal.vue
components/pastil-club/PointRuleForm.vue
components/pastil-club/RewardTemplateForm.vue
```

صفحات:

```text
pages/admin/pastilclubpointrule/
pages/admin/pastilclubpointtransaction/
pages/admin/pastilclubrewardtemplate/
pages/admin/pastilclubrewardoffer/
pages/admin/pastilclubrewardredemption/
```

### کار هر صفحه

`pastilclubpointrule`:

- فهرست Ruleها، جستجو و فیلتر Event.
- صفحه Create و Edit با Component مشترک.
- Delete ندارد.

`pastilclubpointtransaction`:

- فیلتر User، TransactionType، SourceType و بازه تاریخ.
- آمار افزایش/کاهش صفحه.
- Modal جزئیات Ledger.

`pastilclubrewardtemplate`:

- کارت‌های قالب جایزه.
- فرم کامل Target، PetType، Picture، انقضا و PastilAI.
- جزئیات و ویرایش.

`pastilclubrewardoffer`:

- فهرست و فیلتر Offerها.
- صفحه ساخت Offer دستی با UserSearchSelect و Template.
- Store متد Approve/Reject دارد؛ هنگام اضافه کردن دکمه تصمیم، Permission و وضعیت Pending حتماً کنترل شود.

`pastilclubrewardredemption`:

- گزارش Read-only دریافت‌ها.
- فیلتر User، Status، RewardType و تاریخ.
- نمایش PointSpent، RemainingPoint و BenefitReferenceId.

## 15. نمایش کد معرف در پنل

کد کاربر در این بخش‌ها نمایش داده شده است:

```text
components/user/UserForm.vue
pages/admin/user/profile-[id].vue
```

کد مالک Companion در صفحه جزئیات نماینده نمایش داده می‌شود:

```text
pages/admin/companion/detail-[id].vue
```

Typeهای Store و Companion در Storeهای Pinia فیلد `referralCode` را دارند. اصل مهم این است که این فیلد فقط نمایشی باشد و داخل Payload ویرایش به عنوان مقدار قابل تغییر در نظر گرفته نشود؛ بک نیز آن را Ignore می‌کند.

## 16. نکات نگهداری و خط قرمزها

- هیچ Controller نباید مقدار امتیاز را Hard-code کند.
- هیچ تغییر PointAccount بدون ClubPointTransaction انجام نشود.
- Ledger حذف یا ویرایش نشود.
- Retry عملیات مالی باید IdempotencyKey ثابت داشته باشد.
- UserId در EndUser فقط از Token گرفته شود.
- مبلغ، Scope و انقضای Benefit در Checkout دوباره سمت بک کنترل شود.
- فرانت منبع محاسبه قیمت یا موجودی نباشد.
- Coupon پاستیل کلاب نباید خارج از ApplicationMethod خود پذیرفته شود.
- ReferralCode از ورودی Create/Update موجودیت پذیرفته نشود.
- برای فعال‌سازی امتیاز Referral ابتدا Anti-Fraud تکمیل شود.
- API عمومی Site و DTO ادمین برای اطلاعات حساس در آینده از هم جدا شوند.

## 17. تست‌های ضروری قبل از انتشار

### امتیاز

- Earn عادی.
- Earn با Debt و تسویه جزئی/کامل.
- Daily/Monthly/Lifetime limit.
- Retry یک Event بدون امتیاز تکراری.
- Reverse با موجودی کافی و ناکافی.
- افزایش و کاهش دستی با RequestId تکراری.

### جایزه

- Offer دستی با Template مجاز/غیرمجاز.
- Approve و Reject تکی و گروهی.
- Redeem با موجودی کافی.
- Redeem با Debt، انقضا، پت نامعتبر و موجودی کم.
- کلیک هم‌زمان Redeem و اثبات یک Redemption.
- شکست Benefit و Rollback امتیاز.

### مزایا و پرداخت

- Coupon در متد درست و رد در متد اشتباه.
- سقف تخفیف درصدی.
- FreeDelivery بر اساس Store/City.
- PromotionalCredit + Wallet + Gateway.
- فقط PromotionalCredit.
- Callback تکراری.
- PastilAI FreeDays و Upgrade.

### ثبت‌نام

- هر پنج ReferralSource.
- کد خالی، غیرعددی و طول اشتباه.
- کد User/Companion/Store معتبر.
- کد نوع اشتباه.
- کد خود کاربر، مالک Companion و عضو Store.
- هم‌زمانی تولید کد و Unique Index.

دستورات کنترل نهایی:

```powershell
dotnet build Api/Api.csproj --no-restore
dotnet test Application.Tests/Application.Tests.csproj --no-restore
dotnet ef migrations has-pending-model-changes --project Persistence --startup-project Api
```

## 18. کارهای باقی‌مانده پیشنهادی

این موارد بخشی از وضعیت فعلی نیستند و برای فاز بعد پیشنهاد می‌شوند:

1. اتصال Signup به Eventهای Referral و ثبت امتیاز معرف/دعوت‌شده/کسب‌وکار.
2. Anti-Fraud معرفی شامل KYC، Device/IP limit و سقف روزانه کسب‌وکار.
3. Job انقضای Offer و Benefit و Notification.
4. Reconciliation Job برای Eventهای امتیاز ناموفق.
5. Dashboard و گزارش مالی Funding.
6. جدا کردن DTO عمومی Site از Admin برای مخفی کردن ReferralCode و اطلاعات داخلی.
7. تکمیل Actionهای Approve/Reject در UI پنل در صورت نیاز عملیاتی.
