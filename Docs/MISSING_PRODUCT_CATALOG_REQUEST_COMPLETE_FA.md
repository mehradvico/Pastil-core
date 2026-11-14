# مستند کامل درخواست افزودن محصول به کاتالوگ (`MissingProduct`)

مبنای این سند کد بک‌اند مخزن در commit `d15ef9e1923b5d754c45afb30600a91cee6c68b3` است. مسیرها نسبت به میزبان API نوشته شده‌اند. این سند قرارداد فعلی را توضیح می‌دهد؛ وجود endpoint در نسخهٔ مستقر و تنظیمات منو باید در همان محیط بررسی شود.

## ۱. مفهوم و مرز دو مسیر

`MissingProduct` درخواست فروشگاهی برای **ساخت محصول جدید در کاتالوگ اصلی** است؛ خودِ ثبت درخواست، محصول کاتالوگ یا موجودی فروشگاه نمی‌سازد. پس از بررسی و تأیید ادمین، یک `Product` ساخته می‌شود. اگر قیمت مؤثر درخواست بزرگ‌تر از صفر باشد، سرویس تلاش می‌کند یک `ProductItem` برای همان فروشگاه نیز بسازد.

| وضعیت کالا | مسیر صحیح |
|---|---|
| محصول در کاتالوگ عمومی وجود دارد، ولی فروشگاه هنوز آن را ندارد | `GET /api/Product?q=...` برای یافتن `productId`، سپس `GET /api/Seller/ProductItem/id?id={productId}` و `POST /api/Seller/ProductItem` برای قیمت و موجودی؛ `MissingProduct` لازم نیست. |
| محصول واقعاً در کاتالوگ نیست | `POST /api/Seller/MissingProduct` ← تکمیل/تصویر ← `submit` ← بررسی ادمین. |
| اسکن هوشمند نتوانست جست‌وجوی کاتالوگ را تمام کند | `catalogMatchFailed=true` یعنی نتیجه نامعلوم است؛ آن ردیف را خودکار «ناموجود» حساب نکنید. |

**نکتهٔ مهم جست‌وجو:** `GET /api/Seller/Product` در کنترلر فروشنده `StoreId` کاربر را روی جست‌وجو می‌گذارد و فقط محصولاتی را می‌یابد که `ProductItem` همین فروشگاه را دارند. برای بررسی وجود کالا در **کل کاتالوگ** از `GET /api/Product?q=...` استفاده کنید. جست‌وجو با نام/نام دوم انجام می‌شود؛ مشابه‌بودن نام به‌تنهایی اثبات یکسان‌بودن برند، وزن یا نوع محصول نیست.

## ۲. دسترسی، قالب پاسخ و شناسه‌ها

- همهٔ مسیرهای `api/Seller/MissingProduct` به `Authorization: Bearer {accessToken}` نیاز دارند. `storeId` از توکن/کاربر جاری گرفته می‌شود؛ در بدنهٔ درخواست فروشنده ارسال نشود. فروشنده فقط رکوردهای فروشگاه خودش را می‌بیند و تغییر می‌دهد.
- همهٔ مسیرهای `api/Admin/MissingProduct` به سیاست `AdminOnly` نیاز دارند. فروشنده با توکن خودش نمی‌تواند approve/reject بزند.
- نتیجهٔ سرویس‌های `MissingProduct` در پوشش `{ "isSuccess": boolean, "code": number, "messages": [], "data": ... }` می‌آید. بسیاری از خطاهای اعتبارسنجی با HTTP 200 و `isSuccess:false` برمی‌گردند؛ **فقط وضعیت HTTP معیار موفقیت نیست**. پیام‌های `messages` در .NET از `Tuple` ساخته می‌شوند؛ معمولاً متن در `item1` است.
- `id` شناسهٔ **درخواست** است. `productId` شناسهٔ محصول **کاتالوگ** پس از تأیید است. `productItemId` شناسهٔ آیتم **فروشگاه** است، و ممکن است با وجود محصول تأییدشده `null` بماند. `pictureId` شناسهٔ تصویر است.
- نام فیلدهای JSON در نمونه‌ها camelCase است. تاریخ‌های با پسوند `Utc` زمان UTC هستند. مقدار `pictureUrl` ممکن است مسیر نسبی باشد؛ آن را با میزبان مناسب فایل نمایش دهید.

