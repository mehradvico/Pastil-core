# اصلاحات بک‌اند قبل از لانچ (۲۰ مهر ۱۴۰۵ / 2026-10-12)

پاسخ به سند «پاستیل — تسک‌های بک‌اند قبل از لانچ» (@AmirMohsen). هر بند: وضعیت قبل، چه عوض شد، فایل‌ها.
مستند فرانت (وب‌اپ): `docs/ai/PRE_LAUNCH_WEBAPP_SPEC_FA.md`.

> **بدون migration و بدون تغییر schema.** دیپلوی: Api و Payment با هم (به‌خاطر تغییرات commit‌نشده‌ی قبلی پرداخت).
> بررسی دیتابیس پروداکشن (۲۰ مهر): هیچ پرداخت موفقی با درگاه تستی نبود، و هیچ رزرو پانسیون روزانه‌ی پرداخت‌شده‌ای با فرمول «روز + ۱» وجود نداشت؛ پس برگشت پول لازم نیست.

## مورد جدید (در سند نبود) — ثبت رزرو با مبلغ دلخواه

`RebateId`/`RebatePrice`/`Discount`/`WalletPrice` در ثبت رزرو پانسیون و مدرسه مستقیم از بدنه map می‌شد. با `RebatePrice` منفی و بعد «حذف کد تخفیف»
(`PaymentPrice += RebatePrice`) یا اعمال یک کد واقعی (پایه = `PaymentPrice + RebatePrice`) مبلغ قابل پرداخت پایین می‌آمد.

- `PansionReserveService.InsertAsyncDto` و `SchoolReserveService.InsertAsyncDto`: این فیلدها صفر/null می‌شوند.
- `SetRebateCodeAsyncDto`/`ClearRebateCodeAsync` (هر دو سرویس): پایه‌ی تخفیف همیشه `Price` سرور است.

## B1 — درگاه تستی
طبق تصمیم مدیر **فعلاً می‌ماند**. `MerchantGatewayRules` (اصلاح ۲۲ مهر): همه (و مهمان) زرین‌پال؛ «مدیر اصلی» (RoleId=2) **علاوه بر آن** درگاه تستی پاستیل.
در لیست و در `StartPayment` هر دو اجباری است. نتیجه‌ی درگاه تستی با برچسب TEST ثبت می‌شود و وارد تسویه نمی‌شود.
باگ ۲۲ مهر: مدیر اصلی «فقط» درگاه تستی می‌گرفت و BFF وب‌اپ (`app/server/api/cart/merchant.get.js`، کامیت QA) درگاه تستی را برای همه حذف می‌کرد ⇒ ادمین هیچ درگاهی نداشت.
وب‌اپ: پیش‌فرض `showTestGateways` در `nuxt.config.ts` حالا `true` است (بک‌اند تصمیم می‌گیرد)؛ `NUXT_SHOW_TEST_GATEWAYS=false` کلید خاموش‌کردن کامل است.

## B2 — قیمت پانسیون «هر شب»
- `PansionReserveRules.NightCount` = `toDate − fromDate`. ورود و خروج در یک روز رد می‌شود (`PansionCheckoutMustBeAfterCheckin`). `DayCount` از این به بعد یعنی «تعداد شب».
- تداخل اقامت‌ها شب‌محور شد: روز خروج یکی می‌تواند روز ورود دیگری باشد (`<`/`>` به‌جای `<=`/`>=`، هم در ثبت و هم در callback).
- شروع پرداخت (`PaymentService.InsertPansionReservePaymentAsyncDto`): مبلغ پایه دوباره از تعرفه‌ی فعلی × شب/ساعت ساخته می‌شود (`PansionReserveRules.Reprice`) و همان به درگاه می‌رود؛ رزروهای پرداخت‌نشده‌ی قدیمی هم این‌جا اصلاح می‌شوند.
- مهد: ساعت ناقص یک ساعت کامل حساب می‌شود (گرد به بالا عمدی است و حفظ شد).
- جانبی: در همین متد رزرو با `AsTracking` خوانده می‌شود، پس سهم کیف پول (`WalletPrice`) که آن‌جا حساب می‌شود حالا واقعاً ذخیره می‌شود (قبلاً با NoTracking گم می‌شد).

## B3 — اعتبارسنجی رزرو
قبلاً بود: `to > from` (حالا سخت‌تر)، تاریخ گذشته، `start < end` مهد، مالکیت پت، تداخل همان پت. اضافه شد:
- نوع پت با «حیوانات مجاز پذیرش» (`PansionPets`) — مرکزی که فهرستی ندارد همه را می‌پذیرد (`PansionPetTypeNotAccepted`).
- ساعت مهد داخل `OpenHour`/`CloseHour` مرکز (اگر قابل خواندن باشند) (`Pattern.PansionOutsideWorkingHours`).
- تکرار چک‌ها بعد از برگشت از درگاه: تاریخ رزرو گذشته باشد ⇒ اعمال نمی‌شود و پول درگاه با مسیر موجود `RefundUnappliedToWalletAsync` به کیف پول برمی‌گردد. شروع پرداخت هم رزرو تاریخ‌گذشته را رد می‌کند.
- **انجام نشد (نیاز به تصمیم + migration):** ظرفیت کلی پانسیون — فیلد ظرفیت در `Pansion` وجود ندارد. «ساعت تحویل» (`drop` در URL وب‌اپ) اصلاً به بک‌اند فرستاده/ذخیره نمی‌شود.

