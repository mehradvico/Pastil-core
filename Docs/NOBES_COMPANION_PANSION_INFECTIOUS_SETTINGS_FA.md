# مستند اپ نماینده: ثبت و مدیریت پانسیون عفونی و عادی

**مخاطب:** اپ نماینده / کلینیک پاستیل  
**Scope:** تعریف، ثبت، ویرایش، فعال/غیرفعال‌سازی، پذیرش گونهٔ پت و تصاویر پانسیون. رزرو مشتری فقط در حد اثری که تنظیمات پانسیون روی آن می‌گذارند توضیح داده شده است.

> پانسیون (`Pansion`) با «مدرسهٔ تربیت» جداست. بااین‌حال یک پانسیون می‌تواند `IsDaycare=true` باشد؛ یعنی حالت مهد/نگهداری ساعتی-روزانه، نه پانسیون اقامتی شبانه.

---

## 1. دو محور مستقل تنظیمات

هر پانسیون دو ویژگی مستقل دارد؛ این‌ها را در UI یکی نکنید:

| فیلد | مقدار | معنی و اثر |
| --- | --- | --- |
| `isInfectious` | `false` | پانسیون عادی؛ فقط پت بدون بیماری خاص ثبت‌شده را می‌پذیرد. |
| `isInfectious` | `true` | پانسیون ایزوله/عفونی؛ فقط پت با `SpecificDisease` ثبت‌شده را می‌پذیرد. |
| `isDaycare` | `false` | پانسیون اقامتی؛ قیمت روزانه با `pansionPrice` و بازهٔ `fromDate` تا `toDate`. |
| `isDaycare` | `true` | مهد/نگهداری روزانه؛ قیمت ساعتی با `schoolPrice` و `schoolCreateDate` + `startTime`/`endTime`. |

Backend در حال حاضر تمام ترکیب‌های `isInfectious × isDaycare` را می‌پذیرد. بنابراین اگر سیاست کسب‌وکار شما «پانسیون عفونی فقط اقامتی است» باشد، اپ باید انتخاب `isInfectious=true` و `isDaycare=true` را محدود کند یا برای enforce قطعی، validation جدید backend لازم است.

### قاعده‌ی قطعی UI: «عادی» و «غیرعفونی» یک انتخاب‌اند

`isInfectious=false` تنها یک معنی دارد: **پانسیون عادی/غیرعفونی**. «عادی» و «غیرعفونی» دو نوع مستقل نیستند و نباید به‌شکل دو کارت یا دو گزینه‌ی جدا نمایش داده شوند؛ در غیر این صورت هر دو پس از ذخیره یک مقدار یکسان دارند و در صفحه‌ی ویرایش قابل تشخیص نیستند.

UI پیشنهادی برای نماینده، دو انتخاب مستقل دوتایی است:

| محور | گزینه‌ی نمایش | مقدار ارسالی |
| --- | --- | --- |
| پذیرش سلامت | عادی (غیرعفونی) | `isInfectious=false` |
| پذیرش سلامت | ایزوله / عفونی | `isInfectious=true` |
| نوع نگهداری | اقامتی | `isDaycare=false` |
| نوع نگهداری | مهد / روزانه | `isDaycare=true` |

اگر UI به‌جای دو selector مستقل، کارت ترکیبی نشان می‌دهد، فقط این **چهار** کارت مجازند:

1. عادیِ اقامتی: `false, false`
2. عادیِ مهد: `false, true`
3. عفونیِ اقامتی: `true, false`
4. عفونیِ مهد: `true, true`

ترتیب مقادیر در فهرست بالا `(isInfectious, isDaycare)` است. در list و صفحه‌ی ویرایش، هر دو badge را هم‌زمان نشان دهید؛ مثلاً «عادی · مهد» یا «ایزوله · اقامتی». مقدار پیش‌فرض پیشنهادی برای ساخت جدید: «عادی · اقامتی» یعنی `isInfectious=false` و `isDaycare=false`.

### محدودیت فعلی ظرفیت

