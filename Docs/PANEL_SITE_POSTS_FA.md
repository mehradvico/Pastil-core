# مستند مدیریت Post در Panel سایت Pastil

این مستند برای پیاده‌سازی صفحات مدیریت پست، رسانه‌های پست، دسته‌بندی، هشتگ، کامنت و پاسخ مدیر در Layout مدیریت سایت است.

## 1. احراز هویت و Permission

ورود مستقیم ادمین سایت:

```http
POST /api/Account/signin
```

```json
{
  "mobile": "09xxxxxxxxx",
  "password": "password",
  "code": "",
  "cartCode": "",
  "isAdmin": false,
  "isSiteAdmin": true,
  "rememberMe": true
}
```

تمام endpointهای مدیریتی باید Header زیر را داشته باشند:

```http
Authorization: Bearer {accessToken}
```

Permissionهای مرتبط در وضعیت فعلی بک‌اند:

| بخش | Controller Permission | Parent فعلی |
|---|---|---|
| ورود مدیریت سایت | `SiteDashboard` | `SiteManagement` |
| مدیریت پست سایت | `SitePost` | `SiteManagement` |
| مدیریت کامنت پست | `PostComment` | گروه قدیمی مدیریت محتوا |
| مدیریت مستقل تصاویر پست | `PostPicture` | گروه قدیمی مدیریت محتوا |
| مدیریت مستقل فایل‌های پست | `PostFile` | گروه قدیمی مدیریت محتوا |
| دسته‌بندی | `Category` | Permission عمومی مدیریت دسته‌بندی |

برای ادمین تولید محتوا حداقل `SiteDashboard` و `SitePost` تخصیص داده شود. اگر صفحه مدیریت کامنت نیز نمایش داده می‌شود، Permission مربوط به `PostComment` هم باید از پنل اصلی به Role او داده شود.

نکته: در وضعیت فعلی `PostComment` زیر Parent `SiteManagement` نیست؛ بنابراین Sidebar سایت باید آن را جداگانه از مجموعه Permissionهای کاربر تشخیص دهد یا تا زمان انتقال Permission در بک‌اند، دسترسی آن جدا تخصیص داده شود.

## 2. Routeهای پیشنهادی Panel

```text
/site-admin/posts
/site-admin/posts/new
/site-admin/posts/:id/edit
/site-admin/posts/:id/comments
```

صفحه‌ها و دکمه‌ها فقط در صورت داشتن Permission متناظر نمایش داده شوند. مخفی‌کردن منو جایگزین کنترل دسترسی بک‌اند نیست.

## 3. دریافت لیست پست‌ها

```http
GET /api/Admin/SitePost
Authorization: Bearer {token}
```

نمونه:

```http
GET /api/Admin/SitePost?pageIndex=1&pageSize=20&sortBy=1&allAdminConfirm=true
```

فیلترها:

| پارامتر | نوع | توضیح |
|---|---:|---|
| `pageIndex` | number | شماره صفحه |
| `pageSize` | number | تعداد در صفحه |
| `q` | string | جستجو در `name` |
| `sortBy` | number | مرتب‌سازی |
| `active` | boolean | فیلد موجود در قرارداد؛ محدودیت فعلی آن پایین توضیح داده شده است |
| `publish` | boolean | گذشته یا آینده بودن تاریخ انتشار |
| `adminConfirm` | boolean | وضعیت تأیید |
| `allAdminConfirm` | boolean | اگر `false` باشد فیلتر تأیید اعمال می‌شود |
| `edited` | boolean | نسخه ویرایشی |
| `categoryIds` | number[] | فیلتر دسته‌ها |
| `categoryLabels` | string[] | فیلتر Label دسته‌ها |
| `hashtags` | string | هشتگ‌ها با جداکننده `-` |

ستون‌های پیشنهادی جدول:

- عنوان `name` یا `subject`
- تصویر اصلی
- دسته اصلی
- نویسنده
- `active`
- `adminConfirm`
- `publishDate`
- `visitCount`
- `commentCount`
- عملیات ویرایش و حذف

محدودیت فعلی: Search سرویس قبل از اعمال فیلترها، `active=true` را اجباری می‌کند. در نتیجه پست غیرفعال در لیست مدیریت دیده نمی‌شود و ارسال `active=false` نیز نتیجه‌ای برنمی‌گرداند. برای مدیریت Draft/Inactive این بخش نیازمند اصلاح بک‌اند است.

## 4. دریافت پست برای ویرایش

```http
GET /api/Admin/SitePost/{id}
Authorization: Bearer {token}
```

پاسخ از نوع `PostDto` است و شامل `categoryIds`، `hashTagList` و `postFilesList` می‌شود.

