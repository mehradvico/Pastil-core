# مستند اپ نوبس: پکیج مشاوره آنلاین، چت، تماس صوتی و ویدیوکال

این سند قرارداد اجرایی اپ نوبس برای «پکیج مشاوره آنلاین» است: انتخاب پکیج، خرید و پرداخت، انتظار برای نماینده، شروع جلسه، چت/تماس/ویدیو، بازگشت به جلسه و تغییر وضعیت تا پایان.

> این محصول با پکیج/رزرو عادی نماینده فرق دارد. برای این flow از `ConsultationPackage`، `ConsultationPurchase` و `OnlineSession` استفاده کنید؛ از `CompanionReserve`، `CompanionReserveMessage` و `JoinCall(reserveId)` استفاده نکنید.

تمام endpointهای این سند به `Authorization: Bearer <access-token>` نیاز دارند. در پاسخ‌های API، `isSuccess` معیار موفقیت است؛ HTTP 200 به تنهایی موفقیت را تضمین نمی‌کند.

---

## 1. مدل محصول و کانال‌ها

هر کلینیک می‌تواند برای هر ترکیب «کانال × مدت» یک پکیج فعال با قیمت مثبت داشته باشد. مدت‌ها فقط ۳۰ یا ۶۰ دقیقه‌اند و زمان از لحظه‌ی **شروع نماینده** آغاز می‌شود، نه از زمان خرید کاربر.

| `channelId` | نام | رفتار کاربر |
| ---: | --- | --- |
| 1 | چت | صفحه‌ی گفت‌وگوی مدت‌دار |
| 2 | تماس درون‌برنامه‌ای | تماس صوتی WebRTC از داخل اپ |
| 3 | تماس تصویری | تماس صوتی‌تصویری WebRTC از داخل اپ |
| 4 | تماس تلفنی | نماینده از خارج اپ تماس می‌گیرد؛ اپ فقط وضعیت و زمان باقی‌مانده را نمایش می‌دهد |

پکیج عمومی:

```ts
type ConsultationPackage = {
  id: number
  companionId: number
  channelId: 1 | 2 | 3 | 4
  durationMinutes: 30 | 60
  price: number
}
```

backend فقط پکیج فعال و با قیمت مثبت را برمی‌گرداند. اگر لیست خالی بود، متن «این مرکز فعلاً مشاوره آنلاین فعالی ندارد» نمایش دهید.

---

## 2. ماشین حالت خرید مشاوره

```text
PendingPayment
  ├─ پرداخت موفق ──────────> Paid
  │                            ├─ نماینده شروع می‌کند ─> Active
  │                            │                            └─ پایان زمان ─> Completed
  │                            ├─ لغو کاربر ───────────> Cancelled ─> Refunded
  │                            └─ شروع‌نشدن تا ۲۴ ساعت ─> Expired ───> Refunded
  └─ پرداخت ناموفق ────────> Cancelled
```

| `status` | نام نمایشی | معنا و CTA اپ |
| ---: | --- | --- |
| 1 | در انتظار پرداخت | پرداخت کامل نشده؛ وضعیت را از API بازیابی کنید |
| 2 | منتظر شروع نماینده | پرداخت موفق شده؛ لغو قبل از شروع ممکن است |
| 3 | در جریان | پنجره‌ی ۳۰/۶۰ دقیقه‌ای باز است؛ ورود/بازگشت به session |
| 4 | پایان‌یافته | زمان تمام شده؛ بدون ارسال پیام یا اتصال تماس |
| 5 | منقضی‌شده | حالت میانجیِ شروع‌نشدن؛ معمولاً بلافاصله به بازپرداخت‌شده می‌رسد |
| 6 | لغوشده | پرداخت ناموفق یا لغو آغاز شده؛ به `refundDate` و status نهایی توجه کنید |
| 7 | بازپرداخت‌شده | مبلغ `paymentPrice` به کیف پول برگشته است |

قواعد مهم:

- کاربر برای یک کلینیک و یک کانال هم‌زمان نمی‌تواند بیش از یک خرید `Paid` یا `Active` داشته باشد.
- کاربر فقط در `Paid` می‌تواند لغو کند؛ بعد از `Active` لغو ندارد.
- `Active` فقط یک‌بار توسط نماینده ساخته می‌شود. قطع زودهنگام چت/تماس، پنجره‌ی زمان را متوقف یا reset نمی‌کند.
- پایان خودکار و بازپرداخت توسط job سرور انجام می‌شود؛ app فقط state را refresh و نمایش می‌دهد.

---

## 3. انتخاب پکیج و خرید

### 3.1 دریافت پکیج‌های یک کلینیک

```http
GET /api/EndUser/ConsultationPackage?companionId=245
```

صفحه‌ی خرید باید کانال‌ها را از پکیج‌های برگشتی بسازد، سپس durationهای همان کانال را نمایش دهد. کانال یا مدت را hard-code به عنوان موجودی کلینیک نکنید.

### 3.2 ساخت خرید و آغاز پرداخت

```http
POST /api/EndUser/ConsultationPurchase
Authorization: Bearer <token>
Idempotency-Key: 7273d44f-88c2-44b5-a88c-227e1f8dc8ac
Content-Type: application/json
```

```json
{
  "packageId": 810,
  "fromWallet": true,
  "merchantId": 3,
  "rebateCode": "OPTIONAL"
}
```

فقط این فیلدها را بفرستید. قیمت، مدت، channel، سهم کیف پول، مبلغ درگاه، وضعیت، زمان پایان و شناسه‌ی کاربر توسط server تعیین می‌شود.

- اگر بعد از تخفیف و کیف پول مبلغی باقی بماند، `merchantId` معتبر لازم است.
- `fromWallet: true` به‌معنای پوشش قطعی کل مبلغ نیست؛ backend مقدار واقعی قابل برداشت را در زمان خرید تعیین می‌کند.
- برای هر checkout یک UUID استاندارد در `Idempotency-Key` بفرستید و هنگام retry همان checkout همان کلید را نگه دارید.
- دکمه‌ی خرید را تا پاسخ busy کنید.

پاسخ موفق `data` از نوع شروع پرداخت است. رفتار آن:

```text
isSuccess=false → پیام API را نمایش بده
paymentIsLink=true + paymentUrl → فقط همان paymentUrl را external-open کن
paymentIsLink=false → انتقال به درگاه نده؛ فهرست خریدها را refresh کن
```

`paymentUrl` و callback token را app نسازد یا تغییر ندهد. برگشت از درگاه به معنی پرداخت‌شده نیست؛ نتیجه را از API خرید بخوانید.

### 3.3 بازیابی پس از قطع شبکه یا برگشت درگاه

```http
GET /api/EndUser/ConsultationPurchase
GET /api/EndUser/ConsultationPurchase/{purchaseId}
```

مدل نمایشی خرید:

```ts
type ConsultationPurchase = {
  id: number
  purchaseCode: string
  companionId: number
  companionName: string
  channelId: number
  durationMinutes: number
  price: number
  rebatePrice: number
  paymentPrice: number
  walletPrice: number
  status: number
  createDate: string
  paidDate?: string | null
  startDeadline?: string | null
  startDate?: string | null
  expireDate?: string | null
  onlineSessionId?: number | null
  cancelDate?: string | null
  refundDate?: string | null
  serverNow: string
}
```

در نتیجه‌ی نامعلوم POST، ابتدا فهرست خریدها را refresh کنید و وضعیت واقعی را نمایش دهید. به‌ویژه زمانی که پرداختی در ۱۵ دقیقه‌ی گذشته آغاز شده است، backend خرید تکراری همان پکیج را قفل می‌کند؛ برای حل آن خرید جدید نسازید و مسیر پرداخت/وضعیت موجود را بازیابی کنید.

---

## 4. انتظار برای شروع نماینده و لغو/بازپرداخت

پس از پرداخت موفق، وضعیت `Paid` است و `startDeadline = paidDate + 24 ساعت` برمی‌گردد. این صفحه باید به کاربر بگوید «منتظر شروع نماینده بمانید؛ زمان مشاوره از شروع نماینده حساب می‌شود.»