در مدل و API فعلی **فیلد ظرفیت، تعداد جای خالی، اتاق یا سقف رزرو هم‌زمان وجود ندارد**. هنگام رزرو نیز server فقط هم‌پوشانی همان `UserPetId` در همان پانسیون را کنترل می‌کند، نه ظرفیت کل پانسیون. بنابراین در اپ نماینده صفحه یا وعدهٔ «ظرفیت باقی‌مانده» نسازید و این تنظیم از همین APIها قابل ثبت نیست. پیاده‌سازی ظرفیت واقعی، نیازمند تغییر جداگانه در backend و قرارداد رزرو است.

### قانون جداسازی عفونی در رزرو

هنگام رزرو، server بیماری پت را از پروفایل پت می‌خواند، نه از دادهٔ ارسالی اپ:

```text
SpecificDisease خالی       → فقط پانسیون isInfectious=false
SpecificDisease غیرخالی    → فقط پانسیون isInfectious=true
```

پس عنوان یا badge «عفونی» نباید صرفاً نمایشی باشد. تغییر آن روی امکان خرید رزروهای بعدی اثر می‌گذارد. این تغییر، رزروهای موجود را خودکار لغو یا جابه‌جا نمی‌کند؛ پیش از تغییر از عادی به عفونی یا برعکس، اپ باید هشدار تأیید جدی نشان دهد و رزروهای فعال را به نماینده یادآوری کند.

---

## 2. وضعیت‌ها و مالکیت داده

تمام endpointهای این سند به `Authorization: Bearer {accessToken}` نیاز دارند. `companionId` از کاربر لاگین‌شده در server تعیین می‌شود؛ اپ هرگز شناسهٔ کلینیک دلخواه را برای ساخت یا ویرایش پانسیون معتبر فرض نکند.

| فیلد | مالک واقعی | رفتار اپ |
| --- | --- | --- |
| `id` و `companionId` | server | در ساخت `id=0` و `companionId` را ارسال نکنید/به آن تکیه نکنید؛ پاسخ server را نگه دارید. |
| `active` | نماینده، پس از تأیید | فقط با endpoint فعال‌سازی تغییر کند؛ switch آن پیش از `approve=true` غیرفعال باشد. |
| `approve` و `approvalValue` | مدیر پاستیل | فقط نمایش بدهید؛ نماینده آن را set نکند. دلیل رد در `approvalValue` می‌آید. |
| `showToSite` و `suggested` | server/مدیر | در فرم نماینده کنترل قابل‌ویرایش ندارند. |
| قیمت، ساعت، نوع، عفونی‌بودن، آدرس و قوانین | نماینده | در ثبت و ویرایش ارسال می‌شوند و سپس پانسیون برای بررسی مجدد می‌رود. |

### چرخهٔ lifecycle

```text
ثبت یا ویرایش نماینده
  → Active=false, Approve=false, ShowToSite=false
  → در انتظار بررسی مدیر
  → تأیید مدیر: Approve=true و Active=true
  → نماینده در صورت نیاز Active را خاموش/روشن می‌کند

رد مدیر → Approve=false، Active=false، ApprovalValue=علت رد
ویرایش مجدد → ارسال دوباره برای بررسی (Approve/Active/ShowToSite دوباره false)
```

**نتیجهٔ مهم UX:** پس از `POST` یا `PUT` هرگز پیام «پانسیون شما فعال شد» نشان ندهید. پیام صحیح «برای بررسی ارسال شد» است و نتیجه را از list/detail تازه بخوانید.

---

## 3. APIهای اصلی پانسیون

| کاربرد | Method | مسیر |
| --- | --- | --- |
| فهرست پانسیون‌های کلینیک من | GET | `/api/Companion/Pansion` |
| جزئیات یک پانسیون خودم | GET | `/api/Companion/Pansion/{id}` |
| ثبت درخواست پانسیون | POST | `/api/Companion/Pansion` |
| ویرایش و ارسال دوباره برای بررسی | PUT | `/api/Companion/Pansion` |
| روشن/خاموش‌کردن فعالیت | PUT | `/api/Companion/PansionActive` |
| گونه‌های قابل‌پذیرش | GET/POST/PUT/DELETE | `/api/Companion/PansionPet` |
| گالری تصاویر | GET/POST/PUT/DELETE | `/api/Companion/PansionPicture` |

