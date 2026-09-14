# راهنمای اتصال Push برای اپ فلاتر (اندروید / iOS / ویندوز)

این سند برای تیم فلاتر است. بک‌اند از سه مسیر پوش پشتیبانی می‌کند که هم‌زمان و مستقل از هم کار می‌کنند:

| پلتفرم | مکانیزم | چرا |
|---|---|---|
| وب‌اپ (مرورگر/PWA) | Web Push (بدون تغییر) | همون چیزی که از قبل بود، ربطی به اپ فلاتر نداره |
| اندروید / iOS (اپ فلاتر) | Firebase Cloud Messaging (FCM) | پوش واقعی سیستم‌عامل، حتی وقتی اپ کاملاً بسته است |
| ویندوز (اپ فلاتر) | اتصال زنده‌ی SignalR | FCM از ویندوز دسکتاپ پشتیبانی رسمی نمی‌کند؛ چون اپ دسکتاپ معمولاً باز می‌ماند، این عملاً معادل Push زنده است — فقط وقتی اپ واقعاً در حال اجراست پیام می‌رسد |

بک‌اند سمت سرور کامل پیاده و دیپلوی شده (شامل کلید Firebase). از الان می‌تونید توسعه رو شروع کنید.

Base URL: `https://api.pastil.pet`

---

## ۱. اندروید و iOS — ثبت FCM Token

### ۱.۱ راه‌اندازی Firebase در پروژه‌ی فلاتر

```yaml
# pubspec.yaml
dependencies:
  firebase_core: ^3.x
  firebase_messaging: ^15.x
```

پروژه رو با `flutterfire configure` به پروژه‌ی Firebase (`pastil-a870d`) وصل کنید (فایل‌های `google-services.json` / `GoogleService-Info.plist` رو همون CLI براتون می‌سازه).

```dart
// main.dart
import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';

@pragma('vm:entry-point')
Future<void> _firebaseBackgroundHandler(RemoteMessage message) async {
  // اپ کاملاً بسته یا در Background است — این‌جا فقط داده پردازش می‌شود،
  // نوتیفیکیشن خودتان را با flutter_local_notifications بسازید (بخش ۱.۳)
}

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await Firebase.initializeApp();
  FirebaseMessaging.onBackgroundMessage(_firebaseBackgroundHandler);
  runApp(const MyApp());
}
```

⚠️ ثبت `onBackgroundMessage` الزامی است — بدون آن، وقتی اپ کاملاً بسته است (نه فقط Background) پیام روی اندروید/iOS اصلاً نمایش داده نمی‌شود.

### ۱.۲ گرفتن Token و ثبت آن در بک‌اند

```dart
Future<void> registerFcmToken() async {
  final messaging = FirebaseMessaging.instance;
  await messaging.requestPermission(); // روی iOS الزامی، روی اندروید 13+ هم لازم است

  final token = await messaging.getToken();
  if (token != null) {
    await _subscribeFcm(token);
  }

  // هر بار توکن رفرش شد (ممکن است در طول عمر اپ چند بار رخ دهد)
  messaging.onTokenRefresh.listen(_subscribeFcm);
}

Future<void> _subscribeFcm(String fcmToken) async {
  final deviceKey = await getOrCreateDeviceKey(); // بخش ۳
  await http.post(
    Uri.parse('https://api.pastil.pet/api/EndUser/push/subscribe-fcm'),
    headers: {
      'Content-Type': 'application/json',
      if (authToken != null) 'Authorization': 'Bearer $authToken', // اختیاری، اگر کاربر لاگین است
    },
    body: jsonEncode({
      'deviceKey': deviceKey,
      'fcmToken': fcmToken,
      'platform': Platform.isIOS ? 'ios' : 'android',
      'userAgent': 'Pastil Flutter App',
    }),
  );
}
```

**Request** — `POST /api/EndUser/push/subscribe-fcm` (بدون نیاز به Authorization؛ اگر بفرستید و معتبر باشد، subscription مستقیم به همان کاربر وصل می‌شود):

```json
{
  "deviceKey": "11111111-1111-1111-1111-111111111111",
  "fcmToken": "توکن گرفته‌شده از Firebase",
  "platform": "android",
  "userAgent": "Pastil Flutter App 1.0.0"
}
```