## B4 — سقف pageSize
`BaseInputDto` از قبل سقف ۱۰۰۰ و clamp منفی/صفر داشت (برای همین `pageSize=-1` خطا نمی‌داد). endpoint عمومی محصولات (`GET /api/Product`) حالا سقف **۱۰۰** دارد.
سقف کلی ۱۰۰۰ برای پنل (لیست برند/دسته تا ۱۰۰۰) دست نخورد. پاسخ ۲۰۰ با `isSuccess` طبق قرارداد پروژه است.

## B5 — جستجو زیر بار (`SearchService`)
- اول **بدون فازی**؛ فازی (بیگرام‌ها) فقط اگر هیچ نتیجه‌ای نبود و `enableFuzzy` true بود.
- کش ۵ دقیقه‌ای بر اساس q نرمال‌شده + تعدادها (`IMemoryCache`، Size=1). پاسخ `fromCache` دارد.
- سقف کل ۳ ثانیه: بعد از آن بخش‌های تمام‌شده برمی‌گردند و `isPartial=true` (این نتیجه کش نمی‌شود). شکست یک بخش دیگر کل جستجو را ۵۰۰ نمی‌کند.
- هر بخش حداکثر ۱۰ نتیجه (`MaxPerTypeCount` از ۲۰ به ۱۰).
- محصولات هم در scope جدا اجرا می‌شوند تا پرس‌وجوی جامانده روی DbContext درخواست اثر نگذارد.
- کار بعدی (انجام نشد): Full-Text Index روی `Products` (SQL Server) — پرس‌وجوی محصولات هنوز `LIKE '%…%'` روی چند ستون + `ProductFeatureValues` است.

## B6 — دوره‌های شروع‌شده
- `SchoolCourseRules`: ثبت‌نام فقط تا قبل از شروع جلسه‌ی اول (تاریخ + ساعت). هم در ثبت‌نام (`SchoolReserveService`، شامل ادامه‌ی ثبت‌نام پرداخت‌نشده) و هم در شروع پرداخت رد می‌شود (`SchoolCourseRegistrationClosed`).
- `SchoolCourseVDto.IsRegistrationOpen` (فعال + جلسه‌ی اول شروع نشده + ظرفیت دارد) در لیست و جزئیات.
- جلسات همیشه به ترتیب تاریخ/ساعت برمی‌گردند (لیست/جزئیات دوره و جزئیات ثبت‌نام؛ جلسه‌ی حذف‌شده در جزئیات ثبت‌نام دیگر نمی‌آید).
- ساخت/ویرایش جلسه: ساعت معتبر و `start < end`، زمان جدید در آینده، تعداد جلسات ≤ «تعداد جلسات» دوره. ویرایش دوره: «تعداد جلسات» کمتر از جلسات تعریف‌شده نمی‌شود.
- باگ «۷ جلسه در هفته»: فرمول بر کسرِ هفته تقسیم می‌کرد؛ حالا بر تعداد هفته‌های تقویمی (۲ جلسه با ۲ روز فاصله = ۲ در هفته). `TotalDurationWeeks` هم عدد صحیح هفته است.
- ظرفیت از قبل داخل تراکنش Serializable کم می‌شد (درست بود).

## B7 — Rate limit پیامک
قبلاً بود: per-IP (`OtpSend`/`OtpVerify`)، ۳ کد در ۱۰ دقیقه برای هر شماره، کد عوض‌شونده بعد از هر تلاش اشتباه، قفل بعد از ۴ اشتباه، انقضای ۳ دقیقه، قفل رمز عبور.
اضافه شد (`OtpSendRules`، حافظه‌ی همان کانتینر): **هر شماره/ایمیل ۱ کد در ۲ دقیقه و ۵ کد در ساعت** (`Pattern.OtpResendTooSoon` / `OtpHourlyLimitReached`). شامل تغییر موبایل/ایمیل هم می‌شود.

**روی سرور چک شود:** `Security:ClientIpAttestationKey` ست باشد؛ بدونش per-IP عملاً شل است (۳۰۰). طول کد با `PASTIL_OTP_LENGTH` (۴ تا ۸) قابل افزایش است (اپ/وب‌اپ از `codeLength` پاسخ می‌خوانند). nginx: بند پایین.