### فهرست و جزئیات

```http
GET /api/Companion/Pansion?PageIndex=1&PageSize=20&IsInfectious=true
GET /api/Companion/Pansion/{pansionId}
```

فیلترهای مفید فهرست: `IsInfectious`, `IsDaycare`, `Approve`, `Available`، `Q`، `PageIndex` و `PageSize`. scope کلینیک از توکن می‌آید؛ فرستادن `CompanionId` در query ضروری نیست و server آن را override می‌کند.

در screen list حداقل این‌ها را نمایش دهید: نام، badge «عفونی/عادی»، badge «مهد/اقامتی»، قیمت متناظر، `approve`، `active`، علت رد (`approvalValue`) و تصویر اصلی. قبل از edit، detail را با `GET /Pansion/{id}` دوباره بخوانید؛ نتیجه شامل تصاویر و گونه‌های پت هم هست.

---

## 4. فرم ثبت پانسیون

### 4.1 فیلدهای قابل‌ارسال

```ts
type PansionPayload = {
  id: number                 // 0 در ساخت، شناسهٔ server در ویرایش
  name: string
  isDaycare: boolean         // اجباری؛ null/undefined مجاز نیست
  isInfectious: boolean
  stateId: number
  cityId: number
  addressValue: string
  discription?: string       // املای API فعلی همین است
  pictureId?: number | null
  pansionPrice: number       // قیمت هر روز برای isDaycare=false
  schoolPrice: number        // قیمت هر ساعت برای isDaycare=true
  regulations?: string
  openHour: string
  closeHour: string
}
```

هرچند DTO فیلدهای دیگری نیز دارد، این مقادیر را client source-of-truth نکنید: `companionId`, `active`, `approve`, `approvalValue`, `showToSite`, `suggested`, `commentCount`, `rateAvg`, `rateCount`, commissionها. server در ثبت اولیه وضعیت‌های انتشار و تأیید را خودش false می‌کند.

### 4.2 اعتبارسنجی قبل از ارسال

| شرط server | اعتبارسنجی پیشنهادی اپ |
| --- | --- |
| نام لازم است | trim و non-empty |
| نوع مرکز لازم است | قبل از نمایش دکمهٔ ثبت، کاربر باید مهد یا اقامتی را انتخاب کند |
| استان و شهر معتبر و هم‌خوان | ابتدا استان، سپس شهرهای همان استان؛ شناسه‌ها عددی و مثبت |
| آدرس لازم است | trim و non-empty |
| `openHour` و `closeHour` لازم‌اند | متن non-empty؛ app بهتر است time picker `HH:mm` بدهد، اما backend فعلی فقط خالی‌نبودن را validate می‌کند |
| مهد: `schoolPrice > 0` | فقط قیمت ساعتی را اجباری و برجسته کنید |
| اقامتی: `pansionPrice > 0` | فقط قیمت روزانه را اجباری و برجسته کنید |
| `pictureId` در صورت وجود | فقط ID خروجی از upload موفق خود پاستیل |

قیمت غیرمرتبط را هم می‌توان صفر نگه داشت؛ فقط قیمت مربوط به نوع انتخاب‌شده شرط `> 0` دارد. قیمت را با واحد پول ثابت UI (مطابق قرارداد فعلی API) نمایش دهید و پیش از ارسال به number تبدیل کنید؛ جداکنندهٔ هزارگان یا رشتهٔ محلی نفرستید.

### 4.3 ثبت

```http
POST /api/Companion/Pansion
Content-Type: application/json
```

نمونهٔ پانسیون عادیِ اقامتی:

```json
{
  "id": 0,
  "name": "پانسیون آرام",
  "isDaycare": false,
  "isInfectious": false,
  "stateId": 8,
  "cityId": 112,
  "addressValue": "تهران، ...",
  "discription": "پانسیون اقامتی برای پت‌های سالم",
  "pictureId": 901,
  "pansionPrice": 850000,
  "schoolPrice": 0,
  "regulations": "کارت واکسیناسیون الزامی است.",
  "openHour": "08:00",
  "closeHour": "22:00"
}
```

نمونهٔ پانسیون عفونیِ اقامتی فقط در دو فیلد متفاوت است:

```json
{
  "isDaycare": false,
  "isInfectious": true,
  "pansionPrice": 1200000
}
```

پس از موفقیت، `id` برگشتی را ذخیره کنید و کاربر را به صفحهٔ «در انتظار بررسی» ببرید. upload عکس‌های بیشتر و تعریف گونه‌های قابل‌پذیرش فقط وقتی ممکن است که `pansionId` واقعی را داشته باشید.

---

## 5. ویرایش، تأیید و فعال‌سازی

### 5.1 ویرایش

```http
PUT /api/Companion/Pansion
```

ابتدا جزئیات را دریافت کنید، فقط فیلدهای فرم را ویرایش کنید، سپس کل payload با `id` معتبر را بفرستید. server این درخواست را یک **resubmit** می‌داند و حتی با یک تغییر کوچک، این مقادیر را reset می‌کند:

```text
active=false
approve=false
approvalValue=null
showToSite=false
```

بنابراین دیالوگ تأیید باید صریح بگوید: «با ذخیرهٔ تغییرات، پانسیون تا تأیید مجدد از رزرو جدید خارج می‌شود.» بعد از پاسخ موفق، list/detail را refresh و UI را روی pending قرار دهید.

برای تغییر `isInfectious` یک تأیید دوم نشان دهید: «نوع پذیرش عفونی/عادی عوض می‌شود و مشتریان جدید بر اساس بیماری ثبت‌شدهٔ پت فیلتر خواهند شد.» در backend کنونی lock خودکار بر مبنای رزروهای باز وجود ندارد؛ اپ این اثر را پنهان نکند.

### 5.2 فعال/غیرفعال‌کردن پس از تأیید

```http
PUT /api/Companion/PansionActive
Content-Type: application/json

{ "id": 73, "active": false }
```

- این عمل فقط باید برای پانسیون `approve=true` در UI در دسترس باشد.
- `active=false` از رزرو جدید جلوگیری می‌کند؛ رزروهای قبلی را cancel یا refund نمی‌کند.
- پس از هر toggle، detail را refresh کنید. endpoint فقط نتیجهٔ ساده می‌دهد و نباید state محلی را بدون refresh قطعی بدانید.
- اگر پانسیون pending یا ردشده است، به‌جای switch وضعیت بررسی/علت رد را نشان دهید. فراخوانی endpoint از نظر backend ممکن است approval را دوباره validate نکند، اما UX نباید امکان دورزدن flow تأیید را بسازد.

---

## 6. گونه‌های قابل‌پذیرش و تصاویر

### 6.1 گونه‌های پت

پانسیون باید گونه/نوع پت‌هایی را که می‌پذیرد مشخص کند. این تنظیم جدا از عفونی/عادی بودن است.

```http
GET    /api/Companion/PansionPet?PageIndex=1&PageSize=100
POST   /api/Companion/PansionPet
PUT    /api/Companion/PansionPet
DELETE /api/Companion/PansionPet?id={pansionPetId}
```

```json
{ "id": 0, "pansionId": 73, "petId": 4 }
```

برای ویرایش، `id` اتصال موجود را بفرستید؛ `pansionId` آن توسط server از رکورد قبلی نگه داشته می‌شود. برای حذف، شناسهٔ رکورد `PansionPet` را بفرستید، نه `petId`. app باید قبل از POST/PUT/DELETE ownership را به پاسخ server واگذار کند و بعد از هر تغییر، detail پانسیون را refresh کند.

### 6.2 گالری تصاویر

اول تصویر را با flow آپلود فایل استاندارد اپ به `pictureId` تبدیل کنید، سپس آن را به پانسیون متصل کنید:

```http
GET    /api/Companion/PansionPicture?PageIndex=1&PageSize=100
POST   /api/Companion/PansionPicture
PUT    /api/Companion/PansionPicture
DELETE /api/Companion/PansionPicture?id={pansionPictureId}
```

```json
{ "id": 0, "pansionId": 73, "pictureId": 901 }
```

