# ردیابی پیک میاره — مستند کامل اپ نماینده (فلاتر) — بخش فروشگاه/فروشنده

> مخاطب: تیم اپ فلاتر — امیرمحسن.
> وضعیت بک‌اند: **ساخته و تست‌شده، دیپلوی نشده.** اول اسکریپت `backend/scripts/AddShipmentCourierTracking.sql` روی دیتابیس، بعد Api. قبل از آن endpoint جدید خطا می‌دهد.
> قرارداد مشترک و طراحی داخلی: [MIARE_COURIER_TRACKING_FA.md](MIARE_COURIER_TRACKING_FA.md). این سند **مستقل** است. سند وب‌اپ جداست: [MIARE_COURIER_TRACKING_WEBAPP_FA.md](MIARE_COURIER_TRACKING_WEBAPP_FA.md).
> مکمل: [MIARE_EXPRESS_SELLER_APP_FA.md](MIARE_EXPRESS_SELLER_APP_FA.md) (درخواست پیک اکسپرس) و [MIARE_SELLER_APP_AWAITING_LIST_FA.md](MIARE_SELLER_APP_AWAITING_LIST_FA.md) (رفع کارت کهنه‌ی «درخواست پیک»).

## ۰) دامنه — کدام کاربر، کدام بخش

- این قابلیت مال **نقش فروشگاه (Seller)** داخل اپ نماینده است: کاربری که فروشگاه دارد و endpointهای `api/Seller/*` را می‌زند. در مستندات پروژه «اپ نماینده» به‌طور کلی اپ مراکز خدماتی (Companion) است؛ **کاربر Companionِ بدون فروشگاه این بخش را ندارد** و کاری برایش لازم نیست.
- ردیابی فقط برای سفارش‌های **ارسال با میاره** (`shippingProvider == 4`) است. سفارش پستی دکمه/صفحه‌ی ردیابی ندارد.
- فروشنده در این صفحه **فقط می‌خواند**؛ هیچ اقدامی (لغو/تغییر) از این صفحه انجام نمی‌شود. «درخواست پیک» همان قبلی است (`POST Seller/Shipment/Ready`).
- راننده‌ی پت‌رسان (`api/Driver/*`) هیچ ارتباطی با این سند ندارد.

## ۱) اصول ثابت API

- پایه: `https://api.pastil.pet/api`. هدرها: `Authorization: Bearer <token>`، `StoreId: <id فروشگاه جاری>` (مثل بقیه‌ی `Seller/*`؛ در Swagger «Your StoreId») و `Accept: application/json`، `Accept-Language: fa`.
- **پاسخ همیشه HTTP 200** با `{ "isSuccess": bool, "code": int, "messages": [{ "item1": "...", "item2": "" }], "data": ... }`. موفقیت را **فقط** از `isSuccess` بخوانید؛ پیام خطا `messages[0].item1` (فارسی، آماده‌ی نمایش).
- JSON **camelCase**. زمان‌ها **UTC** (با `Z`) هستند و باید **به وقت تهران** نمایش داده شوند. `toLocal()` فقط وقتی درست است که منطقه‌ی زمانی دستگاه ایران باشد؛ برای اطمینان از کتابخانه‌ی `timezone` با `Asia/Tehran` استفاده کنید (بخش ۷).
- **enumها عددی‌اند**: `status`: ۲ درخواست‌شده، ۳ پیک‌تعیین‌شد، ۴ در راه، ۵ تحویل، ۶ لغو، ۷ ناموفق، ۸/۹ آماده‌سازی. `shippingProvider`: **۴ = میاره**.
- خطای شبکه/۴۰۱: همان رفتار سایر صفحه‌های اپ (رفرش توکن/ورود دوباره). `HTTP != 200` یعنی مشکل زیرساخت، نه منطق.

## ۲) endpoint جدید

### `GET /api/Seller/Shipment/Tracking?productOrderStoreId={long}`

- `productOrderStoreId` همان شناسه‌ای است که در `GET Seller/Shipment/Awaiting` و بدنه‌ی `POST Seller/Shipment/Ready` استفاده می‌کنید.
- فروشگاه از هدر/توکن می‌آید. سفارش فروشگاه دیگر ⇒ `isSuccess=false` و پیام «یافت نشد» (`ResourceNotFind`). فروشگاه نامعتبر ⇒ «دسترسی ندارید».

