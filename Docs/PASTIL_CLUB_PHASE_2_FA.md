# پاستیل کلاب — فاز دوم (اتصال رویدادهای امتیاز)

در این فاز Point Ledger فاز اول به فرایندهای واقعی پاستیل متصل شده است. تمام مقادیر امتیاز از `ClubPointRule` خوانده می‌شوند و هیچ مقدار امتیازی در سرویس سفارش، رزرو، پت یا خاطره Hard Code نشده است.

## معماری اجرا

مسیر تمام رویدادها به شکل زیر است:

```text
Business Service
    -> IClubPointIntegrationService
    -> IClubPointEventService
    -> Active ClubPointRule
    -> IClubPointService
    -> Serializable Transaction
    -> Point Account + Immutable Ledger
```

`IClubPointIntegrationService` مرز اتصال دامنه‌های اصلی به پاستیل کلاب است. ساخت کلید یکتا، پیدا کردن Rule، کنترل محدودیت‌ها و تغییر حساب امتیاز داخل سرویس‌های مرکزی انجام می‌شود و Controllerها منطق مالی ندارند.

اگر عملیات Loyalty به علت خطای موقت شکست بخورد، فرایند اصلی سفارش یا رزرو 500 نمی‌شود و خطا در Log ثبت می‌شود. این جداسازی برای جلوگیری از خراب شدن خرید یا رزرو کاربر است؛ در فاز Automation امکان Reconciliation رویدادهای ناموفق تکمیل خواهد شد.

## رویدادهای متصل‌شده

### سفارش محصول

امتیاز فقط زمانی ثبت می‌شود که:

- وضعیت سفارش `ProductOrderStatus_Delivered` باشد.
- سفارش پرداخت‌شده باشد (`IsPaid = true`).
- وضعیت سفارش Cancel نشده باشد.

تغییر State سفارش به `ProductOrderState_Canceled` یا تأیید درخواست لغو، امتیاز همان سفارش را Reverse می‌کند. تکرار وضعیت Delivered یا Cancel امتیاز تکراری ایجاد نمی‌کند.

Rule موردنیاز:

```text
EventType = 1 (ProductOrderCompleted)
```

### رزرو نماینده / خدمات

با رسیدن Operator State به `OperatorState_Complete` امتیاز ثبت می‌شود. لغو عمومی رزرو یا تغییر Operator State به `OperatorState_Cancelled` امتیاز را Reverse می‌کند.

Rule موردنیاز:

```text
EventType = 2 (CompanionReservationCompleted)
```

### رزرو پانسیون

امتیاز فقط در وضعیت `PansionReserveState_Complete` و زمانی ثبت می‌شود که رزرو نهایی شده و لغو نشده باشد. لغو رزرو امتیاز را Reverse می‌کند.

Rule موردنیاز:

```text
EventType = 3 (PansionReservationCompleted)
```

### تکمیل پروفایل پت

هر پت فقط یک مرتبه امتیاز تکمیل پروفایل دریافت می‌کند. شرط کامل بودن فعلی:

- پت Active و حذف‌نشده باشد.
- نام، نوع پت، تصویر، نژاد و تاریخ تولد معتبر داشته باشد.
- تاریخ تولد در آینده نباشد.
- اگر پت Mix Breed است، نژاد دوم موجود و با نژاد اول متفاوت باشد.

این بررسی هم بعد از افزودن و هم بعد از ویرایش UserPet انجام می‌شود.

Rule موردنیاز:

```text
EventType = 4 (PetProfileCompleted)
```

### خاطره روزانه

هر کاربر در هر روز تهران فقط یک خاطره فعال می‌تواند داشته باشد و فقط یک بار برای همان روز امتیاز می‌گیرد. مرز روز بر اساس `Asia/Tehran` محاسبه می‌شود.

- افزودن خاطره: Earn
- حذف خاطره: Reverse
- انتقال خاطره به روز دیگر: Reverse روز قبلی و Earn روز جدید
- ویرایش در همان روز: بدون امتیاز اضافه

Rule موردنیاز:

```text
EventType = 5 (MemoryCreated)
DailyLimit = 1
```

محدودیت `DailyLimit = 1` پیشنهاد می‌شود؛ علاوه بر آن، کلید روزانه و Validation خاطره نیز از ثبت تکراری جلوگیری می‌کنند.

