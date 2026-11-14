# مستند کامل پانسیون و مهد پت برای پنل نماینده (`Companion`)

این سند **فقط قرارداد و رفتار پنل نماینده** را توضیح می‌دهد: ساخت و تنظیم مرکز، حالت اقامتی/مهد، پذیرش عفونی/غیرعفونی، تصاویر و گونه‌های پت، فهرست و جزئیات رزروهای ورودی، تغییر وضعیت و محدودیت لغو. مبنا، controllerها، DTOها و serviceهای فعلی همین مخزن است. نام `Pansion` و املای `discription` مطابق API حفظ شده‌اند.

## ۱. تصویر کلی و پاسخ به پرسش «تأیید رزرو»

نماینده ابتدا مرکز خود را ثبت می‌کند. مرکز در حالت در انتظار بررسی قرار می‌گیرد. پس از تأیید **خود مرکز**، نماینده می‌تواند آن را فعال یا غیرفعال کند و رزروهای آن را ببیند.

**در API فعلی پنل نماینده، endpoint جداگانه‌ای برای «قبول رزرو» یا «رد رزرو» وجود ندارد.** رزرو تازه `Registered=46` و `isReserved=false` است. پس از پرداخت موفق کاربر، سرور آن را `Paid=47` و `isReserved=true` می‌کند. نماینده می‌تواند وضعیت رزرو را تغییر دهد؛ کاربرد معمولِ ثبت پایان خدمت، `Complete=48` است. `Paid` نتیجهٔ پرداخت است، نه تأیید دستی پانسیون. `Complete` هم پذیرش اولیهٔ درخواست نیست.

| وضعیت قابل نمایش در پنل | دادهٔ تعیین‌کننده | معنی دقیق | اقدام منطقی نماینده |
|---|---|---|---|
| در انتظار پرداخت | `statusId=46`, `isReserved=false` | درخواست ثبت شده، پرداخت قطعی نیست | مشاهدهٔ اطلاعات؛ از عنوان «منتظر تأیید پانسیون» استفاده نشود |
| پرداخت‌شده | `statusId=47`, `isReserved=true`, `isCancel=false` | رزرو پس از callback موفق قطعی شده است | آماده‌سازی و ارائهٔ خدمت |
| تکمیل‌شده | `statusId=48` | وضعیت پایان خدمت ثبت شده است | مشاهدهٔ سوابق؛ اگر پرداخت‌شده و لغونشده بوده، فرایند امتیاز/یادآوری نظر اجرا می‌شود |
| لغوشده | `isCancel=true`، صرف‌نظر از `statusId` | لغو فیلد مستقلی است، status جدا ندارد | نمایش علت و زمان لغو؛ لغو معمول در اختیار نماینده نیست |

**نکتهٔ مهم پیاده‌سازی:** endpoint تغییر وضعیت در service، انتقال `47→48` را enforce نمی‌کند؛ هر `statusId` ارسالی را پس از چند کنترل محدود ذخیره می‌کند. بنابراین پنل باید فقط اقدام معنادار نشان دهد، اما این محدودیت UI جای اعتبارسنجی backend را نمی‌گیرد. برای «تأیید/رد دستی رزرو» باید قرارداد جدید backend طراحی شود.

## ۲. دسترسی، پاسخ‌ها و قواعد مشترک

