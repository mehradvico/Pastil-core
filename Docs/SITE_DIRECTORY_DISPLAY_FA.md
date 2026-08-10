# مستند نمایش خدمات و مجموعه‌ها در سایت Pastil

این مستند فقط برای نمایش عمومی موارد زیر در سایت `pastil.pet` است:

- پانسیون‌ها (`Pansion`)
- فروشگاه‌ها (`Store`)
- نمایندگان (`Companion`)
- خدمات (`Assistance`)

در این مستند هیچ عملیات افزودن، ویرایش، حذف، رزرو یا پرداختی وجود ندارد. تمام endpointهای معرفی‌شده `GET` و بدون نیاز به Token هستند.

## تنظیمات پایه

```text
API_BASE_URL=https://api.pastil.pet
FILE_BASE_URL=https://file.pastil.pet
```

پاسخ لیست‌ها:

```json
{
  "totalCount": 12,
  "pageIndex": 1,
  "pageSize": 12,
  "list": []
}
```

پاسخ جزئیات:

```json
{
  "isSuccess": true,
  "messages": [],
  "code": 0,
  "data": {}
}
```

در جزئیات، فقط وقتی `isSuccess=true` و `data` دارای مقدار است صفحه نمایش داده شود. پاسخ `isSuccess=false` برای فرانت معادل `404` است، حتی اگر HTTP Status برابر `200` باشد.

## پارامترهای مشترک لیست‌ها

| پارامتر | نوع | توضیح |
|---|---:|---|
| `pageIndex` | number | شماره صفحه؛ پیش‌فرض `1` |
| `pageSize` | number | تعداد در صفحه؛ پیش‌فرض `20` |
| `sortBy` | number | نوع مرتب‌سازی |
| `q` | string | جستجوی متنی؛ پشتیبانی آن برای هر بخش جدا توضیح داده شده است |

مقادیر عمومی `sortBy` که در این چهار بخش استفاده می‌شوند:

| مقدار | مفهوم |
|---:|---|
| `1` | جدیدترین |
| `2` | قدیمی‌ترین |
| `4` | بیشترین امتیاز در Pansion |
| `5` | کمترین امتیاز در Pansion |
| `6` | اولویت حساب طلایی/نقره‌ای در Companion |
| `8` | گران‌ترین Pansion |
| `9` | ارزان‌ترین Pansion |
| `10` | بیشترین امتیاز در Store و Companion |
| `11` | کمترین امتیاز در Store و Companion |

پارامترهای `available`، `showToSite`، `approve` و `approved` را فرانت ارسال نکند. کنترلرهای Site این مقادیر را در بک‌اند به‌صورت اجباری تنظیم می‌کنند و فقط آیتم‌های مجاز برای نمایش را برمی‌گردانند.

## ساخت URL تصویر

ساختار `PictureVDto`:

```json
{
  "id": 108,
  "url": "/Media/2026/8/9/abc.webp",
  "baseUrl": "/Media/2026/8/9",
  "guidName": "abc",
  "extension": ".webp",
  "orginalName": "photo.jpg"
}
```

تصویر اصلی:

```ts
const imageUrl = `${FILE_BASE_URL}${picture.url}`
```

نسخه‌های بهینه‌شده:

```ts
const smallImage =
  `${FILE_BASE_URL}${picture.baseUrl}/${picture.guidName}-sm${picture.extension}`

const mediumImage =
  `${FILE_BASE_URL}${picture.baseUrl}/${picture.guidName}-md${picture.extension}`

const largeImage =
  `${FILE_BASE_URL}${picture.baseUrl}/${picture.guidName}-lg${picture.extension}`
```

پیشنهاد مصرف:

- کارت لیست: `sm` یا `md`
- صفحه جزئیات: تصویر اصلی یا `lg`
- اگر `picture` برابر `null` بود، Placeholder داخلی سایت نمایش داده شود.
- قبل از اتصال رشته‌ها از ایجاد `//` اضافی جلوگیری شود.

---

# Pansion

