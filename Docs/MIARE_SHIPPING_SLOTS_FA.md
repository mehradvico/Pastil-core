# ارسال فروشگاه با میاره: روز و بازه‌ی تحویل + تأیید فروشنده

وضعیت: بک‌اند و پنل پیاده‌سازی شد (۱۴۰۵/۰۷/۱۳، 2026-10-05). **Migration نوشته شده ولی اجرا نشده.** مستند اپ‌ها: `MIARE_SHIPPING_WEBAPP_FA.md` و `MIARE_SHIPPING_SELLER_APP_FA.md`.

## جریان کلی

1. مشتری سبد را می‌بندد و به مرحله‌ی «اطلاعات ارسال» می‌رسد (آدرس، بعد برای **هر فروشگاه** یک کارت).
2. برای هر فروشگاه `POST /api/EndUser/ShippingQuote` قیمت زنده‌ی میاره را می‌دهد (هزینه‌ی پیکِ همان فروشگاه که مشتری می‌پردازد).
   - اگر `Shipping:MiareOnly=true` و میاره آدرس را **پوشش بدهد**: فقط میاره برمی‌گردد (`requiresSlot=true`).
   - اگر آدرس **خارج از محدوده‌ی میاره** باشد (یا استعلام میاره خطا بدهد): روش‌های دیگر فروشگاه (مثلاً **پست**) برمی‌گردد با `requiresSlot=false`؛ **برای این‌ها روز و بازه نمایش داده نمی‌شود.**
3. اگر `requiresSlot`: مشتری برای **همان فروشگاه** از `GET /api/EndUser/ShippingSlot?storeId=` روز و بازه را انتخاب می‌کند. بازه‌ها بر اساس **حداکثر زمان آماده‌سازی همان فروشگاه** (`Store.MaxPreparationMinutes`) فیلتر می‌شوند: بازه فقط وقتی نمایش داده می‌شود که کالا بعد از «الان + آماده‌سازی + ۳۰ دقیقه اطمینان» هنوز بتواند قبل از «پایان بازه − ۶۰ دقیقه» به پیک برسد. اگر آماده‌سازی یک روز است، برای صبح فردا بازه‌ای نشان داده نمی‌شود.
4. `POST /api/EndUser/ShippingSelection` با `{ quoteToken, slotId, slotDate }`. میاره بدون بازه‌ی معتبر انتخاب نمی‌شود؛ ظرفیت، افق و زمان آماده‌سازی هنگام پرداخت (`CartService`) دوباره چک می‌شود. **قیمت = برآورد میاره** (`/estimate/price/`) و برای همه‌ی روزها/بازه‌ها یکی است؛ برچسب یا قیمت متفاوت برای روز نداریم.
5. بعد از پرداخت: برای هر فروشگاهِ میاره یک `Shipment` با وضعیت **`AwaitingSellerConfirm` (8)** ساخته می‌شود؛ سفر میاره هنوز ساخته نمی‌شود. به کاربران فروشگاه پوش `PushShipmentAwaitingSeller` می‌رود و برای مشتری یک **کد تحویل ۵ رقمی** ساخته می‌شود.
6. **مرحله‌ی ۱ فروشنده — تأیید سفارش** (`POST /api/Seller/Shipment/Confirm`) تا `SellerConfirmDeadlineUtc` ← وضعیت **`Preparing` (9)** («در حال آماده‌سازی»). سفر هنوز ساخته نمی‌شود.
7. **مرحله‌ی ۲ فروشنده — «آماده تحویل به پیک»** (`POST /api/Seller/Shipment/Ready`) تا `ReadyDeadlineUtc` (= پایان بازه − ۶۰ دقیقه) ← **همان لحظه** سفر میاره با `pickup.deadline = max(شروع بازه − ۳۰ دقیقه، الان + ۱۵ دقیقه)` و `delivery_code` ساخته می‌شود (وضعیت `Requested`). پیک زودتر از موعد نمی‌آید (کالا بیرون از بازه نمی‌رسد) و دیرتر از آخرین ساعت مفید هم نه.
8. هشدارها: ۱۵ دقیقه به مهلت مرحله‌ی ۱ و ۳۰ دقیقه به مهلت مرحله‌ی ۲ پوش یادآوری به فروشنده. بعد از هر مهلت: مرسوله `Failed` (`SellerConfirmExpired` / `SellerNotReady`)، `CancelRequestDate` سفارش ثبت می‌شود (صف «درخواست لغو» ادمین) و به مشتری پوش می‌رسد.
9. پیک کالا را می‌برد، مشتری کد تحویل را به پیک می‌گوید؛ تحویل با وب‌هوک ثبت می‌شود (جزئیات: `MIARE_SHIPPING_LIFECYCLE_FA.md`).

