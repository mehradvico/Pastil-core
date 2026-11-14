# مستند قرارداد فرم‌های درخواست و تأیید

**دامنه:** درخواست رانندگی، ثبت فروشگاه، ثبت نمایندگی، ثبت پانسیون/مهد، ثبت مدرسه، عضویت در تیم نماینده، فعال‌سازی خدمت و ثبت کارت بانکی. در انتها سایر APIهایی که نام «درخواست» دارند فهرست شده‌اند تا با فرم‌های تأسیس کسب‌وکار اشتباه نشوند.  
**منبع:** کد بک‌اند مخزن `mehradvico/Pastil-core`، شاخه `master`، بازبینی‌شده روی commit `50c6c6f96b5527745ccab4daa32d41c56cbfdf20` در ۱۰ اکتبر ۲۰۲۶.  
**مخاطب:** پیاده‌ساز فرم در اپ/پنل، تیم بک‌اند، تست و پشتیبانی. نام‌های انگلیسی فیلدها و مسیرها عین قرارداد کد هستند. برچسب‌های فارسی زیر پیشنهاد نمایش‌اند، نه متن الزام‌آور UI.

## ۱. نقشهٔ جریان‌ها

| درخواست | ثبت‌کننده و پیش‌نیاز | ثبت | پیگیری / جزئیات | ویرایش یا ارسال مجدد | تصمیم |
|---|---|---|---|---|---|
| راننده | کاربر واردشده | `POST /api/EndUser/Driver` | `GET /api/EndUser/Driver` و `/{id}` | `PUT /api/EndUser/Driver` | `PUT /api/Admin/DriverUpdateStatus` |
| فروشگاه | کاربر واردشده | `POST /api/EndUser/Store` | `GET /api/EndUser/Store` و `/{id}` | `PUT /api/EndUser/Store` فقط تا قبل از تأیید | `PUT /api/Admin/StoreApproval` |
| نمایندگی / کلینیک | کاربر واردشده | `POST /api/EndUser/Companion` | `GET /api/EndUser/Companion` و `/{id}` | `PUT /api/EndUser/Companion` | `PUT /api/Admin/CompanionActivation` |
| پانسیون یا مهد | نمایندهٔ فعال و تأییدشده | `POST /api/Companion/Pansion` | `GET /api/Companion/Pansion` و `/{id}` | `PUT /api/Companion/Pansion` | `PUT /api/Admin/PansionApprove` |
| مدرسه | نمایندهٔ فعال و تأییدشده | `POST /api/Companion/School` | `GET /api/Companion/School` و `/{id}` | `PUT /api/Companion/School` | `PUT /api/Admin/SchoolApprove` |
| عضویت کارمند/متخصص | دعوت از سوی نماینده، تصمیم با کاربر دعوت‌شده | `POST /api/Companion/CompanionUser` | `GET /api/Companion/CompanionUser` و `GET /api/EndUser/CompanionUser` | `PUT /api/Companion/CompanionUser` برای فعال‌سازی/تخصص | `PATCH /api/EndUser/CompanionUser/{id}/decision` |
| خدمت نماینده | نماینده | `POST /api/Companion/CompanionAssistance` | `GET /api/Companion/CompanionAssistance` و `/{id}` | `PUT /api/Companion/CompanionAssistance` | `PUT /api/Admin/CompanionAssistanceActivation` |
| کارت بانکی کاربر | کاربر واردشده | `POST /api/EndUser/UserBankCard` | `GET /api/EndUser/UserBankCard` و `/{id}` | `PUT /api/EndUser/UserBankCard` با محدودیت ویرایش | `PUT /api/Admin/UserBankCardApprove` |

این‌ها **هشت موجودیت مستقل** هستند؛ «درخواست پانسیون» به معنی درخواست ایجاد مرکز پانسیون/مهد است. رزرو اقامت مشتری (`PansionReserve`) جریان دیگری دارد و تأیید مرکز توسط ادمین به معنی تأیید رزرو مشتری نیست. همچنین وضعیت درخواست راننده با وضعیت سفر راننده فرق دارد.

### ترتیب وابستگی

1. کاربر احراز هویت می‌کند و توکن را در `Authorization: Bearer <token>` می‌فرستد.
2. شناسه‌های مرجع مانند شهر، نوع فروشگاه و تصاویر را دریافت/ایجاد می‌کند.
3. برای راننده، فروشگاه یا نمایندگی درخواست `EndUser` می‌فرستد؛ شناسهٔ مالک از توکن گرفته می‌شود.
4. بعد از تأیید و فعال‌شدن نمایندگی، نماینده می‌تواند پانسیون/مهد، مدرسه، عضویت کارکنان و خدمت ثبت کند.
5. در هر مرحله نتیجه را از API فهرست/جزئیات همان موجودیت بخوانید؛ وضعیت را از پاسخ ثبت اولیه حدس نزنید.

## ۲. قرارداد مشترک فرم‌ها

### ۲.۱ درخواست و پاسخ