## دریافت لیست پانسیون‌ها

```http
GET /api/Site/Pansion
```

نمونه ساده:

```http
GET /api/Site/Pansion?pageIndex=1&pageSize=12&sortBy=1
```

فیلترهای قابل استفاده:

| پارامتر | نوع | توضیح |
|---|---:|---|
| `stateId` | number | استان |
| `cityId` | number | شهر |
| `neighborhoodIds` | number[] | محله‌های نماینده صاحب پانسیون |
| `companionId` | number | پانسیون‌های یک نماینده |
| `petId` | number | پانسیون‌هایی که نوع حیوان مشخص را می‌پذیرند |
| `isSchool` | boolean | مدرسه/آموزش یا پانسیون |
| `suggested` | boolean | موارد پیشنهادی |
| `sortBy` | number | جدید، قدیم، امتیاز یا قیمت |

پارامتر `q` در قرارداد مشترک وجود دارد، اما Search فعلی Pansion آن را اعمال نمی‌کند؛ برای جستجوی نام پانسیون به آن متکی نباشید.

برای ارسال چند محله:

```http
GET /api/Site/Pansion?cityId=10&neighborhoodIds=21&neighborhoodIds=22
```

فیلتر `isSchool` در بک‌اند آیتم‌هایی را که مقدار `isSchool=null` دارند نیز در نتیجه نگه می‌دارد.

## دریافت جزئیات پانسیون

```http
GET /api/Site/Pansion/{id}
```

نمونه:

```http
GET /api/Site/Pansion/15
```

فیلدهای مناسب نمایش:

| فیلد | کاربرد در سایت |
|---|---|
| `id` | شناسه پانسیون |
| `name` | نام پانسیون |
| `discription` | توضیحات؛ املای فیلد در API دقیقاً به همین شکل است |
| `addressValue` | آدرس متنی |
| `stateId` / `cityId` | شناسه استان و شهر |
| `city` | اطلاعات شهر و معمولاً `city.state` |
| `picture` | تصویر اصلی |
| `pansionPictures` | گالری تصاویر |
| `pansionPets` | حیوانات قابل پذیرش |
| `isSchool` | مدرسه/مرکز آموزشی بودن |
| `pansionPrice` | قیمت پانسیون |
| `schoolPrice` | قیمت مدرسه/آموزش |
| `openHour` | ساعت شروع فعالیت |
| `closeHour` | ساعت پایان فعالیت |
| `regulations` | قوانین پانسیون |
| `suggested` | نشان پیشنهادی |
| `rateAvg` | میانگین امتیاز |
| `rateCount` | تعداد امتیازها |
| `commentCount` | تعداد نظرهای تأییدشده ثبت‌شده روی مدل |
| `companion` | اطلاعات حداقلی نماینده مرتبط |

واحد قیمت را از تنظیمات و قرارداد عمومی مالی سایت دریافت کنید و در فرانت حدس نزنید.

برای نمایش گالری:

```ts
const gallery = data.pansionPictures
  ?.filter(item => item.picture?.url)
  .map(item => `${FILE_BASE_URL}${item.picture.url}`) ?? []
```

### مواردی که از پاسخ Pansion نباید نمایش داده شوند

- `dailyCommissionPercent`
- `hourlyCommissionPercent`
- وضعیت‌های مدیریتی مانند `showToSite` و `approve`
- `pansionComments` به‌صورت مستقیم

رابطه `pansionComments` در جزئیات Load می‌شود، اما endpoint جزئیات تضمین نمی‌کند که فقط کامنت‌های تأییدشده داخل این Array باشند. بنابراین سایت نباید این Array را مستقیماً نمایش دهد.

---

# Store

## دریافت لیست فروشگاه‌ها

```http
GET /api/Site/Store
```

نمونه:

```http
GET /api/Site/Store?pageIndex=1&pageSize=12&sortBy=10
```

فیلترهای قابل استفاده:

