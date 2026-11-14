# مستند تنظیمات پنل‌های Seller، Companion، Driver و Operator

**مرجع:** پیاده‌سازی بک‌اند در `Pastil-core/master`، commit `50c6c6f96b5527745ccab4daa32d41c56cbfdf20` (۲۰۲۶/۱۰/۱۰). این مخزن سورس رابط کاربری چهار پنل را ندارد؛ بنابراین نام تب‌ها و چیدمان صفحه را از خودمان حدس نمی‌زنیم. این سند تمام قراردادهای مرتبط با **پروفایل، حساب، تصاویر، اعلان، کارت بانکی و تنظیمات عملیاتی قابل ویرایش** را که در API همین مخزن تأیید شده‌اند شرح می‌دهد. اگر یک کنترل در UI پنل وجود دارد ولی در این API نیست، تنظیم محلی فرانت یا محصول دیگری است و باید با کد همان پنل سنجیده شود.

## ۱) نقشهٔ سریع؛ هر پنل تنظیماتش را کجا ذخیره می‌کند؟

| پنل | پروفایل/اطلاعات اصلی | تنظیمات دیگرِ تأییدشده | نتیجهٔ ذخیرهٔ پروفایل |
|---|---|---|---|
| **Seller** | `GET /api/Seller/Store/{storeId}` و `PUT /api/Seller/Store` | روش‌های ارسال `Seller/Delivery`، حساب مشترک `EndUser/User`، کارت بانکی و پوش | فیلدهای تأیید و امتیاز از مقدار ذخیره‌شده حفظ می‌شوند؛ `StoreDto` روی همان رکورد map و save می‌شود. |
| **Companion** | `GET /api/Companion/Companion/{id}` و `PUT /api/Companion/Companion` | ساعات مرکز/خدمت، منطقه فعالیت، مشاوره، پانسیون/مهد، مدرسه، اعضای تیم، حساب مشترک | **ارسال مجدد درخواست نمایندگی**؛ `active=false`, `approved=false`, `showToSite=false` تا بررسی دوبارهٔ ادمین. |
| **Driver** | `GET /api/Driver/Driver` یا `/{id}` و `PUT /api/Driver/Driver` | حساب مشترک، تصاویر مدارک/خودرو، کارت بانکی، پوش | **ارسال مجدد درخواست رانندگی**؛ `statusId=82`, `active=false`, `approved=false` تا تأیید دوباره. |
| **Operator** | در `api/Operator` **endpoint تنظیمات/ویرایش پروفایل ندارد** | حساب شخصی مشترک `EndUser/CurrentUser` و `EndUser/User`، رمز، موبایل/ایمیل، عکس، کارت بانکی و پوش | فقط رکورد `User` شخصی تغییر می‌کند؛ تنظیمات مرکز/راننده/فروشگاه از مسیر Operator وجود ندارد. |

همهٔ مسیرهای زیر، به‌جز `GET /api/EndUser/push/public-key` و ثبت اولیهٔ اشتراک پوش، توکن `Authorization: Bearer <token>` می‌خواهند. در بیشتر عملیات، HTTP 200 هم در موفقیت هم خطای تجاری برمی‌گردد؛ **`isSuccess` و `messages` را بخوانید**. بعضی مسیرها `403`/`404`/`400` واقعی هم دارند. نام JSON در نمونه‌ها `camelCase` است. `PUT`های پروفایل **PATCH نیستند**: مدل کامل DTO به موجودیت map می‌شود؛ برای حفظ فیلدها مقدار فعلی را بخوانید، فیلدهای ویرایشی را تغییر دهید و payload کامل مربوط به همان DTO را بفرستید.

## ۲) تنظیمات مشترک حساب شخصی هر چهار پنل

پنل‌ها از هویت کاربر مشترک استفاده می‌کنند. رکورد حساب `User` با پروفایل کسب‌وکار `Store`، `Companion` یا `Driver` فرق دارد. اپراتور فقط همین بخش مشترک را برای پروفایل خود دارد.

### ۲.۱ خواندن حساب و ویرایش نام/عکس/ویژگی‌های کاربر

