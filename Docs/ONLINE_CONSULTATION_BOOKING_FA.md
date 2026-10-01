# مشاوره آنلاین «قابل رزرو» (انتخاب ساعت، مثلاً ساعت ۱۶:۰۰) — سند طراحی بک‌اند

> وضعیت: **بک‌اند پیاده‌سازی شد (۱۴۰۵/۰۷/۰۹)؛ مایگریشن‌ها فقط به‌صورت فایل ساخته شده و روی هیچ دیتابیسی اجرا نشده.** · تست واحد: `Application.Tests/ConsultationBookingRulesTests.cs`
> مستندهای اپ: [نماینده/امیرمحسن](CONSULTATION_BOOKING_AGENT_AMIRMOHSEN_FA.md) · [کاربر/آرمان](CONSULTATION_BOOKING_USER_ARMAN_FA.md)
> پیش‌نیاز: [ONLINE_CONSULTATION_PACKAGES_FA.md](ONLINE_CONSULTATION_PACKAGES_FA.md) (خرید فوری و ماشین حالت)

## ۱) مسئله و تصمیم

مدل قبلی «خرید فوری» است: کاربر می‌خرد و پنجره‌ی ۳۰/۶۰ دقیقه‌ای از لحظه‌ی «شروع» نماینده حساب می‌شود. نیاز جدید: کاربر **ساعت** انتخاب کند («فردا ساعت ۱۶:۰۰ ویدیوکال») و مشاوره همان ساعت انجام شود.

تصمیم: **همان جدول‌ها و همان ماشین حالت** (`ConsultationPackage` / `ConsultationPurchase` / `OnlineSession`) با افزودن یک ویژگی «قابل رزرو»، نه سیستم موازی. خرید فوری بدون هیچ تغییر رفتاری کار می‌کند.

| | خرید فوری (قبلی) | قابل رزرو (جدید) |
| --- | --- | --- |
| پکیج | `bookable = false` | `bookable = true` |
| کاربر ساعت انتخاب می‌کند؟ | خیر | بله (`scheduledStart` الزامی) |
| شرط «نماینده آنلاین است» هنگام خرید | بله | **خیر** |
| مهلت شروع نماینده | ۲۴ ساعت بعد از پرداخت | **ساعت رزرو + ۱۵ دقیقه** |
| نماینده کِی می‌تواند «شروع» بزند | هر وقت | **از ۱۰ دقیقه قبل از ساعت رزرو** |
| لغو آزاد کاربر | قبل از شروع | **تا ۲ ساعت قبل از ساعت رزرو** |
| پنجره‌ی مشاوره | از لحظه‌ی شروع تا + مدت | **بدون تغییر** (از لحظه‌ی واقعی شروع؛ اگر نماینده دیر شروع کند کاربر مدت کامل را می‌گیرد) |
| بعد از مهلت شروع نشد | Expired + بازپرداخت | **بدون تغییر** (همان job) |

## ۲) مدل داده (مایگریشن `AddConsultationBooking` — اجرا نشده)

- `ConsultationPackages.Bookable bit NOT NULL DEFAULT 0`
- `ConsultationPurchases.ScheduledStart datetime2 NULL`, `ScheduledEnd datetime2 NULL` (= شروع + مدت؛ برای شمارش هم‌پوشانی) + ایندکس `(CompanionId, ScheduledStart)`
- جدول جدید **`ConsultationAvailabilities`**: بازه‌ی کاری هفتگی کلینیک: `CompanionId, WeekDayId (۱ شنبه … ۷ جمعه), StartMinute, EndMinute (دقیقه از نیمه‌شب), Capacity, Active, Deleted, CreateDate`.
- مایگریشن `SeedConsultationBookingPushTypes`: پوش‌های ۸۳ / ۸۴ / ۸۵ (بند ۶).
- اسکریپت idempotent: `backend/scripts/AddConsultationBooking.sql` (از `SeedSchoolClassReminderPushType` تا آخر).

## ۳) قواعد (کد خالص: `ConsultationBookingRules`)

