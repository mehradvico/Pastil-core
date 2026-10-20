# ردیابی پیک میاره: مشخصات پیک، مراحل ارسال و موقعیت (ادمین، فروشنده، مشتری)

وضعیت: **بک‌اند و پنل ساخته شد (۲۵ مهر ۱۴۰۵)**؛ **migration نوشته شده ولی روی دیتابیس اجرا نشده**. اپ فروشنده و وب‌اپ/اپ مشتری با تیم‌های خودشان (بخش ۶ و ۷).
ادامه‌ی `MIARE_SHIPPING_LIFECYCLE_FA.md` و `MIARE_EXPRESS_SELLER_APP_FA.md`.

## ۱) چه چیزی اضافه شد

وب‌هوک میاره (`POST /api/webhooks/miare`) تا قبل از این فقط **وضعیت مرسوله** را عوض می‌کرد و هر چیز دیگرِ پیلود (نام پیک، تلفن، موقعیت) دور ریخته می‌شد. حالا:

1. **آخرین اطلاعات پیک** (نام، تلفن، مختصات، زمان آخرین موقعیت) روی خود `Shipment` ذخیره می‌شود.
2. **هر رویداد مهم** در جدول جدید `ShipmentEvents` می‌ماند (وضعیت میاره، زمان، پیک، مختصات، متن خام پاک‌سازی‌شده) تا مراحل و زمان‌هایشان نمایش داده شود و بشود مشکل را عیب‌یابی کرد.
3. سه خروجی:
   - **ادمین (پنل):** صفحه‌ی سفارش ← کارت «مرسوله‌ی پیک (میاره)»: نام، تلفن، آخرین موقعیت با لینک نقشه، تاریخچه‌ی رویدادها با متن خام.
   - **فروشنده (اپ/وب):** `GET api/Seller/Shipment/Tracking?productOrderStoreId=`.
   - **مشتری (وب‌اپ/اپ):** `GET api/EndUser/ShipmentTracking?productOrderId=`.

## ۲) ⚠️ پیش‌شرط دیپلوی: اول SQL، بعد Api

ستون‌های جدید روی `Shipments` در **همه‌ی** کوئری‌های مرسوله خوانده می‌شوند. اگر Api جدید قبل از اجرای اسکریپت بالا بیاید، ثبت سفارش/پرداخت/لیست فروشنده که به مرسوله دست می‌زنند خطا می‌دهند.

ترتیب:
1. `backend/scripts/AddShipmentCourierTracking.sql` را روی دیتابیس اجرا کنید (idempotent؛ یک تراکنش؛ ۷ ستون روی `Shipments` + جدول `ShipmentEvents`). Migration: `20261017170431_AddShipmentCourierTracking`.
2. Api (و پنل) را دیپلوی کنید. Payment لازم نیست.
3. ادمین‌های غیراصلی: کنترلر `OrderShipment` را در صفحه‌ی نقش‌ها به آن‌ها بدهید (مثل قبل).

## ۳) ⚠️ ساختار وب‌هوک میاره هنوز تأیید نشده

مستند فنی میاره (miare.ir/docs) در دسترس ما نبود و نمونه‌ی واقعی پیلود هم هنوز نرسیده. بنابراین:
- فقط این‌ها مطمئن‌اند و از قبل در کد بودند: `id` (شناسه‌ی سفر)، `state`، `delivery_cost`.
- نام و تلفن و موقعیت پیک را کد **تحمل‌پذیر** می‌خواند: شیء `courier` (یا `driver`/`rider`/`delivery_man`/`agent`/`assignee`) با کلیدهای `name`/`full_name`/`first_name`+`last_name`، `phone`/`mobile`/`phone_number`، و `location`/`current_location`/… (شیء `{lat,lng}` یا آرایه‌ی دوتایی)؛ یا کلیدهای تخت با پیشوند `courier_`؛ و همین‌ها داخل `data`/`trip`/`payload`.
- **عمداً هرگز** کلید بدون پیشوند روی ریشه (`name`، `phone`، `location`) خوانده نمی‌شود، چون ممکن است مال گیرنده/فرستنده باشد و شماره‌ی مشتری به‌اشتباه «تلفن پیک» نمایش داده نشود.
- متن خام هر رویداد (بعد از حذف `delivery_code` و کلیدهای مشابه) در `ShipmentEvents.PayloadJson` ذخیره می‌شود و ادمین در پنل می‌بیند.

