# قرارداد کامل سرویس پت‌رسان؛ از درخواست کاربر تا نمایش در پنل راننده

**مبنای بررسی:** کد بک‌اند در commit `d15ef9e1923b5d754c45afb30600a91cee6c68b3`، ۲۰۲۶/۱۰/۱۲. این سند قرارداد کد است؛ وضعیت دیتابیس و پاسخ محیط مستقر باید با همان محیط سنجیده شوند.

## ۱. جواب مستقیم به مشکل «سرویس ثبت می‌شود ولی برای راننده نمی‌آید»

`PetResanService` **برنامهٔ هفتگی** کاربر است، نه درخواست سفر قابل‌قبول توسط راننده. راننده endpoint فهرست خودِ سرویس‌ها ندارد. بک‌اند برای نزدیک‌ترین نوبت واجدشرایط هر ردیف برنامه، `Trip` واقعی می‌سازد؛ این سفر تا پیش از پذیرش در `GET /api/Driver/TripAvailable` است. شناسهٔ تشخیص سرویس در `TripVDto` برابر `petResanServiceScheduleId != null` است. فیلد `type: "petresanservice"` در DTO سفر وجود ندارد. در جستجوی **سفرهای منتسب به راننده**، پارامتر متفاوتی به نام `tripKind=service` وجود دارد؛ این پارامتر فیلتر `TripAvailable` نیست.

**زمان نسخهٔ فعلی:** هنگام `POST` سرویس، سرور همان موقع تلاش می‌کند نزدیک‌ترین نوبت هر ردیف را بسازد: تاریخ از `startDate` مؤثر به بعد، حداکثر در ۱۵ روز آینده، و ساعت حرکت **بیش از ۶۰ دقیقه بعد از زمان جاری سرور**. اگر ردیفی Trip آینده داشته باشد، دوباره ساخته نمی‌شود. پس سرویس شنبه می‌تواند از چند روز قبل در پیشنهادهای راننده باشد. job تکراری شبانهٔ ساخت نوبت حذف شده است؛ یک اجرای پشتیبان هنگام راه‌اندازی سرور ردیف‌های بی‌نوبت آینده را پر می‌کند. بعد از تکمیل یا لغو هر نوبت، نوبت هم‌جهت هفتهٔ بعد ساخته می‌شود. `POST` می‌تواند موفق شود ولی ساخت Trip در `try/catch` خطا بدهد؛ برای تشخیص قطعی، وجود `Trip` را بررسی کنید.

**پرداخت نسخهٔ فعلی:** تا وقتی در همان سرویس هیچ Trip پرداخت‌شده‌ای وجود نداشته باشد، نوبت با `isPaid=false` و وضعیت `75` قابل نمایش است؛ راننده می‌پذیرد و سپس کاربر پرداخت می‌کند. پس از اولین پرداخت، برای نوبت‌های بعدی کسر کیف پول امتحان می‌شود. **اگر کسر ناموفق باشد نوبت لغو نمی‌شود**: `fromWallet=false` و `walletPrice=0` می‌شود و سفر پرداخت‌نشده در پیشنهاد راننده می‌ماند تا کاربر بعد از پذیرش پرداخت کند. `isPaid=false` معیار پنهان‌کردن سفر در پنل راننده نیست.

## ۲. نقشهٔ شناسه‌ها و جریان داده

```text
EndUser POST /PetResanService
    └─ PetResanService.id (کل برنامه)
       └─ PetResanServiceSchedule.id (روز + ساعت)
          └─ هنگام ثبت سرویس / پس از پایان نوبت قبلی، Trip.id (نوبت اجرایی)
             ├─ petResanServiceScheduleId = schedule.id
             ├─ tripStatusId = 75, driverId = null, isOnline = true
             └─ Driver GET /TripAvailable → قبول راننده → tripStatusId = 76
                                             └─ Driver GET /TripUpcoming
```

`serviceId`، `scheduleId` و `tripId` سه شناسهٔ متفاوت‌اند. برای قبول سفر باید `tripId` را در فیلد `id` بفرستید، نه شناسهٔ سرویس. `GET /api/Driver/Trip/{id}` فقط سفر **منتسب به همان راننده** را مجاز می‌داند؛ برای سفر هنوز آزاد، خودِ ردیف `TripAvailable` را بخوانید.

| نوع سفر در پاسخ `TripVDto` | شرط تشخیص |
|---|---|
| نوبت سرویس هفتگی | `petResanServiceScheduleId != null` |
| سفر متصل به رزرو مرکز، پانسیون یا مدرسه | به‌ترتیب `companionReserveId`، `pansionReserveId` یا `schoolReserveId != null` |
| رزرو مستقل سفر | `scheduledDepartureAt != null` و بدون شناسهٔ سرویس |
| سفر فوری | هیچ‌یک از شناسه‌های بالا/زمان رزرو مستقل را ندارد |

نوبت سرویس `scheduledDepartureAt` را پر نمی‌کند؛ ساعت حرکتش `tripStartDateTime` است. `isReturnLeg=true` یعنی نوبت برگشتِ جداگانه یا مرحلهٔ برگشت در همان Trip رفت‌وبرگشت. تاریخ‌ها در این مسیر با `DateTime.Now` سرور ساخته می‌شوند؛ اگر پاسخ بدون offset است، timezone محیط مستقر را با تیم بک‌اند هماهنگ کنید و تبدیل UTC حدسی انجام ندهید.

## ۳. API کاربر برای ساخت و مدیریت برنامه

همهٔ مسیرهای `EndUser/PetResanService` با `[Authorize]` محافظت شده‌اند. بدنه‌ها JSON و پاسخ‌های عملیاتی معمولاً `BaseResultDto` با `isSuccess`، `messages`، `data` هستند. HTTP 200 به‌تنهایی به معنای موفقیت نیست.

