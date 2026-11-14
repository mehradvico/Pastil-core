# مستند ورود با شماره و OTP — پنل نماینده / مرکز

**مخاطب:** فرانت‌اند پنل نماینده (Companion)  
**دامنه:** فقط ورود، نگه‌داری نشست، خروج و تشخیص وضعیت دسترسی پنل نماینده  
**Base URL نمونه:** `https://api.pastil.pet`  
**نسخهٔ مبنا:** استخراج‌شده از کد فعلی بک‌اند

---

## نتیجهٔ اصلی

ورود نماینده با کد یک‌بارمصرف در چهار درخواست انجام می‌شود:

```text
شماره موبایل
  │
  ├─ POST /api/Account/userdetail   ← اختیاری برای UX، ولی توصیه‌شده
  │
  ├─ POST /api/Account/otp          ← ارسال SMS
  │
  ├─ POST /api/Account/CheckOtp     ← تأیید کد؛ شرط لازم برای ورود
  │
  └─ POST /api/Account/signin       ← دریافت access token و refresh token
                                      │
                                      └─ درخواست‌های /api/Companion/* با Bearer token
```

برای **نماینده/مرکز** در درخواست `signin` حتماً این دو مقدار را بفرستید:

```json
{
  "isAdmin": false,
  "isSiteAdmin": false
}
```

`isAdmin: true` ورود را وارد مسیر پنل مدیریت می‌کند؛ بک‌اند آن مسیر را فقط برای نقش Admin یا نقش دارای مجوز ناحیهٔ `Admin` مجاز می‌داند. بنابراین برای یک نمایندهٔ عادی معمولاً با `AccessDenied` رد می‌شود.

> این مستند ثبت‌نام نماینده نیست. اگر شماره حساب ندارد یا مالک/عضو فعال مرکز نیست، OTP به‌تنهایی دسترسی پنل Companion ایجاد نمی‌کند.

---

## پیش‌نیازهای دسترسی پنل نماینده

| مورد | شرط واقعی در بک‌اند | نتیجه در پنل |
|---|---|---|
| حساب کاربری | کاربر با همان شماره وجود داشته و `Deleted=false` باشد | در غیر این صورت `signin` ناموفق است. |
| قفل حساب | `Locked=false` | در غیر این صورت ورود متوقف می‌شود. |
| OTP تک‌عاملی | `TwoFactorEnabled=false` | تأیید صحیح OTP برای ورود کافی است. |
| OTP دو‌عاملی | `TwoFactorEnabled=true` | علاوه بر OTP، رمز عبور نیز لازم است؛ ورود «فقط با OTP» ممکن نیست. |
| مالک مرکز | یک `Companion` با `OwnerId=userId`، `Active=true` و `Approved=true` داشته باشد | `currentUser.companionId` ساخته می‌شود و endpointهای مالک‌محور پنل قابل استفاده‌اند. |
| عضو تیم مرکز | رکورد `CompanionUser` باید `Active=true`، `UserAccept=true`، `Deleted=false` داشته باشد و خود مرکز هم فعال و تأییدشده باشد | عضویت Companion برقرار است (`isCompanionUser=true`)؛ اما بسیاری از endpointهای فعلی که صریحاً به `companionId` نیاز دارند فقط با `companionId` مالک کار می‌کنند. |

### نکتهٔ مهم دربارهٔ «نماینده» و «عضو تیم»

سیاست عضویت ناحیهٔ Companion، مالک مرکز یا عضو تأییدشده را عضو می‌شمارد. با این حال DTO کاربر فعلی، `companionId` را فقط از مرکزهای **تحت مالکیت خود کاربر** استخراج می‌کند. بنابراین موفقیت ورود OTP معادل تضمین دسترسی به همهٔ endpointهای مدیریت مرکز نیست. اگر کاربر عضو تیم است و نه مالک، فرانت باید پاسخ هر endpoint را بررسی کند و نباید `companionId` را از سمت کلاینت حدس بزند.

---

## قرارداد عمومی پاسخ‌ها

