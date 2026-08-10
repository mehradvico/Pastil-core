# مستند Post برای فرانت سایت Pastil

این مستند مربوط به نمایش پست‌ها، جزئیات پست، دسته‌بندی، تصاویر، فایل‌ها، کامنت و Like/Dislike کامنت در سایت `pastil.pet` است.

## آدرس سرویس‌ها

در مثال‌ها از متغیرهای زیر استفاده شده است:

```text
API_BASE_URL=https://api.pastil.pet
FILE_BASE_URL=https://file.pastil.pet
```

پاسخ‌های لیستی مستقیماً به شکل زیر هستند:

```json
{
  "totalCount": 25,
  "pageIndex": 1,
  "pageSize": 10,
  "list": []
}
```

پاسخ عملیات و دریافت جزئیات معمولاً این ساختار را دارد:

```json
{
  "isSuccess": true,
  "messages": [],
  "code": 0,
  "data": {}
}
```

ملاک موفقیت عملیات مقدار `isSuccess` است؛ صرفاً به HTTP Status اکتفا نشود.

## 1. دریافت لیست پست‌های منتشرشده

```http
GET /api/Site/Post
```

نمونه:

```http
GET /api/Site/Post?pageIndex=1&pageSize=12&sortBy=1
```

پارامترهای قابل استفاده:

| پارامتر | نوع | توضیح |
|---|---:|---|
| `pageIndex` | number | شماره صفحه؛ پیش‌فرض `1` |
| `pageSize` | number | تعداد در صفحه؛ پیش‌فرض `20` |
| `q` | string | جستجو در فیلد `name` پست |
| `sortBy` | number | نوع مرتب‌سازی |
| `categoryIds` | number[] | فیلتر بر اساس شناسه دسته‌ها |
| `categoryLabels` | string[] | فیلتر بر اساس Label دسته‌ها |
| `isAndCategories` | boolean | اگر `true` باشد پست باید همه دسته‌های ارسال‌شده را داشته باشد |
| `hashtags` | string | هشتگ‌ها با جداکننده `-`، مانند `dog-health-food` |
| `notId` | number | حذف یک پست از نتیجه، مناسب پست‌های مرتبط |

مقادیر `sortBy`:

| مقدار | مفهوم |
|---:|---|
| `0` | پیش‌فرض |
| `1` | جدیدترین |
| `2` | قدیمی‌ترین |
| `3` | نام |
| `4` | بیشترین بازدید |
| `5` | کمترین بازدید |
| `6` | تاریخ انتشار نزولی |
| `7` | تاریخ انتشار صعودی |

بک‌اند در این endpoint به‌صورت اجباری فقط پست‌هایی را برمی‌گرداند که شرایط زیر را داشته باشند:

- `active = true`
- حذف نشده باشند
- `adminConfirm = true`
- `publishDate` از زمان فعلی گذشته باشد
- پست اصلی باشند و نسخه ویرایشی موقت نباشند

نمونه فیلتر دسته‌بندی:

```http
GET /api/Site/Post?pageIndex=1&pageSize=12&categoryLabels=blog&sortBy=1
```

برای ارسال چند مقدار Array، پارامتر را تکرار کنید:

```http
GET /api/Site/Post?categoryLabels=dogs&categoryLabels=health&isAndCategories=false
```

## 2. دریافت جزئیات پست

```http
GET /api/Site/Post/{id}
```

نمونه:

```http
GET /api/Site/Post/125
```

دریافت موفق جزئیات، `visitCount` پست را یک واحد افزایش می‌دهد.

مهم‌ترین فیلدهای `data`:

| فیلد | کاربرد |
|---|---|
| `id` | شناسه پست |
| `name` | نام/عنوان اصلی ذخیره‌شده |
| `subject` | موضوع یا تیتر نمایشی |
| `summary` | خلاصه |
| `subNews` | زیرتیتر یا خلاصه خبر |
| `description` | متن کامل پست |
| `publishDate` | تاریخ انتشار |
| `visitCount` | تعداد بازدید |
| `commentCount` | تعداد کامنت‌های تأییدشده |
| `pictureId` | شناسه تصویر اصلی |
| `picture` | اطلاعات تصویر اصلی |
| `postPictures` | تصاویر ضمیمه پست |
| `postFiles` | فایل‌های ضمیمه پست |
| `hashtags` | هشتگ‌ها |
| `category` | دسته اصلی |
| `seoTitle` | عنوان SEO |
| `seoH1` | تیتر H1 |
| `seoMinDescription` | توضیح کوتاه SEO |
| `seoDescription` | توضیح SEO |
| `seoPictureAlt` | Alt تصویر اصلی |
| `seoCanonical` | Canonical URL |
| `seoNoIndex` | کنترل meta robots |
| `seoNoFollow` | کنترل meta robots |