- درخواست‌های JSON با `Content-Type: application/json` و توکن معتبر ارسال می‌شوند؛ آپلود تصویر استثنا و `multipart/form-data` است.
- در اکثر عملیات، پاسخ HTTP 200 شامل `BaseResultDto` است: `isSuccess`، `code`، `messages` و `data`. موفق‌بودن HTTP به‌تنهایی به معنی ثبت موفق نیست؛ `isSuccess` و پیام‌های اعتبارسنجی را بخوانید. جستجوها DTO صفحه‌بندی اختصاصی برمی‌گردانند و لزوماً همین پوشش را ندارند.
- در `POST` شناسهٔ `id` را نفرستید یا صفر بگذارید. در `PUT` شناسهٔ همان رکورد لازم است. برای اصلاح درخواست قبلی `POST` دوباره نفرستید؛ محدودیت تکرار وجود دارد.
- فیلدهای مدیریتی مانند `ownerId`، `companionId`، `approved`، `approve`، `active`، `statusId`، `approvalValue`، `adminDetail`، امتیاز، کمیسیون و `showToSite` در DTO دیده می‌شوند، اما دکمهٔ فرم متقاضی برای تغییر مستقیم آن‌ها نباید ساخته شود. بعضی در کنترلر و بعضی در سرویس بازنویسی می‌شوند.
- مقدارهای نمونهٔ عددی در این سند فقط شکل JSON را نشان می‌دهند. شناسهٔ واقعی را از محیط عملیاتی دریافت کنید.

### ۲.۲ انتخابگرهای فرم

| داده | منبع / نکته |
|---|---|
| استان و شهر | `GET /api/Common/State`، `GET /api/Common/City` یا `GET /api/Common/City/GetAll`. در پانسیون، شهر باید به استان انتخابی تعلق داشته باشد. |
| نوع فروشگاه | `GET /api/EndUser/Code?codeGroupLabel=Store_Type`؛ `typeId` باید کد فعال همین گروه باشد. |
| محله | `neighborhoodId` اختیاری است؛ اگر ارسال شود باید متعلق به `cityId` باشد. مقدار دلخواه یا محلهٔ شهر دیگر پذیرفته نمی‌شود. |
| نوع خودرو | `vehicleTypeId` در DTO راننده اختیاری است؛ شناسه را از فهرست کد/نوع معتبر محیط بگیرید و مقدار ثابت را در کلاینت حدس نزنید. |
| تخصص عضو تیم | شناسه‌های `expertiseIds` باید تخصص‌های فعال و موجود باشند. |
| تصویر و مدارک | ابتدا `POST /api/PictureUpload` با فیلد multipart دقیق `PictureFile`، سپس `data.id` پاسخ را در `pictureId` یا شناسهٔ مدرک بفرستید. سقف بدنه ۱۵ MiB است. تصویرهای jpg/jpeg/png/webp پذیرفته می‌شوند؛ حداکثر ابعاد ۸۰۰۰×۸۰۰۰ و حداکثر ۲۵ میلیون پیکسل. |
| مکان | `location: {"x": longitude, "y": latitude}`؛ `x` بین ۱۸۰- و ۱۸۰، `y` بین ۹۰- و ۹۰. فیلد داخلی `PointDto.Location` از همین دو عدد محاسبه می‌شود و در JSON ورودی لازم نیست. |

### ۲.۳ الگوی پیاده‌سازی فرم

1. گزینه‌ها و شناسه‌های وابسته را بارگذاری کنید؛ تصاویر را جداگانه آپلود و ID ذخیره کنید.
2. فقط فیلدهای قابل‌ویرایش را برای ثبت ارسال کنید؛ پاسخ `isSuccess=false` را به خطاهای همان فرم نگاشت کنید.
3. `id` برگشتی را نگه دارید و بعد از ثبت، فهرست یا جزئیات را دوباره دریافت کنید.
4. در صفحهٔ پیگیری، وضعیت، دلیل رد و امکان ویرایش همان درخواست را از دادهٔ سرور نمایش دهید.
5. بعد از رد، دادهٔ موجود را با `GET .../{id}` پیش‌پر کنید و با `PUT` همان رکورد را ارسال کنید. توجه کنید قواعد بازگشت به صف بررسی برای هر فرم متفاوت است.

## ۳. فرم درخواست راننده

**فیلدهای ورودی و اعتبارسنجی:**

| فیلد JSON | نوع | الزام / معنی |
|---|---|---|
| `name` | string | اجباری؛ نام راننده/عنوان درخواست. |
| `phone` | string | اجباری؛ سرویس آن را نرمال می‌کند. |
| `vehicle` | string | اجباری؛ شرح خودرو. |
| `licensePlateNumber` | string | اجباری؛ پلاک. |
| `cityId` | long | شهر معتبر، اجباری در منطق سرویس. |
| `neighborhoodId` | long? | اختیاری؛ در صورت وجود متعلق به شهر. |
| `certificatePictureId` | long | **مدرک گواهینامه**؛ شناسهٔ تصویر موجود، اجباری در منطق سرویس. |
| `vehicleCardPictureId` | long | **کارت خودرو**؛ شناسهٔ تصویر موجود، اجباری در منطق سرویس. |
| `profilePictureId` | long? | عکس پروفایل اختیاری؛ در صورت وجود باید معتبر باشد. |
| `vehicleTypeId` | long? | نوع وسیلهٔ نقلیه، اختیاری در DTO. |
| `ownerDetail` | string? | توضیح متقاضی؛ فیلد DTO، بدون الزام جداگانه در اعتبارسنجی درخواست. |

فیلدهای خروجی/مدیریتی DTO: `id`، `ownerId`، `statusId`، `adminDetail`، `approved`، `active` و `rate`. ثبت کاربر، `ownerId` را از توکن می‌گیرد؛ `statusId=82`، `active=false`، `approved=false`، `rate=0` و `adminDetail=null` می‌شود. برای هر مالک فقط یک درخواست رانندهٔ حذف‌نشده مجاز است.

