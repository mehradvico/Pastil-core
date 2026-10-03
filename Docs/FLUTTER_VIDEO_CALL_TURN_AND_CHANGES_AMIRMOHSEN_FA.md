# ویدیو کال iPhone، سرور TURN و فهرست تغییرهای لازم در اپ Flutter — ویژه امیرمحسن

> مخاطب: امیرمحسن (اپ Flutter) · تاریخ: ۱۴۰۵/۰۷/۱۱ (۲۰۲۶-۱۰-۰۳)
> این سند دو چیز را می‌گوید: (۱) چه چیزی در بک و سرور عوض شده و اپ باید چه کاری بکند، (۲) باگ «ویدیو کال iPhone به iPhone تصویر طرف مقابل را نشان نمی‌دهد» را چطور تشخیص بدهیم. در هر بند مشخص شده **اپ باید چیزی تعریف/عوض کند یا فقط اطلاع است**.
> قرارداد سیگنالینگ تماس (هاب، متدها، رویدادها) تغییری نکرده: [ONLINE_CONSULTATION_APP_FA.md](ONLINE_CONSULTATION_APP_FA.md) §۷ و [NOBES_ONLINE_CONSULTATION_CHAT_CALL_VIDEO_FA.md](NOBES_ONLINE_CONSULTATION_CHAT_CALL_VIDEO_FA.md).

## ۰) خلاصه‌ی کارهای اپ

| # | موضوع | اپ باید | وضعیت سمت سرور |
| ---: | --- | --- | --- |
| ۱ | سرور TURN | `iceServers` را در همه‌ی `RTCPeerConnection`ها اضافه کند (بند ۱) | **راه‌اندازی شد** (`46.34.163.222:3478`) |
| ۲ | تصویر طرف مقابل در iPhone | چک‌لیست و لاگ بند ۳ تا ۵ را اجرا کند؛ در صورت لزوم کد دریافت track/رندر را اصلاح کند | علت هنوز مشخص نیست؛ به لاگ‌های شما نیاز داریم |
| ۳ | کانال DataChannel وضعیت میکروفون/دوربین | اختیاری (بند ۶) | وب‌اپ استفاده می‌کند |
| ۴ | مشاوره‌ی قابل‌رزرو (انتخاب ساعت) | صفحه‌های جدید طبق مستند جدا | بک ساخته شد، **مایگریشن اجرا نشده** |
| ۵ | حذف نرم پانسیون/مدرسه توسط نماینده | دکمه‌ی حذف طبق مستند جدا | بک ساخته شد، **SQL اجرا نشده** |
| ۶ | ویرایش خدمت (`PUT /api/Companion/CompanionAssistance`) | تغییری لازم نیست؛ بعد از دیپلوی دوباره تست کند | باگ سمت سرور رفع شد، **دیپلوی نشده** |

## ۱) TURN — چه چیزی راه افتاد و اپ چه کند

### چرا لازم بود
تا امروز در وب‌اپ و (احتمالاً) اپ فقط STUN گوگل بود. دو گوشی روی دیتای موبایل دو اپراتور مختلف (NAT/CGNAT) معمولاً بدون TURN مسیر رسانه نمی‌سازند. سرور TURN راه‌اندازی شد و وب‌اپ از آن استفاده می‌کند.

### مشخصات سرور (برای `iceServers`)
| مورد | مقدار |
| --- | --- |
| آدرس | `46.34.163.222` |
| پورت | `3478` (UDP و TCP) |
| بازه‌ی relay | UDP ‏`49160`–`49400` (سمت سرور باز است؛ اپ لازم نیست چیزی بداند) |
| نام کاربری | `pastil` |
| رمز | **از مهراد بگیرید (در چت/گیت نگذارید)** |
| نوع احراز هویت | long-term credential (نام کاربری و رمز ثابت) |

سقف سمت سرور (`max-bps` و `total-quota` در coturn): حدود ۱٫۵ مگابایت بر ثانیه برای هر session (واحد coturn بایت بر ثانیه است، یعنی تقریباً ۱۲ مگابیت) و حداکثر ۱۰۰ allocation هم‌زمان؛ برای ویدیو ۷۲۰p کاملاً کافی است (وب‌اپ سقف bitrate ۱٫۲ مگابیت بر ثانیه دارد).