پاسخ موفق (مثال: پیک در مسیر فروشگاه، وضعیت ۳):

```json
{
  "isSuccess": true, "code": 0, "messages": [],
  "data": {
    "shipmentId": 123, "productOrderStoreId": 456, "storeId": 5, "storeName": "پی کی پت",
    "provider": "Miare", "status": 3, "statusKey": "Accepted",
    "isExpress": true, "isFinal": false, "isLive": true,
    "currentStepKey": "courier_assigned",
    "headline": "پیک تعیین شد و در مسیر فروشگاه است.",
    "steps": [
      { "key": "preparing", "title": "فروشگاه در حال آماده‌سازی سفارش است.", "done": true, "current": false, "atUtc": "2026-10-25T10:05:00Z" },
      { "key": "picked_up", "title": "پیک سفارش را از فروشگاه تحویل گرفت و در راه است.", "done": false, "current": false, "atUtc": null },
      { "key": "delivered", "title": "سفارش تحویل داده شد.", "done": false, "current": false, "atUtc": null }
    ],
    "courier": {
      "name": "محمدرضا احمدی", "phone": "09123456789", "canCall": true,
      "hasLocation": true, "latitude": 35.7312, "longitude": 51.3947,
      "locationUpdatedAtUtc": "2026-10-25T10:50:08Z", "locationAgeSeconds": 22, "locationStale": false,
      "distanceKm": 1.8, "distanceTarget": "store"
    },
    "store": { "latitude": 35.7045, "longitude": 51.3560 },
    "destination": null,
    "plannedPickupUtc": "2026-10-25T10:50:00Z", "deliveredAtUtc": null,
    "lastUpdateUtc": "2026-10-25T10:50:08Z", "pollSeconds": 15
  }
}
```

(`steps` در پاسخ واقعی همیشه **۵ مرحله** است: `preparing`، `courier_requested`، `courier_assigned`، `picked_up`، `delivered`؛ در لغو/ناموفق مرحله‌ی ششمِ `cancelled` / `failed` / `returning` هم می‌آید. بالا خلاصه شده.)

### فیلدها

| فیلد | معنی | رفتار UI |
|---|---|---|
| `status` / `statusKey` | وضعیت مرسوله | جدول بخش ۳ |
| `isFinal` | تحویل/لغو/ناموفق | **polling را قطع کنید** |
| `isLive` | وضعیت ۳ یا ۴ | نقشه‌ی زنده فقط وقتی |
| `headline` | جمله‌ی فارسی مرحله‌ی جاری از سرور | بالای صفحه نشان دهید؛ نیازی به ترجمه‌ی محلی نیست |
| `steps[]` | مراحل: `key`، `title` (فارسی)، `done`، `current`، `atUtc` | نوار مراحل عمودی؛ `atUtc` فقط برای انجام‌شده/جاری |
| `courier` | **`null` است** وقتی پیک هنوز تعیین نشده **یا** وضعیت ≥ ۴ (برای فروشنده) | UI باید با `null` درست کار کند |
| `courier.name` | **نام کامل** پیک (برای شناسایی هنگام تحویل) | نمایش |
| `courier.phone` / `canCall` | تلفن؛ فقط وضعیت ۳ | دکمه‌ی تماس فقط اگر `canCall` |
| `courier.hasLocation`, `latitude`, `longitude` | آخرین موقعیت پیک | نقشه |
| `courier.locationStale` | موقعیت قدیمی‌تر از ۱۰ دقیقه | «آخرین موقعیت معلوم» + نشانگر کم‌رنگ |
| `courier.locationAgeSeconds` | سن موقعیت | «n ثانیه/دقیقه پیش» (از سرور بخوانید) |
| `courier.distanceKm` | فاصله‌ی **هوایی** پیک تا فروشگاه | «x کیلومتر تا فروشگاه»؛ مسیر/ETA نداریم |
| `store` | مختصات فروشگاه خودتان | نشانگر مبدأ |
| `destination` | **همیشه `null`** برای فروشنده | **هرگز** مقصد/آدرس مشتری نشان ندهید |
| `plannedPickupUtc` | ساعت تقریبی رسیدن پیک | «پیک حدود ساعت …» |
| `pollSeconds` | فاصله‌ی polling | ۱۵ ثانیه (بین ۱۰ و ۶۰ محدود کنید) |