## B8 — کیف پول
- شارژ کاربر: عدد صحیح **۱۰٬۰۰۰ تا ۱۰٬۰۰۰٬۰۰۰ تومان** (`WalletChargeRules`)، در غیر این صورت خطا (قبلاً منفی/صفر بی‌صدا ۱۰٬۰۰۰ می‌شد و عدد بزرگ در درگاه سرریز می‌کرد). `GET /api/EndUser/Wallet/ChargeLimits`.
- callback تکراری و قفل کسر از کیف پول از قبل درست بود (`PaymentAttemptRules`، تراکنش Serializable در `InsertUpdateReferenceAsync`).
- **شرح تراکنش هرگز خالی نیست** (`WalletDescriptionRules` + `WalletDescriptionService`):
  - نمایش (لیست/جزئیات کاربر و پنل): `name` شرح فارسی است، مقدار ذخیره‌شده در `ledgerName`. مثال‌ها: «شارژ کیف پول توسط کاربر (PAY-…)»، «شارژ دستی توسط ادمین — یادداشت»، «پرداخت رزرو پانسیون RSV-… از کیف پول»، «لغو رزرو پانسیون RSV-… — بازگشت وجه به کیف پول»، «لغو خدمت «…» از رزرو خدمت RSV-… — بازگشت وجه»، «پرداخت بدهی خدمت …»، «پاداش معرفی (سفارش …)».
  - کلیدهای ضدتکرار دفتر (`PaymentRefund:{id}`، `PansionReserveRefund:{id}`، `ConsultationRefund:{id}`، `CompanionPackageRefund:{id}`، `OrderCancelRefund:{orderId}`، `OrderItemRefund:{itemId}:{n}`، `ReferralBonus:{orderId}`) **عوض نشدند** و فقط ترجمه می‌شوند.
  - ثبت جدید: اگر Name خالی بود از روی مرجع با کد ساخته و ذخیره می‌شود. شارژ آنلاین دیگر «پرداخت آنلاین» نیست. بدهی خدمت `CompanionDebt:{reserveId}` (متن فارسی hardcode حذف شد). شارژ/کسر دستی پنل `AdminManual:{یادداشت}`؛ پنل: فیلد «دلیل / توضیح (اختیاری)».
  - ردیف‌های قدیمی هم در نمایش شرح می‌گیرند (ردیف بی‌مرجعِ دارای متن = دستی ادمین).

## B9 — دسترسی
نمونه‌ها بررسی شد و درست بود: آدرس/پت/عکس پت/رزروها/پروفایل مالکیت را با توکن چک می‌کنند. اصلاح شد:
`GET /api/OnlineSession/SearchUsers` — هر نماینده/کارمند مرکزی می‌توانست با تکه‌ی نام/شماره کل کاربران و موبایلشان را فهرست کند. حالا شماره‌ی کامل ⇒ همان یک کاربر؛ بقیه فقط بین مشتریان همان مرکز (رزرو خدمت/پانسیون/مدرسه یا جلسه‌ی آنلاین قبلی). ممیزی کامل دو‌حسابی هنوز باید دستی تست شود.

## B13 — کاهش درخواست‌ها
- `GET /api/StoryGroup/Home`: گروه‌های دارای استوری فعال + `itemCount` + `firstItem` (کش ۵ دقیقه) — به‌جای ۱ + N درخواست.
- `GET /api/Companion`: هر مرکز `reserveCount` دارد (شمارش دسته‌ای) — به‌جای `companionReserveCount` برای هر مرکز. فیلتر جدید `HasActiveService=true` برای لیست‌های «برتر» (D1).
- `GET /api/EndUser/Cart`: خواندن سبد با GET (GetCart/GetCount/GetDelivery).
- رفع باگ: `CustomOutputCache` با `SizeLimit` حافظه می‌شکست (ورودی بدون Size)؛ حالا Size=1 دارد. خودِ کش خروجی هنوز با `CacheSettings:DisableAll=true` خاموش است (روشن کردنش قیمت/موجودی را تا ۲۶ دقیقه کهنه نشان می‌دهد؛ عمداً روشن نشد).

## B14 — PastilAI
- اشتراکِ خریده‌شده حتی اگر ادمین بعداً پلن را غیرفعال کند تا پایان مدت معتبر است (قبلاً کاربر به رایگان برمی‌گشت در حالی که سفارش «فعال» دیده می‌شد — محتمل‌ترین علت گزارش).
- `GET quota` حالا `isSubscribed`، `expiresAt`، `remainingChats` دارد.

## B15 — ورودی جستجو
q بیش از ۱۰۰ کاراکتر ⇒ `isSuccess:false` + `SearchQueryTooLong` (`[StringLength]` به‌خاطر `SuppressModelStateInvalidFilter` اعمال نمی‌شد). کد ۴۰۰ عوض نشد: قرارداد بک‌اند ۲۰۰ + `isSuccess` است.

## B19 — ثبت تکراری
`[IdempotentRequest]` (`Api/Filter`) روی POST آدرس، پت، رزرو پانسیون/مدرسه/خدمت، سبد رزرو خدمات، تیکت و پیام تیکت:
- با هدر `Idempotency-Key`: همان کلید تا ۱۰ دقیقه پاسخ موفق اول را برمی‌گرداند.
- بدون هدر: همان کاربر + مسیر + بدنه در ۵ ثانیه = تکراری (پاسخ اول، یا اگر در جریان است `DuplicateRequest`).
- پاسخ ناموفق نگه داشته نمی‌شود. پرداخت از قبل `Idempotency-Key` خودش را دارد.