| متد و مسیر | کار | ورودی | `data` |
|---|---|---|---|
| `POST /api/EndUser/PetResanService/preview-price` | پیش‌نمایش قیمت | `PetResanServiceCreateDto`؛ rate limit با کلید `TripPrice` | عدد قیمت |
| `POST /api/EndUser/PetResanService` | ثبت برنامه | همان DTO؛ هدر اختیاری `Idempotency-Key` | `PetResanServiceVDto` |
| `GET /api/EndUser/PetResanService` | برنامه‌های کاربر | بدون بدنه | لیست `PetResanServiceVDto` |
| `GET /api/EndUser/PetResanService/{id}` | جزئیات برنامهٔ همان کاربر | شناسهٔ سرویس | `PetResanServiceVDto` |
| `PUT /api/EndUser/PetResanService/{id}/cancel` | غیرفعال‌کردن برنامه | شناسهٔ سرویس | نتیجهٔ بدون داده |
| `GET /api/WeekDay` | کاتالوگ روزها | بدون احراز هویت | فهرست روزها، مرتب بر اساس `number` |
| `GET /api/EndUser/TripOption` | کاتالوگ گزینه‌های سفر | احراز هویت | جستجوی گزینه‌ها |

### ورودی ثبت و پیش‌نمایش: `PetResanServiceCreateDto`

| فیلد JSON | نوع و قاعده |
|---|---|
| `userPetIds` | آرایهٔ شناسهٔ پت‌ها؛ اگر غیرخالی باشد بر `userPetId` اولویت دارد. تکراری و پت غیرمتعلق به کاربر رد می‌شود. |
| `userPetId` | شناسهٔ تک‌پت قدیمی؛ وقتی `userPetIds` خالی است استفاده می‌شود. برای ثبت حداقل یک پت لازم است. |
| `origin`, `destination` | نقطهٔ `{ "x": longitude, "y": latitude }`؛ هر دو برای ثبت و preview لازم‌اند. |
| `fromAddress`, `toAddress` | متن آدرس مبدا و مقصد. |
| `totalWeeks` | عدد صحیح مثبت یا `null`؛ خالی یعنی برنامه تا لغو کاربر باز می‌ماند. `endDate` از تاریخ شروع به‌اضافهٔ `7 × totalWeeks` روز ساخته می‌شود. |
| `startDate` | تاریخ شروع اختیاری؛ اگر خالی یا گذشته باشد امروزِ سرور مبناست. اگر آینده باشد، انتخاب نوبت از همان تاریخ شروع می‌شود. |
| `roundTrip` | `bool` با پیش‌فرض `true` در DTO. **نکتهٔ ناسازگاری فعلی:** این مقدار در پیش‌نمایش قیمتِ ردیف بدون `returnTime` اثر دارد، ولی در ساخت Trip آن ردیف، کد `roundTrip: true` را ثابت می‌فرستد. برای `false` بدون ساعت برگشت، روی یکسان‌بودن پیش‌نمایش و سفر واقعی حساب نکنید. |
| `tripOptionIds` | آرایهٔ شناسهٔ گزینه‌های فعال و حذف‌نشده؛ تکراری/نامعتبر رد می‌شود. قیمت و گزینه‌ها به Trip هر نوبت منتقل می‌شوند. |
| `schedules` | آرایهٔ حداقل یک ردیف `{ weekDayId, time, returnTime? }`. ترکیب `(weekDayId,time)` نباید تکراری باشد. |

`time` و `returnTime` باید در قالب دقیق `HH:mm` باشند. `returnTime` اختیاری است و اگر پر شود، باید **بعد از** `time` در همان روز باشد. شناسهٔ روز را از `GET /api/WeekDay` بردارید؛ عدد روز در تولید نوبت شنبه=۱ تا جمعه=۷ است. نسخهٔ فعلی وجود همهٔ `weekDayId`های ارسالی را پیش از ذخیره بررسی می‌کند.

نمونهٔ بدنه:

```json
{
  "userPetIds": [41, 42],
  "origin": { "x": 51.389, "y": 35.689 },
  "destination": { "x": 51.423, "y": 35.732 },
  "fromAddress": "خانه",
  "toAddress": "مهد",
  "startDate": "2026-10-17",
  "totalWeeks": 4,
  "tripOptionIds": [],
  "schedules": [
    { "weekDayId": 1, "time": "08:30", "returnTime": "16:30" },
    { "weekDayId": 3, "time": "08:30" }
  ]
}
```

`Idempotency-Key` اگر ارسال شود باید UUID غیرصفر در قالب canonical `D` باشد. تکرار همان کلید و همان درخواست، سرویس قبلی را برمی‌گرداند؛ کلید یکسان با محتوای متفاوت خطا می‌دهد. **نکتهٔ کد:** مقایسهٔ درخواست تکراری، `roundTrip` و `startDate` را بررسی نمی‌کند؛ برای تغییر معنی درخواست از کلید جدید استفاده کنید. اگر هدر فرستاده نشود، محافظ تکرار این سطح وجود ندارد.

**خروجی سرویس:** `id`, `userPetId`, `userPetName`, `userPets[{userPetId,name}]`, `origin`, `destination`, `fromAddress`, `toAddress`, `startDate`, `totalWeeks`, `endDate`, `active`, `pricePerOccurrence`, `pricePerWeek`, `tripOptions[]`, `schedules[{id,weekDayId,weekDayName,weekDayNumber,time,returnTime}]`. لیست کاربر حتی برنامهٔ لغوشده را برمی‌گرداند؛ `active` وضعیت آن را مشخص می‌کند. لغو برنامه فقط `active=false` و `cancelDate` را ذخیره می‌کند؛ Tripهای ساخته‌شدهٔ قبلی را خودکار لغو نمی‌کند.