```http
POST /api/EndUser/Driver
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "نام راننده",
  "phone": "09120000000",
  "vehicle": "وانت",
  "licensePlateNumber": "نمونه پلاک",
  "cityId": 1,
  "neighborhoodId": null,
  "certificatePictureId": 101,
  "vehicleCardPictureId": 102,
  "profilePictureId": 103,
  "vehicleTypeId": 5,
  "ownerDetail": "توضیح اختیاری"
}
```

| `statusId` | معنی | وضعیت عملیاتی |
|---:|---|---|
| 82 | درخواست‌شده | در انتظار بررسی؛ `approved=false` و `active=false`. |
| 83 | پذیرفته‌شده | `approved=true` و `active=true`؛ پس از به‌روزشدن هویت کاربر `DriverId` در context قابل‌استفاده است. |
| 84 | ردشده | `approved=false` و `active=false`؛ دلیل در `adminDetail`. |

**ارسال مجدد:** `PUT /api/EndUser/Driver` با `id` و فیلدهای معتبر فرم. مالک از توکن کنترل می‌شود؛ وضعیت به ۸۲ و تأیید/فعال‌بودن به false برمی‌گردد، `adminDetail` پاک می‌شود و امتیاز قبلی حفظ می‌شود. حتی ویرایش رانندهٔ تأییدشده دوباره او را در صف بررسی می‌گذارد.  
**تصمیم ادمین:** `PUT /api/Admin/DriverUpdateStatus` با `{"id":123,"statusId":83}` برای پذیرش یا `{"id":123,"statusId":84,"adminDetail":"علت رد"}` برای رد. دلیل رد الزامی است؛ مقدار ۸۲ برای این endpoint تصمیم ادمین نیست.

## ۴. فرم درخواست فروشگاه

**فیلدهای قابل ارسال:**

| فیلد | نوع | الزام / کاربرد |
|---|---|---|
| `name` | string | اجباری و یکتا میان فروشگاه‌های غیرحذف‌شده. |
| `phone` / `mobile` | string? | حداقل یکی اجباری؛ اگر `mobile` داده شود باید با `^\+?\d{10,13}$` مطابق باشد. |
| `address` | string | اجباری. |
| `typeId` | long | اجباری؛ کد فعال گروه `Store_Type`. |
| `cityId` | long | اجباری؛ شهر معتبر. |
| `location` | PointDto? | اختیاری؛ در صورت وجود مختصات معتبر. |
| `pictureId` / `iconId` | long? | اختیاری؛ در صورت وجود تصویر معتبر. |
| `email` | string? | ایمیل معرفی فروشگاه؛ در منطق درخواست اعتبارسنجی ویژه‌ای مشاهده نشد. |
| `summary` / `description` | string? | معرفی کوتاه/کامل؛ از DTO پایه. |
| `maxPreparationMinutes` | int? | سقف آماده‌سازی سفارش؛ در DTO هست، اما منطق درخواست آن را شرط پذیرش قرار نمی‌دهد. |
| فیلدهای SEO | string/bool | `seoH1`، `seoMinDescription`، `seoDescription`، `seoTitle`، `seoPictureAlt`، `seoUrlText`، `seoNoIndex`، `seoNoFollow`، `seoCanonical`. برای فرم اولیه اختیاری‌اند. |

DTO پاسخ افزون بر این‌ها شامل `id`، `referralCode`، `active`، `approved`، `approvalValue`، `showToSite`، `maxDiscountPercent`، `rateAvg`، `rateCount`، `picture` و `users` است. این‌ها ورودی تصمیم متقاضی نیستند. ثبت اولیه فروشگاه را غیرفعال، تأییدنشده و مخفی از سایت می‌سازد؛ کد معرفی تولید می‌کند و کاربر را به فروشگاه وصل می‌کند. برای هر کاربر فقط یک فروشگاه غیرحذف‌شده مجاز است.

```http
POST /api/EndUser/Store
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "فروشگاه نمونه",
  "phone": "02112345678",
  "mobile": "09120000000",
  "address": "نشانی فروشگاه",
  "typeId": 10,
  "cityId": 1,
  "location": {"x": 51.4, "y": 35.7},
  "pictureId": 201,
  "iconId": 202,
  "summary": "معرفی کوتاه"
}
```

**پیگیری:** `GET /api/EndUser/Store` فهرست فروشگاه‌های کاربر و `GET /api/EndUser/Store/{id}` جزئیات درخواست با کنترل مالکیت.  
**ارسال مجدد:** `PUT /api/EndUser/Store` با `id` و فیلدهای معتبر. نام یکتا و مالکیت دوباره بررسی می‌شوند. درخواست **قبلاً تأییدشده** از این مسیر قابل ارسال مجدد نیست؛ ویرایش پروفایل فروشگاه تأییدشده مسیر پنل فروشنده `PUT /api/Seller/Store` دارد. ارسال مجدد، `approved=false`، `active=false`، `approvalValue=null`، `showToSite=false` می‌کند و کد معرفی و داده‌های مدیریتی را نگه می‌دارد.  
**تصمیم ادمین:** `PUT /api/Admin/StoreApproval` با `{"id":123,"approved":true}` یا `{"id":123,"approved":false,"approvalValue":"علت رد"}`. دلیل رد لازم است. تأیید، `active=true` می‌کند؛ `showToSite` را خودکار true نمی‌کند، پس فعال‌بودن و نمایش عمومی را یکی نگیرید.