### کد پیشنهادی (Flutter / `flutter_webrtc`)

```dart
final Map<String, dynamic> rtcConfig = {
  'sdpSemantics': 'unified-plan',
  'iceServers': [
    {'urls': 'stun:46.34.163.222:3478'},
    {
      'urls': [
        'turn:46.34.163.222:3478?transport=udp',
        'turn:46.34.163.222:3478?transport=tcp',
      ],
      'username': 'pastil',
      'credential': turnPassword, // از کانفیگ/ENV بیلد بخوانید؛ داخل مخزن commit نشود
    },
  ],
};
final pc = await createPeerConnection(rtcConfig);
```

نکته‌ها:
- **همه‌ی تماس‌ها** (چت‌صوتی، تصویری، تماس رزرو و جلسه‌ی مشاوره) باید از یک تابع واحد ساخت `PeerConnection` با همین کانفیگ استفاده کنند.
- رمز را در کد منبع نگذارید: از `--dart-define=TURN_PASSWORD=...` یا کانفیگ ریموت استفاده کنید. (در باینری اپ قابل استخراج است؛ به همین دلیل سقف مصرف روی سرور گذاشته شده.) بلندمدت قرار است بک رمز موقت (یک‌ساعته) بدهد؛ وقتی آماده شد خبر می‌دهیم و فقط منبع `credential` عوض می‌شود.
- `iceTransportPolicy` را پیش‌فرض (`all`) نگه دارید. فقط برای تست بند ۴ به‌طور موقت `'relay'` کنید.
- وب‌اپ همین کانفیگ را با JSON `NUXT_PUBLIC_TURN_SERVERS` می‌خواند؛ قالب `urls/username/credential` یکی است.

### چطور بفهمیم TURN کار می‌کند (از اپ)
در `getStats` یک `candidate-pair` با `nominated: true` پیدا کنید و `localCandidateType` / `remoteCandidateType` را ببینید (بند ۵). اگر دو طرف روی شبکه‌های سخت (دیتای موبایل) بودند و `relay` دیدید، TURN دارد کار می‌کند.

## ۲) نکته‌ی تشخیصی مهم

در گزارش باگ: «صدا می‌آید، تصویر طرف مقابل نه، تصویر خودم دیده می‌شود». چون صدا و تصویر در یک اتصال (BUNDLE) و روی یک ICE transport می‌روند، **اگر صدا می‌آید پس ICE وصل است**. یعنی مشکل NAT/TURN نیست (TURN را برای پایداری گذاشتیم، نه برای این باگ). مشکل در یکی از این سه است:

1. **تصویر اصلاً نمی‌رسد** (negotiation): m-line ویدیو در offer/answer نبوده یا `inactive/recvonly/sendonly` شده، یا طرف مقابل track ویدیو را به `pc` اضافه نکرده.
2. **تصویر می‌رسد ولی دیکد نمی‌شود** (codec).
3. **دیکد می‌شود ولی نمایش داده نمی‌شود** (رندر: `RTCVideoRenderer`/`RTCVideoView`، یا در Safari المان `<video>`).

برای تفکیک دقیق این سه، لاگ بند ۵ را لازم داریم.

## ۳) چک‌لیست کد Flutter برای «تصویر طرف مقابل نمی‌آید»

به ترتیب بررسی کنید و هر کدام را که نبود درست کنید:

1. **ترتیب: اول track، بعد offer.** هر دو طرف باید `getUserMedia` را بگیرند و **همه‌ی track‌ها (صدا و تصویر) را قبل از `createOffer`/`createAnswer` با `addTrack` به `pc` بدهند**. اگر offer قبل از اضافه‌شدن track ویدیو ساخته شود، m-line ویدیو در SDP نمی‌آید و تصویر هیچ‌وقت نمی‌رسد (صدا می‌رسد چون زودتر اضافه شده بود).
   ```dart
   final stream = await navigator.mediaDevices.getUserMedia({
     'audio': true,
     'video': {'facingMode': 'user', 'width': {'ideal': 1280}, 'height': {'ideal': 720}, 'frameRate': {'ideal': 24}},
   });
   for (final t in stream.getTracks()) { await pc.addTrack(t, stream); }
   // فقط بعد: createOffer / createAnswer
   ```