### 4.1 لغو توسط کاربر، فقط پیش از شروع

```http
PUT /api/EndUser/ConsultationPurchase/{purchaseId}/Cancel
```

- این درخواست فقط در `status = Paid` پذیرفته می‌شود.
- backend گذار را اتمی انجام می‌دهد؛ اگر نماینده هم‌زمان Start را زده باشد، لغو ناموفق است و باید state تازه نمایش داده شود.
- مبلغی که کاربر واقعاً پرداخت کرده (`paymentPrice` پس از تخفیف) به کیف پول برمی‌گردد؛ بازپرداختِ درگاه به حساب بانکی در این flow وجود ندارد.

### 4.2 شروع‌نشدن تا موعد

اگر نماینده تا ۲۴ ساعت شروع نکند، server هر دقیقه بررسی می‌کند و خرید را منقضی/بازپرداخت می‌کند. کاربر نیاز به درخواست جدا ندارد. در فهرست، پس از refresh باید `Refunded` و `refundDate` نمایش داده شود.

اعلان push مسیر اصلی اطلاع‌رسانی است، اما app نباید تنها به push وابسته باشد. در صفحه‌ی فهرست، pull-to-refresh و refresh هنگام resume را داشته باشید؛ هنگام انتظار می‌توان هر ۱۵ تا ۳۰ ثانیه و فقط در foreground جزئیات خرید را poll کرد.

---

## 5. شروع جلسه و ورود کاربر

کاربر session را شروع نمی‌کند. نماینده‌ی مجاز کلینیک با endpoint اختصاصی خود Start می‌زند. این عمل:

1. خرید را از `Paid` به `Active` می‌برد؛
2. `startDate` و `expireDate = startDate + 30/60 دقیقه` را ثبت می‌کند؛
3. یک `OnlineSession` می‌سازد و `onlineSessionId` را روی خرید می‌گذارد؛
4. به کاربر اطلاع می‌دهد.

کاربر session را با این endpoint پیدا می‌کند:

```http
GET /api/EndUser/ConsultationPurchase/Active
```

پاسخ شامل پنجره‌های فعال است:

```ts
type ConsultationActiveWindow = {
  purchaseId: number
  onlineSessionId: number
  channelId: number
  durationMinutes: number
  startDate: string
  expireDate: string
  serverNow: string
  companionName: string
  agentFullName: string
}
```

### شمارش معکوس

برای شمارش معکوس فقط ساعت سرور پاسخ را مبنا بگیرید:

```ts
remainingMs = Date.parse(expireDate) - Date.parse(serverNow) - elapsedSinceResponseMs
```

ساعت دستگاه، timezone یا ساعت محلی را مبنای پایان جلسه قرار ندهید. در foreground هر ۱۵ تا ۳۰ ثانیه و در resume/focus endpoint Active را refresh کنید. در background polling را قطع کنید.

مسیر UI بر اساس channel:

| channel | صفحه/عمل اپ |
| --- | --- |
| Chat (1) | صفحه‌ی chat با `onlineSessionId` |
| InAppCall (2) | صفحه‌ی تماس صوتی با `onlineSessionId` |
| VideoCall (3) | صفحه‌ی ویدیوکال با `onlineSessionId` |
| Phone (4) | صفحه‌ی وضعیت/شمارش معکوس؛ نماینده خارج از اپ تماس می‌گیرد |

برای تماس تلفنی، دکمه‌ی «بازگشت به تماس» یا اتصال SignalR نسازید. فقط state `Active`، زمان باقی‌مانده و متن «نماینده با شما تماس می‌گیرد» را نشان دهید.

---

## 6. چت پکیج مشاوره

چت بعد از `Active` و با `onlineSessionId` کار می‌کند. ابتدا session را برای کنترل دسترسی/انقضا بخوانید:

```http
GET /api/OnlineSession/{onlineSessionId}
```

### 6.1 دریافت پیام‌ها و pagination

