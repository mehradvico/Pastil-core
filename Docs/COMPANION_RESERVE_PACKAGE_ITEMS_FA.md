# مدیریت تکی پکیج‌های رزرو (تأیید/لغو/افزودن) + بازگشت مبلغ به کیف پول — طراحی بک‌اند

> وضعیت: بک + پنل پیاده شد، **دیپلوی/اعمال SQL نشده**. مستندها: [نماینده/اپ](COMPANION_RESERVE_PACKAGE_ITEMS_AMIRMOHSEN_FA.md) · [وب‌اپ کاربر](COMPANION_RESERVE_PACKAGE_ITEMS_WEBAPP_FA.md)

## مسئله
یک رزرو می‌تواند چند پکیج داشته باشد (مثلاً ۱۰ تا) و پرداخت شده باشد. اگر نماینده فقط بعضی را بتواند در آن زمان ارائه بدهد، تا حالا فقط می‌شد کل رزرو را لغو کرد. حالا نماینده/ادمین هر پکیج را جدا **تأیید** یا **لغو** می‌کند؛ سهم پرداختیِ پکیج لغوشده **به کیف پول کاربر** برمی‌گردد، کاربر در وب‌اپ وضعیت هر پکیج را می‌بیند و لغو کل رزرو (روال قبلی) دست‌نخورده می‌ماند.

## مدل داده (بدون دست‌زدن به داده‌ی موجود)
جدول جدید `CompanionReservePackageItems` (migration `AddCompanionReservePackageItems`، فقط یک جدول جدید). رابطه‌ی چندبه‌چند قبلی `CompanionReserve.CompanionAssistancePackages` **تغییر نکرده**؛ جدول جدید فقط وضعیت/مبلغ هر پکیج را کنارش نگه می‌دارد.

| فیلد | معنی |
| --- | --- |
| CompanionReserveId, CompanionAssistancePackageId | کلید |
| PackageName, PetCount | snapshot نام و تعداد پت |
| Price | قیمت کامل پکیج برای همه‌ی پت‌ها (قبل از تخفیف) |
| PrePaymentPrice | **وزن** سهم پیش‌پرداخت (برای تقسیم بازپرداخت)؛ پکیجِ افزوده‌شده بعد از پرداخت = ۰ |
| Status | ۱ در انتظار تأیید · ۲ تأییدشده · ۳ لغوشده |
| StatusReason / StatusChangedDate / StatusChangedByUserId / StatusChangedByAdmin | چه کسی/چرا/کی |
| RefundAmount / RefundDate | مبلغ برگشتی به کیف پول |
| AddedAfterPayment | بعد از پرداخت اضافه شده |

**رزروهای قدیمی ردیف ندارند**: در نمایش، ردیف مجازی (`id=0`) ساخته می‌شود (وضعیت «در انتظار»، یا «تأییدشده» اگر اپراتور رزرو را «کامل» کرده، یا «لغوشده» اگر کل رزرو لغو شده) و روی اولین عملیات نوشتنی واقعاً ذخیره می‌شود. قیمت‌ها با همان `CompanionPackagePricing.Resolve` (حالت ارائه + تعداد پت) محاسبه می‌شوند.

## قواعد
**چه موقع قابل مدیریت است** (`CanManage`): رزرو لغو نشده، `DoneDate` خالی، اپراتور «کامل/لغو» نزده، و وضعیت رزرو «ثبت‌شده» یا «پیش‌پرداخت‌شده» (۳۰/۳۱) باشد؛ یعنی قبل از ثبت مبلغ نهایی خدمت.

**دسترسی**: ادمین (ناحیه‌ی Admin، با RolePermission کنترلر `CompanionReservePackage`) · مالک کلینیک · کارشناسِ تخصیص‌یافته با عضویت فعال. کاربر فقط می‌بیند.

**تأیید** `Pending → Approved` (فقط رزرو پرداخت‌شده). **لغو** `Pending|Approved → Cancelled` با دلیل اجباری (نمایش به کاربر). لغوشده نهایی است؛ می‌شود همان پکیج را دوباره «افزودن» کرد (ردیف جدید، تاریخچه می‌ماند).

**محاسبه‌ی بازپرداخت** (`CompanionReservePackageItemRules.RefundShare`): از مبلغ باقی‌مانده‌ی پرداخت‌شده‌ی رزرو (`PaymentPrice`، که پیش‌پرداختِ بعد از تخفیف است) متناسب با وزن پیش‌پرداخت پکیج در میان پکیج‌های فعال:
`refund = round(PaymentPrice × وزنِ این پکیج ÷ جمع وزن پکیج‌های فعال)` به تومان کامل.
نمونه‌ها: ۱۰ پکیج هم‌وزن با ۱٬۰۰۰٬۰۰۰ پرداختی، ۴ پکیج لغو ⇒ ۴۰۰٬۰۰۰ برمی‌گردد و ۶۰۰٬۰۰۰ می‌ماند (هر لغو از باقی‌مانده‌ی جاری حساب می‌شود پس ترتیب مهم نیست). با تخفیف: سهم بعد از تخفیف برمی‌گردد نه قیمت اسمی. **آخرین پکیج فعال** که لغو شود کل باقی‌مانده (شامل گرد‌شدن‌ها و هزینه‌ی روش آنلاین قدیمی) برمی‌گردد **و کل رزرو لغو می‌شود** (`IsCancel`، همان اثر لغو کل: برگشت امتیاز باشگاه + لغو سفر پت‌رسان متصل).