در پیاده‌سازی فعلی `postPicturesList` هنگام دریافت Post برای ویرایش Load نمی‌شود. برای دریافت تصاویر فعلی باید Permission `PostPicture` وجود داشته باشد و endpoint زیر جداگانه خوانده شود:

```http
GET /api/Admin/PostPicture?postId={id}&pageIndex=1&pageSize=100
Authorization: Bearer {token}
```

برای Preview تصویر اصلی یا ضمیمه با شناسه نیز می‌توان استفاده کرد:

```http
GET /api/Common/Picture/{pictureId}
```

برای ویرایش همیشه ابتدا همین endpoint خوانده شود، سپس همان DTO کامل با تغییرات کاربر ارسال شود. ارسال Partial Object ممکن است فیلدها یا رابطه‌های قبلی را پاک کند.

## 5. ساخت پست

```http
POST /api/Admin/SitePost
Authorization: Bearer {token}
Content-Type: application/json
```

نمونه Body:

```json
{
  "name": "راهنمای نگهداری از سگ در تابستان",
  "subject": "نگهداری از سگ در فصل گرم",
  "summary": "نکات کوتاه و کاربردی برای فصل تابستان",
  "subNews": "آب کافی، زمان پیاده‌روی و مراقبت از پنجه‌ها",
  "description": "<p>متن کامل مقاله...</p>",
  "pictureId": 108,
  "pictureUrl": "",
  "active": true,
  "publishDate": "2026-08-10T09:00:00+03:30",
  "adminConfirm": true,
  "categoryId": 18,
  "categoryIds": [18, 2],
  "hashTagList": ["سگ", "سلامت", "تابستان"],
  "postPicturesList": [
    {
      "pictureId": 109,
      "name": "تصویر داخل مقاله",
      "label": "summer-dog"
    }
  ],
  "postFilesList": [
    {
      "fileId": 44,
      "name": "فایل راهنما",
      "label": "summer-guide"
    }
  ],
  "seoH1": "راهنمای نگهداری از سگ در تابستان",
  "seoTitle": "نگهداری از سگ در تابستان | پاستیل",
  "seoMinDescription": "راهنمای مراقبت از سگ در روزهای گرم",
  "seoDescription": "نکات کامل نگهداری و مراقبت از سگ در فصل تابستان",
  "seoPictureAlt": "سگ در فصل تابستان",
  "seoUrlText": "",
  "seoCanonical": "https://pastil.pet/blog/125",
  "seoNoIndex": false,
  "seoNoFollow": false,
  "isOld": false
}
```

نکات:

- `id` برای ساخت ارسال نشود یا `0` باشد.
- `userId` را برای تعیین نویسنده نفرستید؛ بک‌اند در ساخت، کاربر جاری را از Token ثبت می‌کند.
- `publishDate` اجباری است. اگر تاریخ گذشته ارسال شود، بک‌اند آن را زمان فعلی قرار می‌دهد.
- `name` و `active` الزامی هستند.
- `categoryId` دسته اصلی است.
- `categoryIds` دسته‌های تکمیلی‌اند؛ والدهای `categoryId` نیز بک‌اند اضافه می‌کند.
- آرایه‌های هشتگ و دسته در بک‌اند یکتا می‌شوند.
- تصویر اصلی با `pictureId` تعیین می‌شود.
- تصاویر و فایل‌های ضمیمه با دو آرایه انتهای DTO ثبت می‌شوند.

## 6. ویرایش پست

```http
PUT /api/Admin/SitePost
Authorization: Bearer {token}
Content-Type: application/json
```

Body باید DTO کامل دریافتی از `GET /api/Admin/SitePost/{id}` باشد و `id` معتبر داشته باشد.

در زمان ویرایش:

- `categoryIds` کامل ارسال شود.
- `hashTagList` کامل ارسال شود.
- `postPicturesList` قبل از ارسال از endpoint تصاویر جداگانه تکمیل شود و `null` نباشد.
- `postFilesList` کامل ارسال شود.
- حذف فایل ضمیمه با حذف آن از `postFilesList` و ارسال مجدد DTO کامل انجام می‌شود.

حذف تصویر ضمیمه در وضعیت فعلی با حذف آن از Array تضمین نمی‌شود، چون Navigation تصاویر در Update Load نشده است. برای حذف قطعی تصویر از endpoint زیر و شناسه رابطه `PostPicture.id` استفاده شود:

```http
DELETE /api/Admin/PostPicture?id={postPictureId}
Authorization: Bearer {token}
```

بک‌اند برای بعضی Roleها و پست‌های تأییدشده، به‌جای ویرایش مستقیم یک نسخه ویرایشی Child می‌سازد تا مدیر آن را تأیید کند. پنل باید پاسخ `data.id` را بررسی کند؛ ممکن است شناسه نسخه ایجادشده با شناسه پست اصلی متفاوت باشد.