2. **طرفی که offer می‌دهد** فقط کسی است که سرور با `callConnected(shouldOffer: true)` تعیین می‌کند. طرف دیگر فقط answer می‌دهد. اگر هر دو offer بدهند (glare) ممکن است ویدیو غیرفعال شود.
3. **`onTrack`**: برای هر `RTCTrackEvent`، اگر `event.streams` خالی نیست `renderer.srcObject = event.streams[0]`. اگر خالی است، یک `MediaStream` بسازید (`createLocalMediaStream`) و track را به آن اضافه کنید و همان را به renderer بدهید. فقط روی track صدا `srcObject` را ست نکنید و تصویر را فراموش نکنید.
   ```dart
   pc.onTrack = (RTCTrackEvent e) async {
     if (e.streams.isNotEmpty) {
       remoteRenderer.srcObject = e.streams[0];
     } else {
       remoteStream ??= await createLocalMediaStream('remote');
       remoteStream!.addTrack(e.track);
       remoteRenderer.srcObject = remoteStream;
     }
     setState(() {});
   };
   ```
4. **رندر:** `final remoteRenderer = RTCVideoRenderer(); await remoteRenderer.initialize();` **قبل** از اولین استفاده، و `dispose()` در پایان. در UI: `RTCVideoView(remoteRenderer, objectFit: RTCVideoViewObjectFit.RTCVideoViewObjectFitCover)`. اگر `RTCVideoView` داخل `Visibility(maintainState: false)` یا `Offstage` بود، شاید renderer بدون frame بماند. ساده‌ترین حالت را تست کنید (بدون پوشش/انیمیشن).
5. **صف ICE:** candidateهایی که قبل از `setRemoteDescription` می‌رسند را صف کنید و بعد از آن `addCandidate` بزنید (قرارداد قبلاً در مستند هاب ذکر شده).
6. **مجوزها (iOS):** در `Info.plist`: `NSCameraUsageDescription` و `NSMicrophoneUsageDescription` (متن فارسی)، و مجوز دوربین را قبل از join بگیرید. اگر مجوز دوربین رد شده باشد ولی صدا داده شده باشد، «تصویر خودم» هم نمی‌آید؛ چون در گزارش تصویر خودتان دیده می‌شود، این مورد احتمالاً مشکل نیست.
7. **`sdpSemantics: 'unified-plan'`** صریحاً ست شود (بند ۱).
8. **آپدیت پکیج:** نسخه‌ی `flutter_webrtc` را با آخرین نسخه‌ی پایدار مقایسه کنید؛ چند باگ iOS مربوط به رندر ویدیوی remote در نسخه‌های قدیمی بوده. اگر قدیمی است، آپدیت و دوباره تست.
9. **iPhone↔iPhone در حالت پس‌زمینه:** اگر اپ به پس‌زمینه رفت و برگشت، `renderer.srcObject` را دوباره ست کنید؛ ممکن است قطع شده باشد.

## ۴) تست تفکیکی (۱۰ دقیقه)

با دو iPhone، یک تماس تصویری بگیرید، **دو بار**:

| تست | کار | نتیجه یعنی |
| --- | --- | --- |
| A | هر دو روی **یک Wi-Fi** | اگر تصویر آمد و روی دو شبکه‌ی متفاوت نیامد ⇒ مشکل مسیر شبکه (TURN/ICE) |
| B | هر دو روی Wi-Fi، `iceTransportPolicy: 'relay'` (فقط تست) | اگر با relay تصویر آمد ⇒ مسیر مستقیم مشکل داشت، TURN حل می‌کند. اگر با relay هم نیامد ⇒ مشکل negotiation/codec/رندر، نه شبکه |
| C | Android↔iPhone | اگر فقط iPhone تصویر نشان نمی‌دهد ⇒ مشکل رندر یا codec در iOS |
| D | iPhone (Flutter) ↔ iPhone (Safari وب‌اپ) | اگر Safari تصویر Flutter را می‌بیند ولی برعکس نه ⇒ مشکل دریافت/رندر در Flutter |

## ۵) لاگی که باید از اپ بگیرید و بفرستید

هر ۲ ثانیه در طول تماس، `getStats` را بخوانید و این‌ها را چاپ کنید (بدون رمز یا token):

