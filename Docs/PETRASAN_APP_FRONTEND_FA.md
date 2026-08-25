# پت‌رسان — مستند پیاده‌سازی سمت App (webapp)

این مستند برای تیم فرانت (Nuxt 4 — `webapp/`) نوشته شده تا بخش کاربر (صاحب پت) و بخش راننده‌ی فیچر **پت‌رسان** رو پیاده‌سازی کنن. بک‌اند این فیچر کامل و build-clean هست؛ این سند قرارداد API رو دقیق و کامل توضیح می‌ده.

> بک‌اند مرتبط: `Application/Services/TripSrv/TripSrv/*`, کنترلرهای `Api/Areas/{EndUser,Driver}/Controllers/Trip*`. برای معماری کلی و تصمیم‌های طراحی، به سند «معماری پت‌رسان» (Artifact این پروژه) مراجعه کن.

---

## ۱. دو حالت فیچر

| | حالت یک — لحظه‌ای (مثل اسنپ) | حالت دو — متصل به رزرو |
|---|---|---|
| کاربر چطور شروع می‌کنه | باز کردن نقشه، انتخاب مبدا/مقصد، جستجوی راننده | از داخل یک رزرو کلینیک/همراه‌دار، انتخاب «راننده هم بیاد» |
| راننده کِی مشخص می‌شه | بلافاصله (کاربر گزینه رو انتخاب می‌کنه، سیستم پیشنهاد می‌ده) | خودکار، ۱ یا ۲ ساعت قبل از موعد رزرو (Hangfire job) |
| Endpoint شروع | `POST /EndUser/TripCurrent` | `POST /EndUser/TripReservation` |

هر دو حالت روی **همون Entity Trip** پیاده شدن — تفاوت فقط در `CompanionReserveId`/`ScheduledLeadMinutes`/`ScheduledDepartureAt` پر بودن یا نبودنشونه.

---

## ۲. قراردادهای عمومی

- **Base URL**: طبق `docs/ai/API_MAP.md` — سرویس Api روی پورت `5000`.
- **همه‌ی مسیرها**: `POST/GET/PUT https://{host}/api/{Area}/{Controller}[/{id}]`
  - Area برای کاربر (صاحب پت): `EndUser`
  - Area برای راننده: `Driver`
- **احراز هویت**: هدر `Authorization: Bearer {token}` روی همه‌ی این endpoint ها اجباریه (`[Authorize]`).
- **پاسخ همیشه HTTP 200** با پوشش زیر (هیچ‌وقت از HTTP status برای تشخیص موفقیت استفاده نکن، همیشه `isSuccess` رو چک کن):

```json
{
  "isSuccess": true,
  "code": 200,
  "messages": [],
  "data": { }
}
```

فیلدهای پاسخ camelCase هستن (`isSuccess`, `data`, ...) چون سریالایزیشن پیش‌فرض ASP.NET Core camelCase هست.

- **خطاها**: وقتی `isSuccess: false` باشه، متن خطا (فارسی، قابل‌نمایش مستقیم به کاربر) توی `messages` یا `data`-adjacent فیلد پیام میاد — دقیقاً مثل بقیه‌ی فرم‌های اپ.

---

## ۳. Enum های کلیدی

### `TripStatusId` (وضعیت کلی سفر — Code-backed، از `GET /EndUser/Code?groupLabel=Trip_Status` یا دیکشنری کد قابل خوندنه)

| Label | مقدار (id واقعی از DB میاد، این‌ها اسم‌ها هستن) | معنی |
|---|---|---|
| `TripStatus_Requested` | — | سفر ثبت شده، منتظر پذیرفتن راننده |
| `TripStatus_Accepted` | — | راننده پذیرفته، سفر «فعال»ه — فقط تو این حالت پولینگ لوکیشن زنده معنی داره |
| `TripStatus_Canceled` | — | لغو شده (کاربر یا راننده یا سیستم) |
| `TripStatus_Compeleted` | — | تمام شده |

