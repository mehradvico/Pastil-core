# مربی (Coach) — جداسازی کامل مثل آرایشگاه و کلینیک (۱۴۰۵/۰۷/۲۹)

مربی‌ها (تربیت سگ، آموزش خصوصی، مشاوره‌ی رفتاری) حالا موجودیت و جریان مستقل دارند: **خدمات، بسته‌ها، ساعت کاری، پرسنل، رزرو، پرداخت، کیف پول، کد تخفیف، امتیاز باشگاه، تسویه (Provider + Kind=5)**. ساختار و قواعد عیناً مثل کلینیک است (`backend/Docs/CLINIC_FA.md` و `GROOMER_FA.md`)؛ فقط تفاوت‌های زیر را دارد.

> **مدرسه (School) و دوره‌ها دست‌نخورده می‌مانند.** دوره‌ی گروهی/ویدیویی هنوز روی `School`/`SchoolCourse` است و تسویه‌اش از مسیر Companion → Provider انجام می‌شود. «مربی» برای رزرو نوبتی خدمت/بسته است (جلسه‌ی خصوصی، مشاوره‌ی رفتاری). 
## انتقال مربی‌های فعلی
مربی در Companion نوع مخصوص ندارد؛ پس `backend/scripts/MigrateCompanionCoachesToCoaches.sql` با شناسه‌ی خودتان کار می‌کند: `@CompanionIds` را (مثلاً قادرپناه و کی‌منش) پر کنید، با `@Commit = 0` اجرا و شمارش‌ها را ببینید، بعد `@Commit = 1`. خدمات، بسته‌ها (با حالت‌های آنلاین/حضوری/در محل)، ساعت کاری، پرسنل، محدوده، گونه‌ها و نظرات کپی می‌شوند؛ گرومینگ منتقل نمی‌شود؛ مدرسه و دوره‌ها اصلاً دخیل نیستند. با `@DeactivateLegacyServices = 1` خدمت‌های منتقل‌شده در Companion غیرفعال می‌شوند تا دو بار رزرو نشوند. روی LocalDB با تست `CoachMigrationScriptTests` راستی‌آزمایی شده است.

## تفاوت با کلینیک
- **نحوه‌ی ارائه (Mode) — مربی فقط سه حالت دارد:** ۳۸ **پانسیون آموزشی (در مرکز)**، ۳۹ در محل مشتری، ۳۷ آنلاین؛ قیمت هر حالت در `CoachServicePackageTypes` (قاعده‌ی بسته بدون ردیف = همه‌ی حالت‌ها با قیمت پایه).
- **بدون:** نوع مرکز (`CoachTypes`)، اقلام بسته‌ی حین ویزیت با وضعیت/برگشت، نسخه‌ی آنلاین و سفر پت‌رسان. `CoachReserveItems` فقط برای رزرو چندخدمتی (تا ۵ بسته) است.
- **شناسه‌ها:** کد پرداخت `CCH...`، `PaymentType_CoachReserve=35`، `RebateType_CoachReserve=73`، `ScoreTransactionType_CoachReserve=77`، `SettlementKindEnum.Coach=5` (جدول `SettlementCoaches`)، `AgencyKindEnum.Coach=7`، پوش‌های ۱۵۴ تا ۱۶۸، باشگاه: رویداد ۱۱، منبع ۱۳، روش ۷، هدف ۱۵، کیف پول `Wallets.CoachReserveId`، خط دفتر `CoachReserveRefund:{id}`.
- **پیامک:** مثل کلینیک از قالب‌های Companion استفاده می‌شود (قالب جدید نداریم).

## خدمات آنلاین (Mode=37)
- **چت رزرو:** `CoachReserveMessage` (+Attachment/Reaction) با کنترلرهای `CoachReserveMessage*`؛ فقط برای رزرو **آنلاین، پرداخت‌شده، لغونشده**؛ فقط رزروکننده/مالک/پرسنل مسئول (ادمین فقط می‌خواند). پوش ۱۶۵.
- **تماس درون‌برنامه‌ای (CallHub):** `JoinCoachCall(id)`، `SendCoachSignal(id,type,payload)`، `EndCoachReserveCall(id)`. کلید تماس = `2e15 + شناسه‌ی رزرو` (کلینیک `1e15+id`، رزرو Companion مثبت، جلسه‌ی آنلاین منفی). ورود از ۳۰ دقیقه قبل نوبت، فقط آنلاین و تأییدشده؛ `CallStartDate/CallEndDate` روی رزرو. پوش ۱۶۶ به رزروکننده.
- **یادآور:** با پرداخت رزرو آنلاین دو پوش زمان‌بندی‌شده (۱۰ دقیقه قبل=۱۶۷، سر ساعت=۱۶۸) به مربی؛ با تغییر زمان جابه‌جا و با لغو حذف می‌شوند.