**قیمت:** `preview-price` در پیاده‌سازی فعلی **جمع قیمت ردیف‌های یکتای هفته** را برمی‌گرداند، نه الزاماً قیمت یک نوبت؛ با `returnTime` قیمت رفت و برگشتِ دو Trip جدا جمع می‌شود. هنگام ثبت، `pricePerOccurrence` از تقسیم این جمع بر تعداد ردیف‌های هفتگی به دست می‌آید؛ `pricePerWeek` مقدار نمایشی حاصل از میانگین و شمار ردیف‌های فعال است. مبلغ قطعی هر Trip موقع تولید دوباره برای ساعت همان حرکت محاسبه می‌شود. برای دریافت مبلغ واقعی به `Trip.price` رجوع کنید.

## ۴. از برنامه تا Trip واقعی

1. `POST`، برنامه و scheduleها را فعال ذخیره می‌کند. ثبت برنامه به‌تنهایی مدرک ساخته‌شدن Trip نیست.
2. کنترلر ثبت بلافاصله `GeneratePetResanServiceTripsForNewServiceAsync(serviceId)` را صدا می‌زند. برای هر schedule فعال، اگر Trip آینده ندارد، نزدیک‌ترین روز مطابق هفته از `startDate` مؤثر در بازهٔ حداکثر ۱۵ روزه انتخاب می‌شود؛ ساعت آن باید بیش از ۶۰ دقیقه تا حرکت فاصله داشته باشد و از `endDate` نگذرد. `Api/Program.cs` job قدیمی `GeneratePetResanServiceTrips` را حذف کرده و یک بار job پشتیبانِ ساخت نوبت‌های مفقود را enqueue می‌کند.
3. بدون `returnTime`: یک Trip با `roundTrip=true` ساخته می‌شود؛ برگشتِ مستقل در ساعت جدا ندارد. با `returnTime`: دو Trip یک‌طرفه ساخته می‌شود؛ Trip دوم مبدأ/مقصد و آدرس‌ها را جابه‌جا دارد و `isReturnLeg=true` است.
4. برای هر leg، پیش از ساخت، وجود Trip با `(petResanServiceScheduleId, tripStartDateTime)` چک می‌شود؛ ایندکس یکتا هم جلوی تکرار هم‌زمان را می‌گیرد. اگر قیمت‌گذاری exception بدهد یا قیمت `<=0` شود، آن leg اصلاً ساخته نمی‌شود و خطا در لاگ ثبت می‌شود.
5. Trip ایجادشده `isOnline=true`، `tripStatusId=75`، `driverStatusId=79`، `driverId=null`، `petResanServiceScheduleId=schedule.id`، `tripStartDateTime=زمان نوبت` و `userPetId` برابر اولین پت دارد. `tripPets` همهٔ پت‌های انتخابی را نگه می‌دارد. `vehicleTypeId` در مسیر تولید مقداردهی نمی‌شود و `null` می‌ماند.
6. اگر در **همان سرویس** قبلاً هیچ Trip با `isPaid=true` وجود ندارد، Trip فعلی بدون کسر کیف پول باقی می‌ماند و برای انتخاب راننده Notice ساخته می‌شود. تا اولین پرداخت، هر نوبت جدید نیز همین شاخه را طی می‌کند؛ با `returnTime` ممکن است هر دو leg اول پرداخت‌نشده باشند.
7. اگر Trip پرداخت‌شدهٔ قبلی وجود دارد، مبلغ همان leg از کیف پول کسر می‌شود. موفقیت → `isPaid=true`, `fromWallet=true`, `walletPrice=price`. شکست → Trip همچنان `tripStatusId=75`, `isPaid=false`, `fromWallet=false`, `walletPrice=0` می‌ماند؛ پوش شارژ کیف پول و Notice کمبود موجودی ثبت می‌شود، اما سفر برای راننده قابل پذیرش است.
8. بعد از ساخت، تعداد Tripهای جدید آزاد محاسبه و پوش تجمیعی `PushTripServiceOccurrencesAvailable` با شناسهٔ **۱۱۳** و `token1=تعداد` برای مالکان راننده‌های `!deleted && active && approved` فرستاده می‌شود. نبود پوش معیار نبود Trip نیست. job قدیمی یادآوری شارژ کیف پول از Hangfire حذف شده است؛ پوش **۱۱۸** هنوز در مسیر شکست کسر کیف پول، حداکثر یک بار در روز، استفاده می‌شود.
9. پس از تکمیل یا لغو نوبت سرویس، همان جهتِ schedule برای هفتهٔ بعد ساخته می‌شود؛ اگر زمان هفتهٔ بعد گذشته/خیلی نزدیک باشد، هفت روز دیگر جلو می‌رود. لغو کل برنامه، برنامه را غیرفعال می‌کند اما Tripهای ازقبل ساخته‌شده را خودکار لغو نمی‌کند.

## ۵. API پنل راننده و شرط دقیق نمایش

| متد و مسیر | کاربرد |
|---|---|
| `GET /api/Driver/TripAvailable` | همهٔ Tripهای آزاد قابل مرور/قبول برای همین راننده؛ جای نمایش نوبت سرویس **پیش از قبول** |
| `PUT /api/Driver/UpdateTripDriverStatus` | قبول یا رد یک Trip؛ بدنه `{ "id": tripId, "driverStatusId": 80 }` برای قبول و `81` برای رد. `driverId` از توکن راننده گرفته و مقدار ارسالی کلاینت بازنویسی می‌شود. |
| `GET /api/Driver/TripUpcoming` | Tripهای **قبول‌شدهٔ همین راننده** با `tripStartDateTime > now` |
| `GET /api/Driver/TripCurrent` | سفر جاریِ منتسب به راننده؛ فهرست درخواست‌های آزاد نیست |
| `GET /api/Driver/Trip` | جستجوی سفرها با `driverId` از توکن؛ فهرست آزاد نیست |
| `GET /api/Driver/Trip/{id}` | جزئیات Trip منتسب به راننده با **`TripDto`**؛ برای Trip آزاد `AccessDenied`. این DTO برخلاف `TripVDto` فیلد `petResanServiceScheduleId` و آرایهٔ آبجکت `userPets` ندارد. |
| `PUT /api/Driver/TripArrivedOrigin/{id}` | اعلام رسیدن به مبدأ |
| `PUT /api/Driver/TripPetPickedUp/{id}` | اعلام تحویل گرفتن پت |
| `PUT /api/Driver/TripArrivedDestination/{id}` | اعلام رسیدن به مقصد؛ برای Trip رفت‌وبرگشتِ قدیمی، مسیر برگشت همان Trip شروع می‌شود |
| `PUT /api/Driver/TripDriverCancel` | لغو Trip پذیرفته‌شده با `{ "id", "cancelReasonCodeId", "cancelDetail" }` |
| `GET /api/Driver/Code?codeGroupLabel=TripCancelReason_Driver` | گزینه‌های معتبر دلیل لغو راننده |
| `GET /api/Driver/TripLive/{id}` | آخرین موقعیت کاربر و مرحلهٔ سفر در سفر پذیرفته‌شده |
| `POST /api/EndUser/UserCurrentLocation` | ارسال موقعیت زندهٔ راننده با `{"location":{"x":51.4,"y":35.7}}` |

