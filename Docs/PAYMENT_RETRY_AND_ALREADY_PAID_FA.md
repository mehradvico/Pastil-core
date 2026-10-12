# پرداخت دوباره بعد از برگشت از درگاه + «قبلاً پرداخت شده» (بک‌اند)

**تاریخ:** ۲۰۲۶-۱۰-۱۲ · **وضعیت:** پیاده‌سازی و build شده، تست واحد موجود سبز (۸۸۶)، منطق SQL روی LocalDB بررسی شد. **بدون migration** (فقط مقدار تازه‌ی `Payments.GatewayStatus`).
**سند وب‌اپ:** [`PAYMENT_GATEWAY_LOADING_WEBAPP_FA.md`](PAYMENT_GATEWAY_LOADING_WEBAPP_FA.md)

## ۱. مشکل

کاربر «پرداخت» را می‌زد، پرداخت (`IsSuccess = null`) ساخته می‌شد، ولی به درگاه نمی‌رسید یا بدون پرداخت برمی‌گشت. چنین پرداختی **هیچ‌وقت callback نمی‌گیرد** و این قفل‌ها را روشن نگه می‌داشت:

| جا | قفل قبلی | پیام |
|---|---|---|
| `ConsultationPurchaseService.PurchaseAsync` | خرید معلق با پرداخت، ۱۵ دقیقه | «پرداخت شروع شده و اطلاعات مالی قابل تغییر نیست.» (همان اسکرین‌شات) |
| کیف پول/تخفیف رزرو کلینیک، پانسیون، مدرسه، سفر، کارگو، بیمه (`HasActivePaymentAsync`) | هر پرداخت `IsSuccess == null` ⇒ **برای همیشه** | «پرداخت شروع شده…» / «…قبلاً شروع شده و امکان تغییر اطلاعات مالی نیست» |
| `SchoolReserveService.InsertAsyncDto` | هر ثبت‌نام لغونشده، حتی پرداخت‌نشده | «قبلا رزرو شده است» ⇒ صفحه‌ی نهایی‌سازی مدرسه بعد از برگشت اصلاً نمی‌توانست ادامه دهد |
| `Insert*PaymentAsyncDto` روی موردِ پرداخت‌شده/لغوشده | — | «اطلاعات وارد شده معتبر نیست» (گنگ) |

## ۲. قاعده‌ی تازه

> تلاش درگاهی که نتیجه نگرفته، کاربر را قفل نمی‌کند. فقط دو چیز جلوی پرداخت/تغییر مبلغ را می‌گیرد: **پرداخت موفق** (⇒ «قبلاً پرداخت شده»، `code: 20`) یا **callbackی که همین الان در حال verify با بانک است** (چند ثانیه، ⇒ «در حال تأیید نهایی بانک»).

کنار گذاشتن تلاش قبلی = `IsSuccess = false`, `GatewayStatus = "SUPERSEDED"`. اگر کاربر بعداً درگاه قدیمی را پرداخت کند، `CallbackPayment` آن را verify نمی‌کند و بانک مبلغ تأییدنشده را خودکار برمی‌گرداند (پیام `PaymentAttemptSuperseded`). این بخش از قبل (تغییرات commit‌نشده‌ی قبلی) در `StartPayment` وجود داشت؛ این‌بار به همه‌ی مسیرها رسید.

### ۲.۱ قفل «در حال تأیید» (VERIFYING) — جلوگیری از پولِ کسرشده‌ی اعمال‌نشده

بدون آن یک race واقعی بود: callback ‏`IsSuccess == null` را می‌خواند ⇒ verify با بانک (پول قطعی شد) ⇒ در همین فاصله تلاش تازه‌ی کاربر همان پرداخت را SUPERSEDED می‌کرد ⇒ `SaveCallbackResultAsync` صفر ردیف ⇒ پول کسر شده ولی رزرو پرداخت‌نشده.