## ۵. فرم درخواست نمایندگی / کلینیک (`Companion`)

این فرم پیش‌نیاز ایجاد پانسیون، مدرسه و خدمات نماینده است. `isPersonal` مشخص می‌کند ثبت برای نمایندهٔ شخصی است یا سازمانی؛ در هر دو حالت قواعد زیر اجرا می‌شوند.

| فیلد | نوع | الزام / کاربرد |
|---|---|---|
| `name` | string | اجباری. |
| `isPersonal` | bool | نوع پروفایل؛ مقدار آگاهانه انتخاب شود. |
| `phone` | string | اجباری؛ الگوی `^\+?\d{10,13}$`. |
| `addressValue` | string | اجباری. |
| `cityId` | long | شهر معتبر، اجباری. |
| `neighborhoodId` | long? | اختیاری؛ باید متعلق به همان شهر باشد. |
| `location` | PointDto? | اختیاری؛ در صورت وجود مختصات معتبر. |
| `pictureId`، `backgroundPictureId`، `iconId` | long? | اختیاری؛ هر شناسهٔ فرستاده‌شده باید تصویر موجود باشد. |
| `searchKey` | string? | کلید جستجوی نماینده. |
| `summary`، `description` و فیلدهای SEO | string/bool | از DTO پایه؛ همان نام‌های بخش فروشگاه. |

فیلدهای `ownerId`، `referralCode`، `goldAccountDate`، `silverAccountDate`، `active`، `approved`، `showToSite` و `activationValue` در DTO هستند، اما وضعیت و مالکیت با بک‌اند تعیین می‌شوند. هر مالک فقط یک نمایندگی حذف‌نشده می‌تواند ثبت کند. ثبت اولیه `active=false`، `approved=false`، `showToSite=false`، `activationValue=null` و کد معرفی جدید دارد.

```http
POST /api/EndUser/Companion
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "مرکز نمونه",
  "isPersonal": false,
  "phone": "09120000000",
  "addressValue": "نشانی مرکز",
  "cityId": 1,
  "neighborhoodId": null,
  "location": {"x": 51.4, "y": 35.7},
  "pictureId": 301
}
```

**پیگیری:** `GET /api/EndUser/Companion` و `GET /api/EndUser/Companion/{id}` فقط دادهٔ مالک را نشان می‌دهند.  
**ارسال مجدد:** `PUT /api/EndUser/Companion` با `id` و فیلدهای معتبر. مالکیت بررسی می‌شود؛ `active`، `approved`، `showToSite` false و `activationValue` پاک می‌شوند. کد معرفی و تاریخ‌های عضویت حفظ می‌شوند. بنابراین ویرایش نمایندهٔ تأییدشده از این مسیر نیز تأیید را از بین می‌برد.  
**تصمیم ادمین:** `PUT /api/Admin/CompanionActivation` با `{"id":123,"approved":true,"active":true}` یا `{"id":123,"approved":false,"active":false,"activationValue":"علت رد"}`. دلیل رد اجباری است. در سرویس، `active` عملاً بر اساس `approved` تعیین می‌شود؛ این دو را در UI متناقض نفرستید. `CompanionId` در هویت عملیاتی پس از تأیید و فعال‌شدن قابل‌استفاده است.

## ۶. فرم درخواست پانسیون و مهد

`Pansion` یک موجودیت برای هر دو نوع است؛ `isDaycare=false` یعنی پانسیون/اقامت و `isDaycare=true` یعنی مهد روزانه. `isInfectious` فیلد جداگانهٔ پذیرش/خدمت عفونی است و جایگزین `isDaycare` نیست.

| فیلد | نوع | الزام / کاربرد |
|---|---|---|
| `name` | string | اجباری. |
| `isDaycare` | bool? | **الزامی در اعتبارسنجی**؛ مقدار true/false صریح بفرستید. |
| `stateId`، `cityId` | long | هر دو معتبر؛ شهر باید به استان تعلق داشته باشد. |
| `addressValue` | string | اجباری. |
| `location` | PointDto | اجباری و در محدودهٔ طول/عرض جغرافیایی. |
| `openHour`، `closeHour` | string | غیرخالی؛ در سرویس قالب سخت‌گیرانهٔ زمان بررسی نمی‌شود. قالب نمایشی مانند `08:00` مناسب است. |
| `pansionPrice` | double | اگر `isDaycare=false` باید بزرگ‌تر از صفر باشد. |
| `schoolPrice` | double | اگر `isDaycare=true` باید بزرگ‌تر از صفر باشد. |
| `isInfectious` | bool | ویژگی مرکز؛ مستقل از پانسیون/مهد. |
| `discription` | string? | **همین املای کد**؛ شرح مرکز. `description` این DTO نیست. |
| `regulations` | string? | قوانین اقامت/مهد. |
| `pictureId` | long? | اختیاری؛ در صورت وجود تصویر معتبر. |

DTO همچنین `id`، `companionId`، `active`، `showToSite`، `approve`، `approvalValue`، `commentCount`، `rateAvg`، `rateCount` و `suggested` دارد؛ این‌ها را از پاسخ بخوانید. شرط ثبت، وجود نمایندهٔ فعال و تأییدشدهٔ همان کاربر است. کنترلر `companionId` را از توکن می‌گذارد؛ ثبت اولیه `active=false`، `approve=false`، `approvalValue=null` و `showToSite=false` است.