## چند بسته از یک خدمت، مدت اقامت و جلسات
- یکتایی رزرو بر اساس **شناسه‌ی بسته** است: تا ۵ بسته‌ی متفاوت، حتی از یک خدمت (مثلاً چند دوره‌ی «مربی سگ»)؛ تکرار همان بسته رد می‌شود. همین قاعده برای آرایشگاه و کلینیک هم هست (و افزودن بسته بعد از پرداخت در کلینیک).
- روی ردیف قیمت هر Mode (`CoachServicePackageType`): `SessionCount` (Mode=۳۹) و برای Mode=۳۸ `StayDays` (تعداد روز پانسیون) و `DailyPrice` (پانسیون روزانه).

## پانسیون آموزشی (Mode=38، در مرکز — ۱۴۰۵/۰۸/۱۴، طرح نهایی)
پرچم مربی همان `ServiceInPerson` است. **مربی** چهار عدد تعیین می‌کند:
- هزینه‌ی دوره (`Price`)
- پیش‌پرداخت (`PrePaymentPrice`؛ حداکثر مبلغ کل)
- تعداد روز (`StayDays`؛ ۱ تا ۳۶۵، با پانسیون روزانه الزامی)
- پانسیون روزانه (`DailyPrice`)

جزئیات:
- **مبلغ:** کل = دوره + روزانه × روز؛ **آنلاین = فقط پیش‌پرداخت**؛ در مرکز = کل − پیش‌پرداخت (باقی‌مانده‌ی دوره + کل پانسیون).
- **مشتری** فقط دوره و زمان تحویل پت را انتخاب می‌کند (داخل ساعت کاری). `EndDate = DoDate + StayDays`. در چند بسته، تعداد روز = بیشینه‌ی بسته‌ها و هر بسته پانسیون خودش را دارد.
- **تشخیص اقامت:** رزرو Mode=۳۸ روی بسته‌ای که `StayDays` دارد. `CoachReserve.DailyPrice` پر (حتی صفر) نشانه‌ی اقامت است. ردیف ۳۸ بدون روز = نوبت ساعتی قدیمی.
- **ظرفیت:** `Coach.BoardingCapacity` (۰ = بدون سقف) جدا از ظرفیت ساعتی است. پت در کل اقامت مشغول است. ساعت تحویل گرفتن با ساعت کاری چک نمی‌شود.
- **لغو و بعد از آن:** لغو رایگان تا ۴۸ ساعت قبل از تحویل. تکمیل از لحظه‌ی تحویل مجاز است. تغییر زمان کل اقامت را جابه‌جا می‌کند.
- **API:**
  - `GET EndUser/CoachReserve/Quote?mode=38&packageIds=` (`basePrice`، `coursePrePayment`، `boardingFee`، `stayDays`، `dailyPrice`)
  - `GET EndUser/CoachReserve/BoardingAvailability?coachId=&packageId=&from=&days=` (`canStart`، `endDate`، `windows`)
  - `POST EndUser/CoachReserve` با `mode=38, doDate`
- **اسناد اپ:** `COACH_BOARDING_WEBAPP_FA.md` (مشتری) و `COACH_BOARDING_NEMAYANDE_APP_FA.md` (نماینده).
- **کد:** `CoachReserveRules` (Boarding*)، `CoachReserveService.IsStay/CheckBoardingAsync/GetBoardingAvailabilityAsync`، پنل `CoachServicesPanel.vue`؛ تست `CoachBoardingTests.cs`.