حالا `CallbackPayment` قبل از verify، با `UPDATE … WHERE IsSuccess IS NULL AND AppliedDate IS NULL` مقدار `GatewayStatus = "VERIFYING"` می‌گذارد (`PaymentAttemptRules.TryMarkVerifyingAsync`):
- اگر صفر ردیف ⇒ کسی زودتر SUPERSEDED کرده ⇒ verify **نمی‌شود** (پیام superseded).
- اگر موفق ⇒ `SupersedePendingAsync` این ردیف را رد می‌کند و تلاش تازه پیام «در حال تأیید» می‌گیرد.
- `finally`: اگر verify بی‌نتیجه ماند (exception/خطای تنظیمات)، `ClearVerifyingAsync` علامت را برمی‌دارد تا کاربر قفل نماند.
- callbackهای هم‌زمانِ همان پرداخت هر دو علامت می‌زنند (رفتار قبلی SaveCallbackResultAsync حفظ شد).
- VERIFYING قدیمی‌تر از `PendingAttemptLifetime` (۳۰ دقیقه، همان مهلت EXPIRED در callback) دیگر محافظت نمی‌شود.

## ۳. تغییرات کد

| فایل | تغییر |
|---|---|
| `Application/Services/Order/PaymentSrv/PaymentAttemptRules.cs` | `VerifyingGatewayStatus`، `PendingAttemptLifetime`، `TargetState {Free, Verifying, Paid}`، `ReleaseForRetryAsync`، `GuardUserRetryAsync` (نتیجه‌ی آماده یا `null`)، `AlreadyPaid(...)` (کد ۲۰ + `PaymentAlreadyPaidDto`)، `HasLiveAttemptAsync` (برای تغییرات سمت نماینده)، `TryMarkVerifyingAsync`/`ClearVerifyingAsync`؛ هر دو Supersede ردیف VERIFYING تازه را رد می‌کنند |
| `Application/Services/Order/PaymentSrv/Dto/PaymentAlreadyPaidDto.cs` | جدید: `{ CallBackTypeLabel, CallBackId }` |
| `PaymentService.StartPayment` | `ReleaseForRetryAsync` به‌جای HasSuccessful + Supersede؛ Paid ⇒ `AlreadyPaid`، Verifying ⇒ پیام |
| `PaymentService.CallbackPayment` | قفل VERIFYING (§۲.۱)؛ مهلت ۳۰ دقیقه از `PendingAttemptLifetime` |
| `PaymentService.Insert{Reserve,PansionReserve,SchoolReserve,CompanionReserveBatch,Trip,Cargo,CompanionInsurancePackageSale}PaymentAsyncDto` | `GuardRetryAsync`: لغوشده ⇒ `PaymentTargetCancelled`، پرداخت‌شده ⇒ پیام اختصاصی + کد ۲۰، وگرنه آزادسازی تلاش‌ها **قبل از** بازمحاسبه‌ی `WalletPrice` روی خود رزرو (تا snapshot یک پرداختِ در حال verify عوض نشود). رزرو عضو سبد: پرداخت سبد هم آزاد می‌شود؛ سبد: پرداخت تکی اعضا هم |
| `ConsultationPurchaseService.PurchaseAsync` | قفل ۱۵ دقیقه‌ای حذف. همه‌ی خریدهای `PendingPayment` همین کاربر: Supersede پرداخت‌ها ⇒ اگر یکی موفق ⇒ `ConsultationAlreadyPurchased` + کد ۲۰؛ اگر VERIFYING ⇒ پیام؛ وگرنه `Cancelled` با `CancelReason = "Superseded"` (ظرفیت اسلات آزاد). ساعت‌دار: همان پکیج + همان `ScheduledStart` پرداخت‌شده ⇒ کد ۲۰. فوری: «مشاوره‌ی فعال دارید» حالا کد ۲۰ + شناسه‌ی همان خرید |
| `CompanionReserveService`, `PansionReserveService`, `SchoolReserveService`, `TripService`, `CargoService`, `CompanionInsurancePackageSaleService` | `HasActivePaymentAsync` ⇒ `GuardFinancialChangeAsync` (SetRebate / ClearRebate / SetWallet) |
| `CompanionReservePackageItemService` | سمت نماینده/ادمین: فقط تلاش «زنده» (کمتر از ۳۰ دقیقه) یا موفق قفل می‌کند (`HasLiveAttemptAsync`)؛ کاربر شاید همین الان روی درگاه باشد، پس آن را SUPERSEDED نمی‌کنیم |
| `SchoolReserveService.InsertAsyncDto` | ثبت‌نام پرداخت‌نشده‌ی همان دوره + پت ⇒ همان ثبت‌نام با موفقیت برمی‌گردد (تلاش‌ها آزاد، `Price/PaymentPrice` = قیمت فعلی دوره، کیف پول/تخفیف صفر). پرداخت‌شده ⇒ کد ۲۰ با `data` = `SchoolReserveDto` |
| `PastilAiPlanService.PurchaseAsync` | پرداخت‌های معلق PastilAI کاربر SUPERSEDED، اشتراک‌های `PendingPayment` ⇒ `PaymentFailed` (دیگر با برگشت به درگاه قدیمی دو پلن خریده نمی‌شود) |
| `Resource/Notification*.resx` + Designer | `PaymentPreviousAttemptVerifying`، `PaymentTargetCancelled`، `ConsultationAlreadyPurchased` |

