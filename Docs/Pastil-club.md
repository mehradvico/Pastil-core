# مستند فرانت پاستیل کلاب، کد معرف و ثبت‌نام

این سند قرارداد نهایی فرانت اپ، سایت و پنل با قابلیت‌های فعلی پاستیل کلاب و کد معرف است. تمام نام فیلدها به شکل JSON و با `camelCase` نوشته شده‌اند.

## 1. قواعد عمومی API

- تمام APIهای `EndUser` پاستیل کلاب به توکن کاربر نیاز دارند.
- تمام APIهای `Admin` به توکن ادمین و Permission متناظر نیاز دارند.
- آدرس پایه در محیط Production از تنظیمات پروژه فرانت خوانده شود و داخل کد ثابت نشود.
- تاریخ‌های `DateTimeOffset` به شکل ISO 8601 ارسال شوند؛ مثال: `2026-08-11T20:30:00+03:30`.
- `pageIndex` از 1 شروع می‌شود.
- `pageSize` در بک حداکثر 100 در نظر گرفته می‌شود.
- `sortBy=0` حالت پیش‌فرض، `sortBy=1` جدیدتر و `sortBy=2` قدیمی‌تر را نمایش می‌دهد. در APIهای پاستیل کلاب حالت پیش‌فرض نیز عملاً جدیدتر است.

پاسخ جزئیات و عملیات معمولاً این ساختار را دارد:

```json
{
  "code": 0,
  "isSuccess": true,
  "messages": [],
  "data": {}
}
```

