# گزارش مالی پانسیون: راهنمای اپ نماینده

**مخاطب:** توسعه‌دهنده‌ی اپ نماینده (Flutter)، بخش «پانسیون من ← مالی»
**تاریخ:** ۱۴۰۵/۰۸/۱۷ (2026-11-08)
**وضعیت بک‌اند:** کد و تست آماده است ولی **هنوز دیپلوی نشده**. تا اعلام دیپلوی، `GET /api/Companion/PansionFinance` روی سرور وجود ندارد و برای کاربر بدون نماینده ۴۰۳ برگشت داده می‌شود.

---

## ۱) مشکل چه بود
پانسیون جدید فقط زیر «ارائه‌دهنده» است و `companionId` ندارد. اندپوینت مالی فقط با `CompanionId` توکن کار می‌کرد:

- `GET /api/Companion/FinanceCompanion` برای کاربری که نماینده ندارد **`403 Forbidden`** برمی‌گرداند، پس صفحه‌ی مالی باز نمی‌شد.
- برای کاربری که نماینده دارد، رزرو پانسیون‌های بدون `companionId` در گزارش نمی‌آمد.

## ۲) کاری که اپ باید بکند
1. صفحه‌ی مالی پانسیون را به **`GET /api/Companion/PansionFinance`** وصل کنید.
2. قبل از این درخواست، وجود `companionId` را چک نکنید و منتظر آن نمانید. فقط توکن کاربر لازم است.
3. برای نقش «پانسیون»، ورود به صفحه را به `companionId` یا نقش «نماینده» مشروط نکنید.
4. `pansionId` و `companionId` را از کش محلی برای کنترل دسترسی استفاده نکنید. سرور مالکیت را از دیتابیس می‌سنجد.

## ۳) اندپوینت جدید
`GET /api/Companion/PansionFinance`

| پارامتر query | نوع | توضیح |
|---|---|---|
| `pansionId` | `long` (اختیاری) | خالی باشد، گزارش **همه‌ی** پانسیون‌های کاربر با هم برمی‌گردد. |
| `permitted` | `bool` (اختیاری) | `true` فقط تأیید مالی‌شده، `false` فقط در انتظار، خالی همه. |

پاسخ مثل بقیه‌ی API با HTTP 200 است. حتماً `isSuccess` را بررسی کنید.

```json
{
  "isSuccess": true,
  "code": 0,
  "messages": [],
  "data": {
    "companionId": 0,
    "reserveCount": 1,
    "totalCompanionShare": 850000,
    "totalSiteShare": 150000,
    "pansions": [
      {
        "id": 12,
        "name": "پانسیون نمونه",
        "isDaycare": false,
        "active": true,
        "deleted": false,
        "providerId": 7,
        "companionId": null,
        "dailyCommissionPercent": 15,
        "hourlyCommissionPercent": 15,
        "hasCommission": true
      }
    ],
    "financeCompanionReserves": [
      {
        "reserveId": "345",
        "reserveCode": "PR-1234",
        "bookerFullName": "نام مشتری",
        "paymentPrice": 1000000,
        "commissionPercent": 15,
        "companionShare": 850000,
        "siteShare": 150000,
        "statusLabel": "تکمیل شده",
        "isPansion": true,
        "isConsultation": false,
        "isSchool": false,
        "packageName": "پانسیون نمونه",
        "durationMinutes": 0,
        "hasPetResan": false,
        "petResanPrice": 0,
        "channelId": 0,
        "permitted": true
      }
    ]
  }
}
```

نکته‌های فیلدها:

| فیلد | توضیح |
|---|---|
| `companionShare` | سهم پانسیون است. نامش برای سازگاری با مدل قبلی عوض نشده است. |
| `packageName` | در ردیف پانسیون، **نام پانسیون** است. وقتی چند پانسیون با هم برگشته، برای نمایش نام هر ردیف از این استفاده کنید. |
| `companionId` | در ریشه‌ی `data` همیشه `0` است. از آن استفاده نکنید. |
| `pansions[].companionId` | ممکن است `null` باشد. |
| `pansions[].deleted` | پانسیون حذف‌شده هم گزارش دارد. آن را با برچسب «حذف‌شده» نشان دهید. |
| `hasPetResan` / `petResanPrice` | هزینه‌ی سفر پت‌رسان فقط برای نمایش است و جزو `companionShare` یا `siteShare` نیست. |
| ردیف‌ها | فقط رزروهای پرداخت‌شده و لغونشده، جدیدترین اول. صفحه‌بندی ندارد. |

```dart
class PansionFinanceInfo {
  final int id;
  final String name;
  final bool? isDaycare;
  final bool active;
  final bool deleted;
  final int? providerId;
  final int? companionId; // nullable
  final double dailyCommissionPercent;
  final double hourlyCommissionPercent;
  final bool hasCommission;
  // constructor و fromJson طبق JSON بالا
}
```

## ۴) خطاها و حالت‌های خالی

| پاسخ | معنی | کار اپ |
|---|---|---|
| `isSuccess=true` و `financeCompanionReserves` خالی | پانسیون وجود دارد ولی رزرو پرداخت‌شده ندارد. | حالت خالی: «هنوز رزروی ثبت نشده» |
| `isSuccess=false` با پیام `دسترسی ندارید` (`AccessDenied`) | کاربر پانسیونی ندارد، یا `pansionId` مال او نیست. | متن `messages` را نشان دهید. |
| `isSuccess=false` با پیام عمومی ناموفق | خطای سرور | پیام خطا و دکمه‌ی تلاش دوباره. آن را «لیست خالی» نشان ندهید. |
| `401` | توکن نامعتبر یا منقضی | مسیر معمول ورود دوباره |

## ۵) سازگاری با نسخه‌ی نصب‌شده‌ی اپ
برای کاربری که `CompanionId` ندارد ولی پانسیون دارد، `GET /api/Companion/FinanceCompanion` هم دیگر ۴۰۳ نمی‌دهد و همین پاسخ را برمی‌گرداند. فیلدهای قبلی سر جایشان هستند.

این فقط راه گذار است. اگر اپ قبل از درخواست خودش `companionId` را چک کند، این تغییر به آن نمی‌رسد و باید طبق بند ۲ اصلاح شود. کاربری که نماینده هم دارد از این اندپوینت گزارش نمایندگی‌اش را می‌گیرد که رزرو پانسیون‌های بدون `companionId` را ندارد، پس بخش پانسیون باید `PansionFinance` را صدا بزند.

## ۶) تسویه تغییری نکرده است
تاریخچه‌ی تسویه همان `GET /api/Companion/PansionSettlement` است (نوع `7`، و `?kind=1` برای تسویه‌های قدیمی). جزئیات در `PANSION_PROVIDER_OWNER_APP_FA.md` بند ۷ آمده است.

## ۷) چک‌لیست تست
- [ ] کاربر فقط ارائه‌دهنده با پانسیون، بدون نماینده: صفحه‌ی مالی باز می‌شود و ردیف‌ها می‌آیند.
- [ ] کاربر با نماینده و پانسیون قدیمی: رزروهای پانسیون می‌آیند.
- [ ] کاربر با چند پانسیون: بدون `pansionId` همه و با `pansionId` فقط همان یکی.
- [ ] `pansionId` پانسیون کاربر دیگر: پیام «دسترسی ندارید».
- [ ] فیلتر `permitted` با هر سه حالت.
- [ ] پانسیون بدون رزرو: حالت خالی، نه خطا.