اکثر endpointهای این مسیر با wrapper زیر پاسخ می‌دهند:

```ts
type ApiResult<T = unknown> = {
  isSuccess: boolean;
  code: number;
  messages: Array<{
    item1: string; // متن پیام
    item2: string; // نام فیلد مرتبط، در صورت وجود
  }>;
  data?: T;
};
```

`messages` در بک‌اند `List<Tuple<string,string>>` است. در serializer پیش‌فرض به صورت `item1` و `item2` دیده می‌شود. در UI ابتدا `messages[0]?.item1` را نشان دهید و اگر `item2` وجود داشت، خطا را روی همان فیلد قرار دهید. پاسخ HTTP `200` می‌تواند `isSuccess:false` داشته باشد؛ تنها به status HTTP اکتفا نکنید.

مقادیر مهم `code` در استعلام ابتدایی:

| code | معنا |
|---:|---|
| `1` | `SignUp` — حسابی با این شماره وجود ندارد. |
| `2` | `OneFactor` — حساب وجود دارد و ورود OTP تک‌عاملی است. |
| `3` | `TwoFactor` — OTP و رمز عبور هر دو لازم هستند. |
| `4` | `Locked` — حساب قفل است. |

---

## 1) بررسی حساب و نوع ورود

### `POST /api/Account/userdetail`

این endpoint بدون توکن است و فقط برای تصمیم‌گیری صفحهٔ ورود استفاده می‌شود. ارسال آن قبل از OTP توصیه می‌شود تا برای شمارهٔ ناشناخته SMS ارسال نشود و حالت رمز عبور مشخص باشد.

**Rate limit:** policy `AccountLookup`؛ بر اساس IP. مقدار دقیق به تنظیم دیده‌شدن IP بستگی دارد: burst اولیه 20 یا 300 درخواست و سپس 10 یا 150 درخواست در دقیقه.

#### Request body

```json
{
  "mobile": "09123456789"
}
```

| فیلد | نوع | اجباری | توضیح |
|---|---|---:|---|
| `mobile` | `string` | بله | شمارهٔ ایران؛ ارقام فارسی پیش از پردازش به انگلیسی تبدیل می‌شوند. شماره را در UI به فرم استاندارد `09xxxxxxxxx` نرمال کنید. |

#### پاسخ‌های مورد انتظار

حساب تک‌عاملی:

```json
{
  "isSuccess": true,
  "code": 2,
  "messages": []
}
```

حساب دو‌عاملی:

```json
{
  "isSuccess": true,
  "code": 3,
  "messages": []
}
```

شمارهٔ ناشناخته:

```json
{
  "isSuccess": true,
  "code": 1,
  "messages": []
}
```

برای پنل نماینده در حالت `code:1` صفحهٔ ثبت‌نام عمومی را خودکار باز نکنید؛ پیام «این شماره برای ورود به پنل نماینده ثبت نشده است» و مسیر پشتیبانی/درخواست همکاری مناسب است.

---

## 2) ارسال کد OTP

### `POST /api/Account/otp`

بک‌اند نوع پیام را مستقل از body روی OTP قرار می‌دهد و کد را از طریق کانال پیام تنظیم‌شده (برای mobile: پیامک) ارسال می‌کند.

**بدون توکن**  
**Rate limit:** policy `OtpSend`؛ بر اساس IP. burst اولیه 3 (یا 300 در حالت IP غیرقابل‌اتکا)، سپس 1 (یا 150) توکن در هر 2 دقیقه.  
**محدودیت گیرنده در دیتابیس:** پس از بیش از سه درخواست ناموفق/بدون ورود موفق در بازهٔ 10 دقیقه‌ای، ارسال مجدد تا پایان همان بازه رد می‌شود. در UI تایمر ارسال مجدد 120 ثانیه‌ای بگذارید.

#### Request body

```json
{
  "mobile": "09123456789"
}
```

