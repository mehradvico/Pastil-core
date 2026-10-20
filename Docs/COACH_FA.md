# مربی (Coach) — جداسازی کامل مثل آرایشگاه و کلینیک (۱۴۰۵/۰۷/۲۹)

مربی‌ها (تربیت سگ، آموزش خصوصی، مشاوره‌ی رفتاری) حالا موجودیت و جریان مستقل دارند: **خدمات، بسته‌ها، ساعت کاری، پرسنل، رزرو، پرداخت، کیف پول، کد تخفیف، امتیاز باشگاه، تسویه (Provider + Kind=5)**. ساختار و قواعد عیناً مثل کلینیک است (`backend/Docs/CLINIC_FA.md` و `GROOMER_FA.md`)؛ فقط تفاوت‌های زیر را دارد.

> **مدرسه (School) و دوره‌ها دست‌نخورده می‌مانند.** دوره‌ی گروهی/ویدیویی هنوز روی `School`/`SchoolCourse` است و تسویه‌اش از مسیر Companion → Provider انجام می‌شود. «مربی» برای رزرو نوبتی خدمت/بسته است (جلسه‌ی خصوصی، مشاوره‌ی رفتاری). اگر مربی‌های فعلی (مثلاً قادرپناه، کی‌منش) باید به مدل جدید بروند، در پنل با «افزودن مربی» ساخته و به ارائه‌دهنده وصل می‌شوند؛ اسکریپت انتقال خودکار نداریم چون مدل دوره‌ای به خدمت/بسته نگاشت یک‌به‌یک ندارد.

## تفاوت با کلینیک
- **نحوه‌ی ارائه (Mode):** ۳۷ آنلاین، ۳۸ حضوری، ۳۹ در محل مشتری؛ قیمت هر حالت در `CoachServicePackageTypes` (قاعده‌ی بسته بدون ردیف = همه‌ی حالت‌ها با قیمت پایه).
- **بدون:** نوع مرکز (`CoachTypes`)، اقلام بسته‌ی حین ویزیت با وضعیت/برگشت، چت رزرو، تماس درون‌برنامه‌ای، نسخه‌ی آنلاین، سفر پت‌رسان، یادآور آنلاین. (در صورت نیاز در فاز بعد مثل کلینیک وصل می‌شود.) `CoachReserveItems` فقط برای رزرو چندخدمتی (تا ۵ بسته) است.
- **شناسه‌ها:** کد پرداخت `CCH...`، `PaymentType_CoachReserve=35`، `RebateType_CoachReserve=73`، `ScoreTransactionType_CoachReserve=77`، `SettlementKindEnum.Coach=5` (جدول `SettlementCoaches`)، `AgencyKindEnum.Coach=7`، پوش‌های ۱۵۴ تا ۱۶۴، باشگاه: رویداد ۱۱، منبع ۱۳، روش ۷، هدف ۱۵، کیف پول `Wallets.CoachReserveId`، خط دفتر `CoachReserveRefund:{id}`.
- **پیامک:** مثل کلینیک از قالب‌های Companion استفاده می‌شود (قالب جدید نداریم).

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
3. در پنل مربی بسازید و به ارائه‌دهنده وصل کنید (تسویه فقط با ارائه‌دهنده کار می‌کند).