| پارامتر | نوع | توضیح |
|---|---:|---|
| `q` | string | جستجو در نام فروشگاه |
| `typeId` | number | نوع فروشگاه |
| `stateId` | number | استان فروشگاه از طریق شهر |
| `cityId` | number | شهر فروشگاه |
| `sortBy=1` | number | جدیدترین |
| `sortBy=2` | number | قدیمی‌ترین |
| `sortBy=10` | number | بیشترین امتیاز |
| `sortBy=11` | number | کمترین امتیاز |

نمونه جستجو:

```http
GET /api/Site/Store?q=پت‌شاپ&cityId=10&pageIndex=1&pageSize=12
```

## دریافت جزئیات فروشگاه

```http
GET /api/Site/Store/{id}
```

فیلدهای مناسب نمایش:

| فیلد | کاربرد در سایت |
|---|---|
| `id` | شناسه فروشگاه |
| `name` | نام |
| `summary` | خلاصه معرفی |
| `description` | توضیحات کامل |
| `phone` | تلفن |
| `mobile` | موبایل عمومی فروشگاه |
| `email` | ایمیل عمومی فروشگاه |
| `address` | آدرس |
| `city` | شهر و استان از `city.state` |
| `location` | مختصات نقشه |
| `picture` | تصویر اصلی یا کاور |
| `icon` | آیکن/لوگو |
| `type` | نوع فروشگاه؛ نام نمایشی از Code |
| `maxDiscountPercent` | بیشترین درصد تخفیف |
| `rateAvg` | میانگین امتیاز |
| `rateCount` | تعداد امتیاز |
| فیلدهای `seo*` | Meta، H1، Alt و Canonical صفحه |

مختصات:

```ts
const longitude = data.location?.x
const latitude = data.location?.y
```

قبل از نمایش نقشه، وجود `location` و معتبر بودن مختصات بررسی شود.

### مواردی که از پاسخ Store نباید نمایش داده شوند

- `commissionPercent`
- `users`
- شناسه‌ها و وضعیت‌های مدیریتی مانند `showToSite`

`users` می‌تواند حاوی اطلاعات حساب کاربران مرتبط با فروشگاه باشد و برای صفحه عمومی سایت طراحی نشده است؛ آن را Render یا در State عمومی ذخیره نکنید.

محصولات فروشگاه در `StoreVDto` قرار ندارند. اگر صفحه فروشگاه باید محصولات را نشان دهد، محصولات باید از API مستقل Product با فیلتر فروشگاه دریافت شوند.

---

# Companion

## دریافت لیست نمایندگان

```http
GET /api/Site/Companion
```

نمونه:

```http
GET /api/Site/Companion?pageIndex=1&pageSize=12&sortBy=6
```

فیلترهای قابل استفاده:

| پارامتر | نوع | توضیح |
|---|---:|---|
| `q` | string | جستجو در نام یا تطبیق دقیق شماره تلفن |
| `stateId` | number | استان |
| `cityId` | number | شهر |
| `neighborhoodIds` | number[] | محله‌ها |
| `typeId` | number | نوع نماینده |
| `petId` | number | نوع حیوان قابل پشتیبانی |
| `assistanceId` | number | نمایندگانی که خدمت مشخص را ارائه می‌کنند |
| `assistanceType` | number | نوع خدمت بر اساس Enum بک‌اند |
| `isPersonal` | boolean | شخصی یا مجموعه‌ای بودن |
| `goldAccount` | boolean | حساب طلایی فعال |
| `silverAccount` | boolean | حساب نقره‌ای فعال |
| `hasInsurance` | boolean | داشتن پکیج بیمه فعال |
| `sortBy=6` | number | اولویت طلایی/نقره‌ای |
| `sortBy=10` | number | بیشترین امتیاز |
| `sortBy=11` | number | کمترین امتیاز |

نمونه دریافت نمایندگان یک خدمت در یک شهر:

```http
GET /api/Site/Companion?assistanceId=32&cityId=10&pageIndex=1&pageSize=12&sortBy=10
```

## دریافت جزئیات نماینده