**بعد از اولین سفر واقعی حتماً چک کنید:**

```sql
SELECT TOP 20 e.Id, e.ShipmentId, e.State, e.ReceivedAtUtc, e.CourierName, e.Latitude, e.Longitude, e.PayloadJson
FROM ShipmentEvents e ORDER BY e.Id DESC;
```

- اگر در `PayloadJson` اطلاعات پیک هست ولی `CourierName`/`Latitude` خالی است، نام فیلدهای میاره با حدس ما نمی‌خواند: یک `PayloadJson` را بدهید تا فهرست کلیدها در `ShipmentCourierRules` (`CourierObjectKeys`، `NameKeys`، `PhoneKeys`، `LocationKeys`) دقیق شود. تغییر فقط یک کلاس خالص + تست است و **بدون migration**.
- پوش/پیامک و منطق وضعیت از این استخراج مستقل‌اند و اگر استخراج شکست بخورد خراب نمی‌شوند (فقط لاگ Warning: `Recording Miare courier info … failed`).

## ۴) داده

### `Shipments` (ستون‌های جدید، همه nullable)
| ستون | معنی |
|---|---|
| `CourierName` (۲۰۰) / `CourierPhone` (۴۰) | آخرین نام/تلفن پیک که میاره فرستاد. رویدادی که این‌ها را نداشته، مقدار قبلی را پاک نمی‌کند |
| `CourierLatitude` / `CourierLongitude` | آخرین موقعیت (WGS84). مختصات ۰،۰ و خارج از بازه رد می‌شود |
| `CourierLocationAtUtc` | لحظه‌ی دریافت آن موقعیت |
| `LastMiareState` (۶۰) / `LastMiareEventAtUtc` | آخرین `state` وب‌هوک (مثل `pickup`) و زمانش |

### `ShipmentEvents` (جدول جدید)
`Id, ShipmentId (FK cascade), State, ReceivedAtUtc, CourierName, Latitude, Longitude, PayloadHash (SHA-256), PayloadJson (≤ ۶۰۰۰ نویسه)`؛ ایندکس `(ShipmentId, ReceivedAtUtc)`.

قواعد ذخیره (`ShipmentCourierRules.ShouldStoreEvent`) تا جدول با وب‌هوک‌های پرتکرار پر نشود:
- متن یکسان با آخرین رویداد همان state = **retry تکراری میاره** (تا ۵ بار) ⇒ ذخیره نمی‌شود.
- state تازه ⇒ همیشه ذخیره می‌شود.
- همان state با موقعیت تازه ⇒ حداکثر یک ردیف در هر **۶۰ ثانیه**؛ ولی `Shipments.Courier*` همیشه به‌روز می‌شود.
- سقف ۳۰۰ رویداد برای هر مرسوله؛ بعد از آن فقط تغییر state.

## ۵) قواعد حریم خصوصی: هر مخاطب چه چیزی می‌بیند

| | ادمین | فروشنده | مشتری |
|---|---|---|---|
| نام پیک | همیشه | فقط وقتی پیک **به‌سمت فروشگاه** می‌آید (وضعیت ۳) | در وضعیت ۳ و ۴؛ **فقط نام کوچک** |
| تلفن پیک | همیشه | فقط وضعیت ۳ (هماهنگی تحویل در فروشگاه) | فقط وضعیت ۴ (کالا را برداشته و می‌آید) |
| موقعیت زنده + فاصله | همیشه (آخرین موقعیت معلوم) | فقط وضعیت ۳؛ فاصله تا فروشگاه | وضعیت ۳: فاصله تا فروشگاه؛ وضعیت ۴: فاصله تا آدرس مشتری |
| مختصات آدرس مشتری | — | **هرگز** | فقط مال خودش |
| متن خام وب‌هوک | بله (پنل) | هرگز | هرگز |
| کد تحویل | هرگز | هرگز | بعد از وضعیت ۴، از `shipmentDeliveryCode` سفارش (مثل قبل) |

