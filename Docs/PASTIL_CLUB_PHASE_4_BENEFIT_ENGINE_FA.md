# پاستیل کلاب — فاز چهارم: Benefit Engine

این سند تغییرات Backend فاز Benefit Engine و قرارداد موردنیاز پنل و اپ را توضیح می‌دهد.

## خروجی این فاز

بعد از Redeem شدن Offer، یکی از مزیت‌های واقعی زیر ساخته می‌شود:

- کد تخفیف شخصی برای سفارش، رزرو نماینده، رزرو پانسیون یا PastilAI
- ارسال رایگان فروشگاه با محدودیت فروشگاه، شهر و سقف مبلغ
- اعتبار تشویقی مستقل، تاریخ‌دار و غیرقابل‌برداشت
- تخفیف پلن، روز رایگان، ماه رایگان یا Upgrade در PastilAI

ساخت Benefit داخل همان Transaction مربوط به کم‌شدن امتیاز انجام می‌شود. اگر ساخت Benefit شکست بخورد، Point کم نمی‌شود و Offer نیز Redeemed نخواهد شد.

## Migration

Migration این فاز:

```text
20260810195933_AddPastilClubBenefitEngine
```

بعد از دریافت کد Backend اجرا شود:

```powershell
Update-Database
```

یا:

```powershell
dotnet ef database update --project Persistence --startup-project Api
```

## تغییر DTO قالب جایزه

در `ClubRewardTemplateDto` این فیلد اضافه شده است:

```json
{
  "applicationMethod": 1,
  "pastilAITarget": null
}
```

مقادیر `applicationMethod`:

| مقدار | کاربرد |
|---:|---|
| 1 | سفارش محصول |
| 2 | رزرو نماینده/خدمت |
| 3 | رزرو پانسیون |
| 4 | PastilAI |

هر کد تخفیف فقط برای همان متد قابل استفاده است. برای مثال کد رزرو پانسیون در سبد فروشگاه یا خرید PastilAI پذیرفته نمی‌شود.

## انواع Reward

| مقدار | نوع |
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

## ساخت Reward تخفیفی

نمونه تخفیف ۲۰ درصدی مخصوص فروشگاه، با سقف ۱۵۰ هزار تومان:

```json
{
  "name": "club-shop-20",
  "title": "۲۰ درصد تخفیف فروشگاه",
  "rewardType": 2,
  "applicationMethod": 1,
  "pointCost": 300,
  "expirationType": 2,
  "benefitValue": 20,
  "maximumBenefitValue": 150000,
  "fundingType": 1,
  "isAutomationAllowed": true,
  "isManualAllowed": true,
  "active": true,
  "notificationLevel": 1,
  "targets": [
    { "targetType": 2, "targetId": 15, "includeChildren": false }
  ],
  "petTypeIds": []
}
```

بعد از Redeem، Backend یک `Rebate` شخصی با `UseCount = 1` می‌سازد. بنابراین فرانت همان فیلد فعلی `rebateCode` را استفاده می‌کند و API جدا برای اعمال Coupon لازم نیست.

قوانین:

- Coupon فقط برای User صاحب Reward معتبر است.
- در هر Checkout فقط یک کد تخفیف پذیرفته می‌شود.
- `maximumBenefitValue` سقف تخفیف درصدی است.
- Target فروشگاه، محصول، دسته‌بندی، نماینده، خدمت، پکیج، پانسیون و پلن PastilAI دوباره هنگام استفاده کنترل می‌شود.
- انقضای Coupon دقیقاً از Offer گرفته می‌شود و بعد از Redeem تمدید نمی‌شود.

## ساخت ارسال رایگان

```json
{
  "name": "free-delivery-tehran",
  "title": "ارسال رایگان تهران",
  "rewardType": 3,
  "applicationMethod": 1,
  "pointCost": 150,
  "expirationType": 2,
  "maximumBenefitValue": 100000,
  "fundingType": 1,
  "isManualAllowed": true,
  "active": true,
  "notificationLevel": 1,
  "targets": [
    { "targetType": 11, "targetId": 1, "includeChildren": false }
  ]
}
```

Target مجاز برای ارسال رایگان فقط `Global`، `Store` و `City` است. Backend مزیت معتبر را خودکار روی Cart اعمال می‌کند و این فیلدها در پاسخ Cart قابل نمایش‌اند:

```json
{
  "clubFreeDeliveryBenefitId": 12,
  "clubDeliveryDiscount": 85000,
  "deliveryPrice": 85000,
  "paymentPrice": 920000
}
```

پس از پرداخت موفق، یک Usage کم و هزینه آن به‌عنوان هزینه تأمین‌شده توسط پاستیل در Ledger ثبت می‌شود.

## اعتبار تشویقی

اعتبار تشویقی با Wallet نقدی یکی نشده است؛ در جدول و Ledger مستقل نگهداری می‌شود و قابل‌برداشت نیست.

```json
{
  "name": "promo-credit-store",
  "title": "۱۰۰ هزار تومان اعتبار خرید",
  "rewardType": 4,
  "applicationMethod": 1,
  "pointCost": 400,
  "expirationType": 3,
  "benefitValue": 100000,
  "fundingType": 1,
  "isManualAllowed": true,
  "active": true,
  "notificationLevel": 2,
  "targets": [
    { "targetType": 2, "targetId": 15, "includeChildren": false }
  ]
}
```

در پرداخت‌های دارای `fromWallet = true`:

1. اعتبار تشویقی معتبر و منطبق با Scope محاسبه می‌شود.
2. اعتبار با نزدیک‌ترین تاریخ انقضا ابتدا مصرف می‌شود.
3. سپس Wallet نقدی مصرف می‌شود.
4. در صورت باقی‌ماندن مبلغ، درگاه فقط باقی‌مانده را دریافت می‌کند.
5. مصرف با `ReferenceKey` ثبت می‌شود تا Callback تکراری دوباره اعتبار کم نکند.

این مسیر برای سفارش محصول، رزرو نماینده، رزرو پانسیون و PastilAI متصل شده است.

## مزایای PastilAI

برای تمام Rewardهای PastilAI باید `applicationMethod = 4` و `pastilAITarget` ارسال شود:

```json
{
  "rewardType": 7,
  "applicationMethod": 4,
  "benefitValue": 7,
  "targets": [
    { "targetType": 10, "targetId": 2, "includeChildren": false }
  ],
  "pastilAITarget": {
    "planId": 2,
    "targetPlanId": null,
    "freeDays": 7,
    "isUpgrade": false
  }
}
```

نمونه Upgrade:

```json
{
  "rewardType": 9,
  "applicationMethod": 4,
  "targets": [
    { "targetType": 10, "targetId": 2, "includeChildren": false }
  ],
  "pastilAITarget": {
    "planId": 2,
    "targetPlanId": 3,
    "freeDays": 30,
    "isUpgrade": true
  }
}
```

تاریخ پایان Subscription هیچ‌وقت از `RewardOffer.ExpiresAt` عبور نمی‌کند.

## API کیف مزایای کاربر

```http
GET /api/EndUser/PastilClubBenefit
Authorization: Bearer {token}
```

پیش‌فرض فقط مزیت‌های فعال را برمی‌گرداند. برای تاریخچه:

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

در UI اپ پیشنهاد می‌شود چهار Tab مجزا برای همین چهار آرایه ساخته شود و تاریخ انقضا، Scope، مقدار باقی‌مانده و وضعیت مصرف نمایش داده شود.

## جدول‌های افزوده‌شده

- `ClubCoupons`
- `ClubFreeDeliveryBenefits`
- `ClubPromotionalWalletCredits`
- `ClubPromotionalCreditUsages`
- `ClubRewardPastilAITargets`
- `ClubRewardCostTransactions`

همچنین روی Cart و ProductOrder اطلاعات ارسال رایگان Club و روی PastilAI Subscription شناسه Redemption ذخیره می‌شود.

## نکات پنل

- فیلد `applicationMethod` در فرم RewardTemplate اجباری است.
- برای Rewardهای PastilAI فرم `pastilAITarget` نمایش داده شود.
- برای FreeDelivery فقط Targetهای Global، Store و City قابل انتخاب باشند.
- برای Percentage Discount فیلد `maximumBenefitValue` با عنوان «حداکثر مبلغ تخفیف» نمایش داده شود.
- برای Promotional Credit مقدار `benefitValue` اجباری است.
- Funding در نسخه فعلی فقط مقدار `Pastil = 1` دارد.

## کنترل کیفیت انجام‌شده

- Build کل Solution موفق است.
- ۷۹ تست موفق است.
- EF Core اعلام کرده مدل و Migration همگام‌اند و Pending Model Change وجود ندارد.
- Migration روی Database توسط Codex اجرا نشده است.