## ۳) هر وضعیت چه نشان می‌دهد

| `status` | `headline` (سرور) | `courier` | UI |
|---|---|---|---|
| ۸ / ۹ | فروشگاه در حال آماده‌سازی… | `null` | فقط مراحل. دکمه‌ی «درخواست پیک» همان صفحه‌ی لیست «در انتظار» است (نه این صفحه) |
| ۲ | پیک درخواست شد و در انتظار تخصیص است. | `null` | مراحل + متن «در انتظار تخصیص پیک»؛ `plannedPickupUtc` را «پیک حدود ساعت …» بنویسید |
| ۳ | پیک تعیین شد و در مسیر فروشگاه است. | پر (نام کامل، تلفن، موقعیت، فاصله) | کارت پیک + دکمه‌ی **تماس** + نقشه‌ی دو نقطه‌ای (فروشگاه + پیک) + چیپ فاصله |
| ۴ | پیک سفارش را از فروشگاه تحویل گرفت و در راه است. | **`null`** | مراحل + متن «پیک سفارش را تحویل گرفت»؛ بدون کارت/نقشه/تلفن |
| ۵ | سفارش تحویل داده شد. | `null` | مراحل (همه سبز) |
| ۶ | ارسال لغو شد. | `null` | مراحل + مرحله‌ی قرمز |
| ۷ | ارسال با مشکل روبه‌رو شد / کالا در حال بازگشت… | `null` | مراحل + مرحله‌ی قرمز + «با پشتیبانی پاستیل تماس بگیرید» |

## ۴) مدل‌ها (Dart)

مستقل از state-management؛ همان کلاس‌ها را در Riverpod/Bloc/GetX استفاده کنید. همه‌ی فیلدها null-safe و محافظه‌کار parse می‌شوند (بک‌اند فیلد جدید اضافه کند اپ نشکند).

```dart
class ApiEnvelope<T> {
  final bool isSuccess;
  final String? message; // messages[0].item1
  final T? data;
  ApiEnvelope(this.isSuccess, this.message, this.data);
}

DateTime? _utc(dynamic v) => v == null ? null : DateTime.tryParse(v as String)?.toUtc();
double? _d(dynamic v) => v is num ? v.toDouble() : (v is String ? double.tryParse(v) : null);

class TrackPoint {
  final double latitude, longitude;
  TrackPoint(this.latitude, this.longitude);
  static TrackPoint? from(dynamic j) {
    if (j is! Map) return null;
    final la = _d(j['latitude']), lo = _d(j['longitude']);
    return (la == null || lo == null) ? null : TrackPoint(la, lo);
  }
}

class TrackStep {
  final String key, title;
  final bool done, current;
  final DateTime? atUtc;
  TrackStep.fromJson(Map<String, dynamic> j)
      : key = j['key'] as String? ?? '',
        title = j['title'] as String? ?? '',
        done = j['done'] == true,
        current = j['current'] == true,
        atUtc = _utc(j['atUtc']);
}

class TrackCourier {
  final String? name, phone;
  final bool canCall, hasLocation, locationStale;
  final double? latitude, longitude, distanceKm;
  final int? locationAgeSeconds;
  final String? distanceTarget; // "store" برای فروشنده
  TrackCourier.fromJson(Map<String, dynamic> j)
      : name = j['name'] as String?,
        phone = j['phone'] as String?,
        canCall = j['canCall'] == true,
        hasLocation = j['hasLocation'] == true,
        locationStale = j['locationStale'] == true,
        latitude = _d(j['latitude']),
        longitude = _d(j['longitude']),
        distanceKm = _d(j['distanceKm']),
        locationAgeSeconds = (j['locationAgeSeconds'] as num?)?.toInt(),
        distanceTarget = j['distanceTarget'] as String?;
}

class ShipmentTracking {
  final int shipmentId, productOrderStoreId, status, pollSeconds;
  final String storeName, statusKey, headline, currentStepKey;
  final bool isExpress, isFinal, isLive;
  final List<TrackStep> steps;
  final TrackCourier? courier;
  final TrackPoint? store;
  final DateTime? plannedPickupUtc, lastUpdateUtc;

  ShipmentTracking.fromJson(Map<String, dynamic> j)
      : shipmentId = (j['shipmentId'] as num?)?.toInt() ?? 0,
        productOrderStoreId = (j['productOrderStoreId'] as num?)?.toInt() ?? 0,
        status = (j['status'] as num?)?.toInt() ?? 0,
        pollSeconds = ((j['pollSeconds'] as num?)?.toInt() ?? 15).clamp(10, 60).toInt(),
        storeName = j['storeName'] as String? ?? '',
        statusKey = j['statusKey'] as String? ?? '',
        headline = j['headline'] as String? ?? '',
        currentStepKey = j['currentStepKey'] as String? ?? '',
        isExpress = j['isExpress'] == true,
        isFinal = j['isFinal'] == true,
        isLive = j['isLive'] == true,
        steps = ((j['steps'] as List?) ?? const [])
            .whereType<Map<String, dynamic>>()
            .map(TrackStep.fromJson)
            .toList(),
        courier = j['courier'] is Map<String, dynamic> ? TrackCourier.fromJson(j['courier']) : null,
        store = TrackPoint.from(j['store']),
        plannedPickupUtc = _utc(j['plannedPickupUtc']),
        lastUpdateUtc = _utc(j['lastUpdateUtc']);
}
```