| فیلد | نوع | اجباری | ارسال شود؟ | توضیح |
|---|---|---:|---:|---|
| `mobile` | `string` | بله | بله | شمارهٔ دریافت‌کنندهٔ SMS. |
| `email` | `string` | خیر | خیر | این پنل با شماره وارد می‌شود؛ فقط یکی از mobile/email لازم است. |
| `code` | `string` | خیر | خیر | در درخواست ارسال کد نباید ارسال شود. |
| `codeLength` | `number` | خیر | خیر | خروجی سرور است. |
| `type` | enum | خیر | خیر | سرور آن را `Otp` تعیین می‌کند؛ از کلاینت نفرستید. |

#### Response success

```json
{
  "isSuccess": true,
  "code": 0,
  "messages": [],
  "data": {
    "mobile": "09123456789",
    "email": null,
    "code": null,
    "codeLength": 4,
    "type": 1
  }
}
```

`data.codeLength` مرجع UI است. طول پیش‌فرض فعلی 4 است، اما بک‌اند می‌تواند آن را از 4 تا 8 تغییر دهد؛ تعداد inputهای OTP را hard-code نکنید.

#### رفتار OTP در بک‌اند

| موضوع | رفتار |
|---|---|
| اعتبار کد پیش از CheckOtp | 3 دقیقه از زمان آخرین تولید/به‌روزرسانی کد |
| تولید کد جدید | کد قبلی را جایگزین و وضعیت تأیید را پاک می‌کند؛ کد قبلی دیگر معتبر نیست. |
| ثبت تأیید موفق | بعد از `CheckOtp`، همان کد حداکثر 5 دقیقه برای `signin` معتبر است. |
| تعداد خطا | چهار خطای کد ثبت می‌شود؛ تلاش بعدی در بازهٔ 10 دقیقه موقتاً قفل می‌شود. |
| کد منقضی/غلط | سرور کد را تعویض می‌کند؛ کاربر باید کد جدید بگیرد. |

---

## 3) تأیید OTP

### `POST /api/Account/CheckOtp`

این فراخوانی **اجباری** است. `signin` فقط کدی را می‌پذیرد که قبلاً با همین endpoint تأیید شده و هنوز منقضی نشده باشد. ارسال مستقیم `signin` با کد SMS، بدون `CheckOtp`، جریان صحیح نیست.

**بدون توکن**  
**Rate limit:** policy `OtpVerify`؛ بر اساس IP. burst اولیه 6 (یا 300) و سپس 3 (یا 150) بار در دقیقه.

#### Request body

```json
{
  "mobile": "09123456789",
  "code": "1234"
}
```

| فیلد | نوع | اجباری | توضیح |
|---|---|---:|---|
| `mobile` | `string` | بله | باید با شماره‌ای که OTP برای آن ارسال شده یکسان باشد. |
| `code` | `string` | بله | کد را به صورت string ارسال کنید تا صفر ابتدایی از بین نرود. |
| `email` | `string` | خیر | برای این جریان ارسال نشود. |
| `codeLength` | `number` | خیر | در درخواست لازم نیست. |
| `type` | enum | خیر | در درخواست لازم نیست. |

#### Response success

```json
{
  "isSuccess": true,
  "code": 0,
  "messages": [],
  "data": {
    "mobile": "09123456789",
    "code": "1234",
    "hasAccount": true,
    "twoFactorEnabled": false
  }
}
```

| خروجی data | کاربرد فرانت‌اند |
|---|---|
| `mobile` | شمارهٔ تأییدشده؛ برای درخواست ورود بعدی استفاده کنید. |
| `code` | همان OTP تأییدشده؛ فقط در حافظهٔ کوتاه‌مدت نگه دارید و به `signin` بفرستید. |
| `hasAccount` | اگر `false` است، ورود ممکن نیست؛ توکن صادر نمی‌شود. |
| `twoFactorEnabled` | اگر `true` است، فیلد رمز را نشان دهید و با OTP به `signin` ارسال کنید. |

بعد از موفقیت، کد را در log، URL، query string، analytics و storage دائمی ننویسید. فقط تا اتمام درخواست `signin` در state همان صفحه باقی بماند.