## پل موقت — چند نشست هم‌زمان برای صاحب مرکز (۲۱ مهر)
نماینده‌ها حسابشان را بین کارمندان پخش می‌کنند و هر ورود جدید نشست قبلی را می‌بست. چون حساب کارمند (`CompanionUser`) هنوز در ۴۴ از ۵۵ کنترلر
ناحیه‌ی Companion کار نمی‌کند (همه با `CompanionId` صاحب مرکز محدود می‌شوند)، فعلاً:
- صاحب مرکزِ فعال و تأییدشده تا **۵ نشست هم‌زمان** وب‌اپ/اپ دارد؛ ورود ششم قدیمی‌ترین نشست را می‌بندد (`SessionLimitRules`، `UserTokenService.ResetTokenAsync`).
- تنظیم: `Security:CompanionOwnerMaxSessions` (پیش‌فرض ۵ در `Api/appsettings.json`) یا env `PASTIL_COMPANION_OWNER_MAX_SESSIONS`؛ **مقدار ۱ = خاموش**. حداکثر ۱۰.
- شامل ورود پنل ادمین، کاربران عادی و کارمندان نمی‌شود؛ تغییر رمز/موبایل و ریست رمز همچنان همه‌ی نشست‌ها را می‌بندند.
- هر ورود چندنشستی در لاگ امنیتی با `SignIn.CompanionOwnerMultiSession` ثبت می‌شود.
- ریسک‌ها سر جایشان است (رمز مشترک، دسترسی کارمندان به کیف پول/اطلاعات بانکی صاحب مرکز، معلوم نبودن انجام‌دهنده). بعد از راه افتادن حساب کارمندان باید ۱ شود.