**Response** (فرمت استاندارد همه‌ی endpoint های بک‌اند — همیشه HTTP 200، موفقیت را از `isSuccess` بخوانید، نه از status code):
```json
{ "isSuccess": true, "code": 0, "messages": [], "data": null }
```

- `platform`: دقیقاً `"android"` یا `"ios"`.
- `deviceKey`: بخش ۳ را ببینید.
- هر بار Firebase توکن را Refresh کرد (`onTokenRefresh`)، همین Endpoint را دوباره با توکن جدید صدا بزنید — رکورد قبلی بر اساس `fcmToken` قدیمی به‌روزرسانی می‌شود (upsert).

### ۱.۳ ساخت نوتیفیکیشن از روی داده‌ی دریافتی

چون بک‌اند فقط `data` می‌فرستد (نه `notification` payload مستقیم Firebase — چون Firebase وقتی `notification` payload دارد خودش روی iOS/بعضی حالت‌های اندروید مستقیم UI می‌سازد و کنترل کمتری به شما می‌دهد)، اپ باید خودش نوتیفیکیشن را بسازد — هم در Foreground هم Background/Terminated. برای این کار از `flutter_local_notifications` استفاده کنید:

```dart
FirebaseMessaging.onMessage.listen((RemoteMessage message) {
  _showLocalNotification(message.data);
});

// _firebaseBackgroundHandler هم همین data رو می‌گیره (بخش ۱.۱)
```

شکل `data` که همیشه می‌رسد:
```json
{
  "title": "متن عنوان",
  "body": "متن بدنه",
  "url": "/reserve/123",
  "icon": "/icon-192x192.png",
  "tag": "notice-123"
}
```

- `url`: مسیر داخلی اپ که با زدن روی نوتیفیکیشن باید به آن Navigate شود (با `onDidReceiveNotificationResponse` هندل کنید).
- `tag`: شناسه‌ی یکتای منطقی برای دی‌دوپ/بازنویسی نوتیفیکیشن‌های مشابه — می‌توانید از آن به‌عنوان `id` نوتیفیکیشن محلی (هش‌شده به int) استفاده کنید.
- `icon`: مسیر نسبی به دامنه‌ی `app.pastil.pet` — روی موبایل معمولاً آیکون خودِ اپ را نشون بدید و این فیلد را نادیده بگیرید (بیشتر برای وب‌اپ کاربرد دارد).

---

## ۲. ویندوز — اتصال SignalR

هیچ Endpoint ثبتی لازم نیست. اپ فلاتر ویندوز (بعد از لاگین) یک اتصال SignalR باز نگه می‌دارد:

```
wss://api.pastil.pet/hubs/push
```

