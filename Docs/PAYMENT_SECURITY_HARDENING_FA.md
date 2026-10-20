# سخت‌سازی امنیت پرداخت (۲۰۲۶-۱۰-۱۲)

**وضعیت:** پیاده‌سازی شده، build سبز، `Application.Tests` ‏۸۹۹/۸۹۹ (۱۳ تست تازه در `Application.Tests/Payment/PaymentGatewayBindingTests.cs`)، مسیر بازپرداخت به کیف پول روی LocalDB با DI واقعی بررسی شد. **بدون migration.** Api و Payment باید با هم منتشر شوند.
**مرتبط:** [`PAYMENT_RETRY_AND_ALREADY_PAID_FA.md`](PAYMENT_RETRY_AND_ALREADY_PAID_FA.md)

نمره‌ی بررسی قبل: ۵ از ۱۰. همه‌ی موارد زیر رفع شد.

| # | شدت | مشکل | رفع | فایل |
|---|---|---|---|---|
| ۱ | بحرانی (عملکرد) | زرین‌پال و سامان آدرس برگشت را `BaseAdminSetting.PaymentUrl + id` می‌ساختند، **بدون callbackToken**؛ از ۲۰۲۶-۰۸-۱۱ callback بدون توکن رد می‌شود ⇒ هر پرداخت واقعی «ناموفق» | هر دو از `dto.CallbackUrl` استفاده می‌کنند. `CallbackUrl` حالا `…/callback/{id}/{callbackToken}` است (توکن در مسیر تا درگاه بی‌دردسر `?Authority=&Status=` اضافه کند) | `ZarinpalGateway.cs`، `SamanKishGateway.cs`، `PaymentService.StartPayment` |
| ۲ | بحرانی | پارسیان توکن callback را بدون مقایسه confirm می‌کرد (`dbAmount` حساب و رها می‌شد) ⇒ توکن پرداخت ارزان روی رزرو گران | توکن درگاه هنگام شروع در `Payment.Token` ذخیره می‌شود (`PaymentStartDto.GatewayToken`). پارسیان: توکن باید برابر باشد؛ `OrderId` و `Amount` اگر آمدند هم باید بخوانند. زرین‌پال: `Authority` باید برابر باشد. سامان: `ResNum` = شناسه‌ی پرداخت، `Token` برابر، `RefNum` قبلاً برای پرداخت موفق دیگری استفاده نشده، و بعد از verify فقط `resultCode == 0` با `TransactionDetail.OrginalAmount` برابر مبلغ؛ وگرنه `ReverseTransaction` و ناموفق | سه درگاه + `MerchantService.StartAsync` |
| ۳ | مهم | `PaymentTestMode.Enabled: true` در نسخه‌ی commit‌شده‌ی هر چهار appsettings ⇒ اگر منتشر شود همه‌ی خریدها رایگان | در محیط `Production` حالت تست فقط با `PaymentTestMode:AllowInProduction=true` روشن می‌شود؛ هشدار استارتاپ Api هم این را می‌گوید. (نسخه‌ی محلی appsettings از قبل `false` است و commit نشده.) | `PaymentTestModeService.cs`، `PaymentTestModeOptions.cs`، `Api/Program.cs` |
| ۴ | مهم | بانک پول را قطعی می‌کرد ولی اعمال روی هدف شکست می‌خورد (مبلغ هدف عوض شده، کیف پول کافی نبود، هدف لغو شده) ⇒ پول بی‌صاحب | `RefundUnappliedToWalletAsync`: فقط وقتی `GatewayStatus == SUCCESS` (درگاه واقعی، نه TEST_/کیف پول) و شکستِ **تجاری** (نه exception): ادعای اتمی `IsSuccess=false, GatewayStatus=REFUNDED_TO_WALLET` + یک ردیف کیف پول `PaymentRefund:{paymentId}` (idempotent) در تراکنش Serializable؛ ChangeTracker قبلش پاک می‌شود تا تغییرات نیمه‌کاره‌ی rollback‌شده ذخیره نشوند. پیام کاربر: `PaymentRefundedToWallet`. exception همچنان بالا می‌رود تا رفرش صفحه‌ی callback دوباره اعمال را امتحان کند | `PaymentService.ApplyAndMapPaymentAsync` |
| ۵ | مهم | درگاه تستی پاستیل (ادمین) رزرو واقعی را بدون پول پرداخت‌شده می‌کرد و سهم نماینده در تسویه می‌آمد | نتیجه‌ی درگاه تستی `TEST_SUCCESS/TEST_APPLIED` ثبت می‌شود و هدفش (`ProductOrder`, `CompanionReserve`/سبد, `PansionReserve`, `SchoolReserve`, `ConsultationPurchase`) `Permitted = true` می‌خورد ⇒ در تسویه نمی‌آید، به کیف پول هم برنمی‌گردد | `MerchantService.CallbackAsync`، `PaymentService.MarkTestTargetSettledAsync` |
| ۶ | کم | نشت جزئیات فنی: پارسیان `ex.ToString()`، زرین‌پال بدنه‌ی خام JSON و پیام exception، `StartPayment` متن exception در `item2` | پیام عمومی `ErrorOnStartPayment`/`Unsuccess` به کاربر؛ جزئیات فقط در لاگ | سه فایل |
| ۷ | کم | فیلدهای `PaymentStartDto` از بدنه‌ی کلاینت (`PaymentUrl`, `IsTestMode`, `CompanionReserveId/TripId/CargoId/...`) روی ردیف Payment می‌نشستند | `StartPayment` فیلدهای سرور/درگاه را صفر می‌کند؛ `ScopePaymentTarget` ستون‌های هدف را فقط از هدفِ بررسی‌شده پر می‌کند | `PaymentService.cs` |
| ۸ | کم | پارسیان نتیجه را POST می‌کند ولی callback فقط GET بود | `CallbackController`: GET و POST، `[IgnoreAntiforgeryToken]` (امنیت = توکن)، هر دو شکل آدرس | `Payment/Controllers/CallbackController.cs` |