```dart
Future<void> logStats(RTCPeerConnection pc) async {
  final reports = await pc.getStats();
  for (final r in reports) {
    final v = r.values;
    if (r.type == 'inbound-rtp' && (v['kind'] == 'video' || v['mediaType'] == 'video')) {
      print('VIDEO-IN bytes=${v['bytesReceived']} frames=${v['framesReceived']} '
            'decoded=${v['framesDecoded']} dropped=${v['framesDropped']} '
            'w=${v['frameWidth']} h=${v['frameHeight']}');
    }
    if (r.type == 'inbound-rtp' && (v['kind'] == 'audio' || v['mediaType'] == 'audio')) {
      print('AUDIO-IN bytes=${v['bytesReceived']} lost=${v['packetsLost']}');
    }
    if (r.type == 'outbound-rtp' && (v['kind'] == 'video' || v['mediaType'] == 'video')) {
      print('VIDEO-OUT bytes=${v['bytesSent']} frames=${v['framesEncoded']}');
    }
    if (r.type == 'candidate-pair' && v['nominated'] == true) {
      print('PAIR state=${v['state']} local=${v['localCandidateId']} remote=${v['remoteCandidateId']}');
    }
    if (r.type == 'local-candidate' || r.type == 'remote-candidate') {
      print('${r.type} id=${r.id} type=${v['candidateType']} proto=${v['protocol']}');
    }
  }
}
```

و یک‌بار هنگام ساخت اتصال، SDP محلی و دور را در لاگ بگذارید (می‌شود حجیم باشد؛ فقط خط‌های `m=` و `a=sendrecv|recvonly|sendonly|inactive` را چاپ کنید):

```dart
void logSdpLines(String label, String? sdp) {
  for (final l in (sdp ?? '').split('\n')) {
    if (l.startsWith('m=') || l.startsWith('a=sendrecv') || l.startsWith('a=recvonly') ||
        l.startsWith('a=sendonly') || l.startsWith('a=inactive') || l.startsWith('a=rtpmap')) {
      print('$label $l');
    }
  }
}
```

### تفسیر نتیجه
| دیدن در لاگ | یعنی | کار بعدی |
| --- | --- | --- |
| `VIDEO-IN bytes=0` و `AUDIO-IN` بالا می‌رود | تصویر نمی‌رسد | SDP را ببینید: m-line ویدیو هست؟ `a=sendrecv` یا `recvonly/inactive`؟ طرف مقابل `VIDEO-OUT bytes` دارد؟ (اگر ندارد، دوربین طرف مقابل به `pc` وصل نشده؛ بند ۳-۱) |
| `VIDEO-IN bytes` بالا می‌رود ولی `decoded=0` | codec/دیکد مشکل دارد | `a=rtpmap` را ببینید (VP8/H264)؛ نسخه‌ی پکیج را بالا ببرید |
| `decoded` بالا می‌رود ولی تصویر نیست | رندر | بند ۳-۴ و تست D |
| `PAIR` با `candidateType=relay` | TURN استفاده شده | عالی؛ مسیر درست است |

لاگ‌ها و نتیجه‌ی تست A تا D را برای مهراد بفرستید.

## ۶) DataChannel وضعیت میکروفون/دوربین (اختیاری)

وب‌اپ روی هر تماس یک **DataChannel هم‌امضا (negotiated)** می‌سازد تا «دوربین/میکروفون طرف مقابل خاموش است» را نشان دهد:

- label: `pastil-call`، `negotiated: true`، `id: 0`.
- پیام: رشته‌ی JSON فقط با `{"v":1,"mic":true|false,"cam":true|false}` (حداکثر ۲۰۰ نویسه).
- هر طرف هنگام باز شدن کانال و هر بار mute/camera-off پیام می‌فرستد.
- وب‌اپ اگر این کانال را از طرف مقابل نگیرد، همه چیز را «روشن» فرض می‌کند (نشان خاموشی نمی‌دهد) و **تصویر را پنهان نمی‌کند**. پس اگر اپ این کانال را نسازد، تماس خراب نمی‌شود.
- اگر می‌خواهید در Flutter هم نشانگر داشته باشید:
  ```dart
  final init = RTCDataChannelInit()..negotiated = true..id = 0;
  final ch = await pc.createDataChannel('pastil-call', init);
  ch.onDataChannelState = (s) { if (s == RTCDataChannelState.RTCDataChannelOpen) sendState(); };
  ch.onMessage = (m) { /* JSON.parse → {v, mic, cam} */ };
  ```