## پرداخت موفق ولی صفحه‌ی «ناموفق» (۲۵ مهر)
گزارش: پرداخت زرین‌پال واقعاً انجام و verify شد و سفارش «پرداخت شده» ثبت شد، ولی کاربر بعد از ~۲ دقیقه لودینگ صفحه‌ی «پرداخت ناموفق» دید.
علت پیدا‌شده در کد (`ZarinPalGateway.VerifyStoredAuthorityAsync` + `MerchantService.SaveCallbackResultAsync`): هر تایم‌اوت/خطای شبکه/پاسخ نامعتبر
موقع verify «ناموفق» حساب و در دیتابیس **ماندگار** (`IsSuccess=0`، `GatewayStatus=FAILED`) می‌شد؛ یعنی پرداختی که بانک تأیید کرده بود هم برای کاربر ناموفق بود و
هم job تطبیق (فقط پرداخت‌های بدون نتیجه) هرگز آن را به سفارش نمی‌رساند. تایم‌اوت پیش‌فرض HttpClient ۱۰۰ ثانیه است (همان «۲ دقیقه»).
اصلاح:
- verify هر تلاش ۲۰ ثانیه سقف دارد و یک‌بار تکرار می‌شود. تایم‌اوت/خطای شبکه/HTTP 5xx و 429/پاسخ نامعتبر = **IsPending** (چیزی روی پرداخت ثبت نمی‌شود، سفارش و کیف پول دست نمی‌خورد، کاربر پیام «در حال تأیید با بانک» می‌بیند، کد `PaymentVerificationPending=21`).
- کد ۱۰۱ (قبلاً verify شده) برای همین Authority و مبلغ **موفق** حساب می‌شود (verify قبلی که جوابش گم شد). پاسخ قطعی منفی (مثلاً ‎-53‎) همچنان ناموفق است.
- job تطبیق `PaymentReconcileUnconfirmed` پرداخت‌های بدون نتیجه را از ۳ دقیقه بعد (قبلاً ۱۰) verify می‌کند (verify ناموفق ذخیره نمی‌شود، پس امن است).
- اگر وسط callback استثنا رخ دهد، وضعیت واقعی از دیتابیس خوانده می‌شود و پرداختِ اعمال‌شده «ناموفق» نشان داده نمی‌شود.
- **هنوز ناشناخته:** چرا verify/اعمال ~۲ دقیقه طول کشید (کندی زرین‌پال از سرور یا کندی مرحله‌ی اعمال سفارش). برای تشخیص لاگ سرویس Payment حوالی آن ساعت لازم است (`ZarinPal verify`, `Payment callback failed`).
- **نتیجه‌ی بررسی دیتابیس (۲۵ مهر):** کوئری FAILED خالی بود، یعنی این سناریو در ۱۴ روز اخیر رخ نداده؛ و پرداخت همین سفارش (`Id=22`) یک ردیف با `IsSuccess=1`، `Description=100--93620525201` و `AppliedDate` حدود ۲:۴۰ بعد از ساخت بود. پس verify موفق بود و کندی داخل **اعمال پرداخت روی سفارش** (`ProductPaymentCallback`) رخ داده.
- **علت محتمل کندی:** داخل همان تراکنش Serializable، پشت سر کاربر، پشت‌سرهم: ساخت مرسوله‌ی پیک (`CreateForPaidOrderAsync`)، پیامک کاربر، پوش به همه‌ی کاربران فروشگاه و همه‌ی ادمین‌ها (از سرور ایران ممکن است هر پوش FCM ده‌ها ثانیه معلق شود)، اعلان ادمین، پیامک فروشگاه‌ها. `AppliedDate` هم فقط بعد از تمام شدن همه‌ی این‌ها ثبت می‌شد؛ پس رفرش/تلاش دوم کاربر هم وارد همان تراکنش می‌شد و می‌توانست خطا بدهد و صفحه‌ی «ناموفق» نشان بدهد. (هنوز با لاگ تأیید نشده.)
- **اصلاح:** `PostCommitQueue` (`Application/Common/PostCommit`): کارهای جانبی بعد از Commit موفق و در پس‌زمینه (scope جدا) اجرا می‌شوند، نه داخل تراکنش. سفارش محصول: پیامک ثبت سفارش، پوش‌ها، اعلان ادمین و پیامک فروشگاه به `ProductOrderRegisteredNotifier` منتقل شد و از `PaymentService.ApplyAndMapPaymentAsync` بعد از Commit فلاش می‌شود (اگر تراکنش برگردد هیچ‌کدام نمی‌روند). هر مرحله‌ای که بیش از ۳ ثانیه طول بکشد و هر اعمال پرداخت بیش از ۵ ثانیه لاگ Warning می‌شود تا کندیِ واقعی (پوش؟ پیک؟) مشخص شود.
- **تأیید با خط زمانی سفارش ORD-14050725-1530-7583 (۲۵ مهر):** پوش به کاربر ساعت ۱۵:۳۰:۱۸ ساخته شد ولی «پرداخت اعمال شد» و پیامک ساعت ۱۵:۳۰:۵۲ — ۳۴ ثانیه بعد. علت: `PushNotificationService` هر پوش را همان‌جا و پشت‌سرهم برای هر دستگاه کاربر می‌فرستد (بدون تایم‌اوت؛ WebPush پیش‌فرض ۱۰۰ ثانیه) و پوش ثبت سفارش اکسپرس داخل تراکنش پرداخت بود؛ یک دستگاهِ دست‌نیافتنی از سرور ایران کل ثبت سفارش را نگه می‌داشت.
- **اصلاح ۲:** `IPostCommitQueue.Activate/Deactivate/IsActive`؛ `PaymentService.ApplyAndMapPaymentAsync` صف را فعال می‌کند و `PushNotificationService.SendPushAsync` وقتی صف فعال و تراکنش باز است فقط ردیف پوش را ذخیره می‌کند و ارسال (`DeliverAsync`) را بعد از Commit به پس‌زمینه می‌سپارد (اگر به هر دلیل نرفت، job موجود `SendPushGroupAsync` ردیف‌های `IsSend=false` را می‌فرستد). برای پوش‌های بیرون از پرداخت رفتار قبلی (ارسال هم‌زمان) می‌ماند. تایم‌اوت هر دستگاه WebPush هم ۱۵ ثانیه شد.
- **هنوز داخل تراکنش:** ساخت مرسوله‌ی پیک (`ShipmentService.CreateForPaidOrderAsync`) و پیامک/پوش سایر انواع پرداخت (رزرو، پانسیون، سفر، کیف پول...). اگر لاگ Warning نشان داد کجا کند است، همان به صف منتقل شود.
- پرداخت‌های قدیمی که به همین علت FAILED مانده‌اند را این کوئری پیدا می‌کند؛ با پنل زرین‌پال تطبیق دهید (فقط خواندنی):
  `SELECT Id, UserId, Amount, CreateDate, CallBackTypeLabel, CallBackId, Token, Description FROM Payments WHERE IsOnline=1 AND IsSuccess=0 AND GatewayStatus='FAILED' AND Token IS NOT NULL AND CreateDate > DATEADD(day,-14,GETDATE()) ORDER BY Id DESC;`