- **شبکه‌ی ۱۵ دقیقه‌ای**: هم مرز بازه‌های کاری و هم ساعت شروع رزرو باید روی ۰۰/۱۵/۳۰/۴۵ باشد (ثانیه = ۰).
- کل مدت مشاوره باید **داخل یک** بازه‌ی کاری بماند (از وسط تعطیلی/استراحت رد نمی‌شود). بازه‌ی منتهی به `24:00` مجاز است؛ بازه‌ی شبانه‌روزی که از نیمه‌شب رد شود نه (دو بازه تعریف شود).
- بازه‌های هم‌روز هم‌پوشان نباشند (لبه‌به‌لبه مجاز)؛ `capacity` بین ۱ تا ۲۰؛ حداکثر ۵۰ بازه.
- ظرفیت = تعداد مشاوره‌ی هم‌زمان؛ رزروی «ظرفیت‌گیر» است که `Paid` یا `Active` باشد یا `PendingPayment` تازه (≤ ۲۰ دقیقه). رزرو با هر مدت، اسلات‌های هم‌پوشان خودش را مصرف می‌کند.
- حداقل فاصله تا «الان» ۳۰ دقیقه؛ حداکثر ۱۴ روز آینده؛ کاربر در یک ساعت دو رزرو هم‌پوشان نمی‌تواند داشته باشد (با هر کلینیک).
- ثابت‌ها در `ConsultationRules` (`BookingSlotStepMinutes`, `BookingMinLead`, `BookingMaxDaysAhead`, `BookingEarlyStart`, `BookingStartGrace`, `BookingFreeCancelBefore`, `BookingPendingHold`, `BookingReminderLead`, `BookingMaxCapacity`). **عددها پیش‌فرض‌اند و منتظر تأیید محصول** (به‌خصوص لغو ۲ ساعت قبل و مهلت دیرکرد ۱۵ دقیقه).
- ساعت‌ها ساعت دیواری سرور (ایران)، مثل بقیه‌ی مشاوره (قاعده‌ی طلایی زمان در `ONLINE_CONSULTATION_APP_FA.md` §۲).

## ۴) جریان

```
نماینده: پکیج bookable=true  +  PUT ConsultationAvailability (ساعت‌های کاری)
کاربر : Days → Slots → POST ConsultationPurchase{packageId, scheduledStart,...}
        └ تراکنش Serializable: بررسی اسلات/ظرفیت/هم‌پوشانی کاربر + ساخت PendingPayment  (ظرفیت نگه داشته می‌شود)
        └ پرداخت (کیف پول/درگاه) → Paid، StartDeadline = scheduledStart + ۱۵ دقیقه
        └ پوش ۸۳ (نمایندگان) و ۸۴ (کاربر)
۳۰ دقیقه قبل: پوش ۸۵ به هر دو (job ConsultationBookingReminders، هر دقیقه)
از ۱۰ دقیقه قبل: نماینده «شروع» → Active → ادامه مثل خرید فوری (پنجره، چت/تماس، پوش ۷۳، Completed)
نماینده نیامد (> ساعت+۱۵د): job ConsultationExpireOverdue → Expired + بازپرداخت + پوش ۷۴
لغو کاربر: تا ۲ ساعت قبل → Cancelled + بازپرداخت کامل؛ دیرتر: خطای ConsultationBookingCancelTooLate
```

- شروع زودهنگام (> ۱۰ دقیقه قبل) خطای `ConsultationBookingTooEarly`؛ `canStart=false` در فهرست نماینده.
- «هنوز کسی جواب نداده» (`NotifyUnclaimed`) برای رزرو ساعت‌دار فقط وقتی ساعت رزرو تا ۱۰ دقیقه‌ی دیگر رسیده/گذشته باشد می‌رود (نه روزها قبل).
- `NotifyPendingPurchases` (شبکه‌ی اطمینان) برای رزرو ساعت‌دار بر پایه‌ی پوش ۸۴ dedupe می‌کند.

## ۵) API

| endpoint | کاربرد |
| --- | --- |
| `GET/PUT /api/Companion/ConsultationAvailability` | برنامه‌ی هفتگی کلینیک من (مالک). PUT کل برنامه را جایگزین می‌کند؛ `[]` = رزرو بسته |
| `POST/PUT /api/Companion/ConsultationPackage` | فیلد جدید `bookable` (`null` در ویرایش = تغییر نکند) |
| `GET /api/EndUser/ConsultationPackage?companionId=` | فیلد جدید `bookable` در هر پکیج |
| `GET /api/EndUser/ConsultationBooking/Days?packageId=` | ۱۵ روز (امروز تا +۱۴) با `availableSlots` |
| `GET /api/EndUser/ConsultationBooking/Slots?packageId=&date=yyyy-MM-dd` | ساعت‌های شروع آن روز با `available` |
| `POST /api/EndUser/ConsultationPurchase` | فیلد جدید `scheduledStart` (برای پکیج bookable الزامی) |
| `GET /api/EndUser/ConsultationPurchase[/{id}]` | فیلدهای جدید `scheduledStart`, `scheduledEnd`, `cancelAllowedUntil` |
| `GET /api/Companion/ConsultationSession` | فیلدهای جدید `scheduledStart`, `scheduledEnd`؛ `canStart` شامل شرط «۱۰ دقیقه قبل» |

