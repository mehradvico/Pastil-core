# جداسازی کامل مشاوره‌ی آنلاین از Companion (کلینیک و مربی مستقل)

**وضعیت (۱۴۰۵/۰۸/۰۲ ≈ 2026-10-24):** کد و تست آماده است؛ **مایگریشن `VenueConsultationOwners` روی دیتابیس اجرا نشده** و چیزی دیپلوی نشده است.

## چه چیزی عوض شد
قبلاً سامانه‌ی مشاوره (بسته، خرید، جلسه، «آنلاینم»، برنامه‌ی رزرو، تسویه) فقط با `CompanionId` کار می‌کرد و برای کلینیک/مربی مستقل یک Companion «لنگر» پنهان ساخته می‌شد. حالا:

| لایه | قبل | بعد |
|---|---|---|
| `ConsultationPackage` / `ConsultationPurchase` / `ConsultationAvailability` | `CompanionId` الزامی | `CompanionId` / `ClinicId` / `CoachId` همگی nullable؛ **دقیقاً یکی** پر (CHECK `CK_ConsultationPackages_Owner` و مشابه) |
| «من آنلاینم» | `CompanionUsers.ConsultationOnline` | برای کلینیک/مربی: `ClinicUsers` / `CoachUsers` (فیلدهای `ConsultationOnline`, `ConsultationOnlineExpiresAt`) |
| صف نماینده (شروع/تخصیص/ورود) | مالک یا عضو Companion | مالک یا عضو فعال و تأییدشده‌ی همان مرکز (کلینیک/مربی/نمایندگی؛ یک نماینده می‌تواند در هر سه باشد) |
| تسویه | استخر Companion (`SettlementCompanion.ConsultationPurchaseId`) | کلینیک/مربی: استخر ارائه‌دهنده‌ی خودشان با جدول `SettlementConsultations` (ایندکس یکتا روی خرید) |
| Companion لنگر | ساخته می‌شد | **دیگر ساخته نمی‌شود** |

نمایندگی قدیمی (Companion) هیچ تغییر رفتاری ندارد.

## کد
- کلید مشترک: `Application/Services/ConsultationSrvs/ConsultationOwnerSrv/ConsultationOwner.cs` (`ConsultationOwner(Kind, Id)`، فیلترهای `ForOwner`، `Assign`، `IsOpenAsync`، `AgentUserIdsAsync`، `OwnerIdsOfAgentAsync`).
- همه‌ی سرویس‌های مشاوره (Package/Online/Booking/Purchase/Session/Notification/Admin) به‌جای `companionId` یک `ConsultationOwner` می‌گیرند.
- `VenueConsultationService`: احراز دسترسی (مالک یا عضو تیم؛ نوشتن فقط برای مرکز فعال و تأییدشده) و واگذاری؛ دیگر Companion نمی‌سازد.
- مالی: `SettlementSources.{Clinic|Coach}ConsultationPurchases`، `ProviderFinanceQueries.{Clinic|Coach}ConsultationGroups`، لیست «در انتظار تسویه» و جزئیات تسویه (ردیف `Type = "Consultation"` با نام مرکز).
- پرداخت/کد تخفیف/کیف پول بدون تغییر: خرید همچنان با `PaymentCallbackTypeEnum.ConsultationPurchase` و `packageId` ساخته می‌شود و مالک از روی پکیج کپی می‌شود.

## API
**عمومی (کلاینت مشتری)**
- `GET /api/Clinic/{id}/ConsultationPackages` و `GET /api/Coach/{id}/ConsultationPackages` (بدون تغییر قالب؛ فیلد `companionId` برای این‌ها null و `clinicId`/`coachId` پر است).
- `GET /api/EndUser/ConsultationPackage?companionId|clinicId|coachId=` و `GET /api/EndUser/ConsultationOnline?companionId|clinicId|coachId=` (دقیقاً یکی از سه پارامتر).
- خرید: `POST /api/EndUser/ConsultationPurchase` با `packageId` — بدون تغییر. در پاسخ‌های خرید/جلسه `companionId` nullable شد و `clinicId` / `coachId` اضافه شد؛ `companionName` همچنان نام مرکز است.
- لینک نتیجه‌ی جستجوی سراسری: `/consultations/buy?clinicId=…&packageId=…` (یا `coachId=` / `companionId=`).
- `GET /api/EndUser/ConsultationBooking/Days|Slots` بدون تغییر (با `packageId`).