- همهٔ endpointهای پنل در این سند `[Authorize]` دارند و Bearer token می‌خواهند. `CompanionId` مرکز جاری از token خوانده می‌شود.
- در فهرست پانسیون و رزرو، `companionId` ورودی client با مقدار token جایگزین می‌شود. در ساخت/ویرایش پانسیون نیز `companionId` body با مقدار token جایگزین می‌شود. این شناسه را در UI قابل‌ویرایش نکنید.
- در اکثر actionها حتی خطای کسب‌وکار با HTTP `200` و `isSuccess=false` برمی‌گردد. برای عملیات تغییردهنده `isSuccess`, `messages`, `data`, `code` را بررسی کنید. GET فهرست، پاسخ صفحه‌بندی‌شدهٔ `list` و `totalCount` می‌دهد.
- `pageIndex` از ۱ شروع می‌شود، پیش‌فرض ۱ است؛ `pageSize` پیش‌فرض ۲۰ و سقف آن ۱۰۰۰ است. `sortBy=1` جدیدترین و `sortBy=2` قدیمی‌ترین بر اساس `id` است.
- اگر token مقدار `CompanionId` نداشته باشد، بعضی actionهای رزرو در سرویس فیلتر مالکیت را اعمال نمی‌کنند. رابط باید دسترسی بدون Companion فعال را مسدود کند و این نقص باید در backend اصلاح شود.

## ۳. چهار حالت مرکز: پانسیون/مهد × عفونی/غیرعفونی

| `isDaycare` | `isInfectious` | عنوان مناسب پنل | قیمت مبنا | رزرو قابل ثبت |
|---:|---:|---|---|---|
| `false` | `false` | پانسیون اقامتی عادی | `pansionPrice` برای هر روز | پت بدون `specificDisease` ثبت‌شده؛ بازهٔ `fromDate` تا `toDate` |
| `false` | `true` | پانسیون اقامتی عفونی | `pansionPrice` برای هر روز | فقط پت دارای `specificDisease` غیرخالی؛ بازهٔ تاریخ |
| `true` | `false` | مهد پت عادی | `schoolPrice` برای هر ساعت | پت بدون بیماری ثبت‌شده؛ `schoolCreateDate` و ساعت شروع/پایان |
| `true` | `true` | مهد پت عفونی | `schoolPrice` برای هر ساعت | فقط پت دارای بیماری ثبت‌شده؛ روز و بازهٔ ساعت |

هیچ قاعده‌ای در backend ترکیب مهد و عفونی را ممنوع نمی‌کند. `isInfectious` به معنی «پذیرش هم‌زمان سالم و بیمار» نیست: مقدار `true` فقط پت بیمار و `false` فقط پت سالم را می‌پذیرد. معیار بیماری در زمان ثبت رزرو، خالی یا غیرخالی بودن `UserPet.SpecificDisease` است، نه تشخیص مستقل کلینیکی در این سرویس. تغییر نوع/عفونی بودن پانسیون، رزروهای قبلی را خودکار تبدیل نمی‌کند؛ روی رزروهای بعدی اثر می‌گذارد.

`PansionPet` فهرست گونه‌های قابل نمایش/فیلتر کاتالوگ است. سرویس ساخت رزرو در حال حاضر `UserPet.PetId` را با اتصالات `PansionPet` تطبیق نمی‌دهد. ظرفیت کل، اتاق، تخت، سقف هم‌زمان و تعداد جای باقیمانده در مدل/API وجود ندارد. کنترل هم‌پوشانی فقط برای **همان پت و همان پانسیون** و رزرو قطعی لغونشده انجام می‌شود؛ از آن، ظرفیت واقعی استخراج نکنید.

## ۴. API مدیریت مرکز در پنل نماینده

| روش | مسیر دقیق | ورودی | نتیجه |
|---|---|---|---|
| GET | `/api/Companion/Pansion` | فیلترهای `PansionInputDto` در query | فهرست مراکز همین Companion؛ حذف‌شده‌ها در این لیست نیستند |
| GET | `/api/Companion/Pansion/{id}` | شناسهٔ مرکز | `BaseResultDto<PansionVDto>`؛ مرکز غیرمتعلق 404 |
| POST | `/api/Companion/Pansion` | `PansionDto` | ثبت مرکز جدید و ارسال برای بررسی |
| PUT | `/api/Companion/Pansion` | `PansionDto` با `id` | ویرایش و ارسال مجدد برای بررسی |
| PUT | `/api/Companion/PansionActive` | `{ "id":73, "active":false }` | روشن/خاموش کردن `active` مرکز غیرحذف‌شده |
| DELETE | `/api/Companion/Pansion?id=73` | `id` در query | حذف نرم، فقط مالک Companion و بدون رزرو باز پرداخت‌شده |

