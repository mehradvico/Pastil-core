# مستند فرانت سایت معرفی پاستیل — ویژه آرمان

## محدوده مسئولیت

این مستند فقط برای طراحی و توسعه سایت عمومی زیر است:

```text
https://pastil.pet
```

تقسیم مسئولیت پروژه:

| دامنه | کاربرد | مسئول فرانت |
|---|---|---|
| `pastil.pet` | سایت عمومی، معرفی خدمات، محتوا، نوشته‌ها و مجموعه‌ها | آرمان |
| `app.pastil.pet` | اپلیکیشن اصلی کاربران پاستیل | امیرمحسن |
| `panel.pastil.pet` | پنل مدیریت | تیم پنل |

آرمان برای صفحات عمومی `pastil.pet` باید از APIهای دارای پیشوند `/api/Site/` استفاده کند. APIهای App، Admin، Seller و Companion منبع اصلی سایت عمومی نیستند.

## آدرس سرویس‌ها

```env
NUXT_PUBLIC_API_BASE_URL=https://api.pastil.pet
NUXT_PUBLIC_FILE_BASE_URL=https://file.pastil.pet
```

Swagger بک:

```text
https://api.pastil.pet/swagger/index.html
```

تمام Endpointهای معرفی‌شده در این مستند، به‌جز مواردی که صراحتاً خلاف آن گفته شده، عمومی هستند و Token نمی‌خواهند.

## قراردادهای مشترک پاسخ

### پاسخ لیست

Endpointهای لیستی مستقیماً این ساختار را برمی‌گردانند:

```json
{
  "pageIndex": 1,
  "pageSize": 12,
  "q": null,
  "sortBy": 1,
  "available": true,
  "totalCount": 42,
  "list": []
}
```

صفحه‌بندی باید با `totalCount`، `pageIndex` و `pageSize` ساخته شود.

### پاسخ جزئیات

```json
{
  "isSuccess": true,
  "messages": [],
  "code": 0,
  "data": {}
}
```

نکته مهم: بعضی خطاهای منطقی با HTTP Status برابر `200` و مقدار `isSuccess=false` برمی‌گردند. در صفحات جزئیات، `isSuccess=false` یا `data=null` باید به صفحه 404 منتهی شود.

## پارامترهای مشترک لیست‌ها

| پارامتر | نوع | پیش‌فرض | توضیح |
|---|---:|---:|---|
| `pageIndex` | number | `1` | شماره صفحه |
| `pageSize` | number | `20` | تعداد رکورد هر صفحه |
| `q` | string | خالی | متن جستجو |
| `sortBy` | number | `1` | مرتب‌سازی |

مقادیر عمومی `sortBy`:

| مقدار | مفهوم |
|---:|---|
| `0` | پیش‌فرض |
| `1` | جدیدترین |
| `2` | قدیمی‌ترین |
| `3` | نام |
| `4` | بیشترین بازدید یا امتیاز، بسته به Entity |
| `5` | کمترین بازدید یا امتیاز، بسته به Entity |
| `6` | اولویت بیشتر |
| `7` | اولویت کمتر |
| `8` | گران‌ترین |
| `9` | ارزان‌ترین |
| `10` | بیشترین امتیاز در Store و Companion |
| `11` | کمترین امتیاز در Store و Companion |

پارامترهای `available`، `active`، `approved` و `showToSite` را برای APIهای Site نفرستید. بک مقادیر امن آن‌ها را خودش اجبار می‌کند.

## ساخت URL تصویر و فایل

ساختار تصویر:

```ts
type Picture = {
  id: number
  url: string | null
  baseUrl: string | null
  guidName: string | null
  extension: string | null
  orginalName: string | null
}
```

تابع پیشنهادی:

```ts
const FILE_BASE_URL = 'https://file.pastil.pet'

export function resolveMediaUrl(path?: string | null) {
  if (!path) return null
  if (/^https?:\/\//i.test(path)) return path
  return `${FILE_BASE_URL}/${path.replace(/^\/+/, '')}`
}
```