**مالک/عضو مرکز**
- پکیج‌ها: `/api/Companion/{Clinic|Coach}ConsultationPackage?{clinic|coach}Id=` (GET/POST/PUT/DELETE).
- آنلاینم: `/api/Companion/{Clinic|Coach}ConsultationOnline` (GET با `?{clinic|coach}Id=`، PUT با بدنه `{clinicId|coachId, online}`).
- **جدید** برنامه‌ی رزرو: `GET/PUT /api/Companion/{Clinic|Coach}ConsultationAvailability?{clinic|coach}Id=` (همان بدنه‌ی `ConsultationAvailability` قدیمی).
- صف نماینده (`/api/Companion/ConsultationSession…`) خودکار شامل مشاوره‌های کلینیک/مربی‌هایی است که کاربر مالک/عضو آن‌هاست؛ آیتم‌ها `clinicId`/`coachId` دارند.

**ادمین (نیاز به Permission Sync)**
- `GET /api/Admin/ConsultationVenue/{clinic|coach}?q=` فهرست مرکزها با خلاصه‌ی بسته‌ها.
- `GET|DELETE /api/Admin/ConsultationVenue/{kind}/{venueId}/Packages`.
- فیلتر خریدها: `GET /api/Admin/ConsultationPurchase?ClinicId=|CoachId=`.
- درصد سهم سایت هر بسته: `PUT /api/Admin/FinanceConsultationPackage` (بدون تغییر). پنل: `/admin/consultationvenue`.

## ترتیب دیپلوی (مهم)
1. `backend/scripts/VenueConsultationOwners.sql` — ستون‌های مالک و CHECK، ستون‌های آنلاین تیم، جدول `SettlementConsultations`. روی کد قدیمی بی‌خطر است (هیچ ردیفی هنوز NULL نیست).
2. دیپلوی **Api و Payment** نسخه‌ی جدید (هر دو، چون `ConsultationPurchase` را می‌خوانند).
3. بلافاصله `backend/scripts/MoveVenueConsultationToOwners.sql` — ابتدا `@Commit = 0` و بررسی شمارش‌ها، سپس `1`. تا اجرای این اسکریپت، بسته‌ها/خریدهای کلینیک و مربی‌های موجود (که هنوز روی لنگر Companion‌اند) در لیست‌های مرکز دیده نمی‌شوند. اگر اسکریپت ۱ اجرا شود ولی ۲ نه، و ۳ اجرا شود، **کد قدیمی روی ردیف‌های CompanionId = NULL خطا می‌دهد** — ترتیب را رعایت کنید.
4. Permission Sync در پنل، سپس آپلود پنل.

## تسویه‌ی خریدهای قدیمی
- خرید تکمیل‌شده‌ی تسویه‌نشده‌ی مرکز: بعد از اسکریپت ۳ در تسویه‌ی بعدیِ **استخر کلینیک/مربی** همان ارائه‌دهنده می‌آید.
- خرید قبلاً تسویه‌شده (`Permitted = 1`) دست نمی‌خورد و در تسویه‌ی قبلی (`SettlementCompanions`) می‌ماند.
- اگر Provider مرکز با Provider لنگر قدیمی فرق داشته باشد، سهم به Provider مرکز می‌رسد (رفتار درست).

## کار باقی‌مانده / تصمیم‌ها
- Companionهای لنگر خودکار خالی می‌مانند (پنهان با `CompanionMigrationFilter`)؛ پاک نمی‌شوند تا سابقه‌ی مالی/ارجاع‌ها از بین نرود.
- وب‌اپ/اپ‌ها باید پارامتر `clinicId`/`coachId` را در صفحه‌ی خرید مشاوره (`/consultations/buy`) و صف نماینده بپذیرند؛ فقط سند است، کدنویسی وب‌اپ جداست.
- گزارش مالی کلی (`FinanceReport`) و آمار ادمین مشاوره (`SearchPurchases`) از قبل روی همه‌ی خریدها کار می‌کنند و دست نخوردند.