## مشاوره‌ی آنلاین فوری با شناسه‌ی مرکز («من آنلاینم»، بسته‌ی چت/تماس)
- سامانه‌ی مشاوره (`ConsultationPackage/Purchase/Availability`، جلسه، کیف پول، CallHub) برای مربی و کلینیک **مستقیماً روی خودِ مرکز** کلید می‌خورد (`CoachId` / `ClinicId`؛ هر ردیف دقیقاً یک مالک دارد: `CompanionId` یا `ClinicId` یا `CoachId`) و دیگر Companion لنگری وجود ندارد. «آنلاینم» روی `CoachUsers/ClinicUsers` (تیم خود مرکز) ذخیره می‌شود. جزئیات و مسیر دیپلوی: `CONSULTATION_VENUE_OWNERS_FA.md`.
- مشتری: `GET /api/Coach/{id}/ConsultationPackages` و `GET /api/Clinic/{id}/ConsultationPackages` (همان قالب بسته‌ی قدیمی: id، channelId، durationMinutes، price، name، description، picture، companionOnline، bookable). خرید با `POST /api/EndUser/ConsultationPurchase` و `packageId`؛ جلسه/چت/تماس مثل قبل.
- مالک/عضو فعال: `GET/POST/PUT/DELETE /api/Companion/CoachConsultationPackage?coachId=` (و `ClinicConsultationPackage?clinicId=`)، `GET/PUT /api/Companion/CoachConsultationOnline` (و `ClinicConsultationOnline`) با بدنه‌ی `{coachId|clinicId, online}`، و برنامه‌ی رزرو `GET/PUT /api/Companion/CoachConsultationAvailability?coachId=` (و `ClinicConsultationAvailability?clinicId=`).
- `ServiceOnline` در پاسخ عمومی فقط وقتی true است که یا بسته‌ی رزرو آنلاین (Mode=۳۷) رزروشدنی داشته باشد یا بسته‌ی مشاوره‌ی فوری فعال با قیمت مثبت.
- تسویه‌ی مشاوره‌ی «تکمیل‌شده» در استخر **همان ارائه‌دهنده‌ی مربی/کلینیک** (نوع ۵ / ۴) انجام می‌شود (`SettlementConsultation`)، نه استخر نمایندگی قدیمی.

## فلو
رزرو ← پرداخت (کد تخفیف/کیف پول/درگاه) ← منتظر تأیید مربی (۱۲ ساعت؛ رد یا بی‌پاسخی = لغو + برگشت کل مبلغ به کیف پول، job `CoachReserveExpireOverdue` هر ۵ دقیقه) ← تکمیل ← امتیاز باشگاه ← نظر ← تسویه با ارائه‌دهنده (استخر مستقل «مربی‌ها»).

## API (همان الگوی کلینیک با نام Coach)
- عمومی: `GET /api/Coach`، `/api/Coach/{id}`، `/{id}/Services`، `/Times`، `/Zones`، `/Pictures`، `GET /api/CoachReview`.
- مشتری: `EndUser/CoachReserve` (Quote/Availability/Cancel)، `EndUser/CoachReservePayment`، `EndUser/CoachReview`، `EndUser/CoachStaffInvite`.
- مالک (Companion area): `Coach`، `CoachService` (+`/Package` با `Types[]`)، `CoachSchedule`، `CoachUser`، `CoachPicture`، `CoachReserve`، `CoachSettlement`، `CoachReview`.
- ادمین: همان‌ها + `CoachActive/Approve/Setting`؛ مجوزها در `AdminPermissionCatalog` (Coach و CoachReserve). وصل‌کردن به ارائه‌دهنده: `ProviderVenue` (Assign مربی).
- پنل: `/admin/coach/*`، `/admin/coachreserve`، استخر «مربی‌ها» در `providerfinance`.

## دیپلوی (خودتان اجرا کنید)
1. `scripts/AddCoaches.sql` (بعد از `AddClinicReserveLinks`؛ جدول‌ها، `Wallets.CoachReserveId`، Codeهای پرداخت/تخفیف/امتیاز و پوش‌های ۱۵۴–۱۶۴).
2. دیپلوی Api و Payment با هم، Permission Sync و دادن مجوزهای `Coach*`، آپلود پنل.
3. برای چند بسته/مدت اقامت: `scripts/AddCoachStayDays.sql` (بعد از AddCoachOnline).
4. برای خدمات آنلاین: `scripts/AddCoachOnline.sql` (بعد از AddCoaches؛ جدول‌های چت، ستون‌های تماس، پوش ۱۶۵–۱۶۸).
4. در پنل مربی بسازید و به ارائه‌دهنده وصل کنید (تسویه فقط با ارائه‌دهنده کار می‌کند).
5. پانسیون آموزشی: `scripts/CoachBoarding.sql` (اجرا شده) و بعد `scripts/CoachBoardingSimplify.sql` (حذف ستون‌های بی‌استفاده‌ی MinDays/MaxDays/ServiceBoarding و افزودن `CoachServicePackageTypes.DailyPrice`) قبل از دیپلوی Api و Payment.
