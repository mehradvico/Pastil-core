# پاستیل کلاب — فاز اول (Foundation و Point Ledger)

این سند قرارداد فاز اول پاستیل کلاب را برای بک، پنل و اپ توضیح می‌دهد. دامنه جدید از `ClubReward` قدیمی مستقل است و تمام کلاس‌های آن پیشوند `PastilClub` یا Namespace مستقل `PastilClubField` دارند.

## فازبندی کل پروژه

1. Foundation، Permission، Point Account، Ledger، Debt و Point Rule.
2. اتصال امتیاز به سفارش، رزروها، تکمیل پت و Memory.
3. Reward Template، Target، Offer، Approval و Redemption.
4. Coupon، Free Delivery، Promotional Credit و PastilAI Benefit.
5. Referral، Business Attribution و Commission Resolver.
6. Automation روزانه، Notification و Expiration Jobها.
7. Dashboard، Reportها و تکمیل پنل.

## Permission

گروه والد مستقل زیر به Reflection اضافه شده است:

```text
PastilClubManagement
```

زیرمجموعه‌های فاز اول:

```text
PastilClubPointRule          IsMenu = true
PastilClubPointTransaction   IsMenu = true
PastilClubPointIncrease      IsMenu = false
PastilClubPointDecrease      IsMenu = false
```

بعد از انتشار بک، مدیر اصلی باید Permissionها را همگام کند:

```http
POST /api/Admin/PermissionSync
```

## جداول فاز اول

```text
ClubPointAccounts
ClubPointRules
ClubPointTransactions
```

قواعد دیتابیس:

- هر User فقط یک `ClubPointAccount` دارد.
- `AvailablePoint` و `DebtPoint` منفی نمی‌شوند.
- `IdempotencyKey` در Ledger یکتا است.
- هر `EventType` فقط یک Point Rule دارد.
- Point Account دارای `RowVersion` است.
- Ledger حذف یا ویرایش نمی‌شود؛ اصلاح‌ها با Transaction جدید ثبت می‌شوند.

## منطق بدهی

در Reverse اگر امتیاز موجود کمتر از مبلغ برگشتی باشد، موجودی صفر و مابقی Debt می‌شود. هر Earn جدید ابتدا Debt را تسویه می‌کند و فقط باقی‌مانده وارد AvailablePoint می‌شود.

## API پنل — قوانین امتیاز

```http
GET  /api/Admin/PastilClubPointRule
GET  /api/Admin/PastilClubPointRule/{id}
POST /api/Admin/PastilClubPointRule
PUT  /api/Admin/PastilClubPointRule
```

نمونه افزودن Rule:

```json
{
  "name": "امتیاز ثبت خاطره روزانه",
  "eventType": 5,
  "pointAmount": 10,
  "dailyLimit": 1,
  "monthlyLimit": null,
  "lifetimeLimit": null,
  "active": true,
  "startDate": null,
  "endDate": null,
  "description": "برای هر روز فقط یک‌بار"
}
```

`pointAmount` در کد Hard Code نشده و پنل آن را مدیریت می‌کند.

## EventType

```text
1 = ProductOrderCompleted
2 = CompanionReservationCompleted
3 = PansionReservationCompleted
4 = PetProfileCompleted
5 = MemoryCreated
6 = UserReferralReferrer
7 = UserReferralReferee
8 = BusinessReferralUser
```

## API پنل — گردش امتیاز

```http
GET /api/Admin/PastilClubPointTransaction
```

Queryهای قابل ارسال:

```text
pageIndex
pageSize
q
sortBy
userId
transactionType
sourceType
fromDate
toDate
```

## افزایش دستی

```http
POST /api/Admin/PastilClubPointIncrease
```

```json
{
  "userId": 15,
  "amount": 100,
  "reason": "اصلاح امتیاز توسط پشتیبانی",
  "requestId": "f46e6628-f249-4226-bb5f-d6e1066a3468"
}
```

## کاهش دستی

```http
POST /api/Admin/PastilClubPointDecrease
```

بدنه همان ساختار افزایش را دارد. `requestId` باید برای هر عملیات در فرانت یک UUID جدید باشد. تکرار همان Request باعث اجرای دوباره عملیات نمی‌شود. کاهش دستی فقط در صورت موجودی کافی و نداشتن Debt انجام می‌شود.

## API اپ

```http
GET /api/EndUser/PastilClubPoint
GET /api/EndUser/PastilClubPointTransaction
```

API اول موجودی، بدهی و آمار طول عمر را برمی‌گرداند. API دوم فقط Ledger کاربر لاگین‌شده را نمایش می‌دهد و `userId` ارسالی از سمت کلاینت نادیده گرفته می‌شود.

## Migration

```text
20260810183310_AddPastilClubFoundation
```

پس از تأیید و انتشار نسخه بک:

```powershell
Update-Database
```

## خارج از محدوده این فاز

در این فاز هنوز Eventهای Order، Reservation، Pet و Memory به Point Rule متصل نشده‌اند. این اتصال در فاز دوم انجام می‌شود تا ابتدا Ledger، Debt، Idempotency و قرارداد پنل پایدار باشند.