## ۳. چرخهٔ وضعیت و نمایش در پنل‌ها

| `status` | معنی | اقدام فروشنده | حضور در فهرست ادمین |
|---|---|---|---|
| `draft` | پیش‌نویس ثبت شده؛ هنوز درخواست بررسی نیست | ویرایش، افزودن/حذف تصویر، ارسال، حذف | در فهرست پیش‌فرض نیست؛ فقط با `status=draft` قابل جست‌وجو است. |
| `submitted` | در صف بررسی | فقط نمایش | در فهرست پیش‌فرض و فیلتر صف هست. |
| `approved` | ادمین تأیید و محصول کاتالوگ ساخته شده | فقط نمایش و استفاده از `productId` | در تاریخچه و فیلتر approved هست. |
| `rejected` | ادمین با دلیل رد کرده | نمایش دلیل؛ ویرایش یا حذف | در تاریخچه و فیلتر rejected هست. |

گذار مجاز: `draft → submitted → approved/rejected` و `rejected → draft → submitted`. **ویرایش متن یا تصویر درخواست ردشده** آن را به `draft` برمی‌گرداند و `rejectionReason` را پاک می‌کند؛ برای ورود دوباره به صف باید `submit` فراخوانی شود. روی `submitted` و `approved` ویرایش/حذف مجاز نیست. endpoint «لغو ارسال» وجود ندارد.

## ۴. API فروشنده

| متد و مسیر | کار | بدنه |
|---|---|---|
| `POST /api/Seller/MissingProduct` | ساخت پیش‌نویس‌های batch | JSON: `items[]` |
| `GET /api/Seller/MissingProduct` | فهرست درخواست‌های همین فروشگاه | ندارد؛ `pageSize` در query |
| `PUT /api/Seller/MissingProduct` | تکمیل/ویرایش پیش‌نویس یا ردشده | JSON: `MissingProductUpdateDto` |
| `POST /api/Seller/MissingProduct/{id}/picture` | افزودن یک تصویر | multipart: `image` |
| `DELETE /api/Seller/MissingProduct/{id}/picture/{pictureId}` | حذف تصویر | ندارد |
| `POST /api/Seller/MissingProduct/{id}/submit` | ارسال به صف بررسی | ندارد |
| `DELETE /api/Seller/MissingProduct/{id}` | حذف پیش‌نویس یا ردشده | ندارد |

### ۴.۱ ثبت یک یا چند پیش‌نویس

`POST /api/Seller/MissingProduct`، `Content-Type: application/json`

```json
{
  "items": [
    {"name": "غذای خشک گربه نمونه ۲ کیلوگرم", "brand": "برند نمونه", "packageSize": "۲ کیلوگرم", "source": "manual"},
    {"name": "تشویقی سگ نمونه ۱۰۰ گرم", "brand": "برند نمونه", "packageSize": "۱۰۰ گرم", "source": "shelf"}
  ]
}
```

`items` باید ۱ تا ۱۰۰ عضو داشته باشد. هر عضو: `name` رشتهٔ لازم، پس از trim حداکثر ۲۰۰ نویسه؛ `brand` و `packageSize` اختیاری، هر کدام هنگام ایجاد به ۱۰۰ نویسه بریده می‌شوند؛ `source` اختیاری، حداکثر ۳۰ نویسه و در صورت خالی‌بودن `shelf`. `source` فقط منشأ ثبت برای ردیابی است، نه وضعیت بررسی. مقدار `manual` صرفاً نمونهٔ پیشنهادی فرانت است؛ بک‌اند مقدار خاصی را به این فیلد تحمیل نمی‌کند.

