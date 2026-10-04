# جستجوی پت با میکروچیپ + درخواست پیگیری توسط پاستیل (طراحی بک‌اند)

> وضعیت: پیاده شد (بک + پنل)، **دیپلوی/اعمال SQL نشده**. مستند اپ/سایت: [PET_MICROCHIP_LOOKUP_APP_FA.md](PET_MICROCHIP_LOOKUP_APP_FA.md)

## هدف
در بخش «ارتباط با ما» کاربر (حتی مهمان) کد میکروچیپ را می‌زند. اگر در دیتابیس پاستیل پتی با این کد باشد، **پروفایل عمومی پت** نشان داده می‌شود؛ **بدون** شماره/نام/ایمیل/شناسه‌ی مالک و بدون اطلاعات درمانی. زیر پروفایل دکمه‌ی «درخواست پیگیری توسط پاستیل» است؛ با زدن آن مشخصات تماس خود جستجوگر ثبت و در پنل ادمین صف می‌شود. ادمین مالک را می‌بیند و خودش واسطه می‌شود (پاستیل هرگز اطلاعات مالک را مستقیم نمی‌دهد).

## موجودیت: `PetMicrochipRequests`
یک ردیف برای هر جستجو (حتی پیدا‌نشده؛ برای ممیزی/تشخیص سوءاستفاده). فایل: `Entities/Entities/PetMicrochipRequest.cs`، migration `AddPetMicrochipRequest`، اسکریپت idempotent: `backend/scripts/AddPetMicrochipRequest.sql` (فقط یک جدول جدید؛ به داده‌ی فعلی دست نمی‌زند).

| فیلد | توضیح |
| --- | --- |
| MicrochipCode | فقط رقم (نرمال‌شده) |
| PublicToken | یکتا؛ فقط وقتی پت پیدا شده؛ ۲۴ ساعت اعتبار؛ بدون آن «پیگیری» ممکن نیست |
| UserId / ClientIp | کاربر لاگین‌شده (اختیاری) / IP |
| FoundUserPetId, MatchCount | پت پیدا‌شده (جدیدترین اگر چند پت یک کد داشتند) و تعداد تطبیق |
| FollowUpRequested/Date, FullName, Mobile, Email, Message | مشخصات **جستجوگر** هنگام درخواست پیگیری |
| Status | ۱ جستجو‌شده · ۲ درخواست پیگیری · ۳ در حال پیگیری · ۴ حل‌شده · ۵ بسته |
| AdminNote, HandledByUserId, ClosedDate | رسیدگی ادمین |

## قواعد
- **نرمال‌سازی میکروچیپ**: ارقام فارسی/عربی→انگلیسی، حذف فاصله/خط‌تیره/نقطه/نیم‌فاصله؛ هر کاراکتر دیگر یا طول خارج از ۹–۱۵ ⇒ خطا. جستجو روی `UserPet.MicroChipCode` (با حذف فاصله/خط‌تیره سمت دیتابیس) و `!Deleted`.
- **پروفایل عمومی** (`PetMicrochipPublicPetVDto`): نام پت، نوع، نژاد (و نژاد دوم برای مخلوط)، جنسیت، عقیم‌شدگی، سن (ماه)، سایز، وزن، عکس اصلی و حداکثر ۶ عکس. **نیست**: مالک، SpecificDisease، SpecificMedicene، آدرس، شناسه‌ی UserPet.
- **درخواست پیگیری**: توکن لازم و معتبر (۲۴ ساعت)، یک‌بار مصرف (دوباره ⇒ «قبلاً ثبت شده»)؛ نام و موبایل معتبر لازم (لاگین‌شده‌ها از پروفایل پر می‌شود)؛ سقف ۵ درخواست پیگیری در ۲۴ ساعت برای هر موبایل.
- **ادمین**: انتقال‌ها: ۲→{۳،۴،۵}، ۳→{۴،۵}، ۴→۳، ۵→۳ (`PetMicrochipRules.AllowedNext`).
- **ضد حدس‌زدن انبوه**: rate-limit «MicrochipLookup» = ۱۰ درخواست/۱۰ دقیقه برای هر IP (روی هر دو endpoint عمومی). پاسخ «پیدا نشد» عمداً HTTP 200 و `isSuccess:true` با `found:false` است.

## API
عمومی (`AllowAnonymous`؛ اگر Bearer باشد کاربر شناخته می‌شود):
- `POST /api/PetMicrochip/Search` — `{ microchipCode }`
- `POST /api/PetMicrochip/FollowUp` — `{ token, fullName, mobile, email?, message? }`

ادمین (`Authorize` + RolePermission با نام کنترلر `PetMicrochipRequest` در `AdminPermissionCatalog` — بعد از دیپلوی «همگام‌سازی دسترسی‌ها» و دادن دسترسی به نقش‌ها):
- `GET /api/Admin/PetMicrochipRequest` (فیلتر `status`, `includeSearches`, `q`/`microchipCode`, صفحه‌بندی؛ پیش‌فرض فقط پیگیری‌ها)
- `GET /api/Admin/PetMicrochipRequest/{id}` (پت + **مالک** + درخواست‌دهنده + `allowedNextStatuses`)
- `PUT /api/Admin/PetMicrochipRequest/{id}` — `{ status, adminNote }`

## فایل‌ها
`Application/Services/Accounting/PetMicrochipSrv/*`، `Api/Controllers/PetMicrochipController.cs`، `Api/Areas/Admin/Controllers/PetMicrochipRequestController.cs`، policy در `Api/Program.cs`، DI در `ConfigureServices.cs`، پیام‌ها `Resource.Notification.Microchip*`، تست `Application.Tests/PetMicrochipRulesTests.cs`. پنل: `store/useAdminPetMicrochipStore.ts`، `pages/admin/petmicrochiprequest/{index,[id]}.vue`.

## دیپلوی
۱) بکاپ ← اجرای `scripts/AddPetMicrochipRequest.sql` ← دیپلوی Api ← در پنل «همگام‌سازی دسترسی‌ها» ← دسترسی «PetMicrochipRequest» را به نقش‌های پشتیبانی بدهید ← دیپلوی پنل.
۲) **اعلان/پوش ادمین**: با ثبت «درخواست پیگیری» یک Notice با Label `PetMicrochip.FollowUpRequested` (Importance=3 ⇒ realtime پنل + پوش به ادمین‌ها) ساخته می‌شود و لینک به `/admin/petmicrochiprequest/{id}` دارد (dedupe با شناسه‌ی درخواست؛ شکست اعلان ثبت را خراب نمی‌کند). نیازمند اجرای `scripts/AddPetMicrochipFollowUpNotice.sql` (مایگریشن `SeedPetMicrochipFollowUpNotice`) **بعد از** اسکریپت اول.

## یادداشت حریم خصوصی
گرچه مالک دیده نمی‌شود، عکس/نام/نژاد پت می‌تواند نشانه‌ی هویتی باشد؛ به همین دلیل هیچ اطلاعات درمانی و موقعیتی نشان داده نمی‌شود و همه‌ی جستجوها (IP/زمان/کاربر) ثبت می‌شوند.