بعد از **تحویل / لغو / ناموفق** نام، تلفن و موقعیت برای فروشنده و مشتری برنمی‌گردد (`courier = null`).
این‌ها در `ShipmentCourierRules.ShowName/ShowPhone/ShowLocation` متمرکزند و ماتریس‌شان تست دارد.

## ۶) API

همه‌ی پاسخ‌ها مثل بقیه‌ی بک‌اند HTTP 200 با `{ isSuccess, code, messages, data }` هستند. زمان‌ها UTC با `Z`. **خطای نمایش را از `isSuccess` بخوانید.**

### مشتری: `GET api/EndUser/ShipmentTracking?productOrderId={شناسه یا شماره‌ی سفارش}`

- لیست مرسوله‌های **پیکی (میاره)** سفارش، به‌ازای هر فروشگاه یکی. سفارش بدون پیک (پست) یا سفارش دیگران = `data: []` (وجود سفارش لو نمی‌رود).
- از وضعیت‌های «در حال آماده‌سازی» به بعد جواب دارد (قبل از پرداخت مرسوله‌ای نیست).

```json
{
  "isSuccess": true, "code": 0, "messages": [],
  "data": [{
    "shipmentId": 123, "productOrderStoreId": 456, "storeId": 5, "storeName": "پی کی پت",
    "provider": "Miare",
    "status": 4, "statusKey": "PickedUp",
    "isExpress": true, "isFinal": false, "isLive": true,
    "currentStepKey": "picked_up",
    "headline": "پیک سفارش را از فروشگاه تحویل گرفت و در راه است.",
    "steps": [
      { "key": "preparing",         "title": "فروشگاه در حال آماده‌سازی سفارش است.",        "done": true,  "current": false, "atUtc": "2026-10-25T10:05:00Z" },
      // … دو مرحله‌ی میانی (courier_requested و courier_assigned) با همین ساختار
      { "key": "picked_up",         "title": "پیک سفارش را از فروشگاه تحویل گرفت و در راه است.", "done": false, "current": true,  "atUtc": "2026-10-25T10:41:00Z" },
      { "key": "delivered",         "title": "سفارش تحویل داده شد.",                          "done": false, "current": false, "atUtc": null }
    ],
    "courier": {
      "name": "علی", "phone": "09120000000", "canCall": true,
      "hasLocation": true, "latitude": 35.7101, "longitude": 51.4102,
      "locationUpdatedAtUtc": "2026-10-25T10:50:08Z", "locationAgeSeconds": 22, "locationStale": false,
      "distanceKm": 3.4, "distanceTarget": "destination"
    },
    "store": { "latitude": 35.70, "longitude": 51.39 },
    "destination": { "latitude": 35.73, "longitude": 51.42 },
    "plannedPickupUtc": "2026-10-25T10:50:00Z", "deliveredAtUtc": null,
    "lastUpdateUtc": "2026-10-25T10:50:08Z", "pollSeconds": 15
  }]
}
```

### فروشنده: `GET api/Seller/Shipment/Tracking?productOrderStoreId={شناسه}`

- همان ساختار، ولی **بدون** `destination` (مختصات مشتری)، نام کامل پیک و فقط وقتی پیک به‌سمت فروشگاه می‌آید (وضعیت ۳).
- سفارش فروشگاه دیگر ⇒ `isSuccess=false` با پیام «یافت نشد». `productOrderStoreId` همان مقدار `POST Ready` / `Awaiting` است.

