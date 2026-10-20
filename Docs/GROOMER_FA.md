# آرایشگاه (Groomer) — جداسازی از Companion (۱۴۰۵/۰۷/۲۸، فاز ۱)

## وضعیت
- **فاز ۱ ساخته و تست شده:** جدول‌ها، مایگریشن `AddGroomers`، اسکریپت انتقال داده، سرویس‌ها، کنترلرها، نمونه‌کار اینستاگرامی (لایک و کامنت)، لیست یکپارچه‌ی نمایندگی‌ها.
- **فاز ۲ ساخته شد:** GroomerReserve + پرداخت + کیف پول + کد تخفیف + تأیید/رد با برگشت وجه + تسویه + نظرها (پایین).
- **پنل ساخته شد:** ساخت/ویرایش/جزئیات آرایشگاه (خدمات، ساعت کاری و محدوده، پرسنل، گالری و نمونه‌کار، نظرات)، منوی سه‌نقطه در «نمایندگی‌ها»، صفحه‌ی «رزروهای آرایشگاه» و استخر آرایشگاه در حسابداری ارائه‌دهنده.

## جدول‌ها
`Groomers` (ProviderId، OwnerId، LegacyCompanionId، فیلدهای جدید)، `GroomerUsers` (پرسنل)، `GroomerServices` (خدمت از کاتالوگ Assistance)، `GroomerServicePackages` (+ `…Pictures`)، `GroomerServiceUsers`، `GroomerTimes`، `GroomerZones`، `GroomerPets`، `GroomerPictures`، `GroomerComments`، `GroomerPortfolioPosts/Pictures/Likes/Comments`.

فیلدهای تازه‌ی آرایشگاه: Description/Regulations، EstablishedYear، LicenseNumber/LicenseApproved، Instagram/Website، ServiceAtHome/ServiceInSalon، PickupAvailable/PickupFee، HasParking/WaitingRoom/Camera، MaxConcurrentPets، StaffCount، PortfolioCount/PortfolioLikeCount، CommissionPercent.

## اجرا (به ترتیب)
1. `backend/scripts/AddGroomers.sql` را در SSMS اجرا کنید (یا `Update-Database`).
2. `backend/scripts/MigrateCompanionGroomersToGroomers.sql`: اول با `@Commit = 0` (فقط شمارش)، بعد `@Commit = 1`. فقط ردیف جدید می‌سازد و idempotent است؛ Companion قدیمی پاک نمی‌شود و رزرو/تسویه‌ی قبلی روی آن می‌ماند. آرایشگاه منتقل‌شده از لیست نمایندگی‌ها به‌عنوان Companion دیگر نمایش داده نمی‌شود. نوع «Grooming» = ۴۵ است؛ اگر «Barber» (۴۳) هم آرایشگاه حیوان است به `@TypeIds` اضافه کنید.
3. Api را دیپلوی و Permission Sync بزنید؛ مجوزهای `Groomer*` را به نقش‌ها بدهید.

## API
- **ادمین** (`/api/Admin/…`): `Groomer` (جستجو/ساخت/ویرایش/حذف/Restore/Pets)، `GroomerActive`، `GroomerApprove`، `GroomerSetting` (پیشنهادی، پروانه، کمیسیون، نمایش در سایت)، `GroomerService` (خدمت + بسته + گرومرها)، `GroomerSchedule` (Times/Zones)، `GroomerUser`، `GroomerPicture`، `GroomerPortfolio` (+Moderate)، `GroomerPortfolioComment`.
- **مالک** (`/api/Companion/…`): همان‌ها فقط روی آرایشگاه‌های خود کاربر. ساخت و ویرایش توسط مالک منتظر تأیید ادمین می‌ماند؛ خدمت جدید تا تأیید ادمین قابل رزرو نیست.
- **عمومی** (`/api/Groomer`، `/api/GroomerPortfolio`): فقط آرایشگاه تأییدشده و فعال؛ شناسه‌ی مالک/ارائه‌دهنده پنهان است.
- **کاربر** (`/api/EndUser/GroomerPortfolioLike` و `…Comment`): لایک (idempotent) و کامنت.

