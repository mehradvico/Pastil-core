# مستند پایان تماس و خروج/پایان چت برای نماینده و اپراتور

**مخاطب:** فرانت‌اند پنل نماینده/کلینیک و اپراتور مرکز  
**دامنه:** تماس و چت خدمات آنلاینِ رزرو (`CompanionReserve`) و جلسهٔ آنلاین مستقل (`OnlineSession`).

## جمع‌بندی قطعی

| قابلیت | روش فعلی | نتیجهٔ سرور |
|---|---|---|
| پایان تماس صوتی/تصویری درون‌برنامه‌ایِ رزرو | SignalR: `CallHub.EndCall()` | تماس برای طرف مقابل پایان می‌یابد، `callEnded` ارسال می‌شود و `callEndDate` رزرو ثبت می‌گردد. |
| پایان تماس جلسهٔ آنلاین مستقل | SignalR: `CallHub.EndCall()` | تماس WebRTC و محاسبهٔ segment فعلی پایان می‌یابد؛ `callEndDate` و مدت تماس ثبت می‌شود، اما خود `OnlineSession.endDate` بسته نمی‌شود. |
| «پایان چت» رزرو | فقط خروج از صفحه/بستن UI | endpoint سروری برای بستن چت وجود ندارد؛ تاریخچه و امکان ارسال پیام تا زمانی که رزرو لغو نشده باقی می‌ماند. |
| «پایان چت» جلسهٔ آنلاین مستقل | فقط خروج از صفحه/بستن UI | endpoint دستی برای ثبت `endDate` وجود ندارد. در مشاورهٔ مدت‌دار، پایان پنجرهٔ زمانی سرور چت را خودکار فقط‌خواندنی/بسته می‌کند. |
| تماس تلفنی عادی (`tel:`) | پایان در سیستم تلفن | سرور تماس را ردیابی یا خاتمه نمی‌دهد. |

بنابراین دکمهٔ تماس باید «پایان تماس» واقعی داشته باشد. دکمهٔ چت در API فعلی فقط می‌تواند «خروج از گفتگو» باشد، نه «بستن گفتگو برای هر دو طرف».

## ۱) تماس درون‌برنامه‌ایِ رزرو خدمت

این مدل مربوط به رزروهای `CompanionReserve` است. UI فقط وقتی باید دکمهٔ تماس را نمایش دهد که روش ارتباط آنلاین رزرو، تماس/ویدیو باشد. خود `CallHub` صرفاً مالکیت رزرو را کنترل می‌کند و نوع پکیج آنلاین را برای نمایش UI جایگزین نکنید.

### اتصال به hub

**آدرس:** `wss://api.pastil.pet/hubs/call`  
در محیط توسعه همان `API_BASE_URL` با مسیر `/hubs/call` استفاده می‌شود.

hub نیاز به JWT دارد. کلاینت SignalR باید token را با `accessTokenFactory` بدهد؛ سرور آن را به‌صورت query parameter `access_token` برای مسیر hub می‌پذیرد.

```ts
const connection = new HubConnectionBuilder()
  .withUrl(`${API_BASE_URL}/hubs/call`, {
    accessTokenFactory: () => accessToken
  })
  .withAutomaticReconnect()
  .build();

await connection.start();
await connection.invoke("JoinCall", reserveId);
```

### مجوز اتصال

سرور برای `JoinCall(reserveId)` این افراد را می‌پذیرد:

- خود کاربر رزروکننده؛
- مالک مرکز؛
- اپراتور/کارمند تخصیص‌داده‌شده به همان رزرو.

رزرو لغوشده یا تماسی که `callEndDate` دارد قابل پیوستن نیست. اپراتور نباید صرفاً به‌دلیل عضو مرکز بودن، برای رزروی که به او تخصیص ندارد دکمهٔ تماس فعال ببیند.

### رویدادهای دریافتی