### ۴.۱. فیلدهای فرم ساخت و ویرایش (`PansionDto`)

| فیلد JSON | نوع | کاربرد / اعتبارسنجی واقعی |
|---|---|---|
| `id` | `long` | شناسهٔ مرکز؛ برای ساخت صفر/حذف از body، برای ویرایش مثبت |
| `name` | string | نام مرکز؛ غیرخالی و trim می‌شود |
| `isDaycare` | `bool?` | الزامی؛ `false` اقامتی، `true` مهد |
| `isInfectious` | `bool` | فقط پذیرش پت بیمار در `true`، فقط پت بدون بیماری ثبت‌شده در `false` |
| `stateId`, `cityId` | `long` | استان باید موجود باشد؛ شهر باید متعلق به همان استان باشد |
| `addressValue` | string | آدرس؛ غیرخالی و trim می‌شود |
| `location` | object | **الزامی**؛ `{ "x": طول جغرافیایی, "y": عرض جغرافیایی }`، x بین ‎−۱۸۰ تا ۱۸۰، y بین ‎−۹۰ تا ۹۰ |
| `discription` | string | متن معرفی؛ املای API همین است؛ الزام جداگانه در validator ندارد |
| `regulations` | string | قوانین مرکز؛ الزام جداگانه در validator ندارد |
| `pictureId` | `long?` | تصویر کاور؛ در صورت ارسال شناسهٔ مثبت، باید Picture موجود باشد |
| `pansionPrice` | number | مبلغ هر روز برای مرکز اقامتی؛ اگر `isDaycare=false` باید بیشتر از صفر باشد |
| `schoolPrice` | number | مبلغ هر ساعت برای مهد؛ اگر `isDaycare=true` باید بیشتر از صفر باشد |
| `openHour`, `closeHour` | string | غیرخالی؛ سرویس فقط خالی نبودن را کنترل می‌کند، نه قالب یا تقدم/تأخر ساعت |
| `companionId` | `long` | از token سرور؛ فیلد ورودی معتبر برای انتخاب مالک نیست |
| `active`, `approve`, `approvalValue`, `showToSite` | bool/string | وضعیت‌های بررسی/انتشار؛ در create و resubmit توسط سرور بازنویسی می‌شوند |
| `suggested`, `commentCount`, `rateAvg`, `rateCount` | bool/number | پیشنهاد ویژه و آمار؛ در create مقدار اولیه و در resubmit مقدار قبلی حفظ می‌شود |
| `dailyCommissionPercent`, `hourlyCommissionPercent` | decimal | درصد کمیسیون روزانه/ساعتی؛ در create صفر و در resubmit حفظ می‌شود؛ از فرم نماینده تغییر مستقیم ندارد |

نمونهٔ اقامتی عادی:

```http
POST /api/Companion/Pansion
Authorization: Bearer <token>
Content-Type: application/json

{"name":"پانسیون آرام","isDaycare":false,"isInfectious":false,"stateId":8,"cityId":112,"addressValue":"تهران، ...","location":{"x":51.4,"y":35.7},"discription":"نگهداری اقامتی","regulations":"کارت واکسن لازم است","pictureId":901,"pansionPrice":850000,"schoolPrice":0,"openHour":"08:00","closeHour":"22:00"}
```

برای پانسیون اقامتی عفونی فقط `isInfectious=true` می‌شود؛ قیمت همان `pansionPrice` است. برای مهد، `isDaycare=true`, `schoolPrice>0` و معمولاً `pansionPrice=0` بگذارید. مهد عفونی هر دو پرچم را `true` دارد. backend قیمت نامرتبط را الزاماً صفر نمی‌کند؛ صفر گذاشتن آن برای شفافیت فرم است.

### ۴.۲. چرخهٔ بررسی و فعال‌سازی مرکز