پاسخ نمونه:

```json
{"isSuccess":true,"code":0,"messages":[],"data":{"created":2,"alreadyKnown":0}}
```

`created` شمار رکوردهای تازه است؛ `alreadyKnown` شمار نام‌های تکراری داخل batch یا نام‌های موجود در **همان فروشگاه** است. کلید یکتا `(storeId, normalizedName)` است: تفاوت بعضی شکل‌های «ی/ک»، رقم و فاصله پس از نرمال‌سازی ممکن است یک نام محسوب شود. تکراری‌بودن مستقل از وضعیت رکورد قبلی است؛ درخواست rejected/approved با همان نام دوباره ساخته نمی‌شود. عضو دارای نام خالی/بیش از ۲۰۰ نویسه در batch **بی‌صدا رد می‌شود و الزاماً در دو شمارنده نمی‌آید**؛ بنابراین `created + alreadyKnown` لزوماً برابر طول `items` نیست. ثبت batch `id` برنمی‌گرداند. بعد از آن فهرست را بگیرید و رکورد موردنظر را با نام/منشأ تطبیق دهید؛ اگر `alreadyKnown>0` است، رکورد قبلی را برای اصلاح یا پیگیری نشان دهید.

### ۴.۲ فهرست درخواست‌های فروشگاه

`GET /api/Seller/MissingProduct?pageSize=200`

پاسخ `data:{items:[MissingProductDto],total:number}` است. فهرست به ترتیب `id` نزولی و فقط برای فروشگاه توکن است. `pageSize` پیش‌فرض ۲۰۰، حداکثر ۲۰۰؛ مقدار صفر/منفی به ۲۰۰ تبدیل می‌شود. **شمارهٔ صفحه/offset در این endpoint وجود ندارد**؛ اگر `total>200` باشد، با همین API امکان پیمایش رکوردهای قدیمی‌تر نیست.

`MissingProductDto`:

| فیلد | نوع | توضیح |
|---|---|---|
| `id` | عدد | شناسهٔ درخواست؛ برای همهٔ عملیات بعدی. |
| `name` | رشته | نام پیشنهادی فروشنده. |
| `brand`, `packageSize` | رشته یا null | متن آزاد، شناسهٔ رسمی برند نیستند. |
| `description` | رشته یا null | توضیح آزاد فروشنده. |
| `price` | عدد صحیح یا null | قیمت پیشنهادی برای آیتم فروشگاه؛ `0` یا `null` باعث ساخت خودکار آیتم فروشگاه نمی‌شود. واحد را با قرارداد قیمت `ProductItem` سامانه یکسان نگه دارید. |
| `quantity` | عدد صحیح یا null | موجودی پیشنهادی؛ `null` هنگام ساخت خودکار به صفر می‌رسد. |
| `pictureUrl` | رشته یا null | تصویر اصلی/کاور؛ نخستین تصویر درخواست. |
| `pictures` | آرایه | هر عضو `{pictureId,pictureUrl}`؛ حداکثر پنج تصویر به ترتیب. |
| `status` | رشته | `draft`, `submitted`, `approved`, `rejected`. |
| `rejectionReason` | رشته یا null | فقط در وضعیت `rejected` پر می‌شود. |
| `source` | رشته | مقدار زمان ایجاد؛ از PUT تغییر نمی‌کند. |
| `createdAtUtc` | تاریخ UTC | زمان ساخت درخواست. |
| `productId` | عدد یا null | شناسهٔ محصول کاتالوگ پس از تأیید. |