| API | قرارداد |
|---|---|
| `GET /api/EndUser/CurrentUser` | `BaseResultDto<CurrentUserDto>`؛ منبع `userId`, `storeId`, `companionId`, `driverId`, نام، موبایل، ایمیل، عکس، نقش، `isFemale`, `expertise`, `walletAmount`. |
| `GET /api/EndUser/User/{id}` | `BaseResultDto<UserDto>`؛ **پارامتر `id` در کنترلر نادیده گرفته می‌شود** و همیشه کاربر توکن خوانده می‌شود. برای فرم ویرایش کامل، از این پاسخ استفاده کنید. |
| `PUT /api/EndUser/User` | بدنهٔ JSON از نوع `UserDto`. کنترلر `id` و `roleId` را از توکن تحمیل می‌کند، `mobile` را از کاربر جاری بازنویسی و `password=null` می‌کند. `locked=false` هم اجباری می‌شود. |

فیلدهای `UserDto` که برای فرم حساب اهمیت دارند: `firstName`, `lastName` (**لازم؛ فقط حروف فارسی و فاصله، ۲ تا ۵۰ کاراکتر**)، `email`, `mobile`، `pictureId`، `isFemale`، `expertise`، `twoFactorEnabled`. همچنین DTO دارای `id`, `roleId`, `locked`, `companionId`, `driverId`, `isCompanion` و چند فیلد ارجاع است؛ آن‌ها را به‌عنوان کنترل تنظیمات کاربر نمایش ندهید. Mapper فقط برخی فیلدهای ارجاع و رمز را نادیده می‌گیرد؛ برای جلوگیری از صفرشدن/تغییر ناخواستهٔ بقیه، خروجی `GET User/{id}` را مبنای payload کامل بگذارید. عکس پروفایل **`pictureId`** است، نه خود فایل یا URL. `expertise` در سرویس فقط برای مالک/عضو تأییدشدهٔ Companion معتبر است؛ برای دیگران ممکن است خطا یا پاک‌شدن مقدار رخ دهد.

`twoFactorEnabled` در `UserDto` هست و `PUT User` آن را به موجودیت map می‌کند؛ در بک‌اند فعلی endpoint مستقلی برای تنظیم جداگانهٔ 2FA در چهار Area دیده نشد. تغییر موبایل یا رمز را **هرگز** با همین `PUT` انجام ندهید؛ کنترلر آن‌ها را عمداً نادیده می‌گیرد.

**ناسازگاری مهم ایمیل:** برخلاف موبایل و رمز، کنترلر `PUT User` مقدار `email` بدنه را بازنویسی/مسدود نمی‌کند و سرویس آن را map می‌کند. در کنار مسیر OTPدار `ChangeEmail`، در نسخهٔ فعلی امکان تغییر مستقیم ایمیل از `PUT User` هم وجود دارد. این دو مسیر از نظر تأیید هویت یکسان نیستند؛ در UI مسیر OTPدار را برای تغییر ایمیل به کار ببرید و هنگام ذخیرهٔ سایر فیلدها، ایمیل فعلی را بدون تغییر نگه دارید.

نمونهٔ الگویی (فیلدهای دیگر را از `GET User/{id}` نگه دارید):

```http
PUT /api/EndUser/User
Authorization: Bearer <token>
Content-Type: application/json

{"firstName":"علی","lastName":"رضایی","email":"ali@example.com","pictureId":1044,"isFemale":false,"twoFactorEnabled":true}
```

نمونهٔ بالا **حداقلِ آموزشی** است؛ چون عملیات map کامل است، در محصول آن را با payload بازسازی‌شده از `GET User/{id}` بفرستید. بعد از پاسخ موفق، `GET CurrentUser` را دوباره بخوانید.

### ۲.۲ رمز، موبایل و ایمیل

| کار | متد و مسیر | ورودی و رفتار |
|---|---|---|
| تغییر رمز | `POST /api/Account/changepassword` | `{ "mobile": "شماره یا ایمیل ورود", "oldPassword": "...", "newPassword": "..." }`. رمز فعلی را می‌سنجد، رمز جدید را hash می‌کند و توکن را reset می‌کند. rate limit `AccountRecovery`. |
| درخواست تغییر موبایل | `POST /api/EndUser/ChangeMobile/changemobilerequest?mobile=09...` | پارامتر **query string** سادهٔ `mobile`، نه DTO در بدنه؛ کد OTP به موبایل جدید فرستاده می‌شود. |
| ثبت موبایل جدید | `POST /api/EndUser/ChangeMobile/changemobile` | `{ "mobile": "09...", "code": "..." }`؛ `userId` از توکن گرفته می‌شود؛ پس از تأیید OTP، توکن reset می‌شود. |
| درخواست تغییر ایمیل | `POST /api/EndUser/ChangeEmail/changeemailrequest?Email=...` | پارامتر سادهٔ `Email` از query؛ OTP ایمیل. |
| ثبت ایمیل جدید | `POST /api/EndUser/ChangeEmail/changeemail` | `{ "email": "...", "code": "..." }`؛ `userId` از توکن گرفته می‌شود. |