تصاویر معمولاً نسخه‌های `sm`، `md` و `lg` نیز دارند:

```ts
export function resolvePictureVariant(
  picture: Picture | null | undefined,
  size: 'sm' | 'md' | 'lg'
) {
  if (!picture?.baseUrl || !picture.guidName || !picture.extension) {
    return resolveMediaUrl(picture?.url)
  }

  const path = `${picture.baseUrl}/${picture.guidName}-${size}${picture.extension}`
  return resolveMediaUrl(path)
}
```

پیشنهاد مصرف:

- کارت‌های لیست: `sm` یا `md`
- Hero و صفحه جزئیات: `lg` یا `url`
- اگر تصویر وجود نداشت: Placeholder داخلی سایت
- برای `alt` از `seoPictureAlt` و در نبود آن از `name` استفاده شود.

## دسته‌بندی‌ها و Labelها

نام نمایشی دسته ممکن است در پنل تغییر کند. برای منطق فرانت از `label` استفاده شود و `name` فقط برای نمایش باشد.

دریافت دسته و زیرمجموعه‌ها با Label:

```http
GET /api/Category/label/{label}
```

ریشه دسته‌های بنر سایت:

```http
GET /api/Category/label/pastil-site-banner
```

ریشه دسته‌های نوشته:

```http
GET /api/Category/label/post
```

زیرمجموعه‌ها از آرایه `data.children` خوانده شوند. Label جایگاه‌ها و دسته‌های جدید در فرانت Hard-code نشود، مگر برای Routeهای ثابت مورد توافق طراحی.

---

# 1. بنرهای سایت

## لیست بنرها

```http
GET /api/Site/Banner
```

فیلترها:

| پارامتر | توضیح |
|---|---|
| `categoryId` | شناسه جایگاه یا دسته بنر |
| `categoryLabel` | Label جایگاه بنر؛ روش پیشنهادی |
| `q` | جستجو در نام بنر |
| `sortBy=6` | اولویت بیشتر |
| `sortBy=7` | اولویت کمتر |

نمونه:

```http
GET /api/Site/Banner?categoryLabel=home-main-slider&pageIndex=1&pageSize=20&sortBy=6
```

بک فقط بنرهای زیر را برمی‌گرداند:

```text
Active = true
ShowToSite = true
```

برای سایت از `/api/Banner` استفاده نشود؛ آن مسیر متعلق به مصرف عمومی App است.

فیلدهای مهم:

| فیلد | مصرف |
|---|---|
| `id` | شناسه |
| `name` | عنوان نمایشی |
| `label` و `slug` | شناسه متنی |
| `summary` و `description` | متن بنر |
| `url` | لینک مقصد بنر |
| `priority` | ترتیب نمایش |
| `picture` | تصویر اصلی |
| `picture2` | تصویر دوم یا نسخه طراحی دوم |
| `category` | جایگاه بنر |

## جزئیات بنر

```http
GET /api/Site/Banner/{id}
```

## رفتار کلیک

- اگر `url` خالی بود، بنر کلیک‌پذیر نباشد.
- URL داخلی با Router سایت باز شود.
- URL خارجی با اعتبارسنجی `http/https` باز شود.
- برای لینک خارجی در Tab جدید، `rel="noopener noreferrer"` استفاده شود.

---

# 2. نوشته‌ها و وبلاگ

## لیست نوشته‌های منتشرشده

```http
GET /api/Site/Post
```

نمونه:

```http
GET /api/Site/Post?pageIndex=1&pageSize=12&sortBy=1
```

فیلترها:

