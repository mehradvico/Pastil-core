# پرامپت برای Codex: نوشتن مستند کامل «آرایشگاه (Groomer)» در پاستیل

> این فایل را کامل به Codex بدهید. خروجی مورد انتظار در انتهای فایل آمده است.

## نقش و هدف
تو مستندنویس فنی پروژه‌ی پاستیل هستی. بک‌اند ماژول **آرایشگاه (Groomer)** کامل ساخته شده است. باید **مستند کامل و دقیق** بنویسی تا تیم‌های فرانت (پنل ادمین، وب‌اپ مشتری، اپ نماینده/آرایشگاه، اپ اندروید) بتوانند فقط با خواندن آن، همه‌ی صفحه‌ها و فراخوانی‌ها را بسازند.

**قانون اصلی:** هر ادعا را از کد بخوان و تأیید کن؛ حدس نزن. اگر چیزی در کد نبود بنویس «در کد نیست». مقدارهای Secret/کلید را ننویس. متن‌های کاربر فارسی و RTL است.

## ساختار مخزن
چهار ریپوی مستقل: `backend/` (.NET 9، EF Core 9، SQL Server)، `panel/` (Nuxt 3)، `webapp/` (Nuxt 4)، `website/`. همه‌ی کدِ این ماژول در `backend/` و `panel/` است؛ **وب‌اپ و اپ‌ها هنوز ساخته نشده‌اند** و مستند باید برایشان قرارداد API و جریان صفحه‌ها بدهد.
پاسخ‌های بک‌اند تقریباً همیشه HTTP 200 با `{IsSuccess, Code, Messages, Data}` هستند؛ موفقیت را از `IsSuccess` بخوان، نه از کد HTTP.

## منابعی که باید بخوانی (به ترتیب)
1. `backend/Docs/GROOMER_FA.md` (خلاصه‌ی فازها، دیپلوی، قالب پیامک‌ها)
2. `docs/ai/FEATURE_MAP.md` (بخش Groomer) و `docs/ai/PROJECT_OVERVIEW.md`
3. Entities: `backend/Entities/Entities/GroomerField/*`، `Entities/Entities/SettlementGroomer.cs`
4. پیکربندی دیتابیس: `backend/Persistence/Context/GroomerModelConfiguration.cs`، مایگریشن‌ها `AddGroomers`، `AddGroomerReserves`، `AddGroomerPhase3`، `AddAssistanceIsGrooming` در `backend/Persistence/Migrations/`
5. سرویس‌ها: `backend/Application/Services/GroomerSrvs/**` (Core، Catalog، Profile، Portfolio، ReserveSrv، ReviewSrv، `GroomerNotifier.cs`)، `Application/Maping/GroomerMap.cs`
6. کنترلرها: `backend/Api/Areas/{Admin,Companion,EndUser}/Controllers/Groomer*.cs` و `backend/Api/Controllers/Groomer*.cs` (عمومی)
7. یکپارچگی‌ها: `PaymentService`، `WalletService`، `RebateService`، `SettlementService`، `ProviderFinanceService`، `ClubPointIntegrationService`، `AdminPermissionCatalog`، `Program.cs` (Hangfire)
8. اسکریپت‌ها: `backend/scripts/{AddGroomers,AddGroomerReserves,AddGroomerPhase3,AddAssistanceIsGrooming,MigrateCompanionGroomersToGroomers,SeedGroomerSms,DeactivateClinicGroomingServices}.sql`
9. تست‌ها (برای فهمیدن قوانین): `backend/Application.Tests/Groomer/*`
10. پنل: `panel/store/useAdminGroomerStore.ts`، `panel/components/groomer/*`، `panel/pages/admin/groomer/*`، `panel/pages/admin/groomerreserve/index.vue`، `panel/pages/admin/companion/index.vue` (لیست یکپارچه‌ی نمایندگی‌ها)

## خلاصه‌ی فلو و قوانین (برای راستی‌آزمایی و تفصیل، از کد تأیید کن)

### ۱. چرا جدا شد
آرایشگاه قبلاً یک `Companion` بود. حالا موجودیت مستقل `Groomer` است با جریان کامل مثل Pansion/School، و **ارائه‌دهنده (Provider)** مالک آن است (`Groomer.ProviderId`). در لیست یکپارچه‌ی نمایندگی‌ها (ادمین) نوع ۵ است. Companionهای قدیمی که به Groomer منتقل شده‌اند (`Groomer.LegacyCompanionId`) و نوع دیگری (مثل کلینیک) ندارند، از لیست ادمین نمایندگی‌ها و از جستجوی عمومی نماینده/کلینیک/«نزدیک من»/جستجوی سراسری مخفی‌اند.