## ۴. قرارداد پاسخ

```jsonc
// قبلاً پرداخت شده
{ "isSuccess": false, "code": 20, "messages": [{ "item1": "این رزرو قبلاً پرداخت شده است.", "item2": "" }],
  "data": { "callBackTypeLabel": "CompanionReserve", "callBackId": "1234" } }
// تأیید بانک در جریان
{ "isSuccess": false, "code": 0, "messages": [{ "item1": "پرداخت قبلی شما همین الان در حال تأیید نهایی بانک است؛ چند ثانیه دیگر دوباره تلاش کنید.", "item2": "" }] }
```

## ۵. چیزهایی که عمداً دست نخورد

- **سبد خرید فروشگاه**: هر checkout سفارش تازه می‌سازد و قفلی نداشت. اگر کاربر درگاه سفارش قبلی را هم پرداخت کند دو سفارش ثبت می‌شود؛ Supersede سراسری خطرناک بود (کاربری که هم‌زمان از دو فروشگاه خرید می‌کند). اگر لازم شد: Supersede فقط سفارش‌های پرداخت‌نشده‌ی همان کاربر **و همان فروشگاه**.
- `ProductOrder` و `Wallet` مسیر Supersede هدف‌محور قبلی `StartPayment` را دارند (بدون CallBackId برای Wallet ⇒ بدون تغییر).
- پیام‌های قدیمی `*FinancialDataLocked` در resx ماندند (دیگر استفاده نمی‌شوند به‌جز سمت نماینده در PackageItem).

## ۶. استقرار

- **Api و Payment هر دو** باید با هم منتشر شوند (هر دو از `Application` استفاده می‌کنند؛ قفل VERIFYING در Payment اجرا می‌شود، Supersede در Api).
- migration ندارد.
- پیش از commit: `backend/scripts/Test-NoTrackedSecrets.ps1`.

## ۷. بررسی‌ها

- `dotnet build` Application / Api / Payment بدون خطا؛ `Application.Tests` ‏۸۸۶/۸۸۶ سبز.
- اسکریپت یک‌بارمصرف روی LocalDB (دیتابیس موقت، بعدش حذف شد) ۲۰ حالت `PaymentAttemptRules` را با SQL واقعی چک کرد: رهاشده ⇒ Free+SUPERSEDED، موفق ⇒ Paid، VERIFYING تازه ⇒ Verifying و دست‌نخورده، VERIFYING قدیمی ⇒ Free، علامت‌زدن بعد از Supersede شکست می‌خورد، Supersede ردیف علامت‌خورده را رد می‌کند، Clear فقط ردیف بی‌نتیجه، پنجره‌ی زنده‌ی ۳۰ دقیقه، Supersede کاربرمحور فقط همان کاربر/نوع، و شکل نتیجه‌ی کد ۲۰.
- **تست نشده:** جریان کامل خرید مشاوره و callback واقعی زرین‌پال روی دیتابیس واقعی — قبل از انتشار یک‌بار در حالت تست درگاه سناریوهای §۷ سند وب‌اپ را اجرا کنید.