اگر توکن `DriverId <= 0` داشته باشد، `TripAvailable` و `TripUpcoming` با HTTP 200 و `isSuccess=false`/`AccessDenied` برمی‌گردند. شرط SQL فهرست آزاد در کد:

```text
tripStatusId == 75
AND driverId == null
AND isOnline == true
AND (vehicleTypeId == null OR vehicleTypeId == نوع خودروی راننده)
AND برای این (tripId,driverId) رکورد TripDriverExclusion وجود ندارد
```

**شرط پیش از کوئری:** اگر راننده Trip دیگری با `tripStatusId=76` و زمان حرکت `<= الان + ۱ ساعت` دارد، پاسخ `TripAvailable` موفق ولی **آرایهٔ خالی** است. این گارد در پذیرش هم اعمال می‌شود. خودِ کوئری `TripAvailable` فیلتر زمان حرکت، فاصلهٔ جغرافیایی، `isPaid`، `active/approved` راننده یا فیلتر «فقط سفر فوری» ندارد. برای نوبت سرویس با `vehicleTypeId=null`، ناسازگاری نوع خودرو هم مانع نیست. `active/approved` در **گیرندگان پوش** چک می‌شود، نه در خود کوئری فهرست؛ وضعیت حساب راننده/توکن را جدا بررسی کنید.

پس از قبول موفق، آپدیت اتمیک `driverId=راننده`, `driverStatusId=80`, `tripStatusId=76`, `progressStageId=1` را ثبت می‌کند. تنها اولین پذیرنده موفق است. رد با `driverStatusId=81` سفر را برای بقیه آزاد نگه می‌دارد اما همان راننده را در `TripDriverExclusions` می‌گذارد؛ بعد از آن در لیست خودش نمی‌بیند. Trip قبول‌شده از `TripAvailable` خارج و تا قبل از زمان حرکت در `TripUpcoming` حاضر می‌شود. `TripCurrent` فقط یک Trip جاری را برمی‌گرداند و برای «پیشنهادها» مناسب نیست.

### فیلدهای لازم برای کارت راننده (`TripVDto`)

| فیلد | استفاده |
|---|---|
| `id` | شناسهٔ واقعی Trip برای پذیرش و مراحل اجرا |
| `petResanServiceScheduleId` | برچسب «سرویس هفتگی»؛ شناسهٔ schedule، نه service |
| `tripStartDateTime` | زمان حرکت نوبت در زمان محلی سرور؛ `scheduledDepartureAt` اینجا `null` است |
| `origin`, `destination`, `fromAddress`, `toAddress` | مسیر؛ برای برگشتِ مستقل از قبل جابه‌جا شده‌اند |
| `roundTrip`, `isReturnLeg` | یک Trip رفت‌وبرگشت یا leg برگشت مستقل |
| `tripStatusId`, `driverStatusId`, `driverId` | وضعیت و مالکیت؛ ۷۵/۷۹ پیش از قبول و ۷۶/۸۰ پس از آن |
| `isPaid`, `fromWallet`, `walletPrice`, `price`, `paymentPrice` | وضعیت و مبلغ پرداخت؛ اولین نوبت می‌تواند `isPaid=false` باشد |
| `userPetId`, `userPet`, `userPets[]`, `tripOptions[]` | پت‌ها و گزینه‌های سفر |
| `progressStageId`, `companionReserveId`, `pansionReserveId`, `schoolReserveId`, `vehicleTypeId` | مرحله، تمایز از سفرهای رزرو متصل و نوع خودرو |

`TripVDto` آبجکت کاملِ برنامهٔ مادر، روزهای هفتگی یا نام مهد/مرکز را به راننده نمی‌دهد. فقط شناسهٔ schedule در Trip هست. فیلد اختصاصی `type: "petresanservice"` در این DTO وجود ندارد. واحد پول در فیلد `price` مشخص نشده است؛ قالب تومان/ریال را از قرارداد مالی همان محیط بگیرید، نه از نوع `double` این DTO.

### فهرست دقیق فیلدهای پاسخ سفر و نمایش پیشنهادی در پنل

مبنای کارت‌های **پیشنهاد** و **پیش‌رو**، `TripVDto` است. همهٔ فیلدهای این DTO در قرارداد JSON تعریف شده‌اند، ولی مقادیر رابطه‌ای بسته به `Include`های هر endpoint می‌توانند `null` یا خالی باشند. فیلدهای با برچسب «داخلی» را صرفاً برای منطق/عیب‌یابی نگه دارید و روی کارت به کاربر نمایش ندهید.