| پارامتر | نوع | توضیح |
|---|---:|---|
| `q` | string | جستجو در نام نوشته |
| `categoryIds` | number[] | شناسه دسته‌ها |
| `categoryLabels` | string[] | Label دسته‌ها؛ روش پیشنهادی |
| `isAndCategories` | boolean | `true`: نوشته باید همه دسته‌ها را داشته باشد |
| `hashtags` | string | هشتگ‌ها با جداکننده `-` |
| `notId` | number | حذف یک نوشته از نتیجه، مناسب مطالب مرتبط |
| `sortBy=4` | number | پربازدیدترین |
| `sortBy=5` | number | کم‌بازدیدترین |
| `sortBy=6` | number | تاریخ انتشار نزولی |
| `sortBy=7` | number | تاریخ انتشار صعودی |

آرایه Query باید با تکرار پارامتر ارسال شود:

```http
GET /api/Site/Post?categoryLabels=health&categoryLabels=dogs&isAndCategories=false
```

بک فقط نوشته‌ای را برمی‌گرداند که فعال، حذف‌نشده، تأییدشده و زمان انتشار آن رسیده باشد.

## جزئیات نوشته

```http
GET /api/Site/Post/{id}
```

هر دریافت موفق جزئیات، `visitCount` را یک واحد افزایش می‌دهد؛ بنابراین در SSR درخواست جزئیات دوبار اجرا نشود.

فیلدهای مهم:

| فیلد | مصرف |
|---|---|
| `id` | شناسه نوشته |
| `name` و `subject` | عنوان |
| `summary` و `subNews` | خلاصه و زیرعنوان |
| `description` | محتوای کامل |
| `publishDate` | تاریخ انتشار |
| `visitCount` | بازدید |
| `commentCount` | تعداد نظرهای تأییدشده ثبت‌شده روی پست |
| `picture` | تصویر اصلی |
| `postPictures` | تصاویر ضمیمه |
| `postFiles` | فایل‌های ضمیمه |
| `hashtags` | هشتگ‌ها |
| `category` | دسته اصلی |
| فیلدهای `seo*` | SEO صفحه |

Endpoint جزئیات فعلاً بر اساس `id` است. Route فرانت می‌تواند SEO-friendly باشد، ولی باید ID را نیز نگه دارد؛ نمونه:

```text
/blog/125/pet-vaccination-guide
```

## محتوای HTML

اگر `description` به‌صورت HTML Render می‌شود، قبل از `v-html` یا روش مشابه باید با کتابخانه معتبر Sanitize شود. Script، iframe ناشناس، event handler و URLهای خطرناک مجاز نیستند.

## نظرات نوشته

Endpoint موجود برای خواندن نظرات:

```http
GET /api/EndUser/PostComment?postId={postId}&pageIndex=1&pageSize=20&sortBy=1
```

در نسخه فعلی سرویس، فیلتر وضعیت تأیید نظر به‌صورت قابل اتکا اعمال نمی‌شود. تا زمان اصلاح بک، آرمان نباید لیست متن نظرات را در Production نمایش دهد. نمایش `commentCount` خود نوشته مانعی ندارد.

خود Post در بک فعلی Like مستقل ندارد. Like/Dislike موجود مربوط به Comment و نیازمند ورود کاربر در App است؛ در سایت معرفی فعلاً استفاده نشود.

---

# 3. گالری‌ها

## لیست گالری‌های فعال

```http
GET /api/Site/Gallery
```

فیلترها:

```text
categoryId
categoryLabel
q
pageIndex
pageSize
sortBy
```

## دریافت گالری با Label

```http
GET /api/Site/Gallery/label/{label}
```

نمونه:

```http
GET /api/Site/Gallery/label/home-gallery
```

## دریافت آیتم‌های گالری

ابتدا ID گالری دریافت و سپس آیتم‌ها فراخوانی شوند:

```http
GET /api/GalleryItem?galleryId={galleryId}&pageIndex=1&pageSize=100&sortBy=6
```

فیلدهای مهم هر آیتم:

```text
id
name
summary
description
link
priority
pictureId
picture
galleryId
```

اگر `link` مقدار نداشت، آیتم فقط تصویر باشد.

---

# 4. نمایندگان

## لیست نمایندگان قابل نمایش

```http
GET /api/Site/Companion
```