```http
GET /api/OnlineSessionMessage?onlineSessionId=501&PageSize=30
GET /api/OnlineSessionMessage?onlineSessionId=501&afterMessageId=128&PageSize=30
GET /api/OnlineSessionMessage?onlineSessionId=501&beforeMessageId=90&PageSize=30
```

- بدون cursor، جدیدترین صفحه را می‌گیرد و در پاسخ به ترتیب زمانی صعودی می‌دهد.
- برای پیام جدید از `afterMessageId` استفاده کنید.
- برای scroll به تاریخچه از `beforeMessageId` استفاده کنید.
- `PageSize` را بین ۱ تا ۱۰۰ نگه دارید؛ مقدار پیشنهادی ۳۰ است.
- polling پیام‌ها فقط وقتی صفحه‌ی chat visible است اجرا شود؛ فاصله‌ی پیشنهادی ۴ تا ۵ ثانیه است.

### 6.2 ارسال پیام

```http
POST /api/OnlineSessionMessage
Content-Type: application/json
```

```json
{
  "onlineSessionId": 501,
  "content": "سلام، برای پت من ...",
  "imageUrl": null,
  "imageThumbnailUrl": null
}
```

قواعد backend:

- پیام باید متن یا تصویر داشته باشد؛ caption تصویر اختیاری است.
- حداکثر طول متن ۴۰۰۰ کاراکتر است.
- URL تصویر و thumbnail باید از دامنه/مسیر مجاز upload پاستیل باشند؛ URL دلخواه اینترنتی را ارسال نکنید.
- فقط دو participant همان session می‌توانند بخوانند/ارسال کنند.
- بعد از رسیدن `expireDate`، server ارسال را رد می‌کند؛ UI هم composer را غیرفعال و متن «گفتگو فقط‌خواندنی است» نمایش دهد.
- تاریخچه بعد از پایان قابل خواندن می‌ماند.

### 6.3 read receipt

وقتی کاربر پیام‌های طرف مقابل را واقعاً در صفحه مشاهده کرد:

```http
PUT /api/OnlineSessionMessage/Read

{ "onlineSessionId": 501, "lastMessageId": 128 }
```

این عملیات idempotent است؛ آخرین شناسه‌ی دیده‌شده را نگه دارید و برای هر render تکرار نکنید.

---

## 7. تماس صوتی و ویدیوکال داخل اپ

برای channel 2 و 3، media مستقیماً با WebRTC بین دو دستگاه عبور می‌کند؛ backend فقط signaling SignalR را روی Hub زیر relay می‌کند:

```text
wss://{API_HOST}/hubs/call?access_token={accessToken}
```

Hub فقط با token معتبر قابل استفاده است. clientهای native باید token را با سازوکار access token SignalR/در handshake (در صورت نیاز query `access_token`) ارسال کنند. token را در log، notification payload یا analytics ثبت نکنید.

### 7.1 ترتیب ورود کاربر به تماس

1. از `GET /OnlineSession/{id}` بررسی کنید session وجود دارد، شما participant هستید، `endDate` خالی است و `channelId` دقیقاً ۲ یا ۳ است.
2. بررسی کنید `expireDate` نگذشته باشد؛ اگر گذشته، صفحه تماس را باز نکنید.
3. فقط با tap مستقیم کاربر، permission میکروفون (و برای video، دوربین) را درخواست کنید. در iOS و Android/WebView درخواست خارج از user gesture قابل اتکا نیست.
4. به `/hubs/call` وصل شوید و `JoinSessionCall(sessionId)` را invoke کنید.
5. رویدادها را طبق جدول زیر handle کنید؛ سپس WebRTC peer connection را بسازید.

### 7.2 قرارداد Hub برای session پکیج