| گروه | فیلدهای `TripVDto` | استفاده در پنل راننده |
|---|---|---|
| شناسه و نوع | `id`, `petResanServiceScheduleId`, `companionReserveId`, `pansionReserveId`, `schoolReserveId`, `scheduledDepartureAt`, `previousTripId` | `id` برای همهٔ actionها؛ `petResanServiceScheduleId != null` برچسب «سرویس هفتگی»؛ سه شناسهٔ رزرو، اتصال به رزرو مرکز/پانسیون/مدرسه؛ `previousTripId` ارتباط سفر جایگزین/قبلی است و در کارت اصلی لازم نیست. |
| زمان | `createDate`, `tripStartDateTime`, `scheduledLeadMinutes`, `progressUpdateDate` | زمان حرکت سرویس از `tripStartDateTime`؛ `createDate` زمان ثبت سفر است. `scheduledLeadMinutes` برای رزرو مستقل است و در نوبت سرویس معمولاً `null`. `progressUpdateDate` زمان تغییر مرحله/وضعیت است. |
| مسیر | `origin`, `destination`, `secondDestination`, `fromAddress`, `toAddress`, `routeLength`, `fromCityId`, `fromCity`, `tripStop`, `stopTime`, `stopMinutes` | نمایش مبدا/مقصد، نقشه و آدرس. نقطه `{x: طول جغرافیایی, y: عرض جغرافیایی}` است. `secondDestination`، توقف و طول مسیر در سفر هفتگی تولیدشده معمولاً مقدار مفید ندارند؛ فقط اگر پاسخ مقدار معتبر داشت نمایش دهید. |
| جهت و همراهی | `roundTrip`, `isReturnLeg`, `ownerRidesAlong` | تشخیص یک سفر رفت‌وبرگشت یا سفر برگشت مستقل؛ نوبت هفتگی تولیدشده `ownerRidesAlong=false` دارد. |
| پت و مشتری | `userPetId`, `userPet`, `userPets[]`, `userId`, `user`, `passengerId`, `userDetail` | نام/گونه/تعداد پت‌ها از `userPets[]`؛ برای سازگاری `userPet` نخستین پت است. `userDetail` توضیح کاربر برای سفر. `user`/`userPet.user` ممکن است بسته به endpoint کامل نباشد؛ نمایش نام و تماس فقط از دادهٔ واقعاً حاضر در پاسخ. |
| گزینه‌ها و خودرو | `tripOptions[]`, `vehicleTypeId`, `vehicleType` | خدمات افزودهٔ انتخابی؛ هر گزینهٔ `TripOptionVDto` شامل `id`, `name`, `price`, `active` است. نوبت سرویس در کد تولید `vehicleTypeId=null` دارد. |
| وضعیت | `tripStatusId`, `tripStatus`, `driverStatusId`, `driverStatus`, `driverId`, `driver`, `progressStageId`, `isOnline` | وضعیت سفر، پاسخ راننده و دکمهٔ مرحلهٔ بعد؛ در پیشنهاد آزاد `driverId=null`. برچسب‌های `tripStatus`/`driverStatus` کاتالوگی‌اند و ممکن است در برخی پاسخ‌ها بارگذاری نشده باشند؛ اعداد را نیز نگه دارید. |
| پرداخت | `price`, `paymentPrice`, `discount`, `rebateId`, `rebatePrice`, `rebate`, `isPaid`, `fromWallet`, `walletPrice`, `manualPayDate`, `driverShare`, `siteShare`, `wallet` | مبلغ قابل نمایش را از `price`/`paymentPrice` سفر بگیرید، نه `pricePerWeek` سرویس. `isPaid` وضعیت پرداخت است؛ `driverShare`/`siteShare` اطلاعات تسویه‌اند و در کارت پیشنهاد لازم نیست. `manualPayDate` فقط پس از ثبت دستی معنا دارد. |
| لغو | `cancelInitiatorId`, `cancelReasonCodeId`, `cancelReasonCode`, `cancelReasonDetail` | در سابقهٔ سفر لغوشده، عامل و دلیل را اگر موجود بود نمایش دهید. |
| ارزیابی | `userComment`, `userRate` | نظر/امتیاز پس از سفر؛ در کارت پیشنهاد معمولاً خالی/صفر است. |
| داخلی/انتقال | `connectionId`, `userToken` | اطلاعات فنی، برای UI سفر نمایش داده نشود. |

**ساختار پت:** `userPets[]` آرایهٔ `UserPetVDto` است؛ فیلدهای نمایشی رایج شامل `id`, `name`, `petId`, `pet.name`, `petBreedId`, `petBreed2Id`, `isMixBreed`, `size`, `weight`, `picture`, `isMale`, `isSterile` است. فیلدهای `birthday`, `microChipCode`, `specificDisease`, `specificMedicene`, `addressValue` و سوابق نیز در DTO پت وجود دارند؛ این‌ها دادهٔ حساس/تخصصی‌اند و در کارت عمومی پیشنهاد لازم نیستند. برای لیست مختصر، `userPets[]` را با fallback به `userPet` بخوانید. `TripCurrent` در کوئری فعلی `tripPets` را `Include` نمی‌کند، پس کامل بودن `userPets[]` در آن تضمین نشده است.

`TripAvailable` و `TripUpcoming` رابطهٔ `TripPets → UserPet` را بارگذاری می‌کنند، اما برای همهٔ پت‌ها رابطهٔ عمیق‌تر `Pet`، تصویر و سوابق را صریح بارگذاری نمی‌کنند؛ پس نام پت می‌تواند حاضر باشد در حالی که گونه/عکس/جزئیات پزشکی در همان پاسخ خالی است. UI تعداد پت‌ها را از آرایهٔ حاضر محاسبه کند و برای دادهٔ غایب مقدار ساختگی نشان ندهد.