**محدودیت پیاده‌سازی فعلی:** سرویسِ `GET` فروشنده فقط تصویر کاور را `Include` می‌کند و رابطهٔ `Pictures` را بارگیری نمی‌کند؛ بنابراین ممکن است `pictureUrl` موجود باشد ولی `pictures:[]` در فهرست برگردد. پاسخ عملیات آپلود/حذف/ویرایش تصویر، رابطه را بارگیری می‌کند. فرانت نباید خالی بودن `pictures[]` در فهرست را اثبات «بدون تصویر» بداند. endpoint جزئیات `GET /api/Seller/MissingProduct/{id}` نیز در حال حاضر وجود ندارد.

### ۴.۳ تکمیل یا اصلاح درخواست

`PUT /api/Seller/MissingProduct`، `Content-Type: application/json`

```json
{
  "id": 123,
  "name": "غذای خشک گربه نمونه ۲ کیلوگرم",
  "brand": "برند نمونه",
  "packageSize": "۲ کیلوگرم",
  "description": "طعم مرغ، بستهٔ ۲ کیلوگرمی",
  "price": 850000,
  "quantity": 4
}
```

فقط `draft` و `rejected` قابل PUT هستند. `name` لازم و حداکثر ۲۰۰ نویسه؛ `brand` و `packageSize` هر کدام حداکثر ۱۰۰؛ `description` حداکثر ۲۰۰۰؛ `price` و `quantity` در صورت حضور باید نامنفی باشند. این PUT **جایگزینی فیلدها** است: حذف فیلد اختیاری یا فرستادن `null` مقدار قبلی آن را پاک می‌کند. برای حفظ داده‌های قبلی، فرم باید همهٔ مقدارهای فعلی را بفرستد. نام نرمال‌شدهٔ تکراری در همان فروشگاه رد می‌شود. پاسخ، `MissingProductDto` به‌روز است.

### ۴.۴ افزودن و حذف تصویر

`POST /api/Seller/MissingProduct/{id}/picture`، `multipart/form-data` با فیلد دقیقاً `image`؛ **هر درخواست یک فایل**. پسوندهای `.jpg`, `.jpeg`, `.png`, `.webp` و محتوای واقعی JPEG/PNG/WebP مجازند؛ فایل باید بیش از صفر و حداکثر ۸ MiB باشد. حد بدنهٔ HTTP در کنترلر ۱۰ MiB است. حداکثر پنج تصویر برای هر درخواست. این endpoint تصویر را **به فهرست اضافه** می‌کند؛ تصویر قبلی را جایگزین نمی‌کند. توکن باید همراه درخواست باشد و سرویس آن را برای آپلود فایل نیز به کار می‌برد.

`DELETE /api/Seller/MissingProduct/{id}/picture/{pictureId}` تصویر مشخصی را از درخواست حذف می‌کند. هر دو عملیات فقط برای `draft`/`rejected` مجازند، پاسخِ موفق `MissingProductDto` جدید می‌دهد، و حذف کاور سبب می‌شود تصویر بعدی کاور شود. حذف رابطهٔ تصویر از درخواست، به معنای حذف قطعی فایل از سرویس فایل نیست. برای تعویض تصویر، ابتدا تصویر نامطلوب را DELETE و سپس تصویر جدید را POST کنید.

### ۴.۵ ارسال به صف بررسی و حذف درخواست

`POST /api/Seller/MissingProduct/{id}/submit` بدون بدنه. فقط برای `draft` با `name` غیرخالی و **حداقل یک تصویر آپلودشده** موفق است. وضعیت `submitted` و `submittedAtUtc` داخلی ثبت می‌شود؛ از این لحظه درخواست در صف ادمین قرار می‌گیرد. قیمت، توضیح، برند و موجودی برای submit الزامی نیستند.

`DELETE /api/Seller/MissingProduct/{id}` فقط برای `draft`/`rejected` مجاز است. رکورد درخواست و روابط تصاویرش حذف می‌شوند؛ این عملیات راه لغو درخواست submitted نیست.

## ۵. API ادمین و صفحهٔ «درخواست محصولات»