> ⚠️ **مقدار عددی این‌ها رو هاردکد نکن** — همیشه از endpoint دیکشنری کد (`Code`/`CodeGroup`) بخون، دقیقاً مثل بقیه‌ی enum های Code-backed پروژه (`ProductStatus`, `OrderStatus` و ...). گروه‌شون `Trip_Status` و `Driver_Status` هست.

### `ProgressStageId` (مرحله‌ی ریز پیشرفت — **فقط یک عدد int ساده، Code-backed نیست**، فقط وقتی `TripStatusId == Accepted`معنی داره)

| مقدار | معنی |
|---|---|
| `0` | هنوز شروع نشده (None) |
| `1` | راننده در مسیر مبدا (EnRouteOrigin) |
| `2` | راننده به مبدا رسید (ArrivedOrigin) |
| `3` | پت تحویل گرفته شد، سفر آغاز شد (PetPickedUp) |
| `4` | به مقصد رسید (ArrivedDestination) |

این عدد رو مستقیم توی `TripDto`/`TripVDto`/`TripLiveDto` می‌گیری، نیازی به دیکشنری کد نیست.

**نکته‌ی رفت‌وبرگشت (`RoundTrip`)**: اگه سفر رفت‌وبرگشت باشه، وقتی راننده به مقصد می‌رسه (مرحله ۴)، اگه هنوز مسیر برگشت شروع نشده باشه (`isReturnLeg == false`)، بک‌اند خودش مبدا/مقصد رو جابه‌جا می‌کنه و `ProgressStageId` رو دوباره به `1` برمی‌گردونه و `IsReturnLeg = true` می‌کنه. یعنی UI باید به `isReturnLeg` هم نگاه کنه، نه فقط `progressStageId`، تا بفهمه توی کدوم لگ از سفره.

---

## ۴. حالت یک — سفر لحظه‌ای

### ۴.۱ پیش‌نمایش قیمت (قبل از ثبت نهایی)

```
POST /EndUser/PriceCalculation
```
Body: یک `TripDto` نیمه‌پر (حداقل `Origin`, `Destination`, گزینه‌های انتخابی) → پاسخ شامل قیمت محاسبه‌شده. این یه endpoint موجود از قبل بود، تغییری نکرده.

### ۴.۲ ثبت سفر

```
POST /EndUser/TripCurrent
```

Body (`TripDto` — فقط فیلدهای مرتبط با پت‌رسان رو لیست می‌کنم، بقیه‌ی فیلدهای عمومی سفر قبلاً بوده):

```jsonc
{
  "origin": { "x": 51.389, "y": 35.6892 },       // x=lng, y=lat
  "destination": { "x": 51.41, "y": 35.70 },
  "fromAddress": "string",
  "toAddress": "string",
  "roundTrip": false,
  "userPetIds": [12, 15],                          // ← جدید: چند پت در یک سفر
  "tripOptionIds": [1, 2],                          // گزینه‌های سفر (قبلاً بوده)
  "userDetail": "توضیح کاربر (اختیاری)"
  // userId توسط بک‌اند از توکن پر می‌شه، لازم نیست بفرستی
}
```

> `userPetIds` جایگزین فیلد قدیمی `userPetId` شده. فیلد قدیمی هنوز برای سازگاری عقب‌رو پر می‌شه (اولین پت لیست) ولی **در UI جدید همیشه از `userPetIds` استفاده کن**.

پاسخ: `BaseResultDto<TripVDto>` — شامل `id` سفر تازه‌ساخته‌شده، که برای مراحل بعدی لازمشه.

پت‌ها رو از `GET /EndUser/UserPet` (endpoint موجود) بگیر و به کاربر چندانتخابی نشون بده.

### ۴.۳ گرفتن سفر جاری / پولینگ وضعیت کلی

```
GET /EndUser/TripCurrent
```