| جهت | نام | payload / اقدام |
| --- | --- | --- |
| client → hub | `JoinSessionCall(sessionId)` | ورود به تماس session |
| hub → client | `waitingForPeer` | حالت ringing/waiting؛ نماینده‌ی آغازکننده push تماس را برای کاربر می‌فرستد |
| hub → client | `callConnected(shouldOffer)` | اگر `true`، SDP offer بسازید؛ در غیر این صورت منتظر offer بمانید |
| client → hub | `SendSignal(-sessionId, type, payload)` | signaling WebRTC؛ کلید session **منفی** است |
| hub → client | `signal(type, payload)` | offer / answer / ICE طرف مقابل |
| client → hub | `EndCall()` | قطع کاربر |
| hub → client | `callEnded` | تماس را بسته و همه stream/resourceها را cleanup کنید |
| hub → client | `callError(message)` | تماس را باز نکنید/ببندید و پیام قابل‌فهم نمایش دهید |

مقادیر مجاز `type` فقط `offer`، `answer` و `ice` هستند و payload سیگنال حداکثر ۳۲KB است. `SendSignal` برای session باید حتماً با `-onlineSessionId` فراخوانی شود؛ شناسه‌ی مثبت برای flow قدیمی رزرو است.

### 7.3 تفاوت تماس صوتی و تصویری

| مورد | صوتی | تصویری |
| --- | --- | --- |
| channel | 2 | 3 |
| مجوز لازم | microphone | microphone + camera |
| getUserMedia | `audio: true, video: false` | `audio: true, video: true` |
| UI ضروری | mute، speaker، پایان تماس | mute، camera on/off، local preview، remote video، پایان تماس |

اگر permission رد شد، تماس را به‌طور خودکار retry نکنید؛ راهنمای فعال‌کردن permission در تنظیمات app/OS نشان دهید. `EndCall` و cleanup stream/peer/hub را در خروج صفحه، قطع شبکه و `callEnded` انجام دهید.

### 7.4 تماس ورودی و push fallback

وقتی نماینده به Hub وارد شود، backend برای کاربر push تماس صوتی/تصویری می‌فرستد. اگر push نرسید یا app هنگام تماس باز شد، این endpoint را در resume و در foreground با فاصله‌ی ملایم (مثلاً ۵ تا ۱۰ ثانیه) بررسی کنید:

```http
GET /api/EndUser/CallPending
```

پاسخ نمونه:

```json
{
  "reserveId": null,
  "sessionId": 501,
  "callerName": "نام نماینده",
  "isVideo": true
}
```

برای این محصول، اگر `sessionId` موجود بود صفحه‌ی تماس session را باز کنید. `reserveId` مربوط به flow قدیمی CompanionReserve است و در این سند استفاده نمی‌شود.

### 7.5 پایان زمان در میانه‌ی تماس

سرور هنگام رسیدن `expireDate` به دو طرف `callEnded` می‌فرستد و اتصال جدید بعد از زمان را رد می‌کند. بنابراین timer محلی فقط برای UX است؛ بستن واقعی تماس را با event hub و refresh state نیز handle کنید.

---

## 8. اتمام جلسه و refresh نهایی

وقتی `expireDate` برسد:

- chat composer فقط‌خواندنی می‌شود؛
- `JoinSessionCall` برای session رد می‌شود؛
- تماس باز با `callEnded` بسته می‌شود؛
- job سرور حداکثر در اجرای دقیقه‌ای بعدی status خرید را از `Active` به `Completed` تغییر می‌دهد و `OnlineSession.endDate` را می‌بندد.

در این نقطه app باید:

1. streamهای تماس، peer connection، timer و SignalR را cleanup کند؛
2. `GET /ConsultationPurchase/{id}` یا فهرست خریدها را refresh کند؛
3. وضعیت «پایان‌یافته» و تاریخچه‌ی فقط‌خواندنی chat را نمایش دهد؛
4. از ایجاد session، ارسال پیام یا شروع تماس جدید با همان خرید جلوگیری کند.

کاربر endpoint دستی برای `Completed` کردن خرید ندارد.

---

## 9. پنل نماینده / کلینیک

این بخش بخشی از flow کاربر نیست، اما برای عملی‌شدن کامل پکیج‌های مشاوره باید در پنل نماینده یا کلینیک پیاده‌سازی شود. مسیرهای این بخش با توکن نماینده فراخوانی می‌شوند.

### 9.1 مدیریت پکیج‌ها در «خدمات»