اگر `isSuccess=false` باشد پست وجود ندارد، هنوز منتشر نشده یا دسترسی عمومی به آن مجاز نیست؛ صفحه `404` نمایش داده شود.

در وضعیت فعلی endpoint جزئیات پست فقط با `id` کار می‌کند و endpoint مستقلی برای دریافت Post با Slug وجود ندارد. مسیر فرانت می‌تواند SEO-friendly باشد، ولی برای دریافت داده باید `id` پست را در اختیار داشته باشد.

## 3. ساخت URL تصویر و فایل

اطلاعات تصویر معمولاً شامل `baseUrl`، `url`، `guidName` و `extension` است. در DTO فعلی، `url` مسیر کامل نسبی فایل و `baseUrl` مسیر پوشه است:

```ts
const pictureUrl = `${FILE_BASE_URL}${picture.url}`

const mediumPictureUrl =
  `${FILE_BASE_URL}${picture.baseUrl}/${picture.guidName}-md${picture.extension}`
```

تصاویر آپلودشده به WebP تبدیل می‌شوند و نسخه‌های زیر نیز ساخته می‌شوند:

```text
{guidName}-lg.webp
{guidName}-md.webp
{guidName}-sm.webp
```

برای فایل ضمیمه نیز `file.url` مسیر کامل نسبی فایل است:

```ts
const downloadUrl = `${FILE_BASE_URL}${file.url}`
```

از چسباندن دوباره `/` جلوگیری کنید.

پیشنهاد نمایش:

- کارت لیست: نسخه `sm` یا `md`
- صفحه جزئیات: نسخه اصلی یا `lg`
- مقدار `seoPictureAlt` برای `alt` تصویر اصلی
- در نبود تصویر، Placeholder داخلی سایت

## 4. دریافت دسته‌بندی‌ها

```http
GET /api/Category?pageIndex=1&pageSize=100&active=true
GET /api/Category/{id}
GET /api/Category/label/{label}
```

برای فیلتر پست‌ها بهتر است فرانت از `label` دسته استفاده کند، زیرا نام نمایشی دسته ممکن است در پنل تغییر کند.

## 5. دریافت کامنت‌های یک پست

endpoint موجود:

```http
GET /api/EndUser/PostComment?postId={postId}&pageIndex=1&pageSize=20&sortBy=1
```

این GET نیاز به Token ندارد.

فیلدهای مهم هر کامنت:

| فیلد | کاربرد |
|---|---|
| `id` | شناسه کامنت |
| `postId` | شناسه پست |
| `name` | نام مهمان |
| `text` | متن پاک‌سازی‌شده کامنت |
| `createDate` | تاریخ ثبت |
| `answer` | پاسخ مدیر |
| `user` | اطلاعات حداقلی کاربر، در صورت وجود |
| `likeCount` | تعداد Like |
| `disLikeCount` | تعداد Dislike |
| `upOrDownThumb` | وضعیت رأی کاربر؛ در وضعیت فعلی همیشه قابل اتکا نیست |
| `statusId` | شناسه وضعیت بررسی کامنت |

### هشدار فعلی بک‌اند درباره نمایش کامنت

در پیاده‌سازی فعلی، پارامتر `allStatus=false` فقط مرتب‌سازی را تغییر می‌دهد و کامنت‌ها را واقعاً بر اساس وضعیت «تأییدشده» فیلتر نمی‌کند. بنابراین تا زمان اصلاح بک‌اند، فرانت نباید فرض کند تمام آیتم‌های این پاسخ تأییدشده‌اند. اتصال این بخش به سایت Production باید بعد از اصلاح فیلتر وضعیت انجام شود.

## 6. ثبت کامنت

برای کاربر واردشده:

```http
POST /api/EndUser/PostComment
Authorization: Bearer {accessToken}
Content-Type: application/json
```

برای مهمان endpoint عمومی زیر نیز وجود دارد:

```http
POST /api/PostComment
Content-Type: application/json
```

بدنه نمونه مهمان:

```json
{
  "postId": 125,
  "name": "مهراد",
  "text": "مطلب مفیدی بود",
  "rate": null,
  "userId": null
}
```

قواعد:

- `text` اجباری است و در بک‌اند Sanitize می‌شود.
- اگر `userId` خالی باشد، `name` اجباری است.
- اگر `rate` ارسال شود باید بین `1` تا `5` باشد.
- وضعیت اولیه کامنت همیشه `Comment_NotChecked` است.
- `statusId`، `answer`، `likeCount` و `disLikeCount` را هنگام ثبت از سمت فرانت تعیین نکنید.
- بعد از ثبت موفق، کامنت را فوراً در لیست عمومی نمایش ندهید؛ پیام «نظر شما ثبت شد و پس از بررسی نمایش داده می‌شود» نشان داده شود.

### هشدار فعلی هویت کامنت

سرویس فعلی `userId` را مستقیماً از Body می‌پذیرد و آن را از Token جایگزین نمی‌کند. فرانت سایت نباید `userId` کاربر دیگری را ارسال کند. برای Production بهتر است بک‌اند طوری اصلاح شود که در endpoint احراز هویت‌شده، `userId` فقط از Token خوانده شود.

## 7. Like، Dislike و حذف رأی کامنت

برای خود Post در بک‌اند فعلی Like وجود ندارد. endpoint زیر فقط مربوط به Like/Dislike کامنت است و Token کاربر می‌خواهد:

```http
POST /api/EndUser/CommentLike
Authorization: Bearer {accessToken}
Content-Type: application/json
```

Like:

```json
{
  "commentId": 501,
  "isLike": true
}
```

Dislike:

```json
{
  "commentId": 501,
  "isLike": false
}
```

حذف رأی قبلی:

```json
{
  "commentId": 501,
  "isLike": null
}
```

برای هر کاربر و کامنت فقط یک رأی نگهداری می‌شود. ارسال مجدد Like یا Dislike، رأی قبلی همان کاربر را تغییر می‌دهد. بعد از پاسخ موفق، لیست کامنت‌ها دوباره دریافت یا شمارنده‌ها در UI به‌صورت کنترل‌شده به‌روزرسانی شوند.

در صورت نداشتن Token، کاربر به Login هدایت شود. در وضعیت فعلی endpoint مستقلی برای دریافت رأی فعلی کاربر روی تمام کامنت‌ها وجود ندارد و `upOrDownThumb` نیز در جستجوی PostComment قابل اتکای کامل نیست.

## 8. جریان پیشنهادی صفحه پست

1. جزئیات Post با `GET /api/Site/Post/{id}` دریافت شود.
2. Meta و Canonical از فیلدهای SEO ساخته شود.
3. تصویر اصلی، تصاویر ضمیمه و فایل‌ها نمایش داده شوند.
4. پست‌های مرتبط با `categoryLabels` و `notId` دریافت شوند.
5. کامنت‌ها بعد از رفع فیلتر وضعیت از endpoint کامنت دریافت شوند.
6. ثبت کامنت در حالت Pending انجام شود.
7. Like/Dislike کامنت فقط برای کاربر واردشده فعال باشد.

## 9. وضعیت‌های خطا

| وضعیت | رفتار فرانت |
|---|---|
| HTTP `401` | Token نامعتبر؛ Refresh یا Login |
| HTTP `403` | کاربر مجوز عملیات را ندارد |
| HTTP `404` | صفحه پست پیدا نشد |
| HTTP `200` و `isSuccess=false` | پیام‌های `messages` نمایش داده شوند |
| خطای شبکه | Retry محدود و پیام قابل فهم |

## 10. مواردی که API فعلی ندارد

- Like مستقیم خود Post
- Bookmark/Save Post
- Share Counter
- دریافت Post با Slug
- endpoint قابل اتکای «رأی فعلی من» برای کامنت‌ها
- فیلتر امن و قطعی کامنت تأییدشده در endpoint عمومی فعلی

فرانت نباید برای این موارد API فرضی صدا بزند.
