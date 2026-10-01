# پاستیل لایو (مدرسه): پخش زنده‌ی یک‌به‌چند + ذخیره + کامنت زنده + حضور/غیاب — مستند کامل (بک‌اند)

**درخواست اصلی (خلاصه):** برای دوره‌های آنلاین «لایو پاستیل» مدرسه (در کنار Skype و Adobe Connect که خودِ مربی لینکش را می‌دهد)، یک پخش زنده‌ی بومی مثل اینستاگرام لایو: مربی پخش می‌کند، کاربران فقط تماشا می‌کنند و کامنت می‌گذارند، مربی کامنت‌ها را همان لحظه می‌بیند و جواب می‌دهد، قابلیت ذخیره‌ی لایو (با نام/توضیحات که مربی موقع شروع مشخص می‌کند) و نمایش دوباره در «دوره‌های من» کاربر، نوتیف ۵ دقیقه مانده + نوتیف شروع با دکمه‌ی «ورود به جلسه»، و مربی در حین لایو مشخصات پت‌های حاضر/غایب دوره را ببیند.

---

## ۰. تصمیم معماریِ گرفته‌شده (مهم، قبل از هر چیز بخوانید)

سیستم سیگنالینگ تماس تصویریِ فعلی پروژه (`CallHub.cs`) فقط **یک‌به‌یک، P2P خالص** است — هیچ جریان تصویری از سرور رد نمی‌شود، پس نه یک‌به‌چند جواب می‌دهد، نه قابل ضبط است. برای «پاستیل لایو» (یک‌به‌چند + ضبط سمت سرور) به یک **سرور رسانه (SFU)** نیاز است. با هماهنگیِ مستقیم، **LiveKit Cloud** انتخاب شد (سرویس ابری آماده، SDK وب/فلاتر، ضبط سمت سرور با Egress API، بدون نیاز به زیرساخت خودمان).

**نکته‌ی صداقت فنی:** RPCهای LiveKit (`RoomService`/`Egress`) با HTTP خام (Twirp JSON) پیاده شده‌اند، بدون افزودن SDK رسمی LiveKit به پروژه (که نصبش در این نشست قابل تست نبود). نام دقیق فیلدهای JSON مطابق مستندات رسمی LiveKit در زمان نوشتن این کد است — **قبل از اتصال کلیدهای واقعی، حتماً یک تماس آزمایشی واقعی (مثلاً `CreateRoom` روی یک روم تستی) بزنید** تا مطمئن شوید نسخه‌ی فعلی LiveKit Cloud همین قرارداد را می‌پذیرد. اگر فرق داشت، فقط `LiveKitService.cs` نیاز به اصلاح دارد؛ بقیه‌ی سیستم (دیتابیس، Hub، Endpointها) تحت تأثیر نیست.

---

## ۱. مدل داده

### `SchoolCourseLiveSession` (جدید)
یک ردیف برای هر جلسه‌ی زنده (`SchoolCourseSession`) — دقیقاً همان جلسه‌هایی که از قبل برای دوره‌های Live با تاریخ/ساعت دقیق تعریف می‌شدند (کار قبلی همین اسپرینت). فیلد یکتا `SchoolCourseSessionId` (هر جلسه حداکثر یک لایو دارد).

| فیلد | توضیح |
|---|---|
| `RoomName` | نام اتاق LiveKit، یکتا و مستقل از Id داخلی |
| `StatusId` | `SchoolLiveStatusEnum`: NotStarted=۱, Live=۲, Ended=۳ |
| `WantsRecording` / `RecordingName` / `RecordingDescription` | انتخاب مربی **همان لحظه‌ی شروع** (نه قبلش) |
| `RecordingStatusId` | `SchoolLiveRecordingStatusEnum`: None=۰, Requested=۱, Processing=۲, Ready=۳, Failed=۴ |
| `EgressId` | شناسه‌ی ضبط سمت LiveKit، برای تطبیق وب‌هوک |
| `SchoolCourseVideoId` | بعد از تمام‌شدن ضبط، لینک به ردیف ساخته‌شده در `SchoolCourseVideo` |

### `SchoolLiveComment` (جدید)
کامنت‌های زنده، بعد از پایان لایو هم نگه داشته می‌شوند (تاریخچه قابل مرور).

### `SchoolCourseSession.ReminderPushSentDate` (فیلد جدید)
جدا از `StartingPushSentDate` قبلی (که سرِ لحظه‌ی شروع می‌رود) — این یکی برای یادآوریِ «۵ دقیقه مانده».