```text
POST نماینده → active=false, approve=false, showToSite=false
بررسی مرکز → در صورت تأیید: approve=true, active=true
PUT نماینده → دوباره active=false, approve=false, showToSite=false
PUT PansionActive → تغییر active برای مرکز غیرحذف‌شده
DELETE نماینده → deleted=true, active=false, showToSite=false, suggested=false
```

ساخت مرکز فقط وقتی Companion مربوط فعال، تأییدشده و حذف‌نشده است انجام می‌شود. ویرایش علاوه بر آن، مالک بودن `OwnerId` را بررسی می‌کند؛ هر ویرایش یک resubmit است. `approvalValue` دلیل رد مرکز است؛ در تأیید پاک می‌شود. بنابراین بعد از PUT موفق، در UI «ارسال برای بررسی» نشان دهید، نه «انتشار موفق». `PansionActive` در سرویس، شرط `approve=true` را مستقل کنترل نمی‌کند؛ اما برای رزرو جدید هر دو شرط `active && approve` و حذف‌نشدن لازم‌اند. در رابط، روشن کردن مرکز تأییدنشده را به‌عنوان انتشار قطعی نمایش ندهید.

حذف نرم فقط برای مالک Companion است. اگر رزرو `isReserved=true`, `isCancel=false` و هنوز Complete نشده باشد، حذف رد می‌شود. رزروهای قبلی و اطلاعات مالی باقی می‌مانند. بازگردانی مرکز در پنل نماینده endpoint ندارد.

### ۴.۳. فیلترهای فهرست مراکز

`GET /api/Companion/Pansion` این queryها را می‌پذیرد: `isDaycare`, `isInfectious`, `approve`, `stateId`, `cityId`, `neighborhoodIds`, `suggested`, `showToSite`, `petId`, `available`, `pageIndex`, `pageSize`, `sortBy`, `q`. `available` روی `active` فیلتر می‌کند؛ `petId` وجود اتصال گونه در `PansionPet` را بررسی می‌کند؛ `neighborhoodIds` به محلهٔ Companion مربوط است. `q` در `PansionService.Search` فعلاً جست‌وجو ایجاد نمی‌کند. `companionId` ارسالی override می‌شود. در کد فعلی فیلتر `isDaycare` علاوه بر مقدار خواسته‌شده، رکوردهای قدیمی با مقدار `null` را هم می‌آورد. مرتب‌سازی‌ها: `1/2` شناسه، `4/5` امتیاز، `8/9` قیمت متناسب با نوع مرکز.

جزئیات `PansionVDto` علاوه بر فیلدهای اصلی، `deleted`, `deleteDate`, `companion`, `state`, `city`, `picture`, `pansionPets`, `pansionPictures`, `pansionComments`, درصدهای کمیسیون و آمار امتیاز/نظر را می‌دهد. پس از POST/PUT/Active، detail را دوباره بخوانید.

## ۵. کاور، گالری و گونه‌های قابل پذیرش

تصویر کاور با `pictureId` در خود `PansionDto` تنظیم می‌شود. تصاویر دیگر رکوردهای `PansionPicture` هستند. فایل تصویر باید پیش‌تر از سرویس فایل آپلود شده و شناسهٔ Picture گرفته شده باشد؛ رکورد گالری خودش فایل را آپلود نمی‌کند.