| رویداد Hub | معنی | رفتار فرانت |
|---|---|---|
| `waitingForPeer` | یک طرف وارد تماس شده و منتظر طرف دوم است. | حالت انتظار و دکمهٔ لغو/پایان تماس را نمایش دهید. |
| `callConnected(isOfferer)` | هر دو طرف متصل شده‌اند؛ boolean تعیین می‌کند این سمت offer WebRTC را بسازد یا منتظر answer بماند. | media و signaling را شروع کنید. |
| `signal(type, payload)` | پیام WebRTC از طرف دیگر؛ `type` یکی از `offer`، `answer` یا `ice` است. | به `RTCPeerConnection` بدهید. |
| `callEnded` | طرف دیگر تماس را پایان داده، disconnect شده یا پنجرهٔ مجاز تمام شده است. | فوراً tracks و peer connection را ببندید و UI را «تماس پایان یافت» کنید. |
| `callError(message)` | اتصال/عملیات مجاز یا معتبر نبوده است. | پیام را نمایش دهید و UI تماس را ببندید. |

### ارسال signaling

پس از `callConnected` فقط از hub استفاده کنید؛ payload حداکثر ۳۲KiB است.

```ts
await connection.invoke("SendSignal", reserveId, "offer", JSON.stringify(offer));
await connection.invoke("SendSignal", reserveId, "answer", JSON.stringify(answer));
await connection.invoke("SendSignal", reserveId, "ice", JSON.stringify(candidate));
```

## ۲) دکمهٔ «پایان تماس»

### قرارداد دقیق

```ts
await connection.invoke("EndCall");
```

`EndCall` **بدون body و بدون `reserveId`** فراخوانی می‌شود. سرور بر اساس همان connection فعال، تماس را پیدا می‌کند. پس در هر صفحه فقط connection مرتبط با همان تماس را پایان دهید.

کارهایی که سرور انجام می‌دهد:

1. connection تماس را از گروه SignalR حذف می‌کند.
2. رویداد `callEnded` را به سایر شرکت‌کنندگان می‌فرستد.
3. برای تماس رزرو، اگر `callEndDate` خالی باشد آن را با زمان فعلی ثبت می‌کند.
4. اگر تماس جلسهٔ آنلاین باشد، قطعهٔ زمانی‌ای که هر دو طرف واقعاً وصل بوده‌اند به مدت تماس اضافه می‌شود.

خروج ناگهانی مرورگر، قطع اینترنت یا `connection.stop()` هم در `OnDisconnectedAsync` همین مسیر پایان را اجرا می‌کند؛ با این حال دکمهٔ پایان باید همیشه ابتدا `EndCall` را invoke کند تا طرف مقابل بی‌درنگ رویداد پایان بگیرد.

### پیاده‌سازی پیشنهادی دکمه

```ts
async function endCallByAgent() {
  if (ending) return;
  ending = true;

  try {
    // به hub فرصت دهید طرف مقابل را مطلع و زمان پایان را ثبت کند.
    await connection.invoke("EndCall");
  } catch {
    // در قطع شبکه، stop کردن connection نیز مسیر پایان سرور را فعال می‌کند؛
    // UI نباید در حالت تماس گیر کند.
  } finally {
    localStream?.getTracks().forEach(track => track.stop());
    peerConnection?.close();
    await connection.stop().catch(() => undefined);
    setCallState("ended");
  }
}
```

در handler رویداد `callEnded` دوباره `EndCall` صدا نزنید؛ فقط media و UI را ببندید. این کار از loop و پیام پایان تکراری جلوگیری می‌کند.

### اثر روی وضعیت رزرو

پس از پایان تماس رزرو، برای refresh جزئیات از یکی از endpointهای زیر استفاده کنید:

```http
GET /api/Companion/CompanionReserve/{reserveId}
GET /api/Operator/CompanionReserve/{reserveId}
```

در پاسخ اپراتور، `callStartDate` و `callEndDate` برمی‌گردد. `callEndDate` به معنای پایان media call است، نه کامل‌شدن خدمت یا پایان کل رزرو؛ وضعیت اجرای خدمت همچنان از `operatorState` و وضعیت مالی از `state` خوانده می‌شود.