### ۲. موجودیت‌ها
Groomer، GroomerUser (پرسنل؛ `UserAccept` null=منتظر، true=پذیرفته، false=رد، `InviteDate`)، GroomerService (خدمت از کاتالوگ `Assistance`)، GroomerServicePackage (قیمت، پیش‌پرداخت، اندازه‌ی پت، عکس)، GroomerServiceUser (تخصیص خدمت به پرسنل)، GroomerTime (ساعت کاری هفتگی با ظرفیت)، GroomerZone، GroomerPet (پت‌های پذیرفته‌شده)، GroomerPicture، GroomerComment، **نمونه‌کار** (GroomerPortfolioPost/Picture/Like/Comment شبیه اینستاگرام)، GroomerReserve، GroomerReserveItem (آیتم‌های رزرو چندخدمتی)، SettlementGroomer. جدول‌ها، کلیدها، ایندکس‌ها و CHECKها را از `GroomerModelConfiguration.cs` بنویس.

### ۳. کاتالوگ خدمت و جداسازی از کلینیک
`Assistances.IsGrooming`: خدمت گرومینگ فقط برای آرایشگاه است. آرایشگاه فقط خدمت `IsGrooming=true` می‌سازد (`GroomerServiceNotGrooming`). کلینیک/نماینده نمی‌تواند آن را بسازد، تغییر دهد، ببیند یا رزرو کند (`CompanionAssistanceGroomingNotAllowed`). `GET /api/Assistance` در endpointهای عمومی/EndUser/Site پیش‌فرض `IsGrooming=false` است؛ اپ آرایشگاه با `?IsGrooming=true` می‌گیرد. ادمین همه را می‌بیند. اسکریپت `DeactivateClinicGroomingServices.sql` ردیف‌های قدیمی کلینیک‌ها را غیرفعال می‌کند.

### ۴. تأیید آرایشگاه (ادمین) و پروفایل
ایجاد توسط ادمین یا مالک، تأیید/رد توسط ادمین (`GroomerApprove`) با اتمیک بودن (ExecuteUpdate) و پوش+SMS به مالک. تنظیمات: سرویس در آرایشگاه/در منزل، دریافت و تحویل (Pickup + PickupFee)، `MaxConcurrentPets`، پت‌های پذیرفته‌شده، محدوده‌ها (Zone)، ساعت کاری، گالری، پرسنل.

### ۵. دعوت پرسنل
مالک با موبایل دعوت می‌سازد (`UserAccept=null` و پوش+SMS `GroomerStaffInvite`). کاربر در `GET/PUT /api/EndUser/GroomerStaffInvite` می‌پذیرد یا رد می‌کند. فقط پذیرفته‌شده‌ها در پرسنل، `StaffCount` و تخصیص نوبت حساب می‌شوند. دعوت دوباره بعد از رد، قوانین را از `GroomerProfileService` بخوان.

### ۶. رزرو
- **Quote:** `GET /api/EndUser/GroomerReserve/Quote?packageIds=..&packageIds=..&pickupRequested=`؛ چندخدمتی تا ۵ بسته، هر خدمت یک‌بار، همه از یک آرایشگاه.
- **Availability:** `GET .../Availability?groomerId&date&packageId&assigneeUserId&extraPackageIds=` (ظرفیت از GroomerTime، `MaxConcurrency`، تداخل گرومر انتخابی و تداخل پت).
- **ایجاد:** `POST /api/EndUser/GroomerReserve` با `GroomerReserveInputDto` (`ExtraPackageIds`، `IsHomeService`، `AddressId`، `PickupRequested`، `AssigneeUserId`، `BookerNote`). رزرو پرداخت‌نشده ۱۵ دقیقه ظرفیت را نگه می‌دارد.
- **مبلغ:** آنلاین = پیش‌پرداخت (اگر 0<پیش‌پرداخت<قیمت وگرنه کل قیمت) + هزینه‌ی دریافت/تحویل؛ باقی‌مانده حضوری است و وارد حسابداری نمی‌شود.
- **پرداخت:** `GroomerReservePayment` با کد پرداخت `GRM...`؛ کد تخفیف (Rebate، `RebateType_GroomerReserve`)، کیف پول (`Wallets.GroomerReserveId`)، درگاه و callback (`PaymentCallbackTypeEnum.GroomerReserve`) با بررسی دوباره‌ی ظرفیت.
- **تأیید آرایشگاه:** بعد از پرداخت منتظر تأیید؛ مهلت ۱۲ ساعت (حداقل ۳۰ دقیقه، و تا ۳۰ دقیقه قبل از نوبت). رد یا بی‌پاسخی (Hangfire هر ۵ دقیقه `GroomerReserveExpireOverdue`) = لغو + برگشت کل مبلغ به کیف پول (ایدمپوتنت با `GroomerReserveRefund:{id}`).
- **لغو:** کاربر تا ۱۲ ساعت قبل از نوبت (یا تا قبل از تأیید) با برگشت کامل؛ بعد از آن بدون برگشت. ادمین با/بدون برگشت. بعد از شروع/تکمیل لغو نمی‌شود.
- **تغییر زمان:** `PUT Companion|Admin/GroomerReserve/Reschedule {id,newDoDate}` فقط روی اسلات آزاد؛ `RescheduleCount/OriginalDoDate/LastRescheduleDate`؛ پوش+SMS به مشتری (و اگر ادمین انجام داد، به آرایشگاه).
- **تکمیل:** پس از انجام خدمت؛ امتیاز/پوینت باشگاه؛ امکان ثبت نظر.
- **وضعیت‌ها:** `GroomerReserveStatusEnum`، `OwnerDecision` (۱=منتظر)، `IsReserved`، `IsCancel`، `CancelledByBooker` را از کد بنویس و جدول گذار وضعیت بکش.