```http
POST /api/Companion/Pansion
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "پانسیون نمونه",
  "isDaycare": false,
  "stateId": 1,
  "cityId": 10,
  "addressValue": "نشانی مرکز",
  "location": {"x": 51.4, "y": 35.7},
  "openHour": "08:00",
  "closeHour": "20:00",
  "pansionPrice": 500000,
  "schoolPrice": 0,
  "isInfectious": false,
  "discription": "معرفی مرکز",
  "regulations": "قوانین مرکز",
  "pictureId": 401
}
```

برای مهد، `isDaycare=true` و `schoolPrice>0` بفرستید؛ کد ثبت الزام نمی‌کند هر دو قیمت مثبت باشند.  
**پیگیری:** `GET /api/Companion/Pansion` و `GET /api/Companion/Pansion/{id}`؛ رکورد به نمایندهٔ جاری محدود می‌شود.  
**ارسال مجدد:** `PUT /api/Companion/Pansion` با `id`. مالکیت نماینده و اعتبار همهٔ فیلدهای الزامی دوباره بررسی می‌شود. `active=false`، `approve=false`، `showToSite=false`، `approvalValue=null` می‌شوند؛ حتی ویرایش مرکز تأییدشده دوباره آن را در صف تأیید می‌گذارد.  
**تصمیم ادمین:** `PUT /api/Admin/PansionApprove` با `{"id":123,"approve":true}` یا `{"id":123,"approve":false,"approvalValue":"علت رد"}`. دلیل رد لازم است. تأیید، `active=true` می‌کند اما `showToSite` را به‌تنهایی true نمی‌کند.  
**حذف:** `DELETE /api/Companion/Pansion?id=123` با کنترل مالکیت. تنظیم گالری و گونه‌های حیوان از موجودیت‌های جدا (`PansionPicture` و `PansionPet`) انجام می‌شود؛ بخشی از JSON ثبت اولیه نیست.

**نکتهٔ پاسخ خطا:** در مسیر ثبت پانسیون، وقتی `ValidateRequestAsync` ناموفق است، سرویس نتیجهٔ ناموفق را بدون انتقال جزئیات فیلدی checker برمی‌گرداند. کلاینت باید اعتبارسنجی سمت فرم انجام دهد و صرفاً به `messages` آن پاسخ برای تعیین فیلد معیوب تکیه نکند.

## ۷. فرم درخواست مدرسه

مدرسه (`School`) بعد از فعال‌شدن نمایندگی قابل ثبت است. فرم آن شبیه پانسیون است، اما در کد فعلی شرط‌های اعتبارسنجی یکسان نیستند.

| فیلد | نوع | توضیح |
|---|---|---|
| `name` | string | اجباری در ثبت. |
| `location` | PointDto | اجباری با مختصات معتبر. |
| `stateId`، `cityId` | long | در DTO وجود دارند؛ سرویس ثبت مدرسه رابطهٔ شهر/استان را مانند پانسیون بررسی نمی‌کند. برای دادهٔ درست، از انتخابگرهای معتبر استفاده کنید. |
| `addressValue` | string? | نشانی در DTO؛ اعتبارسنجی جداگانهٔ ثبت مانند پانسیون ندارد. |
| `discription`، `regulations` | string? | توضیح و قوانین؛ املای `discription` در قرارداد حفظ شده است. |
| `pictureId` | long? | تصویر مرکز؛ در منطق ثبت مدرسه اعتبارسنجی تصویر مانند پانسیون دیده نمی‌شود. |

`id`، `companionId`، `active`، `showToSite`، `approve`، `approvalValue`، `suggested` و آمار نظرات/امتیاز فیلدهای مدیریتی/خروجی‌اند. ثبت اولیه `active=false`، `approve=false`، `approvalValue=null`، `showToSite=false` است.

```http
POST /api/Companion/School
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "مدرسه نمونه",
  "stateId": 1,
  "cityId": 10,
  "addressValue": "نشانی مدرسه",
  "location": {"x": 51.4, "y": 35.7},
  "discription": "معرفی مدرسه",
  "regulations": "قوانین مدرسه",
  "pictureId": 501
}
```

**پیگیری:** `GET /api/Companion/School` و `GET /api/Companion/School/{id}`.  
**ویرایش:** `PUT /api/Companion/School` با `id` و دادهٔ جدید؛ برخلاف پانسیون، مسیر فعلی ویرایش مقادیر `approve`، `active` و `showToSite` موجود را حفظ می‌کند و به طور خودکار درخواست تازهٔ تأیید نمی‌سازد.  
**تصمیم ادمین:** `PUT /api/Admin/SchoolApprove` با `{"id":123,"approve":true}` یا `{"id":123,"approve":false,"approvalValue":"علت رد"}`. دلیل رد الزامی است؛ تأیید `active` و `showToSite` را true می‌کند.  
**حذف:** `DELETE /api/Companion/School?id=123` حذف نرم با کنترل مالکیت است؛ در صورت وجود ثبت‌نام بازِ پرداخت‌شده انجام نمی‌شود. `PUT /api/Companion/SchoolActive` برای تغییر فعال‌بودن مرکز است، نه ثبت درخواست تأیید.

## ۸. دعوت و درخواست عضویت در تیم نماینده

این فرم را **نماینده** برای کاربر موجود ثبت می‌کند و **خود کاربر دعوت‌شده** تصمیم می‌گیرد؛ درخواست خوداظهاری برای ایجاد نمایندگی نیست.

