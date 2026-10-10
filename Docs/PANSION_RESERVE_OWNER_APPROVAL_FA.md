# تأیید رزرو پانسیون/مهد توسط مرکز (بعد از پرداخت)

از این به بعد **هر رزرو پانسیون/مهدی که پرداخت می‌شود باید توسط مرکز تأیید شود.** اگر مرکز رد کند یا تا مهلت پاسخ ندهد، رزرو لغو می‌شود و **کل مبلغ پرداخت‌شده به کیف پول کاربر برمی‌گردد** (مثل لغو سفارش فروشگاه).

## جریان
```
کاربر رزرو می‌کند (۴۶ ثبت‌شده)  ──پرداخت──►  ۴۷ پرداخت‌شده + ownerDecision = 1 (منتظر تأیید مرکز)
                                                   │
        ┌──────────── مرکز تأیید می‌کند ───────────┤──► ownerDecision = 2 ← پوش به کاربر؛ ادامه‌ی مسیر عادی
        │                                          │
        │      مرکز (یا ادمین) رد می‌کند + دلیل ───┤──► ownerDecision = 3، رزرو لغو، کل مبلغ به کیف پول، پوش به کاربر
        │                                          │
        └──── تا مهلت (پیش‌فرض ۱۲ ساعت) پاسخ نداد ─┘──► ownerDecision = 4 (لغو خودکار)، کل مبلغ به کیف پول، پوش به کاربر
```
- `statusId` همان ۴۶/۴۷/۴۸ می‌ماند؛ **وضعیت تأیید فیلد جداست:** `ownerDecision`.
- «تکمیل‌شده» (۴۸) فقط برای رزرو تأییدشده (`ownerDecision = 2`) یا رزروهای قدیمی (`0`) مجاز است؛ برای منتظر/رد/منقضی بک‌اند خطای «این رزرو هنوز توسط مرکز تأیید نشده است» می‌دهد.

## فیلدهای جدید در پاسخ رزرو (`PansionReserveVDto`)
| فیلد | معنی |
|---|---|
| `ownerDecision` | `0` نیاز به تأیید ندارد (رزروهای قدیمی/پرداخت‌نشده)، `1` منتظر پاسخ مرکز، `2` تأیید شد، `3` رد شد، `4` مرکز پاسخ نداد (لغو خودکار) |
| `ownerApprovalDeadline` | مهلت پاسخ مرکز (ساعت تهران، بدون offset) |
| `ownerDecisionDate` | زمان تصمیم |
| `ownerDecisionReason` | دلیل رد (برای `3`) یا «مرکز در مهلت مقرر پاسخ نداد» (برای `4`) |

مهلت: ۱۲ ساعت از پرداخت، اما حداکثر تا **یک ساعت قبل از شروع رزرو** (و دست‌کم ۳۰ دقیقه). شروع پانسیون = `fromDate`؛ شروع مهد = `schoolCreateDate` + `startTime`.

## API
| کاربر | Endpoint | بدنه |
|---|---|---|
| مرکز | `PUT api/Companion/PansionReserveApprove` | `{ "id": 123 }` |
| مرکز | `PUT api/Companion/PansionReserveReject` | `{ "id": 123, "reason": "ظرفیت تکمیل است" }` — `reason` الزامی |
| ادمین | `PUT api/Admin/PansionReserveApprove` | `{ "id": 123 }` |
| ادمین | `PUT api/Admin/PansionReserveReject` | `{ "id": 123, "reason": "..." }` |

- پاسخ استاندارد `{IsSuccess, Messages}`. مرکز فقط رزروهای پانسیون/مهد **خودش** را می‌تواند تأیید/رد کند (`AccessDenied` در غیر این صورت).
- خطاها: «این رزرو منتظر تأیید نیست» (قبلاً پاسخ داده شده/لغو/پرداخت‌نشده)، «دلیل رد رزرو را وارد کنید»، «برگشت مبلغ انجام نشد؛ دوباره تلاش کنید» (در این حالت رزرو لغو **نمی‌شود**).
- برگشت پول **یک‌بار** انجام می‌شود (دفتر کیف پول با نام یکتا `PansionReserveRefund:{id}`)؛ تکرار درخواست پول دوباره برنمی‌گرداند.
- خواندن رزروها همان endpointهای قبلی است و `ownerDecision` را برمی‌گردانند: `GET api/Companion/PansionReserve`، `GET api/EndUser/PansionReserve` و ...

## قرارداد دقیق برای اپ نماینده

> **هشدار املا:** مسیر خواندنِ رزروهای نماینده در بک‌اند با غلط املایی `Comapnion` ثبت شده است؛ مسیرهای تأیید/رد با املای درست `Companion` هستند. هر دو را دقیقاً همین‌طور صدا بزنید.

```
GET  api/Comapnion/PansionReserve           ← لیست (املای Comapnion)
GET  api/Comapnion/PansionReserve/{id}      ← جزئیات
PUT  api/Companion/PansionReserveApprove    body: { "id": 123 }
PUT  api/Companion/PansionReserveReject     body: { "id": 123, "reason": "ظرفیت تکمیل است" }
```

- پاسخ approve/reject: `{ "isSuccess": bool, "messages": [...] }`. بک‌اند همیشه HTTP 200 برمی‌گرداند؛ فقط `isSuccess` را چک کنید.
- `reason` در رد الزامی است؛ خالی = «دلیل رد رزرو را وارد کنید».
- مسیر ناموجود هم HTTP 200 با `isSuccess:false` می‌دهد؛ پس از روی پاسخ نمی‌توان وجود یک مسیر را از بیرون فهمید.
- `website/swagger.json` (تاریخ ۲۰ آگوست) قدیمی است و این مسیرها در آن نیست؛ به آن تکیه نکنید.