### مرحله‌ی دوم (همان روز)

| # | مشکل | رفع | فایل |
|---|---|---|---|
| ۹ | سبد فروشگاه: پرداخت درگاه سفارش قبلی ⇒ دو سفارش | `CartService.ReleaseStaleStoreCheckoutsAsync` قبل از ساخت سفارش تازه: پرداخت‌های نیمه‌کاره‌ی همین کاربر برای سفارش‌های پرداخت‌نشده‌ی **همین فروشگاه** (یک ساعت اخیر) SUPERSEDED می‌شوند؛ فروشگاه‌های دیگر دست نمی‌خورند؛ اگر پرداخت قبلی همین الان در حال تأیید بانک است ⇒ پیام «چند ثانیه دیگر» | `CartService.cs` |
| ۱۰ | کاربر در زرین‌پال پرداخت کرد ولی مرورگرش به callback برنگشت ⇒ تأیید نمی‌شد، بانک پول را برمی‌گرداند، رزرو ثبت نمی‌شد | Hangfire `PaymentReconcileUnconfirmed` هر ۵ دقیقه: پرداخت‌های زرین‌پالِ بی‌نتیجه که ۱۰ تا ۲۹ دقیقه از شروعشان گذشته با Authority ذخیره‌شده و مبلغ خودشان verify می‌شوند (همان قفل VERIFYING)؛ فقط کد ۱۰۰ ثبت و اعمال می‌شود، «پرداخت نشده» هیچ اثری ندارد (کاربر شاید هنوز روی درگاه باشد) | `PaymentService.ReconcileUnconfirmedAsync`، `MerchantService.ReconcileUnconfirmedAsync`، `ZarinPalGateway.VerifyStoredAuthorityAsync`، `Api/Program.cs` |
| ۱۱ | پرداخت تأییدشده‌ی دوم برای هدفی که قبلاً پرداخت اعمال‌شده دارد بی‌صدا «APPLIED» می‌شد (کسر دوباره) | `ApplyAndMapPaymentAsync`: اگر پرداخت اعمال‌شده‌ی دیگری برای همان هدف هست، اعمال نمی‌شود و (درگاه واقعی) به کیف پول برمی‌گردد؛ پیام `PaymentDuplicateRefundedToWallet` | `PaymentService.cs` |

نکته‌ی ۱۰: endpoint تأیید زرین‌پال روی Authority پرداخت‌نشده خطا برمی‌گرداند و اثری ندارد؛ با این حال **یک‌بار در عمل ببینید**: پرداخت کوچک، بعد از صفحه‌ی موفق بانک مرورگر را ببندید، ۱۰ تا ۱۵ دقیقه بعد رزرو باید پرداخت‌شده باشد و در لاگ `reconciled` بیاید.