> وضعیت‌های ۸ و ۹ و ۲ و ۳ (قبل از گرفتن کالا) داخلی‌اند: مشتری فقط «در حال آماده‌سازی» می‌بیند.

## قواعد زمانی (قابل تنظیم در `Shipping:` — پیش‌فرض‌ها در `Api/appsettings.json`)

| تنظیم | پیش‌فرض | معنی |
|---|---|---|
| `MiareOnly` | true | فقط میاره؛ پست/روش‌های دیگر فقط وقتی که میاره پوشش ندهد |
| `SlotHorizonDays` | 3 | از **فردا** چند روز قابل انتخاب است (تحویل همان روز نداریم) (به‌علاوه‌ی روزهایی که آماده‌سازی فروشگاه عملاً می‌خورد) |
| `PrepBufferMinutes` | 30 | فاصله‌ی اطمینان بعد از زمان آماده‌سازی فروشگاه (تأیید فروشنده، رسیدن پیک و ...) |
| `SellerConfirmMinutes` | 60 | مهلت تأیید فروشنده بعد از پرداخت |
| `MinDeliveryMinutes` | 60 | حداقل زمان لازم از تحویل به پیک تا پایان بازه (آخرین ساعت تحویل به پیک = پایان بازه − این مقدار) |
| `PickupLeadMinutes` | 30 | ساعت پیش‌فرض تحویل به پیک = شروع بازه − این مقدار (حداقل الان + ۱۵ دقیقه) |

مهلت تأیید سفارش (مرحله‌ی ۱) = کمینه‌ی (زمان پرداخت + `SellerConfirmMinutes`) و (پایان بازه − `MinDeliveryMinutes` − ۱۰ دقیقه). مهلت «آماده تحویل به پیک» (مرحله‌ی ۲) = پایان بازه − `MinDeliveryMinutes`.

نکته: میاره **فقط ددلاین تحویل‌گیری از فروشگاه** (`pickup.deadline`) را می‌گیرد و ددلاینِ تحویل به مشتری ندارد. پس «رساندن داخل بازه» را با ساعتِ تحویل به پیک، مهلت تأیید و مانیتور وب‌هوک تضمین می‌کنیم، نه با فیلدی در میاره.

## داده (migration `AddShippingSlotsAndSellerConfirm` + `SeedShippingSlotsAndSellerPushTypes`)

- جدول `ShippingSlots` (روز هفته `DayOfWeek`: یکشنبه=۰ … شنبه=۶، `StartTime`/`EndTime` به وقت تهران، `Capacity`، `Active`، `Deleted`).
- `CartStores` و `ProductOrderStores`: `ShippingSlotId`, `ShippingSlotDate` (date).
- `Shipments`: `SlotStartUtc/SlotEndUtc` (snapshot)، `SellerConfirmDeadlineUtc`، `SellerConfirmedAtUtc`، `SellerReminderSentAtUtc`، `PickupDeadlineUtc`، `DeliveryCode`.
- `ShipmentStatusEnum.AwaitingSellerConfirm = 8`، `Preparing = 9`؛ ستون‌های `Shipments.ReadyDeadlineUtc/ReadyAtUtc/ReadyReminderSentAtUtc` و `Stores.MaxPreparationMinutes` (پیش‌فرض ۱۲۰؛ migration `AddStorePreparationAndShipmentReady`، اسکریپت `scripts/AddShipmentPreparation.sql` بعد از `AddShipmentLifecycle.sql`).
- Seed: بازه‌های پیش‌فرض هر ۷ روز (۹–۱۳، ۱۳–۱۷، ۱۷–۲۱ با ظرفیت ۱۰) فقط اگر جدول خالی باشد + سه PushType: `94 PushShipmentAwaitingSeller`، `95 PushShipmentSellerReminder`، `96 PushShipmentSellerExpiredUser`.
- اسکریپت idempotent: `scripts/AddShippingSlotsAndSellerConfirm.sql` (**قبل از آپلود Api اجرا شود**؛ بدون آن ستون‌های جدید نیستند و پرداخت/سبد خطا می‌دهد).

## API