بک به‌صورت اجباری فقط نماینده‌های زیر را برمی‌گرداند:

```text
Active = true
Approved = true
ShowToSite = true
```

فیلترها:

| پارامتر | توضیح |
|---|---|
| `q` | جستجو در نام یا شماره تلفن دقیق |
| `stateId` | استان |
| `cityId` | شهر |
| `neighborhoodIds` | محله‌ها؛ پارامتر تکرارشونده |
| `typeId` | نوع نماینده |
| `petId` | نوع حیوان قابل پشتیبانی |
| `assistanceId` | نمایندگان ارائه‌دهنده خدمت مشخص |
| `assistanceType` | نوع رابطه خدمت |
| `isPersonal` | شخصی یا مجموعه‌ای |
| `goldAccount` | حساب طلایی |
| `silverAccount` | حساب نقره‌ای |
| `hasInsurance` | دارای بیمه |

نمونه:

```http
GET /api/Site/Companion?assistanceId=32&cityId=10&pageIndex=1&pageSize=12&sortBy=10
```

## جزئیات نماینده

```http
GET /api/Site/Companion/{id}
```

فیلدهای مناسب نمایش:

```text
id, name, summary, description
phone, addressValue
city, neighborhood, location
picture, backgroundPicture, icon
isPersonal, isGold, isSilver
rateAvg, rateCount, commentCount
hasPansion
companionTypes, companionPets, companionZones
seo*
```

برای پانسیون‌های یک نماینده از آرایه `pansions` داخل جزئیات استفاده نشود، چون آن رابطه ممکن است آیتم غیرمجاز برای سایت داشته باشد. مسیر امن:

```http
GET /api/Site/Pansion?companionId={companionId}&pageIndex=1&pageSize=20
```

فیلدهای داخلی زیر Render یا در State عمومی ذخیره نشوند:

```text
owner
referralCode
activationValue
searchKey
goldAccountDate
silverAccountDate
silverAccountCreateDate
approved
showToSite
```

---

# 5. خدمات

## لیست خدمات

```http
GET /api/Site/Assistance
```

فیلترها:

| پارامتر | توضیح |
|---|---|
| `q` | جستجو در نام، خلاصه و توضیحات |
| `assistanceGroupId` | گروه خدمت |
| `isPersonal` | نوع شخصی یا غیرشخصی |

نمونه:

```http
GET /api/Site/Assistance?assistanceGroupId=3&pageIndex=1&pageSize=30&sortBy=1
```

## جزئیات خدمت

```http
GET /api/Site/Assistance/{id}
```

فیلدهای مهم:

```text
id, name, summary, description
isPersonal
picture
assistanceGroupId
assistanceGroup
```

نمایندگان ارائه‌دهنده یک خدمت:

```http
GET /api/Site/Companion?assistanceId={assistanceId}&pageIndex=1&pageSize=12&sortBy=10
```

خود Assistance کاتالوگ نوع خدمت است و قیمت پکیج نماینده را برنمی‌گرداند.

---

# 6. پانسیون‌ها

## لیست پانسیون‌ها

```http
GET /api/Site/Pansion
```

بک فقط پانسیون‌های فعال، تأییدشده و دارای `ShowToSite=true` را برمی‌گرداند.

فیلترها:

| پارامتر | توضیح |
|---|---|
| `q` | جستجو در نام |
| `isSchool` | پانسیون یا مدرسه |
| `companionId` | پانسیون‌های یک نماینده |
| `stateId` | استان |
| `cityId` | شهر |
| `neighborhoodIds` | محله‌ها |
| `suggested` | پیشنهادشده |
| `petId` | نوع حیوان |
| `sortBy=4` | بیشترین امتیاز |
| `sortBy=5` | کمترین امتیاز |
| `sortBy=8` | گران‌ترین |
| `sortBy=9` | ارزان‌ترین |

نمونه:

```http
GET /api/Site/Pansion?cityId=10&petId=1&pageIndex=1&pageSize=12&sortBy=4
```