## نمونه‌کار (برای فرانت)
فید: `GET /api/GroomerPortfolio?groomerId=&cityId=&petId=&pageIndex=&pageSize=` → `{totalCount, list:[{id, groomerName, groomerIcon, caption, pictures:[{kind 0عکس/1قبل/2بعد/3ویدیو, sortOrder, picture}], likeCount, commentCount, likedByMe}]}`. لایک: `POST/DELETE /api/EndUser/GroomerPortfolioLike?postId=` → `{liked, likeCount}`. کامنت: `POST /api/EndUser/GroomerPortfolioComment {postId, text, parentId?}`؛ لیست: `GET /api/GroomerPortfolio/Comments?postId=&parentId=`. پاسخ به پاسخ به همان نظر اصلی وصل می‌شود. پست جدید فوری منتشر می‌شود و ادمین می‌تواند پنهانش کند (با دلیل).

## تصمیم‌ها و محدودیت‌های شناخته‌شده
- نظر مربوط به رزرو (`GroomerReserveId`) با فاز ۲ می‌آید.
- لایک‌های تک‌تک نظرهای قدیمی (CommentLikes) منتقل نمی‌شود، فقط شمارنده.


## فاز ۲ — رزرو، پرداخت و تسویه (ساخته‌شده، ۱۴۰۵/۰۷/۲۸)
- **رزرو:** `GroomerReserve` (یک پت، یک بسته). مبلغ آنلاین = پیش‌پرداخت بسته (یا کل قیمت اگر پیش‌پرداخت ندارد) + هزینه‌ی دریافت/تحویل؛ باقی‌مانده حضوری است و در حسابداری نمی‌آید. زمان از GroomerTime با ظرفیت هم‌زمان (و MaxConcurrentPets)، تداخل گرومر انتخابی و تداخل پت بررسی می‌شود؛ رزرو پرداخت‌نشده ۱۵ دقیقه ظرفیت را نگه می‌دارد.
- **پرداخت:** `PaymentCallbackTypeEnum.GroomerReserve` (کد رزرو GRM...)؛ کد تخفیف روی مبلغ آنلاین (هدف کوپن باشگاه: آرایشگاه/خدمت)، کیف پول (ستون `Wallets.GroomerReserveId`)، برگشت از درگاه با callback و بررسی مجدد ظرفیت.
- **تأیید آرایشگاه:** بعد از پرداخت منتظر تأیید می‌ماند (مهلت ۱۲ ساعت، حداکثر تا ۳۰ دقیقه قبل از نوبت)؛ رد یا بی‌پاسخی (job هر ۵ دقیقه `GroomerReserveExpireOverdue`) = لغو + برگشت کل مبلغ به کیف پول (ایدمپوتنت با کلید دفتر `GroomerReserveRefund:{id}`).
- **لغو:** کاربر تا ۱۲ ساعت قبل از نوبت (یا تا قبل از تأیید) با برگشت کامل، بعد از آن بدون برگشت؛ ادمین با یا بدون برگشت. بعد از شروع/تکمیل لغو نمی‌شود.
- **تسویه:** `SettlementKindEnum.Groomer = 3`، جدول `SettlementGroomers`؛ فقط رزرو «تکمیل‌شده». سهم‌ها با درصد خدمت (یا پیش‌فرض آرایشگاه) در لحظه‌ی پرداخت ثبت می‌شود. حسابداری ارائه‌دهنده یک استخر سوم (GroomerPool) دارد؛ گزارش مالی کل هم منبع GroomerReserve دارد.
- **نظر:** `GroomerReview`: فقط بعد از رزرو تکمیل‌شده، یک‌بار، با تأیید ادمین.
- **API جدید:** کاربر `/api/EndUser/GroomerReserve|GroomerReservePayment|GroomerReview`، مالک `/api/Companion/GroomerReserve|GroomerSettlement|GroomerReview`، ادمین `/api/Admin/GroomerReserve|GroomerReview`، عمومی `/api/GroomerReview`.