---

## 4) ورود و دریافت نشست

### `POST /api/Account/signin`

**بدون توکن**  
**Rate limit:** حداکثر 10 درخواست در هر دقیقه برای هر IP (`AccountSignIn`).

### ورود OTP تک‌عاملی نماینده

```json
{
  "mobile": "09123456789",
  "code": "1234",
  "isAdmin": false,
  "isSiteAdmin": false,
  "rememberMe": true
}
```

### ورود OTP + رمز برای کاربر دو‌عاملی

```json
{
  "mobile": "09123456789",
  "code": "1234",
  "password": "رمز-حساب-کاربر",
  "isAdmin": false,
  "isSiteAdmin": false,
  "rememberMe": true
}
```

### همهٔ فیلدهای request

| فیلد | نوع | برای پنل نماینده | توضیح |
|---|---|---:|---|
| `mobile` | `string` | اجباری | همان شمارهٔ تأییدشده. بک‌اند ارقام فارسی را نرمال می‌کند. |
| `code` | `string` | اجباری در ورود OTP | همان کدی که با `CheckOtp` موفق تأیید شده است. |
| `password` | `string` | فقط در `twoFactorEnabled=true` | در حساب دو‌عاملی OTP به‌تنهایی کافی نیست. در حساب تک‌عاملی حذف یا `null` باشد. |
| `isAdmin` | `boolean` | **همیشه `false`** | `true` مخصوص پنل ادمین است و نماینده را رد می‌کند. |
| `isSiteAdmin` | `boolean` | **همیشه `false`** | برای دسترسی ویژهٔ SiteDashboard ادمین است. |
| `rememberMe` | `boolean` | قابل ارسال | در مسیر نماینده، بک‌اند فعلی نشست را persistent ایجاد می‌کند؛ زمان معتبر نهایی را از `refreshTokenExp` پاسخ بخوانید. |
| `deviceId` | `string` | اختیاری | شناسهٔ تصادفی پایدار مرورگر، حداکثر 200 کاراکتر. برای سازگاری آینده بفرستید؛ در ورود نمایندهٔ فعلی، چون `isAdmin=false` است نشست به deviceId گره نمی‌خورد. |
| `cartCode` | `string` | ارسال نشود | فقط برای انتقال سبد خرید کاربر عادی است و به پنل نماینده ارتباطی ندارد. |

> دنبالهٔ صحیح: `CheckOtp` موفق → فوراً `signin`. OTP تأییدشده فقط 5 دقیقه برای ورود معتبر می‌ماند.

#### Response success

```json
{
  "isSuccess": true,
  "code": 0,
  "messages": [],
  "data": {
    "id": 0,
    "userId": 125,
    "token": "eyJ...",
    "refreshToken": "2f78c5db-...",
    "firstName": "نام",
    "lastName": "خانوادگی",
    "tokenExp": "2026-09-30T10:00:00Z",
    "refreshTokenExp": "2026-10-07T10:00:00Z"
  }
}
```

| فیلد data | توضیح |
|---|---|
| `userId` | شناسهٔ کاربر لاگین‌شده. این مقدار شناسهٔ مرکز (`companionId`) نیست. |
| `token` | JWT کوتاه‌مدت؛ در درخواست‌های محافظت‌شده با `Authorization: Bearer <token>` ارسال می‌شود. |
| `refreshToken` | credential بلندمدت‌تر برای تعویض جفت توکن؛ مانند رمز عبور محافظت شود. |
| `firstName` / `lastName` | برای نمایش اولیهٔ هدر پنل. |
| `tokenExp` | زمان انقضای access token به UTC؛ مبنای refresh پیشگیرانه است. |
| `refreshTokenExp` | زمان انقضای refresh token به UTC؛ اگر گذشت، کاربر باید دوباره OTP بگیرد. |
| `provider` / `deviceName` | در DTO تعریف شده‌اند، ولی سازندهٔ پاسخ ورود فعلی آن‌ها را مقداردهی نمی‌کند؛ روی آن‌ها منطق UI نسازید. |