اگه سفر فعالی وجود نداشته باشه `data: null` برمی‌گرده. این رو هر چند ثانیه (مثلاً هر ۵ ثانیه، قبل از پذیرفته‌شدن توسط راننده) صدا بزن تا بفهمی راننده پذیرفته یا نه (`tripStatusId` عوض میشه).

### ۴.۴ موقعیت زنده‌ی راننده (فقط وقتی سفر `Accepted` شد)

```
GET /EndUser/TripLive/{tripId}
```

پاسخ (`TripLiveDto`):

```jsonc
{
  "tripStatusId": 76,               // Accepted
  "progressStageId": 2,             // ArrivedOrigin
  "isReturnLeg": false,
  "counterpartLocation": { "x": 51.395, "y": 35.691 },   // آخرین موقعیت شناخته‌شده‌ی راننده
  "counterpartLocationUpdatedAt": "2026-08-24T10:15:03"
}
```

- **Polling interval پیشنهادی: هر ۳ تا ۵ ثانیه** (طبق تصمیم پروژه — نه SignalR، چون WebSocket روی سرور فعلی مشکل ۵۰۴ داره).
- اگه سفر `Accepted` نباشه، این endpoint خطای «سفر در حال حاضر فعال نیست» برمی‌گردونه — یعنی UI باید فقط وقتی وارد صفحه‌ی «سفر فعال» شدی این رو صدا بزنی، نه همیشه.
- `counterpartLocation` ممکنه `null` باشه اگه راننده هنوز هیچ لوکیشنی نفرستاده — این حالت رو توی UI هندل کن (مثلاً «در حال دریافت موقعیت راننده...»).

### ۴.۵ نمایش مراحل پیشرفت

همون `GET /EndUser/TripCurrent` یا `GET /EndUser/TripLive/{id}` مقدار `progressStageId`/`isReturnLeg` رو می‌ده — طبق جدول بخش ۳ روی UI (مثلاً یک stepper ۴ مرحله‌ای) نمایش بده. **کاربر خودش این مرحله رو تغییر نمی‌ده — فقط راننده.**

### ۴.۶ گزینه‌ها، کد تخفیف، کیف‌پول (همه از قبل بودن، بدون تغییر)

```
PUT /EndUser/TripSetRebate      body: { id, rebateCode }
PUT /EndUser/TripRemoveRebate?id={tripId}     (بدون body — id از query string)
PUT /EndUser/TripSetWallet      body: { id, fromWallet: true|false }
```

### ۴.۷ پرداخت

```
POST /EndUser/TripPayment
```
همون فلوی پرداخت موجود (درگاه)، بدون تغییر.

### ۴.۸ لغو توسط کاربر

```
PUT /EndUser/UpdateTripUserStatus
```
Body: `{ id: tripId, tripStatusId: <Canceled> }`

⚠️ **قانون جدید**: اگه `progressStageId >= 3` (یعنی پت از قبل تحویل گرفته شده — `PetPickedUp` یا `ArrivedDestination`)، این درخواست با خطای فارسی «پت تحویل گرفته شده و سفر دیگر از این مسیر قابل لغو نیست.» رد می‌شه. یعنی **دکمه‌ی «لغو سفر» رو توی UI بعد از این مرحله غیرفعال/مخفی کن** تا کاربر تجربه‌ی بد نبینه.

### ۴.۹ پایان سفر و امتیازدهی

بعد از `TripStatus_Compeleted`، فلوی امتیازدهی/نظر (فیلدهای `userRate`/`userComment` روی همون `Trip`) از قبل موجوده — تغییری نکرده.

---

## ۵. حالت دو — سفر متصل به رزرو

### ۵.۱ پیش‌نیاز: کاربر باید از قبل یک رزرو (Companion) داشته باشه

از `GET /EndUser/CompanionReserve` (endpoint موجود) رزروهای آینده‌ی کاربر رو بگیر و توی صفحه‌ی رزرو، گزینه‌ی «راننده هم بیاد» رو نشون بده.

### ۵.۲ ثبت سفر رزروی