| متد و مسیر | کار |
|---|---|
| `GET /api/Admin/MissingProduct` | لیست و فیلتر درخواست‌ها |
| `GET /api/Admin/MissingProduct/{id}` | جزئیات و مشابه‌های کاتالوگ |
| `POST /api/Admin/MissingProduct/approve` | تأیید و ایجاد Product، سپس تلاش برای ایجاد ProductItem |
| `POST /api/Admin/MissingProduct/reject` | رد با دلیل |

### ۵.۱ فهرست و فیلتر

`GET /api/Admin/MissingProduct?status=submitted&pageNumber=1&pageSize=20&storeId=12&q=غذای%20خشک`

پارامترها: `status` یکی از `draft`, `submitted`, `approved`, `rejected` و اختیاری؛ بدون آن **همه به‌جز draft**. `storeId` اختیاری برای محدود کردن به فروشگاه؛ `q` جست‌وجوی زیررشته در نام یا برند؛ `pageNumber` از ۱ و پیش‌فرض ۱؛ `pageSize` پیش‌فرض ۲۰ و بازهٔ ۱ تا ۱۰۰. پاسخ `data:{items:[MissingProductAdminDto],total:number}` است. با `status=submitted` قدیمی‌ترین زمان ارسال اول است؛ در حالت‌های دیگر شناسهٔ جدیدتر اول. برای تب «در انتظار بررسی» **حتماً `status=submitted`** بفرستید.

هر عضو، فیلدهای `MissingProductDto` را دارد و افزون بر آنها `storeId`, `storeName`, `productItemId`, `submittedAtUtc`, `reviewedAtUtc` را برمی‌گرداند. `similarProducts` فقط در جزئیات پر می‌شود و در لیست معمولاً `null` است. `productItemId` حتی پس از approved می‌تواند null باشد. تصویرهای `pictures[]` در **فهرست ادمین** بارگیری می‌شوند.

### ۵.۲ جزئیات درخواست و بررسی مشابه‌ها

`GET /api/Admin/MissingProduct/{id}` تمام مشخصات درخواست، تصاویر، فروشگاه و `similarProducts` با حداکثر پنج کاندید از کاتالوگ را برمی‌گرداند. هر کاندید `{productId,name,brandName,pictureUrl}` دارد. جست‌وجوی مشابه کمکی است؛ اگر جست‌وجو خطا بدهد، آرایهٔ خالی برمی‌گردد و خود درخواست همچنان نمایش داده می‌شود. قبل از approve، ادمین باید یکسانی محصول و گونه/وزن/برند را بررسی کند، چون approve فعلی **همیشه Product جدید می‌سازد**؛ endpoint «اتصال درخواست به محصول موجود» در این کنترلر وجود ندارد.

### ۵.۳ تأیید و ساخت محصول واقعی

`POST /api/Admin/MissingProduct/approve`، `Content-Type: application/json`

```json
{
  "id": 123,
  "name": "غذای خشک گربه نمونه ۲ کیلوگرم",
  "productLabel": "cat-food-sample-2kg",
  "categoryId": 45,
  "brandId": 9,
  "secondName": "Sample Cat Food 2kg",
  "summary": "غذای خشک گربه",
  "description": "توضیح رسمی محصول",
  "pictureId": null,
  "price": 850000,
  "quantity": 4
}
```