### خطاهای مهم `signin`

| وضعیت | پاسخ/رفتار مورد انتظار فرانت |
|---|---|
| شماره خالی | پیام ورود شماره یا ایمیل را نشان می‌دهد. |
| حساب پیدا نشد یا حذف شده | `isSuccess:false`؛ ورود نماینده را متوقف کنید. |
| حساب Locked | `isSuccess:false`؛ پیام قفل حساب و مسیر پشتیبانی. |
| OTP تأیید نشده، غلط یا منقضی | خطا به فیلد `code` برمی‌گردد؛ کاربر را به مرحلهٔ OTP برگردانید. |
| حساب دو‌عاملی و password خالی | خطا به فیلد `password` برمی‌گردد. |
| رمز نادرست | ورود ناموفق؛ بعد از 5 خطای رمز در 15 دقیقه، تلاش رمز برای 15 دقیقه محدود می‌شود. |
| `isAdmin:true` برای نماینده | `AccessDenied`؛ این flag را اصلاح کنید، نه اینکه به کاربر خطای OTP نشان دهید. |
| HTTP 429 | فرم را disable کنید، retry خودکار تهاجمی نزنید و پیام «تلاش‌های زیاد؛ کمی بعد دوباره امتحان کنید» نمایش دهید. |

---

## استفاده از access token در پنل

پس از ورود موفق، همهٔ endpointهای محافظت‌شدهٔ پنل را با header زیر صدا بزنید:

```http
Authorization: Bearer <data.token>
Content-Type: application/json
```

نمونهٔ endpoint مالک مرکز:

```http
GET /api/Companion/Companion/{id}
Authorization: Bearer <token>
```

### نکتهٔ بسیار مهم دربارهٔ `GET /api/Companion/Companion/{id}`

در پیاده‌سازی فعلی، وجود یک `id` عددی در مسیر الزامی است، اما سرور **id مسیر را برای خواندن رکورد استفاده نمی‌کند**؛ مرکز را از `currentUser.companionId` استخراج می‌کند. پس:

- فرانت نباید `id` مسیر را منبع مجوز یا شناسهٔ قابل‌اعتماد مرکز بداند.
- برای کاربر مالک دارای مرکز فعال و تأییدشده، پاسخ، مرکز خودش را برمی‌گرداند.
- اگر token نداشته باشد/نامعتبر یا منقضی باشد، پاسخ authentication معمولاً `401` است.
- اگر کاربر `companionId` نداشته باشد، controller صریحاً `Forbid()` می‌دهد؛ پاسخ `403` است.

ویرایش مرکز (`PUT /api/Companion/Companion`) در کد فعلی مقدارهای `Active=false` و `Approved=false` می‌گذارد تا مرکز برای بررسی مجدد برود. از آن لحظه mapping کاربر فعلی دیگر `companionId` را پر نمی‌کند و `GET` بالا می‌تواند بلافاصله `403` بدهد؛ این رفتار از کد فعلی ناشی می‌شود، نه خطای OTP یا تغییر توکن. UI پس از ثبت ویرایش باید حالت «در انتظار بررسی» را نمایش دهد و refresh کورکورانهٔ همین GET را خطای خروج از حساب تلقی نکند.

---

## تمدید نشست (Refresh token)

### `POST /api/Account/refreshtoken`

این endpoint جفت access/refresh token قبلی را rotate می‌کند: هر دو مقدار جدید پاسخ را باید **اتمی و هم‌زمان** جایگزین مقادیر قبلی کنید.

**بدون نیاز به Bearer header** (خود جفت توکن در body ارسال می‌شود)  
**Rate limit:** 30 درخواست در دقیقه در حالت IP قابل اتکا، یا 3000 در حالت fallback.

#### Request body

```json
{
  "token": "eyJ...",
  "refreshToken": "2f78c5db-...",
  "isAdmin": false,
  "deviceId": "uuid-persistent-for-this-browser"
}
```