### ادمین: `GET api/Admin/OrderShipment?productOrderId={شناسه}` (قبلاً بود؛ غنی‌تر شد)
هر مرسوله حالا این‌ها را هم دارد: `courierName, courierPhone, courierLatitude, courierLongitude, courierLocationAtUtc, lastMiareState, lastMiareEventAtUtc` و `events[]` (آخرین ۶۰ رویداد: `state, receivedAtUtc, courierName, latitude, longitude, payloadJson`).

### فیلدها
| فیلد | معنی |
|---|---|
| `status` / `statusKey` | ۸،۹ آماده‌سازی؛ ۲ پیک درخواست شد؛ ۳ پیک تعیین شد؛ ۴ در راه؛ ۵ تحویل شد؛ ۶ لغو؛ ۷ ناموفق |
| `isFinal` | ۵/۶/۷: دیگر تغییر نمی‌کند ⇒ **polling را متوقف کنید** |
| `isLive` | ۳ یا ۴: موقعیت زنده معنی دارد |
| `steps[]` | ۵ مرحله‌ی ثابت (`preparing, courier_requested, courier_assigned, picked_up, delivered`)؛ در لغو/ناموفق یک مرحله‌ی پایانی (`cancelled` / `failed` / `returning`) اضافه و `current` می‌شود و مراحلِ رسیده‌شده `done` می‌مانند. `atUtc` فقط برای مرحله‌ی انجام‌شده/جاری |
| `headline` | متن فارسی مرحله‌ی جاری (از Resource؛ نیازی به ترجمه‌ی سمت کلاینت نیست) |
| `courier` | `null` اگر پیک هنوز تعیین نشده یا اطلاعاتش برای این مخاطب/وضعیت مجاز نیست؛ **UI باید با `null` درست کار کند** |
| `courier.locationStale` | موقعیت قدیمی‌تر از ۱۰ دقیقه؛ روی نقشه «آخرین موقعیت معلوم» نشان دهید، نه «زنده» |
| `courier.distanceKm` | فاصله‌ی **هوایی** (نه مسیر) و رُندشده؛ ETA نداریم (میاره زمان‌بندی نمی‌دهد) |
| `pollSeconds` | هر چند ثانیه دوباره بخوانید (۱۵) |
| `plannedPickupUtc` | ساعت تقریبیِ آمدن پیک به فروشگاه (`pickup.deadline` واقعی ساخته‌شده) |

## ۷) راهنمای اپ‌نویس‌ها

> **مستندهای کامل و جدا برای هر تیم** (کد، قالب، حالت‌ها، چک‌لیست تست):
> - وب‌اپ (مشتری + بک‌آفیس فروشنده‌ی وب‌اپ): [MIARE_COURIER_TRACKING_WEBAPP_FA.md](MIARE_COURIER_TRACKING_WEBAPP_FA.md)
> - اپ نماینده (فلاتر، بخش فروشگاه): [MIARE_COURIER_TRACKING_NEMAYANDE_APP_FA.md](MIARE_COURIER_TRACKING_NEMAYANDE_APP_FA.md)
>
> خلاصه‌ی زیر فقط مرور سریع است.

### اپ فروشنده (فلاتر / وب)
- در کارت سفارشِ ارسال‌شده با پیک (اکسپرس و روز/بازه)، وقتی `status ≥ 2` دکمه‌ی **«ردیابی پیک»** بگذارید ← `GET api/Seller/Shipment/Tracking`.
- صفحه: نوار مراحل از `steps`؛ کارت پیک اگر `courier != null`: نام، دکمه‌ی **تماس** (`tel:`، فقط اگر `canCall`)، «فاصله تا فروشگاه: x کیلومتر» (`distanceKm`)، و نقشه‌ی کوچک با دو نقطه‌ی `store` و `courier`.
- وقتی پیک تعیین شد (وضعیت ۳) از **پوش** استفاده نمی‌کنیم؛ صفحه را باز کنید و polling کنید. تا `isFinal=true` یا بستن صفحه هر `pollSeconds` ثانیه؛ اپ در پس‌زمینه polling نکند.
- بعد از `status = 4` فروشنده دیگر تلفن/موقعیت پیک را نمی‌بیند؛ همان `headline` («پیک سفارش را تحویل گرفت») کافی است.
- یادآوری: کارت «درخواست پیک» و مسیر `Awaiting` بدون تغییر است (`MIARE_SELLER_APP_AWAITING_LIST_FA.md`).

