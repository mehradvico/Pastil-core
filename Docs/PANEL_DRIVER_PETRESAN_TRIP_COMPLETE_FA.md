# مستند فرانت‌اند پنل راننده / پت‌رسان

> نسخهٔ بازبینی‌شده از سورس فعلی API در ۴ اکتبر ۲۰۲۶  
> محدوده: **کل جریان قابل‌استفادهٔ راننده**؛ ۱۶ کنترلر Area راننده به‌اضافهٔ endpointهای مشترک لازم برای ورود، ثبت‌نام، آپلود مدرک، موقعیت و اعلان. عملیات ادمین و ایجاد سفر توسط مشتری فقط برای توضیح رابطه‌ها آمده‌اند.  
> ریشهٔ مسیرهای API اصلی: `/api`؛ بدنهٔ عملیات داده‌ای JSON است و آپلود تصویر در سرویس File با `multipart/form-data` انجام می‌شود.

## 1. دامنه، احراز هویت و قواعد پایه

تمام endpointهای بخش Driver با توکن راننده فراخوانی می‌شوند:

```http
Authorization: Bearer <access-token>
Content-Type: application/json
```

- راننده از روی توکن شناسایی می‌شود؛ `DriverId` یا `OwnerId` ارسالی کلاینت در عملیات حساس، معتبر شمرده نمی‌شود و سرور آن را از توکن جای‌گذاری می‌کند.
- راننده فقط پروفایل متعلق به حساب خود، سفرهایی که `DriverId` آن‌ها با رانندهٔ توکن یکی است، و سفرهای آزادِ واجدشرایط را می‌بیند.
- در بیشتر endpointها خطای کسب‌وکاری با HTTP `200` و `isSuccess: false` برمی‌گردد. بنابراین صرفاً HTTP status را معیار موفقیت ندانید. نبودن یا نامعتبر بودن توکن معمولاً `401/403` است.
- هیچ endpointی برای ایجاد یا حذف پروفایل راننده در پنل راننده وجود ندارد. ایجاد و تأیید اولیه خارج از این پنل است.
- قرارداد عضویت Area راننده در `AdminAreaAuthorizationConvention` اضافه می‌شود: عضو بودن با `driverId>0` سنجیده می‌شود؛ حالت پیش‌فرض `Security:AreaPolicyMode` فقط audit می‌کند و در حالت `Enforce` غیرعضو را رد می‌کند. بنابراین خود UI باید وضعیت پروفایل را بررسی کند و نتیجهٔ هر API را هم مستقل کنترل کند.

### مسیر کامل ورود تا فعال‌شدن راننده

1. ورود OTP مشترک: `POST /api/Account/otp` → `POST /api/Account/CheckOtp` → `POST /api/Account/signin` با `isAdmin=false`, `isSiteAdmin=false` → `GET /api/EndUser/CurrentUser`. قرارداد همهٔ فیلدها، refresh token و خطاها در [مستند ورود OTP پنل‌ها](PANEL_OTP_LOGIN_ALL_ROLES_COMPLETE_FA.md) است. صرف داشتن توکن یا تأیید OTP، رانندهٔ تأییدشده ایجاد نمی‌کند.
2. اگر پروفایل راننده وجود ندارد، عکس مدرک و کارت وسیله را با `POST {FILE_BASE_URL}/api/PictureUpload` بفرستید و `data.id` هر عکس را بگیرید. درخواست `multipart/form-data` با نام فیلد دقیق `PictureFile` و توکن است؛ تصویرهای `jpg/jpeg/png/webp`، حداکثر درخواست ۱۵ MiB، عرض/ارتفاع حداکثر ۸۰۰۰ و حداکثر ۲۵ مگاپیکسل پذیرفته می‌شوند. تصویر به WebP تبدیل می‌شود. این endpoint روی **سرویس File** است، نه API اصلی.
3. `POST /api/EndUser/Driver` با `DriverDto` بخش ۴.۳، بدون `id`، درخواست رانندگی را ثبت می‌کند. `ownerId` از توکن و `statusId=82`، `active=false`، `approved=false` از سرور تعیین می‌شود. هر کاربر فقط یک درخواست غیرحذف‌شده می‌تواند داشته باشد. این مسیر در Area Driver وجود ندارد، چون متقاضی هنوز عضو آن Area نیست.
4. `GET /api/EndUser/Driver` یا پس از تأیید `GET /api/Driver/Driver` وضعیت را می‌دهد. `82=در انتظار`، `83=تأیید`، `84=رد`؛ در ردشدن `adminDetail` دلیل ادمین است. ویرایش/ارسال مجدد با `PUT /api/EndUser/Driver` یا `PUT /api/Driver/Driver` دوباره وضعیت را به ۸۲ و `active/approved` را false می‌برد.
5. وقتی status برابر ۸۳ شد، `CurrentUser.driverId` معتبر می‌شود؛ این مقدار از وضعیت Driver در دیتابیس مشتق می‌شود. برای نمایش پیشنهاد سفر، علاوه بر status، `active` و `approved` را هم بررسی کنید. قرارداد کامل فرم ثبت‌نام در [مستند حساب راننده](DRIVER_ACCOUNT_APP_FRONTEND_FA.md) نیز آمده است.

### پوشش پاسخ استاندارد

عملیات تکی و عملیاتی معمولاً envelope زیر را دارند:

```json
{
  "code": 0,
  "isSuccess": true,
  "messages": [],
  "data": {}
}
```

`messages` آرایه‌ای از جفت پیام/نام‌فیلد است. در پاسخ JSON معمولاً هر عضو به‌شکل آرایهٔ دو عضوی نمایش داده می‌شود. بعضی PUTهای قدیمی فقط `isSuccess`، `code` و `messages` برمی‌گردانند. پاسخ‌های فهرستی (`GET /Driver/Trip`، `Driver`، `Code` و lookupها) مستقیم search object هستند و envelope `data` ندارند:

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "q": null,
  "sortBy": 1,
  "available": null,
  "totalCount": 42,
  "list": []
}
```

قواعد صفحه‌بندی مشترک: `pageIndex` پیش‌فرض `1` و حداقل `1` است؛ `pageSize` پیش‌فرض `20`، کمینهٔ مؤثر `20` وقتی مقدار کمتر از ۱ باشد، و بیشینه `1000` است. `sortBy`: `1=New`، `2=Old` (مقادیر دیگر در این بخش تضمین‌شده نیستند).

## 2. نقشهٔ قابلیت‌های پنل

| نیاز در UI | endpoint اصلی | نتیجه |
|---|---|---|
| وضعیت/مدارک راننده | `GET /Driver/Driver` و `GET /Driver/Driver/{id}` | فقط رکورد رانندهٔ خود کاربر |
| ویرایش خودرو و مدارک | `PUT /Driver/Driver` | ارسال مجدد برای تأیید؛ راننده موقتاً غیرفعال می‌شود |
| سفرهای آزاد قابل قبول | `GET /Driver/TripAvailable` | سفرهای online، باز و سازگار با خودرو؛ شامل بعضی سفرهای آینده نیز می‌شود |
| سفرهای پذیرفته‌شدهٔ آینده | `GET /Driver/TripUpcoming` | فهرست بدون صفحه‌بندی، مرتب بر اساس زمان حرکت |
| رزرو/سرویسِ متصل به پت‌رسان | همان `TripAvailable`، `TripCurrent` و `Trip` | رزرو مستقل در پنل راننده وجود ندارد؛ هر مورد به Trip تبدیل می‌شود |
| قبول یا رد پیشنهاد | `PUT /Driver/UpdateTripDriverStatus` | قبول رقابتی/اتمیک یا پنهان‌کردن برای همین راننده |
| سفر فعال | `GET /Driver/TripCurrent` | حداکثر سفر onlineِ requested/accepted راننده |
| لیست و سابقهٔ خود راننده | `GET /Driver/Trip` | فیلتر و صفحه‌بندی، با `DriverId` اجباری از توکن |
| جزئیات سفر | `GET /Driver/Trip/{id}` | فقط سفر خود راننده |
| اجرای مراحل سرویس | مسیرهای `TripArrived...` و `TripPetPickedUp` | توالی اجباری و تکمیل خودکار در مقصد نهایی |
| GPS زندهٔ کاربر | `GET /Driver/TripLive/{id}` | در طول سفر accepted |
| ارسال GPS راننده | `POST /EndUser/UserCurrentLocation` | وابستگی ضروری راننده؛ توضیح در بخش 9 |
| لغو توسط راننده | `PUT /Driver/TripDriverCancel` | فقط پیش از تحویل پت و با دلیل راننده |
| ثبت پرداخت نقدی/دستی | `PUT /Driver/ManualTripPayment` | فقط برای سفر خود راننده؛ نکتهٔ مهم در بخش 8 |
| کدها/گزینه‌ها/توقف‌ها | `GET /Driver/Code`, `TripOption`, `TripStop` | دیکشنری‌های فقط‌خواندنی |
| اعلان‌ها و آپلود مدرک | `/api/EndUser/push`, `/api/EndUser/notifications`, سرویس File | endpointهای مشترک لازم برای راننده |

## 3. مدل وضعیت سفر: سه مفهوم مستقل

این سه فیلد را با هم یکی نگیرید:

| فیلد | مقادیر/معنا | کاربرد UI |
|---|---|---|
| `tripStatusId` | `75 Requested`، `76 Accepted`، `77 Canceled`، `78 Compeleted` | وضعیت کلی عمر سفر |
| `driverStatusId` | `79 Requested`، `80 Accepted`، `81 Rejected` | پاسخ راننده به پیشنهاد؛ رد شدن در عمل به exclusion شخصی تبدیل می‌شود |
| `progressStageId` | `0 None`، `1 EnRouteOrigin`، `2 ArrivedOrigin`، `3 PetPickedUp`، `4 ArrivedDestination` | پیشرفت داخلی یک سفر Accepted |

چرخهٔ عادی:

```text
Requested / None
  └─ راننده قبول می‌کند → Accepted / EnRouteOrigin
       └─ رسیدن به مبدا → Accepted / ArrivedOrigin
            └─ تحویل‌گرفتن پت → Accepted / PetPickedUp
                 └─ رسیدن به مقصد نهایی → Compeleted / ArrivedDestination