```http
GET /api/Site/Companion/{id}
```

فیلدهای مناسب نمایش:

| فیلد | کاربرد در سایت |
|---|---|
| `id` | شناسه نماینده |
| `name` | نام نماینده/مجموعه |
| `summary` | معرفی کوتاه |
| `description` | معرفی کامل |
| `phone` | تلفن عمومی نماینده |
| `addressValue` | آدرس |
| `city` | شهر و استان از `city.state` |
| `neighborhood` | محله |
| `location` | مختصات نقشه |
| `picture` | تصویر اصلی |
| `backgroundPicture` | تصویر پس‌زمینه/کاور |
| `icon` | لوگو یا آیکن |
| `isPersonal` | شخصی یا مجموعه‌ای |
| `isGold` | نشان طلایی |
| `isSilver` | نشان نقره‌ای |
| `rateAvg` | میانگین امتیاز |
| `rateCount` | تعداد امتیازها |
| `commentCount` | تعداد کامنت‌های تأییدشده ثبت‌شده روی مدل |
| `hasPansion` | داشتن پانسیون |
| `companionTypes` | نوع‌های نماینده |
| `companionPets` | حیوانات قابل پشتیبانی |
| `companionZones` | محدوده‌های خدمت‌رسانی |
| فیلدهای `seo*` | اطلاعات SEO صفحه |

در محدوده‌های خدمت‌رسانی:

- اگر `coversWholeCity=true` باشد کل شهر پوشش داده می‌شود.
- در غیر این صورت `neighborhood` محدوده دقیق‌تر را مشخص می‌کند.

برای گرفتن خدمات یک نماینده از endpoint امن لیست Companion استفاده نمی‌شود، زیرا جزئیات Companion آرایه خدمات را برنمی‌گرداند. برای نمایش «نمایندگان ارائه‌دهنده یک خدمت»، روش پیشنهادی فیلتر `assistanceId` روی `/api/Site/Companion` است.

### دریافت پانسیون‌های قابل نمایش یک نماینده

از آرایه `pansions` داخل جزئیات Companion مستقیماً استفاده نکنید؛ این رابطه در سرویس جزئیات بدون فیلتر `ShowToSite` Load می‌شود. مسیر امن:

```http
GET /api/Site/Pansion?companionId={companionId}&pageIndex=1&pageSize=20
```

این مسیر فقط پانسیون‌های Active، تأییدشده و مجاز برای سایت را برمی‌گرداند.

### مواردی که از پاسخ Companion نباید نمایش داده شوند

- `activationValue`
- تاریخ‌های داخلی حساب طلایی/نقره‌ای، مگر طراحی سایت صراحتاً به آن نیاز داشته باشد
- `searchKey`
- `owner` به‌صورت مستقیم
- `pansions` به‌صورت مستقیم
- وضعیت‌های مدیریتی مانند `approved` و `showToSite`

`owner` می‌تواند موبایل، ایمیل و اطلاعات حساب مالک را داشته باشد. برای تماس فقط از `phone` عمومی خود Companion استفاده شود.

---

# Assistance

## دریافت لیست خدمات

```http
GET /api/Site/Assistance
```

نمونه:

```http
GET /api/Site/Assistance?pageIndex=1&pageSize=50&sortBy=1
```

فیلترهای قابل استفاده:

| پارامتر | نوع | توضیح |
|---|---:|---|
| `q` | string | جستجو در نام، خلاصه و توضیحات |
| `assistanceGroupId` | number | گروه خدمت |
| `isPersonal` | boolean | نوع شخصی/غیرشخصی خدمت |
| `sortBy=1` | number | جدیدترین |
| `sortBy=2` | number | قدیمی‌ترین |

نمونه:

```http
GET /api/Site/Assistance?assistanceGroupId=3&q=آرایش&pageIndex=1&pageSize=20
```

بک‌اند علاوه بر Active و `ShowToSite` بودن خدمت، فعال و حذف‌نشده بودن گروه خدمت را نیز کنترل می‌کند.