```
POST /EndUser/TripReservation
```

Body (`TripReservationCreateDto`):

```jsonc
{
  "companionReserveId": 4021,
  "origin": { "x": 51.389, "y": 35.6892 },
  "destination": { "x": 51.41, "y": 35.70 },
  "fromAddress": "string",
  "toAddress": "string",
  "scheduledLeadMinutes": 60,        // فقط 60 یا 120 مجازه — گزینه‌ی رادیویی به کاربر بده، نه ورودی آزاد
  "ownerRidesAlong": true,
  "userPetIds": [12]
}
```

خطاهای رایج که ممکنه برگرده (پیام‌های فارسی مستقیم قابل‌نمایش):
- فاصله‌ی زمانی غیر از ۶۰/۱۲۰ → «فاصله‌ی زمانی حرکت راننده فقط می‌تواند ۶۰ یا ۱۲۰ دقیقه باشد.»
- رزرو پیدا نشد یا مال این کاربر نیست → پیام «یافت نشد»
- زمان محاسبه‌شده‌ی حرکت (`DoDate - leadMinutes`) از الان گذشته باشه → رد می‌شه (رزرو خیلی نزدیکه، دیگه نمی‌شه راننده رزرو کرد)
- برای همون رزرو قبلاً یه سفر پت‌رسان ثبت شده → رد می‌شه (هر رزرو فقط یک سفر متصل می‌تونه داشته باشه)

پاسخ موفق: `TripDto` تازه‌ساخته‌شده با `tripStatusId = Requested` و `driverId = null` — **راننده هنوز مشخص نیست**.

### ۵.۳ چی بعدش اتفاق می‌افته (فقط جهت اطلاع، کاری از UI لازم نیست)

یک Job زمان‌بندی‌شده (`DispatchScheduledTrips`، هر ۱ دقیقه) دقیقاً در لحظه‌ی `scheduledDepartureAt` نزدیک‌ترین راننده‌ی فعال رو خودکار به این سفر اختصاص می‌ده. کاربر یه پوش می‌گیره وقتی این اتفاق افتاد (یا بعدش، وقتی راننده مراحل رو طی می‌کنه).

### ۵.۴ پولینگ وضعیت

همون `GET /EndUser/TripCurrent` — قبل از dispatch، `driverId` هنوز `null`ه؛ بعد از dispatch، `driverId` پر می‌شه و از همون‌جا سفر دقیقاً مثل حالت یک ادامه پیدا می‌کنه (پذیرش ضمنیه، مستقیم می‌ره سمت مراحل پیشرفت).

---

## ۶. سمت راننده

### ۶.۱ دیدن سفر پیشنهادی / سفر جاری

```
GET /Driver/TripCurrent
```

### ۶.۲ پذیرش/رد سفر

```
PUT /Driver/UpdateTripDriverStatus
```
Body: `{ id: tripId, driverStatusId: <Accepted|Rejected> }` — `driverId` رو خودت نفرست، بک‌اند از توکن راننده پر می‌کنه.

وقتی راننده قبول می‌کنه، بک‌اند خودکار `progressStageId` رو `1` (EnRouteOrigin) می‌کنه.

### ۶.۳ اعلام مراحل (هر سه‌تا فقط `PUT`، بدون body، `id` توی route)

```
PUT /Driver/TripArrivedOrigin/{tripId}      → به مبدا رسیدم
PUT /Driver/TripPetPickedUp/{tripId}         → پت رو تحویل گرفتم
PUT /Driver/TripArrivedDestination/{tripId}  → به مقصد رسیدم
```

