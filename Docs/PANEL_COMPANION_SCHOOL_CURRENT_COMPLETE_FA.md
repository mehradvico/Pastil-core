# مستند کامل و به‌روز پنل نماینده: مدرسه، دوره و ثبت‌نام

> مخاطب: فرانت‌اند پنل **نماینده / Companion**  
> مبنا: کد فعلی API در ۲۹ سپتامبر ۲۰۲۶  
> محدوده: فقط Area `Companion` و داده/عملیاتی که نمایندهٔ مالک مدرسه مجاز است انجام دهد.  
> مسیر پایه: `/api/Companion`

این سند شامل مدرسه، گالری، دوره، جلسات، ویدئو، پت‌های قابل‌پذیرش و ثبت‌نام‌های دوره است. عملیات کاربر نهایی (ثبت‌نام، پرداخت، کد تخفیف، لغو)، ادمین (تأیید/نمایش سایت/کمیسیون) و API عمومی عمداً در قرارداد عملیاتی این پنل نیستند.

## 1. قواعد کلیدی نقش نماینده

### چندمدرسه‌ای بودن

هر `Companion` می‌تواند **صفر، یک یا چند School** داشته باشد؛ در مدل یا سرویس سقف «یک مدرسه» وجود ندارد.

```text
Companion
 ├─ School A
 │   ├─ Course A-1
 │   └─ Course A-2
 └─ School B
     └─ Course B-1
```

`schoolId` باید در navigation/state پنل نگهداری شود. دوره، گالری، جلسه، ویدئو و ثبت‌نام همیشه زیرمجموعهٔ مدرسهٔ انتخاب‌شده‌اند.

### احراز هویت و مالکیت

تمام routeها نیاز دارند:

```http
Authorization: Bearer <companion-access-token>
Content-Type: application/json
```

- `companionId` برای فهرست‌ها از توکن اعمال می‌شود؛ آن را از query یا body قابل‌اعتماد ندانید.
- ایجاد School فقط وقتی ممکن است که Companion توکن `active=true` و `approved=true` باشد.
- برای detail/create/update دوره، محتوا، گالری و ثبت‌نام، سرور مالکیت School/Course را بررسی می‌کند؛ با این حال فیلتر `schoolId` را همیشه در list دوره‌ها بفرستید.
- اغلب خطاهای کسب‌وکاری HTTP `200` و بدنهٔ `isSuccess=false` دارند. `401/403` معمولاً مشکل توکن/اجازه است؛ `404` در برخی detailها برای مورد غیرمالک می‌آید.

### envelope پاسخ

عملیات تکی معمولاً این شکل را دارند:

```json
{
  "code": 0,
  "isSuccess": true,
  "messages": [],
  "data": {}
}
```