شکل فیلدهای مهم پاسخ رزرو (`PansionReserveVDto`، camelCase، از روی کد استخراج شده):

```json
{
  "id": 123, "reserveCode": "...", "pansionId": 5, "userPetId": 9,
  "statusId": 47, "isReserved": true, "isCancel": false, "cancelDetail": null,
  "price": 0, "paymentPrice": 0,
  "fromDate": "2026-10-10T00:00:00", "toDate": "2026-10-12T00:00:00",
  "startTime": null, "endTime": null, "schoolCreateDate": null,
  "ownerDecision": 1,
  "ownerApprovalDeadline": "2026-10-09T10:00:00",
  "ownerDecisionDate": null,
  "ownerDecisionReason": null
}
```

`ownerDecision`: `0` نیاز ندارد · `1` منتظر مرکز · `2` تأیید · `3` رد · `4` بی‌پاسخ (لغو خودکار). تاریخ‌ها ساعت تهران و بدون offset هستند.

## پوش‌ها
| نوع | گیرنده | متن |
|---|---|---|
| `PushPansionReserveApprovalRequired` (۱۱۴) | مرکز | «رزرو جدید {پت} در {مرکز} منتظر تأیید شماست.» (لینک `/companionProfile/pansionReserve`) |
| `PushPansionReserveApproved` (۱۱۵) | کاربر | «رزرو {پت} در {مرکز} توسط مرکز تأیید شد.» |
| `PushPansionReserveRejected` (۱۱۶) | کاربر | «رزرو شما در {مرکز} توسط مرکز رد شد و مبلغ به کیف پول شما برگشت. دلیل: …» |
| `PushPansionReserveExpired` (۱۱۷) | کاربر | «مرکز {مرکز} در مهلت مقرر به رزرو شما پاسخ نداد؛ رزرو لغو و مبلغ به کیف پول شما برگشت.» |

متن‌ها از پنل (الگوهای پوش) قابل ویرایش‌اند.

## کار هر کلاینت

### وب‌اپ (انجام‌شده)
- **پنل نماینده** (`companionProfile/pansionReserve/index.vue`): برچسب «منتظر تأیید شما» + مهلت پاسخ؛ دکمه‌ی «تأیید رزرو» و «رد رزرو» (با دلیل). روت‌های BFF: `pansionReserveApprove.put.js`، `pansionReserveReject.put.js`.
- **لیست رزروهای کاربر** (`reserve/index.vue`): وضعیت «منتظر تأیید مرکز»، «رد شد؛ مبلغ به کیف پول برگشت»، «لغو شد؛ مرکز پاسخ نداد، مبلغ برگشت».
- قاعده‌ی نمایش «رزرو قطعی»: فقط `ownerDecision` ∈ {۰، ۲} و `statusId` ∈ {۴۷، ۴۸}. رزرو `ownerDecision = 1` پرداخت‌شده ولی **هنوز قطعی نیست**.

### اپ نماینده (فلاتر) — لازم است
1. در لیست و جزئیات رزرو پانسیون/مهد، اگر `ownerDecision == 1` و `isCancel == false`: دو دکمه‌ی «تأیید» و «رد» (رد با فیلد دلیل الزامی) و نمایش `ownerApprovalDeadline`.
2. با پوش `PushPansionReserveApprovalRequired` لیست را باز/تازه کنید.
3. دکمه‌ی «تغییر وضعیت به تکمیل‌شده» را برای `ownerDecision == 1` غیرفعال کنید (بک‌اند رد می‌کند).

### اپ کاربر (فلاتر)
وضعیت `ownerDecision` را در لیست/جزئیات رزرو نشان دهید (جدول بالا) و پوش‌های ۱۱۵ تا ۱۱۷ را نمایش دهید. رزرو رد یا منقضی `isCancel = true` است و مبلغش در کیف پول برگشته.

## پنل ادمین (انجام‌شده)
صفحه‌ی جزئیات رزرو پانسیون (`/admin/pansionreserve/detail-[id]`): کارت «منتظر تأیید مرکز» با مهلت، و دکمه‌های «تأیید رزرو» و «رد رزرو و برگشت پول» (به‌جای مرکز).

## ملاحظات
- **رزروهای قبلی** `ownerDecision = 0` دارند و مثل قبل بدون تأیید پیش می‌روند. فقط رزروهای پرداخت‌شده **بعد از دیپلوی** وارد این مسیر می‌شوند.
- در رد/انقضا: امتیاز باشگاه (`PansionReserveReversedAsync`) و سفر پت‌رسانِ متصل (`CancelLinkedTripForPansionReserveAsync`) لغو می‌شوند. **امتیازِ کیف‌پولی/Score و شمارش استفاده‌ی کد تخفیف برگردانده نمی‌شود** (همان رفتار لغو فعلی ادمین).
- پیامک جدا نداریم (فقط پوش). اگر لازم شد، قالب پیامک به‌صورت جدا اضافه می‌شود.
- job انقضا هر ۵ دقیقه اجرا می‌شود (`PansionReserveExpireOverdue`)؛ پس لغو خودکار تا ۵ دقیقه بعد از مهلت اتفاق می‌افتد.

## دیتابیس
مایگریشن‌های `AddPansionReserveOwnerApproval` (چهار ستون روی `PansionReserves`) و `SeedPansionReserveApprovalPush` (پوش‌های ۱۱۴ تا ۱۱۷). اسکریپت idempotent: `backend/scripts/AddPansionReserveApproval.sql`.