این صفحه باید یک ماتریس **۴ کانال × ۲ مدت = ۸ خانه** نشان دهد؛ هر خانه قیمت مستقل و کلید فعال/غیرفعال دارد:

| کانال | ۳۰ دقیقه | ۶۰ دقیقه |
| --- | --- | --- |
| Chat (`channelId: 1`) | قیمت + فعال/غیرفعال | قیمت + فعال/غیرفعال |
| InAppCall (`channelId: 2`) | قیمت + فعال/غیرفعال | قیمت + فعال/غیرفعال |
| VideoCall (`channelId: 3`) | قیمت + فعال/غیرفعال | قیمت + فعال/غیرفعال |
| Phone (`channelId: 4`) | قیمت + فعال/غیرفعال | قیمت + فعال/غیرفعال |

```http
GET /api/Companion/ConsultationPackage
PUT /api/Companion/ConsultationPackage
```

خروجی و بدنه‌ی ذخیره‌سازی به این شکل است. در `PUT` هر ۸ خانه را بفرستید، حتی خانه‌های غیرفعال، تا UI و server یک ماتریس قطعی داشته باشند:

```json
{
  "items": [
    { "id": 101, "channelId": 1, "durationMinutes": 30, "price": 250000, "active": true },
    { "id": 102, "channelId": 1, "durationMinutes": 60, "price": 450000, "active": true },
    { "id": 0, "channelId": 2, "durationMinutes": 30, "price": 0, "active": false }
  ]
}
```

- فقط ترکیب‌های channel ۱ تا ۴ و duration ۳۰ یا ۶۰ معتبرند؛ duplicate نفرستید.
- `price` باید صفر یا مثبت باشد، اما `active: true` با `price <= 0` رد می‌شود.
- خانه‌ی جدیدِ غیرفعال با قیمت صفر ممکن است `id: 0` داشته باشد؛ این طبیعی است و در server ذخیره نمی‌شود.
- غیرفعال‌کردن، پکیج را از خریدهای جدید پنهان می‌کند؛ **خرید Paid یا Active قبلی را لغو یا متوقف نمی‌کند**.
- قیمتِ پکیجِ تازه فقط برای خریدهای بعدی است؛ مبلغ خریدهای موجود snapshot شده و نباید در پنل آن‌ها را با قیمت تازه نمایش/محاسبه کرد.

### 9.2 فهرست «مشاوره‌های من» و صف شروع

```http
GET /api/Companion/ConsultationSession
```

این endpoint فقط خریدهای `Paid` و `Active` کلینیک‌هایی را می‌دهد که کاربر جاری نماینده‌ی مجاز آن‌ها است؛ ابتدا sessionهای Active و سپس خریدهای Paid می‌آیند. هر row شامل مشخصات مشتری، `channelId`، مدت، status، `startDeadline`، `expireDate`، `onlineSessionId` و مهم‌تر از همه `canStart`، `canEnter` و `serverNow` است.

پنل باید دو گروه واضح بسازد:

| گروه | شرط | عمل |
| --- | --- | --- |
| منتظر شروع | `status = Paid` و `canStart = true` | دکمهٔ «شروع جلسه» |
| جلسهٔ فعال | `status = Active` و `canEnter = true` | دکمهٔ «ورود/بازگشت به جلسه» |

ردیف Paid با `canStart: false` را با علت «مهلت شروع گذشته است» یا پس از refresh با status نهایی نمایش دهید، نه با دکمهٔ Start. اگر `canEnter: false` است، دکمهٔ ورود را نشان ندهید؛ session فعال ممکن است توسط نماینده‌ی دیگری شروع شده باشد و فقط خود شروع‌کننده یا مالک کلینیک اجازهٔ ورود دوباره دارد.

برای شروع، تنها این درخواست را با تأیید صریح نماینده بزنید:

```http
POST /api/Companion/ConsultationSession/{purchaseId}/Start
```

