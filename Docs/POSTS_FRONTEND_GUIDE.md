# راهنمای API نوشته‌ها (Posts) — پاستیل

> هر چیزی که برای ساختن صفحه‌ی لیست، جزئیات، فرم ساخت/ویرایش و بخش نظرات نوشته‌ها لازم دارید — بدون نیاز به هیچ دانش قبلی از پروژه.

**سرویس:** Api (.NET 9) · **مبنا:** کد سورس، نه اسپک تولیدشده

---

## فهرست

| # | بخش | # | بخش |
|---|---|---|---|
| ۱ | [شروع سریع](#۱-شروع-سریع) | ۹ | [اندپوینت‌های پنل ادمین](#۹-اندپوینتهای-پنل-ادمین) |
| ۲ | [قرارداد پاسخ](#۲-قرارداد-پاسخ) | ۱۰ | [فیلتر و مرتب‌سازی](#۱۰-فیلتر-و-مرتبسازی) |
| ۳ | [احراز هویت](#۳-احراز-هویت) | ۱۱ | [تصاویر و فایل‌ها](#۱۱-تصاویر-و-فایلها) |
| ۴ | [مدل نوشته](#۴-مدل-نوشته) | ۱۲ | [دسته‌بندی و هشتگ](#۱۲-دستهبندی-و-هشتگ) |
| ۵ | [وضعیت انتشار](#۵-وضعیت-انتشار) | ۱۳ | [نظرات](#۱۳-نظرات) |
| ۶ | [Label و Slug](#۶-label-و-slug) | ۱۴ | [نسخه‌بندی ویرایش](#۱۴-نسخهبندی-ویرایش) |
| ۷ | [فیلدهای SEO](#۷-فیلدهای-seo) | ۱۵ | [نمونه‌های کامل](#۱۵-نمونههای-کامل) |
| ۸ | [اندپوینت‌های عمومی](#۸-اندپوینتهای-عمومی) | ۱۶ | [تله‌های رایج](#۱۶-تلههای-رایج) |

---

## ۱. شروع سریع

سه سرویس مستقل در کارند. برای نوشته‌ها فقط با دوتای اول کار دارید:

| سرویس | Base URL | کارش |
|---|---|---|
| Api | `https://api.pastil.pet/api` | همه‌ی داده‌ها: نوشته، دسته، نظر، احراز هویت |
| File | `https://file.pastil.pet/api` | فقط آپلود تصویر و فایل |
| File (استاتیک) | `https://file.pastil.pet` | سرو کردن خود فایل‌ها |

ساده‌ترین درخواست ممکن — گرفتن ۱۰ نوشته‌ی منتشرشده:

```js
const res  = await fetch("https://api.pastil.pet/api/Post?pageIndex=1&pageSize=10");
const data = await res.json();

console.log(data.totalCount);   // 137
console.log(data.list);         // [{ id, name, slug, publishDate, ... }, ...]
```

> **نکته‌ی اول و مهم‌ترین:** این اندپوینت هیچ توکنی نمی‌خواهد و فقط نوشته‌های واقعاً منتشرشده را برمی‌گرداند. سرور خودش فیلتر انتشار را اعمال می‌کند و کلاینت نمی‌تواند دورش بزند.

---

## ۲. قرارداد پاسخ

بک‌اند تقریباً همیشه **HTTP 200** برمی‌گرداند — حتی وقتی عملیات شکست خورده. وضعیت واقعی داخل بدنه است. اگر فقط `res.ok` را چک کنید، خطاها را بی‌صدا از دست می‌دهید.

اما **دو شکل پاسخ متفاوت** وجود دارد و تفکیکشان حیاتی است:

### الف) پاسخ تکی — با پوشش `BaseResultDto`

هر اندپوینتی که **یک آیتم** برمی‌گرداند یا یک **عملیات نوشتن** است:

```jsonc
{
  "isSuccess": true,
  "code": 0,
  "messages": [],
  "data": { "id": 42, "name": "...", ... }
}

// حالت خطا — باز هم HTTP 200
{
  "isSuccess": false,
  "code": 0,
  "messages": [ { "item1": "برچسب تکراری است", "item2": "Label" } ],
  "data": null
}
```

`messages` آرایه‌ای از تاپل است: `item1` متن پیام (فارسی، آماده‌ی نمایش) و `item2` نام فیلد مقصر (ممکن است رشته‌ی خالی باشد). برای اتصال خطا به فیلد فرم از `item2` استفاده کنید.

### ب) پاسخ لیستی — **بدون** پوشش

اندپوینت‌های جستجو مستقیماً آبجکت صفحه‌بندی را برمی‌گردانند. اینجا **هیچ** `isSuccess` یا `data`ای وجود ندارد:

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "q": null,
  "sortBy": 1,
  "totalCount": 137,
  "list": [ { }, { } ]
}
```

> ⚠️ **تله:** اگر روی نتیجه‌ی `GET /api/Post` دنبال `response.data.list` بگردید، همیشه `undefined` می‌گیرید. مسیر درست `response.list` است. برعکسش هم صادق است: `GET /api/Post/{id}` حتماً `response.data` می‌خواهد.

یک هلپر که هر دو حالت را پوشش می‌دهد:

```js
async function call(path, options) {
  const res  = await fetch(`https://api.pastil.pet/api${path}`, options);
  const body = await res.json();

  // شکل «لیست»: خودش نتیجه است
  if (Array.isArray(body?.list)) return body;

  // شکل «پوشش‌دار»: باید isSuccess چک شود
  if (body?.isSuccess === false) {
    const msg = body.messages?.[0]?.item1 ?? "عملیات ناموفق بود.";
    throw new Error(msg);
  }
  return body.data ?? body;
}
```

> **حساسیت حروف:** سریالایزر پیش‌فرض `camelCase` می‌دهد (`isSuccess`، `publishDate`). اما در بعضی مسیرها ممکن است `PascalCase` ببینید. اگر می‌خواهید مقاوم باشید، هر دو را چک کنید: `body.data ?? body.Data`.

---

## ۳. احراز هویت

JWT با هدر `Authorization: Bearer <token>`. خواندن نوشته‌ها توکن نمی‌خواهد؛ ساخت/ویرایش/حذف و آپلود می‌خواهد.

### `POST /api/Account/signin` · بدون توکن

ورود با موبایل + رمز، یا موبایل + کد یک‌بارمصرف.

```jsonc
{
  "mobile":     "09120000000",
  "password":   "...",     // یا خالی بگذارید و از code استفاده کنید
  "code":       "",        // کد OTP
  "isAdmin":    false,     // برای ورود به پنل ادمین true
  "rememberMe": true
}
```

جریان ورود با کد یک‌بارمصرف:

```
POST /Account/userdetail  →  POST /Account/otp  →  POST /Account/CheckOtp  →  POST /Account/signin
```

`userdetail` می‌گوید کاربر در چه وضعیتی است: `SignUp=1` (باید ثبت‌نام کند)، `OneFactor=2`، `TwoFactor=3`، `Locked=4`.

> **درباره‌ی دسترسی ادمین:** فقط اندپوینت‌های زیر مسیر `/api/Admin/…` با سیستم مجوز نقش‌محور محافظت می‌شوند. بقیه‌ی ناحیه‌ها صرفاً «کاربر لاگین‌کرده» می‌خواهند. اگر با نقش غیرادمین به `/api/Admin/Post` بزنید، خطای دسترسی می‌گیرید.

---

## ۴. مدل نوشته

دو شکل متفاوت وجود دارد و اشتباه‌گرفتنشان شایع‌ترین خطاست: **PostVDto** چیزی است که **می‌خوانید**، و **PostDto** چیزی است که **می‌نویسید**.

### PostVDto — پاسخ خواندن

| فیلد | نوع | توضیح |
|---|---|---|
| `id` | long | شناسه |
| `name` | string | عنوان نوشته (تنها فیلدی که `q` رویش جستجو می‌کند) |
| `label` | string | شناسه‌ی لاتین؛ مبنای ساخت slug |
| `slug` | string | تولید سرور. برای URL از این استفاده کنید |
| `subject` | string | تیتر نمایشی (ممکن است با `name` فرق کند) |
| `subNews` | string | زیرتیتر |
| `summary` | string | خلاصه — مناسب کارت لیست |
| `description` | string | متن کامل، HTML |
| `pictureId` | long? | شناسه‌ی تصویر شاخص |
| `picture` | PictureVDto | آبجکت کامل تصویر (بخش ۱۱) |
| `pictureUrl` | string | مسیر متنی جایگزین؛ ممکن است خالی باشد |
| `publishDate` | DateTime | زمان انتشار؛ آینده = هنوز منتشر نشده |
| `createDate` | DateTime | زمان ساخت |
| `active` | bool | فعال بودن |
| `adminConfirm` | bool | تایید ادمین |
| `visitCount` | int | تعداد بازدید |
| `commentCount` | int | فقط نظرات **تاییدشده** |
| `isOld` | bool | پرچم «آرشیوی» |
| `edited` | bool | نسخه‌ی ویرایشی تاییدنشده دارد (بخش ۱۴) |
| `categoryId` | long? | دسته‌ی اصلی |
| `category` | CategoryParentVDto | دسته با زنجیره‌ی والدها |
| `user` | UserVDto | نویسنده |
| `admin` | UserVDto | ادمین تاییدکننده |
| `hashtags` | `[{id,name}]` | هشتگ‌ها |
| `postPictures` | PostPictureVDto[] | گالری |
| `postFiles` | PostFileVDto[] | پیوست‌ها |
| `children` | PostVDto[] | فقط در حالت ادمین پر می‌شود |

> ⚠️ **دقت:** در پاسخ خواندن، گالری و پیوست‌ها `postPictures` و `postFiles` نام دارند، ولی هنگام **نوشتن** باید `postPicturesList` و `postFilesList` بفرستید. نام‌ها یکی نیستند.

### PostDto — بدنه‌ی ساخت و ویرایش

| فیلد | نوع | الزامی | توضیح |
|---|---|---|---|
| `id` | long | در ویرایش | در ساخت نفرستید یا صفر بگذارید |
| `name` | string | بله | عنوان |
| `label` | string | بله | فقط `A-Z a-z 0-9` و `-` `_` و فاصله (بخش ۶) |
| `publishDate` | DateTime | بله | ISO-8601. گذشته = فوری منتشر شود |
| `active` | bool | بله | — |
| `summary` | string | خیر | — |
| `description` | string | خیر | HTML بدنه |
| `subject` / `subNews` | string | خیر | — |
| `pictureId` | long? | خیر | از آپلود می‌آید |
| `categoryId` | long? | خیر | دسته‌ی اصلی |
| `categoryIds` | long[] | خیر | دسته‌های تکمیلی |
| `hashTagList` | string[] | خیر | متن هشتگ‌ها، نه شناسه |
| `postPicturesList` | object[] | خیر | `{id, postId, pictureId, name, label}` |
| `postFilesList` | object[] | خیر | `{id, postId, fileId, name, label}` |
| `adminConfirm` | bool | خیر | عملاً از مسیر `confirm` ست می‌شود |
| `isOld` | bool | خیر | — |

> **رفتار مهم `categoryId`:** وقتی مقدار می‌دهید، سرور خودش تمام دسته‌های **والد** آن را هم به `categoryIds` اضافه می‌کند. یعنی نوشته‌ای در «غذای سگ» خودکار زیر «سگ» هم پیدا می‌شود. لازم نیست والدها را دستی بفرستید.

---

## ۵. وضعیت انتشار

یک نوشته وقتی **عمومی دیده می‌شود** که هر سه شرط برقرار باشد:

| شرط | یعنی |
|---|---|
| `active == true` | نویسنده فعالش کرده |
| `adminConfirm == true` | ادمین تاییدش کرده |
| `publishDate < now` | زمان انتشار رسیده |

پارامتر `available=true` همین سه شرط را با هم اعمال می‌کند. اندپوینت‌های عمومی (`/api/Post` و `/api/Site/Post`) آن را **اجباراً** روی `true` می‌گذارند — مقداری که کلاینت بفرستد نادیده گرفته می‌شود.

`adminConfirm` در دیتابیس سه‌حالته است و این تفاوت در پنل مهم است:

| مقدار | معنی | چطور فیلتر کنیم |
|---|---|---|
| `null` | در انتظار بررسی | `allAdminConfirm=false` بدون `adminConfirm` |
| `true` | تاییدشده | `allAdminConfirm=false&adminConfirm=true` |
| `false` | ردشده | `allAdminConfirm=false&adminConfirm=false` |

> ⚠️ **رفتار غیرمنتظره‌ای که باید بدانید:** کوئری پایه‌ی جستجو **همیشه** شرط `active == true` را دارد — حتی در حالت ادمین. بنابراین `?active=false` همیشه لیست خالی برمی‌گرداند و «نوشته‌های غیرفعال» را از راه جستجو نمی‌شود دید. برای رسیدن به یک نوشته‌ی غیرفعال باید مستقیم با شناسه‌اش صدایش بزنید.

---

## ۶. Label و Slug

`label` را کاربر می‌نویسد؛ `slug` را سرور از رویش می‌سازد و در URL استفاده می‌شود. قوانین نرمال‌سازی:

- حروف انگلیسی به کوچک تبدیل می‌شوند.
- فاصله، `_`، `-` و علائم پشت‌سرهم به یک `-` تبدیل می‌شوند.
- علائم اضافی حذف می‌شوند.
- **حروف فارسی پذیرفته نمی‌شود** و باعث خطا می‌شود.
- اگر بعد از پاک‌سازی چیزی نماند، درخواست رد می‌شود.
- slug باید بین نوشته‌ها **یکتا** باشد.

```
"How To Care  For Dogs!"  →  "how-to-care-for-dogs"
"راهنمای نگهداری"          →  خطا: فقط حروف انگلیسی و اعداد مجاز است
```

همین منطق را سمت فرانت هم اعمال کنید تا کاربر اصلاً نتواند فارسی تایپ کند:

```js
const sanitizeLabel = (v) => String(v ?? "").replace(/[^A-Za-z0-9 _-]/g, "");

const previewSlug = (v) =>
  sanitizeLabel(v).trim().toLowerCase()
    .replace(/[\s_-]+/g, "-")
    .replace(/^-+|-+$/g, "");
```

### بررسی زنده‌ی تکراری‌نبودن

### `GET /api/Admin/SitePost/CheckLabel?label=...&excludeId=...` · توکن ادمین · 🆕 جدید

قبل از ثبت، آزاد بودن label را چک می‌کند — مثل یوزرنیم تلگرام. در حالت ویرایش، `excludeId` را شناسه‌ی همان نوشته بگذارید تا label خودش تکراری حساب نشود.

```jsonc
// آزاد است
{ "isSuccess": true,  "data": true,  "messages": [] }

// قبلاً استفاده شده
{ "isSuccess": true,  "data": false, "messages": [] }

// label نامعتبر (مثلاً فارسی)
{ "isSuccess": false, "data": false,
  "messages": [ { "item1": "Label فقط می‌تواند شامل حروف انگلیسی و اعداد باشد.", "item2": "" } ] }
```

> 💡 **الگوی پیشنهادی:** با ۵۰۰ms تاخیر (debounce) بعد از تایپ صدا بزنید، و درخواست‌های قدیمی‌تر را با یک شمارنده باطل کنید تا پاسخ‌های دیررس وضعیت جدید را خراب نکنند.

---

## ۷. فیلدهای SEO

روی هر نوشته موجودند، هم در خواندن و هم در نوشتن. هیچ‌کدام الزامی نیستند:

| فیلد | کاربرد |
|---|---|
| `seoTitle` | تگ `<title>` |
| `seoMinDescription` | متا دیسکریپشن |
| `seoDescription` | توضیح بلند |
| `seoH1` | تیتر `H1` صفحه |
| `seoPictureAlt` | متن جایگزین تصویر شاخص |
| `seoUrlText` | متن دلخواه URL |
| `seoCanonical` | لینک کانونیکال |
| `seoNoIndex` | `bool` — در نتایج ثبت نشود |
| `seoNoFollow` | `bool` — لینک‌ها دنبال نشوند |

> **محدودیت طول ندارند.** نه سرور و نه پنل دیگر تعداد کاراکتر را محدود نمی‌کنند. اگر می‌خواهید راهنمای بصری بدهید (مثلاً ۶۰ کاراکتر برای عنوان)، صرفاً نمایشی باشد و جلوی ثبت را نگیرد.

---

## ۸. اندپوینت‌های عمومی

### `GET /api/Post` · بدون توکن
لیست نوشته‌های منتشرشده. پاسخ: شکل «لیست» (بخش ۲-ب). پارامترها در بخش ۱۰.

### `GET /api/Post/{id}` · بدون توکن
یک نوشته با تمام روابط. پاسخ: `BaseResultDto<PostVDto>`.
**هر فراخوانی `visitCount` را یک واحد زیاد می‌کند** — پس در حالت StrictMode یا رندر دوباره، بی‌دلیل صدایش نزنید.

### `GET /api/Site/Post` · بدون توکن
دقیقاً همان رفتار `/api/Post`. برای سایت معرفی جدا شده است.

### `GET /api/Site/Post/{id}` · بدون توکن
معادل `/api/Post/{id}`، با همان افزایش بازدید.

### `GET /api/PostSiteMap` · بدون توکن
فهرست سبک برای ساخت sitemap: `{ id, name, categoryName, updateDate }`. پاسخ پوشش‌دار است.

### `GET /api/PostProduct/{id}` · بدون توکن
محصولات فروشگاهی مرتبط با یک نوشته. `{id}` شناسه‌ی نوشته است.

---

## ۹. اندپوینت‌های پنل ادمین

دو خانواده‌ی موازی وجود دارد که **روی یک داده** کار می‌کنند: `/api/Admin/SitePost` ساده و CRUD خالص است، `/api/Admin/Post` همان است به‌علاوه‌ی گردش‌کار تایید. برای فرم ساخت/ویرایش، `SitePost` کافی است.

### SitePost

| متد | مسیر | کار |
|---|---|---|
| `GET` | `/api/Admin/SitePost` | جستجو با تمام فیلترهای بخش ۱۰. پاسخ: شکل «لیست» |
| `GET` | `/api/Admin/SitePost/{id}` | پاسخ `PostDto` است (نه `PostVDto`) — همان شکلی که برای ویرایش می‌فرستید |
| `POST` | `/api/Admin/SitePost` | ساخت. بدنه: `PostDto`. `id` نفرستید |
| `PUT` | `/api/Admin/SitePost` | ویرایش. بدنه: `PostDto` کامل به‌همراه `id` |
| `DELETE` | `/api/Admin/SitePost?id={id}` | حذف نرم — رکورد می‌ماند و فقط `deleted` می‌شود |

### Post — گردش‌کار تایید

| متد | مسیر | بدنه |
|---|---|---|
| `PUT` | `/api/Admin/Post/confirm` | `{ "id": 42, "adminConfirm": true, "publishDate": "2026-09-01T08:00:00" }` |
| `PUT` | `/api/Admin/Post/changeuser` | `{ "id": 42, "userId": 7 }` |

`GET` / `POST` / `PUT` / `DELETE` روی `/api/Admin/Post` هم موجودند و رفتارشان با `SitePost` یکسان است.

### گالری و پیوست به‌صورت جداگانه

معمولاً کافی است تصاویر و فایل‌ها را داخل `postPicturesList` / `postFilesList` همراه خود نوشته بفرستید. اما اگر می‌خواهید بدون ذخیره‌ی کل نوشته یک آیتم را مدیریت کنید:

| متد | مسیر | کار |
|---|---|---|
| `GET` | `/api/Admin/PostPicture?postId={id}` | گالری یک نوشته |
| `POST` | `/api/Admin/PostPicture` | افزودن تصویر |
| `PUT` | `/api/Admin/PostPicture` | ویرایش عنوان/برچسب |
| `DELETE` | `/api/Admin/PostPicture?id={id}` | حذف از گالری |
| `GET/POST/PUT/DELETE` | `/api/Admin/PostFile` | همان الگو برای پیوست‌ها |
| `POST` | `/api/Admin/PostProduct` | اتصال محصول به نوشته |

---

## ۱۰. فیلتر و مرتب‌سازی

همه به‌صورت query string. روی `/api/Post`، `/api/Site/Post` و `/api/Admin/SitePost` یکسان کار می‌کنند.

| پارامتر | نوع | پیش‌فرض | رفتار |
|---|---|---|---|
| `pageIndex` | int | `1` | شماره‌ی صفحه، از ۱ |
| `pageSize` | int | `20` | تعداد در صفحه |
| `q` | string | `null` | **فقط داخل `name`** می‌گردد؛ متن و خلاصه را نمی‌گردد |
| `sortBy` | int | `1` | جدول پایین |
| `categoryIds` | long[] | — | تکرار کلید: `categoryIds=3&categoryIds=8` |
| `categoryLabels` | string[] | — | **فقط وقتی `categoryIds` خالی باشد** اعمال می‌شود |
| `isAndCategories` | bool | `false` | `false` = هر کدام، `true` = همه‌ی دسته‌ها |
| `hashtags` | string | — | با `-` جدا شود؛ منطق **AND**: `سگ-تغذیه` |
| `publish` | bool | — | `true` = گذشته، `false` = زمان‌بندی‌شده |
| `adminConfirm` | bool | — | با `allAdminConfirm=false` معنا پیدا می‌کند |
| `allAdminConfirm` | bool | — | `false` فیلتر تایید را روشن می‌کند |
| `edited` | bool | — | فقط نوشته‌های دارای نسخه‌ی ویرایشی |
| `notId` | long | — | حذف یک شناسه — مناسب «مطالب مرتبط» |
| `active` | bool | — | عملاً بی‌اثر (بخش ۵) |

### مقادیر `sortBy`

| مقدار | نام | ترتیب |
|---|---|---|
| `0` | Default | جدیدترین بر اساس شناسه |
| `1` | New | جدیدترین بر اساس شناسه |
| `2` | Old | قدیمی‌ترین |
| `3` | Name | نام، نزولی |
| `4` | MoreVisit | پربازدیدترین |
| `5` | LessVisit | کم‌بازدیدترین |
| `6` | MorePriority | تاریخ انتشار، نزولی |
| `7` | LessPriority | تاریخ انتشار، صعودی |

> بقیه‌ی مقادیر enum (۸ تا ۱۱، مربوط به قیمت و فروش) برای نوشته‌ها بی‌اثرند و ترتیب دیتابیس را می‌دهند.

---

## ۱۱. تصاویر و فایل‌ها

آپلود روی **سرویس File** انجام می‌شود، نه Api. جریان همیشه دو مرحله است:

```
آپلود روی File → گرفتن id  →  فرستادن id همراه نوشته به Api
```

### `POST https://file.pastil.pet/api/PictureUpload` · توکن

`multipart/form-data` با نام فیلد **`PictureFile`**. حداکثر **۵ مگابایت**.
فرمت‌ها: `jpg jpeg png webp` و ویدیو `mp4 webm ogg`.

تصاویر خودکار به **WebP** تبدیل و در سه اندازه‌ی `lg` (۹۰۰px)، `md` (۵۰۰px) و `sm` (۳۰۰px) ذخیره می‌شوند. ویدیوها دست‌نخورده می‌مانند.

### `POST https://file.pastil.pet/api/FileUpload` · توکن

نام فیلد **`file`**. حداکثر **۲۰ مگابایت**.
فرمت‌ها: `jpg jpeg png webp gif mp4 webm mov pdf`.

```js
async function uploadPicture(file, token) {
  const fd = new FormData();
  fd.append("PictureFile", file);          // نام فیلد دقیقاً همین است

  const res  = await fetch("https://file.pastil.pet/api/PictureUpload", {
    method:  "POST",
    headers: { Authorization: `Bearer ${token}` },   // Content-Type را ست نکنید
    body:    fd,
  });
  const body = await res.json();
  if (!body.isSuccess) throw new Error(body.messages?.[0]?.item1 ?? "آپلود ناموفق");

  return body.data;   // { id, url, baseUrl, guidName, extension, orginalName }
}
```

### ساختن URL نمایش

آبجکت `PictureVDto` این فیلدها را دارد:

| فیلد | نمونه | یعنی |
|---|---|---|
| `url` | `/Media/2026/8/28/a1b2.webp` | مسیر کامل تصویر اصلی |
| `baseUrl` | `/Media/2026/8/28` | فقط پوشه |
| `guidName` | `a1b2` | نام بدون پسوند |
| `extension` | `.webp` | پسوند |
| `orginalName` | `dog.jpg` | نام اصلی کاربر (املا با یک `i`) |

```js
const FILE_HOST = "https://file.pastil.pet";

// تصویر اصلی
const full = (p) => (p?.url ? FILE_HOST + p.url : "");

// بندانگشتی: "lg" | "md" | "sm"
const thumb = (p, size) =>
  p?.baseUrl && p?.guidName
    ? `${FILE_HOST}${p.baseUrl}/${p.guidName}-${size}${p.extension}`
    : full(p);

// <img src={thumb(post.picture, "md")} alt={post.seoPictureAlt || post.name} />
```

> بندانگشتی‌ها فقط برای تصاویر ساخته می‌شوند. برای ویدیو `guidName-lg.webp` وجود ندارد؛ روی `url` برگردید. همچنین اگر تصویر اصلی از ۹۰۰px کوچک‌تر بوده، اندازه‌ها با هم برابرند ولی فایل‌ها موجودند.

---

## ۱۲. دسته‌بندی و هشتگ

دسته‌ها درختی‌اند و هر نوشته یک دسته‌ی اصلی و چند دسته‌ی تکمیلی دارد.

| متد | مسیر | کار |
|---|---|---|
| `GET` | `/api/Category` | جستجوی دسته‌ها (بدون توکن) |
| `GET` | `/api/Category/{id}` | یک دسته |
| `GET` | `/api/Category/label/{label}` | دسته با برچسب لاتین |
| `GET` | `/api/CategoryParent/{id}` | دسته به‌همراه زنجیره‌ی والدها |
| `GET` | `/api/Admin/Category` | مدیریت دسته‌ها (توکن ادمین) |
| `GET` | `/api/Admin/Hashtag` | فهرست هشتگ‌ها برای autocomplete |

فیلدهای مهم دسته: `id`، `name`، `label`، `slug`، `parentId`، `priority`، `picture`، `icon`.

> 💡 **نکته‌ی مهم درباره‌ی برچسب دسته‌ها:** برچسب دسته فقط **زیر یک والد مشترک** باید یکتا باشد. یعنی «دامپزشکی» می‌تواند هم‌زمان زیر «سگ» و زیر «گربه» وجود داشته باشد. پس برای شناسایی قطعی یک دسته همیشه از `id` استفاده کنید، نه `label`.

هشتگ‌ها موقع ذخیره‌ی نوشته به‌صورت آرایه‌ی متنی می‌روند و سرور خودش می‌سازدشان:

```jsonc
{ "hashTagList": ["سگ", "تغذیه", "توله"] }
```

```
// و در فیلتر با - جدا می‌شوند (منطق AND)
GET /api/Post?hashtags=سگ-تغذیه
```

---

## ۱۳. نظرات

| متد | مسیر | دسترسی | کار |
|---|---|---|---|
| `GET` | `/api/PostComment?postId={id}` | بدون توکن | نظرات یک نوشته. پاسخ: شکل «لیست» |
| `POST` | `/api/PostComment` | بدون توکن | ثبت نظر مهمان. بدنه: `{ postId, name, text, rate }` |
| `POST` | `/api/EndUser/PostComment` | توکن | ثبت نظر با حساب کاربری |
| `PUT` | `/api/Admin/PostComment` | توکن ادمین | تایید/رد و پاسخ. بدنه: `{ id, statusId, answer }` |

وضعیت‌ها:

| Label | یعنی |
|---|---|
| `Comment_NotChecked` | در انتظار بررسی — وضعیت هر نظر تازه |
| `Comment_Accept` | تاییدشده — فقط این‌ها باید نمایش داده شوند |
| `Comment_Reject` | ردشده |

> 🔴 **این مورد را جدی بگیرید:** اندپوینت عمومی نظرات **هیچ فیلتری روی وضعیت اعمال نمی‌کند**. یعنی `GET /api/PostComment?postId=5` نظرات بررسی‌نشده و حتی **ردشده** را هم برمی‌گرداند.
>
> اگر خودتان فیلتر نکنید، محتوای تاییدنشده روی سایت منتشر می‌شود. حتماً سمت کلاینت فیلتر کنید:

```js
const visible = (data.list ?? []).filter(
  (c) => c.status?.label === "Comment_Accept"
);
```

دقت کنید `commentCount` روی خود نوشته فقط نظرات **تاییدشده** را می‌شمارد، پس ممکن است با طول لیست فیلترنشده نخواند — و این طبیعی است.

ساختار هر نظر: `id`، `name`، `text`، `rate`، `createDate`، `answer`، `likeCount`، `disLikeCount`، `user`، `status` (`{id, name, label}`).

> متن نظر سمت سرور از HTML خطرناک پاک‌سازی می‌شود، و `statusId`ای که کلاینت بفرستد هنگام ثبت **نادیده گرفته می‌شود** — همیشه «بررسی‌نشده» ذخیره می‌شود.

---

## ۱۴. نسخه‌بندی ویرایش

رفتاری هست که اگر ندانید گیج‌کننده می‌شود: یک نوشته می‌تواند یک **نسخه‌ی فرزند** داشته باشد که ویرایشِ در انتظارِ تایید است.

- `parentId` پر باشد یعنی این رکورد خودش یک **پیش‌نویس ویرایش** است، نه نوشته‌ی اصلی.
- `edited: true` روی نوشته‌ی اصلی یعنی نسخه‌ی ویرایشی تاییدنشده دارد.
- وقتی ادمین آن نسخه را تایید می‌کند، محتوایش روی نوشته‌ی اصلی می‌نشیند و فرزند حذف نرم می‌شود.

> 💡 **برای فرانت چه اهمیتی دارد؟** جستجو همیشه شرط `parentId == null` دارد، پس پیش‌نویس‌ها هرگز در لیست‌ها ظاهر نمی‌شوند و نیازی نیست کاری کنید. فقط اگر پنل می‌سازید، می‌توانید با `edited=true` فیلتر کنید و نشان «ویرایش در انتظار تایید» بگذارید.

---

## ۱۵. نمونه‌های کامل

### صفحه‌ی لیست با صفحه‌بندی و فیلتر دسته

```js
async function fetchPosts({ page = 1, size = 12, categoryId, search } = {}) {
  const qs = new URLSearchParams({
    pageIndex: String(page),
    pageSize:  String(size),
    sortBy:    "1",              // جدیدترین
  });
  if (categoryId) qs.append("categoryIds", String(categoryId));
  if (search)     qs.set("q", search);

  const res  = await fetch(`https://api.pastil.pet/api/Post?${qs}`);
  const data = await res.json();

  return {
    items: data.list ?? [],
    total: data.totalCount ?? 0,
    pages: Math.ceil((data.totalCount ?? 0) / size),
  };
}
```

### صفحه‌ی جزئیات + مطالب مرتبط

```js
async function fetchPostPage(id) {
  const res  = await fetch(`https://api.pastil.pet/api/Post/${id}`);
  const body = await res.json();

  if (!body.isSuccess || !body.data) throw new Error("نوشته پیدا نشد.");
  const post = body.data;

  // مرتبط: هم‌دسته، با حذف خود نوشته
  const qs = new URLSearchParams({ pageSize: "4", notId: String(id) });
  if (post.categoryId) qs.append("categoryIds", String(post.categoryId));

  const rel = await (await fetch(`https://api.pastil.pet/api/Post?${qs}`)).json();

  return { post, related: rel.list ?? [] };
}
```

### ساخت نوشته با تصویر شاخص

```js
async function createPost({ token, file, form }) {
  // ۱) آپلود تصویر روی سرویس File
  let pictureId = null;
  if (file) {
    const fd = new FormData();
    fd.append("PictureFile", file);
    const up = await (await fetch("https://file.pastil.pet/api/PictureUpload", {
      method: "POST",
      headers: { Authorization: `Bearer ${token}` },
      body: fd,
    })).json();
    if (!up.isSuccess) throw new Error(up.messages?.[0]?.item1 ?? "آپلود ناموفق");
    pictureId = up.data.id;
  }

  // ۲) بررسی آزاد بودن label
  const chk = await (await fetch(
    `https://api.pastil.pet/api/Admin/SitePost/CheckLabel?label=${encodeURIComponent(form.label)}`,
    { headers: { Authorization: `Bearer ${token}` } },
  )).json();
  if (chk.isSuccess && chk.data === false) throw new Error("این Label قبلاً استفاده شده است.");

  // ۳) ثبت نوشته
  const res = await fetch("https://api.pastil.pet/api/Admin/SitePost", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization:  `Bearer ${token}`,
    },
    body: JSON.stringify({
      name:        form.name,
      label:       form.label,
      summary:     form.summary,
      description: form.description,
      publishDate: new Date(form.publishDate).toISOString(),
      active:      true,
      pictureId,
      categoryId:  form.categoryId,
      categoryIds: form.categoryIds ?? [],
      hashTagList: form.hashtags ?? [],
      seoTitle:           form.seoTitle ?? "",
      seoMinDescription:  form.seoMinDescription ?? "",
      seoNoIndex:  false,
      seoNoFollow: false,
      postPicturesList: [],
      postFilesList:    [],
    }),
  });

  const body = await res.json();
  if (!body.isSuccess) throw new Error(body.messages?.[0]?.item1 ?? "ثبت ناموفق");
  return body.data;
}
```

### ویرایش

```js
// ۱) شکل قابل‌ویرایش را بگیرید (PostDto، نه PostVDto)
const cur = (await (await fetch(`https://api.pastil.pet/api/Admin/SitePost/${id}`, {
  headers: { Authorization: `Bearer ${token}` },
})).json()).data;

// ۲) تغییرات را روی همان بریزید و کامل برگردانید — PUT جزئی نیست
await fetch("https://api.pastil.pet/api/Admin/SitePost", {
  method: "PUT",
  headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
  body: JSON.stringify({ ...cur, name: "عنوان تازه" }),
});
```

> 🔴 **`PUT` جایگزینی کامل است، نه patch.** اگر فقط چند فیلد بفرستید، بقیه با مقدار پیش‌فرض بازنویسی می‌شوند و مثلاً دسته‌ها و هشتگ‌ها پاک می‌شوند. همیشه اول رکورد فعلی را بگیرید، رویش تغییر بدهید و کامل برگردانید.

---

## ۱۶. تله‌های رایج

| نشانه | علت و راه‌حل |
|---|---|
| لیست همیشه `undefined` | روی پاسخ جستجو دنبال `data.list` گشته‌اید. جستجو پوشش ندارد → `response.list` |
| خطا رخ می‌دهد ولی کد ادامه می‌دهد | فقط `res.ok` چک شده. همیشه `body.isSuccess` را ببینید |
| «Slug ساخته‌شده تکراری است» | label تکراری است. با `CheckLabel` قبل از ثبت بررسی کنید |
| «Label فقط می‌تواند شامل حروف انگلیسی…» | فارسی در label. ورودی را حین تایپ پاک‌سازی کنید |
| دسته‌ها بعد از ویرایش پاک شدند | `PUT` ناقص فرستاده‌اید. رکورد کامل بفرستید |
| گالری بعد از ذخیره خالی است | `postPictures` فرستاده‌اید؛ نام درست `postPicturesList` است |
| نظرات تاییدنشده نمایش داده می‌شوند | سرور فیلتر نمی‌کند. روی `status.label === "Comment_Accept"` فیلتر کنید |
| نوشته‌ی تازه در لیست عمومی نیست | یکی از سه شرط بخش ۵ برقرار نیست — معمولاً `adminConfirm` |
| بازدید بی‌دلیل بالا می‌رود | `GET /api/Post/{id}` شمارنده را زیاد می‌کند. در افکت‌های تکراری صدایش نزنید |
| `?active=false` خالی برمی‌گرداند | رفتار شناخته‌شده‌ی کوئری پایه (بخش ۵) |
| آپلود ۴۱۵ یا خطای فرمت می‌دهد | `Content-Type` را دستی ست کرده‌اید. با `FormData` نگذاریدش تا مرورگر خودش boundary بسازد |
| نام فیلد آپلود کار نمی‌کند | تصویر `PictureFile` و فایل `file` — دقیقاً با همین حروف |

> 💡 **در یک جمله:** جستجو بدون پوشه، تکی با پوشه؛ `isSuccess` را همیشه چک کنید؛ `PUT` را کامل بفرستید؛ و نظرات را خودتان فیلتر کنید.