فهرست‌ها envelope `data` ندارند:

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "q": null,
  "sortBy": 1,
  "available": null,
  "totalCount": 0,
  "list": []
}
```

`pageIndex` حداقل 1، `pageSize` پیش‌فرض 20 و حداکثر 1000 است. `sortBy=1` جدیدترین و `sortBy=2` قدیمی‌ترین است، مگر آنجا که صراحتاً نوع دیگری گفته شده باشد.

## 2. فهرست کامل endpointهای پنل نماینده

| حوزه | Method | endpoint | کاربرد |
|---|---|---|---|
| مدرسه | GET | `/School` | فهرست مدرسه‌های خود نماینده |
| مدرسه | GET | `/School/{id}` | جزئیات یک مدرسهٔ خود |
| مدرسه | POST | `/School` | ایجاد مدرسهٔ جدید |
| مدرسه | PUT | `/School` | ویرایش مدرسهٔ خود |
| فعال‌سازی مدرسه | PUT | `/SchoolActive` | تغییر `active` مدرسهٔ خود |
| دوره | GET | `/SchoolCourse` | فهرست دوره‌ها؛ `schoolId` را بفرستید |
| دوره | GET | `/SchoolCourse/{id}` | جزئیات دورهٔ خود |
| دوره | POST | `/SchoolCourse` | ایجاد دوره |
| دوره | PUT | `/SchoolCourse` | ویرایش دورهٔ خود |
| دوره | PUT | `/SchoolCourse/active/{id}/{active}` | فعال/غیرفعال‌کردن دوره |
| جلسه | POST | `/SchoolCourseSession` | ساخت جلسه |
| جلسه | PUT | `/SchoolCourseSession` | ویرایش جلسه |
| جلسه | DELETE | `/SchoolCourseSession/{id}` | حذف نرم جلسه |
| ویدئو | POST | `/SchoolCourseVideo` | ساخت association ویدئو |
| ویدئو | PUT | `/SchoolCourseVideo` | ویرایش association ویدئو |
| ویدئو | DELETE | `/SchoolCourseVideo/{id}` | حذف نرم association |
| پت قابل‌پذیرش | GET | `/SchoolCoursePet` | فهرست محدودیت‌های پت دوره‌های خود |
| پت قابل‌پذیرش | GET | `/SchoolCoursePet/{id}` | detail یک مورد |
| پت قابل‌پذیرش | POST | `/SchoolCoursePet` | افزودن گونه/نژاد مجاز به دوره |
| پت قابل‌پذیرش | DELETE | `/SchoolCoursePet?id={id}` | حذف نرم گونه/نژاد مجاز |
| گالری | GET | `/SchoolPicture` | فهرست گالری‌های مدرسه‌های خود |
| گالری | GET | `/SchoolPicture/{id}` | detail یک تصویر |
| گالری | POST | `/SchoolPicture` | افزودن عکس به گالری |
| گالری | PUT | `/SchoolPicture` | ویرایش label عکس |
| گالری | DELETE | `/SchoolPicture?id={id}` | حذف نرم تصویر از گالری |
| ثبت‌نام | GET | `/SchoolReserve` | فهرست ثبت‌نام دوره‌های مدرسه‌های خود |
| ثبت‌نام | GET | `/SchoolReserve/{id}` | جزئیات ثبت‌نام خود |
| ثبت‌نام | PUT | `/SchoolReserve/{id}/Complete` | ثبت تکمیل‌شدن ثبت‌نام پرداخت‌شده |

هیچ DELETE برای School یا SchoolCourse و هیچ POST/PUT برای ساخت، پرداخت، لغو، تخفیف یا تغییر مالی ثبت‌نام در این Area وجود ندارد.

## 3. مدرسه (`School`)

### 3.1 قرارداد ورودی `SchoolDto`

| فیلد | نوع | کاربرد و مالک واقعی |
|---|---:|---|
| `id` | long | ساخت: 0/حذف؛ ویرایش: شناسهٔ مدرسهٔ خود |
| `name` | string | اجباری؛ نام مدرسه |
| `companionId` | long | سرور از توکن می‌گذارد؛ در فرم قابل ویرایش نیست |
| `stateId`, `cityId` | long | استان و شهر |
| `addressValue` | string | آدرس متنی |
| `discription` | string | معرفی مدرسه؛ املای API عمداً `discription` است |
| `pictureId` | long? | تصویر کاور از سرویس فایل |
| `regulations` | string | قوانین و شرایط شرکت |
| `active` | bool | با endpoint جدا تغییر می‌کند؛ در PUT عادی حفظ می‌شود |
| `approve`, `approvalValue` | bool, string | فقط ادمین؛ فقط خواندنی در Companion |
| `showToSite`, `suggested` | bool | فقط ادمین/سیستم؛ فقط خواندنی |
| `commentCount`, `rateAvg`, `rateCount` | number | آمار سیستمی؛ فقط خواندنی |

### 3.2 ایجاد مدرسه

#### `POST /api/Companion/School`

```json
{
  "name": "مدرسه تربیت پت نمونه",
  "stateId": 8,
  "cityId": 112,
  "addressValue": "تهران، ...",
  "discription": "آموزش رفتار و تربیت پت",
  "pictureId": 901,
  "regulations": "شرایط حضور و مدارک لازم"
}
```

اعتبارسنجی قطعی server: فقط `name` نباید خالی باشد و Companion باید فعال و تأییدشده باشد. با این حال UI باید state/city/address/regulations و کاور را برای مدرسهٔ قابل انتشار الزامی و از دادهٔ مرجع معتبر انتخاب کند.

پس از موفقیت، سرور بدون توجه به مقادیر body این مقدارها را می‌گذارد:

```text
active=false, approve=false, approvalValue=null,
showToSite=false, suggested=false,
commentCount=0, rateAvg=0, rateCount=0
```

پس POST موفق یعنی «ثبت شد و در انتظار تصمیم ادمین است»، نه انتشار یا فعال‌بودن.

### 3.3 فهرست و جزئیات

#### `GET /api/Companion/School`

سرور `companionId` را از توکن اعمال می‌کند. Queryهای پشتیبانی‌شده:

| query | نوع | معنی |
|---|---:|---|
| `approve` | bool? | تأیید ادمین |
| `available` | bool? | `active` مدرسه |
| `showToSite` | bool? | نمایش عمومی |
| `stateId`, `cityId` | long? | محدوده |
| `suggested` | bool? | برچسب ادمین |
| `q` | string | نام مدرسه |
| `sortBy` | int | 1 جدید، 2 قدیمی، 4 امتیاز بیشتر، 5 امتیاز کمتر |

#### `GET /api/Companion/School/{id}`

فقط School متعلق به Companion توکن. پاسخ `SchoolVDto` است و افزون بر فیلدهای `SchoolDto` شامل `companion`، `state`، `city`، `picture`، `schoolCourses[]` و `schoolPictures[]` است. `schoolCourses` در این پاسخ خلاصهٔ DTO دوره است؛ برای sessions/videos/capacity/summary هر دوره، detail دوره را جدا بخوانید.

### 3.4 ویرایش و فعال‌سازی

#### `PUT /api/Companion/School`

همان body `SchoolDto` با `id` معتبر. کنترلر پیش از update این مقدارها را با مقدار فعلی جایگزین می‌کند؛ تغییر آن‌ها از پنل نماینده ممکن نیست:

`companionId`, `approve`, `approvalValue`, `active`, `showToSite`, `suggested`, `commentCount`, `rateAvg`, `rateCount`.

ویرایش School در کد فعلی، آن را به pending بازنمی‌گرداند و خودکار غیرفعال نمی‌کند.

#### `PUT /api/Companion/SchoolActive`

```json
{ "id": 73, "active": true }
```

فقط مالک School می‌تواند آن را تغییر دهد. **نکتهٔ واقعی API:** endpoint شرط `approve=true` را enforce نمی‌کند؛ UI باید فعال‌سازی مدرسهٔ تأییدنشده را پنهان/غیرفعال کند تا lifecycle محصول رعایت شود.

## 4. دوره (`SchoolCourse`)

### 4.1 فیلدهای `SchoolCourseDto`

| فیلد | نوع | قانون |
|---|---:|---|
| `id` | long | ساخت 0/حذف؛ ویرایش شناسهٔ دورهٔ خود |
| `schoolId` | long | در ساخت ضروری و باید مدرسهٔ خود باشد؛ در ویرایش از رکورد فعلی حفظ می‌شود |
| `name` | string | اجباری |
| `discription` | string | شرح دوره؛ املای API همین است |
| `courseTypeId` | int | `1=Video`، `2=Live`، `3=InPerson` |
| `price` | double | در ساخت باید بزرگ‌تر از صفر باشد |
| `sessionCount` | int | در ساخت باید بزرگ‌تر از صفر باشد |
| `sessionDurationMinutes` | int | مدت هر جلسه؛ اعتبارسنجی مثبت‌بودن سمت server ندارد، UI مثبت ارسال کند |
| `capacity` | int | در ساخت باید بزرگ‌تر از صفر باشد |
| `petId`, `petBreedId` | long? | محدودیت تک‌گونه/تک‌نژاد قدیمی؛ null یعنی بدون این فیلتر |
| `commissionPercent` | decimal | فقط ادمین؛ create آن را 0 می‌کند و update مقدار ذخیره‌شده را حفظ می‌کند |
| `active` | bool | create سرور true می‌گذارد؛ تغییر رسمی با endpoint active |

### 4.2 ساخت، لیست، detail و edit

#### `POST /api/Companion/SchoolCourse`

```json
{
  "schoolId": 73,
  "name": "تربیت پایه سگ",
  "discription": "فرمان‌پذیری و اجتماعی‌سازی",
  "courseTypeId": 3,
  "price": 4500000,
  "sessionCount": 8,
  "sessionDurationMinutes": 90,
  "capacity": 12,
  "petId": 1,
  "petBreedId": null
}
```

کنترلر ابتدا مالکیت School را بررسی، `id=0` و `commissionPercent=0` می‌گذارد. سرویس نام، وجود School، price/sessionCount/capacity مثبت و معتبر بودن `courseTypeId` را کنترل می‌کند. در ایجاد `active=true` است.

#### `GET /api/Companion/SchoolCourse`

Query: `schoolId`, `courseTypeId`, `petId`, `available`, `q`, `pageIndex`, `pageSize`, `sortBy`. مرتب‌سازی: `8=Expensive` و `9=Inexpensive` نیز پشتیبانی می‌شوند.

همیشه `schoolId` مدرسهٔ انتخاب‌شده را بفرستید. کنترلر فقط وقتی `schoolId` داده شود مالکیت را بررسی می‌کند؛ بدون آن سرویس فیلتر مالکیت اعمال نمی‌کند. این یک شکاف API است و نباید با حذف فیلتر از فرانت دور زده شود.

#### `GET /api/Companion/SchoolCourse/{id}`

فقط دورهٔ مدرسهٔ خود نماینده. خروجی `SchoolCourseVDto` است.

#### `PUT /api/Companion/SchoolCourse`

قبل از update، مالکیت دورهٔ ذخیره‌شده بررسی و سپس `schoolId` و `commissionPercent` از رکورد فعلی بازنویسی می‌شوند؛ انتقال دوره یا تغییر کمیسیون ممکن نیست. اعتبارسنجی اختصاصی create برای price/capacity/type در update تکرار نشده است؛ UI همان قوانین create را در فرم edit هم enforce کند.

#### `PUT /api/Companion/SchoolCourse/active/{id}/{active}`

نمونه: `PUT /api/Companion/SchoolCourse/active/404/false`. فقط مالک دوره اجازه دارد.

### 4.3 خروجی کامل `SchoolCourseVDto`

| گروه | فیلدها |
|---|---|
| پایه | `id`, `name`, `schoolId`, `discription`, `courseTypeId`, `price`, `sessionCount`, `sessionDurationMinutes`, `capacity`, `active`, `commissionPercent` |
| محدودیت پت | `petId`, `petBreedId`, `pet`, `petBreed`, `acceptedPets[]` |
| وابستگی | `school` |
| محتوا | `schoolCourseSessions[]`, `schoolCourseVideos[]` |
| ظرفیت | `remainingCapacity` |
| خلاصهٔ زمان‌بندی محاسباتی | `isSingleSession`, `firstSessionDate`, `lastSessionDate`, `sessionsPerWeek`, `totalDurationWeeks` |

`remainingCapacity = capacity - تعداد ثبت‌نام لغونشده` است و در لحظه تغییر می‌کند؛ برای تصمیم نهایی ثبت‌نام منبع حقیقت نیست. summary از sessionهای حذف‌نشده محاسبه می‌شود، ذخیره نمی‌شود: اگر جلسه‌ای تعریف نشده باشد تاریخ‌ها null و اگر تنها یک session باشد `totalDurationWeeks=0` است.

## 5. پت‌های مورد پذیرش دوره (`SchoolCoursePet`)

این بخش مکمل `petId/petBreedId` قدیمی دوره است و چند گونه/نژاد مجاز را برای یک دوره تعریف می‌کند.

### قرارداد

```json
{
  "id": 0,
  "schoolCourseId": 404,
  "petId": 1,
  "petBreedId": 12
}
```

`SchoolCoursePetVDto` علاوه بر idها، `pet` و `petBreed` را برمی‌گرداند.

### APIها

- `GET /api/Companion/SchoolCoursePet?schoolCourseId=404&pageIndex=1&pageSize=20` — سرور `companionId` را از توکن scope می‌کند. فیلترهای `schoolCourseId`, `q`, pagination و sort مشترک‌اند.
- `GET /api/Companion/SchoolCoursePet/{id}` — detail. در controller فعلی ownership check صریح ندارد؛ پنل فقط idهای دریافت‌شده از لیست خود را درخواست کند. این مورد باید سمت بک‌اند هم اصلاح شود.
- `POST /api/Companion/SchoolCoursePet` — فقط اگر course متعلق به نماینده باشد. تکرار دقیق `(schoolCourseId, petId, petBreedId)` رد می‌شود.
- `DELETE /api/Companion/SchoolCoursePet?id={id}` — حذف نرم؛ id باید query باشد، نه route path.

در سرویس Insert، اعتبارسنجی وجود `petId`/`petBreedId` یا انطباق breed با pet انجام نمی‌شود؛ UI فقط از lookup معتبر انتخاب کند. در UI، `acceptedPets` را منبع جدیدتر محدودیت چندگانه بدانید و با فیلدهای تک‌پت دوره تناقض نسازید.

## 6. جلسات Live/InPerson (`SchoolCourseSession`)

جلسه list/detail مستقل ندارد؛ در `GET /SchoolCourse/{id}` داخل `schoolCourseSessions[]` می‌آید.

### body مشترک

```json
{
  "id": 0,
  "schoolCourseId": 404,
  "sessionDate": "2026-10-05T00:00:00",
  "startTime": "18:00",
  "endTime": "19:30",
  "meetingUrl": "https://meet.example.com/class-404-1",
  "active": true
}
```

| فیلد | توضیح |
|---|---|
| `id` | صفر=ساخت؛ بزرگ‌تر از صفر=ویرایش همان session همان course |
| `schoolCourseId` | دورهٔ خود نماینده |
| `sessionDate` | تاریخ calendar session |
| `startTime`, `endTime` | string؛ API فرمت/ترتیب را validate نمی‌کند؛ UI باید `HH:mm` و پایان بعد از شروع را enforce کند |
| `meetingUrl` | اختیاری؛ اگر پر باشد فقط URL مطلق `https` یا `http` مجاز است |
| `active` | فعال‌بودن session |

| Method | endpoint | رفتار |
|---|---|---|
| POST | `/SchoolCourseSession` | upsert؛ id=0 معمولاً ساخت |
| PUT | `/SchoolCourseSession` | همان upsert؛ id>0 فقط برای همان course |
| DELETE | `/SchoolCourseSession/{id}` | حذف نرم |

API نوع دوره، گذشته‌بودن تاریخ، تداخل sessions یا تطابق تعداد جلسه با `sessionCount` را enforce نمی‌کند. این‌ها را در UI کنترل کنید؛ برای تضمین هم‌زمانی/امنیت داده، validation بک‌اند لازم است.

## 7. ویدئوهای دوره (`SchoolCourseVideo`)

فایل ویدئو در endpointهای School آپلود نمی‌شود؛ ابتدا از سرویس فایل مجاز پروژه `fileId` بگیرید و فقط association آن را اینجا بسازید. نام/حجم/فرمت upload به سرویس فایل وابسته است و قرارداد School نیست.

```json
{
  "id": 0,
  "name": "جلسه اول: اصول ارتباط با سگ",
  "schoolCourseId": 404,
  "fileId": 9001,
  "sortOrder": 1,
  "durationSeconds": 1240,
  "active": true
}
```

| فیلد | توضیح |
|---|---|
| `id` | صفر=ساخت؛ بزرگ‌تر از صفر=ویرایش association همان course |
| `name` | عنوان ویدئو |
| `schoolCourseId` | دورهٔ خود نماینده |
| `fileId` | باید بزرگ‌تر از صفر باشد؛ شناسهٔ فایل آپلودشده |
| `sortOrder` | ترتیب نمایش؛ API یکتایی/مثبت‌بودن را کنترل نمی‌کند |
| `durationSeconds` | مدت اختیاری بر حسب ثانیه |
| `active` | نمایش‌پذیری محتوا |

| Method | endpoint |
|---|---|
| POST | `/SchoolCourseVideo` |
| PUT | `/SchoolCourseVideo` |
| DELETE | `/SchoolCourseVideo/{id}` |

POST و PUT upsert هستند و ownership course را کنترل می‌کنند. DELETE فقط association را soft-delete می‌کند؛ فایل اصلی حذف نمی‌شود.

## 8. گالری مدرسه (`SchoolPicture`)

گالری از تصویر کاور (`School.pictureId`) جداست و هر item صرفاً یک reference به `Picture` دارد. ترتیب نمایش برای گالری در DTO/API وجود ندارد.

```json
{
  "id": 0,
  "schoolId": 73,
  "pictureId": 901,
  "label": "فضای کلاس"
}
```

خروجی `SchoolPictureVDto`: `id`, `schoolId`, `pictureId`, `label`, `picture`. `picture` معمولاً `id`, `url`, `orginalName`, `baseUrl`, `guidName`, `extension` دارد.

| Method | endpoint | نکته |
|---|---|---|
| GET | `/SchoolPicture?schoolId=73&pageIndex=1&pageSize=20` | سرور companionId توکن را اعمال می‌کند؛ schoolId را نیز بفرستید |
| GET | `/SchoolPicture/{id}` | controller ownership check صریح ندارد؛ فقط idهای list خود را مصرف کنید |
| POST | `/SchoolPicture` | School باید متعلق به نماینده باشد |
| PUT | `/SchoolPicture` | SchoolId از item موجود حفظ می‌شود؛ انتقال بین مدرسه‌ها ممکن نیست |
| DELETE | `/SchoolPicture?id={id}` | حذف نرم؛ id query است |

سرویس insert/update اعتبارسنجی اختصاصی برای وجود `pictureId` یا label ندارد. UI باید فقط `pictureId` معتبر خروجی سرویس فایل را ارسال کند. در GET detail School نیز `schoolPictures[]` وجود دارد، اما endpoint list گالری برای pagination و مدیریت مناسب‌تر است.

## 9. ثبت‌نام‌ها (`SchoolReserve`)

نماینده در این Area ثبت‌نام جدید، پرداخت، لغو، تخفیف، کیف پول یا تغییر وضعیت دلخواه انجام نمی‌دهد. فقط می‌بیند و می‌تواند ثبت‌نام **پرداخت‌شده و لغونشدهٔ مدرسهٔ خود** را `Complete` کند.

### 9.1 فهرست و detail

#### `GET /api/Companion/SchoolReserve`

سرور `companionId` را از توکن اعمال می‌کند. Queryهای مجاز:

| query | نوع |
|---|---:|
| `bookerId` | long? |
| `schoolCourseId` | long? |
| `schoolId` | long? |
| `statusId` | int? |
| `pageIndex`, `pageSize`, `sortBy` | عمومی |

`SchoolReserveSearchDto` در خروجی query echo فقط `bookerId`, `schoolCourseId`, `statusId` را دارد، اما `schoolId` در query server اعمال می‌شود؛ نبود آن از scope Companion خارج نمی‌شود.

#### `GET /api/Companion/SchoolReserve/{id}`

فقط وقتی دورهٔ ثبت‌نام متعلق به Companion توکن باشد. خروجی `SchoolReserveVDto`:

| گروه | فیلدها |
|---|---|
| هویت | `id`, `reserveCode`, `schoolCourseId`, `bookerId`, `userPetId` |
| مالی | `price`, `paymentPrice`, `fromWallet`, `walletPrice`, `rebateId`, `rebatePrice`, `totalPrice` |
| وضعیت | `isReserved`, `isCancel`, `statusId`, `cancelDetail`, `createDate`, `cancelDate` |
| اشیا | `schoolCourse`, `userPet`, `booker` |
| پت‌رسان متصل | `petResanTrip` یا null |

کدهای وضعیت ثبت‌نام:

| `statusId` | label |
|---:|---|
| 1 | `Registered` — ایجادشده، هنوز پرداخت‌نشده |
| 2 | `Paid` — پرداخت‌شده |
| 3 | `Complete` — تکمیل‌شده |
| 4 | `Cancelled` — لغوشده |

`isReserved=true` در پرداخت موفق تنظیم می‌شود. هنگام لغو، `isCancel=true` و `statusId=4` است.

در `GET /SchoolReserve` لیست اصلی ثبت‌نام‌ها برمی‌گردد، اما `petResanTrip` و `totalPrice` فقط در سرویس detail به‌صورت صریح محاسبه/پر می‌شوند. برای نمایش مبلغ مجموع یا وضعیت سفر پت‌رسانِ یک ردیف، `GET /SchoolReserve/{id}` را بزنید؛ مقدار null/0 آن دو فیلد در list را منبع حقیقت ندانید.

### 9.2 سفر پت‌رسان متصل به ثبت‌نام مدرسه

در detail ثبت‌نام ممکن است `petResanTrip` برگردد. این فیلد nullable است و پرداخت سفر از پرداخت ثبت‌نام کاملاً جدا است.

```json
{
  "tripId": 520,
  "price": 280000,
  "paymentPrice": 250000,
  "isPaid": false,
  "tripStatusLabel": "...",
  "driverStatusLabel": "...",
  "driverId": 19,
  "driverName": "..."
}
```

`totalPrice = paymentPrice ثبت‌نام + (paymentPrice سفر اگر بزرگ‌تر از صفر باشد، وگرنه price سفر)`. این مقدار صرفاً نمایشی/گزارشی است؛ هر پرداخت جدا انجام می‌شود. نماینده از این پنل سفر را ایجاد یا مدیریت نمی‌کند.

### 9.3 کامل‌کردن دوره

#### `PUT /api/Companion/SchoolReserve/{id}/Complete`

بدنه ندارد. فقط وقتی موفق است که همهٔ شروط زیر برقرار باشند:

- ثبت‌نام متعلق به مدرسهٔ Companion توکن باشد؛
- `isReserved=true`؛
- `isCancel=false`؛
- `statusId=2` (`Paid`).

موفقیت وضعیت را به `statusId=3` (`Complete`) تغییر می‌دهد و پاسخ `BaseResultDto` دارد. برای Registered، Cancelled، Complete قبلی یا مورد غیرمالک پاسخ ناموفق می‌آید. دکمه را فقط برای Paid نشان دهید و پس از success list/detail را refresh کنید.

## 10. فیلدهای nested مهم

| مدل | فیلدهای قابل انتظار |
|---|---|
| `SchoolVDto` | تمام فیلدهای School + `companion`, `state`, `city`, `picture`, `schoolCourses`, `schoolPictures` |
| `SchoolCourseSessionDto` | `id`, `schoolCourseId`, `sessionDate`, `startTime`, `endTime`, `meetingUrl`, `active` |
| `SchoolCourseVideoDto` | `id`, `name`, `schoolCourseId`, `fileId`, `sortOrder`, `durationSeconds`, `active` |
| `SchoolCoursePetVDto` | `id`, `schoolCourseId`, `petId`, `petBreedId`, `pet`, `petBreed` |
| `UserPetVDto` | مشخصات پت، جنسیت، وزن/سایز، بیماری/دارو، تصویر، user و سوابق؛ فقط در context ثبت‌نام نمایش دهید |
| `UserMinVDto` | شناسه و مشخصات حداقلی رزروکننده؛ آن را به اندازهٔ لازم نمایش دهید |

از logکردن اطلاعات تماس/پزشکی پت و اطلاعات مالی کاربر پرهیز کنید. دادهٔ ثبت‌نام فقط در صفحات عملیاتی همان نماینده نمایش داده شود.

## 11. lifecycle و flow پیشنهادی پنل

```text
ایجاد School
  → active=false / approve=false / showToSite=false
  → تصمیم ادمین
  → مدرسهٔ تأییدشده
  → ایجاد Course (active=true, commissionPercent=0)
  → تعریف محدودیت پت، session یا video و gallery
  → کاربر ثبت‌نام و پرداخت می‌کند
  → Registered(1) → Paid(2)
  → Companion فقط Paid را Complete(3) می‌کند