| روش | مسیر | فیلدها / رفتار |
|---|---|---|
| GET | `/api/Companion/PansionPicture?pansionId=73` | فهرست گالری با `pansionId`, `pageIndex`, `pageSize` و scope Companion |
| GET | `/api/Companion/PansionPicture/{id}` | جزئیات `PansionPictureVDto` شامل `id`, `pansionId`, `pictureId`, `picture` |
| POST | `/api/Companion/PansionPicture` | `{ "pansionId":73, "pictureId":901 }`؛ مالکیت مرکز کنترل می‌شود |
| PUT | `/api/Companion/PansionPicture` | `{ "id":12, "pansionId":73, "pictureId":902 }`؛ `pansionId` به مقدار رکورد موجود برگردانده می‌شود |
| DELETE | `/api/Companion/PansionPicture?id=12` | حذف اتصال گالری؛ ID اتصال را بفرستید، نه `pictureId` |
| GET | `/api/Companion/PansionPet` | اتصال گونه‌های مراکز نماینده، با صفحه‌بندی؛ query برای `pansionId` ندارد |
| GET | `/api/Companion/PansionPet/{id}` | `PansionPetVDto` شامل `id`, `pansionId`, `petId`, `pet` |
| POST | `/api/Companion/PansionPet` | `{ "pansionId":73, "petId":4 }`؛ تکراری رد می‌شود |
| PUT | `/api/Companion/PansionPet` | `{ "id":12, "pansionId":73, "petId":5 }`؛ مالکیت رکورد موجود بررسی می‌شود |
| DELETE | `/api/Companion/PansionPet?id=12` | حذف اتصال گونه؛ ID رکورد اتصال را بفرستید |

برای نمایش گونه‌های یک مرکز، `pansionPets` جزئیات خود مرکز را بخوانید یا فهرست scoped را در client فیلتر کنید. برای تصاویر، query `pansionId` موجود است. در GET جزئیات تک‌موردی Picture/Pet، کنترل مالکیت مجدد در controller دیده نمی‌شود؛ mutationها کنترل مالکیت دارند. این فاصلهٔ backend باید اصلاح شود.

## ۶. بخش رزروهای ورودی نماینده

**املای route لیست و جزئیات رزرو `Comapnion` است**، چون در controller همین مقدار برای Area ثبت شده است. آن را به `Companion` تصحیح نکنید مگر backend هم اصلاح شده باشد.

| روش | مسیر دقیق | هدف |
|---|---|---|
| GET | `/api/Comapnion/PansionReserve` | فهرست رزروهای پانسیون‌های Companion جاری |
| GET | `/api/Comapnion/PansionReserve/{id}` | جزئیات رزرو همان Companion |
| PUT | `/api/Companion/PansionReserveChangeStatus` | تغییر وضعیت با `PansionReserveStatusDto` |
| PUT | `/api/Companion/PansionReserveCancel` | route موجود؛ لغو برای نمایندهٔ عادی با قواعد فعلی سرویس مجاز نیست |

### ۶.۱. فهرست رزروها: ورودی، پاسخ و تب‌ها

```http
GET /api/Comapnion/PansionReserve?pansionId=73&statusId=47&pageIndex=1&pageSize=20&sortBy=1
Authorization: Bearer <token>
```

| query | نوع / اثر |
|---|---|
| `pansionId` | `long?`؛ فقط رزروهای یک مرکز متعلق به نماینده |
| `statusId` | `long?`؛ ۴۶ ثبت‌شده، ۴۷ پرداخت‌شده، ۴۸ تکمیل‌شده |
| `isDaycare` | `bool?`؛ تفکیک مهد و اقامتی بر اساس مرکز مرتبط |
| `bookerId` | `long?`؛ مشتری خاص در محدودهٔ مراکز نماینده |
| `userPetId` | `long?`؛ پت خاص در همان محدوده |
| `q` | string؛ برابری دقیق با `reserveCode` یا contains در نام، نام خانوادگی و موبایل مشتری |
| `pageIndex`, `pageSize` | int؛ پیش‌فرض ۱ و ۲۰ |
| `sortBy` | enum؛ فقط `1=New` و `2=Old` در جست‌وجوی رزرو اعمال می‌شود |
| `companionId` | `long?`؛ در controller از token جایگزین می‌شود و برای انتخاب نمایندهٔ دیگر نیست |

پاسخ `PansionReserveSearchDto` شامل `list` از `PansionReserveVDto`, `totalCount` و فیلترهای اعمال‌شده است. `isCancel` و بازهٔ تاریخ، query قابل فیلتر در این endpoint ندارند. بنابراین تب «لغوشده» را نمی‌توان با `totalCount` دقیق از این API ساخت. برای «در انتظار پرداخت» `statusId=46`، برای «پرداخت‌شده» `47` و برای «تکمیل‌شده» `48` بگیرید؛ در هر تب `isCancel=true` را جدا مشخص کنید.