## دریافت جزئیات خدمت

```http
GET /api/Site/Assistance/{id}
```

فیلدهای مناسب نمایش:

| فیلد | کاربرد در سایت |
|---|---|
| `id` | شناسه خدمت |
| `name` | نام خدمت |
| `summary` | معرفی کوتاه |
| `description` | توضیحات کامل |
| `isPersonal` | نوع خدمت |
| `picture` | تصویر خدمت |
| `assistanceGroupId` | شناسه گروه |
| `assistanceGroup.name` | نام نمایشی گروه |
| `assistanceGroup.priority` | اولویت نمایش گروه |

برای نمایش نمایندگان ارائه‌دهنده این خدمت:

```http
GET /api/Site/Companion?assistanceId={assistanceId}&pageIndex=1&pageSize=12&sortBy=10
```

این روش باعث می‌شود فقط Companionهای Active، تأییدشده و دارای `ShowToSite=true` نمایش داده شوند.

قیمت و پکیج نماینده در DTO خود Assistance وجود ندارد؛ Assistance فقط کاتالوگ نوع خدمت است. نمایش قیمت یا پکیج به APIهای رابطه CompanionAssistance وابسته است و داخل این قرارداد معرفی عمومی چهار بخش قرار ندارد.

---

# Routeهای پیشنهادی سایت

```text
/pansions
/pansions/:id

/stores
/stores/:id

/companions
/companions/:id

/services
/services/:id
```

در وضعیت فعلی API جزئیات این چهار Entity با `id` دریافت می‌شود و endpoint اختصاصی دریافت با Slug وجود ندارد. Route می‌تواند در فرانت عنوان SEO-friendly داشته باشد، ولی `id` باید برای درخواست API قابل استخراج باشد.

# جریان پیشنهادی نمایش

## صفحه لیست

1. فیلترهای URL خوانده شوند.
2. endpoint Site مربوطه با `pageIndex` و `pageSize` فراخوانی شود.
3. Skeleton نمایش داده شود.
4. در نبود نتیجه Empty State مناسب نمایش داده شود.
5. Pagination بر اساس `totalCount` ساخته شود.
6. تصویر کارت از نسخه `sm` یا `md` دریافت شود.

## صفحه جزئیات

1. جزئیات با `GET /api/Site/{Entity}/{id}` دریافت شود.
2. اگر `isSuccess=false` بود صفحه 404 نمایش داده شود.
3. تصویر، توضیحات، موقعیت، امتیاز و اطلاعات تماس عمومی نمایش داده شوند.
4. فیلدهای داخلی و روابط ناامن مشخص‌شده Render نشوند.
5. برای روابط قابل نمایش از endpointهای Site فیلترشده استفاده شود.

# نکات امنیت و حریم خصوصی

- فقط چهار endpoint دارای پیشوند `/api/Site/` منبع اصلی صفحه عمومی باشند.
- اطلاعات `owner` و `users` در صفحه عمومی نمایش داده نشوند.
- Commissionها و علت‌های Activation اطلاعات داخلی هستند.
- آرایه `pansions` در Companion و `pansionComments` در Pansion مستقیماً نمایش داده نشوند.
- اطلاعات تماس فقط از فیلد عمومی خود Store یا Companion نمایش داده شود.
- داده‌های `null` در UI مدیریت شوند و از شکستن صفحه جلوگیری شود.

# مدیریت خطا

| وضعیت | رفتار فرانت |
|---|---|
| HTTP `200` و `isSuccess=false` | صفحه جزئیات 404 شود |
| HTTP `404` | صفحه 404 |
| HTTP `500` | پیام خطای موقت و امکان Retry |
| خطای شبکه | Empty Error State و Retry محدود |
| تصویر ناموجود | Placeholder |

این APIها صرفاً برای نمایش هستند و فرانت سایت نباید از endpointهای `/api/Admin`، `/api/Companion` یا عملیات `POST/PUT/DELETE` برای این صفحات استفاده کند.