`Account/changepassword` روی کنترلر عمومی Account است و خودش `[Authorize]` ندارد، اما رمز قبلی را لازم دارد. در همهٔ درخواست‌های OTP، rate limit `OtpSend`/`OtpVerify` فعال است. بعد از reset توکن، نشست/توکن تازه را طبق پاسخ و جریان احراز هویت اپ ذخیره کنید.

### ۲.۳ آپلود عکس و شناسهٔ فایل

`POST /api/PictureUpload` روی سرویس **File** است، با `Authorization` و `multipart/form-data`، نام دقیق فیلد **`PictureFile`**. پاسخ موفق `BaseResultDto<PictureDto>` و `data.id` است. اول فایل را آپلود کنید؛ سپس `pictureId` / `iconId` / `backgroundPictureId` / `profilePictureId` / `certificatePictureId` / `vehicleCardPictureId` را در API تنظیمات مربوط بفرستید. فرستادن URL یا `multipart` به `PUT Store/Companion/Driver/User` قرارداد این API نیست.

حداکثر اندازهٔ درخواست ۱۵ MiB؛ تصویر `jpg/jpeg/png/webp` با حداکثر ۸۰۰۰×۸۰۰۰ و ۲۵ میلیون پیکسل. تصاویر به WebP تبدیل می‌شوند. فایل تصویری نامعتبر معمولاً پاسخ `isSuccess=false` دارد. شناسهٔ عکس قدیمی را هنگام ویرایش نگه دارید مگر کاربر واقعاً عکس را عوض کند.

### ۲.۴ اعلان دستگاه و کارت بانکی

| API | کاربرد/بدنه |
|---|---|
| `GET /api/EndUser/push/public-key` | کلید عمومی Web Push؛ بدون توکن. |
| `POST /api/EndUser/push/subscribe` | اشتراک وب: `{ deviceKey: UUID, endpoint, keys, userAgent }`؛ قابلیت فراخوانی بی‌توکن دارد. |
| `POST /api/EndUser/push/subscribe-fcm` | اپ: `{ deviceKey: UUID, fcmToken, platform: "android"|"ios"|"windows", userAgent }`. |
| `POST /api/EndUser/push/attach` | بعد از ورود، `{ "deviceKey": "UUID" }` را به کاربر توکن وصل می‌کند. |
| `POST /api/EndUser/push/unsubscribe-fcm` | هنگام خروج، همان `deviceKey` را از کاربر جدا می‌کند. |
| `GET /api/EndUser/notifications`، `/unread-count`، `POST /{id}/read`، `/read-all` | Inbox اعلان‌ها؛ این‌ها تنظیم «روشن/خاموش کردن نوع اعلان» نیستند. |
| `GET/POST/PUT/DELETE /api/EndUser/UserBankCard` | فهرست/ساخت/ویرایش/حذف کارت‌های **همان کاربر**؛ DTO: `id`, `cardNumber`, `shebaNumber`, `cardHolderName`, `bankCardId`. `userId` از توکن و `approved=false` از سرور تعیین می‌شود. |

در بانک‌کارت، شماره کارت باید ۱۶ رقم و BIN آن شناخته‌شده باشد؛ شبا ۲۴ رقم پس از `IR` پذیرفته می‌شود. ویرایش کارت برای غیرادمین فقط تا پیش از ویرایش قبلی مجاز است و تأیید را دوباره به `false` می‌برد. این تنظیم کارت **حساب شخصی** است؛ از روی این مسیر به‌تنهایی نمی‌توان فرض کرد هر تسویهٔ فروشگاه/نماینده/راننده به همین کارت می‌رود. در این بک‌اند endpoint اختصاصی ترجیح‌های اعلان به تفکیک نوع برای این چهار پنل پیدا نشد.

## ۳) Seller: تنظیمات فروشگاه

### ۳.۱ دریافت و ذخیرهٔ پروفایل فروشگاه