### دیپلوی پروداکشن (به ترتیب؛ خودتان اجرا کنید)
1. `scripts/AddGroomers.sql` سپس `scripts/AddGroomerReserves.sql` (idempotent). دومی نوع پرداخت/تخفیف (Codes) و ۵ الگوی پوش را هم می‌سازد؛ اگر ردیف `PaymentType_PansionReserve` در Codes نباشد Code ساخته نمی‌شود.
2. `scripts/MigrateCompanionGroomersToGroomers.sql` با `@Commit = 0` و بعد `1`.
3. هر دو سرویس Api و Payment را با هم دیپلوی کنید (PaymentService به رزرو آرایشگاه وابسته شد)، Permission Sync بزنید و مجوزهای Groomer* را بدهید.
4. پنل را آپلود کنید.

### محدودیت‌ها
- هنوز فرانت وب‌اپ/اپ نماینده برای آرایشگاه نوشته نشده (فقط پنل ادمین).
- زمان رزرو چندخدمتی با جمع مدت همه‌ی خدمت‌ها یک اسلات پیوسته می‌گیرد (بدون تقسیم بین چند گرومر).

## فاز ۳ — تکمیل‌ها (ساخته‌شده، ۱۴۰۵/۰۷/۲۸)
- **پوش و SMS:** تأیید/رد خود آرایشگاه (به مالک)، ثبت/تأیید/رد/انقضا/لغو/لغو توسط پشتیبانی/تغییر زمان رزرو، دعوت پرسنل. همه از `GroomerNotifier` (خطا رزرو را خراب نمی‌کند). پوش‌های جدید ۱۳۱ تا ۱۳۶ در مایگریشن `AddGroomerPhase3`.
- **باشگاه:** امتیاز `floor(PaymentPrice/10000)` با نوع `ScoreTransactionType_GroomerReserve` بعد از پرداخت؛ پوینت باشگاه با رویداد `GroomerReservationCompleted` هنگام تکمیل؛ هدف کوپن «آرایشگاه».
- **رزرو چندخدمتی:** `GroomerReserveInputDto.ExtraPackageIds` (حداکثر ۵ بسته، هر خدمت یک‌بار)، `GET EndUser/GroomerReserve/Quote?packageIds=1&packageIds=2`، `Availability?...&extraPackageIds=`. جدول `GroomerReserveItems`؛ کمیسیون هر آیتم با درصد خودش و جمع دقیقاً برابر پرداخت.
- **تغییر زمان:** `PUT Companion|Admin/GroomerReserve/Reschedule {id,newDoDate}` (فقط روی اسلات آزاد؛ پوش+SMS به مشتری، و اگر ادمین انجام داد به آرایشگاه). فیلدهای `RescheduleCount/OriginalDoDate/LastRescheduleDate`.
- **ریز تسویه:** `GET Admin/Settlement/{id}/Items` و `GET Companion/GroomerSettlement/{id}/Items` (همه‌ی انواع، از جمله آرایشگاه)؛ در پنل با کلیک روی تعداد آیتم.
- **دعوت پرسنل:** افزودن با موبایل فقط دعوت می‌سازد (`UserAccept=null`)؛ کاربر در `GET/PUT EndUser/GroomerStaffInvite` می‌پذیرد/رد می‌کند؛ فقط پذیرفته‌شده‌ها در پرسنل/تخصیص نوبت و `StaffCount` می‌آیند.

### دیپلوی فاز ۳ (به ترتیب؛ خودتان اجرا کنید)
1. `scripts/AddGroomerPhase3.sql` (idempotent؛ ستون‌ها، جدول آیتم‌ها + آیتم برای رزروهای قبلی، Code امتیاز، پوش‌های ۱۳۱–۱۳۶).
2. `scripts/SeedGroomerSms.sql` (۱۲ MessageType + SmsSetting) و ساخت ۱۲ قالب Lookup در کاوه‌نگار با نام = Label (بدون زیرخط).
3. دیپلوی Api (و Payment)، Permission Sync، آپلود پنل.