قوانین مهم که باید توی UI رعایت بشه (بک‌اند هم اجباری می‌کنه، ولی بهتره دکمه‌ها رو هم UI-side غیرفعال کنی تا خطای بی‌مورد نگیره):
- **ترتیب اجباریه** — نمی‌شه مرحله رو رد کرد یا برگردوند عقب. اگه `progressStageId` فعلی `1` باشه فقط `TripArrivedOrigin` مجازه، بعدش فقط `TripPetPickedUp`، بعدش فقط `TripArrivedDestination`.
- سفر باید `TripStatusId == Accepted` باشه، وگرنه خطای «مرحله‌ی درخواستی با توالی مجاز سفر همخوانی ندارد.» می‌گیری.
- بعد از هر مرحله، پوش خودکار برای کاربر می‌ره (نیازی به کاری از UI نیست).
- روی `TripArrivedDestination` برای سفر رفت‌وبرگشت: اگه هنوز لگ برگشت شروع نشده، همون درخواست باعث می‌شه `progressStageId` دوباره `1` بشه (رفتن به لگ برگشت) — یعنی دکمه‌ی بعدی که باید فعال بشه دوباره «به مبدا رسیدم» می‌شه، نه «پایان سفر». به `isReturnLeg` نگاه کن تا بفهمی این آخرین‌باره یا لگ برگشته.

### ۶.۴ موقعیت زنده‌ی کاربر (برای نمایش روی نقشه‌ی راننده)

```
GET /Driver/TripLive/{tripId}
```

دقیقاً همون شکل `TripLiveDto`ی بخش ۴.۴ ولی `counterpartLocation` اینجا موقعیت **کاربره**، نه راننده.

---

## ۷. گزارش موقعیت زنده (هم کاربر هم راننده)

یک endpoint مشترک — **چه کاربر باشی چه راننده، از همین یکی استفاده می‌کنی** (چون لوکیشن راننده روی `User.Id` خودِ راننده (نه `Driver.Id`) ذخیره می‌شه):

```
POST /EndUser/UserCurrentLocation
```
Body: `{ "location": { "x": 51.395, "y": 35.691 } }`

- این endpoint هیچ ربطی به سفر نداره — فقط «آخرین موقعیت این کاربر» رو ست/آپدیت می‌کنه. حتی توی صفحه‌ی راننده (که Area توکنش `Driver`ه)، همین مسیر `EndUser/UserCurrentLocation` رو صدا بزن، چون توکن همیشه به یک `User` واحد وصله.
- **باید در طول کل سفر فعال (`Accepted`) هر ۳ تا ۵ ثانیه صدا زده بشه** — هم از اپ کاربر (اگه `ownerRidesAlong` یا برای نمایش به راننده لازمه) و هم از اپ راننده. وقتی سفر فعال نیست، لازم نیست پیوسته بفرستی (میشه کمتر/فقط periodic عادی).
- این همون جدولیه که پنل ادمین هم برای نقشه‌ی زنده‌ش استفاده می‌کنه — یعنی اگه این رو درست پیاده کنی، همزمان توی پنل ادمین هم دیده می‌شه، تست دوطرفه‌ی ساده‌ای داری.

---

## ۸. پوش نوتیفیکیشن‌ها (جهت اطلاع — سمت بک‌اند خودکار ارسال می‌شه)

این‌ها رو UI لازم نیست خودش trigger کنه، فقط بدونه چه پوشی کِی میاد تا اگه لازم بود روی کلیک پوش، deep-link مناسب بزنه:

| رویداد | گیرنده | متن |
|---|---|---|
| راننده به مبدا رسید | کاربر | «راننده به مبدا رسید» |
| پت تحویل گرفته شد | کاربر | «پت شما تحویل گرفته شد، سفر آغاز شد» |
| راننده به مقصد رسید | کاربر | «پت شما به مقصد رسید» |
| سفر تکمیل شد | کاربر | «سفر پت‌رسان تکمیل شد — لطفا به راننده امتیاز دهید» |
| سفر لغو شد | کاربر/راننده | «سفر پت‌رسان لغو شد» |
| تایید/رد احراز هویت راننده | راننده | (مرتبط با فلوی ثبت‌نام راننده، نه سفر) |

---

## ۹. جدول کامل Endpoint ها (مرجع سریع)