| فیلد | نوع | مقدار برای نماینده | توضیح |
|---|---|---|---|
| `token` | `string` | access token فعلی | همراه refresh token باید با هم ارسال شود. |
| `refreshToken` | `string` | refresh token فعلی | یک‌بارمصرف در هر rotate؛ پس از موفقیت دیگر استفاده نشود. |
| `isAdmin` | `boolean` | `false` | برای پنل نماینده true نکنید. |
| `deviceId` | `string` | همان مقدار login، در صورت وجود | برای سازگاری نشست‌ها. |

#### Response success

پاسخ دقیقاً `ApiResult<UserTokenDto>` و شامل `token`، `refreshToken`، `tokenExp` و `refreshTokenExp` جدید است.

### قواعد لازم در فرانت

1. حدود 30 تا 60 ثانیه پیش از `tokenExp` یک refresh انجام دهید؛ روی هر درخواست refresh جداگانه نفرستید.
2. هنگام هم‌زمانی 401ها، فقط **یک** refresh promise مشترک داشته باشید و بقیهٔ درخواست‌ها را منتظر همان promise کنید.
3. بعد از پاسخ موفق، `token` و `refreshToken` را با هم ذخیره کنید؛ ثبت یکی بدون دیگری باعث خروج در refresh بعدی می‌شود.
4. درخواست تکراری با refresh token قدیمی خارج از grace period 15 ثانیه‌ای، به‌عنوان احتمال سوءاستفاده تلقی و نشست‌های فعال کاربر را revoke می‌کند. در این حالت storage را پاک و کاربر را به ورود ببرید.
5. اگر refresh منقضی/نامعتبر شد، retry نامحدود نکنید؛ خروج محلی و ورود OTP مجدد.

---

## خروج از حساب

### `POST /api/Account/signout`

```http
POST /api/Account/signout
Authorization: Bearer <token>
```

بدنه لازم ندارد. سرور token جاری را باطل می‌کند و wrapper موفقیت برمی‌گرداند.

پس از `isSuccess:true`، `token`، `refreshToken`، state کاربر و هر cache اختصاصی پنل را پاک کنید و به صفحهٔ ورود بروید. اگر شبکه قطع بود یا signout ناموفق شد، پاک‌سازی محلی را متوقف نکنید؛ درخواست بعدی با توکن نامعتبر/قدیمی نباید مجوز UI بدهد.

---

## State machine پیشنهادی فرانت‌اند

```text
mobile
  ├─ invalid                → خطای اعتبارسنجی محلی
  ├─ userdetail: SignUp     → شماره برای پنل نماینده ثبت نشده
  ├─ userdetail: Locked     → حساب قفل است
  └─ userdetail: One/TwoFactor
       └─ otp sent
            ├─ otp invalid / expired → امکان ارسال مجدد طبق timer
            └─ otp verified
                 ├─ twoFactor=true  → دریافت password
                 └─ signin
                      ├─ failed      → نمایش خطا، بدون ساخت نشست
                      └─ succeeded   → ذخیرهٔ جفت token → ورود به پنل
```

### قواعد UX و امنیت

- شماره را در input به انگلیسی نرمال کنید، اما کد را به عنوان `string` نگه دارید.
- دکمهٔ «ارسال مجدد» را حداقل 120 ثانیه غیرفعال کنید؛ با این حال متن خطای سرور اولویت نهایی را دارد.
- زمان‌های `tokenExp` و `refreshTokenExp` UTC هستند؛ نمایش محلی را با تبدیل timezone انجام دهید اما مقایسه را با timestamp انجام دهید.
- هیچ‌گاه access token یا refresh token را در URL، console، crash report، Sentry breadcrumb، analytics event یا screenshot log ثبت نکنید.
- در وب، نگه‌داری credential باید با ملاحظات XSS طراحی شود. اگر localStorage استفاده می‌شود، CSP، جلوگیری از XSS و پاک‌سازی کامل در logout ضروری است. refresh token را مانند password حساس بدانید.
- HTTP 401 را با 403 یکی نکنید: 401 معمولاً مشکل authentication/session است؛ 403 در مسیر Companion می‌تواند نداشتن `companionId` یا ورود مرکز به چرخهٔ بررسی مجدد باشد.

