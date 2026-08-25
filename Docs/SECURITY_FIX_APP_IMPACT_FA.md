# فیکس‌های امنیتی که روی وب‌اپ اثر می‌ذارن — ۲۰۲۶/۰۸/۲۵

این سند فقط اون بخش از فیکس‌های امنیتیِ بک‌اند رو توضیح می‌ده که **روی رفتار وب‌اپ اثر می‌ذاره**. بقیه‌ی فیکس‌ها (SQL Injection، لغو سفر، آدرس) کاملاً داخلی بودن و هیچ تغییری توی قرارداد API یا رفتار فرانت نمی‌خوان.

## چه چیزی عوض شد و چرا

دو تا از Endpointهای سرویس File (`api.pastil.pet` نه، بلکه **`file.pastil.pet`**) قبلاً کاملاً بدون احراز هویت بودن — هرکسی بدون توکن می‌تونست فایل آپلود کنه، حتی فایل با پسوند دلخواه (`.html`, `.exe`, ...) بدون هیچ چک محتوایی. این یه حفره‌ی امنیتیِ بحرانی بود که همین امروز بسته شد:

| Endpoint | قبل | الان |
|---|---|---|
| `POST https://file.pastil.pet/api/FileUpload` | بدون احراز هویت، بدون چک نوع فایل | **نیاز به توکن JWT معتبر** + فقط پسوندهای `.jpg .jpeg .png .webp .gif .mp4 .webm .mov .pdf` |
| `POST https://file.pastil.pet/api/PictureUpload` | بدون احراز هویت | **نیاز به توکن JWT معتبر** (چک نوع فایل از قبل هم بود، دست‌نخورده موند) |

توکن JWT همون توکنیه که از لاگین/`refreshtoken` سرویس Api می‌گیرید — سرویس File هم از همون `JWtConfig` (Key/Issuer/Audience) استفاده می‌کنه، پس یه توکن معتبر روی Api، دقیقاً همون‌جوری روی File هم معتبره؛ نیازی به گرفتن توکن جدا نیست.

## چرا وب‌اپ الان می‌شکنه

`webapp/app/components/global/UploadImage.vue` (تابع `executeUpload`) مستقیماً از مرورگر با `$fetch` به `https://file.pastil.pet/api/FileUpload` و `.../PictureUpload` درخواست می‌زنه — **بدون هیچ هدر Authorization**. با این تغییر، این درخواست‌ها از الان `401` می‌گیرن و آپلود (چه توی صفحه‌ی پروفایل، چه ضمیمه‌ی تیکت پشتیبانی) کار نمی‌کنه.

## چرا نمی‌شه فقط یه هدر توی همون کامپوننت اضافه کرد

توکن لاگین وب‌اپ توی یه Cookie به اسم `token` نگه‌داری می‌شه که فقط سمت سرور (Nitro server routes، مثل الگوی موجود توی `webapp/app/server/api/cart/cart.post.js`) خونده می‌شه — نه مستقیم توی کد کلاینت/مرورگر. تمام Endpointهای دیگه‌ی وب‌اپ (Cart، Wallet، Payment، ...) از همین الگو پیروی می‌کنن: یه Server Route محلی (BFF) داخل `webapp/app/server/api/`، توکن رو از Cookie می‌خونه و به بک‌اند واقعی Forward می‌کنه. آپلود فایل تنها استثنا بود که مستقیم از مرورگر به `file.pastil.pet` می‌زد — و دقیقاً به همین خاطر بدون احراز هویت هم مونده بود.

## راه‌حل پیشنهادی — هم‌راستا با الگوی خودِ پروژه

### ۱. دو تا Server Route جدید بساز

```ts
// webapp/app/server/api/upload/file.post.ts
export default defineEventHandler(async (event) => {
  const token = getCookie(event, 'token')

  const parts = await readMultipartFormData(event)
  const filePart = parts?.find((p) => p.name === 'file')

  if (!filePart) {
    throw createError({ statusCode: 400, statusMessage: 'فایلی ارسال نشده است.' })
  }

  const formData = new FormData()
  formData.append(
    'file',
    new Blob([filePart.data], { type: filePart.type }),
    filePart.filename || 'upload'
  )

  return await $fetch('https://file.pastil.pet/api/FileUpload', {
    method: 'POST',
    headers: token ? { Authorization: `Bearer ${token}` } : {},
    body: formData
  })
})
```

```ts
// webapp/app/server/api/upload/picture.post.ts
export default defineEventHandler(async (event) => {
  const token = getCookie(event, 'token')

  const parts = await readMultipartFormData(event)
  const filePart = parts?.find((p) => p.name === 'PictureFile')

  if (!filePart) {
    throw createError({ statusCode: 400, statusMessage: 'فایلی ارسال نشده است.' })
  }

  const formData = new FormData()
  formData.append(
    'PictureFile',
    new Blob([filePart.data], { type: filePart.type }),
    filePart.filename || 'upload'
  )

  return await $fetch('https://file.pastil.pet/api/PictureUpload', {
    method: 'POST',
    headers: token ? { Authorization: `Bearer ${token}` } : {},
    body: formData
  })
})
```

### ۲. `UploadImage.vue` رو به این Route های محلی وصل کن

توی `executeUpload` (خط ۲۲۶-۲۲۸ فعلی)، به‌جای زدن مستقیم به `file.pastil.pet`:

```js
// قبل
const targetUrl = props.isTicket
  ? "https://file.pastil.pet/api/FileUpload"
  : (imageUploaderUrl || "https://file.pastil.pet/api/PictureUpload");

// بعد
const targetUrl = props.isTicket
  ? "/api/upload/file"
  : "/api/upload/picture";
```

فیلد فرم (`fieldName`) و بقیه‌ی منطق کامپوننت دست‌نخورده می‌مونه — همون `file`/`PictureFile` که الان هست.

### ۳. نوع فایل‌های مجاز رو با بک‌اند هماهنگ کن

برای `FileUpload` (تیکت‌ها)، الان فقط این پسوندها قبول می‌شن: `.jpg .jpeg .png .webp .gif .mp4 .webm .mov .pdf`. اگه جایی توی وب‌اپ نوع دیگه‌ای (مثلاً `.doc`) رو تبلیغ/قبول می‌کنید که واقعاً لازمه، بگید تا به Allowlist بک‌اند هم اضافه کنم — فعلاً چیزی که همین الان از سمت کلاینت با `accept="image/*,video/*,.pdf"` محدود شده رو دقیقاً matching کردم.

## خلاصه‌ی اقدام لازم

- [ ] دو فایل Server Route بالا رو اضافه کن.
- [ ] `UploadImage.vue` رو به مسیرهای محلی وصل کن.
- [ ] تست کن: آپلود عکس پروفایل، آپلود ضمیمه‌ی تیکت پشتیبانی.
- [ ] اگه panel هم از همین الگو استفاده می‌کرد نگران نباش — `panel/composables/useFiling.ts` همین امروز به همین شکل (هدر Authorization از سشن پنل) فیکس شد، نیازی به کار روش نیست.

هر سوالی بود یا نوع فایل جدیدی لازم شد که Allowlist بک‌اند اجازه نمی‌ده، بگو تا هماهنگ کنیم.