**نکتهٔ مهم جزئیات:** `GET /api/Driver/Trip/{id}` خروجی `TripDto` دارد، نه `TripVDto`. در آن `userPetIds[]` هست، اما `petResanServiceScheduleId` و `userPets[]` نیست. برای حفظ نشان «سرویس هفتگی» در صفحهٔ جزئیات، نوع را از `TripAvailable`، `TripUpcoming` یا `GET /api/Driver/Trip?tripKind=service` در state نگه دارید. اگر صفحهٔ جزئیات بدون state مستقیم باز می‌شود، خودِ پاسخ `/{id}` برای تشخیص قطعی نوع کافی نیست؛ این یک کمبود قرارداد API است.

### صفحه‌ها و دادهٔ پیشنهادی

| صفحه | endpoint منبع | اطلاعات لازم روی کارت/صفحه | عمل |
|---|---|---|---|
| پیشنهادهای قابل قبول | `GET /api/Driver/TripAvailable` سپس در کلاینت `petResanServiceScheduleId != null` | برچسب سرویس هفتگی، `id`، روز/ساعت `tripStartDateTime`، آدرس و مختصات، جهت، تعداد و نام پت، گزینه‌ها، قیمت و نشان پرداخت | قبول `80` یا رد `81` با `PUT /UpdateTripDriverStatus` |
| سفرهای پذیرفته‌شدهٔ آینده | `GET /api/Driver/TripUpcoming` | همان داده‌ها، `driverId`، `tripStatusId=76`، زمان باقی‌مانده؛ مرتب‌سازی سرور بر اساس زمان حرکت | ورود به جزئیات و آمادگی؛ سفر پس از گذشت ساعت از این فهرست بیرون می‌رود |
| سفر جاری | `GET /api/Driver/TripCurrent` و در صورت نیاز `GET /api/Driver/TripLive/{id}` | مرحله، مسیر فعلی، وضعیت پرداخت، آخرین موقعیت طرف مقابل | دکمهٔ دقیق مرحلهٔ بعد؛ فقط در سفر پذیرفته‌شده |
| تاریخچه و فیلتر سرویس | `GET /api/Driver/Trip?tripKind=service` | فیلترهای `tripStatusId`, `isPaid`, `fromDate`, `toDate`, `pageIndex`, `pageSize`؛ پاسخ جستجویی با `list[]` | مشاهدهٔ سفرهای منتسب به همین راننده، نه سفرهای آزاد |
| جزئیات بعد از قبول | `GET /api/Driver/Trip/{id}` | فیلدهای `TripDto` و شناسه‌های پت `userPetIds[]`؛ نوع سرویس را از state قبلی نگه دارید | عملیات بعدی با `id` همان Trip |

`tripKind` در جستجوی سفر سه مقدار شناخته‌شده دارد: `instant`, `reserve`, `service`. مقدار `petresanservice` در switch کد تطبیق ندارد و در نتیجه فیلتر مؤثر نمی‌شود. `TripAvailable` پارامتر `tripKind` ندارد؛ اگر تب «فقط سرویس» می‌خواهید، پاسخ آن را در کلاینت بر اساس شناسهٔ schedule فیلتر کنید.

`TripCurrent` یک رکورد منتسب و آنلاین با وضعیت ۷۵/۷۶ را بدون شرط «زمان حرکت گذشته» و بدون مرتب‌سازی برمی‌گرداند؛ پس برای صفحهٔ «پیش‌رو» از `TripUpcoming` استفاده کنید. پس از گذشت ساعت حرکت، سفر پذیرفته‌شده از `TripUpcoming` بیرون می‌رود، اما ممکن است هنوز در `TripCurrent` و `GET /api/Driver/Trip` باشد.

### نمونهٔ خلاصهٔ سفر در `TripAvailable`

این JSON فقط فیلدهای مهم را نشان می‌دهد؛ پاسخ واقعی ممکن است فیلدهای دیگر جدول بالا را نیز داشته باشد:

```json
{
  "isSuccess": true,
  "data": [
    {
      "id": 9001,
      "petResanServiceScheduleId": 501,
      "tripStartDateTime": "2026-10-17T08:30:00",
      "scheduledDepartureAt": null,
      "tripStatusId": 75,
      "driverStatusId": 79,
      "driverId": null,
      "isOnline": true,
      "isPaid": false,
      "roundTrip": false,
      "isReturnLeg": false,
      "origin": { "x": 51.389, "y": 35.689 },
      "destination": { "x": 51.423, "y": 35.732 },
      "fromAddress": "خانه",
      "toAddress": "مهد",
      "price": 350000,
      "paymentPrice": 350000,
      "userPetId": 41,
      "userPets": [{ "id": 41, "name": "پت نمونه" }],
      "tripOptions": []
    }
  ]
}
```

شناسه‌ها، زمان و مبلغ بالا نمونه‌اند؛ تضمینی برای دادهٔ محیط عملیاتی نیستند. `TripVDto` خودِ `serviceId`، `weekDayName`، `returnTime` یا `pricePerWeek` را ندارد. برای نمایش این‌ها به اتصال/endpoint تازه در بک‌اند نیاز است؛ API پنل راننده فقط `scheduleId` را برمی‌گرداند.

## ۶. پرداخت، اعلان و تخصیص دستی

برای **نوبت پرداخت‌نشده**، بعد از قبول راننده، کاربر از `POST /api/EndUser/TripPayment` با فیلد `tripId` از `PaymentStartDto` پرداخت را شروع می‌کند؛ سرویس پرداخت مالکیت، `isPaid=false` و `tripStatusId=76` را بررسی می‌کند. استفاده از کیف پول با `PUT /api/EndUser/TripSetWallet` و بدنه `{ "id": tripId, "fromWallet": true }` در جریان موجود پرداخت سفر امکان‌پذیر است. callback موفق `isPaid=true` می‌کند. اگر کاربر پرداخت نکند، این کد job مشخصی برای لغو خودکار Trip پذیرفته‌شدهٔ پرداخت‌نشده ندارد.