| فیلد | نوع | قاعده |
|---|---|---|
| `phone` | string? | تلفن کاربر ثبت‌شده؛ سرویس ارقام را انگلیسی و قالب `+98`/`98` را به `0` تبدیل می‌کند و کاربر را با موبایل پیدا می‌کند. |
| `userId` | long | جایگزین `phone`؛ اگر تلفن داده نشود باید شناسهٔ کاربر موجود باشد. |
| `expertiseIds` | long[] | ۱ تا ۱۰ تخصص فعال و معتبر؛ ترتیب حفظ می‌شود و اولی تخصص اصلی است. |
| `expertiseId` | long? | جایگزین سازگار با کلاینت قدیمی، اگر `expertiseIds` خالی باشد. |

`companionId` از توکن نماینده می‌آید؛ در ثبت `active=false` و `userAccept=null` تنظیم می‌شوند. برای یک جفت نماینده/کاربر فقط یک عضویت غیرحذف‌شده مجاز است.

```http
POST /api/Companion/CompanionUser
Authorization: Bearer <companion-token>
Content-Type: application/json

{"phone":"09120000000","expertiseIds":[11,12]}
```

نماینده دعوت‌ها را از `GET /api/Companion/CompanionUser` می‌بیند. کاربر دعوت‌های خودش را از `GET /api/EndUser/CompanionUser` دریافت می‌کند؛ این فهرست فیلتر شناسهٔ کاربر را از توکن اعمال می‌کند و همهٔ وضعیت‌های تصمیم را نشان می‌دهد. تصمیم پیشنهادی:

```http
PATCH /api/EndUser/CompanionUser/123/decision
Authorization: Bearer <invitee-token>
Content-Type: application/json

{"userAccept":true}
```

برای رد `false` بفرستید. مسیر قدیمی `POST /api/EndUser/CompanionUser` با `{"id":123,"userAccept":true|false}` نیز وجود دارد. سرویس مالکیت دعوت را کنترل می‌کند، فقط یک بار تصمیم می‌پذیرد، و پس از قبول `active=true` می‌شود؛ بعد از رد `active=false`. نماینده با `PUT /api/Companion/CompanionUser` می‌تواند `active` و تخصص‌ها را تغییر دهد. این جریان با ثبت حساب `operator` یا راننده یکی نیست.

## ۹. درخواست فعال‌سازی خدمت نماینده

`POST /api/Companion/CompanionAssistance` یک خدمت (`assistanceId`) را به نمایندگی جاری وصل می‌کند. DTO شامل `id`، `companionId`، `assistanceId`، `isSinglePackage`، `companionTypeId`، `companionAssistanceTypeIds`، `active`، `approved` و `activationValue` است. کنترلر `companionId` را از توکن و `active=false` می‌گذارد. سرویس ثبت، تکرار همان `assistanceId` برای همان نماینده را رد می‌کند؛ فهرست حالت‌های خدمت در کد فعلی برای کلاینت قدیمی پذیرفته می‌شود، اما حالت‌های عرضه اکنون از پکیج‌های فعال مشتق می‌شوند.

```json
{"assistanceId":25,"isSinglePackage":false,"companionTypeId":7,"companionAssistanceTypeIds":[]}
```

`GET /api/Companion/CompanionAssistance` و `/{id}` برای پیگیری و `PUT /api/Companion/CompanionAssistance` برای ویرایش‌اند. کنترلر در ویرایش نیز `active=false` می‌گذارد؛ سرویس `approved` را از ورودی غیرادمین تغییر نمی‌دهد. تصمیم در `PUT /api/Admin/CompanionAssistanceActivation` با `{"id":123,"approved":true,"active":true}` یا حالت رد و `activationValue` است. منطق این سرویس سه حالت برای ترکیب `active/approved` دارد؛ بنابراین وضعیت آن را با وضعیت دودویی فروشگاه یا پانسیون یکی نگیرید. برای فرم بسته‌ها، زمان‌بندی و نوع ارائهٔ خدمت، قراردادهای `CompanionAssistancePackage` و `CompanionAssistanceTime` جداگانه‌اند.

### فرم‌های زیرمجموعهٔ خدمت

بعد از ایجاد `CompanionAssistance`، سه فرم تأییددار دیگر در همین خانواده وجود دارد. این‌ها درخواست ایجاد حساب نماینده نیستند و هر کدام رکورد و تصمیم مستقلی دارند:

| فرم | مسیرهای نماینده | ورودی اصلی DTO | تصمیم مدیریتی |
|---|---|---|---|
| بستهٔ خدمت | `GET/POST/PUT/DELETE /api/Companion/CompanionAssistancePackage` و `GET .../{id}` | `name`، `companionAssistanceId`، `price`، `prePaymentPrice`، `pictureId`، `discription`، `petSize`، `types[]`؛ `active` و `activationValue` نیز در DTO هستند. | `PUT /api/Admin/CompanionAssistancePackageActivation` با `id`، `active`، `activationValue`. |
| اتصال کاربر/متخصص به خدمت | `GET/POST/PUT/DELETE /api/Companion/CompanionAssistanceUser` و `GET .../{id}` | `companionAssistanceId`، `userId`؛ `active` و `activationValue` هم در DTO هستند. | `PUT /api/Admin/CompanionAssistanceUserActivation` با `id`، `active`، `activationValue`. |
| بستهٔ بیمهٔ نماینده | `GET/POST/PUT/DELETE /api/Companion/CompanionInsurancePackage` و `GET .../{id}` | `name`، `summary`، `description`، `dayCount`، `companionId`، `price`، `petId`، `priority`؛ `active` و `activationValue` هم در DTO هستند. | `PUT /api/Admin/CompanionInsurancePackageActivation` با `id`، `active`، `activationValue`. |