پاسخ شامل `purchaseId`، `onlineSessionId`، `channelId`، `startDate`، `expireDate`، `serverNow` و مشخصات مشتری است. همین لحظه timer شروع می‌شود؛ پس دکمهٔ Start را پیش از ارسال disable کنید. در خطای شبکه یا پاسخ «قبلاً شروع شده»، کورکورانه retry نکنید: `GET /ConsultationSession` را refresh کنید و براساس row تازه عمل کنید. این endpoint گذار Paid → Active و ساخت OnlineSession را به صورت اتمی انجام می‌دهد.

### 9.3 کنترل جلسهٔ فعال در پنل

برای ورود دوباره یا بعد از refresh صفحه ابتدا مجوز و زمان پنجره را از server بگیرید:

```http
POST /api/Companion/ConsultationSession/{purchaseId}/Enter
```

اگر پاسخ موفق بود، route مناسب را از `channelId` انتخاب کنید و countdown را با `expireDate` و `serverNow` بسازید؛ ساعت دستگاه source of truth نیست. در foreground هر ۱۵ تا ۳۰ ثانیه و در resume فهرست sessionها را refresh کنید. بعد از رسیدن زمان، کنترل‌ها را ببندید و منتظر refresh status به `Completed` بمانید.

| channel | کنترل پنل نماینده در جلسهٔ فعال |
| --- | --- |
| 1 — Chat | صفحهٔ چت با `onlineSessionId`؛ دریافت/ارسال/read receipt طبق بخش ۶ |
| 2 — تماس درون‌برنامه‌ای | صفحهٔ تماس صوتی؛ اتصال Hub و WebRTC طبق بخش ۷ |
| 3 — ویدیوکال | صفحهٔ ویدیو؛ اتصال Hub و WebRTC طبق بخش ۷ |
| 4 — تماس تلفنی | فقط شمارش معکوس و اطلاعات تماس مشتری؛ **SignalR/WebRTC یا دکمهٔ تماس داخل‌برنامه‌ای نسازید** |

برای channelهای ۱ تا ۳ نیز نماینده فقط در بازه‌ی فعال می‌تواند وارد session شود. در خطای `callEnded`، `callError`، انقضا یا ترک صفحه، streamهای media، peer connection، timer و SignalR را کامل cleanup کنید. پایان تماس WebRTC به معنای توقف/تمدید زمان مشاوره نیست.

---

## 10. state پیشنهادی نوبس

```ts
type ConsultationUiState = {
  companionId: number
  packageId?: number
  purchaseId?: number
  onlineSessionId?: number
  channelId?: 1 | 2 | 3 | 4
  paymentIdempotencyKey?: string
  purchaseStatus?: 1 | 2 | 3 | 4 | 5 | 6 | 7
  startDeadline?: string
  expireDate?: string
  serverNow?: string
  lastReadMessageId?: number
  lastFetchedMessageId?: number
}
```

| lifecycle | داده‌ای که باید پایدار نگه دارید | منبع truth هنگام resume |
| --- | --- | --- |
| checkout | `packageId` و UUID همان checkout | `GET /ConsultationPurchase` |
| منتظر شروع | `purchaseId` و `startDeadline` | `GET /ConsultationPurchase/{id}` |
| جلسه فعال | `purchaseId`، `onlineSessionId`، channel و `expireDate` | `GET /ConsultationPurchase/Active` |
| chat | `onlineSessionId` و cursor پیام | `GET /OnlineSessionMessage` |
| call | فقط `onlineSessionId`؛ نه SDP/ICE یا token | `GET /OnlineSession/{id}` سپس Hub |

رمز، token، SDP، ICE candidate و raw message attachment را در analytics/log دائمی ذخیره نکنید.

---

## 11. چک‌لیست QA اپ نوبس و پنل نماینده

### خرید و وضعیت

- [ ] فقط پکیج‌های برگردانده‌شده برای کلینیک نمایش داده می‌شوند.
- [ ] price/duration/channel از client قابل جعل نیست و در UI از پاسخ server نمایش می‌گیرد.
- [ ] checkout همان تلاش UUID ثابت دارد؛ در timeout ابتدا فهرست خریدها recover می‌شود.
- [ ] `paymentIsLink=false` redirect ندارد و خرید refresh می‌شود.
- [ ] لغو فقط در Paid نمایش دارد و پس از Active مخفی/غیرفعال است.
- [ ] نتیجه‌ی انقضای شروع‌نشدن به‌صورت Refunded و موجودی کیف پول در UI refresh می‌شود.