## «درخواست پیک انجام نشد» در پنل فروشنده (۲۵ مهر)
فروشنده «درخواست پیک» می‌زند و پیام عمومی `ShipmentCourierRequestFailed` می‌بیند. علت واقعی در `Shipment.FailureReason` ذخیره می‌شود، ولی `MiareShippingProvider`
فقط فیلد `detail`/`code` پاسخ میاره را می‌خواند؛ خطای اعتبارسنجی فیلدها (مثل `{"pickup":{"image":["..."]}}`) گم می‌شد و `FailureReason` فقط «Miare request failed.» بود.
اصلاح: `FailureReason` و لاگ حالا `HTTP <کد>: <پیام/بدنه‌ی پاسخ تا ۶۰۰ نویسه>` است (درخواست و کلید API ثبت نمی‌شود).
**علت قطعی (از `FailureReason` سفارش ORD-14050725-1419-5221): `Object reference not set to an instance of an object.`** — `AppSettingsHelper.Initialize(...)` فقط در `Payment/Program.cs` فراخوانی می‌شد، نه در Api.
`ShipmentService.BuildStoreImageUrl` (ساخت آدرس تصویر فروشگاه برای میاره) در Api به `AppSettingsHelper.FileBaseUrl` می‌رسید که `_configuration` آن null بود ⇒ NullReferenceException برای **هر فروشگاهی که لوگو/تصویر دارد** (فروشگاهِ بدون تصویر زودتر برمی‌گشت و مشکل نداشت). پس درخواست پیک اصلاً به میاره نمی‌رسید.
اصلاح: `AppSettingsHelper.Initialize(builder.Configuration)` به `Api/Program.cs` اضافه شد، گترهای URL آن null-safe شدند (`_configuration?[...]`)، `MiareShippingProvider` هر exception را با stack لاگ می‌کند و در `FailureReason` نوع exception را هم می‌نویسد، و اگر میاره تصویر فروشگاه (مثلاً webp) را رد کند یک‌بار با `DefaultPickupImageUrl` دوباره می‌سازد.
مرسوله‌ی مانده (`Shipments.Id=2`، وضعیت ۸) بعد از دیپلوی Api تا پایان مهلت فروشنده با همان دکمه‌ی «تلاش دوباره» قابل ثبت است؛ بعد از مهلت `ProcessUnconfirmedAsync` آن را Failed و سفارش را «درخواست لغو» می‌کند.
**دومین علت (بعد از دیپلوی اصلاح اول، `FailureReason` = `HTTP 403: {"code":"deadline_too_soon","detail":""}`):** میاره `pickup.deadline` کمتر از ~۳۰ دقیقه از الان را رد می‌کند؛ ما برای اکسپرس «الان + ۱۵ دقیقه» (و برای سفارش‌های بازه‌ای هم حداقل الان + ۱۵) می‌فرستادیم، پس درخواست پیک همه‌ی سفارش‌های اکسپرس شکست می‌خورد.
اصلاح: حداقل فاصله ۳۰ دقیقه شد (`ShippingSlotRules.MinPickupFromNowMinutes`، `ExpressPickup`، پیش‌فرض `Express.PickupLeadMinutes=30`؛ ساعتی که به فروشنده نشان داده می‌شود هم همین است)، و اگر میاره باز هم `deadline_too_soon` بدهد provider با ۴۵ و سپس ۶۰ دقیقه دوباره می‌سازد و مهلت واقعی را در `Shipment.PickupDeadlineUtc` ذخیره می‌کند.
(قبل از این یافته، مظنون‌های دیگر که الان رد شده‌اند:)
مظنون‌های رایج (تأییدنشده تا دیدن پاسخ واقعی): تصویر پیش‌فرض `favicon.ico` یا آیکن `.webp` فروشگاه (`pickup.image`)، تلفن/آدرس/مختصات فروشگاه خالی یا نامعتبر، فرمت موبایل گیرنده، حداقل فاصله‌ی `pickup.deadline` (اکسپرس: الان + ۱۵ دقیقه).
کوئری تشخیص (فقط خواندنی):
`SELECT s.Id, s.Status, s.IsExpress, s.FailureReason, s.PickupDeadlineUtc, st.Id StoreId, st.Name, st.Phone, st.Address, st.Location.STAsText() StoreLocation, st.IconId, st.PictureId, a.Mobile RecipientMobile, a.Location.STAsText() RecipientLocation FROM Shipments s JOIN ProductOrderStores ps ON ps.Id = s.ProductOrderStoreId JOIN ProductOrders o ON o.Id = ps.ProductOrderId JOIN Stores st ON st.Id = ps.StoreId LEFT JOIN Addresses a ON a.Id = o.AddressId WHERE o.OrderCode = 'ORD-14050725-1419-5221';`
پوش «سفارش اکسپرس ثبت شد» برای مشتری و پوش «فروشنده هنوز آماده نکرده» برای ادمین طبق طراحی‌اند (اولی لحظه‌ی ثبت سفارش، دومی ۳۰ دقیقه بعد اگر مرسوله هنوز تأیید نشده)؛ با شکست درخواست پیک، مرسوله تأییدنشده می‌ماند و پوش ادمین درست رفته است.