در فهرست، `pansion`, `status`, `booker`, `userPet` بارگذاری می‌شوند. اطلاعات سفر پت‌رسان مرتبط و `totalPrice` در service **فقط هنگام GET جزئیات تک‌رزرو** تکمیل می‌شوند. برای کارت سفر یا جمع رزرو+سفر به detail بروید؛ `totalPrice` فهرست را مبنا نگیرید.

### ۶.۲. فیلدهای کارت و جزئیات رزرو (`PansionReserveVDto`)

| گروه | فیلدهای دقیق | کاربرد پنل |
|---|---|---|
| شناسه‌ها | `id`, `reserveCode`, `pansionId`, `bookerId`, `userPetId` | پیگیری و ارتباط با مرکز، مشتری و پت |
| زمان ثبت/خدمت | `createDate`, `fromDate`, `toDate`, `schoolCreateDate`, `startTime`, `endTime` | اقامتی با دو تاریخ؛ مهد با یک روز و ساعت‌ها |
| مقدار خدمت | `dayCount`, `hourCount` | تعداد روز/ساعت محاسبه‌شده توسط سرور |
| مالی | `price`, `discount`, `rebateId`, `rebatePrice`, `fromWallet`, `walletPrice`, `paymentPrice` | مبلغ پایه، تخفیف، کیف پول و مبلغ نهایی رزرو |
| سهم‌ها | `companionShare`, `siteShare` | سهم نماینده و سایت پس از اعمال کمیسیون |
| وضعیت | `isReserved`, `isCancel`, `cancelDetail`, `cancelDate`, `statusId`, `status` | قطعی بودن، لغو و وضعیت جاری |
| اشیای مرتبط | `pansion`, `userPet`, `booker`, `rebate` | مشخصات مرکز، پت، مشتری و کوپن |
| سفر اختیاری | `petResanTrip`, `totalPrice` | فقط در detail به‌صورت صریح تکمیل می‌شود؛ پرداخت سفر مستقل است |

`petResanTrip` در صورت وجود دارای `tripId`, `price`, `paymentPrice`, `isPaid`, `tripStatusLabel`, `driverStatusLabel`, `driverId`, `driverName` است. `totalPrice` جمع نمایشی رزرو پانسیون و سفر است؛ پرداخت و وضعیت مالی آن‌ها یکی نیست. `status` شیء کد است و برای متن نمایشی از همان یا کاتالوگ Code استفاده کنید.

### ۶.۳. تغییر وضعیت؛ action واقعی نماینده

```http
PUT /api/Companion/PansionReserveChangeStatus
Authorization: Bearer <token>
Content-Type: application/json

{"id":501,"statusId":48}
```

body فقط `id` رزرو و `statusId` مقصد است. سرویس رزرو را پیدا می‌کند؛ اگر `CompanionId` token مقدار داشته باشد، تعلق پانسیون به همان Companion را کنترل می‌کند. اگر `paymentPrice==0` باشد، خطای `TheFinalPriceHasNotYetBeenRecordedForThisReserve` می‌دهد. سپس `statusId` را **بدون بررسی وضعیت قبلی، پرداخت قطعی، لغو یا معتبر بودن کد مقصد** ذخیره می‌کند. پاسخ از نوع `BaseResultDto<PansionReserveStatusDto>` با `isSuccess` است؛ بعد از موفقیت، detail را refresh کنید.

تنها side effect ویژهٔ `48` وقتی اجرا می‌شود که وضعیت قبلی Complete نبوده، `isReserved=true` و `isCancel=false` باشد: رویداد تکمیل برای امتیاز باشگاه ثبت می‌شود و push یادآوری نظر برای ۱۲ ساعت بعد به کاربر برنامه‌ریزی می‌شود. اگر شرط پرداخت/لغو برقرار نباشد، خود status باز هم ۴۸ ذخیره می‌شود ولی این side effectها اجرا نمی‌شوند. فرستادن ۴۷ از پنل، پرداخت واقعی را انجام نمی‌دهد و نباید دکمهٔ «تأیید» باشد.