1. `GET /api/EndUser/CurrentUser` → `storeId` را بگیرید. یا `GET /api/EndUser/StoreUser`، فهرست فروشگاه‌های فعال کاربر را می‌دهد.
2. `GET /api/Seller/Store/{storeId}` → `data` برای فرم. اگر شناسهٔ مسیر متعلق به فروشگاه توکن نباشد `AccessDenied` در `BaseResultDto` می‌دهد.
3. تغییر فیلدهای مجاز و `PUT /api/Seller/Store` با JSON نوع **`StoreDto`**. کنترلر `id` را از `currentUser.storeId` بازنویسی می‌کند؛ شناسهٔ دلخواه کلاینت پذیرفته نیست. بعد از موفقیت `GET` را تکرار کنید.

**فیلدهای StoreDto:**

| گروه | فیلدها و قاعده |
|---|---|
| هویت/متن | `name` لازم؛ `summary`, `description`, `phone`, `mobile`, `email`, `address` |
| محل/نوع | `typeId` (شناسهٔ `Store_Type`)، `cityId`, `location: {x: longitude,y: latitude}` |
| تصویر | `pictureId`, `iconId`؛ فایل ابتدا در `PictureUpload` |
| آماده‌سازی سفارش | `maxPreparationMinutes` اختیاری. اگر ارسال نشود، Mapper مقدار قبلی را نگه می‌دارد؛ اگر ارسال شود به بازهٔ **۰ تا ۱۰۰۸۰ دقیقه** clamp می‌شود. |
| SEO | `seoH1`, `seoMinDescription`, `seoDescription`, `seoTitle`, `seoPictureAlt`, `seoUrlText`, `seoNoIndex`, `seoNoFollow`, `seoCanonical` |
| فقط سرور/ادمین | `referralCode`, `maxDiscountPercent`, `rateAvg`, `rateCount`, `active`, `approved`, `approvalValue`, `showToSite`, `users`, `picture` |

در `PUT Seller/Store` کنترلر مقدار ذخیره‌شدهٔ `referralCode/maxDiscountPercent/rateAvg/rateCount/active/approved/approvalValue/showToSite` را روی DTO برمی‌گرداند؛ Mapper نیز `users`, امتیازها، `maxDiscountPercent`, `picture/icon` آبجکتی و کد ارجاع را نادیده می‌گیرد. تغییر `pictureId/iconId` باید با شناسه انجام شود. `UpdateDto`، مدل را validate و نام تکراری را رد می‌کند، سپس به موجودیت map می‌کند؛ **payload ناقص می‌تواند بقیهٔ متن/SEO را تهی کند**. قوانین سخت‌گیرانهٔ درخواست ثبت فروشگاه در `EndUser/Store` را با رفتار `PUT Seller/Store` یکی فرض نکنید.

نمونهٔ ویرایش از روی مقدار خوانده‌شده:

```http
PUT /api/Seller/Store
Authorization: Bearer <token>
Content-Type: application/json

{"name":"فروشگاه نمونه","phone":"02112345678","mobile":"09120000000","email":"shop@example.com","address":"تهران، ...","typeId":12,"cityId":1,"location":{"x":51.389,"y":35.689},"pictureId":1042,"iconId":1043,"maxPreparationMinutes":60,"summary":"...","description":"..."}
```

این فقط نمونهٔ شکل درخواست است؛ شناسه‌های `typeId`, `cityId` و عکس باید از دادهٔ واقعی همان محیط گرفته شوند. اگر فروشگاه هنوز در مرحلهٔ درخواست/رد است، مسیر `GET/PUT /api/EndUser/Store` برای خواندن و **ارسال مجدد درخواست** است؛ `PUT EndUser/Store` برای فروشگاه تأییدشده رد می‌شود. `PUT Seller/Store` همان مسیر ویرایش فروشگاهِ صاحب توکن است و به‌خودی‌خود تأیید را reset نمی‌کند.

### ۳.۲ روش‌های ارسال فروشگاه

`GET /api/Seller/Delivery`, `GET /{id}`, `GET /types`, `POST`, `PUT`, `DELETE /api/Seller/Delivery?id=...`؛ همه به `storeId` توکن محدود شده‌اند و اگر فروشگاه فعال در توکن نباشد ممکن است HTTP 400 بدهند. برای ساخت `id` را نفرستید؛ برای ویرایش `id` همان رکورد را بفرستید. `DeliveryDto` شامل `deliveryTypeId`, `basePrice`, `minPriceForFree`, `minCountForFree`, `maxDays`, `cityId?`, `stateId?`, `active`, `afterRent`, `shippingProvider`, `livePricing`, `allowPrepaid`, `allowReceiverPay` است؛ `storeId` از سرور تعیین می‌شود. گزینه‌های معتبر نوع ارسال را از `/types` بگیرید؛ مقدار enum/provider را از قرارداد همان روش ارسال و دادهٔ خوانده‌شده نگه دارید. حذف و ویرایش فقط برای رکورد فروشگاه جاری مجاز است. تنظیم «حداکثر زمان آماده‌سازی» روی **StoreDto** است و از Delivery جداست.