## دکمه‌ی «تمدید مهلت فروشنده» برای ادمین (۲۵ مهر)
وقتی مشکل از پاستیل/میاره است و فروشنده نمی‌تواند «درخواست پیک» بزند، قبلاً بعد از پایان مهلت مرسوله Failed و سفارش «درخواست لغو» می‌شد و فقط با SQL قابل برگشت بود.
- پنل: صفحه‌ی جزئیات سفارش (`panel/pages/admin/productorder/detail-[id].vue`) کارت «مرسوله‌ی پیک (میاره)» دارد: فروشگاه، اکسپرس/عادی، وضعیت، مهلت فروشنده (قرمز اگر گذشته)، دلیل شکست (`FailureReason`) و — وقتی مجاز است — انتخاب ۳۰ دقیقه/۱/۲/۴ ساعت + دکمه‌ی «تمدید مهلت فروشنده».
- API (`Api/Areas/Admin/Controllers/OrderShipmentController.cs`): `GET api/Admin/OrderShipment?productOrderId=` و `PUT api/Admin/OrderShipment/ExtendSellerDeadline {shipmentId, minutes}` (۱۵ تا ۴۸۰ دقیقه). نقش «مدیر اصلی» بدون تنظیم دسترسی دارد؛ بقیه‌ی نقش‌های ادمین باید این کنترلر را در صفحه‌ی نقش‌ها تیک بزنند.
- قواعد (`ShipmentDeadlineRules`، `ShipmentService.ExtendSellerDeadlineAsync`): قابل تمدید = منتظر فروشنده (`AwaitingSellerConfirm`: مهلت = max(مهلت فعلی، الان) + دقیقه‌ها)، در حال آماده‌سازی (`Preparing`: `ReadyDeadlineUtc`)، و ناموفقِ فقط‌به‌خاطر پایان مهلت (`Failed` با `FailureReason=SellerConfirmExpired`) که احیا می‌شود: وضعیت برمی‌گردد به منتظر فروشنده، `CancelRequestDate` و یادداشت «لغو خودکار» برداشته می‌شود (لغو درخواستیِ دستی دست نمی‌خورد)، یادآورها ریست می‌شوند. سایر وضعیت‌ها (پیک درخواست‌شده/تحویل/لغو/ناموفقِ دلایل دیگر) قابل تمدید نیستند.
- به فروشنده همان پوش «سفارش ثبت شد؛ تا ساعت X درخواست پیک بزنید» با مهلت جدید می‌رود (پوش جدید/migration لازم نیست). هر تمدید با شناسه‌ی ادمین در لاگ ثبت می‌شود.
- تست: `Application.Tests/ShipmentDeadlineRulesTests.cs`.

## میاره روی Staging است، نه Production (۲۵ مهر) — پیک واقعی هیچ‌وقت نمی‌آید
علائم: مرسوله بعد از «درخواست پیک» روی وضعیت ۲ می‌ماند (هیچ‌وقت ۳/۴ نمی‌شود)، لینک رهگیری روی `www.miare.ir` می‌گوید «سفر مورد نظر شما یافت نشد»، وب‌هوکی نمی‌آید.
علت: `backend/.env` (و طبق `BACKEND_LIVE_SHIPPING_FA.md` توکن فعلی) روی محیط آزمایشی میاره است:
`PASTIL_SHIPPING_MIARE_BASE_URL=https://ws.staging.miare.ir/trip-management/third-party-api/v2` و `PASTIL_SHIPPING_MIARE_ACCOUNTING_BASE_URL=https://www.staging.miare.ir/api/accounting`.
سفر در Staging ساخته می‌شود (ثبت موفق، شناسه‌ی سفر هم برمی‌گردد) ولی پیک واقعی ندارد و صفحه‌ی رهگیری Production آن را نمی‌شناسد. `Shipping:TestMode` فقط AloPeyk/SnappBox را شبیه‌سازی می‌کند و روی Miare اثر ندارد.
برای لانچ (روی سرور، نه در کد):
۱. از میاره **کلید API محیط Production** و آدرس‌های Production را بگیرید (الگوی احتمالی: `https://ws.miare.ir/trip-management/third-party-api/v2` و `https://www.miare.ir/api/accounting`؛ حتماً با مستند/پشتیبانی میاره تأیید شود، حدس نزنید).
۲. `PASTIL_SHIPPING_MIARE_BASE_URL`، `PASTIL_SHIPPING_MIARE_ACCOUNTING_BASE_URL` و `PASTIL_SHIPPING_MIARE_API_KEY` سرور را عوض و Api را ری‌استارت کنید.
۳. از میاره بخواهید برای همان حساب Production وب‌هوک `https://api.pastil.pet/api/webhooks/miare` را با هدر `Authorization: Token <همان کلید>` ثبت کنند (هنوز ثبت نشده؛ سؤال باز از ۱۳ مهر).
۴. Boost اکسپرس برای حساب Production باید توسط تیم بازرگانی میاره فعال شود (`MIARE_EXPRESS_SHIPPING_FA.md`)، وگرنه سفر بدون Boost ساخته می‌شود.
۵. با سوییچ، هر سفارش اکسپرس **پیک واقعی و هزینه‌ی واقعی** دارد؛ تست‌ها را با سفارش کوچک و حساب خودتان انجام دهید.
۶. سفارش‌های ساخته‌شده تا الان (مثل ۵۲۲۱ و ۷۵۸۳) سفرشان فقط در Staging است و هرگز پیک نمی‌شوند؛ آن‌ها را در پنل لغو کنید (پول به کیف پول برمی‌گردد) و بعد از سوییچ یک سفارش تازه بزنید. وب‌هوک Production برای شناسه‌های Staging «unknown trip» لاگ می‌کند و بی‌اثر است.
۷. بعد از سوییچ با لاگ `Miare webhook: trip … state …` (بعد از دیپلوی Api) و وضعیت ۳/۴ مرسوله‌ها (`Shipments.Status`) تأیید کنید.