پاسخ جستجو و فهرست معمولاً مستقیم به شکل زیر است:

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "q": null,
  "sortBy": 1,
  "available": null,
  "totalCount": 25,
  "list": []
}
```

در تمام عملیات، موفقیت فقط از `isSuccess === true` تشخیص داده شود. وجود HTTP 200 به‌تنهایی به معنی موفق بودن منطق عملیات نیست.

## 2. صفحه اصلی پاستیل کلاب در اپ

برای صفحه اصلی کلاب پیشنهاد می‌شود داده‌ها به ترتیب زیر خوانده شوند:

1. موجودی امتیاز کاربر
2. پیشنهادهای جایزه فعال
3. کیف مزایای کاربر
4. آخرین گردش‌های امتیاز
5. تاریخچه جوایز دریافت‌شده

## 3. موجودی امتیاز کاربر

```http
GET /api/EndUser/PastilClubPoint
Authorization: Bearer {token}
```

نمونه پاسخ:

```json
{
  "isSuccess": true,
  "messages": [],
  "data": {
    "userId": 15,
    "availablePoint": 820,
    "debtPoint": 0,
    "lifetimeEarnedPoint": 1200,
    "lifetimeSpentPoint": 300,
    "lifetimeReversedPoint": 80
  }
}
```

معنی فیلدها:

| فیلد | توضیح |
|---|---|
| `availablePoint` | امتیاز قابل مصرف فعلی |
| `debtPoint` | بدهی امتیاز ناشی از برگشت عملیات قبلی |
| `lifetimeEarnedPoint` | مجموع امتیاز کسب‌شده |
| `lifetimeSpentPoint` | مجموع امتیاز مصرف‌شده |
| `lifetimeReversedPoint` | مجموع امتیاز برگشت‌خورده |

اگر کاربر هنوز حساب امتیاز نداشته باشد، API موفق است و اعداد صفر برمی‌گردند.

اگر `debtPoint > 0` باشد، دریافت جایزه مجاز نیست و امتیازهای جدید ابتدا بدهی را تسویه می‌کنند.

## 4. گردش امتیاز کاربر

```http
GET /api/EndUser/PastilClubPointTransaction
Authorization: Bearer {token}
```

Queryهای مجاز:

```text
pageIndex
pageSize
q
sortBy
transactionType
sourceType
fromDate
toDate
```

فرانت نباید `userId` را مبنای امنیت قرار دهد؛ بک همیشه UserId را از توکن استخراج می‌کند.

نمونه آیتم:

```json
{
  "id": 321,
  "userId": 15,
  "userFullName": "مهراد حسینی",
  "userMobile": "09339750767",
  "transactionType": 1,
  "amount": 20,
  "availableBefore": 800,
  "availableAfter": 820,
  "debtBefore": 0,
  "debtAfter": 0,
  "sourceType": 5,
  "sourceId": 91,
  "pointRuleId": 5,
  "parentTransactionId": null,
  "description": "Memory created",
  "createDate": "2026-08-11T18:20:00Z",
  "createdByAdminId": null
}
```

### نوع تراکنش (`transactionType`)

| مقدار | عنوان |
|---:|---|
| 1 | کسب امتیاز |
| 2 | مصرف امتیاز |
| 3 | برگشت امتیاز کسب‌شده |
| 4 | استرداد مصرف |
| 5 | ایجاد بدهی |
| 6 | تسویه بدهی |
| 7 | افزایش دستی |
| 8 | کاهش دستی |
| 9 | امتیاز معرفی |
| 10 | اصلاح حساب |

### منبع تراکنش (`sourceType`)

| مقدار | عنوان |
|---:|---|
| 0 | بدون منبع |
| 1 | سفارش محصول |
| 2 | رزرو نماینده/خدمت |
| 3 | رزرو پانسیون |
| 4 | تکمیل پروفایل پت |
| 5 | خاطرات |
| 6 | معرفی کاربر |
| 7 | معرفی کسب‌وکار |
| 8 | دریافت جایزه |
| 9 | ادمین |
| 10 | سیستم |

`amount` برای افزایش مثبت و برای مصرف یا برگشت منفی است. رنگ و علامت UI از خود `amount` تعیین شود.

## 5. رویدادهای خودکار کسب امتیاز

فرانت برای این رویدادها API جداگانه امتیاز صدا نمی‌زند. بعد از موفقیت عملیات اصلی، بک امتیاز را بر اساس قانون فعال ثبت می‌کند:

| رویداد | مقدار `eventType` | زمان ثبت |
|---|---:|---|
| تکمیل سفارش محصول | 1 | سفارش پرداخت‌شده و تحویل‌شده |
| تکمیل رزرو نماینده | 2 | تکمیل نهایی رزرو |
| تکمیل رزرو پانسیون | 3 | تکمیل نهایی رزرو |
| تکمیل پروفایل پت | 4 | اولین بار که اطلاعات ضروری پت کامل شود |
| ثبت خاطره | 5 | ثبت خاطره معتبر روزانه |
| معرف کاربر | 6 | زیرساخت آماده؛ هنوز به Signup متصل نیست |
| کاربر دعوت‌شده | 7 | زیرساخت آماده؛ هنوز به Signup متصل نیست |
| معرفی توسط کسب‌وکار | 8 | زیرساخت آماده؛ هنوز به Signup متصل نیست |

پس از سفارش، رزرو، ویرایش پت یا ثبت خاطره موفق، فرانت می‌تواند API موجودی و گردش را Refresh کند. شکست موقت ثبت امتیاز نباید باعث نمایش شکست عملیات اصلی شود.

## 6. پیشنهادهای جایزه کاربر

فهرست:

```http
GET /api/EndUser/PastilClubRewardOffer
Authorization: Bearer {token}
```

جزئیات:

```http
GET /api/EndUser/PastilClubRewardOffer/{id}
Authorization: Bearer {token}
```

Queryهای فهرست:

```text
pageIndex
pageSize
q
sortBy
rewardType
petTypeId
```

بک فقط پیشنهادهای تأییدشده، منقضی‌نشده، سازگار با پت و متعلق به کاربر لاگین‌شده را برمی‌گرداند. فیلدهای مدیریتی `userId`، `status`، `sourceType`، `rewardTemplateId` و بازه تاریخ در Endpoint کاربر مبنای فیلتر نیستند و نباید از فرانت اپ ارسال شوند.

نمونه آیتم:

```json
{
  "id": 40,
  "userId": 15,
  "rewardTemplateId": 3,
  "templateName": "club-shop-20",
  "title": "۲۰ درصد تخفیف فروشگاه",
  "shortDescription": "تا سقف ۱۵۰ هزار تومان",
  "description": "...",
  "terms": "...",
  "rewardType": 2,
  "sourceType": 1,
  "status": 2,
  "pointCost": 300,
  "benefitValue": 20,
  "maximumBenefitValue": 150000,
  "generatedDate": "2026-08-11T12:00:00Z",
  "approvedDate": "2026-08-11T12:05:00Z",
  "rejectedDate": null,
  "expiresAt": "2026-08-18T20:29:59Z",
  "redeemedDate": null,
  "rejectReason": null,
  "picture": null,
  "targets": [
    { "id": 8, "targetType": 2, "targetId": 15, "includeChildren": false }
  ],
  "petTypeIds": [],
  "canRedeem": true
}
```

دکمه «دریافت جایزه» فقط وقتی فعال شود که `canRedeem === true` باشد. این مقدار در بک با موجودی، بدهی، نوع پت، وضعیت و تاریخ انقضا محاسبه می‌شود.

### وضعیت پیشنهاد (`status`)

| مقدار | عنوان |
|---:|---|
| 1 | در انتظار تأیید |
| 2 | تأییدشده |
| 3 | ردشده |
| 4 | دریافت‌شده |
| 5 | منقضی |
| 6 | لغوشده |

### منبع پیشنهاد (`sourceType`)

| مقدار | عنوان |
|---:|---|
| 1 | ثبت دستی ادمین |
| 2 | خودکار |

### نوع جایزه (`rewardType`)

| مقدار | عنوان |
|---:|---|
| 1 | تخفیف مبلغ ثابت |
| 2 | تخفیف درصدی |
| 3 | ارسال رایگان |
| 4 | اعتبار تشویقی |
| 5 | تخفیف ثابت پلن PastilAI |
| 6 | تخفیف درصدی پلن PastilAI |
| 7 | روز رایگان PastilAI |
| 8 | ماه رایگان PastilAI |
| 9 | ارتقای پلن PastilAI |

## 7. دریافت جایزه

```http
POST /api/EndUser/PastilClubRewardRedeem
Authorization: Bearer {token}
Content-Type: application/json
```

```json
{
  "rewardOfferId": 40
}
```

در موفقیت، `data` از نوع `ClubRewardRedemptionVDto` است:

```json
{
  "id": 17,
  "rewardOfferId": 40,
  "rewardTemplateId": 3,
  "userId": 15,
  "rewardTitle": "۲۰ درصد تخفیف فروشگاه",
  "rewardType": 2,
  "pointTransactionId": 501,
  "pointSpent": 300,
  "remainingPoint": 520,
  "benefitType": 1,
  "benefitReferenceId": 74,
  "redeemedDate": "2026-08-11T14:20:00Z",
  "expiresAt": "2026-08-18T20:29:59Z",
  "status": 1
}
```

دریافت جایزه اتمیک و Idempotent است؛ تکرار درخواست برای همان Offer نباید دو بار امتیاز کم کند. با این حال، هنگام ارسال درخواست دکمه غیرفعال و Loader نمایش داده شود.

خطاهای مهمی که ممکن است داخل `messages` بیایند:

```text
CLUB_REWARD_NOT_FOUND
CLUB_REWARD_NOT_APPROVED
CLUB_REWARD_EXPIRED
CLUB_REWARD_TEMPLATE_NOT_AVAILABLE
CLUB_REWARD_PET_NOT_ELIGIBLE
CLUB_POINT_NOT_ENOUGH
```

بعد از موفقیت Redeem، این سه بخش Refresh شوند:

- موجودی امتیاز
- کیف مزایا
- تاریخچه جوایز دریافت‌شده

## 8. تاریخچه جوایز دریافت‌شده

فهرست:

```http
GET /api/EndUser/PastilClubRewardRedemption
```

جزئیات:

```http
GET /api/EndUser/PastilClubRewardRedemption/{id}
```

Queryها:

```text
pageIndex
pageSize
q
sortBy
rewardTemplateId
status
rewardType
fromDate
toDate
```

وضعیت دریافت:

| مقدار | عنوان |
|---:|---|
| 1 | تکمیل‌شده |
| 2 | ناموفق |
| 3 | لغوشده |
| 4 | منقضی |

نوع مزیت (`benefitType`):

| مقدار | عنوان |
|---:|---|
| 1 | تخفیف |
| 2 | ارسال رایگان |
| 3 | اعتبار تشویقی |
| 4 | PastilAI |

## 9. کیف مزایای کاربر

فقط مزایای فعال:

```http
GET /api/EndUser/PastilClubBenefit
```

فعال‌ها به همراه تاریخچه مصرف‌شده:

```http
GET /api/EndUser/PastilClubBenefit?includeConsumed=true
```

پاسخ:

```json
{
  "isSuccess": true,
  "data": {
    "coupons": [],
    "freeDeliveries": [],
    "promotionalCredits": [],
    "pastilAIBenefits": []
  }
}
```

### کوپن‌ها

```json
{
  "id": 1,
  "rewardTitle": "۲۰ درصد تخفیف فروشگاه",
  "code": "club-f-a1b2c3d4e5",
  "applicationMethod": 1,
  "rewardType": 2,
  "benefitValue": 20,
  "maximumBenefitValue": 150000,
  "expiresAt": "2026-08-18T20:29:59Z",
  "used": false
}
```

`applicationMethod`:

| مقدار | محل مصرف |
|---:|---|
| 1 | سفارش محصول |
| 2 | رزرو نماینده/خدمت |
| 3 | رزرو پانسیون |
| 4 | خرید پلن PastilAI |

کد هر جایزه فقط در متد خودش معتبر است. فرانت همان `code` را در فیلد فعلی `rebateCode` فرآیند مربوط ارسال کند؛ API مستقل جدیدی برای اعمال کوپن لازم نیست.

### ارسال رایگان

```json
{
  "id": 12,
  "rewardTitle": "ارسال رایگان تهران",
  "storeId": 4,
  "cityId": 1,
  "maximumDeliveryAmount": 100000,
  "remainingUsageCount": 1,
  "expiresAt": "2026-08-18T20:29:59Z"
}
```

مزیت ارسال رایگان توسط بک روی Cart معتبر اعمال می‌شود. فرانت مبلغ نهایی Cart را مرجع قرار دهد و خودش تخفیف را محاسبه نکند.

### اعتبار تشویقی

```json
{
  "id": 8,
  "rewardTitle": "۱۰۰ هزار تومان اعتبار خرید",
  "originalAmount": 100000,
  "remainingAmount": 60000,
  "serviceScopeType": 2,
  "serviceScopeId": 15,
  "expiresAt": "2026-08-21T20:29:59Z",
  "status": 1
}
```

اعتبار تشویقی پول قابل برداشت نیست. وقتی کاربر `fromWallet=true` انتخاب می‌کند، بک ابتدا اعتبار تشویقی معتبر، سپس کیف پول نقدی و در آخر درگاه را برای باقی‌مانده استفاده می‌کند.

وضعیت اعتبار تشویقی:

| مقدار | عنوان |
|---:|---|
| 1 | فعال |
| 2 | مصرف‌شده |
| 3 | منقضی |
| 4 | لغوشده |

### مزیت PastilAI

```json
{
  "id": 23,
  "rewardTitle": "۷ روز رایگان پاستیل AI",
  "planId": 2,
  "planName": "Premium",
  "status": 1,
  "startDateUtc": "2026-08-11T14:20:00Z",
  "endDateUtc": "2026-08-18T14:20:00Z"
}
```

## 10. ثبت‌نام و منبع آشنایی

```http
POST /api/Account/signup
```

دو فیلد جدید `SignUpDto`:

| فیلد | نوع | توضیح |
|---|---|---|
| `referralSource` | number | نحوه آشنایی کاربر با پاستیل |
| `referralCode` | string/null | کد معرف واردشده |

### مقادیر `referralSource`

| مقدار | عنوان |
|---:|---|
| 0 | مشخص‌نشده/سازگاری نسخه قدیمی |
| 1 | کلینیک |
| 2 | پت‌شاپ |
| 3 | آشنایان |
| 4 | شبکه اجتماعی |
| 5 | سایر |

قواعد فرم:

- برای گزینه‌های 1، 2 و 3 فیلد کد معرف نمایش داده و اجباری شود.
- برای گزینه‌های 4 و 5 فیلد کد معرف مخفی و `null` ارسال شود.
- ورودی کد فقط عددی باشد.
- کلینیک و پت‌شاپ: دقیقاً 10 رقم.
- آشنایان: دقیقاً 7 رقم.
- ارقام فارسی توسط بک به انگلیسی تبدیل می‌شوند، ولی بهتر است فرانت نیز آن‌ها را Normalize کند.

نمونه:

```json
{
  "mobile": "09120000000",
  "email": "user@example.com",
  "firstName": "نام",
  "lastName": "نام خانوادگی",
  "password": "password",
  "code": "12345",
  "cartCode": null,
  "referralSource": 1,
  "referralCode": "1123456789"
}
```

اعتبارسنجی بک:

- کلینیک فقط کد Companion فعال و تأییدشده را می‌پذیرد.
- پت‌شاپ فقط کد Store فعال را می‌پذیرد.
- آشنایان فقط کد User فعال و قفل‌نشده را می‌پذیرد.
- کد متعلق به نوع دیگر پذیرفته نمی‌شود.
- کاربر نمی‌تواند کد خودش را وارد کند.
- مالک Companion نمی‌تواند کد همان Companion را وارد کند.
- کاربر متصل به Store نمی‌تواند کد همان Store را وارد کند.
- کدهای نامعتبر یا تکرار هویت با پیام خطا برگردانده می‌شوند.

نکته: این کنترل مالکیت با موبایل و ایمیل انجام می‌شود. اگر یک شخص با هویت تماس کاملاً متفاوت ثبت‌نام کند، تشخیص قطعی بدون کد ملی/KYC ممکن نیست.

امتیاز ثبت‌نام با کد معرف پس از ساخته‌شدن موفق کاربر، در دو تراکنش مستقل و Idempotent ثبت می‌شود:

- کد کاربر: Event شماره `6` برای معرف و Event شماره `7` برای کاربر جدید.
- کد کلینیک یا پت‌شاپ: Event شماره `6` برای مالک کسب‌وکار و Event شماره `8` برای کاربر جدید.
- مقدار پیش‌فرض هر Event برابر `100` امتیاز است و ادمین می‌تواند آن را از بخش «تنظیمات امتیاز» تغییر دهد.
- Refresh یا Retry باعث ثبت دوباره امتیاز نمی‌شود.

## 11. کدهای اختصاصی User، Companion و Store

| موجودیت | شکل کد | محل نمایش پیشنهادی |
|---|---|---|
| User | 7 رقم | پروفایل و بخش دعوت دوستان |
| Companion/کلینیک | 10 رقم، شروع با 1 | جزئیات نماینده و حساب مالک |
| Store/پت‌شاپ | 10 رقم، شروع با 2 | جزئیات فروشگاه و حساب مدیر |
| Pansion | کد مستقل ندارد | کد Companion مالک استفاده می‌شود |

کدها توسط بک تولید می‌شوند. فرانت و پنل نباید امکان تایپ یا ویرایش `referralCode` را بدهند؛ فقط Read-only و قابل کپی نمایش داده شود.

در پاسخ DTOهای فعلی این فیلدها وجود دارند:

- `UserDto.referralCode`
- `UserVDto.referralCode`
- `CompanionDto.referralCode`
- `CompanionVDto.referralCode`
- `StoreDto.referralCode`
- `StoreVDto.referralCode`

در جزئیات User همچنین اطلاعات انتساب ثبت‌نام وجود دارد:

```json
{
  "registrationReferralSource": 1,
  "usedReferralCode": "1123456789",
  "referredByUserId": null,
  "referredByCompanionId": 14,
  "referredByStoreId": null
}
```

در هر ثبت‌نام حداکثر یکی از سه `referredBy...Id` مقدار دارد.

## 12. APIهای پنل مدیریت پاستیل کلاب

### قوانین امتیاز

```http
GET  /api/Admin/PastilClubPointRule
GET  /api/Admin/PastilClubPointRule/{id}
POST /api/Admin/PastilClubPointRule
PUT  /api/Admin/PastilClubPointRule
```

بدنه افزودن/ویرایش:

```json
{
  "id": 0,
  "name": "امتیاز خاطره روزانه",
  "eventType": 5,
  "pointAmount": 10,
  "dailyLimit": 1,
  "monthlyLimit": null,
  "lifetimeLimit": null,
  "active": true,
  "startDate": null,
  "endDate": null,
  "description": "برای هر روز یک مرتبه"
}
```

برای هر `eventType` فقط یک Rule تعریف می‌شود. حذف Rule وجود ندارد؛ برای توقف، `active=false` ارسال شود.

### گردش امتیاز ادمین

```http
GET /api/Admin/PastilClubPointTransaction
```

علاوه بر فیلترهای EndUser، `userId` نیز قابل ارسال است.

افزایش دستی:

```http
POST /api/Admin/PastilClubPointIncrease
```

کاهش دستی:

```http
POST /api/Admin/PastilClubPointDecrease
```

```json
{
  "userId": 15,
  "amount": 100,
  "reason": "اصلاح امتیاز توسط پشتیبانی",
  "requestId": "f46e6628-f249-4226-bb5f-d6e1066a3468"
}
```

`requestId` برای هر کلیک جدید یک UUID جدید باشد. در Retry همان عملیات، همان UUID تکرار شود تا امتیاز دوباره تغییر نکند.

### قالب‌های جایزه

```http
GET  /api/Admin/PastilClubRewardTemplate
GET  /api/Admin/PastilClubRewardTemplate/{id}
POST /api/Admin/PastilClubRewardTemplate
PUT  /api/Admin/PastilClubRewardTemplate
```

فیلترها:

```text
q
rewardType
targetType
petTypeId
isManualAllowed
isAutomationAllowed
pageIndex
pageSize
sortBy
```

نمونه کامل قالب:

```json
{
  "id": 0,
  "name": "club-shop-20",
  "title": "۲۰ درصد تخفیف فروشگاه",
  "shortDescription": "تا سقف ۱۵۰ هزار تومان",
  "description": "...",
  "rewardType": 2,
  "applicationMethod": 1,
  "pointCost": 300,
  "startDate": null,
  "endDate": null,
  "expirationType": 2,
  "expirationValue": null,
  "fixedExpirationDate": null,
  "benefitValue": 20,
  "maximumBenefitValue": 150000,
  "fundingType": 1,
  "isAutomationAllowed": true,
  "isManualAllowed": true,
  "active": true,
  "notificationLevel": 1,
  "pictureId": null,
  "terms": "...",
  "targets": [
    { "id": 0, "targetType": 2, "targetId": 15, "includeChildren": false }
  ],
  "petTypeIds": [],
  "pastilAITarget": null
}
```

انواع Target:

| مقدار | عنوان |
|---:|---|
| 1 | عمومی |
| 2 | فروشگاه |
| 3 | محصول |
| 4 | دسته‌بندی محصول |
| 5 | نماینده |
| 6 | خدمت |
| 7 | پکیج خدمت نماینده |
| 8 | پانسیون |
| 9 | PastilAI |
| 10 | پلن PastilAI |
| 11 | شهر |

نوع انقضا:

| مقدار | عنوان |
|---:|---|
| 1 | پایان همان روز |
| 2 | هفت روز |
| 3 | ده روز |
| 4 | سی روز |
| 5 | تاریخ ثابت |

در صورت `expirationType=5`، مقدار `fixedExpirationDate` لازم است. `fundingType` فعلاً فقط 1 یعنی تأمین توسط پاستیل است.

### پیشنهاد جایزه از پنل

```http
GET  /api/Admin/PastilClubRewardOffer
GET  /api/Admin/PastilClubRewardOffer/{id}
POST /api/Admin/PastilClubRewardOffer
```

ساخت پیشنهاد دستی:

```json
{
  "userId": 15,
  "rewardTemplateId": 3,
  "customExpiresAt": null,
  "approveImmediately": false
}
```

تأیید و رد:

```http
POST /api/Admin/PastilClubRewardApprove
POST /api/Admin/PastilClubRewardReject
POST /api/Admin/PastilClubRewardBulkApprove
POST /api/Admin/PastilClubRewardBulkReject
```

بدنه تکی:

```json
{
  "rewardOfferId": 40,
  "reason": "علت رد فقط در عملیات رد"
}
```

بدنه گروهی:

```json
{
  "rewardOfferIds": [40, 41, 42],
  "reason": "علت رد گروهی"
}
```

### جوایز دریافت‌شده در پنل

```http
GET /api/Admin/PastilClubRewardRedemption
GET /api/Admin/PastilClubRewardRedemption/{id}
```

این بخش فقط مشاهده و گزارش است و افزودن/ویرایش/حذف ندارد.

## 13. نکات UI پنل فعلی

صفحات فعلی پنل مدیریت:

```text
/admin/pastilclubpointrule
/admin/pastilclubpointtransaction
/admin/pastilclubrewardtemplate
/admin/pastilclubrewardoffer
/admin/pastilclubrewardredemption
```

- تنظیمات امتیاز: فهرست، افزودن و ویرایش.
- گردش امتیازها: فیلتر کاربر، نوع، منبع و تاریخ + جزئیات.
- قالب‌های جایزه: کارت، جستجو، فیلتر، افزودن، ویرایش و جزئیات.
- پیشنهادهای جایزه: فهرست، فیلتر، ساخت دستی و جزئیات.
- جوایز دریافت‌شده: فهرست، فیلتر و جزئیات.
- کدهای تخفیف زیر منوی پاستیل کلاب قرار گرفته‌اند، اما مصرف آن‌ها همچنان از فرآیندهای استاندارد Rebate انجام می‌شود.

## 14. چک‌لیست پیاده‌سازی فرانت

- [ ] همه Enumها از جدول‌های این سند Map شوند.
- [ ] تمام تاریخ‌ها با timezone نمایش داده شوند.
- [ ] دکمه Redeem فقط با `canRedeem=true` فعال باشد.
- [ ] هنگام Redeem کلیک دوم مسدود شود.
- [ ] بعد از Redeem، Balance، Benefit و Redemption دوباره خوانده شوند.
- [ ] محاسبه مبلغ تخفیف، ارسال رایگان و اعتبار تشویقی در فرانت انجام نشود.
- [ ] `referralCode` در ثبت‌نام فقط برای منابع 1 تا 3 نمایش داده شود.
- [ ] کد معرف User/Companion/Store فقط Read-only باشد.
- [ ] خطاهای `messages` کنار فیلد مرتبط یا به صورت Toast نمایش داده شوند.
- [ ] بعد از Signup موفق، موجودی و گردش امتیاز کاربر جدید دوباره دریافت شود.