وجود `active` در DTO به معنی اجازهٔ فعال‌سازی توسط متقاضی نیست؛ کنترلر و سرویس مربوط به هر فرم را برای رفتار نهایی وضعیت ملاک قرار دهید. برای `types[]` بستهٔ خدمت، ساختار هر آیتم در `CompanionAssistancePackageTypeDto` تعریف شده و قیمت هر حالت ارائه را حمل می‌کند؛ آن را با `companionAssistanceTypeIds` خدمت اصلی یکی نگیرید.

## ۹.۱. فرم ثبت و تأیید کارت بانکی

کارت بانکی حساب تجاری تازه نمی‌سازد، اما یک درخواست ثبت با بررسی ادمین است و برای تسویه اهمیت دارد.

| فیلد | نوع | قاعدهٔ ثبت |
|---|---|---|
| `cardHolderName` | string | اجباری. |
| `cardNumber` | string | اجباری؛ پس از حذف کاراکترهای غیررقمی باید دقیقاً ۱۶ رقم باشد. شش رقم ابتدایی باید با بانک موجود منطبق شود. کارت تکراری غیرحذف‌شده رد می‌شود. |
| `shebaNumber` | string? | اختیاری؛ در صورت ارسال، پیشوند `IR` و جداکننده‌ها نرمال می‌شوند و باید ۲۴ رقم باقی بماند. |
| `bankCardId` | long | در DTO وجود دارد، اما سرویس بانک را از BIN شماره کارت تعیین می‌کند؛ از ورودی کاربر برای انتخاب بانک تکیه نکنید. |

```http
POST /api/EndUser/UserBankCard
Authorization: Bearer <token>
Content-Type: application/json

{"cardHolderName":"نام دارنده کارت","cardNumber":"6037991234567890","shebaNumber":"IR123456789012345678901234"}
```

شماره‌های نمونه بالا صرفاً ساختار درخواست را نشان می‌دهند و برای ثبت واقعی معتبر فرض نشوند. `userId` از توکن گرفته می‌شود؛ درخواست جدید با `approved=false` و `adminDetail=null` ذخیره می‌شود. شماره کارت در ذخیره‌سازی محافظت می‌شود و تشخیص تکرار با شاخص جداگانه انجام می‌شود. `GET /api/EndUser/UserBankCard` و `/{id}` برای پیگیری است. `PUT /api/EndUser/UserBankCard` برای مالک فقط در نوبت نخست ویرایش (بر اساس برابری زمان ایجاد و آخرین ویرایش) مجاز است؛ پس از ویرایش `approved=false` می‌شود. `DELETE /api/EndUser/UserBankCard?id=123` با کنترل مالکیت وجود دارد. تصمیم ادمین: `PUT /api/Admin/UserBankCardApprove` با `{"id":123,"approved":true}` یا `{"id":123,"approved":false,"adminDetail":"علت رد"}`؛ رد بدون دلیل پذیرفته نمی‌شود.

## ۱۰. مدل نمایش وضعیت و تفاوت‌ها

| موجودیت | انتظار بررسی | تأیید | رد / دلیل | رفتار `PUT` متقاضی |
|---|---|---|---|---|
| راننده | `statusId=82` | `statusId=83`، `approved=true` | `statusId=84`، `adminDetail` | بازگشت به ۸۲ |
| فروشگاه | `approved=false` و `approvalValue=null` در ثبت | `approved=true` | `approved=false` و `approvalValue` | فقط تأییدنشده؛ بازگشت به انتظار |
| نمایندگی | `approved=false` و `activationValue=null` در ثبت | `approved=true` و `active=true` | `approved=false` و `activationValue` | بازگشت به انتظار، حتی پس از تأیید |
| پانسیون/مهد | `approve=false` و `approvalValue=null` در ثبت | `approve=true` | `approve=false` و `approvalValue` | بازگشت به انتظار، حتی پس از تأیید |
| مدرسه | `approve=false` و `approvalValue=null` در ثبت | `approve=true` | `approve=false` و `approvalValue` | وضعیت تأیید موجود حفظ می‌شود |
| عضویت تیم | `userAccept=null` | `userAccept=true` | `userAccept=false` | تصمیم دعوت‌شده فقط یک بار |

به‌جز راننده، اغلب موجودیت‌ها **enum صریح «منتظر بررسی» ندارند**. ترکیب بولین و متن دلیل در زمان ثبت برای نمایش وضعیت به‌کار می‌رود. اگر ادمین بعداً مرکز/فروشگاه تأییدشده را جداگانه غیرفعال کند، `active=false` الزاماً به معنی «رد درخواست» نیست؛ در UI `approved/approve` و متن دلیل را جدا از `active` نمایش دهید. `showToSite` نیز مستقل از تأیید است.

## ۱۱. خطاها و نکات پیاده‌سازی