پوش **۱۱۳** (`PushTripServiceOccurrencesAvailable`) به راننده فقط سیگنال بازخوانی `TripAvailable` است. یادآوری پذیرفته‌شدهٔ سرویس حدود **۶۰ دقیقه** پیش از حرکت با پوش **۱۲۴** برای راننده و **۱۲۵** برای کاربر ارسال می‌شود؛ job مربوط هر دقیقه اجرا می‌شود و ثبت `DriverReminderSentDate` از تکرار جلوگیری می‌کند. اگر اعلان خاموش باشد، اپ هنگام بازشدن صفحه و بازگشت به foreground فهرست را دوباره بگیرد؛ منبع حقیقت پاسخ API است. اگر تا **۱ ساعت پیش از حرکت** سفر هنوز بی‌راننده باشد، job اعزام به ادمین Notice می‌دهد؛ هنگام موعد نیز هشدار نهایی دارد. `PUT /api/Admin/TripChooseDriver` تخصیص دستی جایگزین است، نه مرحلهٔ اجباری مسیر عادی.

### وضعیت‌ها و دکمه‌های راننده

| فیلد | مقدار | معنی برای UI |
|---|---:|---|
| `tripStatusId` | 75 / 76 / 77 / 78 | درخواست‌شده / پذیرفته‌شده / لغوشده / تکمیل‌شده. |
| `driverStatusId` | 79 / 80 / 81 | منتظر راننده / پذیرفتهٔ راننده / رد راننده. |
| `progressStageId` | 0 / 1 / 2 / 3 / 4 | بدون شروع / به‌سمت مبدا / رسیده به مبدا / پت تحویل گرفته شد / رسیده به مقصد. |

پذیرش: `PUT /api/Driver/UpdateTripDriverStatus` با `{"id":9001,"driverStatusId":80}`. تنها اولین راننده‌ای که آپدیت اتمیک را ثبت کند موفق است. پس از موفقیت، `tripStatusId=76`, `driverStatusId=80`, `progressStageId=1` می‌شود. رد: همین endpoint با `driverStatusId=81`؛ Trip برای رانندگان دیگر آزاد می‌ماند اما برای همین راننده با `TripDriverExclusion` پنهان می‌شود. `driverId` بدنه از توکن بازنویسی می‌شود. پاسخ `isSuccess` را بخوانید و پس از قبول/رد فهرست‌ها را تازه کنید.

مراحل اجرا فقط به ترتیب مجازند: `PUT /api/Driver/TripArrivedOrigin/{tripId}` (۱→۲)، `PUT /api/Driver/TripPetPickedUp/{tripId}` (۲→۳)، `PUT /api/Driver/TripArrivedDestination/{tripId}` (۳→۴ و اتمام). برای سفر `roundTrip=true` بدون ساعت برگشت جدا، اولین «رسیدن به مقصد» همان Trip را با `isReturnLeg=true` و مرحلهٔ ۱ برای مسیر برگشت ادامه می‌دهد؛ دومین بار اتمام نهایی است. در این گذار کد **مختصات** مبدأ/مقصد را جابه‌جا می‌کند اما متن `fromAddress`/`toAddress` را جابه‌جا نمی‌کند؛ روی مرحلهٔ برگشتِ همین Trip، آدرس متنی ممکن است با مختصات فعلی ناسازگار باشد. سفرهایی که `returnTime` دارند، دو `Trip` جدا هستند و Trip برگشت از ابتدا آدرس و مختصات معکوس دارد.

لغو پس از پذیرش: `PUT /api/Driver/TripDriverCancel` با `{"id":9001,"cancelReasonCodeId":123,"cancelDetail":"توضیح"}`. کد دلیل باید فعال و در گروه `TripCancelReason_Driver` باشد؛ پس از `progressStageId>=3` راننده نمی‌تواند لغو کند. لغو، `tripStatusId=77` می‌کند و برای سرویس هفتگی ساخت نوبت هفتهٔ بعد را تلاش می‌کند. برای دریافت فهرست دلیل‌ها از `GET /api/Driver/Code?codeGroupLabel=TripCancelReason_Driver` استفاده کنید. `GET /api/Driver/TripLive/{tripId}` فقط در وضعیت ۷۶ پاسخ موفق می‌دهد و `counterpartLocation`, `counterpartLocationUpdatedAt`, `tripStatusId`, `progressStageId`, `isReturnLeg` را برمی‌گرداند.

`PUT /api/Driver/ManualTripPayment` نیز برای Trip منتسب به همان راننده وجود دارد؛ سرویس آن `isPaid=true` و `manualPayDate=DateTime.Now` ثبت می‌کند. این عمل معادل callback درگاه نیست و نباید صرفاً برای عبور از وضعیت پرداخت‌نشده به‌طور خودکار در UI صدا زده شود.

## ۷. تشخیص گام‌به‌گام علت دیده‌نشدن

برای یک درخواست واقعی، **`serviceId`، `scheduleId`، تاریخ/ساعت تهران، `tripId`، `driverId` و پاسخ خام API** را کنار هم بگذارید:

| مشاهده | نتیجه و بررسی بعدی |
|---|---|
| `POST /PetResanService` `isSuccess=false` | برنامه ثبت نشده؛ پیام `messages` و اعتبارسنجی پت، زمان، گزینه‌ها و قیمت را بررسی کنید. |
| برنامه ثبت شده ولی `Trip` آینده نیست | `startDate`، روز انتخابی، `endDate`، ساعت نوبت و شرط بیش از ۶۰ دقیقه فاصله تا حرکت را بررسی کنید. الگوریتم نزدیک‌ترین تاریخ را حداکثر در ۱۵ روز آینده می‌جوید. |
| `POST` موفق است، اما ساخت نوبت شکست خورد | لاگ `Generating first occurrences ... failed` یا خطای قیمت‌گذاری را بررسی کنید؛ کنترلر خطای مرحلهٔ ساخت Trip را به `isSuccess=false` ثبت سرویس تبدیل نمی‌کند. |
| نوبت قبلی تکمیل/لغو شده ولی نوبت هفتهٔ بعد نیست | فعال‌بودن سرویس و schedule، `endDate`، خطای `CreateNextServiceOccurrenceAsync` و وجود Trip آیندهٔ همان schedule را بررسی کنید. |
| تصور می‌شود job شبانه اجرا نشده | در نسخهٔ فعلی job شبانهٔ ساخت و یادآوری کیف پول از `Api/Program.cs` حذف شده‌اند؛ ساخت در ثبت سرویس و پس از تکمیل/لغو و یک اجرای پشتیبان هنگام startup انجام می‌شود. |
| کیف پول نوبت بعدی کمبود موجودی داشت | Trip نباید فقط به این علت `77` شود؛ در کد فعلی با `isPaid=false`, `fromWallet=false`, `walletPrice=0`, `tripStatusId=75` برای راننده باز می‌ماند. |
| Trip وضعیت `75` دارد، اما `isOnline=false` یا `driverId` پر است | شرط `TripAvailable` برقرار نیست؛ دادهٔ Trip و مسیر تغییر آن را بررسی کنید. |
| Trip وضعیت `75` و آزاد است، ولی فقط یک راننده نمی‌بیند | `TripDriverExclusions`، نوع خودرو، گارد سفر پذیرفته‌شدهٔ نزدیک، و `DriverId` توکن او را بررسی کنید. |
| API `TripAvailable` شامل Trip است، ولی UI نشان نمی‌دهد | فیلتر اپ، parse `petResanServiceScheduleId`، شرط اشتباه `isPaid=true`، استفادهٔ نادرست از `type=petresanservice` و refresh بعد از پوش/بازگشت به صفحه را بررسی کنید. |
| Trip وضعیت `76` دارد | قبلاً پذیرفته شده؛ در `TripUpcoming` رانندهٔ پذیرنده (تا قبل از حرکت) یا صفحهٔ سفر خودش جستجو کنید، نه `TripAvailable`. |

### نمونهٔ پرس‌وجوی فقط خواندنی برای تیم بک‌اند

این SQL را با `@ServiceId` واقعی و دسترسی مجاز اجرا کنید؛ خروجی دادهٔ تولید و شرط پایهٔ نمایش را نشان می‌دهد. نتیجهٔ یک راننده نیازمند بررسی exclusion و گارد سفر نزدیک هم هست.

```sql
DECLARE @ServiceId bigint = 123;

SELECT s.Id AS ServiceId, s.UserId, s.Active AS ServiceActive, s.StartDate, s.EndDate,
       sch.Id AS ScheduleId, sch.WeekDayId, wd.Number AS WeekDayNumber,
       sch.Time, sch.ReturnTime, sch.Active AS ScheduleActive,
       t.Id AS TripId, t.TripStartDateTime, t.TripStatusId, t.DriverId,
       t.IsOnline, t.IsPaid, t.FromWallet, t.Price, t.IsReturnLeg,
       t.CancelReasonDetail,
       CASE WHEN t.TripStatusId = 75 AND t.DriverId IS NULL AND t.IsOnline = 1
            THEN 1 ELSE 0 END AS PassesBaseAvailableFilter
FROM PetResanServices s
JOIN PetResanServiceSchedules sch ON sch.PetResanServiceId = s.Id
LEFT JOIN WeekDays wd ON wd.Id = sch.WeekDayId
LEFT JOIN Trips t ON t.PetResanServiceScheduleId = sch.Id
WHERE s.Id = @ServiceId
ORDER BY sch.Id, t.TripStartDateTime DESC;
```

اگر API موفق با لیست خالی می‌دهد، همان راننده را با درخواست واقعی `GET /api/Driver/TripAvailable` آزمایش کنید. **پوش نرسیدن** و **Trip در API نبودن** دو عیب جدا هستند. اگر Trip در API هست، مسئله در نمایش/فیلتر پنل راننده است؛ اگر نیست، شروط بخش ۵ یا مراحل ساخت بخش ۴ را دنبال کنید.

## ۸. منابع کد و نکتهٔ نسخه

- [کنترلر سرویس کاربر](../Api/Areas/EndUser/Controllers/PetResanServiceController.cs) و [سرویس ثبت، اعتبارسنجی و قیمت](../Application/Services/TripSrv/PetResanServiceSrv/PetResanServiceService.cs)
- [تعریف ورودی](../Application/Services/TripSrv/PetResanServiceSrv/Dto/PetResanServiceCreateDto.cs)، [خروجی برنامه](../Application/Services/TripSrv/PetResanServiceSrv/Dto/PetResanServiceVDto.cs)، [خروجی Trip](../Application/Services/TripSrv/TripSrv/Dto/TripVDto.cs)
- [ساخت نوبت، کیف پول، Broadcast، فهرست و پذیرش راننده](../Application/Services/TripSrv/TripSrv/TripService.cs)؛ تابع‌های `GenerateUpcomingOccurrencesAsync`, `CreateNextServiceOccurrenceAsync`, `GenerateOnePetResanTripAsync`, `GetAvailableTripsForDriverAsync`, `UpdateTripDriverStatusAsync`
- [ثبت jobها](../Api/Program.cs)، [قواعد ساعت تهران](../Application/Services/TripSrv/TripSrv/PetResanServiceTripRules.cs)، [کنترلر لیست آزاد راننده](../Api/Areas/Driver/Controllers/TripAvailableController.cs)
- [اسکریپت بررسی موجود پس از استقرار](../scripts/Check-PetResanServiceAfterDeploy.sql)

برخی توضیح‌ها/کامنت‌ها و مستندات قدیمی مخزن هنوز می‌گویند نوبت سرویس فقط با ادمین به راننده می‌رسد یا اولین نوبت هم از کیف پول کسر می‌شود. پیاده‌سازی `TripService` در commit بالا چنین نیست؛ برای عیب‌یابی این نسخه، شرط واقعی `TripAvailable` و شاخهٔ «اولین پرداخت پس از قبول راننده» ملاک‌اند.