### رفتار چند tab / چند connection

مدیریت تماس در سرور connection-based و درون‌حافظه‌ای است. با پایان یا disconnect هر شرکت‌کننده، `callEnded` برای بقیه ارسال و برای رزرو `callEndDate` ثبت می‌شود. بنابراین یک تماس واحد را در چند tab هم‌زمان باز نکنید و بعد از پایان، همهٔ UIهای مرتبط را با `callEnded` ببندید.

## ۳) تماس جلسهٔ آنلاین مستقل (`OnlineSession`)

نماینده یا اپراتور می‌تواند جلسهٔ آنلاین مستقل را با API زیر شروع کند:

```http
POST /api/OnlineSession
Authorization: Bearer <token>
Content-Type: application/json

{ "targetUserId": 42, "channelId": 2 }
```

نوع‌های channel:

| `channelId` | نوع |
|---:|---|
| 1 | Chat |
| 2 | تماس صوتی درون‌برنامه‌ای |
| 3 | تماس تصویری درون‌برنامه‌ای |
| 4 | تماس تلفنی معمولی |

برای channelهای ۲ و ۳، بعد از دریافت `onlineSessionId` از پاسخ، به همان hub متصل شوید:

```ts
await connection.invoke("JoinSessionCall", onlineSessionId);
```

پایان تماس در این مدل نیز دقیقاً `connection.invoke("EndCall")` است. تفاوت‌ها:

- hub با کلید داخلی `-onlineSessionId` کار می‌کند؛ کلاینت همچنان فقط `EndCall` بدون پارامتر می‌فرستد.
- `callStartDate`، `callEndDate` و `callSeconds` برای گزارش تماس ثبت می‌شوند.
- `OnlineSession.endDate` با فشار دکمهٔ پایان تماس پر نمی‌شود؛ پس کاربر می‌تواند در پنجرهٔ زمانی معتبر دوباره تماس بگیرد.
- برای مشاورهٔ مدت‌دار، رسیدن به `expireDate` به هر دو طرف `callEnded` می‌فرستد و تماس جدید را رد می‌کند.

تماس تلفنی معمولی (`channelId=4`) WebRTC/hub ندارد. پایان تماس در اختیار تلفن/سیستم‌عامل است و API فعلی event یا زمان پایان قابل ثبت برای آن ندارد.

## ۴) چتِ مرتبط با رزرو

### endpointهای اصلی

| کاربرد | endpoint |
|---|---|
| دریافت پیام‌ها | `GET /api/CompanionReserveMessage?companionReserveId={id}&pageIndex=1&pageSize=30&sortBy=Old` |
| ارسال پیام | `POST /api/CompanionReserveMessage` |
| ثبت تحویل یک پیام | `PUT /api/CompanionReserveMessageDelivered` |
| ثبت خوانده‌شدن تا یک پیام | `PUT /api/CompanionReserveMessageRead` |

نمونه ارسال متن:

```json
{
  "companionReserveId": 981,
  "senderUserId": 71,
  "companionReserveMessageTypeId": 140,
  "content": "سلام، برای تماس آماده هستم."
}
```

نوع‌های پیام:

| شناسه | نوع |
|---:|---|
| 140 | Text |
| 141 | Image |
| 142 | Voice |
| 143 | System — فرانت اجازهٔ ارسال ندارد |

چت فقط وقتی سرور اجازهٔ ارسال می‌دهد که رزرو لغو نشده باشد، روش ارتباط آنلاین واقعاً «چت» باشد و کاربر شرکت‌کنندهٔ مجاز باشد. شرکت‌کننده‌ها همان کاربر رزروکننده، مالک مرکز و اپراتور تخصیص‌یافته‌اند.

### دکمهٔ «پایان چت» چه کاری می‌کند؟