| Method | Route | Area | توضیح |
|---|---|---|---|
| POST | `/EndUser/PriceCalculation` | EndUser | پیش‌نمایش قیمت |
| POST | `/EndUser/TripCurrent` | EndUser | ثبت سفر لحظه‌ای (حالت ۱) |
| GET | `/EndUser/TripCurrent` | EndUser | سفر جاری کاربر |
| GET | `/EndUser/TripLive/{id}` | EndUser | موقعیت زنده‌ی راننده + مرحله |
| PUT | `/EndUser/TripSetRebate` | EndUser | اعمال کد تخفیف |
| PUT | `/EndUser/TripRemoveRebate?id={id}` | EndUser | حذف کد تخفیف |
| PUT | `/EndUser/TripSetWallet` | EndUser | پرداخت از کیف‌پول |
| POST | `/EndUser/TripPayment` | EndUser | شروع پرداخت |
| PUT | `/EndUser/UpdateTripUserStatus` | EndUser | لغو سفر توسط کاربر |
| POST | `/EndUser/TripReservation` | EndUser | ثبت سفر متصل به رزرو (حالت ۲) |
| POST | `/EndUser/UserCurrentLocation` | EndUser | گزارش موقعیت زنده (کاربر یا راننده) |
| GET | `/EndUser/UserPet` | EndUser | لیست پت‌های کاربر (برای انتخاب چندتایی) |
| GET | `/EndUser/CompanionReserve` | EndUser | لیست رزروهای کاربر (برای حالت ۲) |
| GET | `/Driver/TripCurrent` | Driver | سفر جاری/پیشنهادی راننده |
| PUT | `/Driver/UpdateTripDriverStatus` | Driver | پذیرش/رد سفر |
| PUT | `/Driver/TripArrivedOrigin/{id}` | Driver | اعلام رسیدن به مبدا |
| PUT | `/Driver/TripPetPickedUp/{id}` | Driver | اعلام تحویل پت |
| PUT | `/Driver/TripArrivedDestination/{id}` | Driver | اعلام رسیدن به مقصد |
| GET | `/Driver/TripLive/{id}` | Driver | موقعیت زنده‌ی کاربر + مرحله |

---

## ۱۰. نکات پیاده‌سازی و UX

1. **Polling، نه SignalR** — روی سرور فعلی WebSocket مشکل ۵۰۴ داره؛ همه‌ی لحظه‌به‌لحظه‌ها با فاصله‌ی ۳-۵ ثانیه پیاده بشن (`setInterval` + پاک‌کردنش روی unmount کامپوننت — حواست به memory leak باشه).
2. **پولینگ رو فقط وقتی صفحه‌ی مرتبط بازه انجام بده** — مثلاً `TripLive` رو فقط توی صفحه‌ی «سفر فعال» صدا بزن، نه همیشه در background، هم برای مصرف باتری هم برای فشار روی سرور.
3. **`PointDto`**: همیشه `{ x: longitude, y: latitude }` — دقیقاً برعکس ترتیب معمول `lat,lng`. توی همه‌ی endpoint های پت‌رسان همینه.
4. **دکمه‌های مرحله‌ی راننده رو state-machine مانند پیاده کن** — بر اساس `progressStageId` فقط دکمه‌ی مرحله‌ی بعدی فعال باشه، بقیه غیرفعال/مخفی.
5. **چندپتی بودن سفر** (`userPetIds`) یعنی UI باید یک چندانتخابی پت داشته باشه، نه یک select تکی مثل قبل.
6. **دکمه‌ی لغو سفر** بعد از `progressStageId >= 3` باید مخفی/غیرفعال بشه (بک‌اند رد می‌کنه، ولی بهتره UX از قبل جلوش رو بگیره).
7. برای حالت ۲، فیلد `scheduledLeadMinutes` **باید یک انتخاب محدود (۱ ساعت / ۲ ساعت)** باشه، نه input آزاد عددی — چون بک‌اند فقط این دو مقدار رو قبول می‌کنه.