## ۴) Companion: تنظیمات نماینده و مرکز

### ۴.۱ پروفایل اصلی نمایندگی

`GET /api/Companion/Companion/{id}` از `CurrentUser.CompanionId` می‌خواند و پارامتر مسیر را عملاً برای انتخاب مرکز دیگری به کار نمی‌برد. `PUT /api/Companion/Companion` بدنهٔ `CompanionDto` می‌گیرد، `ownerId` و `id` را از توکن پر می‌کند و `ResubmitAsyncDto` را اجرا می‌کند. برای نمایندگی منتظر بررسی/ردشده که `companionId` در `CurrentUser` ندارد، از `GET/PUT /api/EndUser/Companion` با مالکیت کاربر استفاده کنید.

**فیلدهای قابل ارسال CompanionDto:** `name`، `isPersonal`، `phone`، `cityId`، `neighborhoodId?`، `addressValue`، `location?`، `searchKey`، `pictureId?`، `backgroundPictureId?`، `iconId?`، به‌علاوهٔ `summary/description` و همان ۹ فیلد SEO بخش Seller. `name`, `phone` با الگوی `^\+?\d{10,13}$`, `cityId`, `addressValue` لازم‌اند؛ `neighborhoodId` باید متعلق به شهر باشد، شناسه‌های عکس باید موجود باشند، و مختصات در دامنهٔ طول/عرض جغرافیایی باشند. `companionPets` و `companionTypes` در DTO هستند اما Mapper این رابطه‌ها را در ویرایش پروفایل نادیده می‌گیرد؛ آن‌ها endpointهای جدا دارند.

**فیلدهای غیرقابل‌تنظیم از این فرم:** `ownerId`, `referralCode`, `goldAccountDate`, `silverAccountDate`, `active`, `approved`, `showToSite`, `activationValue`, `rateAvg/rateCount`, عکس‌های آبجکتی. کد ارجاع و تاریخ عضویت در سرویس حفظ می‌شوند؛ تأیید/فعالیت/نمایش سایت عمداً صفر می‌شوند. این endpoint «ویرایش بی‌اثر روی وضعیت» نیست: بعد از ذخیره، نمایندگی برای بررسی دوباره غیرفعال می‌شود. `CurrentUser.CompanionId` فقط نمایندگی **فعال و تأییدشده** را برمی‌گرداند؛ پس بعد از ذخیره ممکن است این مقدار `null` شود و مسیرهای `Companion` تا تأیید مجدد دسترس‌ناپذیر باشند. وضعیت درخواست را از `EndUser/Companion` پیگیری کنید.

نمونهٔ شکل ارسال (بقیهٔ فیلدهای متنی/SEO فعلی را حفظ کنید):

```json
{"id":25,"name":"کلینیک نمونه","isPersonal":false,"phone":"02112345678","cityId":1,"neighborhoodId":5,"addressValue":"تهران، ...","location":{"x":51.389,"y":35.689},"pictureId":1042,"backgroundPictureId":1043,"iconId":1044,"searchKey":"دامپزشکی"}
```

### ۴.۲ ساعات کاری مرکز و خدمات