| متد و مسیر | کاربر | کار |
|---|---|---|
| `GET /api/EndUser/ShippingSlot?storeId=` | مشتری | روزها و بازه‌های **همان فروشگاه** (بر اساس زمان آماده‌سازی‌اش) با `remaining/available` |
| `POST /api/EndUser/ShippingQuote` `{storeId}` | مشتری | قیمت‌ها؛ فیلد جدید `requiresSlot` |
| `POST /api/EndUser/ShippingSelection` `{quoteToken, slotId?, slotDate?}` | مشتری | انتخاب؛ برای میاره `slotId` و `slotDate` (روز، مثل `2026-10-06`) الزامی |
| `GET /api/EndUser/ProductOrder/{id}` | مشتری | در `productOrderStores[]`: `shipmentStatus`, `shipmentSlotStartUtc/EndUtc`, `shipmentPickupDeadlineUtc`, `shipmentTrackingCode`, `shipmentDeliveryCode` |
| `GET /api/Seller/Shipment/Awaiting` | فروشنده | سفارش‌های منتظر اقدام: `step` ۱ (تأیید) یا ۲ (آماده تحویل به پیک) با مهلت‌ها |
| `POST /api/Seller/Shipment/Confirm` `{productOrderStoreId}` | فروشنده | مرحله‌ی ۱: تأیید سفارش (در حال آماده‌سازی) |
| `POST /api/Seller/Shipment/Ready` `{productOrderStoreId}` | فروشنده | مرحله‌ی ۲: آماده تحویل به پیک ← ساخت سفر میاره |
| `GET/POST/PUT/DELETE /api/Admin/ShippingSlot` | ادمین | مدیریت بازه‌ها (پنل: «بازه‌های تحویل») |

`shipmentDeliveryCode` فقط در پاسخ مشتری پر است؛ در پاسخ فروشنده/ادمین همیشه `null` است.

## استقرار (ترتیب)

1. اجرای `scripts/AddShippingSlotsAndSellerConfirm.sql` (و قبلش اسکریپت‌های قبلی، اگر اجرا نشده‌اند).
2. هر فروشگاه باید یک روش ارسال (`Delivery`) با `ShippingProvider = Miare`، `LivePricing = true`، `AllowPrepaid = true` و **مختصات** (`Store.Location`) داشته باشد، وگرنه میاره برایش پیشنهاد نمی‌شود. برای «پست» هم یک `Delivery` ثابت‌قیمت (`ShippingProvider = None`) باید تعریف باشد تا بیرون از محدوده‌ی میاره جایگزین شود.
3. `Shipping:Miare:*` و کلید میاره (env) و فعال‌سازی PoD (کد تحویل third-party) در حساب میاره — تأیید شده.
4. آدرس وب‌هوک میاره (مثل قبل) ثبت شود.
5. بنر/متن‌ها: پوش‌ها از پنل (الگوی پوش) قابل ویرایش‌اند.

## محدودیت‌ها و کارهای بعدی

- **استرداد پول خودکار نیست.** بعد از مهلت، سفارش فقط وارد صف «درخواست لغو» ادمین می‌شود (جریان فعلی لغو/استرداد). در کد پروژه ابزار استردادِ خودکار سفارش فروشگاهی وجود ندارد.
- ظرفیت بازه هنگام انتخاب و قبل از پرداخت چک می‌شود؛ چند پرداخت هم‌زمان ممکن است ظرفیت را چند واحد رد کنند (قفل سخت ندارد).
- بازه‌ها طبق خواست شما با هم‌پوشانی در یک روز **رد** می‌شوند؛ بازه‌ها را پشت‌سرهم و بدون تداخل تعریف کنید (مثل ۹–۱۳، ۱۳–۱۷، ۱۷–۲۱).
- ظرفیت بازه‌ها سراسری است و به منطقه/فروشگاه وابسته نیست؛ سقف ۱۰ سفر هم‌زمان میاره برای هر منطقه است.
- نقشه‌ی زنده‌ی پیک، فاصله و زمان تقریبی رسیدن و پوش «تحویل گرفتید؟» هنوز ساخته نشده‌اند (طرح در `project-miare-delivery-slots`).
- در `ShipmentService.ConfirmBySellerAsync` اگر میاره خطا بدهد، مرسوله منتظر تأیید می‌ماند و فروشنده تا پایان مهلت می‌تواند دوباره امتحان کند.
- `bill_number` میاره حالا `"{orderId}-{productOrderStoreId}"` است (میاره شماره‌ی تکراری در سفر فعال را رد می‌کند)، و آدرس محل تحویل‌گیری آدرس **فروشگاه** است (قبلاً اشتباهاً آدرس گیرنده بود).

## محدوده‌ی ارسال میاره (فاصله)
`area_coverage` در `/estimate/price/` طبق مستند میاره فقط **مبدأ** (فروشگاه) را در محدوده می‌سنجد، نه مقصد؛ پس آدرس شهر دیگر هم «پوشش‌داده‌شده» برمی‌گشت و قیمت (مثلاً ۱۳۰ هزار) می‌داد. حالا اگر فاصله‌ی هوایی فروشگاه تا آدرس از `Shipping:Miare:MaxDistanceKm` (پیش‌فرض ۴۵ کیلومتر؛ صفر = بدون محدودیت) بیشتر باشد، میاره اصلاً استعلام نمی‌شود و روش‌های دیگر فروشگاه (پست) می‌آید. قیمت میاره طبق مستندش «فقط هزینه‌ی course» است و هزینه‌ی trip و Boost را شامل نمی‌شود؛ مبلغ نهایی واقعی با webhook (`delivery_cost`) می‌آید.
