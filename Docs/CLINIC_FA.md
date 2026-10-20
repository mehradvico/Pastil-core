# کلینیک (Clinic) — جداسازی از Companion (۱۴۰۵/۰۷/۲۹)

کلینیک (نوع‌های کلینیک ۴۰، آزمایشگاه ۴۲، پرستار ۴۴) از `Companion` جدا شد و مثل آرایشگاه موجودیت و جریان مستقل دارد: **خدمات، بسته‌ها، ساعت کاری، پرسنل، رزرو، پرداخت، کیف پول، کد تخفیف، امتیاز باشگاه، تسویه (Provider + Kind=4)**. مدل و قواعد رزرو عیناً مثل آرایشگاه است (`backend/Docs/GROOMER_FA.md`)؛ فقط تفاوت‌های زیر را دارد.

## تفاوت با آرایشگاه
- **نحوه‌ی ارائه (Mode):** ۳۷ آنلاین، ۳۸ حضوری در کلینیک، ۳۹ در محل مشتری (هم‌ارز `CompanionAssistanceTypeEnum`). قیمت و پیش‌پرداخت هر بسته به‌ازای هر حالت در `ClinicServicePackageTypes` است؛ بسته‌ی بدون ردیف همه‌ی حالت‌ها را با قیمت پایه ارائه می‌دهد؛ بسته‌ی دارای ردیف فقط حالت‌های لیست‌شده را (قیمت پایه = ارزان‌ترین). پرچم‌های کلینیک: `ServiceOnline`، `ServiceInPerson`، `ServiceAtHome`.
- **نوع مرکز:** `ClinicTypes` (۴۰/۴۲/۴۴؛ خالی = ۴۰). فیلتر `TypeId` در جستجو.
- **بدون** دریافت/تحویل (Pickup) و بدون نمونه‌کار (Portfolio).
- **خدمات گرومینگ** (`Assistances.IsGrooming = 1`) برای کلینیک ساخته/دیده/رزرو نمی‌شود (`ClinicServiceIsGrooming`).
- کد پرداخت `CLN...`، `PaymentType_ClinicReserve=34`، `RebateType_ClinicReserve=72`، `ScoreTransactionType_ClinicReserve=76`، `SettlementKindEnum.Clinic=4`، پوش‌های ۱۳۷ تا ۱۴۷، باشگاه: رویداد ۱۰، منبع ۱۲، روش ۶، هدف ۱۴.
- **پیامک:** قالب جدید نداریم. از همان قالب‌های Companion استفاده می‌شود: ثبت/پرداخت رزرو `CompanionReserveForUser` (token = نام خدمت) و `CompanionReserveForCompanion` (token = نام خدمت، token2 = نام مشتری)؛ تغییر زمان `CompanionReserveRescheduledUser` (مرکز، تاریخ قبلی، تاریخ جدید، ساعت جدید، علت). بقیه‌ی رویدادها (تأیید/رد/انقضا/لغو/دعوت پرسنل) فقط پوش دارند.

## فلو (خلاصه)
رزرو → پرداخت (کد تخفیف/کیف پول/درگاه) → منتظر تأیید کلینیک (۱۲ ساعت؛ رد یا بی‌پاسخی = لغو + برگشت کل مبلغ به کیف پول، job `ClinicReserveExpireOverdue` هر ۵ دقیقه) → تکمیل → امتیاز/پوینت باشگاه → نظر → تسویه با ارائه‌دهنده. لغو کاربر تا ۱۲ ساعت قبل رایگان. تغییر زمان توسط کلینیک/ادمین. رزرو چندخدمتی تا ۵ بسته. دعوت پرسنل با پذیرش کاربر.

## API
- عمومی: `GET /api/Clinic`، `/api/Clinic/{id}`، `/{id}/Services?petSize=`، `/Times`، `/Zones`، `/Pictures`، `GET /api/ClinicReview`.
- مشتری: `GET/POST EndUser/ClinicReserve`، `GET .../Quote?packageIds=&mode=` (mode الزامی)، `GET .../Availability?clinicId&date&packageId&extraPackageIds`، `PUT .../Cancel?id=`، `EndUser/ClinicReservePayment` (Rebate/Wallet/پرداخت)، `EndUser/ClinicReview`، `EndUser/ClinicStaffInvite`.
- ورودی رزرو: `ClinicReserveInputDto { ClinicId, ClinicServicePackageId, ExtraPackageIds[], UserPetId, DoDate, Mode, AddressId?, AssigneeUserId?, BookerNote }` (`AddressId` فقط برای Mode=۳۹).
- مالک (Companion area): `Clinic`، `ClinicService` (+`/Package` با `Types[]`)، `ClinicSchedule`، `ClinicUser`، `ClinicPicture`، `ClinicReserve` (Approve/Reject/Reschedule/Complete)، `ClinicSettlement`، `ClinicReview`.
- ادمین: همان خانواده‌ها + `ClinicActive/ClinicApprove/ClinicSetting` و `ClinicReserve` (لغو با/بی بازگشت وجه). مجوزها در `AdminPermissionCatalog` (بخش Clinic و ClinicReserve).