**قاعدهٔ پیشنهادی نمایش پنل:** دکمهٔ «ثبت پایان خدمت» را فقط برای `isReserved=true && isCancel=false && statusId=47` نشان دهید؛ برای ۴۶ برچسب «در انتظار پرداخت» و برای لغوشده حالت فقط‌خواندنی بگذارید. این قرارداد UI، نقص کنترل انتقال در backend را پنهان نمی‌کند.

### ۶.۴. لغو از پنل نماینده

`PUT /api/Companion/PansionReserveCancel` با `PansionReserveCancelDto` شامل `id`, `isCancel`, `cancelDetail` وجود دارد، اما سرویس لغو را فقط برای نقش `Admin` یا دارندهٔ permission ناحیهٔ Admin می‌پذیرد. نمایندهٔ عادی `AccessDenied` می‌گیرد. رزرو باید `isReserved=true` باشد و برای `isCancel=true` علت غیرخالی لازم است. در لغو موفق، `isCancel`, `cancelDetail`, `cancelDate` ثبت و سفر پت‌رسان متصل لغو می‌شود؛ `statusId` در این متد تغییر نمی‌کند. برای رزروهای Paid/Complete، شرط اضافهٔ `RoleId` می‌تواند حتی بعضی کاربران دارای permission Admin را رد کند. **این route را به‌عنوان دکمهٔ لغو عادی نماینده نمایش ندهید.**

## ۷. قواعد رزرو که نماینده باید هنگام نمایش بداند

- مرکز فقط وقتی رزرو جدید می‌پذیرد که `active=true`, `approve=true`, `deleted=false` باشد.
- پت دارای `specificDisease` غیرخالی فقط به مرکز عفونی، پت بدون آن فقط به مرکز غیرعفونی می‌رود. این قاعده هنگام ایجاد رزرو در سرور کنترل می‌شود.
- در اقامتی `fromDate` و `toDate` لازم‌اند؛ روز شروع و پایان هر دو شمرده می‌شوند: `dayCount=(to−from)+1` و `price=pansionPrice×dayCount`. تاریخ شروع نباید گذشته باشد و پایان نباید قبل از شروع باشد.
- در مهد `schoolCreateDate`, `startTime`, `endTime` لازم‌اند. ساعت‌ها باید `HH:mm` باشند، شروع قبل از پایان و در آینده باشد. `hourCount=ceil(مدت به ساعت)` و `price=schoolPrice×hourCount` است؛ مثلاً ۴٫۵ ساعت، ۵ ساعت حساب می‌شود.
- رزرو ابتدا قطعی نیست. پس از پرداخت موفق، سرور تداخل همان پت/همان مرکز را دوباره کنترل، سپس `isReserved=true`, `statusId=47` و سهم‌ها را ثبت می‌کند. ثبت اولیه یا پیام اطلاع‌رسانی به معنی پرداخت نیست.
- `siteShare=paymentPrice×درصد کمیسیون/100` و `companionShare=paymentPrice−siteShare`. برای مهد درصد `hourlyCommissionPercent` و برای اقامتی `dailyCommissionPercent` استفاده می‌شود.
- اعلان ثبت رزرو به مالک پانسیون تلاش می‌شود، اما منبع قطعی صفحه، `GET` رزروهای خود نماینده است. برای دیدن تغییر پرداخت یا لغو، فهرست/detail را تازه کنید.
- برای یک رزرو پانسیون ممکن است سفر پت‌رسان متصل وجود داشته باشد. قیمت سفر در `totalPrice` جزئیات صرفاً برای نمایش جمع آمده است و پرداخت آن جداست.

## ۸. چینش پیشنهادی پنل بر اساس API موجود