## جزئیات پانسیون

```http
GET /api/Site/Pansion/{id}
```

فیلدهای مناسب نمایش:

```text
id, name, discription
isSchool
addressValue, state, city
picture, pansionPictures
pansionPets
rateAvg, rateCount, commentCount
suggested
pansionPrice, schoolPrice
regulations
openHour, closeHour
companion
```

موارد زیر داخلی هستند و نمایش داده نشوند:

```text
dailyCommissionPercent
hourlyCommissionPercent
showToSite
approve
pansionComments
```

برای نظرات عمومی پانسیون باید Endpoint فیلترشده مستقل استفاده شود؛ آرایه `pansionComments` پاسخ جزئیات مستقیماً Render نشود.

---

# 7. فروشگاه‌ها

## لیست فروشگاه‌ها

```http
GET /api/Site/Store
```

فیلترها:

| پارامتر | توضیح |
|---|---|
| `q` | جستجو در نام فروشگاه |
| `typeId` | نوع فروشگاه |
| `stateId` | استان |
| `cityId` | شهر |
| `sortBy=10` | بیشترین امتیاز |
| `sortBy=11` | کمترین امتیاز |

نمونه:

```http
GET /api/Site/Store?cityId=10&pageIndex=1&pageSize=12&sortBy=10
```

## جزئیات فروشگاه

```http
GET /api/Site/Store/{id}
```

فیلدهای مناسب نمایش:

```text
id, name, summary, description
phone, mobile, email, address
city, location
picture, icon
type
maxDiscountPercent
rateAvg, rateCount
seo*
```

فیلدهای زیر داخلی هستند و نمایش داده نشوند:

```text
users
referralCode
commissionPercent
showToSite
```

محصولات داخل `StoreVDto` نیستند. اگر در طراحی آینده صفحه فروشگاه باید محصول نشان دهد، قرارداد مستقل Product باید با بک هماهنگ شود؛ از API App بدون هماهنگی استفاده نشود.

---

# 8. داده‌های فیلتر داینامیک

گزینه‌های Dropdown نباید به‌صورت دستی داخل فرانت نوشته شوند.

| داده | Endpoint پیشنهادی |
|---|---|
| استان‌ها | `GET /api/Common/State?pageIndex=1&pageSize=100&available=true` |
| تمام شهرها با استان | `GET /api/Common/City/GetAll` |
| شهرهای یک استان | `GET /api/Common/City?stateId={stateId}&pageIndex=1`؛ این Endpoint در هر صفحه حداکثر ۵۰ نتیجه می‌دهد |
| محله‌ها | `GET /api/Neighborhood?cityId={cityId}&pageIndex=1&pageSize=500&available=true` |
| گروه خدمات | `GET /api/AssistanceGroup?pageIndex=1&pageSize=100` |
| خدمات سایت | `GET /api/Site/Assistance?pageIndex=1&pageSize=200` |
| نوع نماینده | `GET /api/CompanionType?pageIndex=1&pageSize=100` |
| دسته‌ها | `GET /api/Category/label/{rootLabel}` |

در UI مقدار `id` ارسال و `name` نمایش داده شود. برای دسته‌های محتوایی، `label` برای Route و فیلتر ترجیح دارد.

---

# 9. Routeهای پیشنهادی pastil.pet

```text
/
/blog
/blog/:id/:slug?
/companions
/companions/:id/:slug?
/services
/services/:id/:slug?
/pansions
/pansions/:id/:slug?
/stores
/stores/:id/:slug?
/gallery/:label
```

فعلاً API جزئیات Entityها با ID کار می‌کند. Slug در Route فرانت برای SEO است و نباید جای ID درخواست بک را بگیرد.

## SEO صفحات جزئیات

برای Entityهایی که فیلد SEO دارند:

```text
title       = seoTitle || name
h1          = seoH1 || name
description = seoMinDescription || summary
canonical   = seoCanonical || URL فعلی pastil.pet
robots      = seoNoIndex / seoNoFollow
image alt   = seoPictureAlt || name
```