### session و چت

- [ ] `Active` با ساعت server countdown می‌سازد، در background poll نمی‌کند و در resume refresh می‌شود.
- [ ] user هیچ endpoint Start session را فراخوانی نمی‌کند.
- [ ] ارسال پیام بعد از expire هم در UI و هم از خطای server handle می‌شود.
- [ ] read receipt فقط برای آخرین پیام واقعاً دیده‌شده ثبت می‌شود.

### تماس و ویدیو

- [ ] قبل از `JoinSessionCall`، channel، participant و زمان session validate می‌شود.
- [ ] permission با tap مستقیم کاربر گرفته می‌شود.
- [ ] `SendSignal` با `-sessionId` و فقط offer/answer/ice فرستاده می‌شود.
- [ ] `callEnded`/`callError` همه‌ی streamها، peer connection و Hub را cleanup می‌کند.
- [ ] `CallPending` در resume کار می‌کند و فقط `sessionId` را برای پکیج باز می‌کند.
- [ ] پایان پنجره در تماس و chat بدون امکان restart همان خرید نمایش داده می‌شود.

### پنل نماینده / کلینیک

- [ ] صفحهٔ خدمات دقیقاً ۸ خانهٔ channel × duration را از `GET /Companion/ConsultationPackage` می‌سازد و ذخیره با `PUT` همهٔ خانه‌ها را می‌فرستد.
- [ ] فعال‌سازی با قیمت صفر پیش از ارسال در UI منع می‌شود و خطای server هم نمایش درست دارد.
- [ ] صفحهٔ مشاوره‌ها فقط وقتی `canStart` است Start و فقط وقتی `canEnter` است Enter را فعال می‌کند.
- [ ] double-tap یا retry Start به ساخت دو session یا دو timer در UI منجر نمی‌شود؛ پس از هر نتیجه فهرست refresh می‌شود.
- [ ] channel تلفنی فقط countdown و اطلاعات تماس را دارد و مسیر Hub/WebRTC را باز نمی‌کند.

---

## 12. فهرست endpointها و Hub

| کاربرد | Method | مسیر / متد |
| --- | --- | --- |
| پکیج‌های فعال کلینیک | GET | `/api/EndUser/ConsultationPackage?companionId={id}` |
| خرید و شروع پرداخت | POST | `/api/EndUser/ConsultationPurchase` |
| فهرست خریدهای من | GET | `/api/EndUser/ConsultationPurchase` |
| جزئیات خرید | GET | `/api/EndUser/ConsultationPurchase/{id}` |
| پنجره‌های فعال | GET | `/api/EndUser/ConsultationPurchase/Active` |
| لغو قبل از شروع | PUT | `/api/EndUser/ConsultationPurchase/{id}/Cancel` |
| جزئیات session | GET | `/api/OnlineSession/{id}` |
| پیام‌ها | GET/POST | `/api/OnlineSessionMessage` |
| read پیام‌ها | PUT | `/api/OnlineSessionMessage/Read` |
| تماس در انتظار | GET | `/api/EndUser/CallPending` |
| signaling | SignalR | `/hubs/call`: `JoinSessionCall`, `SendSignal`, `EndCall` |
| ماتریس پکیج پنل | GET/PUT | `/api/Companion/ConsultationPackage` |
| صف/جلسه‌های نماینده | GET | `/api/Companion/ConsultationSession` |
| شروع جلسه توسط نماینده | POST | `/api/Companion/ConsultationSession/{purchaseId}/Start` |
| ورود دوباره نماینده | POST | `/api/Companion/ConsultationSession/{purchaseId}/Enter` |

**وضعیت سند:** منطبق با backend فعلی پاستیل در ۲۱ سپتامبر ۲۰۲۶.