## ۵) لایه‌ی API (مثال با Dio؛ با `http` هم همین است)

```dart
Future<ApiEnvelope<ShipmentTracking>> getShipmentTracking(int productOrderStoreId) async {
  final res = await dio.get(
    '/Seller/Shipment/Tracking',
    queryParameters: {'productOrderStoreId': productOrderStoreId},
    options: Options(headers: {'StoreId': currentStoreId}),
  );
  final body = res.data as Map<String, dynamic>;
  final ok = body['isSuccess'] == true;
  final msgs = body['messages'] as List?;
  final msg = (msgs != null && msgs.isNotEmpty) ? (msgs.first as Map)['item1'] as String? : null;
  return ApiEnvelope(ok, msg, ok && body['data'] is Map<String, dynamic> ? ShipmentTracking.fromJson(body['data']) : null);
}
```

## ۶) polling و چرخه‌ی عمر (الزامی)

قواعد: ۱) هر `pollSeconds` ثانیه؛ ۲) فقط وقتی صفحه روی صفحه است **و** اپ foreground است؛ ۳) با `isFinal=true` یا بستن صفحه متوقف؛ ۴) هیچ‌وقت دو درخواست هم‌زمان؛ ۵) بعد از ۳ خطای پیاپی نمایش «اتصال برقرار نیست؛ آخرین وضعیت: n دقیقه پیش» و ادامه‌ی تلاش با فاصله‌ی دو برابر (حداکثر ۶۰ ثانیه)؛ ۶) با برگشت اپ به foreground **فوراً** یک بار تازه شود.

```dart
class CourierTrackingController extends ChangeNotifier with WidgetsBindingObserver {
  CourierTrackingController(this._api, this.productOrderStoreId);
  final Future<ApiEnvelope<ShipmentTracking>> Function(int) _api;
  final int productOrderStoreId;

  ShipmentTracking? data;
  String? error;            // پیام سرور (isSuccess=false)
  bool loading = true;      // فقط بار اول
  int _failures = 0;
  bool _inFlight = false, _disposed = false;
  Timer? _timer;

  void start() {
    WidgetsBinding.instance.addObserver(this);
    _tick();
  }

  Future<void> _tick() async {
    if (_inFlight || _disposed) return;
    _inFlight = true;
    try {
      final r = await _api(productOrderStoreId);
      if (r.isSuccess && r.data != null) {
        data = r.data; error = null; _failures = 0;
      } else {
        error = r.message ?? 'ردیابی در دسترس نیست'; // مثلاً «یافت نشد»
        _failures++;
      }
    } catch (_) {
      _failures++;      // قطع اینترنت: داده‌ی قبلی بماند
    } finally {
      _inFlight = false; loading = false;
      if (!_disposed) notifyListeners();
      _schedule();
    }
  }

  void _schedule() {
    _timer?.cancel();
    final d = data;
    if (_disposed || (d != null && d.isFinal) || (d == null && error != null)) return; // نهایی یا «یافت نشد»: دیگر نه
    final base = (d?.pollSeconds ?? 15);
    final secs = _failures >= 3 ? (base * 2).clamp(10, 60).toInt() : base;
    _timer = Timer(Duration(seconds: secs), _tick);
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState s) {
    if (s == AppLifecycleState.resumed) { _timer?.cancel(); _tick(); }
    if (s == AppLifecycleState.paused) { _timer?.cancel(); }
  }

  @override
  void dispose() {
    _disposed = true;
    _timer?.cancel();
    WidgetsBinding.instance.removeObserver(this);
    data = null; // اطلاعات پیک در حافظه نماند
    super.dispose();
  }
}
```