**نکته‌ی کلیدی طراحی:** برای «ذخیره و نمایش دوباره در دوره‌های کاربر»، به‌جای ساختن یک سیستم پخش موازی جدید، از **همان `SchoolCourseVideo`** که از قبل برای دوره‌های Video وجود داشت استفاده شد — یعنی وقتی ضبط لایو آماده شود، یک ردیف `SchoolCourseVideo` جدید برای همان دوره ساخته می‌شود و **خودکار** در همان لیست ویدیوهای دوره (که پنل/وب‌اپ از قبل نمایشش می‌دادند) ظاهر می‌شود — بدون نیاز به هیچ UI جدید برای «مرور دوباره».

---

## ۲. فلوی کامل

### مربی: قبل از شروع
`GET /api/Companion/SchoolCourseLive/session/{schoolCourseSessionId}` — وضعیت فعلی لایوِ این جلسه (اگر نبوده، با `StatusId=NotStarted` ساخته می‌شود).

### مربی: شروع لایو
`POST /api/Companion/SchoolCourseLive/start`
```jsonc
{ "schoolCourseSessionId": 501, "wantsRecording": true, "recordingName": "تربیت پایه سگ - جلسه ۳", "recordingDescription": "..." }
```
- اگر `wantsRecording=true` و `recordingName` خالی باشد → خطا `SchoolLiveRecordingNameRequired`.
- اتاق LiveKit ساخته می‌شود؛ اگر ذخیره خواسته شده، Egress شروع می‌شود.
- پاسخ: `{ liveSessionId, roomName, token, statusId }` — `token` را مستقیم به SDK کلاینت LiveKit بدهید (`canPublish=true, canSubscribe=false` — مربی فقط پخش می‌کند، بیننده‌ها را نمی‌بیند/نمی‌شنود، طبق درخواست).
- Idempotent: اگر لایو از قبل Live بود، همان روم را با توکن تازه برمی‌گرداند (دابل‌کلیک مربی مشکلی ایجاد نمی‌کند).

### مربی: پایان لایو
`POST /api/Companion/SchoolCourseLive/end/{liveSessionId}` — Egress متوقف می‌شود (اگر ضبط فعال بود)، اتاق حذف می‌شود، `StatusId=Ended`.

### کاربر: نوتیف‌ها (خودکار، بدون اقدام مربی)
- **۵ دقیقه مانده**: `PushSchoolClassReminder5Min` (Job جدید، هر دقیقه چک می‌کند).
- **لحظه‌ی شروع**: همان مکانیزم قبلی همین اسپرینت (`PushSchoolClassStarting`) — بدون تغییر، فقط تأیید می‌شود که با این فیچر هماهنگ است. متن پوش شامل دکمه‌ی «ورود به جلسه» با لینک `/schoolSession/{sessionId}` است (از قبل پیاده بود).

### کاربر: ورود به لایو
`GET /api/EndUser/SchoolCourseLive/{liveSessionId}/token`
- چک می‌شود کاربر واقعاً در این دوره ثبت‌نام کرده (`SchoolReserve` فعال).
- توکن `canPublish=false, canSubscribe=true` — فقط تماشا، دقیقاً طبق درخواست («نیازی نیست کاربرها بتوانند حرف بزنند یا ویدیو داشته باشند»).
- اگر مربی هنوز نزده Start، `statusId=NotStarted` برمی‌گردد — صفحه باید «منتظر شروع مربی» نشان دهد و Poll کند یا صبر کند تا Hub خبر بدهد.

### حین لایو: کامنت
از طریق SignalR Hub (`/hubs/schoolLive`)، نه REST — چون باید بلادرنگ باشد:
- `JoinAsViewer(liveSessionId, userPetId)` — بعد از این، هم عضو گروه SignalR می‌شود هم در ردیاب حضور ثبت می‌شود.
- `PostComment(liveSessionId, message)` — هم مربی هم بیننده‌ها می‌توانند بفرستند؛ نتیجه با رویداد `newComment` به کل گروه (شامل مربی) می‌رود.
- تاریخچه‌ی کامنت‌ها (برای لود اولیه‌ی صفحه): `GET /api/{Companion|EndUser}/SchoolCourseLive/{liveSessionId}/comments`.

### حین لایو: حضور/غیاب پت‌ها (فقط مربی)
`GET /api/Companion/SchoolCourseLive/{liveSessionId}/participants` — همه‌ی پت‌های ثبت‌نامیِ فعالِ دوره (نه فقط کسانی که وصل‌اند)، هرکدام با `present: true/false`. `present` از یک ردیاب درون‌حافظه‌ای (`SchoolLiveParticipantTracker`، دقیقاً مثل `CallSessionTracker` موجود) می‌آید که با `JoinAsViewer`/قطع‌اتصال SignalR به‌روز می‌شود. هر آیتم شامل نام/عکس/نوع/نژاد پت و نام/موبایل صاحبش — یعنی مربی می‌تواند از همین‌جا «پروفایل پت» را ببیند، بدون نیاز به Endpoint جدا.