## دیپلوی (به ترتیب؛ خودتان اجرا کنید)
1. `AddClinics` (migration یا `scripts/AddClinics.sql`؛ جدول‌ها، ستون `Wallets.ClinicReserveId`، Codeهای پرداخت/تخفیف/امتیاز و پوش‌های ۱۳۷–۱۴۷).
2. خدمات گرومینگ را در پنل علامت بزنید (`IsGrooming`) و `scripts/DeactivateClinicGroomingServices.sql` را اجرا کنید (پیش‌نیاز انتقال تمیز).
3. `scripts/MigrateCompanionClinicsToClinics.sql` با `@Commit = 0` و بعد `1` (روی LocalDB با تست `ClinicMigrationScriptTests` راستی‌آزمایی شده). Companion شخصی منتقل نمی‌شود؛ رزروهای قدیمی روی Companion می‌مانند.
4. دیپلوی Api و Payment با هم، Permission Sync و دادن مجوزهای Clinic*، آپلود پنل.

انتقال‌یافته‌ها از لیست‌های عمومی نماینده/کلینیک، «نزدیک من» و جستجوی سراسری مخفی می‌شوند (`CompanionMigrationFilter`)؛ تا اتصال اپ‌ها به `GET /api/Clinic` کارت‌های کلینیک در اپ‌های فعلی (که از Companion می‌خوانند) خالی می‌شود.

## اتصال به رزرو کلینیک (فاز ۴)
- **اقلام بسته (PackageItems):** `ClinicReserveItem` با وضعیت (در انتظار/تأییدشده/لغو)، تأیید یا لغو توسط کلینیک/ادمین، برگشت سهم پرداختی به کیف پول (`ClinicPackageRefund:{itemId}`)، پوش ۱۴۸/۱۴۹. کنترلر `ClinicReservePackage` در سه area (Admin/Companion/EndUser).
- **نسخه‌ی آنلاین:** `OnlinePrescription.ClinicReserveId` (یک نسخه به‌ازای هر رزرو کلینیک؛ ایندکس یکتای فیلتردار)؛ مقصد رزرو کلینیک در `PrescriptionSrv`.
- **سفر پت‌رسان (Mode=۳۹ در محل):** `Trip.ClinicReserveId`؛ ساخت از `EndUser/TripClinicReservation`؛ با تغییر زمان/لغو رزرو، سفر هم‌زمان جابه‌جا/لغو می‌شود و راننده پوش می‌گیرد.
- **چت رزرو:** `ClinicReserveMessage` (+Attachment/Reaction) با کنترلرهای `ClinicReserveMessage*`؛ فقط برای رزرو **آنلاین، پرداخت‌شده، لغونشده** و فقط رزروکننده/مالک/پرسنل مسئول (ادمین فقط می‌خواند). پیام سیستمی را کلاینت نمی‌تواند بفرستد. پوش ۱۵۰.
- **تماس درون‌برنامه‌ای (CallHub):** متدهای `JoinClinicCall(id)`، `SendClinicSignal(id,type,payload)`، `EndClinicReserveCall(id)`. کلید تماس = `1e15 + شناسه‌ی رزرو` تا با رزرو Companion (مثبت) و جلسه‌ی آنلاین (منفی) تداخل نکند. ورود از ۳۰ دقیقه قبل از نوبت، فقط Mode=آنلاین و تأییدشده. `CallStartDate/CallEndDate` روی رزرو ثبت می‌شود. پوش ۱۵۱ به رزروکننده.
- **یادآور آنلاین:** با پرداخت رزرو آنلاین دو پوش زمان‌بندی‌شده (۱۰ دقیقه قبل=۱۵۲، سر ساعت=۱۵۳) به پزشک/کلینیک؛ با تغییر زمان جابه‌جا و با لغو حذف می‌شوند (`ClinicReserveReminders`).
- مایگریشن `AddClinicReserveLinks` (اسکریپت `scripts/AddClinicReserveLinks.sql`، idempotent): ستون‌های جدید، ۳ جدول چت، کلیدها/ایندکس‌ها و سید پوش ۱۴۸–۱۵۳.

## دیپلوی فاز ۴
۱) `scripts/AddClinicReserveLinks.sql` (پس از AddClinics) ۲) دیپلوی Api و Payment با هم ۳) آپلود پنل.

## محدودیت‌ها (هنوز وصل نشده)
بدهی اپراتور، بیمه و رزرو گروهی چندپتی به ClinicReserve وصل نشده‌اند. `ClinicReserveVDto` اقلام بسته را embed نمی‌کند (از کنترلر `ClinicReservePackage` خوانده شود). جریان ساخت سفر از رزرو کلینیک تست یکپارچه ندارد.