| تنظیم | API و بدنه | رفتار ذخیره |
|---|---|---|
| ساعات عمومی مرکز | `GET /api/Companion/CompanionTime/{companionId}`؛ `POST /api/Companion/CompanionTime` با `{ companionId, companionTimeUpdateList:[{ weekDay:{id}, companionTimes:[{id,startTime,endTime,capacity,weekDayId,companionId}]}] }` | سرویس `companionId` را با شناسهٔ توکن می‌سنجد؛ `id=0` ردیف جدید می‌سازد، `id>0` را نگه می‌دارد و همهٔ ردیف‌های موجودِ **حذف‌شده از کل payload** را soft-delete می‌کند. زمان‌های جدید هم‌پوشان رد می‌شوند. |
| ساعات خدمت خاص | `GET /api/Companion/CompanionAssistanceTime/{companionAssistanceId}`؛ `POST /api/Companion/CompanionAssistanceTime` با `{ companionAssistanceId, companionAssistanceTimeUpdateList:[{ weekDay:{id}, companionAssistanceTimes:[{id,startTime,endTime,weekDayId,companionAssistanceId}]}] }` | همان الگوی حفظ `id>0`/ساخت `id=0`/حذف بقیه؛ مالکیت خدمت نسبت به مرکز بررسی می‌شود. `PUT` همین مسیر برای تغییر `active` یک `CompanionAssistanceTimeDto` است، نه ذخیرهٔ کل فهرست. |
| مشاورهٔ آنلاین قابل رزرو | `GET /api/Companion/ConsultationAvailability`؛ `PUT` با **آرایهٔ مستقیم** `[{"weekDayId":1,"start":"09:00","end":"13:00","capacity":1}]` | کل برنامهٔ هفتگی را جایگزین می‌کند. آرایهٔ خالی رزروهای آینده را می‌بندد؛ رزروهای ثبت‌شدهٔ قبلی تغییر نمی‌کنند. `weekDayId` ۱ شنبه تا ۷ جمعه، ساعت `HH:mm` و `end="24:00"` مجاز؛ ظرفیت ۱ تا ۲۰؛ هم‌پوشانی رد می‌شود. |

**برای دو `POST` ساعتی، فهرست کاملِ همهٔ روزها و همهٔ `id`های قابل حفظ را بفرستید؛ ارسال فقط روز ویرایش‌شده، ردیف‌های روزهای دیگر را حذف نرم می‌کند.** `GET CompanionDaySchedule` فقط نمای روز/رزروهاست و endpoint ذخیرهٔ تنظیمات نیست.

در این دو سرویس، ردیف با `id>0` فقط «حفظ» می‌شود؛ تغییر `startTime/endTime` همان `id` در این متد ذخیره نمی‌شود. برای تغییر ساعت، ردیف قبلی را از لیست حذف و ردیف تازه با `id=0` اضافه کنید. اعتبارسنجی هم‌پوشانی، ردیف جدید را با **ردیف‌های قدیمی دیتابیس** می‌سنجد؛ بنابراین جایگزینی با بازهٔ هم‌پوشانِ ساعت قبلی ممکن است رد شود. در این حالت پاسخ `isSuccess=false` را نشان دهید و فرض نکنید ذخیره انجام شده است.

### ۴.۳ محدوده، خدمات، تیم، پانسیون/مهد و مدرسه

| تنظیم | مسیر و فیلد اصلی |
|---|---|
| مناطق فعالیت | `GET/POST/PUT/DELETE /api/Companion/CompanionZone`؛ `CompanionZoneDto {id,cityId,neighborhoodId?}`؛ `companionId` در POST از توکن تحمیل و در ویرایش مالکیت بررسی می‌شود. |
| خدمات قابل ارائه | `GET/POST/PUT/DELETE /api/Companion/CompanionAssistance`؛ `CompanionAssistanceDto {id,assistanceId,companionTypeId,isSinglePackage,companionAssistanceTypeIds,...}`؛ `companionId` از توکن. تأیید و فعال‌سازی این مدل را با ویرایش پروفایل مرکز یکی نگیرید. |
| اعضای مرکز | `GET/POST/PUT/DELETE /api/Companion/CompanionUser`؛ `CompanionUserDto {id,phone,userId,expertiseId?,expertiseIds[],active,...}`. ساخت، `active=false` و `userAccept=null` ثبت می‌کند؛ این API حساب شخصی اپراتور را ویرایش نمی‌کند. |
| پانسیون/مهد | `GET/POST/PUT/DELETE /api/Companion/Pansion`؛ `PansionDto` شامل `name,isDaycare,isInfectious,stateId,cityId,addressValue,location,description (در DTO با املای discription),pictureId,pansionPrice,schoolPrice,regulations,openHour,closeHour`. فعال/غیرفعال: `PUT /api/Companion/PansionActive` با `{id,active}`. |
| مدرسه | `GET/POST/PUT/DELETE /api/Companion/School`؛ `SchoolDto` شامل `name,stateId,cityId,addressValue,location,discription,pictureId,regulations`. فعال/غیرفعال: `PUT /api/Companion/SchoolActive` با `{id,active}`. |

ویرایش پانسیون از `ResubmitAsyncDto` استفاده می‌کند و قواعد تأیید مخصوص خودش را دارد؛ ویرایش مدرسه کنترلر فیلدهای تأیید/فعالیت/امتیاز را از مقدار ذخیره‌شده حفظ می‌کند. این دو را با `PUT Companion/Companion` که تأیید **خود نمایندگی** را reset می‌کند اشتباه نگیرید. جزئیات رزرو، قیمت‌گذاری و اعتبارسنجی پانسیون/مدرسه در سند تخصصی خودشان آمده و تنظیم حساب شخصی نیستند.

