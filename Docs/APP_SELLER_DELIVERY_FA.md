# مستند کامل «روش‌های ارسال فروشگاه» (پنل / اپ فروشنده)

این مستند بر اساس کد فعلی بک‌اند نوشته شده و مرجع فرانت (پنل فروشنده، اپ و Nuxt Server API) برای بخش **ارسال‌ها / روش‌های ارسال فروشگاه** است.

فهرست:

1. [نمای کلی و مفاهیم](#۱-نمای-کلی-و-مفاهیم)
2. [قواعد عمومی API](#۲-قواعد-عمومی-api)
3. [مدل داده و توضیح فیلدها](#۳-مدل-داده-و-توضیح-فیلدها)
4. [لیست Endpointها](#۴-لیست-endpointها)
5. [قوانین اعتبارسنجی و پیام خطاها](#۵-قوانین-اعتبارسنجی-و-پیام-خطاها)
6. [ترکیب‌های رایج تنظیم (مثال)](#۶-ترکیبهای-رایج-تنظیم)
7. [این تنظیمات در خرید مشتری چه اثری دارند](#۷-اثر-تنظیمات-روی-خرید-مشتری)
8. [بعد از پرداخت: Shipment و پیگیری](#۸-بعد-از-پرداخت-shipment-و-پیگیری)
9. [وزن و ابعاد محصول](#۹-وزن-و-ابعاد-محصول)
10. [تنظیمات سرور (فقط برای اطلاع)](#۱۰-تنظیمات-سرور)
11. [راهنمای پیاده‌سازی فرانت](#۱۱-راهنمای-پیادهسازی-فرانت)
12. [نکات و رفتارهای مهم / ابهام‌های شناخته‌شده](#۱۲-نکات-مهم-و-رفتارهای-شناختهشده)

---

## ۱. نمای کلی و مفاهیم

هر فروشگاه می‌تواند چند «روش ارسال» (`Delivery`) تعریف کند. هر روش ارسال یک رکورد مستقل است با:

- **نوع ارسال** (`deliveryTypeId`): یکی از `Code`های گروه `DeliveryType` (پیک، پست، تیپاکس، تحویل حضوری، الوپیک، اسنپ‌باکس، میاره).
- **قیمت‌گذاری**: ثابت (قیمت پایه + آستانه ارسال رایگان) **یا** لحظه‌ای (استعلام از شرکت حمل‌ونقل، `livePricing`).
- **حالت پرداخت کرایه**: همراه سفارش (`allowPrepaid`)، در مقصد توسط گیرنده (`allowReceiverPay`) یا پس‌کرایه (`afterRent`).
- **محدوده جغرافیایی** (اختیاری): استان و/یا شهر.
- **زمان تحویل** (`maxDays`).

فروشنده فقط روش‌های ارسال **فروشگاه خودش** را می‌بیند و مدیریت می‌کند.

شماتیک ارتباط:

```text
پنل فروشنده  ──►  /api/Seller/Delivery      (تعریف روش‌های ارسال فروشگاه)
                        │
مشتری (سبد)   ──►  /api/EndUser/ShippingQuote      (قیمت هر روش ارسال فعال فروشگاه)
              ──►  /api/EndUser/ShippingSelection  (انتخاب یک Quote)
              ──►  SetOrder + پرداخت
                        │
بک‌اند        ──►  Shipment برای Providerهای لحظه‌ای ساخته می‌شود (بخش ۸)
```

---

## ۲. قواعد عمومی API

- Base URL: `/api/Seller/Delivery`
- همه Endpointها `[Authorize]` هستند: هدر `Authorization: Bearer {accessToken}` الزامی است.
- **`storeId` هرگز از فرانت گرفته نمی‌شود.** بک آن را از کاربر لاگین‌شده (`CurrentUser.StoreId`) می‌خواند. اگر کاربر فروشگاه فعال نداشته باشد → **HTTP 400** با بدنه:

```json
{ "isSuccess": false, "messages": [{ "item1": "فروشگاه فعالی برای کاربر جاری یافت نشد.", "item2": "" }], "code": 0 }
```

- بقیه خطاها (اعتبارسنجی، پیدا نشدن رکورد، مالکیت) معمولاً **HTTP 200 با `isSuccess: false`** هستند. موفقیت فقط وقتی است که HTTP موفق **و** `isSuccess === true` باشد.
- قالب استاندارد پاسخ:

```json
{
  "isSuccess": true,
  "messages": [ { "item1": "متن پیام", "item2": "" } ],
  "code": 0,
  "data": { }
}
```

  متن خطا: `messages[0].item1`.
- Enumها در JSON **عددی** هستند (نه رشته). مثلاً `shippingProvider: 1`.
- مالکیت: اگر `id` متعلق به فروشگاه دیگری باشد، رفتار مثل «پیدا نشد» است:
  `"روش ارسال یافت نشد یا متعلق به این فروشگاه نیست."`

---

## ۳. مدل داده و توضیح فیلدها

### 3.1 بدنه ثبت/ویرایش (`DeliveryDto`)

| فیلد | نوع | پیش‌فرض اگر ارسال نشود | توضیح |
|---|---|---|---|
| `id` | long | `0` | در `POST` نادیده گرفته می‌شود (بک `0` می‌گذارد). در `PUT` الزامی است. |
| `deliveryTypeId` | long | `0` (نامعتبر) | شناسه `Code` نوع ارسال، **حتماً از `GET /types`**. هاردکد نکنید. |
| `basePrice` | number | `0` | هزینه ارسال پایه (ریال). |
| `minPriceForFree` | number | `0` | حداقل مبلغ سبد فروشگاه برای ارسال رایگان (بخش ۷.۱ را ببینید). |
| `minCountForFree` | int | `0` | حداقل تعداد آیتم برای ارسال رایگان. |
| `maxDays` | int | `0` | حداکثر روز تحویل. `0` یعنی «ارسال فوری/بلافاصله». |
| `stateId` | long? | `null` | محدودیت به استان. `null` یعنی همه استان‌ها. |
| `cityId` | long? | `null` | محدودیت به شهر. `null` یعنی همه شهرها. |
| `active` | bool | **`false`** | ⚠️ اگر ارسال نشود، روش ارسال **غیرفعال** ثبت می‌شود. همیشه صریح بفرستید. |
| `afterRent` | bool | `false` | پس‌کرایه (مشتری کرایه را هنگام دریافت مستقیم به پیک/باربری می‌دهد). |
| `shippingProvider` | int (enum) | `0` | ارائه‌دهنده ارسال لحظه‌ای؛ جدول پایین. |
| `livePricing` | bool | `false` | قیمت‌گیری لحظه‌ای از Provider. نیازمند `shippingProvider ≠ 0`. |
| `allowPrepaid` | bool | **`true`** | کرایه همراه سفارش (آنلاین) پرداخت شود. |
| `allowReceiverPay` | bool | `false` | کرایه در مقصد توسط گیرنده پرداخت شود. |
| `storeId`, `deleted` | — | — | **ارسال نکنید.** بک در ثبت، `storeId` را از کاربر و `deleted=false` را ست می‌کند. |

> نکته: `PUT` هم همه فیلدهای بالا را **جایگزین** می‌کند (Partial Update نیست). در ویرایش، مقدار فعلی همه فیلدها را بفرستید، وگرنه فیلدهای فراموش‌شده به پیش‌فرض برمی‌گردند (مثلاً `active` به `false`).

### 3.2 `shippingProvider` (`ShippingProviderEnum`)

| مقدار | نام | توضیح |
|---|---|---|
| `0` | None | روش داخلی/ثابت (پیک فروشگاه، پست، حضوری، …) |
| `1` | AloPeyk | الوپیک |
| `2` | Tipax | تیپاکس |
| `3` | SnappBox | اسنپ‌باکس |
| `4` | Miare | میاره |

### 3.3 نوع ارسال (Code گروه `DeliveryType`)

از `GET /api/Seller/Delivery/types` بخوانید. Labelهای شناخته‌شده بک (`DeliveryTypeEnum`):

| Label | مقدار Enum (مرجع) | معنی |
|---|---|---|
| `DeliveryType_Courier` | 88 | پیک |
| `DeliveryType_Post` | 89 | پست |
| `DeliveryType_Tipax` | 90 | تیپاکس |
| `DeliveryType_InStore` | 91 | تحویل حضوری |
| `DeliveryType_AloPeyk` | 92 | الوپیک |
| `DeliveryType_SnappBox` | 93 | اسنپ‌باکس |
| `DeliveryType_Miare` | 94 | میاره |

⚠️ شناسه واقعی `Code` در دیتابیس (`deliveryTypeId`) لزوماً با مقدار Enum بالا یکی نیست (بعد از بازسازی دیتابیس تغییر می‌کند). **فقط `id` برگشتی از `/types` را ارسال کنید.** عنوان نمایشی را از `name` بگیرید.

### 3.4 پاسخ خواندن (`DeliveryVDto`)

همه فیلدهای `DeliveryDto` به‌علاوه:

```json
{
  "id": 7,
  "deliveryTypeId": 88,
  "basePrice": 80000,
  "minPriceForFree": 1000000,
  "minCountForFree": 0,
  "maxDays": 1,
  "cityId": null,
  "stateId": null,
  "active": true,
  "deleted": false,
  "storeId": 12,
  "afterRent": false,
  "shippingProvider": 0,
  "livePricing": false,
  "allowPrepaid": true,
  "allowReceiverPay": false,
  "store": { },
  "city": { },
  "state": { },
  "deliveryType": { "id": 88, "name": "پیک فروشگاه", "label": "DeliveryType_Courier" },
  "deliveryDistance": [ { "fromD": 0, "toD": 5, "price": 30000 } ]
}
```

`deliveryDistance` پله‌های قیمت بر اساس فاصله است؛ در پنل فروشنده قابل مدیریت **نیست** (بخش ۱۲).

---

## ۴. لیست Endpointها

### 4.1 انواع فعال ارسال

```http
GET /api/Seller/Delivery/types
```

پاسخ موفق (`data`: آرایه‌ای مرتب‌شده بر اساس `priority` سپس `id`):

```json
{
  "isSuccess": true,
  "messages": [],
  "code": 0,
  "data": [
    { "id": 88, "name": "پیک فروشگاه", "label": "DeliveryType_Courier", "priority": 1 }
  ]
}
```

- فقط Codeهای **فعال** که `label` آن‌ها یکی از نام‌های `DeliveryTypeEnum` است برگردانده می‌شود.
- اگر هیچ نوع فعالی نباشد: `isSuccess: false` و پیام `"هیچ نوع روش ارسال فعالی در تنظیمات سیستم یافت نشد."` با `data: []`.
- ⚠️ یک نوع ارسال زمانی در این لیست ظاهر می‌شود که Code آن در دیتابیس **وجود داشته و فعال** باشد. Migrationهای موجود Codeهای `Courier/Post/Tipax/InStore/AloPeyk/SnappBox` را دارند؛ ولی برای **`DeliveryType_Miare` هیچ Migrationای Code نمی‌سازد** و باید از پنل ادمین (مدیریت Code، گروه `DeliveryType`) ساخته شود تا در این لیست بیاید.

### 4.2 لیست روش‌های ارسال فروشگاه

```http
GET /api/Seller/Delivery?PageIndex=1&PageSize=20&SortBy=1&DeliveryTypeEnum=88
```

| پارامتر | توضیح |
|---|---|
| `PageIndex` | پیش‌فرض `1` |
| `PageSize` | پیش‌فرض `20` |
| `SortBy` | `1`=جدید (پیش‌فرض)، `2`=قدیمی، `0`=بدون مرتب‌سازی مشخص. سایر مقادیر `SortEnum` برای این لیست اثری ندارند. |
| `DeliveryTypeEnum` | فیلتر بر اساس نوع؛ مقدار Enum (`88..94`) یا نام (`DeliveryType_Courier`). فیلتر روی `label` انجام می‌شود. |
| `StoreId` | **نادیده گرفته می‌شود**؛ بک آن را با فروشگاه کاربر جایگزین می‌کند. |

⚠️ پاسخ این Endpoint **داخل `BaseResultDto` نیست** و مستقیماً خود شیء جستجوست:

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "q": null,
  "sortBy": 1,
  "available": null,
  "totalCount": 3,
  "deliveryTypeEnum": null,
  "storeId": 12,
  "list": [ { "id": 7, "deliveryTypeId": 88, "...": "..." } ]
}
```

- روش‌های حذف‌شده (`deleted`) نمایش داده نمی‌شوند؛ روش‌های **غیرفعال** نمایش داده می‌شوند (برای نمایش Toggle فعال/غیرفعال).
- `deliveryDistance` در آیتم‌های لیست بارگذاری نمی‌شود؛ برای جزئیات از `GET /{id}` استفاده کنید.

### 4.3 جزئیات یک روش ارسال

```http
GET /api/Seller/Delivery/{id}
```

- موفق: `BaseResultDto<DeliveryVDto>`.
- پیدا نشد/متعلق به فروشگاه دیگر/حذف‌شده: `isSuccess:false`، `data:null`، پیام «روش ارسال یافت نشد یا متعلق به این فروشگاه نیست.»

### 4.4 ثبت روش ارسال

```http
POST /api/Seller/Delivery
Content-Type: application/json
```

```json
{
  "deliveryTypeId": 88,
  "basePrice": 80000,
  "minPriceForFree": 1000000,
  "minCountForFree": 5,
  "maxDays": 1,
  "stateId": null,
  "cityId": null,
  "active": true,
  "afterRent": false,
  "shippingProvider": 0,
  "livePricing": false,
  "allowPrepaid": true,
  "allowReceiverPay": false
}
```

- موفق: `BaseResultDto<DeliveryDto>` که `data.id` شناسه جدید است.
- ناموفق: `isSuccess:false` با پیام‌های بخش ۵؛ `data` همان بدنه ارسالی است.

### 4.5 ویرایش

```http
PUT /api/Seller/Delivery
```

بدنه مانند ثبت + `id` روش ارسال. پاسخ: `BaseResultDto` (بدون `data`).
اگر `id` متعلق به این فروشگاه نباشد، پیام «روش ارسال یافت نشد یا متعلق به این فروشگاه نیست.»

### 4.6 حذف

```http
DELETE /api/Seller/Delivery?id={deliveryId}
```

حذف **نرم** است: `deleted=true` و `active=false`. رکورد از لیست و از خرید مشتری خارج می‌شود ولی از دیتابیس پاک نمی‌شود (سفارش‌ها و Quoteهای قبلی سالم می‌مانند).

---

## ۵. قوانین اعتبارسنجی و پیام خطاها

ثبت و ویرایش، به‌ترتیب زیر بررسی می‌شوند و **اولین خطا** برگردانده می‌شود:

| # | شرط خطا | پیام |
|---|---|---|
| 1 | کاربر فروشگاه ندارد | فروشگاه فعالی برای کاربر جاری یافت نشد. |
| 2 | `deliveryTypeId ≤ 0` یا Code فعالی با آن شناسه و Label معتبر نیست | نوع روش ارسال معتبر نیست. |
| 3 | `basePrice`، `minPriceForFree`، `minCountForFree` یا `maxDays` منفی است | مقادیر هزینه، تعداد و زمان تحویل نمی‌توانند منفی باشند. |
| 4 | `shippingProvider` جزو مقادیر `0..4` نیست | ارائه‌دهنده ارسال معتبر نیست. |
| 5 | `livePricing=true` و `shippingProvider=0` | برای قیمت‌گذاری لحظه‌ای باید ارائه‌دهنده ارسال انتخاب شود. |
| 6 | `allowPrepaid=false` و `allowReceiverPay=false` و `afterRent=false` | حداقل یکی از حالت‌های پرداخت کرایه باید فعال باشد. |
| 7 | `cityId` در دیتابیس نیست | شهر انتخاب‌شده معتبر نیست. |
| 8 | `stateId` در دیتابیس نیست | استان انتخاب‌شده معتبر نیست. |
| 9 | هر دو ارسال شده‌اند ولی شهر متعلق به استان نیست | شهر انتخاب‌شده متعلق به استان انتخاب‌شده نیست. |

نکته: در `PUT` اگر `id` پیدا نشود، قبل از اعتبارسنجی بالا خطای «روش ارسال یافت نشد…» برمی‌گردد.

---

## ۶. ترکیب‌های رایج تنظیم

**الف) پیک فروشگاه با هزینه ثابت، کرایه آنلاین**

```json
{ "deliveryTypeId": 88, "basePrice": 80000, "maxDays": 1, "active": true,
  "allowPrepaid": true, "allowReceiverPay": false, "afterRent": false,
  "shippingProvider": 0, "livePricing": false }
```

**ب) ارسال رایگان بالای مبلغ و تعداد مشخص** (هر دو مقدار باید بزرگ‌تر از صفر باشند؛ بخش ۷.۱)

```json
{ "deliveryTypeId": 89, "basePrice": 60000, "minPriceForFree": 1500000, "minCountForFree": 2,
  "maxDays": 4, "active": true, "allowPrepaid": true }
```

**ج) پس‌کرایه (مثلاً تیپاکس/باربری)**

```json
{ "deliveryTypeId": 90, "basePrice": 0, "maxDays": 3, "active": true,
  "afterRent": true, "allowPrepaid": false, "allowReceiverPay": false }
```

**د) تحویل حضوری از فروشگاه**

```json
{ "deliveryTypeId": 91, "basePrice": 0, "maxDays": 0, "active": true, "allowPrepaid": true }
```

**ه) قیمت‌گیری لحظه‌ای (الوپیک)**

```json
{ "deliveryTypeId": 92, "basePrice": 0, "maxDays": 0, "active": true,
  "shippingProvider": 1, "livePricing": true, "allowPrepaid": true, "allowReceiverPay": true }
```

**و) محدود به یک شهر/استان**: `stateId` و/یا `cityId` را مقدار دهید (شهر باید متعلق به استان باشد).

---

## ۷. اثر تنظیمات روی خرید مشتری

دو مسیر در سیستم وجود دارد و هر دو از همین رکوردها استفاده می‌کنند.

### 7.1 قیمت ثابت (`livePricing=false` یا `shippingProvider=0`)

قیمت نهایی برای یک سبد از این ترتیب محاسبه می‌شود:

1. شروع با `basePrice`.
2. **ارسال رایگان** فقط وقتی اعمال می‌شود که **همزمان**:
   `minPriceForFree > 0` **و** `minCountForFree > 0` **و** تعداد کل آیتم‌های سبد ≥ `minCountForFree` **و** مبلغ سبد آن فروشگاه ≥ `minPriceForFree`.
   (تنظیم فقط یکی از این دو، ارسال را رایگان **نمی‌کند**؛ فقط متن «ارسال رایگان بالای …» نمایش داده می‌شود. → بخش ۱۲)
3. اگر رایگان نشد و برای این روش «پله فاصله» (`DeliveryDistance`) تعریف شده و آدرس مشتری مختصات دارد: قیمت از پله‌ای که فاصله در بازه‌اش است خوانده می‌شود؛ اگر هیچ پله‌ای مطابق نبود، **این روش برای مشتری نمایش داده نمی‌شود**.
4. اگر قیمت `0` شد: متن نمایشی «پس‌کرایه» (اگر `afterRent` و رایگان نبود) یا «رایگان».
5. `maxDays > 0` → «تحویل تا N روز»؛ `maxDays = 0` → «ارسال فوری».

### 7.2 حالت پرداخت (Quote)

در مسیر جدید (`POST /api/EndUser/ShippingQuote`) برای هر روش ارسال فعال فروشگاه یک یا دو Quote ساخته می‌شود:

| نوع روش | Prepaid | ReceiverPays |
|---|---|---|
| ثابت | اگر `allowPrepaid` و `afterRent=false` | اگر `allowReceiverPay` **یا** `afterRent` |
| لحظه‌ای (Provider) | اگر `allowPrepaid` | اگر `allowReceiverPay` |

- `Prepaid`: کرایه به مبلغ قابل پرداخت سفارش اضافه می‌شود.
- `ReceiverPays`: `quotedPrice` فقط برآورد است و `payableDeliveryPrice = 0`؛ اپ باید «پرداخت کرایه در مقصد» نمایش دهد، نه «رایگان».

### 7.3 قیمت لحظه‌ای (`livePricing=true`)

- برای Quote نیاز است **فروشگاه و آدرس مشتری هر دو مختصات (`Location`) داشته باشند**؛ در غیر این‌صورت آن روش بی‌صدا رد می‌شود.
- اگر Provider خطا بدهد یا پوشش ندهد، همان روش در نتیجه نمی‌آید. اگر هیچ روشی Quote نشود، مشتری پیام «در حال حاضر امکان دریافت قیمت ارسال وجود ندارد.» می‌بیند.
- ورودی قیمت‌گیری: مختصات مبدأ/مقصد، وزن کل (مجموع وزن×تعداد، حداقل وزن پیش‌فرض)، طول/عرض = بیشینه اقلام، ارتفاع = مجموع ارتفاع×تعداد، و ارزش کالا. (بخش ۹)
- Quote پیش‌فرض ۵ دقیقه اعتبار دارد و به کاربر، سبد، آدرس و محتوای سبد بسته است؛ با تغییر آدرس یا سبد باید دوباره قیمت گرفت.

### 7.4 فیلتر منطقه (مسیر قدیمی `GetDeliveries`)

در مسیر قدیمی سبد (`CartUpdateType` نمایش روش‌های ارسال):

- اگر آدرس روی سبد انتخاب شده باشد: روش‌هایی نمایش داده می‌شوند که `cityId` آن برابر شهر آدرس باشد، یا `cityId=null` و `stateId` برابر استان آدرس، یا هر دو `null`.
- اگر آدرس انتخاب نشده باشد: **فقط** روش‌هایی با نوع `DeliveryType_InStore`.
- نتیجه بر اساس ارزان‌ترین قیمت مرتب می‌شود.

---

## ۸. بعد از پرداخت: Shipment و پیگیری

- بعد از پرداخت موفق، برای هر فروشگاهِ سفارش که روش ارسالش Provider دارد (`shippingProvider ≠ 0`) یک `Shipment` ساخته می‌شود (یک‌بار برای هر `ProductOrderStore`؛ idempotent).
- خطای Provider نتیجه پرداخت را تغییر **نمی‌دهد**؛ فقط `Shipment.Status = Failed` و `failureReason` ذخیره می‌شود.
- لغو سفارش، Shipmentهای غیرنهایی را (با فراخوانی Provider) لغو می‌کند.

وضعیت‌های `Shipment`:

| مقدار | نام | معنی |
|---|---|---|
| 1 | Pending | ساخته شد، هنوز به Provider ارسال نشده |
| 2 | Requested | درخواست به Provider ثبت شد |
| 3 | Accepted | پذیرفته/پیک تخصیص یافت (میاره: `pickup`) |
| 4 | PickedUp | بار تحویل پیک شد (میاره: `dropoff`) |
| 5 | Delivered | تحویل مشتری شد |
| 6 | Cancelled | لغو شد |
| 7 | Failed | ناموفق (شامل `returning` میاره) |

**میاره (Miare)** به‌صورت Webhook (`POST /api/webhooks/miare`، احراز هویت با Token اختصاصی میاره) وضعیت را به‌روز می‌کند و در رویداد `delivered` هزینه واقعی سفر در `Shipment.ProviderCost` ذخیره می‌شود. برای جزئیات فنی [BACKEND_LIVE_SHIPPING_FA.md](BACKEND_LIVE_SHIPPING_FA.md) را ببینید.

### کد رهگیری (تنظیم دستی توسط فروشنده)

```http
PUT /api/Seller/ProductOrderTrackingCode
Content-Type: application/json
```

```json
{ "id": "شناسه سفارش", "trackingCode": "123456789" }
```

- سفارش باید حداقل یک فروشگاهِ متعلق به کاربر لاگین‌شده داشته باشد؛ در غیر این‌صورت `isSuccess:false` با پیام «AccessDenied».
- کد رهگیری روی **کل سفارش** ذخیره می‌شود و اگر سفارش نوع ارسال داشته باشد، پیامک/ایمیل «کد رهگیری» با نام نوع ارسال برای مشتری ارسال می‌شود.
- این مقدار مستقل از `Shipment.TrackingCode` (کد رهگیری Provider) است.

> در حال حاضر Endpointی در پنل Seller برای خواندن وضعیت `Shipment` وجود ندارد؛ وضعیت Provider فقط در دیتابیس/بک ذخیره می‌شود.

---

## ۹. وزن و ابعاد محصول

برای دقت قیمت لحظه‌ای، در افزودن/ویرایش محصول (`/api/Seller/Product…`) این فیلدها ارسال شود:

```json
{
  "shippingWeightGrams": 1500,
  "shippingLengthCm": 30,
  "shippingWidthCm": 20,
  "shippingHeightCm": 15
}
```

واحدها: گرم و سانتی‌متر. اگر خالی باشند، مقادیر پیش‌فرض سرور استفاده می‌شود (۱۰۰۰ گرم و ۲۰×۲۰×۲۰ سانتی‌متر).

---

## ۱۰. تنظیمات سرور

بخش `Shipping` در `appsettings` (کلیدها در Git/appsettings نگذارید؛ از متغیر محیطی):

| کلید | پیش‌فرض | توضیح |
|---|---|---|
| `Shipping:TestMode` | `true` | در حالت تست، الوپیک/تیپاکس/اسنپ‌باکس قیمت و Shipment تستی می‌سازند. |
| `Shipping:QuoteTtlMinutes` | `5` | اعتبار Quote (بین ۱ تا ۳۰ دقیقه). |
| `Shipping:DefaultWeightGrams` | `1000` | وزن پیش‌فرض. |
| `Shipping:DefaultLength/Width/HeightCm` | `20` | ابعاد پیش‌فرض. |
| `Shipping:{Provider}:Enabled/BaseUrl/ApiKey` | — | برای `AloPeyk`, `Tipax`, `SnappBox`, `Miare` (میاره: `AccountingBaseUrl` هم). |

وضعیت Adapterها: الوپیک، تیپاکس و اسنپ‌باکس فعلاً Test Mode هستند (در Production بدون اتصال رسمی قیمت واقعی نمی‌سازند). میاره به API واقعی (Staging) متصل است.

---

## ۱۱. راهنمای پیاده‌سازی فرانت

### ترتیب پیشنهادی صفحه

1. هنگام باز شدن صفحه: `GET /api/Seller/Delivery` برای لیست روش‌ها.
2. هنگام باز شدن فرم ثبت/ویرایش: `GET /api/Seller/Delivery/types` و ساخت DropDown از `data` (مقدار = `id`، عنوان = `name`).
3. فرم:
   - وقتی `livePricing` روشن شد: انتخاب `shippingProvider` اجباری شود؛ فیلدهای `basePrice`/آستانه رایگان برای آن غیرفعال یا پنهان شوند (قیمت را Provider می‌دهد).
   - وقتی `afterRent` روشن شد: `basePrice` را `0` بفرستید و فیلد را غیرفعال کنید.
   - حداقل یکی از `allowPrepaid` / `allowReceiverPay` / `afterRent` انتخاب شود.
   - اگر `cityId` انتخاب می‌شود، `stateId` همان استان باشد.
   - `active` همیشه صریح ارسال شود.
4. قبل از Submit: منفی نبودن اعداد را چک کنید.
5. بعد از Submit: `isSuccess` را چک کنید و در خطا `messages[0].item1` را نمایش دهید.
6. Toggle فعال/غیرفعال: با `PUT` و ارسال **همه فیلدها** (Partial نیست).
7. حذف: `DELETE ?id=` با تأیید کاربر.

### نمایش خطا

```ts
const message = response?.messages?.[0]?.item1 || 'عملیات ناموفق بود'
```

### Nuxt Server API (`delivery.post.js`, `delivery.put.js`, …)

شیء خام `error` را `return` نکنید؛ پاسخ بک را حفظ کنید:

```ts
catch (error) {
  throw createError({
    statusCode: error?.response?.status || error?.statusCode || 500,
    statusMessage: error?.data?.messages?.[0]?.item1 || 'خطا در ارتباط با سرور',
    data: error?.data
  })
}
```

### تفاوت ساختار پاسخ

| Endpoint | ساختار |
|---|---|
| `GET /types`, `GET /{id}`, `POST`, `PUT`, `DELETE` | `BaseResultDto` (`isSuccess`, `messages`, `data`) |
| `GET /` (لیست) | مستقیم `{ totalCount, list, pageIndex, … }` بدون `isSuccess` |
| نبود فروشگاه | HTTP 400 |

---

## ۱۲. نکات مهم و رفتارهای شناخته‌شده

این موارد از روی کد فعلی استخراج شده‌اند و باید در طراحی UI لحاظ شوند یا در بک اصلاح شوند:

1. **`afterRent` در ثبت (`POST`) `basePrice` را صفر نمی‌کند؛ فقط در ویرایش (`PUT`) صفر می‌شود.** فرانت هنگام `afterRent=true` خودش `basePrice: 0` بفرستد. (مستند قبلی این را برای هر دو فرض کرده بود.)
2. **ارسال رایگان نیاز به هر دو شرط دارد:** `minPriceForFree > 0` و `minCountForFree > 0`. با `minCountForFree = 0` (مثال مستند قبلی) ارسال هرگز رایگان نمی‌شود، هرچند متن «ارسال رایگان بالای …» نمایش داده می‌شود.
3. **محدوده جغرافیایی (`cityId/stateId`) فقط در مسیر قدیمی `GetDeliveries` اعمال می‌شود.** در مسیر Quote (`ShippingQuote`) روش‌های فعال فروشگاه بدون فیلتر شهر/استان Quote می‌شوند.
4. **پله‌های فاصله (`DeliveryDistance`) فقط از پنل ادمین (`/api/Admin/DeliveryDistance`) قابل مدیریت‌اند** و فروشنده Endpointی برای آن ندارد. فاصله نسبت به `BaseDetail.Location` (مختصات پایه پلتفرم) محاسبه می‌شود، نه مختصات فروشگاه؛ و اگر پله‌ای مطابق نباشد روش برای مشتری مخفی می‌شود.
5. **نوع `DeliveryType_Miare` باید دستی Code داشته باشد** (Migration ندارد)، وگرنه در `/types` نمی‌آید.
6. `PUT` جایگزینی کامل است؛ فیلدهای ارسال‌نشده به پیش‌فرض DTO برمی‌گردند (`active=false`, `allowPrepaid=true`, …).
7. `GET /` لیست، روش‌های غیرفعال را هم برمی‌گرداند و پاسخش `BaseResultDto` نیست.
8. `DELETE` نرم است و با `active=false` همراه است.
9. مسیر لحظه‌ای `afterRent` را در نظر نمی‌گیرد؛ برای Provider فقط `allowPrepaid` / `allowReceiverPay` تعیین‌کننده است.

---

## منابع مرتبط

- [APP_LIVE_SHIPPING_FA.md](APP_LIVE_SHIPPING_FA.md): سمت مشتری (Quote/Selection/سبد)
- [BACKEND_LIVE_SHIPPING_FA.md](BACKEND_LIVE_SHIPPING_FA.md): معماری Providerها، Webhook و Migrationها
- [CODE_GROUPS_AND_CODES_FRONTEND_FA.md](CODE_GROUPS_AND_CODES_FRONTEND_FA.md): مدیریت Codeها

کد مرجع:

- `Api/Areas/Seller/Controllers/DeliveryController.cs`
- `Application/Services/Order/DeliverySrv/DeliveryService.cs`
- `Application/Services/Order/ShippingSrv/ShippingQuoteService.cs`, `ShipmentService.cs`
- `Entities/Entities/Delivery.cs`, `Entities/Entities/ShippingField/*`