Canonical مربوط به `app.pastil.pet` نباید روی صفحات `pastil.pet` قرار بگیرد.

## SSR و Cache

- درخواست‌های Public در SSR قابل اجرا هستند.
- جزئیات Post فقط یک‌بار در هر Render درخواست شود چون بازدید را افزایش می‌دهد.
- دسته‌ها، گروه خدمات و جایگاه‌های بنر Cache کوتاه‌مدت شوند.
- لیست‌ها باید Loading، Empty State، Error State و Retry محدود داشته باشند.
- Query فیلترها در URL نگه داشته شود تا صفحات قابل Share و Crawl باشند.

---

# 10. امنیت و حریم خصوصی

- فقط داده‌های لازم برای UI از DTO استخراج شوند.
- آبجکت‌های `owner` و `users` در صفحات عمومی Render یا در Store عمومی فرانت نگهداری نشوند.
- `referralCode` کسب‌وکار یا کاربر در سایت عمومی نمایش داده نشود.
- درصد کمیسیون و فیلدهای مدیریتی نمایش داده نشوند.
- HTML دریافتی قبل از Render پاک‌سازی شود.
- شماره تماس فقط از فیلد عمومی خود Companion یا Store خوانده شود.
- از APIهای `/api/Admin/*` در سایت استفاده نشود.
- Token پنل یا App در سایت Public ذخیره نشود.
- `ShowToSite` فقط توسط بک کنترل می‌شود؛ فرانت نباید برای مخفی‌سازی اطلاعات به شرط UI تکیه کند.

# 11. مدیریت خطا

| وضعیت | رفتار فرانت |
|---|---|
| HTTP `200` و `isSuccess=false` | جزئیات: صفحه 404؛ عملیات: نمایش پیام مناسب |
| HTTP `404` | صفحه 404 |
| HTTP `429` | Retry با تأخیر و بدون Loop |
| HTTP `500` | Error State و Retry محدود |
| خطای شبکه | پیام قطع ارتباط و Retry |
| `picture=null` | Placeholder |
| `list=[]` | Empty State واقعی، نه خطا |

# 12. چک‌لیست تحویل آرمان

- [ ] API Base URL از Environment خوانده می‌شود.
- [ ] File Base URL از Environment خوانده می‌شود.
- [ ] فقط APIهای Site برای داده اصلی صفحات معرفی استفاده شده‌اند.
- [ ] `pastil.pet` با `app.pastil.pet` اشتباه نشده است.
- [ ] صفحه‌بندی بر اساس `totalCount` کار می‌کند.
- [ ] فیلترهای Array به‌صورت Query تکرارشونده ارسال می‌شوند.
- [ ] Dropdownها داینامیک هستند.
- [ ] تصاویر null باعث شکستن صفحه نمی‌شوند.
- [ ] HTML نوشته Sanitize می‌شود.
- [ ] فیلدهای داخلی و اطلاعات مالک Render نمی‌شوند.
- [ ] `isSuccess=false` در جزئیات به 404 تبدیل می‌شود.
- [ ] SEO و Canonical برای دامنه `pastil.pet` تنظیم شده‌اند.
- [ ] نظرات Post تا اصلاح فیلتر تأیید عمومی فعال نشده‌اند.

## خلاصه Endpointهای اصلی

```text
GET /api/Site/Banner
GET /api/Site/Banner/{id}

GET /api/Site/Post
GET /api/Site/Post/{id}

GET /api/Site/Gallery
GET /api/Site/Gallery/label/{label}
GET /api/GalleryItem?galleryId={galleryId}

GET /api/Site/Companion
GET /api/Site/Companion/{id}

GET /api/Site/Assistance
GET /api/Site/Assistance/{id}

GET /api/Site/Pansion
GET /api/Site/Pansion/{id}

GET /api/Site/Store
GET /api/Site/Store/{id}
```