## ۷) صفحه‌ی ردیابی (UI)

### نقطه‌های ورود
1. **کارت سفارش** در لیست سفارش‌های فروشگاه: دکمه‌ی «ردیابی پیک» وقتی `order.isPaid` و `shippingProvider == 4`. `productOrderStoreId` = `order.productOrderStores[0].id` (لیست سفارش‌های فروشنده معمولاً فقط فروشگاه خودش را دارد؛ اگر چند ردیف بود، ردیفی که `storeId` برابر فروشگاه جاری است).
   - ⚠️ لیست سفارش‌های فروشنده مرسوله را لود نمی‌کند؛ `shipmentStatus` در آن خالی است. وضعیت را از همین API ردیابی بخوانید و دکمه را فقط با `shippingProvider` نشان دهید.
2. **بعد از موفقیت «درخواست پیک»** (`POST Seller/Shipment/Ready` با `isSuccess=true`): لیست «در انتظار» را از سرور دوباره بخوانید (قاعده‌ی سند AWAITING) **و** کاربر را مستقیم به صفحه‌ی ردیابی همان `productOrderStoreId` ببرید (اختیاری، تجربه‌ی بهتر).
3. **پوش‌های ارسال** (`PushShipmentSellerReminder`، `PushShipmentCourierCanceled`، `PushShipmentReturning`): همان رفتار قبلی؛ فقط می‌توانید در صفحه‌ای که باز می‌کنند دکمه‌ی ردیابی هم بگذارید. **پوش تازه‌ای برای این قابلیت نداریم.**

### چیدمان (RTL، از بالا به پایین)

```
┌──────────────────────────────────┐
│ ‹  ردیابی پیک              ⚡اکسپرس│  ← AppBar؛ شماره‌ی سفارش زیر عنوان
├──────────────────────────────────┤
│ [ headline — رنگ بر اساس وضعیت ] │  ← سبز=۵، قرمز=۶/۷، آبی=بقیه
│                                  │
│  ● مراحل (عمودی)                 │
│  ✓ فروشگاه در حال آماده‌سازی…  ۱۰:۰۵ │
│  ✓ پیک درخواست شد…             ۱۰:۲۰ │
│  ◉ پیک تعیین شد…               ۱۰:۲۶ │  ← جاری
│  ○ پیک سفارش را تحویل گرفت…      │
│  ○ سفارش تحویل داده شد.          │
├──────────────────────────────────┤
│ کارت پیک (فقط اگر courier != null)│
│  [آواتار] محمدرضا احمدی  [📞 تماس] │
│  ۱٫۸ کیلومتر تا فروشگاه · ۲۲ ثانیه پیش │
│  ┌──────── نقشه ────────┐        │
│  │  🏪 فروشگاه   🛵 پیک │        │
│  └──────────────────────┘        │
└──────────────────────────────────┘
```

قواعد ظاهر:
- مراحل را مستقیم از `steps` رندر کنید (ترتیب سرور). `done` = تیک سبز، `current` = حلقه‌ی آبی، بقیه خاکستری، `cancelled/failed/returning` = قرمز. `atUtc` را به **وقت تهران** و ارقام فارسی (`HH:mm`) نشان دهید.
- `headline` را همان‌طور که هست نشان دهید (فارسی و آماده).
- `locationStale = true` ⇒ نشانگر پیک کم‌رنگ و متن «آخرین موقعیت معلوم: n دقیقه پیش» (نارنجی).
- بعد از `isFinal` یک خط خاکستری «ردیابی پایان یافت» بگذارید و کارت پیک/نقشه را حذف کنید.
- شمارنده/متن‌های نسبی («n ثانیه پیش») را از `locationAgeSeconds` سرور بسازید، نه از ساعت دستگاه.
- ارقام فارسی، تاریخ/ساعت تهران، RTL؛ دکمه‌ها حداقل ۴۴dp.