**اثر مالی روی رزرو** (با هر لغو): `PackagePrice -= Price`، `PrePaymentPrice` و `PaymentPrice` به‌اندازه‌ی بازپرداخت کم می‌شوند و `CompanionShare/SiteShare` دوباره با درصد کمیسیون محاسبه می‌شود (درآمد نماینده/پاستیل برای پکیج لغوشده حذف می‌شود). `WalletPrice` برای سابقه دست نمی‌خورد.

**بازپرداخت**: ردیف افزایشی در `Wallets` با نام یکتای `CompanionPackageRefund:{itemId}` (idempotent، `PaymentId=null` ⇒ در گزارش مالی خودکار «برگشتی به کیف پول» حساب می‌شود). همه‌چیز در یک تراکنش Serializable؛ اگر اعتبار کیف پول ثبت نشود کل لغو برمی‌گردد.

**افزودن** (`POST`): پکیج فعال همان خدمت، ارائه‌ی همان «نحوه ارائه»ی رزرو، خدمات تک‌پکیجی رد می‌شود. رزرو **پرداخت‌نشده**: قیمت و پیش‌پرداخت/کیف پول/کمیسیون بازمحاسبه می‌شود (بعد از شروع پرداخت قفل است). رزرو **پرداخت‌شده**: فقط `PackagePrice` زیاد می‌شود، پیش‌پرداخت اضافه‌ای گرفته نمی‌شود (وزن ۰؛ تفاوت در مبلغ نهایی خدمت تسویه می‌شود)، وضعیت «در انتظار».
**حذف** (`DELETE`): فقط رزرو پرداخت‌نشده (حداقل یک پکیج بماند)؛ برای پرداخت‌شده باید «لغو» شود.

**اعلان به کاربر**: پوش ۹۱ `PushCompanionReservePackageApproved` و ۹۲ `PushCompanionReservePackageCancelled` (لینک `/reserve/{شناسه‌ی رزرو}`؛ token1 نام پکیج، token3 شناسه‌ی رزرو، token4 مبلغ برگشتی). Pattern در `Pattern.resx/.fa.resx` و seed در migration `SeedCompanionReservePackagePushTypes`.

## چیزی که عمداً تغییر نکرد
- لغو کل رزرو (ادمین: `UpdateCancelDto`؛ اپراتور: وضعیت «لغو‌شده») مثل قبل است و **بازپرداخت ندارد**. اگر می‌خواهید آن مسیرها هم مبلغ را به کیف پول برگردانند، تصمیم محصولی جدا است.
- ویرایش پت‌ها توسط کاربر (`UpdateAsyncDto`) قیمت را از همه‌ی پکیج‌های رزرو بازمحاسبه می‌کند، شامل لغوشده‌ها؛ این مسیر فقط برای رزرو پرداخت‌نشده است (`!IsReserved`) و پکیج لغوشده‌ای ندارد.

## API
مشترک (`reserveId`، `packageId` = شناسه‌ی خود پکیج، نه ردیف):

| متد | مسیر | کار |
| --- | --- | --- |
| GET | `/api/EndUser/CompanionReservePackage/{reserveId}` | کاربر: فقط رزرو خودش |
| GET | `/api/Companion/CompanionReservePackage/{reserveId}` · `/api/Admin/...` | نماینده/ادمین |
| PUT | `…/{reserveId}/{packageId}` `{status:2}` یا `{status:3, reason}` | تأیید / لغو + بازپرداخت |
| POST | `…/{reserveId}` `{packageId}` | افزودن |
| DELETE | `…/{reserveId}/{packageId}` | حذف (فقط پرداخت‌نشده) |

علاوه بر این، `PackageItems` (همان ساختار GET) داخل جزئیات رزرو هم می‌آید: `GET /api/EndUser/CompanionReserve/{id}`، `GET /api/Companion/CompanionReserve/{id}` و جزئیات ادمین/اپراتور.

## فایل‌ها
`Entities/Entities/CompanionReservePackageItem.cs` · `Application/Services/CompanionSrvs/CompanionReservePackageItemSrv/*` (Service/Rules/Dto) · کنترلرها `Api/Areas/{Admin,Companion,EndUser}/Controllers/CompanionReservePackageController.cs` · `AdminPermissionCatalog` (`CompanionReservePackage`) · تست `Application.Tests/CompanionReservePackageItemRulesTests.cs` · پنل `components/companion-reserve/ReservePackageManager.vue` داخل `CompanionReserveDetailModal.vue`.

## دیپلوی
بکاپ ← `scripts/AddCompanionReservePackageItems.sql` ← `scripts/SeedCompanionReservePackagePushTypes.sql` ← Api ← «همگام‌سازی دسترسی‌ها» در پنل و دادن دسترسی `CompanionReservePackage` به نقش‌ها ← پنل ← اپ نماینده و وب‌اپ. (اسکریپت‌ها idempotent‌اند و به داده‌ی فعلی دست نمی‌زنند؛ رزروهای قدیمی ردیف نمی‌گیرند تا اولین عملیات.)