- **حتماً** وقتی کاربر دوربینش را خاموش می‌کند `cam:false` بفرستید و وقتی روشن کرد `cam:true`؛ در غیر این صورت وب‌اپ طرف مقابل تصویر را نشان نمی‌دهد (`cam:false` آخرین پیام می‌ماند).

## ۷) سایر تغییرهای بک که اپ باید بداند

### ۷.۱) مشاوره‌ی قابل‌رزرو (انتخاب ساعت مثل ۱۶:۰۰)
- مستند کامل: [CONSULTATION_BOOKING_AGENT_AMIRMOHSEN_FA.md](CONSULTATION_BOOKING_AGENT_AMIRMOHSEN_FA.md) (طرف نماینده) و [CONSULTATION_BOOKING_USER_ARMAN_FA.md](CONSULTATION_BOOKING_USER_ARMAN_FA.md) (طرف کاربر، برای تیم کاربر).
- **اپ باید تعریف کند:** فیلد `bookable` در فرم پکیج، صفحه‌ی «ساعت‌های کاری هفتگی» (`GET/PUT /api/Companion/ConsultationAvailability`)، فیلدهای `scheduledStart/scheduledEnd` در فهرست مدیریت، و پوش‌های ۸۳ و ۸۵.
- وضعیت: **مایگریشن روی سرور اجرا نشده**؛ تا اجرا نشود endpointهای جدید خطا می‌دهند.

### ۷.۲) حذف نرم پانسیون/مهد پت/مدرسه
- مستند کامل: [PANSION_SCHOOL_SOFT_DELETE_AMIRMOHSEN_FA.md](PANSION_SCHOOL_SOFT_DELETE_AMIRMOHSEN_FA.md).
- **اپ باید تعریف کند:** دکمه‌ی «حذف» (فقط مالک) با `DELETE /api/Companion/Pansion?id=` و `DELETE /api/Companion/School?id=`، نمایش پیام «رزرو فعال دارد»، و رفتار 404/«چیزی یافت نشد» برای آیتم حذف‌شده.
- وضعیت: **SQL اجرا نشده**؛ تا اجرا نشود همه‌ی endpointهای پانسیون/مدرسه خطا می‌دهند.

### ۷.۳) ویرایش خدمت (`PUT /api/Companion/CompanionAssistance`)
- باگ سمت سرور بود: ویرایش هر خدمت موجود با پیام «اشکالی در فرایند به وجود آمده است» (و `isSuccess:false`، HTTP 200) شکست می‌خورد. در `CompanionAssistanceService.UpdateAsyncDto` رفع شد.
- **اپ لازم نیست چیزی عوض کند.** بعد از دیپلوی بک، یک‌بار ویرایش یک خدمت موجود را تست کنید، یک بار **بدون** `companionAssistanceTypeIds` و یک بار **با** آن.
- وضعیت: رفع در کد است، **روی سرور دیپلوی نشده**.

## ۸) چک‌لیست اپ

- [ ] ساخت `RTCPeerConnection` فقط از یک تابع و با `iceServers` (STUN + TURN UDP/TCP)، `sdpSemantics: 'unified-plan'`
- [ ] رمز TURN از کانفیگ/ENV بیلد (نه داخل گیت)
- [ ] همه‌ی track‌ها قبل از `createOffer/createAnswer` به `pc` اضافه شوند
- [ ] فقط طرف `shouldOffer` offer می‌دهد
- [ ] `onTrack`: `streams[0]` یا ساخت `MediaStream` و گذاشتن روی `RTCVideoRenderer`
- [ ] `RTCVideoRenderer.initialize()` قبل از استفاده؛ `RTCVideoView` بدون Visibility/Offstage
- [ ] مجوزهای `Info.plist`
- [ ] صف ICE تا بعد از `setRemoteDescription`
- [ ] لاگ `getStats` و خطوط SDP + نتیجه‌ی تست‌های A تا D برای مهراد
- [ ] (اختیاری) DataChannel `pastil-call` (negotiated، id=0) و ارسال `cam/mic`
- [ ] مرور مستندهای ۷.۱ و ۷.۲ و تست ۷.۳ بعد از دیپلوی