| صفحه/بخش | داده و action واقعی |
|---|---|
| مراکز من | `GET /api/Companion/Pansion`، فیلتر نوع/عفونی/تأیید/فعال، نمایش وضعیت بررسی و علت رد |
| ایجاد/ویرایش مرکز | `PansionDto` کامل با مکان روی نقشه، قیمت نوع مربوط، قوانین و کاور؛ پس از PUT نمایش «ارسال برای بررسی» |
| تصاویر | کاور در خود مرکز؛ گالری با `PansionPicture` |
| گونه‌های قابل‌پذیرش | `PansionPet`؛ کنترل محلی تکراری و refresh جزئیات مرکز |
| رزروها | `GET /api/Comapnion/PansionReserve` با تب‌های ۴۶/۴۷/۴۸ و علامت جدا برای `isCancel` |
| جزئیات رزرو | مشتری، پت، بازهٔ اقامت/مهد، مبلغ/سهم، وضعیت پرداخت، سفر اختیاری، علت لغو |
| اقدام رزرو | برای رزرو Paid و لغونشده، `PUT ...ChangeStatus` با `statusId=48` جهت «ثبت پایان خدمت» |

## ۹. محدودیت‌ها و خطاهای فعلی که نباید در طراحی پنل پنهان شوند

1. تأیید یا رد دستی رزرو توسط پانسیون در API وجود ندارد؛ وضعیت‌های ۴۶/۴۷/۴۸ را به چنین مفهومی تغییر نام ندهید.
2. `ChangeStatus` هر شناسهٔ وضعیت را پس از شرط مبلغ غیرصفر می‌پذیرد و شرط `Paid→Complete` را enforce نمی‌کند. رزرو رایگان با `paymentPrice=0` حتی در صورت تخفیف کامل از این مسیر تغییر وضعیت نمی‌گیرد.
3. `CompanionId=null` در بعضی سرویس‌ها باعث حذف scope مالکیت می‌شود؛ نیازمند اصلاح backend و کنترل ورود پنل است.
4. route رزروهای نماینده `Comapnion` است. این typo در contract فعلی وجود دارد.
5. لیست رزرو فیلتر `isCancel`/تاریخ ندارد و اطلاعات سفر/جمع هزینه فقط در detail به‌صورت صریح تکمیل می‌شود.
6. سرویس پذیرش گونهٔ تعریف‌شده در `PansionPet` یا ظرفیت کل را هنگام ساخت رزرو enforce نمی‌کند؛ UI «ظرفیت قطعی باقی‌مانده» ندهد.
7. `PansionActive` شرط تأیید مرکز را خودش بررسی نمی‌کند، ولی رزرو جدید مرکز تأییدنشده مجاز نیست.
8. GET جزئیات تک‌موردی گالری/گونه در controller کنترل مالکیت مجدد ندارد؛ نمایش را از list/detail scoped بسازید و backend را اصلاح کنید.
9. در مدل رزرو، تاریخچهٔ تصمیم نماینده، زمان قبول/رد، دلیل رد و مهلت پاسخ نماینده وجود ندارد. برای فرایند واقعی تأیید رزرو به این قراردادها نیاز است.

## ۱۰. منبع بررسی

`Api/Areas/Companion/Controllers/PansionController.cs`, `PansionActiveController.cs`, `PansionPictureController.cs`, `PansionPetController.cs`, `PansionReserveController.cs`, `PansionReserveChangeStatusController.cs`, `PansionReserveCancelController.cs`؛ DTOها و serviceهای `Application/Services/PansionSrvs/`؛ و موجودیت‌های `Entities/Entities/PansionField/`.

برای اضافه‌کردن «قبول/رد واقعی رزرو قبل از پرداخت»، [قرارداد پیشنهادی بک‌اند](PANSION_RESERVATION_CONFIRMATION_BACKEND_CONTRACT_FA.md) تهیه شده است؛ این قابلیت در API فعلی وجود ندارد.