### وب‌اپ / اپ مشتری
- صفحه‌ی سفارش، بخش «پیگیری ارسال»: برای هر آیتمِ لیست (هر فروشگاه) کارت جدا.
- **نوار مراحل** از `steps` + `headline` بالای کارت. `atUtc` را به وقت تهران نشان دهید.
- **وقتی `isLive`:** نقشه با سه نقطه: فروشگاه (`store`)، پیک (`courier.latitude/longitude`)، آدرس شما (`destination`). با هر بار polling فقط نشانگر پیک را حرکت دهید (انیمیشن نرم)، نه کل نقشه را. چیپ «پیک تا شما x کیلومتر فاصله دارد» (`distanceKm` با `distanceTarget = destination`)؛ برای `store` عبارت «پیک تا فروشگاه x کیلومتر».
- `locationStale = true` ⇒ نشانگر خاکستری و متن «آخرین موقعیت: n دقیقه پیش».
- **تماس:** دکمه فقط وقتی `courier.canCall` (یعنی کالا دست پیک است).
- **کد تحویل** را همان‌طور که تا الان بود از `shipmentDeliveryCode` سفارش نشان دهید (بعد از وضعیت ۴)؛ این API آن را برنمی‌گرداند.
- `isFinal = true`: polling را قطع کنید. `delivered` ⇒ پیام تحویل + همان پرسش «آیا تحویل گرفتید؟» که از قبل بود.
- `courier == null` در وضعیت ۲/۳ ⇒ فقط مراحل را نشان دهید («در انتظار تخصیص پیک»)؛ خطا نیست.
- نقشه: هر کتابخانه‌ی دلخواه (نشان/بلد/OSM)؛ مختصات WGS84 و `latitude`/`longitude` است (ترتیب lat,lng).
- بار سرور: هر درخواست دو کوئری سبک است؛ ولی همه‌ی کلاینت‌ها را روی ۱۵ ثانیه نگه دارید و با بسته‌شدن صفحه/پس‌زمینه polling را متوقف کنید.

### مثال تشخیص سریع
- `courier` همیشه `null` است ولی وضعیت ۳/۴ شده ⇒ احتمالاً نام فیلدهای میاره با استخراج نمی‌خواند (بخش ۳). پنل ادمین ← متن خام رویداد را ببینید.
- `distanceKm` نیست ⇒ مختصات فروشگاه یا آدرس مشتری ثبت نشده (`store`/`destination` هم `null` می‌شود).

## ۸) پنل ادمین

کارت در کامپوننت `panel/components/productorder/ShipmentCard.vue` است (`<ProductorderShipmentCard :order-id @changed>`؛ خودش مرسوله‌ها را از `useAdminOrderStore().getOrderShipments` می‌خواند و بعد از تمدید مهلت `changed` می‌دهد تا صفحه‌ی سفارش دوباره بارگذاری شود). چیدمان ریسپانسیو است (ردیف‌ها `flex-wrap`، متن‌های بلند `break-words`، دکمه/select تمدید در موبایل تمام‌عرض) و در عرض ≈۴۴۰px بدون سرریز افقی با نام فروشگاه بلند و متن خام بلند بررسی شد.