```

برای `roundTrip=true`، رسیدن به مقصدِ مسیر رفت، سفر را کامل نمی‌کند: سرور `origin` و `destination` را جابه‌جا، `isReturnLeg=true` و مرحله را دوباره `EnRouteOrigin` می‌کند. سپس همان سه مرحله برای مسیر بازگشت اجرا می‌شوند. فقط رسیدن به مقصدِ نهایی وضعیت را `Compeleted` می‌کند و سهم راننده/سایت را محاسبه می‌کند.

### قواعد مهم UI

- فقط انتقال به **مرحلهٔ دقیقاً بعدی** مجاز است؛ دکمهٔ مرحلهٔ بعد را بر مبنای `progressStageId` بسازید و از retry کور پرهیز کنید.
- پس از `PetPickedUp`، راننده دیگر اجازهٔ لغو ندارد.
- سفر `Canceled` یا `Compeleted` فعال نیست؛ polling و دکمه‌های عملیاتی را متوقف کنید.
- `progressUpdateDate` زمان آخرین تغییر مرحله/وضعیت است، نه الزاماً زمان واقعی GPS.

## 4. پروفایل راننده و خودرو

### 4.1 گرفتن پروفایل خود راننده

#### `GET /api/Driver/Driver`

برای سازگاری با API خروجی search است، هرچند سرور `ownerId` را از توکن پر می‌کند؛ معمولاً `list[0]` پروفایل راننده است.

Query قابل ارسال:

| فیلد | نوع | توضیح |
|---|---:|---|
| `pageIndex`, `pageSize`, `q`, `sortBy`, `available` | عمومی | `q` در نام راننده شامل‌جستجو می‌کند؛ `available` روی `active` فیلتر می‌کند؛ `sortBy=1/2` بر اساس id جدید/قدیمی است. |
| `vehicle` | string | شامل متن وسیله |
| `rate` | int | در DTO هست، اما سرویس فعلی این فیلتر را اعمال نمی‌کند. |
| `cityId`, `neighborhoodId` | long | محدودهٔ پروفایل |
| `statusId` | long | وضعیت درخواست راننده |
| `approved` | bool | تأییدشده یا نه |
| `vehicleTypeId` | long | نوع وسیله |

`ownerId` حتی اگر در query ارسال شود با `userId` توکن جایگزین می‌شود.

#### `GET /api/Driver/Driver/{id}`

تنها وقتی موفق است که `ownerId` رکورد با کاربر توکن برابر باشد. پاسخ `BaseResultDto<DriverVDto>` است. برای id متعلق به دیگران، پاسخ کسب‌وکاری عدم دسترسی می‌آید.

### 4.2 قرارداد `DriverVDto` (خروجی پروفایل)

| فیلد | نوع | توضیح |
|---|---:|---|
| `id`, `name` | long, string | شناسه و نام راننده |
| `ownerId`, `owner` | long, object | حساب مالک راننده؛ فقط خواندنی |
| `phone` | string | شماره تماس راننده |
| `vehicle`, `licensePlateNumber` | string | مشخصات وسیله و پلاک |
| `ownerDetail` | string | توضیح راننده |
| `active` | bool | امکان فعالیت؛ بعد از ثبت‌مجدد false می‌شود |
| `approved` | bool | تأیید نهایی ادمین |
| `statusId`, `status` | long, `CodeVDto` | وضعیت درخواست ثبت/تأیید راننده؛ `82/83/84` |
| `rate` | int | امتیاز، بازهٔ معتبر 1 تا 5 یا صفر |
| `cityId`, `city` | long, object | شهر فعالیت |
| `neighborhoodId`, `neighborhood` | long?, object | محله، در صورت انتخاب |
| `vehicleTypeId`, `vehicleType` | long?, `CodeVDto` | نوع خودرو |
| `profilePictureId`, `profilePicture` | long?, `PictureVDto` | تصویر پروفایل |
| `certificatePictureId`, `certificatePicture` | long?, `PictureVDto` | مدرک/گواهی راننده |
| `vehicleCardPictureId`, `vehicleCardPicture` | long?, `PictureVDto` | تصویر کارت وسیله |
| `adminDetail` | string | توضیح ادمین؛ مثلاً دلیل رد |

`PictureVDto`: `id`, `url`, `orginalName`، `baseUrl`، `guidName`، `extension`.  `CodeVDto`: `id`, `name`, `label`, `value`.

### 4.3 ویرایش و ارسال مجدد پروفایل

#### `PUT /api/Driver/Driver`

بدنهٔ `DriverDto`:

```json
{
  "id": 19,
  "name": "نام راننده",
  "ownerId": 0,
  "phone": "0912...",
  "vehicle": "خودرو سواری",
  "licensePlateNumber": "...",
  "ownerDetail": "توضیحات اختیاری",
  "active": true,
  "rate": 0,
  "profilePictureId": 101,
  "certificatePictureId": 102,
  "vehicleCardPictureId": 103,
  "cityId": 1,
  "neighborhoodId": 2,
  "statusId": 0,
  "adminDetail": null,
  "approved": false,
  "vehicleTypeId": 7
}
```

| فیلد | اجباری | اعتبارسنجی/رفتار واقعی |
|---|---:|---|
| `id` | بله | باید شناسهٔ پروفایل متعلق به رانندهٔ توکن باشد |
| `name`, `phone`, `vehicle`, `licensePlateNumber` | بله | نباید خالی باشند؛ شماره با ارقام انگلیسی ذخیره می‌شود |
| `cityId` | بله | باید شهر معتبر باشد |
| `certificatePictureId`, `vehicleCardPictureId` | بله | باید به فایل موجود اشاره کنند |
| `profilePictureId` | خیر | اگر داده شود باید فایل معتبر باشد |
| `neighborhoodId` | خیر | باید متعلق به `cityId` باشد |
| `rate` | خیر | صفر یا 1 تا 5 |
| `vehicleTypeId` | خیر | نوع وسیله |
| `ownerId`, `active`, `approved`, `statusId`, `adminDetail` | خیر/غیرقابل‌اعتماد | سرور مالک را از توکن می‌گیرد و وضعیت تأیید را خودش تنظیم می‌کند؛ UI نباید این‌ها را قابل ویرایش بداند |

**اثر موفقیت:** سرور داده‌ها را ذخیره و `statusId=82` (Requested)، `active=false`، `approved=false` و `adminDetail=null` می‌کند. `rate` قبلی حفظ می‌شود. بنابراین پس از پاسخ موفق، پنل باید صفحهٔ «در انتظار تأیید» نشان دهد و امکان قبول سفر را تا فعال‌شدن مجدد محدود کند. پاسخ `BaseResultDto` است و data ندارد.

## 5. مشاهده و دریافت سفر

### 5.1 سفرهای آزاد برای پذیرش

#### `GET /api/Driver/TripAvailable`

پاسخ: `BaseResultDto<List<TripVDto>>`.

سرور این شرط‌ها را اعمال می‌کند:

- `tripStatusId=75`، `driverId=null` و `isOnline=true`؛
- سفر برای این راننده قبلاً رد یا توسط همین راننده لغو نشده باشد؛
- اگر `vehicleTypeId` سفر null نباشد، با `vehicleTypeId` راننده برابر باشد؛ null یعنی برای همهٔ انواع وسیله قابل دیدن است؛
- ترتیب بر اساس قدیمی‌ترین `createDate` است؛
- اگر راننده **هر سفر پذیرفته‌شدهٔ دیگری** با `tripStartDateTime <= اکنون + ۱ ساعت` داشته باشد، پاسخ موفق با `data: []` می‌آید. این شرط در کد به سفر زمان‌دار محدود نشده و سفر دیرکردهٔ تکمیل‌نشده نیز می‌تواند فهرست را خالی نگه دارد.

در query فهرست، **هیچ شرطی برای فوری بودن، خالی بودن `scheduledDepartureAt` یا نداشتن `petResanServiceScheduleId` وجود ندارد**. بنابراین سفر رزرویِ آینده و نوبت هفتگیِ آزاد نیز، اگر هنوز `Requested` و بدون راننده و `isOnline=true` باشند، می‌توانند در همین فهرست ظاهر شوند. نبودن پوش broadcast به معنی نبودن در این فهرست نیست. این لیست یک پیشنهاد است؛ برای قبول حتماً endpoint بخش ۶ را بزنید و پاسخ آن را ملاک قرار دهید.

### 5.2 سفرهای پذیرفته‌شدهٔ پیشِ‌رو

#### `GET /api/Driver/TripUpcoming`

پاسخ `BaseResultDto<List<TripVDto>>` و **بدون query/pagination** است. فقط سفرهای `driverId` همین راننده با `tripStatusId=76`، `tripStartDateTime` مقدار‌دار و **بزرگ‌تر از اکنون** را می‌دهد؛ ترتیب صعودی زمان حرکت است. service در این query نوع منشأ سفر را جدا نمی‌کند: هر سفر پذیرفته‌شدهٔ آینده، از جمله رزرو، سرویس هفتگی یا سفر مستقل تاریخ‌دار، می‌تواند بیاید. با رسیدن زمان، حتی اگر سفر هنوز باز مانده باشد از این فهرست خارج می‌شود؛ برای ادامهٔ اجرا از `TripCurrent`/`Trip` استفاده کنید.

راننده‌ای که یک سفر پذیرفته‌شده با `tripStartDateTime <= اکنون + ۱ ساعت` دارد، سفر آزاد جدیدی نمی‌بیند و `UpdateTripDriverStatus` هم پذیرش جدید را با `DriverHasUpcomingScheduledTrip` رد می‌کند. معیار دقیق فقط همین تاریخ و وضعیت است؛ برچسب «رزروی/سرویسی» به‌تنهایی شرط نیست. وقتی `tripStartDateTime` از اکنون گذشته باشد، تا پیش از تکمیل/لغو همچنان می‌تواند عامل این انسداد باشد.

### 5.3 سفر جاری

#### `GET /api/Driver/TripCurrent`

پاسخ: `BaseResultDto<TripVDto>`. سفر جاری **اولین رکورد بدون ترتیب صریح** از رانندهٔ توکن است که `isOnline=true`، `tripStatusId` برابر Requested یا Accepted و `driverStatusId != 84` باشد؛ عدد ۸۴ کد وضعیت **پروفایل** است و با ۸۱ِ رد پیشنهاد سفر یکی نیست. این شرط دوم عملاً رد سفر را فیلتر دقیق نمی‌کند. متد شرط «زمان حرکت رسیده» ندارد، پس سفر پذیرفته‌شدهٔ آینده نیز ممکن است به‌عنوان current برگردد. اگر رکوردی پیدا نشود `isSuccess=false` است. برای صفحهٔ اصلی، `TripUpcoming` و `TripCurrent` را با زمان و `tripStatusId` کنار هم تفسیر کنید؛ صرف موفق‌بودن `TripCurrent` مجوز شروع فوری عملیات مرحله‌ای نیست.

### 5.4 لیست و سابقهٔ سفرهای خود راننده

#### `GET /api/Driver/Trip`

خروجی `TripSearchDto` (فیلدهای مشترک pagination + `list: TripVDto[]`). سرور همیشه `driverId` را از توکن می‌گذارد؛ ارسال `driverId` دلخواه امنیت یا نتیجه را تغییر نمی‌دهد.

| Query | نوع | فیلتر واقعی |
|---|---:|---|
| `fromCityId` | long | شهر مبدأ |
| `passengerId` | long | مالک پت/مسافر؛ برای تاریخچهٔ خود راننده معمولاً لازم نیست |
| `driverId` | long | توسط سرور بازنویسی می‌شود |
| `driverStatusId` | long | 79/80/81 |
| `tripStatusId` | long | 75/76/77/78 |
| `vehicleTypeId` | long | نوع خودرو |
| `isPaid` | bool | پرداخت‌شده/نشده |
| `manualPay` | bool | پیاده‌سازی فقط «دارای `manualPayDate`» را فیلتر می‌کند؛ مقدار true/false تفاوتی ندارد |
| `fromDate`, `toDate` | DateTime | روی `createDate`، نه زمان سرویس |
| `toMinute` | double | سفرهایی که `createDate + n دقیقه < اکنون` باشد |
| `scheduledOrServiceOnly` | bool? | **فقط وقتی true باشد**: سفرهای متصل به سه نوع رزرو، یا دارای `scheduledDepartureAt`، یا دارای schedule سرویس هفتگی. false و null شرطی اضافه نمی‌کنند. |
| `q` | string | نام/نام خانوادگی/موبایل مالک پت |
| `point.x`, `point.y`, `point.distanceMeter` | number | مبدأهای در شعاعِ کمتر از `distanceMeter` از point؛ مختصات x,y هستند |
| `pageIndex`, `pageSize`, `sortBy` | عمومی | `New=1` نزولی id و `Old=2` صعودی id |

نمونه:

```http
GET /api/Driver/Trip?tripStatusId=78&isPaid=true&pageIndex=1&pageSize=20&sortBy=1
```

### 5.5 جزئیات یک سفر خود راننده

#### `GET /api/Driver/Trip/{id}`

پاسخ `BaseResultDto<TripDto>`. اگر trip به رانندهٔ توکن تعلق نداشته باشد، data پیش‌فرض و `isSuccess=false` است. این خروجی با `TripVDto` کمی تفاوت دارد: به جای `userPets`، فهرست `userPetIds` را دارد؛ `manualPayDate` و `progressUpdateDate` در مدل خروجی آن نیستند.

### 5.6 رزروها و سرویس‌های متصل به Pet-resan در پنل راننده

#### اصل معماری

راننده **رزرو یا سرویس را ایجاد، ویرایش یا مستقلاً دریافت نمی‌کند**. هر درخواست حمل به یک `Trip` عملیاتی تبدیل می‌شود و راننده فقط آن Trip را از endpointهای Driver می‌بیند/می‌پذیرد/اجرا می‌کند. بنابراین هیچ‌یک از endpointهای زیر در Area `Driver` وجود ندارد:

- دریافت جزئیات `CompanionReserve`، `PansionReserve`، `SchoolReserve` یا `PetResanService` با شناسه؛
- ایجاد/لغو خود رزرو یا سرویس توسط راننده؛
- فهرست برنامهٔ هفتگی Pet-resan برای راننده.

اطلاعاتی که راننده واقعاً برای اجرا می‌گیرد، در خود `TripVDto` یا `TripDto` است: آدرس/مختصات، زمان حرکت، پت‌ها، صاحب پت، گزینه‌ها، مبلغ، مرحله و سه شناسهٔ اتصال به رزرو. برای این نقش، `Trip` منبع حقیقت اجرا است؛ تاریخچه یا وضعیت رزرو را از API نقش‌های دیگر درخواست نکنید.

#### انواع منشأ سفر و دادهٔ قابل‌نمایش

| نوع منشأ | چگونه در پاسخ راننده تشخیص داده می‌شود | زمان مهم برای راننده | رفتار دریافت/پذیرش |
|---|---|---|---|
| رزرو خدمت کلینیک یا نماینده | `companionReserveId != null` | `scheduledDepartureAt` و `tripStartDateTime`؛ زمان حرکت = زمان نوبت منهای 60 یا 120 دقیقه | سفر از لحظهٔ ثبت برای رانندگان فعال broadcast و در `TripAvailable` قابل قبول است |
| رزرو پانسیون | `pansionReserveId != null` | `scheduledDepartureAt` و `tripStartDateTime`؛ زمان تحویل پت به پانسیون منهای 60 یا 120 دقیقه | مانند مورد قبل در TripAvailable ظاهر می‌شود |
| ثبت‌نام مدرسه | `schoolReserveId != null` | `scheduledDepartureAt` و `tripStartDateTime`؛ زمان اولین جلسهٔ حضوری آینده منهای 60 یا 120 دقیقه | مانند مورد قبل در TripAvailable ظاهر می‌شود |
| سرویس تکرارشوندهٔ هفتگی پت‌رسان | **در DTO راننده شناسهٔ قابل اتکای منشأ ندارد**؛ بخش محدودیت را ببینید | `tripStartDateTime` زمان occurrence روز بعد است | سامانه شب قبل Trip می‌سازد؛ پوش broadcast اختصاصی در مسیر ساخت سرویس دیده نمی‌شود، اما query `TripAvailable` می‌تواند Trip آزاد آن را برگرداند. تخصیص ادمین هم ممکن است. |
| سفر فوری عادی | هر سه reserve id خالی است؛ با این حال همین شرط به‌تنهایی برای تشخیص سرویس هفتگی کافی نیست | معمولاً `scheduledDepartureAt=null` | TripAvailable در صورت بازبودن |

برای هر سه رزروِ متصل، فقط یک Trip غیرلغوشده برای همان رزرو مجاز است. `scheduledLeadMinutes` فقط `60` یا `120` است. `ownerRidesAlong` نیز صریحاً می‌گوید آیا صاحب پت همراه سفر است؛ آن را به‌صورت نشانگر «همراه مالک» در کارت/جزئیات راننده نمایش دهید.

#### فیلدهای رزرو/زمان‌بندی که پنل راننده باید مصرف کند

| فیلد Trip | نوع | معنا و نحوهٔ استفاده در UI |
|---|---:|---|
| `companionReserveId` | long? | شناسهٔ ارتباط با رزرو خدمات کلینیک/نماینده؛ برای برچسب «رزرو خدمت» کافی است، اما نام مرکز/سرویس را برنمی‌گرداند |
| `pansionReserveId` | long? | شناسهٔ ارتباط با رزرو پانسیون؛ فقط برای برچسب «رزرو پانسیون» قابل اتکاست |
| `schoolReserveId` | long? | شناسهٔ ارتباط با ثبت‌نام مدرسه؛ فقط برای برچسب «رزرو مدرسه» قابل اتکاست |
| `scheduledDepartureAt` | DateTime? | زمان برنامه‌ریزی‌شدهٔ **حرکت راننده**؛ برای زمان‌بندی راننده اولویت دارد |
| `tripStartDateTime` | DateTime? | در سفرهای رزروی با `scheduledDepartureAt` برابر است؛ برای سرویس هفتگی زمان occurrence است |
| `scheduledLeadMinutes` | int? | فاصلهٔ 60 یا 120 دقیقه‌ای که پیش از زمان رزرو حرکت تعیین شده؛ فقط اطلاعاتی است |
| `ownerRidesAlong` | bool | همراه بودن مالک پت |
| `isOnline` | bool | رزروهای متصل نیز `true` هستند؛ آن را مساوی «فوری» فرض نکنید |
| `origin`, `destination`, `fromAddress`, `toAddress` | object/string | مسیر اجرایی و دادهٔ قابل اعتماد برای ناوبری؛ مقصد ممکن است مرکز/پانسیون/مدرسه باشد، اما API نام آن موجودیت را نمی‌دهد |
| `userPets[]` در `TripVDto` | array | همهٔ پت‌های همان سفر؛ در جزئیات `TripDto` فقط `userPetIds[]` وجود دارد |
| `tripOptions[]`, `tripStop`, `stopMinutes` | object/number | خدمات یا توقف‌های انتخاب‌شده برای همان Trip، نه جزئیات رزرو مادر |

#### قواعد عملی هر نوع رزرو

**رزرو خدمت (Companion):** سفر فقط زمانی ساخته می‌شود که نوبت آینده باشد و زمان حرکت با lead time انتخابی از اکنون جلوتر باشد. مقصد و آدرس از request پت‌رسان می‌آیند؛ در DTO راننده، نام خدمت، نام کلینیک/نماینده، زمان خود نوبت یا وضعیت خود رزرو وجود ندارد. راننده باید بر اساس `scheduledDepartureAt`، آدرس و `companionReserveId` عمل کند.

**رزرو پانسیون:** سفر برای تحویل پت هنگام ورود به هتل/مهد ساخته می‌شود. لنگر زمانی آن زمان تحویل واقعی پانسیون است (برای مدل روزانه/مهد از زمان شروع و برای اقامتی از زمان تحویلی که کاربر تعیین کرده) و راننده فقط زمان حرکت محاسبه‌شده را دریافت می‌کند. نام پانسیون، تاریخ خروج و وضعیت رزرو پانسیون در پاسخ Driver نیست.

**رزرو مدرسه:** فقط برای دورهٔ حضوری (`CourseTypeId=3`) و اولین جلسهٔ فعال آینده سفر ساخته می‌شود؛ ویدئویی/آنلاین سفر فیزیکی ندارند. پنل راننده نباید از `schoolReserveId` برای استخراج نام دوره، جلسه یا مدرسه استفاده کند، زیرا Driver API آن داده را ارائه نمی‌دهد.

**سرویس دوره‌ای Pet-resan:** برنامهٔ هفتگی فعال، هر شب ساعت ۲۰ به وقت تهران برای روز بعد Trip واقعی تولید می‌کند. اگر ساعت رفت و برگشت جدا داشته باشد، دو Trip یک‌طرفه ساخته می‌شود: برگشت `isReturnLeg=true` و آدرس‌ها معکوس است. اگر `returnTime` نداشته باشد، یک Trip با `roundTrip=true` تولید می‌شود و راننده مراحل رفت و برگشت را طبق بخش ۷ انجام می‌دهد. هزینهٔ occurrence از کیف پول کاربر کسر می‌شود؛ اگر شارژ ناموفق باشد، Trip با `tripStatusId=77` و `cancelInitiatorId=3` لغو می‌شود و نباید به راننده نمایش داده شود.

#### لغو رزرو مادر و اثر آن برای راننده

اگر رزرو خدمت، پانسیون یا مدرسه لغو شود، سرور Trip متصلِ باز را نیز به‌صورت سیستمی لغو می‌کند (`tripStatusId=77`، `cancelInitiatorId=3` و توضیح لغو). بنابراین:

1. راننده حق ندارد از روی `*ReserveId` وضعیت رزرو را تغییر دهد یا لغو را خودش تشخیص قطعی بدهد.
2. در صفحهٔ active، پاسخ هر poll/عملیات و `GET /Driver/TripCurrent` را بررسی کنید؛ با `Canceled` شدن، مسیر و ارسال GPS را متوقف کنید.
3. UI باید «لغو سیستمی/لغو رزرو مرتبط» را از لغو راننده (`cancelInitiatorId=1`) متمایز نمایش دهد.

#### زمان‌بندی، اعلان و یک ناهماهنگی مهم سرویس هفتگی

- برای سفر پذیرفته‌شده‌ای که `tripStartDateTime` آن تا ۳۰ دقیقهٔ آینده باشد، job هر دقیقه یک‌بار پوش `PushTripDriverUpcomingReminder` با شناسهٔ نوع `81` می‌فرستد؛ `token1` «سرویس» یا «سفر رزروشده» و `token2` شناسهٔ Trip است. اعلان با `driverReminderSentDate` تکراری نمی‌شود. این فیلد در DTO راننده ارائه نشده است؛ UI فهرست `TripUpcoming` را منبع زمان حرکت بگیرد.
- سفر زمان‌دارِ بی‌راننده، یک ساعت مانده به حرکت به ادمین Notice و در موعد حرکت هشدار نهایی می‌دهد. این job به‌طور خودکار راننده تعیین نمی‌کند.
- **ناسازگاری فعلی:** نوبت هفتگی در دیتابیس `petResanServiceScheduleId` دارد ولی `scheduledDepartureAt=null` می‌ماند. job `AutoCancelUnansweredInstantTripsAsync` فقط `scheduledDepartureAt` خالی، `Requested` و بدون راننده بودن را می‌سنجد؛ بنابراین نوبت هفتگیِ فردا اگر ظرف حدود ۵ دقیقه از ساخت در ساعت ۲۰ پذیرفته/تخصیص داده نشود، ممکن است مثل سفر فوری به‌شکل سیستمی لغو شود. فرانت نباید وجود نوبت فردا یا موفقیت ساخت سرویس را به معنی بازماندن قطعی Trip تا فردا بداند. این شرط نیازمند اصلاح بک‌اند است.

#### محدودیت قطعی API فعلی — دادهٔ لازم اما expose نشده

در مدل دیتابیس برای سرویس هفتگی، `petResanServiceScheduleId` وجود دارد، اما در `TripDto` و `TripVDto` راننده برگردانده نمی‌شود. همچنین برای سه `*ReserveId`، navigation/summary رزرو (مانند نام مرکز، نام پانسیون، نام مدرسه، نام دوره، ساعت نوبت/جلسه و وضعیت رزرو) در DTO راننده موجود نیست.

نتیجهٔ عملی: فرانت راننده می‌تواند فقط **نوع سه رزروِ دارای id، زمان حرکت، مسیر، همراه بودن مالک و پت‌ها** را با اطمینان نشان دهد. تشخیص دقیق سرویس هفتگی یا نمایش نام/جزئیات رزرو مادر از پاسخ فعلی ممکن نیست و نباید با حدس یا فراخوانی endpoint کاربر پیاده‌سازی شود.

برای تکمیل تجربهٔ راننده، نیاز به تغییر بک‌اند است؛ یک قرارداد امن پیشنهادی در خروجی `TripVDto`/`TripDto` می‌تواند چنین باشد (این فیلدها اکنون وجود ندارند):

```json
{
  "tripSource": "Instant | CompanionReserve | PansionReserve | SchoolReserve | PetResanService",
  "linkedReservation": {
    "id": 0,
    "displayTitle": "نام مرکز/پانسیون/دوره",
    "scheduledServiceAt": "2026-09-28T10:00:00",
    "statusId": 0,
    "statusLabel": "..."
  },
  "petResanServiceScheduleId": 0
}
```

تا پیش از اضافه‌شدن چنین قراردادی، endpoint جدیدی برای جزئیات رزرو در پنل راننده مستند نمی‌شود؛ چون در Area Driver وجود ندارد و نباید اطلاعات رزروهای دیگران را افشا کند.

## 6. قبول یا رد پیشنهاد سفر

#### `PUT /api/Driver/UpdateTripDriverStatus`

بدنه:

```json
{ "id": 520, "driverId": 0, "driverStatusId": 80 }
```

| فیلد | اجباری | توضیح |
|---|---:|---|
| `id` | بله | شناسهٔ Trip |
| `driverStatusId` | بله | `80` قبول یا `81` رد |
| `driverId` | خیر | هر مقدار ارسالی نادیده گرفته و از توکن پر می‌شود |

### قبول (`driverStatusId=80`)

سرور با یک update اتمیک فقط وقتی سفر را به راننده می‌دهد که هنوز `Requested`، بدون راننده و بدون exclusion برای او باشد. در موفقیت:

- `driverId` از توکن راننده ثبت می‌شود؛
- `driverStatusId=80`؛
- `tripStatusId=76`؛
- `progressStageId=1` (`EnRouteOrigin`)؛
- `progressUpdateDate` ثبت می‌شود؛
- به کاربر اعلان پذیرش ارسال می‌شود.

اگر رانندهٔ دیگری هم‌زمان زودتر پذیرفته باشد، پاسخ ناموفق «برای رانندهٔ دیگر رزرو شده» است. پس پس از failure، آیتم را از فهرست available بردارید و فهرست را refresh کنید.

### رد (`driverStatusId=81`)

سفر همچنان برای سایر راننده‌ها باز می‌ماند، اما یک exclusion برای همین راننده ثبت می‌شود و دیگر در `TripAvailable` او دیده نمی‌شود. این کار idempotent عملی است؛ تکرار رد، exclusion دوم ثبت نمی‌کند. در UI برای رد confirmation بگیرید؛ در این نسخه امکان undo/re-show برای همان راننده وجود ندارد.

پاسخ هر دو حالت: `BaseResultDto<TripDriverChangeStatusDto>` با data شامل `id`، `driverId` (سروری) و `driverStatusId`.

## 7. اجرای سفر پت‌رسان

همهٔ عملیات این بخش فقط برای سفرِ متعلق به رانندهٔ توکن و فقط وقتی `tripStatusId=76` انجام می‌شوند. پاسخ همه `BaseResultDto<TripVDto>` است.

| مرحلهٔ فعلی | اقدام مجاز | endpoint | مرحلهٔ پس از موفقیت |
|---|---|---|---|
| `1 EnRouteOrigin` | اعلام رسیدن به مبدأ | `PUT /api/Driver/TripArrivedOrigin/{id}` | `2 ArrivedOrigin` |
| `2 ArrivedOrigin` | اعلام تحویل‌گرفتن پت | `PUT /api/Driver/TripPetPickedUp/{id}` | `3 PetPickedUp` |
| `3 PetPickedUp` | اعلام رسیدن به مقصد | `PUT /api/Driver/TripArrivedDestination/{id}` | یک‌طرفه: `4/Compeleted`؛ رفت‌وبرگشت: مسیر برگشت با stage=1 |

این endpointها بدنه ندارند. نمونه:

```http
PUT /api/Driver/TripPetPickedUp/520
Authorization: Bearer <access-token>
```

پس از هر پاسخ موفق، `data` برگشتی را جایگزین state محلی کنید؛ به‌ویژه در رفت‌وبرگشت آدرس‌های `origin` و `destination` در همان پاسخ عوض می‌شوند و `isReturnLeg=true` می‌شود.

### لغو توسط راننده

#### `PUT /api/Driver/TripDriverCancel`

ابتدا dictionary دلایل را از بخش 10 بگیرید. بدنه:

```json
{
  "id": 520,
  "cancelReasonCodeId": 431,
  "cancelDetail": "توضیح اختیاری راننده"
}
```

| فیلد | اجباری | قاعده |
|---|---:|---|
| `id` | بله | باید سفر پذیرفته‌شدهٔ همان راننده باشد |
| `cancelReasonCodeId` | بله | کد active در گروه `TripCancelReason_Driver` |
| `cancelDetail` | خیر | متن تکمیلی |

محدودیت‌ها و اثر:

- فقط `Accepted` قابل لغو است؛
- با `progressStageId >= 3` (پس از تحویل پت) سرور لغو را رد می‌کند؛
- موفقیت: `tripStatusId=77`، `cancelInitiatorId=1`، دلیل و توضیح ذخیره می‌شود؛
- سفر دوباره broadcast نمی‌شود و برای همین راننده هم در درخواست‌های بعدی نمایش داده نمی‌شود؛ کاربر باید سفر تازه ایجاد کند؛
- به کاربر اعلان ارسال می‌شود.

`cancelInitiatorId`: `1=Driver`، `2=User`، `3=System`، `4=Admin`.

### endpoint موجود اما برای UI عملیاتی راننده توصیه نمی‌شود

#### `PUT /api/Driver/TripChangeStatus`

بدنه `{ "id": 520, "tripStatusId": 75|76|77|78 }` است و کنترلر فقط مالکیت سفر را بررسی می‌کند. با این حال منطق سرویس برای تغییر دستی ادمین نوشته شده است: تغییر به Requested راننده را آزاد و مراحل/اطلاعات لغو را پاک می‌کند؛ تغییر به Canceled/Compeleted از منطق ادمین استفاده می‌کند. بنابراین **در flow عادی راننده از آن استفاده نکنید**:

- برای قبول/رد فقط `UpdateTripDriverStatus`؛
- برای پیشرفت فقط سه endpoint مرحله؛
- برای لغو فقط `TripDriverCancel`.

این endpoint در فهرست کامل API راننده ثبت شده، اما باید در فرانت به‌عنوان legacy/غیرقابل‌نمایش نگهداری شود تا bypass ترتیب مراحل رخ ندهد.

## 8. پرداخت دستی سفر

#### `PUT /api/Driver/ManualTripPayment`

بدنه:

```json
{ "id": 520, "isPaid": true, "manualPay": "2026-09-28T12:30:00" }
```

کنترلر فقط اجازه می‌دهد راننده روی سفر خود عمل کند. پیاده‌سازی فعلی، در صورت موفقیت **بدون توجه به مقدار ارسالی `isPaid` یا `manualPay`** این مقادیر را روی سرور می‌گذارد:

- `isPaid=true`
- `manualPayDate=DateTime.Now` سرور

پس UI باید این endpoint را صرفاً «ثبت پرداخت دستی دریافت شد» بداند؛ زمان و bool ارسال‌شده را منبع حقیقت نداند. پاسخ `BaseResultDto<ManualPayTripDto>` است و `data` همان payload را echo می‌کند، نه لزوماً timestamp نهایی. برای دیدن زمان ثبت‌شده، تاریخچه را با `GET /Driver/Trip` refresh کنید (`manualPayDate` در `TripVDto` است).

## 9. مکان زنده و حریم دسترسی

### 9.1 دریافت GPS کاربر در سفر فعال

#### `GET /api/Driver/TripLive/{id}`

فقط زمانی موفق است که سفر به رانندهٔ توکن تعلق داشته و `tripStatusId=76` باشد. پاسخ:

```json
{
  "isSuccess": true,
  "data": {
    "tripStatusId": 76,
    "progressStageId": 2,
    "isReturnLeg": false,
    "counterpartLocation": { "x": 51.389, "y": 35.689, "distanceMeter": 0 },
    "counterpartLocationUpdatedAt": "2026-09-28T08:20:00Z"
  }
}
```

`counterpartLocation` مکان آخرین ثبت‌شدهٔ **کاربر صاحب سفر** است، نه مبدأ/مقصد ثابت و نه موقعیت راننده. ممکن است null باشد. `x` و `y` مختصات مکانی‌اند و `distanceMeter` برای این پاسخ عملاً کاربردی ندارد. فقط در صفحهٔ سفر active poll کنید و پس از Cancel/Complete آن را متوقف کنید.

### 9.2 ارسال GPS خود راننده — وابستگی لازم پنل راننده

در Area `Driver` endpoint اختصاصی ثبت GPS وجود ندارد؛ API مکان را به user account متصل می‌کند. چون حساب راننده همان `userId` توکن را دارد، پنل راننده برای اینکه موقعیتش به کاربر نمایش داده شود باید این endpoint احراز‌شده را صدا بزند:

#### `POST /api/EndUser/UserCurrentLocation`

> این endpoint از Area مشترک کاربر برای حساب راننده استفاده می‌کند. مسیرهای مشترک ورود، ثبت‌نام، مدرک و اعلان نیز در بخش‌های ۱ و ۱۴ آمده‌اند.

```json
{
  "location": { "x": 51.389, "y": 35.689, "distanceMeter": 0 }
}
```

| فیلد | اجباری | توضیح |
|---|---:|---|
| `location.x`, `location.y` | بله | عدد finite؛ مختصات مکانی |
| `location.distanceMeter` | خیر | در ثبت استفاده نمی‌شود؛ صفر بفرستید |

سرور userId را از توکن می‌گیرد، شهر/محله را از محدودهٔ جغرافیایی تعیین می‌کند و آخرین مکان را upsert می‌کند. اگر نقطه در شهر/نزدیک‌ترین محلهٔ قابل تشخیص نباشد، نتیجه ناموفق است. پاسخ `BaseResultDto<UserCurrentLocationDto>`:

```json
{
  "id": 6,
  "userId": 44,
  "location": { "x": 51.389, "y": 35.689, "distanceMeter": 0 },
  "cityId": 1,
  "neighborhoodId": 2,
  "lastUpdateDate": "2026-09-28T08:20:00Z"
}
```

**پیشنهاد عملی:** فقط با permission صریح سیستم‌عامل و در سفر `Accepted`، با بازه‌ای معقول و قابل‌تنظیم موقعیت را ارسال کنید؛ پس از پایان/لغو یا خروج راننده از حالت کاری ارسال را قطع کنید. interval در API تعریف نشده است، پس بدون هماهنگی محصول/بک‌اند مقدار ثابت قراردادی فرض نکنید.

## 10. lookupها و کدهای موردنیاز راننده

### 10.1 دلایل لغو سفر راننده

#### `GET /api/Driver/Code?CodeGroupLabel=TripCancelReason_Driver`

برای نمایش select لغو، دقیقاً همین `CodeGroupLabel` را بفرستید. خروجی search object با `list: CodeVDto[]` است:

```json
{
  "id": 431,
  "name": "...",
  "label": "...",
  "value": "..."
}
```

در `TripDriverCancel` فقط `id` کد فعال انتخاب‌شده را در `cancelReasonCodeId` بفرستید. نام/متن دلایل دادهٔ مدیریت‌شوندهٔ سرور است؛ آن‌ها را در کلاینت hard-code نکنید. `available=true` را هم بفرستید؛ سرویس Code بدون آن رکوردهای غیرفعال را نیز در lookup می‌آورد. `q` در این سرویس **برابری دقیق با `name`** است، نه جستجوی شامل متن. `sortBy` ترتیب را تغییر نمی‌دهد؛ کدها بر اساس `priority` مرتب می‌شوند. همین endpoint برای `codeGroupLabel=DriverRequestStatus`، `Driver_Status` و `TripStatus` نیز قابل استفاده است.

گزینه‌های seed فعلی دلیل راننده (Code ID آن‌ها در هر محیط می‌تواند متفاوت باشد):

| label | متن نمایشی |
|---|---|
| `TripCancelReason_Driver_VehicleIssue` | مشکل فنی خودرو |
| `TripCancelReason_Driver_UserUnreachable` | عدم پاسخگویی کاربر |
| `TripCancelReason_Driver_TrafficDelay` | ترافیک و تأخیر زیاد |
| `TripCancelReason_Driver_PetMismatch` | عدم تطابق شرایط پت با امکانات خودرو |
| `TripCancelReason_Driver_Other` | سایر موارد |

### 10.2 گزینه‌های سفر

#### `GET /api/Driver/TripOption`
#### `GET /api/Driver/TripOption/{id}`

lookup فقط‌خواندنی برای تفسیر `tripOptions` هر سفر. list پاسخ search و detail پاسخ `BaseResultDto<TripOptionDto>` است. مدل هر مورد: `id`, `name`, `price`, `active`. `available` روی `active` فیلتر می‌کند؛ `sortBy=1` جدیدتر، `8` گران‌تر، `9` ارزان‌تر است. `q` در سرویس فعلی اعمال نمی‌شود. پنل راننده نباید آن‌ها را ایجاد یا ویرایش کند.

### 10.3 توقف‌های سفر

#### `GET /api/Driver/TripStop`
#### `GET /api/Driver/TripStop/{id}`

lookup فقط‌خواندنی برای `tripStop`/`stopMinutes`. شکل آیتم: `id`, `name`, `price`, `active`. `available` روی `active` فیلتر می‌کند؛ `sortBy=1/8/9` به‌ترتیب جدیدتر/گران‌تر/ارزان‌تر است و `q` اعمال نمی‌شود. در پنل راننده قابل ویرایش نیست.

## 11. قرارداد کامل خروجی سفر

### 11.1 `TripVDto` — خروجی available/current/list/مراحل/لغو

| گروه | فیلدها |
|---|---|
| هویت و مکان | `id`، `origin: PointDto`، `destination: PointDto`، `secondDestination: PointDto`، `fromAddress`، `toAddress`، `routeLength`، `fromCityId`، `fromCity` |
| زمان و نوع | `createDate`، `tripStartDateTime`، `isOnline`، `roundTrip`، `isReturnLeg`، `scheduledLeadMinutes`، `scheduledDepartureAt`، `progressUpdateDate` |
| وضعیت | `tripStatusId`/`tripStatus`، `driverStatusId`/`driverStatus`، `progressStageId`، `cancelInitiatorId`، `cancelReasonCodeId`/`cancelReasonCode`، `cancelReasonDetail` |
| راننده و کاربر | `driverId`/`driver`، `userId`/`user`، `passengerId`، `userPetId`/`userPet`، `userPets[]`، `ownerRidesAlong` |
| مالی | `price`، `paymentPrice`، `discount`، `driverShare`، `siteShare`، `isPaid`، `manualPayDate`، `fromWallet`، `walletPrice`، `rebateId`/`rebate`، `rebatePrice`، `wallet` |
| تنظیمات و اتصال | `vehicleTypeId`/`vehicleType`، `tripStop`، `stopTime`، `stopMinutes`، `tripOptions[]`، `companionReserveId`، `pansionReserveId`، `schoolReserveId`، `previousTripId` |
| متن‌های کاربر/فنی | `userDetail`، `userComment`، `userRate`، `connectionId`، `userToken` |

`PointDto` دقیقاً `x: number`، `y: number`، `distanceMeter: number` دارد. آدرس قابل نمایش را از `fromAddress`/`toAddress` بگیرید، نه از تبدیل خودسرانهٔ مختصات.

### 11.2 `TripDto` — خروجی GET جزئیات

هستهٔ فیلدها مشابه است، اما این تفاوت‌ها را رعایت کنید:

- `userPetIds: long[]` دارد، نه `userPets[]`؛
- `tripStopId` و `tripOptionIds[]` نیز دارد؛
- `rebateCode` دارد؛
- `manualPayDate`، `progressUpdateDate`، `stopTime`، `passengerId` و `wallet` ندارد؛
- nestedهای `driverStatus`، `tripStatus`، `user`، `cancelReasonCode`، `vehicleType`، `userPet`، `driver`، `fromCity`، `rebate`، `tripStop` و `tripOptions[]` ممکن است برگردند.

### 11.3 nestedهای قابل اتکا برای UI راننده

| مدل | فیلدهای قرارداد |
|---|---|
| `CodeVDto` | `id`, `name`, `label`, `value` |
| `TripOptionVDto` / `TripStopVDto` | `id`, `name`, `price`, `active` |
| `UserPetVDto` | `id`, `name`, `petId`, `userId`, `pictureId`, `petBreedId`, `petBreed2Id`, `isMixBreed`, `birthday`, `microChipCode`, `size`, `weight`, `isMale`, `isSterile`, `specificDisease`, `specificMedicene`, `addressValue`, `user`, `pet`, `picture`, `userPetRecords`, `userPetPictures` |
| `PetVDto` | `id`, `name`, `label`, `slug`, `priority` |
| `UserVDto` | `id`, `mobile`, `email`, `firstName`, `lastName`, `fullName`, `referralCode`, `registrationReferralSource`, `usedReferralCode`, referral ids، `pictureId`, `companionId`, `driverId`, `expertise`, `isCompanion`, `createDate`, `roleId`, `roleName`, `isFemale`, `picture`, `userCurrentLocation` |
| `DriverVDto` | تمام فیلدهای بخش 4.2 |
| `CityVDto` | `id`, `name`, `stateId`, `state` |

**اصل حریم داده:** صرف اینکه فیلدی در DTO آمده، به‌معنای نیاز UI به نمایش آن نیست. شماره تماس و اطلاعات پت را فقط در صفحهٔ سفر پذیرفته‌شده/تاریخچهٔ خود راننده و فقط به مقدار لازم نمایش دهید؛ tokenها و referral data را هرگز log یا render نکنید.

## 12. جریان پیشنهادی فرانت‌اند

1. پس از ورود راننده، `GET /Driver/Driver` را بزنید؛ اگر پروفایل نبود یا `statusId!=83` یا `active/approved` false بود، سفرهای قابل قبول را فعال نکنید و وضعیت تأیید/علت (`adminDetail`) را نمایش دهید.
2. در حالت فعال، `GET /Driver/TripUpcoming` و `GET /Driver/TripCurrent` را بگیرید. current ممکن است سفر آینده را برگرداند؛ بر مبنای `tripStartDateTime`، stage و لیست upcoming بین صفحهٔ «پیش‌رو» و «در حال اجرا» تصمیم بگیرید. `TripAvailable` را برای پیشنهادهای آزاد جدا بگیرید؛ empty بودن آن می‌تواند ناشی از پنجرهٔ مشغولی یک‌ساعته باشد.
3. قبول/رد را با `PUT /Driver/UpdateTripDriverStatus` انجام دهید. فقط پاسخ موفق قبول را شروع سفر بدانید. پس از failure، available list را refresh کنید.
4. در سفر accepted، GPS راننده را طبق بخش 9.2 ارسال و `GET /Driver/TripLive/{id}` را فقط در همان زمان poll کنید.
5. دکمهٔ اقدام را فقط برای stage بعدی نشان دهید؛ پس از هر PUT پاسخ Trip را جایگزین state کنید. برای round trip به `isReturnLeg` و تغییر آدرس‌ها توجه کنید.
6. برای لغو، ابتدا `Code` با گروه دلیل راننده را load کرده و سپس `TripDriverCancel` را بزنید. بعد از pickup دکمهٔ لغو را از UI حذف کنید.
7. در پایان (`Compeleted` یا `Canceled`) GPS/polling را قطع، سفر جاری را refresh و وضعیت را به تاریخچه منتقل کنید.

## 13. خطاها، edge caseها و چک‌لیست تست

| سناریو | انتظار فرانت‌اند |
|---|---|
| رانندهٔ دیگر زودتر سفر را قبول کند | `isSuccess=false` در UpdateTripDriverStatus؛ card را refresh/remove کنید |
| یک راننده قبلاً سفر را رد کرده باشد | آن سفر دیگر در TripAvailable او نیست |
| پروفایل پس از ویرایش | `active=false` و `approved=false`؛ صفحهٔ waiting approval |
| مرحله خارج از توالی | پاسخ ناموفق؛ state را از پاسخ/TripCurrent refresh کنید، نه اینکه مرحله را محلی جلو ببرید |
| لغو بعد از pickup | پاسخ ناموفق؛ هیچ fallback به TripChangeStatus نسازید |
| سفر برگشت | رسیدن به مقصد رفت، پایان نیست؛ origin/destination پاسخ را جایگزین کنید |
| GPS کاربر null | map/route اصلی با origin/destination همچنان کار کند؛ نبود GPS خطای UI نیست |
| سفر پذیرفته‌شدهٔ آینده | در `TripUpcoming` بر اساس زمان مرتب است؛ ممکن است `TripCurrent` هم همان را بدهد |
| کمتر از ۱ ساعت تا سفر پذیرفته‌شدهٔ دیگر | `TripAvailable` موفق ولی خالی؛ Accept سفر تازه با پیام `DriverHasUpcomingScheduledTrip` رد می‌شود |
| نوبت هفتگیِ فردا بدون راننده | به‌علت شرط فعلی job لغو خودکار، ممکن است حدود ۵ دقیقه بعد از ساخت لغو شود؛ وضعیت را refresh کنید |
| پرداخت دستی | API شرط وضعیت یا ثبت دوباره ندارد؛ UI آن را فقط طبق قاعدهٔ کسب‌وکار فعال کند و پس از موفقیت سفر را refresh کند. پاسخ، زمان قطعی ثبت‌شده را echo نمی‌کند. |
| دسترسی به Trip id دیگران | business failure/data خالی یا access denied؛ اطلاعات را cache نکنید |

### endpointهای خارج از محدودهٔ راننده

راننده در Area خود **سفر ایجاد نمی‌کند**، قیمت را محاسبه نمی‌کند، راننده انتخاب نمی‌کند، وضعیت تأیید پروفایل را تغییر نمی‌دهد و سفر دیگری را مدیریت نمی‌کند. از Areaهای دیگر فقط مسیرهای لازم برای حساب راننده، بارگذاری مدرک، موقعیت و اعلان در این سند آمده‌اند.

## ۱۴. اعلان‌ها و دسترسی‌های مشترک لازم برای پنل راننده

اعلان سفر از مسیرهای عمومی حساب کاربر استفاده می‌کند؛ API جدا با prefix `/api/Driver/Notifications` وجود ندارد. گیرندهٔ پوش، `ownerId` رکورد Driver است. در همهٔ کانال‌ها، دریافت پوش را فقط محرک refresh بدانید؛ وضعیت قطعی را دوباره از `Trip`, `TripUpcoming` یا `TripCurrent` بخوانید.

### ثبت دستگاه و صندوق اعلان

| Method | endpoint | درخواست/پاسخ مهم |
|---|---|---|
| `GET` | `/api/EndUser/push/public-key` | کلید VAPID عمومی برای Web Push؛ بدون توکن |
| `POST` | `/api/EndUser/push/subscribe` | Web Push: `{deviceKey, endpoint, keys:{p256dh,auth}, userAgent}`؛ برای اتصال به حساب راننده با توکن معتبر بفرستید. |
| `POST` | `/api/EndUser/push/subscribe-fcm` | اپ: `{deviceKey, fcmToken, platform, userAgent}`؛ platform در DTO برای `android`/`ios`/`windows` است. |
| `POST` | `/api/EndUser/push/attach` | `{deviceKey}`؛ دستگاه موجود را به `userId` توکن وصل می‌کند. |
| `POST` | `/api/EndUser/push/unsubscribe-fcm` | `{deviceKey}`؛ هنگام خروج حساب از دستگاه، اتصال FCM را جدا می‌کند. |
| `GET` | `/api/EndUser/notifications` | query مشترک صفحه‌بندی، `q`, `sortBy` و `unreadOnly: bool`؛ search object با `list[]`, `totalCount`, `unreadCount`. |
| `GET` | `/api/EndUser/notifications/unread-count` | عدد سادهٔ اعلان‌های خوانده‌نشده. |
| `POST` | `/api/EndUser/notifications/{id}/read` | علامت‌زدن یک اعلان؛ `BaseResultDto`، idempotent. |
| `POST` | `/api/EndUser/notifications/read-all` | علامت‌زدن همهٔ اعلان‌های همین کاربر؛ idempotent. |

هر آیتم Inbox شامل `id`, `title`, `body`, `url`, `icon`, `tag`, `type`, `createDate`, `sentDate`, `isRead`, `readDateUtc` است. نبود `sentDate` به معنی بی‌اعتبار بودن آیتم نیست؛ ممکن است Push تحویل نشده باشد ولی Inbox آن را داشته باشد. `unreadOnly=true` فقط نخوانده‌ها را برمی‌گرداند؛ `q` عنوان/متن را شامل‌جستجو می‌کند؛ `sortBy=2` قدیمی به جدید و بقیه جدید به قدیم است.

برای کلاینتی که SignalR لازم دارد، هاب احراز‌شدهٔ `/hubs/push` کاربر را به گروه خودش وصل می‌کند و event `push` با payload `{title, body, url, icon, tag, notificationId, type}` می‌فرستد. `notificationId` برای حذف تکرار بین FCM/Web Push/SignalR مفید است. هاب `/hubs/notices` فقط برای ادمین است و به راننده تعلق ندارد.

اعلان‌های مرتبط با راننده در کد: تأیید/رد پروفایل (`39/40`)، پیشنهاد سفر (`54`)، لغو توسط کاربر یا سیستم در مسیرهای مربوط (`58`/`53`) و یادآوری سفر پذیرفته‌شدهٔ نزدیک (`81`). بعضی رویدادهای مرحله و لغو راننده برای **کاربر صاحب سفر** ارسال می‌شوند؛ آن‌ها را به‌عنوان اعلان ورودی راننده نمایش ندهید.

### lookupهای فرم ثبت‌نام و مدارک

| endpoint | کاربرد |
|---|---|
| `GET /api/Common/City` یا `GET /api/Common/City/GetAll` | فهرست شهر؛ مسیر جستجو `pageSize=50` را در کنترلر اعمال می‌کند. |
| `GET /api/Neighborhood` | فهرست محلهٔ فعال؛ مقدار انتخاب‌شده باید متعلق به `cityId` باشد. |
| `POST {FILE_BASE_URL}/api/PictureUpload` | آپلود تصویر با `PictureFile`، توکن و `multipart/form-data`؛ خروجی `BaseResultDto<PictureDto>` و `data.id`. |
| `GET /api/Common/Picture/{id}` | خواندن متادیتای تصویر با توکن؛ خروجی `BaseResultDto<PictureVDto>`. |

`certificatePictureId` و `vehicleCardPictureId` در ثبت‌نام و ارسال مجدد اجباری‌اند؛ `profilePictureId` اختیاری است. `PictureUpload` حداکثر درخواست ۱۵ MiB دارد و مقدار/نوع واقعی فایل را سرور بررسی می‌کند؛ خطای حجم ممکن است HTTP `413` و خطای کسب‌وکاری فایل، HTTP `200` با `isSuccess=false` باشد.

## ۱۵. فهرست تمام endpointهای Area راننده

این جدول برای کنترل پوشش ۱۶ کنترلر و ۲۱ action است. همهٔ مسیرها prefix `/api/Driver` دارند و به توکن نیاز دارند؛ endpointهای مشترک بخش ۱۴ جدا هستند.

| Method | مسیر پس از `/api/Driver` | ورودی | نوع پاسخ | جایگاه در UI |
|---|---|---|---|---|
| GET | `/Driver` | `DriverInputDto` query | `DriverSearchDto` | پروفایل من |
| GET | `/Driver/{id}` | id | `BaseResultDto<DriverVDto>` | جزئیات پروفایل |
| PUT | `/Driver` | `DriverDto` | `BaseResultDto` | ویرایش و ارسال مجدد |
| GET | `/TripAvailable` | ندارد | `BaseResultDto<List<TripVDto>>` | پیشنهادهای آزاد |
| GET | `/TripUpcoming` | ندارد | `BaseResultDto<List<TripVDto>>` | سفرهای پذیرفته‌شدهٔ آینده |
| GET | `/TripCurrent` | ندارد | `BaseResultDto<TripVDto>` | یک سفر بازِ آنلاین |
| GET | `/Trip` | `TripInputDto` query | `TripSearchDto` | تاریخچه و فیلتر |
| GET | `/Trip/{id}` | id | `BaseResultDto<TripDto>` | جزئیات سفر خود راننده |
| PUT | `/UpdateTripDriverStatus` | `{id,driverStatusId}` | `BaseResultDto<TripDriverChangeStatusDto>` | قبول/رد پیشنهاد |
| PUT | `/TripArrivedOrigin/{id}` | id، بدون body | `BaseResultDto<TripVDto>` | رسیدن به مبدأ |
| PUT | `/TripPetPickedUp/{id}` | id، بدون body | `BaseResultDto<TripVDto>` | تحویل گرفتن پت |
| PUT | `/TripArrivedDestination/{id}` | id، بدون body | `BaseResultDto<TripVDto>` | رسیدن به مقصد/پایان یا شروع برگشت |
| PUT | `/TripDriverCancel` | `TripDriverCancelDto` | `BaseResultDto<TripVDto>` | لغو پیش از تحویل پت |
| PUT | `/ManualTripPayment` | `ManualPayTripDto` | `BaseResultDto<ManualPayTripDto>` | ثبت دریافت دستی |
| GET | `/TripLive/{id}` | id | `BaseResultDto<TripLiveDto>` | مکان کاربر در سفر پذیرفته‌شده |
| PUT | `/TripChangeStatus` | `TripChangeStatusDto` | `BaseResultDto<TripChangeStatusDto>` | مسیر legacy؛ برای flow معمول نمایش ندهید |
| GET | `/Code` | `CodeInputDto` query | `CodeSearchDto` | کدهای وضعیت/دلیل |
| GET | `/TripOption` | `TripOptionInputDto` query | `TripOptionSearchDto` | گزینه‌های سفر |
| GET | `/TripOption/{id}` | id | `BaseResultDto<TripOptionDto>` | جزئیات گزینه |
| GET | `/TripStop` | `TripStopInputDto` query | `TripStopSearchDto` | توقف‌ها |
| GET | `/TripStop/{id}` | id | `BaseResultDto<TripStopDto>` | جزئیات توقف |

در Area راننده API مستقل برای **کیف پول یا تسویهٔ راننده، برداشت، فاکتور، مدیریت امتیاز، ایجاد سفر، ویرایش مسیر/قیمت، ساخت سرویس هفتگی، برنامهٔ خام هفتگی، یا مشاهدهٔ جزئیات رزرو مادر** دیده نمی‌شود. ارقام `driverShare` و `siteShare` پس از تکمیل سفر در `TripVDto`/تاریخچه‌اند؛ سهم سایت برابر مبلغ پرداختیِ مثبت (وگرنه قیمت) ضربدر درصد کمیسیون راننده است و سهم راننده باقی آن. از این ارقام نباید endpoint تسویه یا موجودی حساب استنباط کرد.

## ۱۶. مواردی که باید در پیاده‌سازی به‌عنوان رفتار فعلی ثبت شوند

1. `TripAvailable` در نام «سفر آنی» دارد، اما سرویس، تاریخ‌دار/رزروی/هفتگی را حذف نمی‌کند؛ طبقه‌بندی کارت باید از زمان و شناسه‌های موجود Trip باشد، نه نام endpoint.
2. `TripCurrent` `FirstOrDefault` بدون مرتب‌سازی دارد و شرط موعد حرکت ندارد؛ اگر راننده چند سفر باز داشته باشد انتخاب رکورد آن قطعی نیست. `TripUpcoming` برای فهرست آینده لازم است.
3. `TripAvailable` و پذیرش سفر، شرط پنجرهٔ مشغولی یک‌ساعته را دارند. این شرط «سفر آیندهٔ پذیرفته‌شده با زمان کمتر یا مساوی اکنون+۱ ساعت» است و حد پایین ندارد؛ سفر معوقِ باز هم مانع می‌شود.
4. فیلتر `manualPay` در `GET /Trip` صرفاً وجود `manualPayDate` را بررسی می‌کند؛ حتی مقدار false همین شرط را اجرا می‌کند. `scheduledOrServiceOnly=false` فیلتر معکوس نمی‌سازد.
5. `ManualTripPayment` در service فقط شناسهٔ سفر را استفاده می‌کند و هر دو فیلد `isPaid`/`manualPay` ارسالی را نادیده می‌گیرد؛ کنترلر مالکیت را بررسی می‌کند، اما شرط وضعیت سفر یا جلوگیری از ثبت دوباره در این مسیر وجود ندارد. برای دریافت قطعی، پس از عملیات دادهٔ سفر را refresh کنید.
6. نوبت هفتگی به‌علت `scheduledDepartureAt=null` ممکن است در job لغو خودکار سفر فوری قرار بگیرد. تا اصلاح بک‌اند، برنامهٔ آینده را فقط از Tripهای واقعاً باز/پذیرفته‌شده نمایش دهید.
7. سرویس پذیرش پیشنهاد بر خلاف تخصیص دستی ادمین، `active`/`approved` راننده را جداگانه نمی‌سنجد؛ `driverId` توکن از status تأییدشده می‌آید. UI باید هر سه را gate کند و backend برای همسان‌سازی کنترل‌ها نیاز به بررسی دارد.

**منابع اصلی:** [`Api/Areas/Driver/Controllers`](../Api/Areas/Driver/Controllers)، [`TripService.cs`](../Application/Services/TripSrv/TripSrv/TripService.cs)، [`TripVDto.cs`](../Application/Services/TripSrv/TripSrv/Dto/TripVDto.cs)، [`TripInputDto.cs`](../Application/Services/TripSrv/TripSrv/Dto/TripInputDto.cs)، [`DriverService.cs`](../Application/Services/Accounting/DriverSrv/DriverService.cs)، [`PictureUploadController.cs`](../File/Controllers/PictureUploadController.cs)، [`PushController.cs`](../Api/Areas/EndUser/Controllers/PushController.cs) و [`NotificationsController.cs`](../Api/Areas/EndUser/Controllers/NotificationsController.cs).