`pictureId` اصلی در فرم پانسیون (`pictureId`) با گالری (`PansionPicture`) یکی نیست: اولی کاور/تصویر اصلی پانسیون است، دومی مجموعهٔ تصاویر اضافه. هنگام حذف عکس گالری فقط association حذف می‌شود؛ flow حذف فایل اصلی را بدون تأیید جداگانه اجرا نکنید.

---

## 7. اثر تنظیمات بر رزرو مشتری

این قسمت را در صفحهٔ تنظیمات به‌عنوان توضیح کسب‌وکار نشان دهید تا نماینده بداند چه چیزی می‌فروشد:

| نوع پانسیون | ورودی رزرو | فرمول قیمت server |
| --- | --- | --- |
| اقامتی (`isDaycare=false`) | `fromDate` تا `toDate` | `pansionPrice × تعداد روز`؛ هر دو روز ابتدا و انتها محاسبه می‌شوند |
| مهد (`isDaycare=true`) | `schoolCreateDate`، `startTime` و `endTime` | `schoolPrice × ceil(تعداد ساعت)` |

رزرو فقط برای پانسیون `active=true` و `approve=true` ساخته می‌شود. عفونی/عادی هم با وضعیت بیماری واقعی پت کنترل می‌شود؛ اپ نماینده نباید وعده دهد که با تغییر نوع پانسیون، رزرو ناسازگار موجود قابل ثبت یا ادامه خواهد بود.

---

## 8. state پیشنهادی اپ نماینده

```ts
type PansionEditorState = {
  pansionId?: number
  name: string
  isDaycare?: boolean
  isInfectious: boolean
  stateId?: number
  cityId?: number
  pansionPrice: string
  schoolPrice: string
  addressValue: string
  openHour: string
  closeHour: string
  pictureId?: number
  regulations?: string
  approval?: { approve: boolean; reason?: string | null }
  active?: boolean
}
```

| رخداد | source of truth بعد از رخداد |
| --- | --- |
| ساخت موفق | پاسخ `POST`، سپس `GET /Pansion/{id}` |
| ذخیرهٔ ویرایش | `GET /Pansion/{id}`؛ انتظار `approve=false` و `active=false` داشته باشید |
| toggle فعالیت | `GET /Pansion/{id}` |
| بازگشت به app / pull-to-refresh | `GET /Pansion` و در صورت نیاز جزئیات |

---

## 9. چک‌لیست QA

- [ ] پانسیون عادی فقط برای پت بدون `SpecificDisease` و پانسیون عفونی فقط برای پت دارای بیماری خاص در رزرو قابل انتخاب است.
- [ ] `isDaycare` در ساخت اجباری است و price متناظر با آن حتماً مثبت است.
- [ ] صفحهٔ عفونی و عادی از یک boolean مشترک استفاده می‌کند و به‌اشتباه دو نوع entity جدا نمی‌سازد.
- [ ] «عادی» و «غیرعفونی» به‌صورت دو کارت جدا نمایش داده نمی‌شوند؛ هر دو همان `isInfectious=false` هستند.
- [ ] اگر کارت ترکیبی استفاده می‌شود، فقط چهار ترکیب `(isInfectious × isDaycare)` نمایش داده می‌شود و هر کارت در edit با دو badge قابل تشخیص است.
- [ ] POST موفق، پانسیون را pending نمایش می‌دهد، نه active.
- [ ] PUT موفق approve/active را pending می‌کند و صفحهٔ رزروپذیر را فوری نشان نمی‌دهد.
- [ ] دلیل رد مدیر از `approvalValue` نمایش داده می‌شود و نماینده می‌تواند اصلاح و resubmit کند.
- [ ] switch فعال/غیرفعال برای پانسیون تأییدنشده قابل لمس نیست.
- [ ] تغییر عفونی/عادی هشدار اثر بر رزروهای جدید دارد.
- [ ] `PansionPet.id` و `PansionPicture.id` در عملیات update/delete استفاده می‌شوند، نه `petId` یا `pictureId`.
- [ ] پاسخ API، خصوصاً بعد از activation یا edit، source of truth است؛ تغییر خوش‌بینانه بدون refresh باعث نمایش وضعیت غلط نمی‌شود.

**وضعیت سند:** منطبق با backend فعلی پاستیل در ۲۱ سپتامبر ۲۰۲۶.