**نقشه:** مکان‌ها روی «مپ اختصاصی پاستیل» (`iran.pmtiles` خودمیزبان، همان سبک نقشه‌ی سفرها؛ `utils/pastilMapStyle.ts`) نشان داده می‌شوند، نه لینک گوگل. کامپوننت `components/productorder/ShipmentMap.vue` با دکمه‌ی «نمایش روی نقشه» (فقط با کلیک بارگذاری می‌شود) سه نقطه را نشان می‌دهد: فروشگاه (سبز)، آخرین موقعیت پیک (کامیون آبی؛ بیش از ۱۰ دقیقه قدیمی = حاشیه‌ی کهربایی) و آدرس مشتری (قرمز)، به‌علاوه‌ی مسیرِ موقعیت‌های ثبت‌شده‌ی وب‌هوک (خط‌چین + نقطه‌ها با زمان). «موقعیت روی نقشه» کنار هر رویداد نقشه را روی همان نقطه می‌برد. مختصات فروشگاه و مقصد از `GET api/Admin/OrderShipment` می‌آید (`storeLatitude/Longitude`، `destinationLatitude/Longitude`).

**فونت:** کل صفحه‌ی سفارش و کارت پیک با کلاس `numeric-value` (یکان‌بخ با ارقام فارسی، همان قرارداد بقیه‌ی صفحه‌های پنل) نمایش داده می‌شود؛ `font-mono` از اعداد/تاریخ/تلفن حذف شد. فقط متن خام وب‌هوک و متن خطای میاره (کد/JSON لاتین) مونو ماند.

صفحه‌ی `/admin/productorder/detail-{id}` ← «مرسوله‌ی پیک (میاره)»:
- دکمه‌ی **تازه‌سازی** (وب‌هوک‌ها زنده می‌رسند؛ صفحه خودش polling نمی‌کند).
- «اطلاعات پیک»: نام، تلفن (لینک `tel:`)، آخرین موقعیت (n دقیقه پیش؛ قرمز اگر بیش از ۱۰ دقیقه) با لینک «نمایش روی نقشه»، و آخرین state میاره.
- «مراحل ارسال (وب‌هوک میاره)»: جدول زمانی رویدادها با برچسب فارسی، پیک، لینک موقعیت و باز/بسته‌شدنی **«متن خام وب‌هوک»** (برای عیب‌یابی).

## ۹) تست‌ها

- واحد: `Application.Tests/ShipmentCourierRulesTests.cs` (۴۲ مورد): استخراج از شیء/کلید تخت/`data`؛ عدم خواندن کلیدهای ریشه؛ ترتیب آرایه‌ی مختصات؛ رد ۰،۰؛ پاک‌سازی کد تحویل؛ هش؛ throttle رویدادها؛ ماتریس نمایش؛ مراحل (تحویل/لغو/برگشت/ناموفق)؛ فاصله.
- دستی بعد از دیپلوی: یک سفارش اکسپرس واقعی ← بعد از `pickup`/`dropoff` در پنل «اطلاعات پیک» و رویدادها پر شود؛ `GET EndUser/ShipmentTracking` با توکن مشتری همان سفارش داده بدهد و با توکن مشتری دیگر `[]`؛ `GET Seller/Shipment/Tracking` با فروشگاه دیگر «یافت نشد».

## ۱۰) بیرون از این کار (در صورت نیاز بعداً)
- ETA (زمان رسیدن): میاره نمی‌دهد؛ می‌شود از فاصله و سرعت تقریبی تخمین زد (تصمیم محصولی).
- پوش «پیک تعیین شد» به مشتری/فروشنده (الان فقط پوش‌های قبلی: ارسال/تأخیر/لغو).
- «ثبت خودکار گزارش مشکل به میاره» همچنان دستی است (بخش آخر `MIARE_SHIPPING_LIFECYCLE_FA.md`).

## ۱۱) کدها
`ShipmentCourierRules` (قواعد خالص + تست)، `ShipmentService` (`HandleMiareWebhookAsync`، `RecordMiareEventAsync`، `GetForOrderAsync`)، `ShipmentTrackingService` (مشتری/فروشنده)، `Api/Areas/EndUser/Controllers/ShipmentTrackingController.cs`، `Api/Areas/Seller/Controllers/ShipmentController.cs` (`Tracking`)، موجودیت‌های `Shipment`/`ShipmentEvent`، Migration `AddShipmentCourierTracking`، پنل `pages/admin/productorder/detail-[id].vue`.