### ۷. کمیسیون و تسویه
کمیسیون هر آیتم با درصد خدمت (یا پیش‌فرض آرایشگاه) در لحظه‌ی پرداخت ثبت می‌شود؛ تقسیم به تومان گرد می‌شود و جمع دقیقاً برابر مبلغ پرداخت است (`AllocateCommission`). تسویه فقط برای رزرو **تکمیل‌شده** با `SettlementKindEnum.Groomer=3` و `SettlementGroomers`. حسابداری ارائه‌دهنده سه استخر دارد (نمایندگی، فروشگاه، آرایشگاه: `GroomerPool`). ریز تسویه: `GET Admin/Settlement/{id}/Items` و `GET Companion/GroomerSettlement/{id}/Items` (`SettlementItemVDto`). گزارش مالی منبع `GroomerReserve` دارد.

### ۸. نظر (Review)
فقط بعد از رزرو تکمیل‌شده، یک‌بار برای هر رزرو، با تأیید ادمین؛ پاسخ آرایشگاه ممکن است. (`GroomerReviewSrv`)

### ۹. نمونه‌کار
پست‌های نمونه‌کار با چند عکس؛ لایک و کامنت کاربران (`EndUser/GroomerPortfolioLike|Comment`)، مدیریت توسط مالک و ادمین، نمایش عمومی در `Api/Controllers/GroomerPortfolioController.cs`.

### ۱۰. باشگاه (Pastil Club)
امتیاز `floor(PaymentPrice/10000)` با `ScoreTransactionType_GroomerReserve` بعد از پرداخت؛ پوینت با رویداد `GroomerReservationCompleted` هنگام تکمیل؛ هدف کوپن «آرایشگاه»؛ روش اعمال «رزرو آرایشگاه».

### ۱۱. اعلان‌ها
`GroomerNotifier`: پوش‌های `PushTypeEnum` ۱۲۶ تا ۱۳۶ و ۱۲ نوع SMS (`MessageTypeEnum.Groomer*`)؛ خطای اعلان رزرو را خراب نمی‌کند. توکن‌های SMS به کاوه‌نگار: `token`, `token2`, `token3`, و توکن چهارم به **`token10`** نگاشت می‌شود (کاوه‌نگار فقط `token/token2/token3/token10/token20` را می‌پذیرد). متن قالب‌ها در `GROOMER_FA.md` و `SeedGroomerSms.sql`؛ نام قالب = Label بدون زیرخط.

### ۱۲. مجوزها (ادمین)
بخش `Groomer` در `AdminPermissionCatalog`: GroomerActive، GroomerApprove، GroomerSetting، GroomerService، GroomerSchedule، GroomerUser، GroomerPicture، GroomerPortfolio، GroomerPortfolioComment، GroomerReview، و جدا `GroomerReserve`. مناطق Companion و EndUser با بررسی مالکیت در هر endpoint محافظت می‌شوند (فقط area «admin» با RolePermission کنترل می‌شود)؛ این را در مستند هر endpoint مشخص کن.

### ۱۳. پنل ادمین
لیست یکپارچه‌ی نمایندگی‌ها (`/admin/companion`) با منوی سه‌نقطه‌ی عملیات بر اساس نوع، مودال «افزودن نمایندگی» با ۶ نوع، صفحه‌های `/admin/groomer/create|edit-[id]|detail-[id]`، لیست رزروها `/admin/groomerreserve` (تأیید/رد/تکمیل/لغو/تغییر زمان)، پنل پرسنل (وضعیت دعوت)، خدمات، برنامه، گالری، نظرها، حسابداری ارائه‌دهنده با استخر آرایشگاه، ریز تسویه (کلیک روی تعداد آیتم)، تیک «خدمت گرومینگ» در ویرایش خدمت. **پنل هنوز در مرورگر تست نشده؛ فقط سینتکس بررسی شده.** این را در بخش محدودیت‌ها بنویس.