## EventTypeها

```text
1 = ProductOrderCompleted
2 = CompanionReservationCompleted
3 = PansionReservationCompleted
4 = PetProfileCompleted
5 = MemoryCreated
6 = UserReferralReferrer       (برای فاز Referral)
7 = UserReferralReferee       (برای فاز Referral)
8 = BusinessReferralUser      (برای فاز Referral)
```

اگر Rule مربوط به یک رویداد وجود نداشته باشد، غیرفعال باشد یا خارج از بازه `StartDate` و `EndDate` باشد، فرایند اصلی موفق می‌ماند ولی امتیازی ثبت نمی‌شود.

## محدودیت Ruleها

موارد زیر قبل از Earn و داخل تراکنش `Serializable` بررسی می‌شوند:

- `DailyLimit`: تعداد دفعات مجاز دریافت همان Rule در روز تهران
- `MonthlyLimit`: تعداد دفعات مجاز دریافت همان Rule در ماه تهران
- `LifetimeLimit`: تعداد دفعات مجاز دریافت همان Rule در کل عمر حساب

برای Query این کنترل، ایندکس زیر اضافه شده است:

```text
ClubPointTransactions(UserId, PointRuleId, CreateDate)
```

## Idempotency و Reverse

برای هر Source یک Award Key پایدار ساخته می‌شود و روی `IdempotencyKey` ایندکس Unique وجود دارد. بنابراین Retry درخواست یا ثبت چندباره یک وضعیت، امتیاز دوباره تولید نمی‌کند.

Reverse مقدار دقیق تراکنش Earn اصلی را برمی‌گرداند و با `ParentTransactionId` به آن متصل است. اگر کاربر امتیاز را خرج کرده باشد، منطق Debt فاز اول اعمال می‌شود و موجودی منفی نمی‌شود.

## تغییر API اپ

Endpointهای Point همان قرارداد فاز اول را دارند:

```http
GET /api/EndUser/PastilClubPoint
GET /api/EndUser/PastilClubPointTransaction
```

تغییر مهم UserPet این است که Update به شکل امن و Async فقط روی پت متعلق به کاربر لاگین‌شده انجام می‌شود. Endpointهای سفارش، رزرو و Memory قرارداد جدیدی برای فرانت ندارند؛ امتیاز به‌صورت خودکار بعد از موفقیت عملیات ثبت می‌شود و اپ می‌تواند Balance یا Ledger را دوباره دریافت کند.

## تنظیم پنل

قبل از تست هر سناریو، در صفحه Point Rule یک Rule فعال برای EventType مربوط بسازید. برای هر EventType فقط یک Rule قابل تعریف است. مقدار `pointAmount`، محدودیت‌ها و بازه فعال بودن کاملاً در پنل مدیریت می‌شوند.

## Migration

Migration این فاز:

```text
20260810184856_AddPastilClubPointIntegrations
```

این Migration فقط ایندکس کنترل Limit را اضافه می‌کند و داده موجود را حذف یا بازنویسی نمی‌کند.

پس از انتشار نسخه بک:

```powershell
Update-Database
```

## سازگاری با سیستم قبلی Score

سیستم قدیمی `ScoreService` فعلاً حذف نشده است تا رفتارهای موجود پروژه و کلاینت‌های قبلی خراب نشوند. Point Account و Ledger پاستیل کلاب مستقل از Score قدیمی هستند. انتقال یا حذف Score قدیمی باید در یک مرحله جدا و پس از بررسی مصرف‌کننده‌های آن انجام شود.

## کنترل کیفیت

- Build کامل Solution: موفق، بدون Error و Warning
- تست‌ها: 57 تست موفق
- بررسی EF: هیچ Model Change ثبت‌نشده‌ای باقی نمانده است
- تست‌های جدید شامل Limit روزانه/ماهانه/عمر، کامل بودن Pet، کلید روز تهران و ایندکس دیتابیس هستند

## فاز بعد

فاز سوم طبق فازبندی فعلی پروژه شامل این بخش‌ها است:

```text
Reward Template
Reward Target
Reward Pet Type
Reward Offer
Admin Approval
Reward Redemption
Atomic Point Spend
```
