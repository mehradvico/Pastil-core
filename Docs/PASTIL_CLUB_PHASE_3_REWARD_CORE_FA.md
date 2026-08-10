# پاستیل کلاب — فاز سوم (Reward Core)

این فاز هسته پاداش پاستیل کلاب را مستقل از `ClubReward` قدیمی پیاده‌سازی می‌کند. سیستم قدیمی Score/Rebate حذف یا تغییر داده نشده و موجودیت‌های جدید در دامنه `PastilClubField` قرار دارند.

## اجزای اصلی

```text
ClubRewardTemplate
ClubRewardTarget
ClubRewardPetType
ClubRewardOffer
ClubRewardRedemption
```

فرایند اصلی:

```text
Admin creates Template
    -> Admin creates Offer for User
    -> PendingApproval
    -> Approve / Reject
    -> Approved Offer is visible to User
    -> User Redeems
    -> Point Spend + Ledger + Redemption + Offer Update
```

## قوانین قطعی

- تمام Rewardها Point مصرف می‌کنند و `PointCost` باید بیشتر از صفر باشد.
- تأمین مالی در نسخه فعلی همیشه `Pastil` است.
- Offer بدون تأیید ادمین در API کاربر نمایش داده نمی‌شود.
- Offer منقضی، ردشده یا لغوشده نمایش داده نمی‌شود.
- هر Template در نسخه اول فقط یک بار می‌تواند برای هر User صادر شود.
- Pet Targeting هم هنگام نمایش و هم هنگام Redeem بررسی می‌شود.
- Offer فقط یک بار Redeem می‌شود.
- تاریخ انقضا هنگام ساخت Offer Snapshot می‌شود و بعد از Redeem تمدید نمی‌شود.
- کاربر دارای Debt امکان Redeem ندارد.
- عملیات Redeem اتمیک و Idempotent است.

## RewardType

```text
1 = FixedDiscount
2 = PercentageDiscount
3 = FreeDelivery
4 = PromotionalWalletCredit
5 = PastilAIPlanFixedDiscount
6 = PastilAIPlanPercentageDiscount
7 = PastilAIFreeDays
8 = PastilAIFreeMonth
9 = PastilAIUpgrade
```

## TargetType

Targetها بر اساس Entityهای واقعی پروژه Pastil نام‌گذاری شده‌اند:

```text
1  = Global                    targetId = null
2  = Store                     targetId = Store.Id
3  = Product                   targetId = Product.Id
4  = ProductCategory           targetId = Category.Id
5  = Companion                 targetId = Companion.Id
6  = Assistance                targetId = Assistance.Id
7  = CompanionPackage          targetId = CompanionAssistancePackage.Id
8  = Pansion                   targetId = Pansion.Id
9  = PastilAI                  targetId = null
10 = PastilAIPlan              targetId = PastilAiPlan.Id
11 = City                      targetId = City.Id
```

یک Template می‌تواند چند Target داشته باشد. `includeChildren` برای Scopeهایی مانند Category در فاز Benefit Engine هنگام اعمال تخفیف استفاده خواهد شد. Backend معتبر بودن تمام شناسه‌های Target را بررسی می‌کند؛ Dropdownهای پنل باید از API همان Entity به‌صورت Dynamic پر شوند.

## Pet Targeting

`petTypeIds` شناسه‌های جدول `Pets` هستند.

```text
petTypeIds = []
```

یعنی Reward عمومی است.

اگر لیست مقدار داشته باشد، کاربر باید حداقل یک UserPet فعال و حذف‌نشده با `PetId` متناظر داشته باشد. برای مثال Reward سگ برای کاربر Cat-only نمایش داده نمی‌شود ولی برای کاربر Dog یا Dog+Cat نمایش داده می‌شود.

## ExpirationType

```text
1 = EndOfDay
2 = SevenDays
3 = TenDays
4 = ThirtyDays
5 = FixedDate
```

- مرز `EndOfDay` بر اساس `Asia/Tehran` محاسبه می‌شود.
- برای `FixedDate` ارسال `fixedExpirationDate` الزامی است.
- ادمین هنگام ساخت Offer می‌تواند `customExpiresAt` ارسال کند.
- تاریخ سفارشی باید در آینده باشد.

## API پنل — Reward Template

```http
GET  /api/Admin/PastilClubRewardTemplate
GET  /api/Admin/PastilClubRewardTemplate/{id}
POST /api/Admin/PastilClubRewardTemplate
PUT  /api/Admin/PastilClubRewardTemplate
```

نمونه ایجاد Template:

```json
{
  "name": "dog-food-20-percent",
  "title": "۲۰ درصد تخفیف غذای سگ",
  "shortDescription": "ویژه کاربران دارای سگ",
  "description": "تخفیف محصولات غذای سگ",
  "rewardType": 2,
  "pointCost": 300,
  "startDate": null,
  "endDate": null,
  "expirationType": 2,
  "expirationValue": null,
  "fixedExpirationDate": null,
  "benefitValue": 20,
  "maximumBenefitValue": 100000,
  "fundingType": 1,
  "isAutomationAllowed": true,
  "isManualAllowed": true,
  "active": true,
  "notificationLevel": 1,
  "pictureId": 108,
  "terms": "برای محصولات مشخص‌شده قابل استفاده است.",
  "targets": [
    {
      "targetType": 4,
      "targetId": 25,
      "includeChildren": true
    }
  ],
  "petTypeIds": [1]
}
```

فیلترهای لیست:

```text
pageIndex, pageSize, q, sortBy, available
rewardType, targetType, petTypeId
isManualAllowed, isAutomationAllowed
```

## API پنل — Reward Offer

```http
GET  /api/Admin/PastilClubRewardOffer
GET  /api/Admin/PastilClubRewardOffer/{id}
POST /api/Admin/PastilClubRewardOffer
```

نمونه ساخت Offer دستی:

```json
{
  "userId": 15,
  "rewardTemplateId": 3,
  "customExpiresAt": null,
  "approveImmediately": false
}
```

اگر `approveImmediately = true` باشد، رکورد Approval با زمان و Admin جاری روی همان Offer ثبت می‌شود.

فیلترهای Offer:

```text
userId, rewardTemplateId, status, sourceType
rewardType, petTypeId, fromDate, toDate
pageIndex, pageSize, q, sortBy
```

## API پنل — Approval

هر عملیات تغییردهنده Controller مستقل دارد:

```http
POST /api/Admin/PastilClubRewardApprove
POST /api/Admin/PastilClubRewardReject
POST /api/Admin/PastilClubRewardBulkApprove
POST /api/Admin/PastilClubRewardBulkReject
```

Approve/Reject تکی:

```json
{
  "rewardOfferId": 10,
  "reason": "دلیل رد فقط برای Reject الزامی است"
}
```

عملیات گروهی:

```json
{
  "rewardOfferIds": [10, 11, 12],
  "reason": "دلیل رد گروهی"
}
```

فقط Offer با وضعیت `PendingApproval` قابل تأیید یا رد است.

## API پنل — Redemption

```http
GET /api/Admin/PastilClubRewardRedemption
GET /api/Admin/PastilClubRewardRedemption/{id}
```

فیلترها:

```text
userId, rewardTemplateId, status, rewardType
fromDate, toDate, q, pageIndex, pageSize, sortBy
```

## API اپ — Rewardها

```http
GET /api/EndUser/PastilClubRewardOffer
GET /api/EndUser/PastilClubRewardOffer/{id}
```

Backend همیشه UserId را از Token می‌گیرد. فقط Offerهای Approved، غیرمنقضی، مربوط به همان کاربر و سازگار با Petهای او نمایش داده می‌شوند.

فیلد `canRedeem` با توجه به موجودی Point، Debt، انقضا و وضعیت Offer برگردانده می‌شود.

## API اپ — Redeem

```http
POST /api/EndUser/PastilClubRewardRedeem
```

```json
{
  "rewardOfferId": 10
}
```

نمونه نتیجه:

```json
{
  "isSuccess": true,
  "data": {
    "id": 7,
    "rewardOfferId": 10,
    "rewardTemplateId": 3,
    "pointTransactionId": 120,
    "pointSpent": 300,
    "remainingPoint": 550,
    "benefitType": 1,
    "benefitReferenceId": null,
    "redeemedDate": "2026-08-10T20:00:00+00:00",
    "expiresAt": "2026-08-17T20:00:00+00:00",
    "status": 1
  }
}
```

در این فاز Redemption یک Entitlement قطعی ایجاد می‌کند. `benefitReferenceId` در فاز بعد، هنگام اتصال Coupon، Free Delivery، Promotional Credit و PastilAI Benefit مقدار می‌گیرد.

## API اپ — تاریخچه Redemption

```http
GET /api/EndUser/PastilClubRewardRedemption
GET /api/EndUser/PastilClubRewardRedemption/{id}
```

UserId ارسال‌شده از Client نادیده گرفته می‌شود و فقط تاریخچه کاربر لاگین‌شده برمی‌گردد.

## Atomic Redeem

Redeem داخل تراکنش `Serializable` اجرا می‌شود و روی Offer و Point Account قفل `UPDLOCK` می‌گیرد:

```text
Lock Offer
Validate Approval and Expiration
Validate Pet Eligibility
Lock Point Account
Validate Point and Debt
Spend Point
Create Point Transaction
Create Redemption
Mark Offer Redeemed
Commit
```

در صورت شکست هر مرحله، Point کم نمی‌شود. Unique Indexهای Offer و Redemption و کلیدهای Idempotency نیز از Redeem تکراری جلوگیری می‌کنند.

## Permissionها

زیرمجموعه `PastilClubManagement`:

```text
PastilClubRewardTemplate       IsMenu = true
PastilClubRewardOffer          IsMenu = true
PastilClubRewardRedemption     IsMenu = true
PastilClubRewardApprove        IsMenu = false
PastilClubRewardReject         IsMenu = false
PastilClubRewardBulkApprove    IsMenu = false
PastilClubRewardBulkReject     IsMenu = false
```

بعد از انتشار بک:

```http
POST /api/Admin/PermissionSync
```

## Migration

```text
20260810193641_AddPastilClubRewardCore
```

این Migration جدول‌های Reward Core و Indexها/FKهای مربوط را اضافه می‌کند و جدول `ClubRewards` قدیمی را تغییر نمی‌دهد.

بعد از انتشار بک:

```powershell
Update-Database
```

## کنترل کیفیت

- Build کامل Solution: بدون Error و Warning
- تست‌ها: 73 تست موفق
- تست Visibility برای Pending/Approved/Expired
- تست Pet Targeting برای Cat-only، Dog و Dog+Cat
- تست موجودی، Debt و Point Cost
- تست Expiration
- تست Unique Indexهای Offer و Redemption

## فاز بعد

```text
Coupon Benefit
Free Delivery Benefit
Promotional Wallet Credit
PastilAI Benefits
BenefitReferenceId Integration
Checkout Validation
```