### ۱۴. دیپلوی (به ترتیب؛ کاربر خودش اجرا می‌کند)
`AddGroomers` → `AddGroomerReserves` → `MigrateCompanionGroomersToGroomers.sql` (اول `@Commit=0`) → `AddGroomerPhase3` → `AddAssistanceIsGrooming` → `SeedGroomerSms.sql` + ساخت ۱۲ قالب کاوه‌نگار → تیک «خدمت گرومینگ» در پنل → `DeactivateClinicGroomingServices.sql` (اول `@Commit=0`) → دیپلوی Api و Payment با هم → Permission Sync و دادن مجوزهای Groomer* → آپلود پنل. (وضعیت اعمال هر مرحله روی پروداکشن را از کاربر بپرس؛ در کد مشخص نیست.)

### ۱۵. محدودیت‌ها و تصمیم‌های شناخته‌شده
- وب‌اپ/اپ نماینده/اندروید برای آرایشگاه ساخته نشده؛ مستند فرانت لازم است.
- رزرو چندخدمتی یک اسلات پیوسته به اندازه‌ی جمع مدت‌ها می‌گیرد (بدون تقسیم بین چند گرومر).
- لایک‌های تک‌تک نظرهای قدیمی (CommentLikes) هنگام انتقال منتقل نمی‌شود، فقط شمارنده.
- بدون CI/CD؛ دیپلوی دستی.

## خروجی مورد انتظار
فایل‌های زیر را در `backend/Docs/` بساز (فارسی، Markdown، با فهرست مطالب و جدول‌های واضح):

1. **`GROOMER_COMPLETE_FA.md`** — مستند مرجع کامل: معماری، مدل داده (جدول ستون‌ها با نوع و توضیح و قید)، دیاگرام ER به شکل Mermaid، دیاگرام وضعیت رزرو و جریان پرداخت/تأیید/تسویه به شکل Mermaid، قوانین کسب‌وکار با اعداد دقیق (مهلت‌ها، بافرها، گرد کردن)، یکپارچگی‌ها، مجوزها، دیپلوی، محدودیت‌ها.
2. **`GROOMER_API_FA.md`** — مرجع API: برای **هر endpoint** در همه‌ی ناحیه‌ها (عمومی، EndUser، Companion، Admin): متد و مسیر، احراز هویت/مجوز، پارامترها، DTO ورودی با نوع هر فیلد، DTO خروجی، کدهای خطا/پیام‌های رایج (`Resource.Notification`)، مثال JSON درخواست/پاسخ. DTOها را از `Dto/*.cs` و سوگر `website/swagger.json` (اگر به‌روز بود) تطبیق بده.
3. **`GROOMER_FRONTEND_FLOWS_FA.md`** — برای هر اپ (وب‌اپ مشتری، اپ نماینده/آرایشگاه، پنل ادمین): فهرست صفحه‌ها، هر صفحه چه endpointهایی را به چه ترتیبی صدا می‌زند، حالت‌های خالی/خطا/بارگذاری، و متن‌های پیشنهادی فارسی. جریان‌های کلیدی: ثبت‌نام آرایشگاه تا تأیید، افزودن خدمت/بسته/ساعت کاری، دعوت پرسنل و پذیرش، مشتری: جستجو → جزئیات → انتخاب خدمت(ها) → زمان → Quote → ثبت → کد تخفیف/کیف پول → پرداخت → انتظار تأیید → وضعیت‌ها → لغو/تغییر زمان → نظر؛ آرایشگاه: صف تأیید، تأیید/رد، تغییر زمان، تکمیل، تسویه؛ نمونه‌کار (لایک/کامنت).
4. **`GROOMER_TESTING_AND_OPS_FA.md`** — چک‌لیست تست دستی (سناریوهای اصلی و لبه‌ای مثل لغو نزدیک نوبت، بی‌پاسخی آرایشگاه، تخفیف+کیف پول، چندخدمتی، تغییر زمان، دعوت رد‌شده)، چک‌لیست دیپلوی، عیب‌یابی SMS/پوش، کوئری‌های SQL نظارتی (فقط خواندنی).

## قواعد نوشتن
- هر مسیر فایل را با مسیر نسبی از ریشه‌ی مخزن بنویس.
- هر جا قانون عددی هست (مثلاً ۱۲ ساعت، ۱۵ دقیقه) نام ثابت/متدی که آن را تعریف می‌کند را کنارش بیاور (`GroomerReserveRules`).
- بخش «چیزهایی که مطمئن نیستم / در کد نیست» جدا بنویس؛ چیزی اختراع نکن.
- در پایان فایل‌های `docs/ai/FEATURE_MAP.md` را فقط در صورت لزوم برای لینک به مستندهای جدید به‌روز کن؛ کد را تغییر نده.