### پایان ضبط (غیرهمزمان، وب‌هوک)
LiveKit وقتی Egress کامل شد، به `POST /api/webhook/livekit-egress` خبر می‌دهد (باید در پنل LiveKit Cloud ثبت شود). امضا با هدر `Authorization` (JWT با `WebhookApiSecret`) چک می‌شود، نه احراز هویت معمول — این Endpoint در `AnonymousExposureTests` allowlist ثبت شده (بررسی‌شده و عمدی است، نه سهو). موفق → `File` + `SchoolCourseVideo` ساخته می‌شود، `RecordingStatusId=Ready`. ناموفق → `RecordingStatusId=Failed`.

---

## ۳. تنظیمات لازم قبل از استقرار (عملیات/DevOps)

در `appsettings.json` یا Environment Variables، این کلیدهای زیر بخش `LiveKit` باید مقداردهی شوند (فقط نام کلیدها، بدون هیچ مقداری — طبق قانون پروژه، مقادیر هرگز در مستندات/کد نوشته نمی‌شوند):

- `LiveKit:Host` — آدرس پروژه روی LiveKit Cloud
- `LiveKit:ApiKey` / `LiveKit:ApiSecret` — از داشبورد LiveKit Cloud
- `LiveKit:WebhookApiKey` / `LiveKit:WebhookApiSecret` — برای اعتبارسنجی وب‌هوک Egress
- `LiveKit:RecordingBucket` / `LiveKit:RecordingRegion` / `LiveKit:RecordingAccessKey` / `LiveKit:RecordingSecret` — سطل S3-سازگار مقصد ضبط
- `LiveKit:RecordingPublicBaseUrl` — آدرس عمومی برای دسترسی به فایل‌های ضبط‌شده

هیچ مقداری در کد هاردکد نشده (طبق قانون پروژه). این کلیدها را از داشبورد LiveKit Cloud + سطل S3-سازگاری که برای ضبط انتخاب می‌کنید بگیرید، و در تنظیمات پروژه‌ی LiveKit Cloud آدرس وب‌هوک بالا را ثبت کنید.

---

## ۴. وضعیت فنی

- بیلد کامل `Pastil.sln` بدون خطا؛ `dotnet test Application.Tests`: **۵۶۲/۵۶۲ سبز**.
- در همین بررسی، یک باگ نهفته‌ی preexisting هم پیدا و رفع شد: `PushTripDriverUpcomingReminder` (اضافه‌شده در کار قبلیِ همین روز) هیچ‌وقت در `Resource/Pattern.resx`/`.fa.resx` seed نشده بود — تست امنیتیِ خودکار پروژه (`PersianPushTextHelperTests`) همین را گرفت؛ رفع شد.
- دو مایگریشن جدید (فقط ساخته‌شده، **اعمال‌نشده**):
  - `SchoolLivePastilLive` — جدول‌های جدید + یک ستون جدید، بدون تغییر داده‌ی موجود.
  - `SeedSchoolClassReminderPushType` — فقط seed نوع پوش جدید (idempotent).
- تست امنیتی `AnonymousExposureTests` (که هر Endpoint نوشتنیِ بدون احراز هویت را می‌گیرد) به‌روزرسانی شد تا وب‌هوک جدید را با دلیل مشخص بپذیرد.

## ۵. جمع‌بندی چک‌لیست درخواست کاربر

| درخواست | وضعیت |
|---|---|
| مربی لایو شود، کاربرها فقط تماشا + کامنت | ✅ توکن‌های canPublish نامتقارن |
| مربی کامنت‌ها را همان لحظه ببیند و پاسخ دهد | ✅ SignalR Hub دوطرفه |
| قابلیت ذخیره + نام/جلسه‌چندم/توضیحات موقع شروع | ✅ مودال شروع (`wantsRecording`+`recordingName`+`recordingDescription`) |
| نمایش دوباره در پروفایل/دوره‌های کاربر | ✅ با reuse از `SchoolCourseVideo` موجود |
| نوتیف ۵ دقیقه مانده | ✅ Job جدید |
| نوتیف شروع + دکمه‌ی ورود مستقیم | ✅ مکانیزم قبلی همین اسپرینت، تأییدشده |
| مربی حضور/غیاب پت‌ها را ببیند + پروفایل پت | ✅ `GetParticipantsAsync` |

## ۶. کارهای باقی‌مانده (خارج از بک‌اند)
تمام بخش پخش/تماشای واقعیِ تصویر (اتصال SDK جاوااسکریپت LiveKit، رندر ویدیو، UI مودال شروع لایو برای مربی، صفحه‌ی تماشای کاربر با کامنت زنده، پلیر بازپخش) باید در `webapp/` پیاده شود — مستند کامل: `webapp/docs/SCHOOL_LIVE_PASTIL_LIVE_ARMAN_FA.md`.