### دکمه‌ی تماس
```dart
Future<void> callCourier(String phone) =>
    launchUrl(Uri(scheme: 'tel', path: phone)); // package:url_launcher
```
فقط وقتی `courier.canCall && courier.phone != null`. شماره را در UI به‌صورت متن هم نشان دهید (چپ‌به‌راست).

### نقشه
- **از همان کتابخانه‌ی نقشه‌ای که اپ دارد استفاده کنید** (نقشه‌ی سفرهای پت‌رسان/آدرس‌ها). دو نشانگر: فروشگاه (`store`) و پیک (`courier.latitude/longitude`). `destination` نیست.
- اگر اپ نقشه‌ی اختصاصی پاستیل (پروتکل pmtiles + استایل) ندارد، مرجع پیاده‌سازی: `panel/utils/pastilMapStyle.ts` و `webapp/app/utils/pastilMapStyle.ts` (داده‌ی وکتور `https://app.pastil.pet/tiles/iran.pmtiles`؛ فونت و اسپرایت Protomaps). اگر کتابخانه‌ی اپ pmtiles را پشتیبانی نمی‌کند، با تیم هماهنگ کنید؛ تا آن موقع نقشه‌ی فعلی اپ قابل‌قبول است و فقط نشانگرها مهم‌اند.
- فقط بار اول کادر را روی دو نقطه fit کنید؛ با polling بعدی فقط **نشانگر پیک را جابه‌جا کنید** (انیمیشن نرم)، نه دوباره fit (کاربر ممکن است نقشه را حرکت داده باشد).
- نشانگر پیک آیکن موتور/ماشین؛ فروشگاه آیکن فروشگاه. ارتفاع نقشه ≈ ۲۴۰dp. وقتی `courier == null` نقشه را نشان ندهید.

## ۸) حریم خصوصی (قواعد سخت)

- نام/تلفن/مختصات پیک را **ذخیره نکنید**: نه `SharedPreferences`، نه SQLite/Hive، نه کش دیسک، نه لاگ (`print`/Crashlytics/Sentry)، نه آنالیتیکس. در `dispose` کنترلر پاک شود (کد بالا).
- برای ردیابی هیچ‌وقت از endpointهای مشتری استفاده نکنید؛ فروشنده فقط `Seller/Shipment/Tracking`.
- مختصات یا آدرس مشتری را از هیچ‌جای دیگر هم روی نقشه‌ی این صفحه نیاورید.
- اسکرین‌شات‌های QA/لاگ‌ها را بدون شماره‌ی واقعی پیک منتشر کنید.
- سرور بعد از وضعیت ۴ اطلاعات پیک را برنمی‌گرداند؛ اگر دیدید (باگ سرور) نشان ندهید.

## ۹) خطاها

| وضعیت | علت | رفتار اپ |
|---|---|---|
| `isSuccess=false`، پیام «یافت نشد» | سفارش مال این فروشگاه نیست یا مرسوله‌ی میاره ندارد | صفحه‌ی خطای تمیز با پیام سرور + دکمه‌ی «بازگشت»؛ polling نکنید |
| `isSuccess=false`، «دسترسی ندارید» | فروشگاه جاری نامعتبر | همان؛ به انتخاب فروشگاه برگردید |
| HTTP ≠ ۲۰۰ / timeout | شبکه | آخرین داده بماند، نوار «اتصال برقرار نیست»؛ تلاش بعدی طبق بخش ۶ |
| ۴۰۱ | توکن | رفتار استاندارد اپ (refresh/ورود) |
| `courier == null` در وضعیت ۲/۳ | هنوز تعیین نشده / اطلاعات نرسیده | خطا نیست؛ فقط مراحل |

## ۱۰) داده‌ی آزمایشی

تا میاره‌ی Production وصل نیست داده‌ی واقعی نمی‌آید. برای ساخت UI یک `MockTrackingApi` بسازید که برای هر وضعیت یک پاسخ ثابت بدهد (فیلدهای بخش ۲):