| حالت | نتیجه / اقدام فرم |
|---|---|
| توکن یا نقش/شناسهٔ نماینده موجود نیست | `401/403` یا نتیجهٔ عدم دسترسی؛ فرم‌های پانسیون/مدرسه را تا تأیید نمایندگی فعال نکنید. |
| شناسهٔ رکورد متعلق به کاربر/نمایندهٔ فعلی نیست | در جزئیات راننده/نماینده ممکن است `404`؛ در عملیات دیگر نتیجهٔ `isSuccess=false` با عدم دسترسی. مالکیت را با تغییر `ownerId` یا `companionId` دور نزنید. |
| درخواست تکراری راننده/فروشگاه/نماینده | ثبت دوم رد می‌شود؛ رکورد موجود را بخوانید و اگر مجاز است با `PUT` اصلاح کنید. |
| نوع فروشگاه/شهر/محله/تصویر نامعتبر | ابتدا ID مرجع را از APIهای مرجع و آپلود موفق بگیرید؛ ID دستی ثابت نگذارید. |
| رد ادمین بدون دلیل | endpoint تصمیم، درخواست را ناموفق برمی‌گرداند؛ `approvalValue` یا `adminDetail` را پر کنید. |
| مختصات جابه‌جا | `x` طول جغرافیایی و `y` عرض جغرافیایی است؛ برای پانسیون و مدرسه ضروری است. |
| ویرایش مرکز تأییدشده | راننده، نمایندگی و پانسیون تأیید را از دست می‌دهند؛ فروشگاه از مسیر `EndUser` اجازه ندارد؛ مدرسه تأیید را نگه می‌دارد. دکمهٔ ویرایش و پیام هشدار هر فرم باید مطابق همین رفتار باشد. |
| `HTTP 200` همراه `isSuccess=false` | خطای کسب‌وکار است؛ نمایش پیام و جلوگیری از ورود به مرحلهٔ بعد. |

## ۱۲. سایر APIهای دارای مفهوم «درخواست»

این‌ها هم در مخزن وجود دارند، اما فرم تأسیس/عضویت بالا نیستند. قراردادشان را نباید از DTOهای Driver/Store/Pansion کپی کرد:

| موضوع | API ثبت یا اقدام | معنی |
|---|---|---|
| درخواست لغو سفارش محصول | `POST /api/EndUser/ProductOrderCancelRequest` با `ProductOrderDto`؛ در Seller/Admin نیز کنترلرهای متناظر وجود دارد | درخواست مرتبط با سفارش موجود؛ کنترلر کاربر `userId` و `cancelRequest` را تنظیم می‌کند. |
| درخواست تأیید پروفایل پاستیل‌مچ | `POST /api/EndUser/PastilMatchProfileVerificationRequest` با `pastilMatchProfileId` (یا `id` سازگار با کلاینت قدیمی) | بررسی پروفایل موجود. |
| درخواست مچ | `POST /api/EndUser/PastilMatchRequest`؛ `GET`/`DELETE` همان مسیر | درخواست بین دو پروفایل با `senderProfileId`، `receiverProfileId`، `pastilMatchGoalId` و توضیح. |
| درخواست انتخاب راننده توسط ادمین برای سفر | `PUT /api/EndUser/TripRequestAdminDriverSelection` با `id` سفر | اقدام روی سفر، نه درخواست عضویت راننده. |
| پیگیری میکروچیپ | `POST /api/PetMicrochip/Search` سپس `POST /api/PetMicrochip/FollowUp` | جستجو و درخواست پیگیری مستقل؛ API مدیریت درخواست‌ها در `api/Admin/PetMicrochipRequest` است. |

رزرو پانسیون، درخواست سفر و سفارش سرویس نیز جریان‌های تراکنشی مستقل دارند. این سند قرارداد **فرم‌های ورود/تأسیس/فعال‌سازی** را کامل می‌کند و این جریان‌های عملیاتی را فقط برای رفع ابهام فهرست کرده است.

## ۱۳. مرجع کد

- کنترلرهای ثبت: `Api/Areas/EndUser/Controllers/{Driver,Store,Companion}Controller.cs` و `Api/Areas/Companion/Controllers/{Pansion,School,CompanionUser,CompanionAssistance}Controller.cs`.
- کنترلرهای تصمیم: `Api/Areas/Admin/Controllers/{DriverUpdateStatus,StoreApproval,CompanionActivation,PansionApprove,SchoolApprove,CompanionAssistanceActivation}Controller.cs` و `Api/Areas/EndUser/Controllers/CompanionUserController.cs`.
- اعتبارسنجی و تغییر وضعیت: `Application/Services/Accounting/DriverSrv/DriverService.cs`، `Application/Services/ProductSrvs/StoreSrv/StoreService.cs`، `Application/Services/CompanionSrvs/{CompanionSrv,CompanionUserSrv,CompanionAssistanceSrv}/...Service.cs`، `Application/Services/PansionSrvs/PansionSrv/PansionService.cs` و `Application/Services/SchoolSrvs/SchoolSrv/SchoolService.cs`.
- تعریف فیلدها: DTOهای هم‌نام در پوشهٔ `Dto` این سرویس‌ها، به‌همراه `Application/Common/Dto/Field` و `Application/Common/Dto/LocationPoint/PointDto.cs`.
- فایل و دادهٔ مرجع: `File/Controllers/PictureUploadController.cs`، `Api/Areas/Common/Controllers/{State,City}Controller.cs` و `Api/Areas/EndUser/Controllers/CodeController.cs`.

> این سند قرارداد مشاهده‌شده در کد را توصیف می‌کند. اگر API منتشرشده روی محیط استقرار commit دیگری دارد، تفاوت رفتار باید با همان نسخهٔ مستقر سنجیده شود.