```

ترتیب پیشنهادی UI:

1. `GET /School` و انتخاب school؛ وضعیت تأیید را آشکار نشان دهید.
2. `GET /SchoolCourse?schoolId=...`؛ دورهٔ جدید بسازید یا edit کنید.
3. برای هر course، `GET /SchoolCourse/{id}`؛ سپس session/video/accepted pet را مدیریت کنید.
4. گالری را از `/SchoolPicture?schoolId=...` مدیریت کنید؛ کاور را جدا نگه دارید.
5. صفحهٔ ثبت‌نام را با `/SchoolReserve?schoolId=...` بسازید؛ فقط دکمهٔ Complete برای Paid نشان دهید.
6. پس از هر mutation، پاسخ `isSuccess` را کنترل و دادهٔ مربوط (School/Course/Reserve) را refresh کنید.

## 12. چک‌لیست QA و محدودیت‌های شناخته‌شدهٔ API

- [ ] یک Companion می‌تواند چند School بسازد؛ state صفحه باید schoolId را جدا نگه دارد.
- [ ] POST School موفق، مدرسه را active/published تلقی نمی‌کند.
- [ ] نقش Companion نمی‌تواند approval، site visibility، suggested یا commission را تغییر دهد.
- [ ] `GET /SchoolCourse` همیشه با `schoolId` فراخوانی می‌شود؛ نبود آن شکاف scope فعلی server است.
- [ ] create و update Course هیچ‌گاه schoolId یا commission را جابه‌جا نمی‌کنند.
- [ ] `remainingCapacity` نمایش است و امکان race ثبت‌نام کاربر را از بین نمی‌برد.
- [ ] sessionها فقط URL http/https را server validate می‌کند؛ فرمت ساعت/هم‌پوشانی/تاریخ گذشته را UI کنترل می‌کند.
- [ ] video delete و session delete soft-delete هستند؛ حذف video فایل اصلی را پاک نمی‌کند.
- [ ] `SchoolCoursePet` تکراری دقیق را رد می‌کند، ولی API اعتبار مرجع گونه/نژاد را enforce نمی‌کند.
- [ ] GET detail برای SchoolPicture و SchoolCoursePet در controller ownership check صریح ندارد؛ فقط idهای لیست scoped خود را مصرف کنید. این مورد نیازمند اصلاح بک‌اند است.
- [ ] DELETE گالری و accepted pet از query parameter `id` استفاده می‌کنند.
- [ ] Complete فقط Paid و لغونشده را به Complete می‌برد؛ تغییر وضعیت دیگر در پنل نماینده مجاز نیست.
- [ ] HTTP 200 با `isSuccess=false` باید خطای UI محسوب شود.

## 13. فایل‌های مرجع کد

- `Api/Areas/Companion/Controllers/SchoolController.cs`
- `Api/Areas/Companion/Controllers/SchoolActiveController.cs`
- `Api/Areas/Companion/Controllers/SchoolCourseController.cs`
- `Api/Areas/Companion/Controllers/SchoolCourseSessionController.cs`
- `Api/Areas/Companion/Controllers/SchoolCourseVideoController.cs`
- `Api/Areas/Companion/Controllers/SchoolCoursePetController.cs`
- `Api/Areas/Companion/Controllers/SchoolPictureController.cs`
- `Api/Areas/Companion/Controllers/SchoolReserveController.cs`
- `Application/Services/SchoolSrvs/SchoolSrv/SchoolService.cs`
- `Application/Services/SchoolSrvs/SchoolCourseSrv/SchoolCourseService.cs`
- `Application/Services/SchoolSrvs/SchoolReserveSrv/SchoolReserveService.cs`