| سناریو | `status` | `courier` | `isFinal` |
|---|---|---|---|
| آماده‌سازی | ۹ | `null` | false |
| درخواست‌شده | ۲ | `null` | false |
| پیک در مسیر فروشگاه | ۳ | پر، `hasLocation=true`، `canCall=true`، `distanceKm=1.8` | false |
| پیک با موقعیت قدیمی | ۳ | پر، `locationStale=true`، `locationAgeSeconds=900` | false |
| پیک بدون موقعیت | ۳ | نام/تلفن دارد، `hasLocation=false` | false |
| کالا دست پیک | ۴ | `null` | false |
| تحویل | ۵ | `null` | true |
| لغو | ۶ | `null` | true (+ مرحله‌ی `cancelled`) |
| ناموفق/برگشت | ۷ | `null` | true (+ مرحله‌ی `failed` یا `returning`) |

یونیت‌تست‌های پیشنهادی: parse هر سناریو؛ `fromJson` با فیلد ناشناخته‌ی اضافه؛ `courier: null`؛ `steps` خالی؛ کنترلر: توقف بعد از `isFinal`، توقف با «یافت نشد»، backoff بعد از ۳ خطا، توقف در `paused` و ادامه در `resumed`، پاک‌شدن داده در `dispose`.

## ۱۱) چک‌لیست تست دستی (بعد از دیپلوی و اتصال میاره‌ی Production)

1. سفارش اکسپرس واقعی ← «درخواست پیک» ← صفحه‌ی ردیابی: مراحل «درخواست شد».
2. پیک تعیین شد (وضعیت ۳) ← ظرف ≤ ۱۵ ثانیه کارت پیک با نام کامل، تلفن، فاصله، نقشه‌ی دو نقطه‌ای؛ دکمه‌ی تماس شماره‌گیر را باز کند.
3. نشانگر پیک با polling حرکت کند و نقشه‌ی کاربر را به‌هم نریزد.
4. پیک کالا را برداشت (۴) ← کارت پیک و نقشه ناپدید، متن «پیک سفارش را تحویل گرفت»؛ شماره/موقعیت هیچ‌جا نماند.
5. تحویل (۵) ← مراحل سبز؛ polling متوقف (در لاگ شبکه درخواست جدید نباشد).
6. اپ را به پس‌زمینه ببرید ← درخواستی نرود؛ برگردید ← فوراً یک درخواست.
7. اینترنت را قطع کنید ← آخرین داده و نوار قطع؛ وصل که شد خودش جبران کند.
8. سفارش پستی ← دکمه‌ی ردیابی نباشد.
9. سفارش فروشگاه دیگر (دستکاری شناسه) ← «یافت نشد» و بدون polling.
10. در هیچ پاسخی `destination`/آدرس مشتری روی نقشه نباشد.
11. بستن صفحه ← حافظه‌ی پیک پاک (بدون persist؛ در لاگ دستگاه شماره‌ی پیک نباشد).
12. چک اضافه (از سند AWAITING): بعد از «درخواست پیک» موفق، کارت «در انتظار» برنگردد؛ متن‌های ثابت «۱۵ دقیقه» حذف و `plannedPickupUtc` نشان داده شود (میاره حداقل ۳۰ دقیقه می‌خواهد).

## ۱۲) سؤال‌های رایج

- **چرا پس از اینکه پیک کالا را برداشت اطلاعاتش را نمی‌بینم؟** حریم خصوصی: از آن لحظه پیک فقط با مشتری کار دارد. مشتری تلفن و موقعیتش را می‌بیند، نه فروشنده.
- **ETA/زمان رسیدن؟** میاره نمی‌دهد؛ فقط فاصله‌ی هوایی داریم.
- **تاریخچه‌ی مراحل قدیمی‌تر؟** فقط همان `steps` فعلی؛ جزئیات فنی وب‌هوک را ادمین در پنل می‌بیند.
- **پوش «پیک تعیین شد» هست؟** نه (فعلاً). صفحه را باز کنید یا از لیست تازه کنید.
- **اگر نام/موقعیت همیشه خالی است؟** احتمالاً نام فیلدهای وب‌هوک میاره با استخراج سرور نمی‌خواند. اپ کاری نمی‌تواند بکند؛ به تیم بک بگویید تا متن خام وب‌هوک را در پنل ادمین (صفحه‌ی سفارش ← «مراحل ارسال») بررسی کند.