جزئیات و نمونه‌ی JSON در دو مستند اپ.

## ۶) اعلان‌ها (مایگریشن seed؛ منبع فارسی `Pattern.fa.resx`)

| نوع | گیرنده | متن | مقصد |
| ---: | --- | --- | --- |
| 83 `PushConsultationBookedAgent` | نمایندگان | «{کاربر} یک مشاوره‌ی {روش} برای {تاریخ شمسی ساعت HH:mm} رزرو کرده است» | `/consultations/manage` |
| 84 `PushConsultationBookedUser` | کاربر | «مشاوره‌ی شما در {کلینیک} برای … ثبت شد؛ در همان ساعت نماینده با شما ارتباط می‌گیرد» | `/consultations` |
| 85 `PushConsultationBookingReminder` | هر دو | «مشاوره‌ی {طرف مقابل} تا ۳۰ دقیقه‌ی دیگر شروع می‌شود» | `/consultations` یا `/consultations/manage` |

برای رزرو ساعت‌دار پوش‌های ۷۱/۷۲ ارسال **نمی‌شود** (جایگزین‌اند). بقیه‌ی پوش‌ها (۶۶–۷۰، ۷۳، ۷۴، ۷۸–۸۰) مثل قبل.

## ۷) محدودیت‌ها و تصمیم‌های باز

- **رزروِ در انتظار پرداخت ۲۰ دقیقه ظرفیت را نگه می‌دارد.** اگر کاربر بعد از ۲۰ دقیقه در درگاه پرداخت کند، رزرو `Paid` می‌شود و ممکن است ظرفیت اسلات اندکی بیش‌رزرو شود (کلینیک می‌تواند مالک را برای تخصیص به نماینده‌ی دیگر یا ادمین را برای لغو+بازپرداخت درگیر کند). ریسک کم است؛ در صورت نیاز می‌شود در `ActivateAfterPaymentAsync` دوباره ظرفیت را سنجید.
- **ظرفیت ≠ تعداد نماینده‌ی واقعی.** سرور نمی‌داند در آن ساعت چند نماینده آماده‌اند؛ مالک `capacity` را دستی می‌گذارد. هر نماینده‌ی مجاز می‌تواند هر رزرو را شروع کند.
- تغییر/حذف بازه‌های کاری **رزروهای ثبت‌شده را لغو نمی‌کند**؛ اپ نماینده باید هشدار بدهد (مستند امیرمحسن).
- جابه‌جایی ساعت رزرو (Reschedule) نیست؛ کاربر لغو (تا ۲ ساعت قبل) و دوباره رزرو می‌کند.
- ادمین فعلاً برنامه‌ی هفتگی کلینیک را از پنل ویرایش نمی‌کند (فقط خود مالک)؛ `bookable` را از API ادمین پکیج می‌شود زد.
- پنل ادمین فهرست خریدها هنوز ستون ساعت رزرو را نشان نمی‌دهد (DTO ادمین دست نخورده).

## ۸) چک‌لیست دیپلوی (هیچ‌کدام خودکار نیست)

۱. بکاپ دیتابیس (`scripts/Backup-Database.ps1`). ۲. اجرای `scripts/AddConsultationBooking.sql` (یا `update-database`). ۳. بررسی `PushTypes` شناسه‌های ۸۳–۸۵. ۴. دیپلوی Api (Hangfire job جدید `ConsultationBookingReminders` را خودش ثبت می‌کند) و سرویس Payment با همان نسخه‌ی `Application`. ۵. اپ‌ها: نماینده (امیرمحسن) و کاربر (آرمان). ۶. دود‌آزمایی روی استیجینگ: ساخت پکیج bookable → تعریف ساعت → رزرو با کیف پول → پوش ۸۳/۸۴ → شروع از ۱۰ دقیقه قبل → پایان؛ و یک رزرو بدون شروع تا مهلت برای دیدن بازپرداخت.