---

## نمونهٔ TypeScript فشرده

```ts
type OtpCheck = {
  mobile: string;
  code: string;
  hasAccount: boolean;
  twoFactorEnabled: boolean;
};

async function loginCompanion(mobile: string, code: string, password?: string) {
  const verified = await api.post<ApiResult<OtpCheck>>("/api/Account/CheckOtp", {
    mobile,
    code,
  });

  if (!verified.data.isSuccess || !verified.data.data?.hasAccount) {
    throw new Error(verified.data.messages?.[0]?.item1 ?? "کد یا حساب معتبر نیست.");
  }

  const auth = await api.post<ApiResult<UserTokenDto>>("/api/Account/signin", {
    mobile,
    code,
    password: verified.data.data.twoFactorEnabled ? password : undefined,
    isAdmin: false,
    isSiteAdmin: false,
    rememberMe: true,
  });

  if (!auth.data.isSuccess || !auth.data.data?.token) {
    throw new Error(auth.data.messages?.[0]?.item1 ?? "ورود ناموفق بود.");
  }

  saveSessionAtomically(auth.data.data);
  return auth.data.data;
}
```

در عمل پیش از این تابع، `POST /api/Account/otp` انجام می‌شود. بعد از `signin` موفق، OTP و password را از state فرم پاک کنید.

---

## چک‌لیست تحویل فرانت‌اند

- [ ] قبل از ارسال OTP، `userdetail` را صدا می‌زنم و `SignUp`/`Locked`/`TwoFactor` را مدیریت می‌کنم.
- [ ] طول inputهای کد را از `data.codeLength` می‌گیرم.
- [ ] بین `/otp` و `/signin` حتماً `/CheckOtp` موفق وجود دارد.
- [ ] برای نماینده همیشه `isAdmin:false` و `isSiteAdmin:false` می‌فرستم.
- [ ] در `TwoFactorEnabled=true` هم OTP و هم password را ارسال می‌کنم.
- [ ] فقط با `isSuccess:true` و `data.token` وارد پنل می‌شوم.
- [ ] در تمام درخواست‌های محافظت‌شده `Authorization: Bearer` می‌فرستم.
- [ ] refresh را single-flight اجرا و جفت token را atomically جایگزین می‌کنم.
- [ ] 401، 403 و 429 را به یک پیام عمومی تبدیل نمی‌کنم.
- [ ] بعد از `PUT /api/Companion/Companion`، 403 ناشی از pending review را به‌عنوان logout تفسیر نمی‌کنم.
- [ ] در خروج، حتی اگر API ناموفق بود credential و cache محلی را پاک می‌کنم.

---

## Endpointهای این مستند

| Method | URL | Token | کاربرد |
|---|---|---:|---|
| `POST` | `/api/Account/userdetail` | خیر | تشخیص حساب، قفل و تک/دو عاملی بودن. |
| `POST` | `/api/Account/otp` | خیر | ارسال کد OTP به شماره. |
| `POST` | `/api/Account/CheckOtp` | خیر | تأیید OTP پیش از ورود. |
| `POST` | `/api/Account/signin` | خیر | دریافت جفت token برای نماینده. |
| `POST` | `/api/Account/refreshtoken` | خیر | rotate کردن نشست. |
| `POST` | `/api/Account/signout` | Bearer | ابطال access token جاری. |
| `GET` | `/api/Companion/Companion/{id}` | Bearer | خواندن مرکز مالک؛ `id` در route اجباری ولی در controller فعلی برای lookup استفاده نمی‌شود. |

`GET /api/Account/userrole?mobile=...` عمداً جزو این جریان نیست: منطق آن مخصوص تشخیص مجوز پنل **Admin** است، نه عضویت یا دسترسی پنل Companion.