در پیاده‌سازی فعلی، هیچ‌یک از endpointهای `CompanionReserveMessage` فیلد یا actionی برای بستن گفتگو ندارد. بنابراین button فعلی باید با نام دقیق‌تر زیر نمایش داده شود:

```text
خروج از گفتگو
```

رفتار درست آن در فرانت:

1. اگر پیام‌های طرف مقابل دیده شده‌اند، `PUT /api/CompanionReserveMessageRead` را ارسال کنید.
2. polling / subscription صفحه را متوقف کنید.
3. route چت را ببندید و به جزئیات رزرو برگردید.
4. پیام‌ها را حذف نکنید و هیچ درخواست ساختگی مانند `EndChat` نفرستید.

نمونهٔ ثبت خوانده‌شدن:

```http
PUT /api/CompanionReserveMessageRead
Authorization: Bearer <token>
Content-Type: application/json

{ "companionReserveId": 981, "lastMessageId": 5012 }
```

این کار فقط وضعیت seen پیام‌ها را ثبت می‌کند؛ گفتگو را نمی‌بندد.

### نکتهٔ محصولی مهم

اگر منظور از «پایان چت» این است که هر دو طرف دیگر نتوانند پیام تازه بفرستند و یک زمان پایان دائمی ثبت شود، این قابلیت **در API فعلی وجود ندارد**. به UI-only close اتکا نکنید، چون طرف مقابل همچنان می‌تواند پیام ارسال کند.

برای چنین قابلیت محصولی، backend باید endpoint و وضعیت مستقل (مثلاً `chatEndDate` و دلیل/actor پایان) اضافه کند و `InsertAsyncDto` پیام را پس از آن رد کند. این مستند endpoint فرضی را endpoint موجود معرفی نمی‌کند.

## ۵) چت جلسهٔ آنلاین مستقل

برای session مستقل، APIهای زیر وجود دارند:

```http
GET  /api/OnlineSession/{id}
GET  /api/OnlineSession/Active
GET  /api/OnlineSessionMessage?onlineSessionId={id}&pageSize=30
POST /api/OnlineSessionMessage
PUT  /api/OnlineSessionMessage/Read
```

در این مدل نیز برای کاربر، مالک/اپراتور شروع‌کننده و طرف مقابل session، endpoint دستی پایان چت وجود ندارد:

- `endDate` فقط در پایان خودکار مشاورهٔ مدت‌دار ثبت می‌شود.
- بعد از `expireDate`، سرور ارسال پیام را رد می‌کند؛ صرفاً مخفی‌کردن composer در فرانت کافی نیست.
- برای session آزاد، خروج از صفحه فقط یک خروج UI است.

نمونهٔ پایان UI:

```ts
async function leaveChat() {
  await markReadIfNeeded();
  stopMessagePolling();
  navigateToReserveOrSessions();
}
```

## ۶) چک‌لیست فرانت

- [ ] برای تماس درون‌برنامه‌ای، connection را به `/hubs/call` با access token بسازید.
- [ ] ورود به تماس رزرو با `JoinCall(reserveId)` و ورود تماس session با `JoinSessionCall(sessionId)` انجام شود.
- [ ] دکمهٔ پایان تماس فقط یک بار `EndCall` را روی همان connection invoke کند.
- [ ] رویداد `callEnded` در هر دو طرف tracks و `RTCPeerConnection` را ببندد.
- [ ] بعد از پایان تماس رزرو، جزئیات رزرو refresh شود تا `callEndDate` نمایش داده شود.
- [ ] دکمهٔ چت فعلی «خروج از گفتگو» نام داشته باشد، نه «پایان چت»، مگر backend قابلیت بستن دائمی را اضافه کند.
- [ ] خروج از چت، history را پاک یا delete نکند؛ در صورت لزوم فقط `Read` ثبت شود.
- [ ] تماس تلفنی عادی را با تماس WebRTC اشتباه نگیرید؛ `EndCall` برای `tel:` کاربرد ندارد.