## ۵) Driver: تنظیمات پروفایل و خودرو

`GET /api/Driver/Driver` با فیلتر `ownerId=UserId` توکن، فهرست رانندهٔ همان کاربر را می‌دهد؛ `GET /api/Driver/Driver/{id}` جزئیات رکورد متعلق به او را می‌دهد. اگر هنوز تأیید نشده و `CurrentUser.driverId=0` است، `GET /api/EndUser/Driver` و `/{id}` برای وضعیت درخواست در دسترس‌اند. `PUT /api/Driver/Driver` و `PUT /api/EndUser/Driver` هر دو **همان** `ResubmitAsyncDto` را اجرا می‌کنند.

| فیلد `DriverDto` | قاعدهٔ ارسال |
|---|---|
| `id` | شناسهٔ رکورد Driver موجود، اجباری برای `PUT`؛ با `userId` اشتباه نشود. |
| `name`, `phone`, `vehicle`, `licensePlateNumber` | همگی لازم. `phone` trim و ارقامش انگلیسی می‌شوند. |
| `cityId`, `neighborhoodId?` | شهر باید موجود باشد؛ محله اگر پر است باید در همان شهر باشد. |
| `certificatePictureId`, `vehicleCardPictureId` | لازم و باید شناسهٔ Picture واقعی باشند؛ ابتدا آپلود فایل. |
| `profilePictureId?`, `ownerDetail`, `vehicleTypeId?` | عکس پروفایل اختیاری؛ نوع خودرو را از دادهٔ کدهای معتبر بگیرید و مقدار قبلی را بی‌دلیل خالی نکنید. |
| `ownerId`, `statusId`, `active`, `approved`, `rate`, `adminDetail` | از کلاینت برای تغییر وضعیت استفاده نکنید: `ownerId` از توکن، `statusId=82`، `active/approved=false`، امتیاز قبلی حفظ و دلیل ادمین پاک می‌شود. |

نمونهٔ `PUT`:

```http
PUT /api/Driver/Driver
Authorization: Bearer <token>
Content-Type: application/json

{"id":87,"name":"علی رضایی","phone":"09120000000","vehicle":"پژو ۲۰۶","licensePlateNumber":"۱۱ ت ۲۲۳ ۶۷","cityId":1,"neighborhoodId":5,"certificatePictureId":201,"vehicleCardPictureId":202,"profilePictureId":203,"vehicleTypeId":301,"ownerDetail":"..."}
```

هر `PUT`، حتی تغییر سادهٔ عکس/توضیح، راننده را **Requested=82، غیرفعال و تأییدنشده** می‌کند. تا ادمین دوباره تأیید نکند، `CurrentUser.driverId` دیگر شناسهٔ معتبر راننده نیست و APIهای سفر کار نمی‌کنند. `DriverRequestStatus` حساب (۸۲/۸۳/۸۴) را با `Driver_Status` یک سفر (۷۹/۸۰/۸۱) اشتباه نگیرید. برای تنظیم سادهٔ نام/عکس **حساب شخصی** که تغییر مجوز رانندگی لازم ندارد، `PUT EndUser/User` و `pictureId` خود User را به کار برید؛ آن عکس با `profilePictureId` موجودیت Driver جداست. endpoint حذف Driver در Area راننده وجود ندارد.

## ۶) Operator: چه تنظیماتی واقعاً دارد؟

Area `api/Operator` در این نسخه فقط سه کنترلر دارد: `CompanionReserve` (مشاهده)، `CompanionReserveOperatorUpdate` (عملیات رزرو)، و `UserPetRecord` (سوابق پت). **هیچ `OperatorSettings`, `OperatorProfile`, `OperatorAvailability` یا `PUT Operator/User` در این Area نیست.** پس قرارداد تنظیمات حساب اپراتور همان بخش ۲ است: `CurrentUser`، `User/{id}`، `PUT User`، رمز/موبایل/ایمیل، عکس، پوش و کارت شخصی.