### پکیج پیشنهادی
[`signalr_netcore`](https://pub.dev/packages/signalr_netcore) — همان چیزی که احتمالاً برای هاب تماس (`/hubs/call`) از قبل استفاده کردید، همان الگو را اینجا هم به کار ببرید.

```dart
import 'package:signalr_netcore/signalr_client.dart';

Future<HubConnection> connectPushHub(String Function() getAuthToken) async {
  final connection = HubConnectionBuilder()
      .withUrl(
        'https://api.pastil.pet/hubs/push',
        options: HttpConnectionOptions(
          accessTokenFactory: () async => getAuthToken(),
          transport: HttpTransportType.WebSockets,
        ),
      )
      .withAutomaticReconnect()
      .build();

  connection.on('push', (arguments) {
    final data = arguments?[0] as Map<String, dynamic>?;
    if (data == null) return;
    _showWindowsNotification(
      title: data['title'] as String?,
      body: data['body'] as String?,
      url: data['url'] as String?,
    );
  });

  await connection.start();
  return connection;
}
```

- **مهم**: توکن را حتماً از طریق `accessTokenFactory` بدهید، نه هدر دستی — کتابخانه‌های SignalR (در همه‌ی زبان‌ها) خودشان توکن را روی WebSocket به شکل استاندارد (`?access_token=...`) پاس می‌دهند و بک‌اند دقیقاً همین را برای `/hubs/push` می‌پذیرد (مثل `/hubs/call`).
- برای نمایش نوتیفیکیشن سیستم‌عامل ویندوز از `local_notifier` یا `flutter_local_notifications` (که از ویندوز هم پشتیبانی می‌کند) استفاده کنید.
- اتصال را بعد از لاگین برقرار و در `dispose`/logout با `connection.stop()` قطع کنید.

⚠️ این فقط وقتی کار می‌کند که اپ باز و اتصال برقرار باشد — اگر اپ بسته باشد، پیام گم می‌شود (Retry/Queue ندارد). اگر ویندوز واقعاً به Push حین بسته‌بودن اپ نیاز پیدا کرد، آن‌موقع باید سراغ WNS برویم؛ فعلاً طبق تصمیم مشترک، از همین مسیر ساده‌تر استفاده می‌کنیم.

---

## ۳. منطق `DeviceKey` (قبل از لاگین → بعد از لاگین)

این دقیقاً همون الگویی‌ست که وب‌اپ هم استفاده می‌کند — بدونش پوش‌های شخصی‌سازی‌شده (مثل «سفارشت تایید شد») هیچ‌وقت به کاربر درست وصل نمی‌شن:

```dart
Future<String> getOrCreateDeviceKey() async {
  final prefs = await SharedPreferences.getInstance();
  var key = prefs.getString('push_device_key');
  if (key == null) {
    key = const Uuid().v4();
    await prefs.setString('push_device_key', key);
  }
  return key;
}
```

1. همون اول (حتی قبل از لاگین کاربر)، یک UUID بسازید و محلی ذخیره کنید (بالا).
2. `subscribe-fcm` را همیشه با همین `deviceKey` صدا بزنید (چه لاگین باشید چه نباشید) — این برای اندروید/iOS است. **ویندوز نیازی به این مرحله ندارد** (SignalR مستقیماً از JWT کاربر لاگین‌شده گروه‌بندی می‌شود، بدون `DeviceKey`).
3. وقتی کاربر واقعاً لاگین کرد (روی اندروید/iOS)، یک‌بار این را هم صدا بزنید تا Subscription قبلی به حسابش وصل شود:

```http
POST /api/EndUser/push/attach
Authorization: Bearer <token>
Content-Type: application/json
```
```json
{ "deviceKey": "11111111-1111-1111-1111-111111111111" }
```

بدون این مرحله، پوش‌هایی که قبل از لاگین (به‌صورت anonymous) ثبت شدن هیچ‌وقت به کاربر مشخصی وصل نمی‌شن.

---

## ۴. چک‌لیست تست

- [ ] اندروید: نوتیفیکیشن وقتی اپ در Foreground است می‌رسد.
- [ ] اندروید: نوتیفیکیشن وقتی اپ Background است می‌رسد.
- [ ] اندروید: نوتیفیکیشن وقتی اپ کاملاً Kill شده می‌رسد (تست واقعی سخت‌افزار لازم است، Simulator کافی نیست).
- [ ] iOS: همان سه حالت بالا (روی iOS واقعی تست کنید، Simulator پوش FCM را کامل پشتیبانی نمی‌کند).
- [ ] iOS/اندروید: بعد از لاگین، `attach` صدا زده می‌شود و پوش‌های بعدی به همان کاربر می‌رسند.
- [ ] ویندوز: بعد از لاگین، اتصال SignalR برقرار می‌شود و رویداد `push` روی یک اکشن آزمایشی از بک‌اند دریافت می‌شود.
- [ ] ویندوز: بعد از قطع و وصل شدن اینترنت، `withAutomaticReconnect` واقعاً دوباره وصل می‌شود.
- [ ] زدن روی نوتیفیکیشن (هر سه پلتفرم) کاربر را به مسیر `url` داخل اپ می‌برد.

---

## سمت بک‌اند (فقط جهت اطلاع — کاری از شما نمی‌خواهد)

- `PushSubscription` حالا سه‌حالته: `Provider` = WebPush(۱) یا Fcm(۲) — بقیه‌ی منطق (چه وقت، چه پیامی، چند بار Retry) دقیقاً همون Pattern فعلی پوش‌های سایت است، فقط لایه‌ی ارسال گسترش پیدا کرده.
- کلید Firebase (`PASTIL_FCM_SERVICE_ACCOUNT_JSON`) روی سرور تنظیم و دیپلوی شده — یعنی از همین الان ارسال واقعی FCM فعال است، نیازی به هماهنگی اضافه با بک‌اند برای شروع تست نیست.