## 7. حذف پست

```http
DELETE /api/Admin/SitePost?id={id}
Authorization: Bearer {token}
```

حذف Post به‌صورت Soft Delete انجام می‌شود و نسخه‌های ویرایشی فرزند آن نیز حذف منطقی می‌شوند.

قبل از حذف Dialog تأیید نمایش داده شود و فقط در صورت `isSuccess=true` ردیف از جدول حذف شود.

## 8. آپلود تصویر اصلی و تصاویر داخل پست

آپلود روی سرویس File انجام می‌شود:

```http
POST https://file.pastil.pet/api/PictureUpload
Content-Type: multipart/form-data
```

نام دقیق فیلد فرم:

```text
PictureFile
```

محدودیت‌ها:

- حداکثر حجم `5 MB`
- تصویر: `.jpg`، `.jpeg`، `.png`، `.webp`
- ویدیو: `.mp4`، `.webm`، `.ogg`
- تصاویر به WebP تبدیل می‌شوند.
- حداکثر ابعاد `8000 × 8000`
- حداکثر `25,000,000` پیکسل

پس از آپلود، مقدار `data.id` به‌عنوان `pictureId` نگهداری شود.

در پاسخ تصویر، `data.url` مسیر کامل نسبی فایل است. URL نمایش:

```ts
const src = `${FILE_BASE_URL}${response.data.url}`
```

تصویر اصلی:

```json
{
  "pictureId": 108
}
```

تصویر ضمیمه داخل مقاله:

```json
{
  "pictureId": 109,
  "name": "تصویر داخل متن",
  "label": "article-section-one"
}
```

مقدار `label` رسانه فقط انگلیسی کوچک، عدد و `-` باشد؛ پنل هنگام تایپ مقدار را Normalize کند.

## 9. آپلود فایل ضمیمه

```http
POST https://file.pastil.pet/api/FileUpload
Content-Type: multipart/form-data
```

نام دقیق فیلد:

```text
file
```

حداکثر حجم فایل `20 MB` است. پس از موفقیت، `data.id` به‌عنوان `fileId` در `postFilesList` استفاده شود:

```json
{
  "fileId": 44,
  "name": "دانلود راهنما",
  "label": "article-guide"
}
```

در پاسخ فایل، `data.url` مسیر کامل نسبی فایل است و آدرس دانلود از ترکیب `FILE_BASE_URL + data.url` ساخته می‌شود.

## 10. مدیریت دسته و هشتگ

برای انتخاب دسته:

```http
GET /api/Admin/Category?pageIndex=1&pageSize=100&active=true
Authorization: Bearer {token}
```

اگر Role سایت Permission مدیریت Category ندارد، برای Dropdown فقط‌خواندنی می‌توان از endpoint عمومی استفاده کرد:

```http
GET /api/Category?pageIndex=1&pageSize=100&active=true
```

در فرم:

- یک دسته به‌عنوان `categoryId` انتخاب شود.
- Multi Select اختیاری برای `categoryIds` قرار گیرد.
- هشتگ‌ها به‌صورت `string[]` در `hashTagList` ارسال شوند.

## 11. لیست و مدیریت کامنت‌ها

Permission لازم: `PostComment`.

لیست کامنت‌های یک پست:

```http
GET /api/Admin/PostComment?postId={postId}&pageIndex=1&pageSize=20&sortBy=1
Authorization: Bearer {token}
```

جزئیات یک کامنت:

```http
GET /api/Admin/PostComment/{id}
Authorization: Bearer {token}
```

فیلدهای قابل نمایش:

- نام مهمان یا اطلاعات `user`
- متن کامنت
- تاریخ ثبت
- وضعیت
- پاسخ مدیر
- `likeCount`
- `disLikeCount`
- نام پست

## 12. تأیید، رد و پاسخ به کامنت

```http
PUT /api/Admin/PostComment
Authorization: Bearer {token}
Content-Type: application/json
```

بک‌اند در ویرایش Comment فقط `id`، `statusId` و `answer` را اعمال می‌کند:

```json
{
  "id": 501,
  "statusId": 2,
  "answer": "ممنون از همراهی شما"
}
```

Label وضعیت‌ها:

| Label | مفهوم |
|---|---|
| `Comment_NotChecked` | بررسی‌نشده |
| `Comment_Accept` | تأییدشده |
| `Comment_Reject` | ردشده |

شناسه Code را Hard-code نکنید. ابتدا CodeGroup/Codeهای سیستم را بخوانید و Code را با `label` پیدا کنید؛ سپس `id` واقعی آن را در `statusId` ارسال کنید.