| فیلد | الزام/رفتار |
|---|---|
| `id` | لازم، شناسهٔ درخواستِ `submitted`. |
| `categoryId` | لازم و باید دستهٔ موجود باشد. |
| `productLabel` | **در فرم ادمین الزامی در نظر بگیرید**: یک label غیرخالی با دست‌کم یک حرف انگلیسی یا رقم بدهید، مثل `cat-food-sample-2kg`. حروف/ارقام غیرلاتین رد می‌شوند؛ فاصله/نشانه‌ها در slug به جداکننده تبدیل می‌شوند. یکتایی slug هنگام ایجاد محصول بررسی می‌شود. DTO این فیلد را nullable تعریف کرده ولی سرویس بعداً `Trim()` بدون بررسی null صدا می‌زند؛ فرستادن null ممکن است خطای سرور بدهد. |
| `name` | اختیاری؛ خالی = نام درخواست فروشنده؛ حداکثر ۲۰۰ نویسه. |
| `brandId` | اختیاری، **شناسهٔ برند رسمی** موجود. متن `brand` فروشنده خودکار به Brand تبدیل نمی‌شود. |
| `secondName`, `summary` | اختیاری، برای Product اصلی؛ محدودیت‌های نهایی سرویس Product بر آنها اعمال می‌شود. |
| `description` | اختیاری؛ خالی = `description` درخواست. |
| `pictureId` | اختیاری؛ در صورت مقدار داشتن باید Picture موجود باشد؛ خالی = کاور درخواست. |
| `price`, `quantity` | اختیاری؛ در صورت ارسال نامنفی. مقدار مؤثر هر کدام = مقدار ادمین اگر داده شده، وگرنه مقدار درخواست فروشنده. |

پاسخ موفق نمونه:

```json
{"isSuccess":true,"code":0,"messages":[],"data":{"productId":456,"storeItemCreated":true,"productItemId":789}}
```

خروجی approve یک `Product` فعال، با وضعیت Available و نوع Product می‌سازد. `productId` روی درخواست ذخیره می‌شود. کاور درخواست یا `pictureId` جایگزین، تصویر اصلی Product است. سرویس سپس **تلاش می‌کند** همهٔ تصاویر درخواست را به گالری Product کپی کند؛ خطای این مرحله تأیید را برنمی‌گرداند. اگر قیمت مؤثر `>0` باشد، سرویس برای فروشگاه درخواست‌دهنده یک ProductItem فعال، بدون تنوع، با موجودی مؤثر (`null→0`) ایجاد می‌کند. اگر قیمت `null` یا صفر باشد، `storeItemCreated:false` طبیعی است. شکست ایجاد ProductItem نیز تأیید Product را برنمی‌گرداند؛ بنابراین فرانت باید `storeItemCreated` و `productItemId` را بررسی کند و در صورت نیاز فروشنده را به فرم قیمت/موجودی محصول هدایت کند. اگر خود ایجاد Product شکست بخورد، سرویس برای بازگرداندن درخواست به `submitted` تلاش می‌کند و پاسخ ناموفق می‌دهد.

### ۵.۴ رد درخواست

`POST /api/Admin/MissingProduct/reject`

```json
{"id":123,"reason":"تصویر وزن و مشخصات محصول خوانا نیست؛ لطفاً عکس واضح‌تری ارسال کنید."}
```

`reason` پس از trim لازم و حداکثر ۵۰۰ نویسه است. فقط درخواست `submitted` رد می‌شود. پاسخ موفق `MissingProductAdminDto` با `status:"rejected"`, `rejectionReason` و زمان بررسی است. فروشنده می‌تواند اصلاح کند؛ نخستین اصلاح وضعیت را به `draft` برمی‌گرداند و باید دوباره submit کند.

## ۶. پیاده‌سازی فرم و سناریوی انتها به انتها