### قالب‌های کاوه‌نگار (نام قالب = Label)
| نام قالب | متن (با توکن‌ها) |
|---|---|
| GroomerApprovedForOwner | آرایشگاه %token در پاستیل تأیید شد و اکنون نمایش داده می‌شود. |
| GroomerRejectedForOwner | آرایشگاه %token رد شد. دلیل: %token2 |
| GroomerReserveForUser | %token عزیز، رزرو شما در %token2 برای %token3 ساعت %token10 ثبت شد و منتظر تأیید آرایشگاه است. |
| GroomerReserveForGroomer | رزرو جدید در %token برای %token2 در %token3 ساعت %token10 ثبت شد. لطفاً در اپ تأیید یا رد کنید. |
| GroomerReserveApprovedForUser | %token عزیز، رزرو شما در %token2 برای %token3 ساعت %token10 تأیید شد. |
| GroomerReserveRejectedForUser | %token عزیز، رزرو شما در %token2 رد شد و مبلغ به کیف پولتان برگشت. دلیل: %token3 |
| GroomerReserveExpiredForUser | %token عزیز، آرایشگاه %token2 در مهلت مقرر پاسخ نداد؛ رزرو لغو و مبلغ به کیف پولتان برگشت. |
| GroomerReserveCancelledForGroomer | رزرو %token در %token2 ساعت %token3 توسط مشتری لغو شد. |
| GroomerReserveCancelledByAdminForUser | %token عزیز، رزرو شما در %token2 توسط پشتیبانی لغو شد. دلیل: %token3 |
| GroomerReserveRescheduledForUser | %token عزیز، زمان رزرو شما در %token2 به %token3 ساعت %token10 تغییر کرد. |
| GroomerReserveRescheduledForGroomer | زمان رزرو %token به %token2 ساعت %token3 تغییر کرد. |
| GroomerStaffInvite | %token عزیز، شما به پرسنل آرایشگاه %token2 دعوت شده‌اید. برای پذیرش وارد اپ پاستیل شوید. |

## جداسازی خدمات گرومینگ از کلینیک (فاز ۳.۱)
- ستون `Assistances.IsGrooming` (مایگریشن `AddAssistanceIsGrooming`، اسکریپت `scripts/AddAssistanceIsGrooming.sql`). در پنل «خدمات» تیک «خدمت گرومینگ».
- آرایشگاه فقط خدمت‌هایی با `IsGrooming=true` می‌سازد (`GroomerServiceNotGrooming`). `GroomerService/GroomerServicePackage` همان کاتالوگ آرایشگاه است.
- کلینیک/نماینده: ساخت یا تغییر به خدمت گرومینگ رد می‌شود (`CompanionAssistanceGroomingNotAllowed`)، در جستجو/جزئیات عمومیِ CompanionAssistance و رزرو دیده نمی‌شود، و `GET /api/Assistance` (عمومی/EndUser/Site) پیش‌فرض `IsGrooming=false` است؛ اپ آرایشگاه با `?IsGrooming=true` می‌گیرد. جستجوی سراسری هم خدمت گرومینگ را برنمی‌گرداند. ادمین همه را می‌بیند.
- ترتیب: ۱) مایگریشن/اسکریپت ۲) در پنل خدمات گرومینگ را تیک بزنید ۳) `scripts/DeactivateClinicGroomingServices.sql` (اول `@Commit=0`) ردیف‌های قدیمی کلینیک‌ها را غیرفعال می‌کند. آرایشگاه‌های منتقل‌شده از Companion فعلاً در جستجوی عمومی نماینده/کلینیک باقی می‌مانند (تصمیم کاربر).
- آرایشگاه‌های منتقل‌شده (Companionی که Groomer دارد و نوع دیگری ندارد) از جستجوی عمومی نماینده/کلینیک، «نزدیک من» و جستجوی سراسری هم مخفی‌اند؛ فقط لیست ادمین نماینده‌ها (`IncludeMigratedGroomers`) آن‌ها را نشان می‌دهد.
