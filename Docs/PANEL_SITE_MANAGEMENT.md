# مستند اتصال Panel به مدیریت سایت Pastil

## هدف

مدیریت `pastil.pet` داخل همان پروژه Panel اصلی انجام می‌شود، اما منوها و Permissionهای آن از مدیریت `app.pastil.pet` جدا هستند. احراز هویت، User، Role و JWT مشترک‌اند و ادمین اصلی دسترسی ادمین تولید محتوا را از بخش Role/Permission فعلی کنترل می‌کند.

## ورود ادمین سایت

### ورود مستقیم از صفحه Site Login

```http
POST /api/Account/signin
Content-Type: application/json
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

نکات:

- مقدار `isSiteAdmin` باید `true` باشد.
- نقش `Customer` اجازه ورود به پنل سایت ندارد.
- کاربر باید Permission مربوط به `SiteDashboard` را داشته باشد.
- نقش اصلی `Admin` بدون نیاز به تخصیص دستی Permission وارد می‌شود.
- پاسخ موفق همان ساختار Token و Refresh Token ورود فعلی پنل را دارد.

### ورود از پنل اصلی

اگر ادمین از قبل در پنل اصلی Login است، نیازی به ورود مجدد نیست. قبل از انتقال به Layout سایت این endpoint بررسی شود:

```http
GET /api/Admin/SiteDashboard
Authorization: Bearer {token}
```

- پاسخ موفق: انتقال به `/site-admin`.
- پاسخ `401`: Token نامعتبر یا منقضی است.
- پاسخ `403`: کاربر Permission ورود به مدیریت سایت ندارد.

## ساختار پیشنهادی Routeهای Panel

```text
/site-login
/site-admin
/site-admin/posts
/site-admin/galleries
/site-admin/banners
/site-admin/companions
/site-admin/assistances
/site-admin/pansions
/site-admin/stores
```

Layout و Sidebar مدیریت سایت از Layout مدیریت App جدا باشند، ولی Auth Store و Refresh Token فعلی قابل استفاده است.

## خروج از پنل سایت

دکمه «خروج» در Layout مدیریت سایت باید Session احراز هویت را پاک کند و کاربر را به Login مخصوص همان پنل هدایت کند:

```text
/site-login
```

جریان خروج:

1. `accessToken` پاک شود.
2. `refreshToken` پاک شود.
3. اطلاعات User و Permissionهای Cacheشده از Auth Store حذف شوند.
4. Cookie یا Storage مربوط به Remember Me پاک شود.
5. با `router.replace('/site-login')` انتقال انجام شود تا Back مرورگر دوباره صفحه محافظت‌شده را نمایش ندهد.

نمونه:

```ts
async function logoutSiteAdmin() {
  authStore.clearSession()
  await router.replace('/site-login')
}
```

Route Guard تمام مسیرهای `/site-admin/**` نیز در صورت نبود Token معتبر باید به `/site-login` هدایت کند، نه صفحه Login پنل اصلی.

همین رفتار برای پاسخ `401` APIهای بخش Site Admin اجرا شود:

```ts
if (error.response?.status === 401) {
  authStore.clearSession()
  await router.replace('/site-login')
}
```

پاسخ `403` به معنی نداشتن Permission است و نباید Logout انجام شود؛ در این حالت صفحه عدم دسترسی نمایش داده شود.

## دریافت منوها و Permissionها

```http
GET /api/EndUser/PermissionMenu
Authorization: Bearer {token}
```

در پاسخ، Parent زیر را پیدا کنید:

```json
{
  "label": "SiteManagement",
  "name": "مدیریت سایت"
}
```

فقط `children` همین Parent در Sidebar مدیریت سایت نمایش داده شوند.

Controllerهای منوی سایت:

| Controller | کاربرد |
|---|---|
| `SiteDashboard` | اجازه ورود به پنل سایت |
| `SitePost` | مدیریت نوشته‌ها |
| `SiteGallery` | مدیریت گالری‌ها |
| `SiteBanner` | مدیریت بنرها |
| `SiteCompanion` | مدیریت نمایندگان |
| `SiteAssistance` | مدیریت خدمات |
| `SitePansion` | مدیریت پانسیون‌ها |
| `SiteStore` | مدیریت فروشگاه‌ها |

نمایش هر منو و فعال‌بودن عملیات ثبت، ویرایش و حذف باید از Permissionهای برگشتی کنترل شود. کنترل اصلی امنیت در بک نیز انجام می‌شود.

## همگام‌سازی Permissionها

پس از انتشار بک، ادمین اصلی یک مرتبه endpoint زیر را اجرا کند:

```http
POST /api/Admin/PermissionSync
Authorization: Bearer {main-admin-token}
```

این endpoint فقط برای Role اصلی `Admin` مجاز است و گروه `SiteManagement` و تمام عملیات Controllerهای سایت را وارد یا به‌روزرسانی می‌کند.

تخصیص Permission به Role از API و صفحه فعلی RolePermission انجام می‌شود:

```http
GET  /api/Admin/RolePermission?roleId={roleId}
POST /api/Admin/RolePermission
```

برای اجازه ورود ادمین تولید محتوا، حداقل Parent با Label=`SiteManagement` و Permission مربوط به Controller=`SiteDashboard` به Role او اختصاص داده شود. سپس Permission هر بخش به‌صورت جدا داده شود.

## فیلدهای جدید

### Companion

```json
{
  "showToSite": true
}
```

### Pansion

```json
{
  "showToSite": true
}
```

### Store

```json
{
  "showToSite": true
}
```

### Assistance

```json
{
  "showToSite": true
}
```

### Banner

```json
{
  "showToApp": true,
  "showToSite": false
}
```

برای Banner دو Switch مستقل نمایش داده شود:

- نمایش در App → `showToApp`
- نمایش در سایت → `showToSite`

برای Companion، Pansion، Store و Assistance یک Switch با عنوان «نمایش در سایت» به فیلد `showToSite` متصل شود.

## APIهای مدیریت سایت

تمام درخواست‌ها به Bearer Token نیاز دارند.

### Banner

```text
GET    /api/Admin/SiteBanner
GET    /api/Admin/SiteBanner/{id}
POST   /api/Admin/SiteBanner
PUT    /api/Admin/SiteBanner
DELETE /api/Admin/SiteBanner?id={id}
```

### Companion

```text
GET    /api/Admin/SiteCompanion
GET    /api/Admin/SiteCompanion/{id}
POST   /api/Admin/SiteCompanion
PUT    /api/Admin/SiteCompanion
DELETE /api/Admin/SiteCompanion?id={id}
```

فیلتر اختیاری لیست:

```text
showToSite=true|false
```

### Assistance

```text
GET    /api/Admin/SiteAssistance
GET    /api/Admin/SiteAssistance/{id}
POST   /api/Admin/SiteAssistance
PUT    /api/Admin/SiteAssistance
DELETE /api/Admin/SiteAssistance?id={id}
```

### Pansion

```text
GET  /api/Admin/SitePansion
GET  /api/Admin/SitePansion/{id}
POST /api/Admin/SitePansion
PUT  /api/Admin/SitePansion
```

### Store

```text
GET    /api/Admin/SiteStore
GET    /api/Admin/SiteStore/{id}
POST   /api/Admin/SiteStore
PUT    /api/Admin/SiteStore
DELETE /api/Admin/SiteStore?id={id}
```

### Post

```text
GET    /api/Admin/SitePost
GET    /api/Admin/SitePost/{id}
POST   /api/Admin/SitePost
PUT    /api/Admin/SitePost
DELETE /api/Admin/SitePost?id={id}
```

### Gallery

```text
GET    /api/Admin/SiteGallery
GET    /api/Admin/SiteGallery/{id}
POST   /api/Admin/SiteGallery
PUT    /api/Admin/SiteGallery
DELETE /api/Admin/SiteGallery?id={id}
```

## نکته مهم ویرایش

Endpointهای `PUT`، DTO کامل همان Entity را دریافت می‌کنند. برای تغییر Switch نمایش:

1. ابتدا `GET /{id}` اجرا شود.
2. مقدار `showToSite` یا فیلد Banner تغییر کند.
3. همان DTO کامل با `PUT` ارسال شود.

ارسال DTO ناقص می‌تواند سایر فیلدها را خالی کند.

## رفتار نمایش سایت

فعال‌کردن `showToSite` به‌تنهایی کافی نیست. بک به‌صورت اجباری شرایط زیر را کنترل می‌کند:

| بخش | شرایط نمایش عمومی |
|---|---|
| Companion | `ShowToSite && Active && Approved && !Deleted` |
| Pansion | `ShowToSite && Active && Approve` |
| Store | `ShowToSite && Active && !Deleted` |
| Assistance | `ShowToSite && Active && !Deleted` |
| Banner | `ShowToSite && Active && !Deleted` |
| Post | شرایط انتشار و Active فعلی Post |
| Gallery | Active و حذف‌نشده طبق سرویس فعلی Gallery |

## مدیریت خطاها در Panel

- `401`: Token منقضی یا نامعتبر؛ Refresh یا انتقال به Login.
- `403`: Permission عملیات وجود ندارد؛ Toast «دسترسی کافی ندارید» و بازخوانی منوها.
- `isSuccess=false`: پیام‌های `messages` نمایش داده شوند.

## ترتیب پیاده‌سازی Panel

1. اضافه‌کردن `isSiteAdmin` به مدل Login.
2. ساخت `/site-login` و Route Guard آن.
3. ساخت Layout مستقل `/site-admin`.
4. خواندن Parent با Label=`SiteManagement` از PermissionMenu.
5. ساخت Sidebar براساس Controller Permissionها.
6. افزودن فیلدهای visibility به Typeها و Formها.
7. ساخت صفحات CRUD با endpointهای `Site*`.
8. افزودن دکمه «ورود به مدیریت سایت» در پنل اصلی.
9. کنترل `401/403` و Refresh Token.