1. فروشنده نام کالا را در `GET /api/Product?q=...` جست‌وجو کند و برند/وزن را با نتیجه بسنجد. اگر محصول موجود بود، شناسهٔ آن را به فرم `GET /api/Seller/ProductItem/id?id={productId}` ببرد و با `POST /api/Seller/ProductItem` قیمت/موجودی را ثبت کند. در `ProductItem`، `storeId` از توکن تحمیل می‌شود؛ برای محصول دارای تنوع باید سطرهای معتبر همان محصول را از پاسخ فرم انتخاب کرد.
2. اگر کالا واقعاً موجود نبود، با `POST /api/Seller/MissingProduct` یک `items` تک‌عضوی یا batch تا ۱۰۰ عضو بفرستد. برای ورود دستی می‌توان `source:"manual"` و برای اسکن قفسه `source:"shelf"` گذاشت. API ساخت یک محصولِ مستقل جدا از batch ندارد.
3. `GET /api/Seller/MissingProduct` بگیرد تا `id` پیش‌نویس را پیدا کند؛ چون پاسخ POST شناسه ندارد. اگر نام از قبل ثبت شده، وضعیت همان رکورد را به فروشنده نشان دهد.
4. با `PUT` توضیح، قیمت و موجودی را ثبت کند. قیمت و موجودی را از واحد/قرارداد معمول آیتم فروشگاه بگیرد.
5. برای هر تصویر یک `POST .../{id}/picture` با فیلد `image` بزند. نتیجهٔ هر فراخوانی را نگه دارد؛ تا پنج تصویر.
6. با `POST .../{id}/submit` به بررسی بفرستد. دکمهٔ ارسال فقط بعد از وجود تصویر و نام فعال شود. فهرست فروشنده همهٔ وضعیت‌ها را نشان دهد؛ `submitted` را قفل و `rejected` را همراه دلیل و دکمهٔ اصلاح نمایش دهد.
7. ادمین در `GET /api/Admin/MissingProduct?status=submitted` درخواست را ببیند، با `GET /{id}` مشخصات و مشابه‌ها را باز کند و با `approve` یا `reject` تصمیم بگیرد.
8. فروشنده پس از refresh فهرست، `approved` و `productId` را ببیند. اگر محصول برای فروشگاه آیتم نگرفته است، `GET /api/Seller/ProductItem/id?id={productId}` و سپس `POST /api/Seller/ProductItem` را اجرا کند.

نمونهٔ ثبت قیمت/موجودی برای **محصول بدون تنوع** که پس از تأیید فقط در کاتالوگ ساخته شده است:

```http
POST /api/Seller/ProductItem
Content-Type: application/json
Authorization: Bearer {accessToken}
```

```json
{
  "productId": 456,
  "productItems": [
    {"id": 0, "basePrice": 850000, "quantity": 4, "active": true, "warranty": "", "varietyItemId": null, "varietyItem2Id": null}
  ]
}
```

در این مسیر `storeId` را فروشنده تعیین نمی‌کند؛ کنترلر آن را از توکن جایگزین می‌کند. پاسخ `POST ProductItem` فقط نتیجهٔ موفق/ناموفق را تضمین می‌کند، نه شناسهٔ آیتم تازه. برای آیتم موجود `id` واقعی همان فروشگاه را از `GET /api/Seller/ProductItem/id?id=...` بردارید. برای محصولات دارای تنوع، سطرهای `productItems` باید از ترکیب‌های معتبر فرم سرور ساخته شوند؛ نمونهٔ بدون تنوع را برای محصول متنوع کپی نکنید.

برای ورودی اسکن هوشمند، خروجی `POST /api/Seller/AiProductMatch/analyze` یا job متناظر فقط **پیشنهاد/نتیجهٔ تطبیق** است و خودش MissingProduct نمی‌سازد. ردیف دارای `catalogMatchFailed:true` را ثبت خودکار نکنید؛ اگر `productId` خالی ولی `matches[]` کاندید دارد، انتخاب دستی/تأیید فروشنده لازم است. پس از اطمینان از ناموجودی، `detectedName`, `brand`, `packageSize` را به batch نگاشت کنید. مقدار `confidence` به‌تنهایی دلیل قطعی نبود محصول در کاتالوگ نیست.

## ۷. عیب‌یابی دقیق