بعد از تغییر وضعیت، بک‌اند `commentCount` پست را بر اساس کامنت‌های دارای Label برابر `Comment_Accept` دوباره محاسبه می‌کند.

در API فعلی حذف Comment و ایجاد Comment از پنل ادمین وجود ندارد؛ پنل فقط وضعیت و پاسخ را مدیریت می‌کند.

## 13. Like و Dislike در Panel

Like/Dislike فقط برای کاربر سایت و روی Comment است:

```http
POST /api/EndUser/CommentLike
```

پنل مدیریتی endpoint جداگانه‌ای برای ایجاد، حذف یا دستکاری Like ندارد. در پنل فقط شمارنده‌های `likeCount` و `disLikeCount` نمایش داده شوند.

برای خود Post نیز در بک‌اند فعلی سیستم Like وجود ندارد؛ در پنل دکمه یا ستون Like پست ساخته نشود.

## 14. وضعیت انتشار Post

یک پست فقط زمانی در سایت دیده می‌شود که:

```text
active = true
adminConfirm = true
publishDate < now
deleted = false
```

کنترلر `SitePost` در وضعیت فعلی endpoint جداگانه `confirm` ندارد. endpoint قدیمی تأیید در مسیر زیر قرار دارد و Permission کنترلر قدیمی `Post` را می‌خواهد:

```http
PUT /api/Admin/Post/confirm
```

```json
{
  "id": 125,
  "adminConfirm": true,
  "publishDate": "2026-08-10T09:00:00+03:30"
}
```

اگر نمی‌خواهید به ادمین سایت Permission قدیمی `Post` داده شود، در فرم `SitePost` وضعیت انتشار را در همان DTO کامل ایجاد/ویرایش مدیریت کنید. برای Workflow تأیید مستقل SitePost، بک‌اند به endpoint و Permission جداگانه نیاز دارد.

## 15. اعتبارسنجی پیشنهادی فرم

- `name`: اجباری
- `subject`: پیشنهاد می‌شود اجباری باشد
- `description`: پیشنهاد می‌شود اجباری باشد
- `publishDate`: اجباری و تاریخ معتبر
- `pictureId`: برای مقاله سایت پیشنهاد می‌شود اجباری باشد
- `categoryId`: برای ساختار سایت پیشنهاد می‌شود اجباری باشد
- `seoTitle`: طول پیشنهادی حداکثر 60 کاراکتر
- `seoMinDescription`: طول پیشنهادی 150 تا 160 کاراکتر
- Label ضمیمه‌ها: فقط `[a-z0-9-]`
- Arrayهای رابطه‌ای همیشه با مقدار `[]` ارسال شوند، نه `null`

## 16. نکات مهم پیاده‌سازی

- برای نتیجه عملیات همیشه `isSuccess` کنترل شود.
- پیام خطا از `messages` استخراج شود.
- فرم ویرایش با DTO کامل دریافتی پر شود.
- بعد از Create/Update/Delete، Cache لیست Post در پنل Refresh شود.
- بعد از تأیید یا رد Comment، هم لیست Comment و هم `commentCount` پست Refresh شود.
- `userId` سازنده Post را پنل تعیین نکند.
- HTML متن مقاله قبل از نمایش در Panel Preview به شکل امن Render شود.
- تاریخ‌ها با timezone صحیح نمایش داده شوند و برای API با ISO 8601 ارسال شوند.

## 17. محدودیت‌ها و موارد نیازمند اصلاح بک‌اند

موارد زیر وضعیت واقعی API فعلی هستند:

1. کامنت عمومی عملاً بر اساس وضعیت تأیید فیلتر قطعی نمی‌شود.
2. `userId` کامنت احراز هویت‌شده از Body پذیرفته می‌شود و باید در بک‌اند از Token گرفته شود.
3. Permission کامنت سایت هنوز `PostComment` قدیمی است و زیر `SiteManagement` قرار ندارد.
4. `SitePost` endpoint جداگانه Confirm/Reject ندارد.
5. برای خود Post سیستم Like وجود ندارد.
6. endpoint دریافت Post با Slug وجود ندارد.
7. وضعیت رأی فعلی کاربر روی Comment در لیست عمومی قابل اتکای کامل نیست.
8. Search مدیریتی Post در وضعیت فعلی همیشه فقط پست‌های Active را برمی‌گرداند.
9. تصاویر فعلی Post در `GET SitePost/{id}` Load نمی‌شوند و حذف تصویر از Array به‌تنهایی کافی نیست.

تا زمان اصلاح موارد امنیتی 1 و 2، بخش عمومی Comment نباید بدون کنترل اضافه وارد Production شود.