اگر اپراتور عضو تیم مرکز باشد، **عضویت، فعال‌سازی و تخصص‌های او** در `CompanionUserDto` و API نماینده/ادمین نگهداری می‌شود، نه از طریق endpoint خودویرایش Operator. `CurrentUser.isCompanionUser` و `canSetExpertise` برای خواندن وضعیت عضویت است. تغییر `expertise` کاربر با `PUT EndUser/User` فقط در شرایطی که سرویس اجازه می‌دهد انجام می‌شود؛ تغییر لیست `expertiseIds` عضویت با آن endpoint انجام نمی‌شود. برای تنظیم شیفت شخصی اپراتور endpoint اختصاصی در Area Operator دیده نشد؛ ساعات مرکز/خدمت در بخش Companion ذخیره می‌شوند.

## ۷) ترتیب درست ذخیره و خطاهای رایج

1. با `GET EndUser/CurrentUser` هویت و شناسهٔ رکورد پنل را بخوانید. برای Companion/Driver در وضعیت انتظار یا رد، از `EndUser/Companion` و `EndUser/Driver` رکورد درخواست را بازیابی کنید؛ `companionId/driverId` جاری ممکن است خالی باشد.
2. جزئیات **همان** موجودیت را با GET اختصاصی بگیرید. `userId`, `storeId`, `companionId`, `driverId`, `pansionId`, `schoolId` یکسان نیستند.
3. عکس تازه را جداگانه به File بفرستید و `data.id` را در DTO بگذارید. آدرس عکس را به جای `pictureId` نفرستید.
4. برای `PUT User/Store/Companion/Driver` از payload کاملِ فیلدهای خوانده‌شده ویرایش‌شده استفاده کنید. برای برنامهٔ ساعت‌های Companion، **تمام ردیف‌ها** را در درخواست جایگزینی نگه دارید.
5. `isSuccess`، `messages` و وضعیت HTTP را با هم بررسی کنید؛ فقط HTTP 200 یا toast محلی، نشانهٔ ذخیرهٔ موفق نیست. پس از ذخیره GET مجدد انجام دهید.
6. در UI قبل از ذخیرهٔ `Companion` یا `Driver` پیام روشن بدهید که پروفایل تا تأیید مجدد ادمین غیرفعال می‌شود. برای Seller چنین اثر مستقیمی در `PUT Seller/Store` نیست.
7. اگر بخشی مثل «ترجیحات اعلان»، «تم برنامه» یا «زبان» در UI دیده می‌شود، در این چهار Area و DTOهای بررسی‌شده قرارداد ذخیرهٔ سروریِ مستقلی ندارد؛ برای مستندسازی دقیق آن بخش باید سورس خود پنل بررسی شود.

## منابع اصلی کد

- [Seller StoreController](../Api/Areas/Seller/Controllers/StoreController.cs)، [StoreDto](../Application/Services/ProductSrvs/StoreSrv/Dto/StoreDto.cs)، [StoreService](../Application/Services/ProductSrvs/StoreSrv/StoreService.cs)، [DeliveryController](../Api/Areas/Seller/Controllers/DeliveryController.cs)
- [CompanionController](../Api/Areas/Companion/Controllers/CompanionController.cs)، [CompanionDto](../Application/Services/CompanionSrvs/CompanionSrv/Dto/CompanionDto.cs)، [CompanionService](../Application/Services/CompanionSrvs/CompanionSrv/CompanionService.cs)، [CompanionTimeService](../Application/Services/CompanionSrvs/CompanionTimeSrv/CompanionTimeService.cs)، [CompanionAssistanceTimeService](../Application/Services/CompanionSrvs/CompanionAssistanceTimeSrv/CompanionAssistanceTimeService.cs)
- [DriverController](../Api/Areas/Driver/Controllers/DriverController.cs)، [DriverDto](../Application/Services/Accounting/DriverSrv/Dto/DriverDto.cs)، [DriverService](../Application/Services/Accounting/DriverSrv/DriverService.cs)
- [Operator controllers](../Api/Areas/Operator/Controllers/CompanionReserveController.cs)، [CurrentUserController](../Api/Areas/EndUser/Controllers/CurrentUserController.cs)، [UserController](../Api/Areas/EndUser/Controllers/UserController.cs)، [UserDto](../Application/Services/Accounting/UserSrv/Dto/UserDto.cs)، [UserService](../Application/Services/Accounting/UserSrv/UserService.cs)
- [AccountController](../Api/Controllers/AccountController.cs)، [PushController](../Api/Areas/EndUser/Controllers/PushController.cs)، [UserBankCardController](../Api/Areas/EndUser/Controllers/UserBankCardController.cs)، [PictureUploadController](../File/Controllers/PictureUploadController.cs)، [AutoMapper configuration](../Application/Maping/AllMap.cs)