## واحد مبلغ‌های میاره در Production «ریال» است، نه تومان (۲۵ مهر) — مشتری ۱۰ برابر قیمت می‌دید
علائم: بعد از سوییچ به Production برای مسیر ≈۱۱ کیلومتر ارسال ≈۸۵۰ تا ۹۰۰ هزار تومان نشان داده شد.
علت (لاگ Api، `Miare GetQuoteAsync … body`): برآورد Production برای دو مسیر ≈۱۱ کیلومتری `901000` و `853000` بود. Staging برای ≈۱۴ کیلومتر `90000` می‌داد و برای ≈۲٫۸ کیلومتر `32000`؛ پس Production دقیقاً ۱۰ برابر Staging است و ۹۰ هزار تومان برای ۱۱ کیلومتر قیمت واقعیِ پیک است ⇒ Production **ریال** برمی‌گرداند و Staging **تومان**. کد همه‌چیز را تومان فرض می‌کرد و مبلغ را بی‌تبدیل به مشتری نشان می‌داد.
اصلاح: `Shipping:Miare:AmountUnit` (`Toman` پیش‌فرض | `Rial`) + `MiareAmountRules`. با `Rial` سه مبلغ میاره به تومان تبدیل می‌شود: برآورد قیمت، مبلغ سطح‌های Boost (و در نتیجه `Shipment.BoostAmount` و اضافه‌ی اکسپرس)، و `delivery_cost` وب‌هوک (برای `ProviderCost`، که به ریال ذخیره می‌شود). اضافه‌ی ثابت اکسپرس (`FallbackSurchargeToman`) همیشه تومان است و تبدیل نمی‌شود.
قبل از لانچ (روی سرور، بعد از دیپلوی Api):
۱. واحد را با یک برآورد نمونه تأیید کنید (یا از میاره بپرسید): قیمت همان مسیر را در پنل/اپ میاره ببینید؛ اگر ≈۹۰ هزار تومان بود (نه ۹۰۰ هزار) Production ریال است.
۲. در `api/.env` اضافه کنید: `Shipping__Miare__AmountUnit=Rial` و `docker compose up -d --force-recreate --no-deps api`.
۳. تأیید: در لاگ بعد از یک استعلام خط `Miare GetQuoteAsync: raw price 901000 (Rial) -> 90100 Toman.` دیده شود و مشتری ≈۹۰ هزار تومان ببیند.
۴. اگر Boost Production را فعال کردید (`UseBoost=true`)، مبلغ سطح‌ها هم ریال است و با همین تنظیم خودکار به تومان تبدیل می‌شود.
⚠️ تنظیم را فقط وقتی `Rial` کنید که مطمئنید؛ اشتباه در جهت مخالف یعنی مشتری ۱۰ برابر کمتر می‌پردازد و پاستیل بقیه را از جیب می‌دهد.

## بک‌اند نیست / بعد از لانچ
- **B10** Firebase/GA در ایران، **B11** `sw.js` با ۵۰۳، **B16** صفحه‌ی `/consultations/success`: وب‌اپ (مستند وب‌اپ). B16: صفحه بدون `?id` فهرست خریدها را می‌خواند؛ بازگشت از درگاه خودش به `/consultations/{id}` می‌رود.
- **B12** nginx (روی سرور، در ریپو نیست):
  ```nginx
  listen 443 ssl;
  http2 on;                       # nginx ≥ 1.25؛ در 1.24: listen 443 ssl http2;
  server_tokens off;
  add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
  proxy_hide_header X-Powered-By;
  gzip on; gzip_types text/css application/javascript application/json image/svg+xml;
  location /_nuxt/ { add_header Cache-Control "public, max-age=31536000, immutable"; proxy_pass ...; }
  location = /sw.js { add_header Cache-Control "no-cache"; proxy_pass ...; }
  # B7 (limit_req_zone داخل بلوک http):  limit_req_zone $binary_remote_addr zone=otp:10m rate=5r/m;
  # location ~* ^/api/account/otp$ { limit_req zone=otp burst=3 nodelay; limit_req_status 429; proxy_pass ...; }
  # اگر وب‌اپ OTP را از سرور Nuxt (BFF) صدا می‌زند، این محدودیت روی مسیر وب‌اپ (/api/auth/...) گذاشته شود نه بک‌اند، وگرنه همه از IP وب‌اپ می‌آیند.
  ```
- **B17** توکن در localStorage، **B18** شناسه‌ی ترتیبی: بعد از لانچ.
- **D1–D8** دیتا از پنل: D3/D4 فیلتر `IsPersonal`/`TypeId` درست کار می‌کند؛ مشکل از داده‌ی خود مراکز است (پورشعبان `IsPersonal=true`، قادرپناه نوع ۴۰). D5 (unique روی مرکز+کاربر) نیاز به migration و تأیید دارد.

## تست‌ها
`Application.Tests/PreLaunchFixes/*`: شب پانسیون، تداخل، ساعت کاری، Reprice، خلاصه‌ی هفتگی و باز بودن ثبت‌نام مدرسه، سقف شارژ، محدودیت OTP، شرح تراکنش کیف پول.