| نشانه | علت/بررسی |
|---|---|
| درخواست در پنل ادمین دیده نمی‌شود | آیا `submit` موفق بوده و `status=submitted` است؟ ثبت POST اولیه فقط draft می‌سازد. فیلتر `storeId`, `q`, `pageNumber` و توکن AdminOnly را بررسی کنید. |
| درخواست در فهرست فروشنده دیده نمی‌شود | توکن باید متعلق به همان فروشگاه باشد؛ فهرست فقط جدیدترین حداکثر ۲۰۰ رکورد را می‌دهد. پاسخ `created` شناسه نیست. |
| `created:0` و `alreadyKnown>0` | نام نرمال‌شده برای این فروشگاه قبلاً ثبت شده، حتی اگر وضعیت قبلی rejected/approved باشد. رکورد قبلی را از فهرست پیدا کنید. |
| `created:0`, `alreadyKnown:0` | ممکن است همهٔ نام‌های batch خالی/نامعتبر بوده باشند؛ اعتبارسنجی نام را پیش از ارسال انجام دهید. |
| submit می‌گوید تصویر لازم است | فایل باید از `POST .../{id}/picture` با `isSuccess:true` ثبت شده باشد؛ داشتن تصویر فقط در UI کافی نیست. |
| `pictureUrl` هست ولی `pictures[]` فهرست فروشنده خالی است | محدودیت بارگیری رابطه در `ListAsync` فعلی؛ پاسخ آپلود/ویرایش و وضعیت کاور را مبنا بگیرید. برای گالری کامل پس از refresh، اصلاح بک‌اند لازم است. |
| بعد از approved کالا در فروشگاه دیده نمی‌شود | `productId` با `productItemId` فرق دارد. قیمت مؤثر باید `>0` باشد و ساخت آیتم هم باید موفق شده باشد. `storeItemCreated` پاسخ ادمین و فرم ProductItem را بررسی کنید. همچنین نمایش عمومی به شرایط موجودی/فعال‌بودن فروشگاه وابسته است. |
| ادمین approve موفق است ولی گالری ناقص است | کپی گالری مرحلهٔ جانبی و شکست آن تأیید را باطل نمی‌کند؛ لاگ مرحلهٔ کپی بررسی شود. |
| منوی «درخواست افزودن به کاتالوگ» دیده نمی‌شود | دسترسی AdminOnly و پیکربندی منو را جداگانه بررسی کنید؛ اسکریپت بررسی/اصلاح `scripts/database/EnsureMissingProductMenu.sql` در مخزن هست. اجرای تغییر دیتابیس فقط پس از بازبینی محیط. |

خطاهای قراردادی رایج: `code:1` برای اعتبارسنجی/وضعیت نامجاز/نام تکراری؛ `code:3` برای درخواست یا تصویر پیدا‌نشده؛ `code:5` برای شکست آپلود فایل. خطای مجوز یا حد اندازهٔ بدنه می‌تواند HTTP غیر ۲۰۰ باشد. پیام فارسی `messages` را نمایش دهید و وضعیت را پس از موفقیت refresh کنید؛ push/اعلان اختصاصی برای تغییر وضعیت MissingProduct در این سرویس پیاده نشده است.

## ۸. منابع کد برای بررسی قرارداد

- `Api/Areas/Seller/Controllers/MissingProductController.cs` و `Api/Areas/Admin/Controllers/MissingProductController.cs`: مسیرها و دسترسی.
- `Application/Services/ProductSrvs/MissingProductSrv/Dto/MissingProductDtos.cs`: مدل‌های ورودی/خروجی.
- `Application/Services/ProductSrvs/MissingProductSrv/MissingProductService.cs`: اعتبارسنجی، وضعیت‌ها، لیست‌ها، approve/reject.
- `Entities/Entities/MissingProduct.cs` و `Persistence/Context/DataBaseContext.cs`: موجودیت، ایندکس یکتا و روابط.
- `Api/Controllers/ProductController.cs` و `Api/Areas/Seller/Controllers/ProductItemController.cs`: جست‌وجوی کاتالوگ و ثبت موجودی کالای موجود.
- `Application/Services/ProductSrvs/AiProductMatchSrv/Dto/AiProductMatchResultItemDto.cs`: تمایز `catalogMatchFailed` از ناموجودی واقعی.