### کدام درگاه برای کدام نقش

`MerchantGatewayRules`: «مدیر اصلی» (نقش Id = 2، Label = `main-admin`) **فقط** درگاه تستی پاستیل را در `GET /api/Merchant` می‌بیند؛ بقیه‌ی نقش‌ها و مهمان **فقط** زرین‌پال. همین در `StartPayment` اجباری است (merchantId دیگر ⇒ «لطفاً درگاه پرداخت را انتخاب کنید»). پنل ادمین (`/api/Admin/Merchant`) همچنان همه را برای مدیریت می‌بیند. علت باگ قبلی: `CurrentUserDto.RoleEnum` همان `Roles.Label` است (`main-admin`) و کد با `"Admin"` مقایسه می‌کرد، پس درگاه تستی برای هیچ‌کس نمی‌آمد و `PastilTestGateway` هم شروع را رد می‌کرد. حالا با `RoleId` مقایسه می‌شود.

## سازگاری و «اختلال نشود»

- پرداخت‌های در جریانِ لحظه‌ی انتشار: آدرس قدیمی `?callbackToken=` همچنان پذیرفته می‌شود. پارسیانِ بدون `Payment.Token` (ساخته‌شده قبل از انتشار) مثل قبل confirm می‌شود؛ حداکثر ۳۰ دقیقه بعد همه منقضی‌اند. زرین‌پال/سامانِ قبل از انتشار به‌هرحال توکن نداشتند و قبلاً هم رد می‌شدند.
- مسیر کیف پول کامل، تست‌مود (وقتی مجاز است)، پرداخت دستی ادمین، idempotency و قفل VERIFYING دست نخورد.
- دامنه‌ی callback همان `Urls:PaymentBaseUrl` (`payment.pastil.pet`) است که پارسیان از قبل با آن کار می‌کرد. **یک‌بار با پرداخت کوچک واقعی زرین‌پال تست کنید** (آدرس برگشت حالا مسیر `/callback/{id}/{token}` دارد).
- سامان: درگاهش فرم HTML برمی‌گرداند (`paymentIsLink=false`) و وب‌اپ فعلاً آن را «تسویه‌شده» تفسیر می‌کند (سند وب‌اپ §۶.۲)؛ تا وب‌اپ اصلاح نشده، مرچنت سامان را در پنل فعال نکنید.

## تست‌ها

- `PaymentGatewayBindingTests`: Authority/توکن/OrderId/مبلغ/ResNum نامطابق در هر سه درگاه رد می‌شود و هیچ تماس شبکه‌ای انجام نمی‌شود؛ start زرین‌پال بدون CallbackUrl ناموفق؛ تست‌مود در Production بدون `AllowInProduction` خاموش و در Development/Staging روشن.
- LocalDB (اسکریپت یک‌بارمصرف با `AddApplicationServices` واقعی): پرداخت verifyشده با هدف نامعتبر ⇒ پیام بازپرداخت، `REFUNDED_TO_WALLET`، دقیقاً یک ردیف ۵۰٬۰۰۰ در کیف پول، رفرش callback بازپرداخت دوم نمی‌کند؛ پرداخت `TEST_SUCCESS` بازپرداخت نمی‌شود؛ SQL علامت تسویه برای همه‌ی انواع هدف اجرا می‌شود.
- LocalDB مرحله‌ی دوم: پرداخت تکراری ⇒ پیام بازپرداخت تکراری، یک ردیف کیف پول، اعمال نشد؛ سبد فروشگاه ۱۱ ⇒ پرداخت سفارش قبلی همان فروشگاه SUPERSEDED، پرداخت فروشگاه ۲۲ دست‌نخورده، پرداخت در حال تأیید فروشگاه ۳۳ ⇒ پیام صبر و دست‌نخورده؛ job تطبیق اجرا می‌شود و پرداختِ تأییدنشده را دست‌نخورده رها می‌کند (قفل VERIFYING پاک می‌شود).
- **تست نشده:** درگاه واقعی زرین‌پال/پارسیان/سامان (نیاز به مرچنت و پول واقعی یا sandbox).
