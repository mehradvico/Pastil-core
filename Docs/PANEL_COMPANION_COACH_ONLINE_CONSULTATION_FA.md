# مستند پنل نماینده/مربی: روشن و خاموش‌کردن مشاورهٔ آنلاین

> مخاطب: فرانت‌اند پنل نماینده، پزشک، مربی یا عضو تیم مرکز (`Companion`)  
> محدوده: فقط وضعیت حضور فرد جاری برای مشاورهٔ آنلاین؛ نه مدیریت پکیج، خرید، جلسه، چت یا تماس.  
> مسیر پایه: `/api/Companion`

## 1. این toggle دقیقاً چه چیزی را تغییر می‌دهد؟

این کلید، `Online` بودن **عضو تیمی که توکن او ارسال شده** را برای مشاورهٔ آنلاین تغییر می‌دهد؛ یک تنظیم سراسری روی خود مرکز نیست.

```text
عضو تیم A در مرکز X آنلاین است ─┐
عضو تیم B در مرکز X آفلاین است ─┼─> مرکز X برای کاربر «آنلاین» دیده می‌شود
عضو تیم C در مرکز X آفلاین است ─┘    (حداقل یک عضو معتبر آنلاین است)
```

بنابراین:

- روشن‌کردن toggle توسط مربی، فقط membership او در مرکز انتخاب‌شده را آنلاین می‌کند.
- مرکز برای کاربر نهایی وقتی آنلاین است که دست‌کم یک عضو تیم با عضویت معتبر، flag روشن و تاریخ انقضانگذشته داشته باشد.
- خاموش‌کردن یک مربی، مرکز را فقط زمانی آفلاین می‌کند که عضو آنلاین معتبر دیگری نمانده باشد.
- این endpoint، پکیج مشاوره را ایجاد/فعال نمی‌کند و جلسه یا تماس را شروع نمی‌کند؛ فقط availability فرد را تعیین می‌کند.

## 2. احراز هویت، عضویت معتبر و پاسخ‌ها

تمام درخواست‌ها نیاز دارند:

```http
Authorization: Bearer <companion-staff-access-token>
Content-Type: application/json
```

سرور فقط membershipهایی را برای کاربر توکن‌دار معتبر می‌داند که هر سه شرط را داشته باشند:

```text
CompanionUser.UserId = کاربر توکن
Deleted = false
Active = true
UserAccept = true
```

اگر چنین عضویتی برای `companionId` انتخابی نباشد، پاسخ معمولاً HTTP `200` با `isSuccess=false` و پیام عدم دسترسی است. صرف داشتن توکن کافی نیست.

فرم پاسخ:

```json
{
  "code": 0,
  "isSuccess": true,
  "messages": [],
  "data": {
    "online": true,
    "expiresAt": "2026-09-30T15:30:00",
    "onlineStaffCount": 0
  }
}
```

در endpointهای پنل Companion، `onlineStaffCount` پر نمی‌شود و مقدار پیش‌فرض آن `0` است؛ این مقدار فقط در نمایش عمومی برای کاربر نهایی معنای تعداد اعضای آنلاین دارد. برای UI مربی، فقط `online` و `expiresAt` را مصرف کنید.

## 3. قرارداد API

### 3.1 خواندن وضعیت فعلی من

#### `GET /api/Companion/ConsultationOnline?companionId={id}`

نمونه:

```http
GET /api/Companion/ConsultationOnline?companionId=42
Authorization: Bearer <token>
```

پاسخ موفق:

```json
{
  "isSuccess": true,
  "data": {
    "online": true,
    "expiresAt": "2026-09-30T15:30:00",
    "onlineStaffCount": 0
  }
}
```

| فیلد خروجی | نوع | معنی |
|---|---:|---|
| `online` | bool | آیا مربی جاری برای مرکز انتخابی در همین لحظه واقعاً آنلاین است |
| `expiresAt` | DateTime? | پایان اعتبار آنلاین‌بودن؛ اگر `online=false` معمولاً null است |
| `onlineStaffCount` | int | در این endpoint پنل مقدار قابل اتکا نیست؛ برای UI این صفحه استفاده نشود |

سرور `online` را فقط با این قاعده true می‌داند:

```text
ConsultationOnline == true
AND ConsultationOnlineExpiresAt وجود داشته باشد
AND زمان فعلی سرور < ConsultationOnlineExpiresAt
```

پس flag ذخیره‌شدهٔ قدیمی به‌تنهایی کافی نیست؛ بعد از expiration، پاسخ `online=false` است حتی اگر مقدار دیتابیس هنوز true مانده باشد.

### 3.2 روشن یا خاموش‌کردن وضعیت من

#### `PUT /api/Companion/ConsultationOnline`

بدنه:

```json
{
  "companionId": 42,
  "online": true
}
```

| فیلد | نوع | اجباری در UX | توضیح |
|---|---:|---:|---|
| `companionId` | long? | بله | شناسهٔ مرکزی که مربی در آن فعال است |
| `online` | bool | بله | `true` برای روشن، `false` برای خاموش |

#### روشن‌کردن

```json
{ "companionId": 42, "online": true }
```

اثر موفق:

- `ConsultationOnline=true` برای membership مربی در همان مرکز ثبت می‌شود؛
- `expiresAt` روی **زمان فعلی سرور + 3 ساعت** قرار می‌گیرد؛
- پاسخ همان `online=true` و تاریخ انقضا را برمی‌گرداند.

هر بار ارسال `online=true`، پنجرهٔ سه‌ساعته را از نو تمدید می‌کند. اگر UI دکمهٔ «ادامهٔ آنلاین‌بودن» دارد، می‌تواند همین درخواست را پیش از پایان زمان بفرستد.

#### خاموش‌کردن

```json
{ "companionId": 42, "online": false }
```

اثر موفق:

- `ConsultationOnline=false`؛
- `expiresAt=null`؛
- مربی بلافاصله آفلاین محسوب می‌شود.

### 3.3 پاسخ ناموفق نمونه

```json
{
  "isSuccess": false,
  "messages": [["AccessDenied", ""]],
  "data": null
}
```

این پاسخ می‌تواند به‌دلیل نبودن membership فعال/تأییدشده، عضویت در مرکز دیگر یا companionId نامعتبر باشد. switch را فقط پس از `isSuccess=true` تغییر دهید؛ در failure، state قبلی را نگه دارید و GET را دوباره بخوانید.

## 4. قانون حیاتی برای مربی عضو چند مرکز

`companionId` در امضای API nullable است، اما پنل باید **همیشه آن را ارسال کند**.

علت: اگر `companionId` ارسال نشود، سرور روی **همهٔ عضویت‌های فعال و پذیرفته‌شدهٔ کاربر** عمل می‌کند:

| درخواست بدون `companionId` | اثر واقعی |
|---|---|
| `GET /ConsultationOnline` | اگر حداقل یکی از مرکزهای کاربر آنلاین باشد، `online=true` و بیشترین expiration را برمی‌گرداند |
| `PUT {"online":true}` | همهٔ membershipهای معتبر کاربر را هم‌زمان برای سه ساعت آنلاین می‌کند |
| `PUT {"online":false}` | همهٔ membershipهای معتبر کاربر را هم‌زمان خاموش می‌کند |

بنابراین نبودن body یا حذف ناخواستهٔ `companionId` خطرناک است. حتی بدتر، body null در controller به `online=false` تبدیل می‌شود؛ درخواست PUT بدون بدنه می‌تواند همهٔ عضویت‌های معتبر کاربر را خاموش کند. در کلاینت یک schema اجباری برای `companionId` و `online` قرار دهید.

## 5. رفتار زمان و expiration

| رخداد | وضعیت پاسخ | رفتار UI |
|---|---|---|
| روشن‌شدن | `online=true`, `expiresAt=now+3h` | badge «آنلاین تا ساعت …» و countdown نمایش دهید |
| تمدید قبل از پایان | expiry جدید از زمان درخواست محاسبه می‌شود | countdown را از پاسخ جدید بازنشانی کنید |
| پایان سه ساعت | در GET بعدی `online=false` | switch را آفلاین کنید؛ برای ادامه باید PUT با true بزنید |
| خاموش‌کردن دستی | `online=false`, `expiresAt=null` | countdown را لغو کنید |
| خروج/بستن اپ بدون خاموش‌کردن | تا expiration، فرد آنلاین محسوب می‌شود | برای جلوگیری از حضور کاذب، روی logout/تغییر مرکز PUT false بزنید |

تاریخ‌ها با `DateTime.Now` سرور ساخته/ارزیابی می‌شوند. کلاینت نباید محاسبهٔ خودش را جایگزین پاسخ سرور کند؛ `expiresAt` پاسخ منبع حقیقت است.

## 6. ارتباط با آنچه کاربر نهایی می‌بیند

در لایهٔ عمومی، وضعیت مرکز از اعضای معتبر تیم محاسبه می‌شود:

```text
مرکز آنلاین است ⇔ تعداد اعضای معتبر با flag=true و expiration معتبر > 0
```

پس تغییر این toggle می‌تواند `companionOnline` بسته‌های مشاورهٔ آن مرکز را برای کاربران تغییر دهد. این endpoint خودش نه هزینه، نه پکیج، نه ظرفیت و نه پیام جدید ایجاد نمی‌کند.

یک endpoint عمومی جداگانه برای خواندن وضعیت مرکز وجود دارد، اما بخشی از پنل مربی نیست و فرانت Companion نباید برای state خود از آن استفاده کند. پاسخ عمومی می‌تواند `onlineStaffCount` واقعی را داشته باشد؛ endpoint پنل فقط وضعیت **خود مربی** را برمی‌گرداند.

## 7. flow پیشنهادی فرانت‌اند

1. پس از انتخاب مرکز فعال در پنل، `GET /ConsultationOnline?companionId=<currentCompanionId>` را فراخوانی کنید.
2. اگر `isSuccess=false`، switch را disabled و پیام «عضویت فعال/تأییدشده در این مرکز ندارید» نمایش دهید.
3. اگر موفق بود، switch را از `data.online` و زمان را از `data.expiresAt` پر کنید.
4. هنگام تغییر کاربر، optimistic update نکنید یا در failure آن را rollback کنید؛ `PUT` را با هر دو فیلد `companionId` و `online` بفرستید.
5. بعد از success از پاسخ PUT state را به‌روزرسانی کنید و countdown را راه‌اندازی/لغو کنید.
6. در focus مجدد برنامه یا نزدیک expiration، GET را refresh کنید. بعد از expiration فقط GET/PUT دوباره حقیقت را مشخص می‌کند.
7. هنگام logout، خروج از شیفت یا تغییر مرکز، برای مرکز فعلی `PUT` با `online:false` بزنید؛ سپس مرکز بعدی را با id مستقل بخوانید.

## 8. چک‌لیست QA

- [ ] GET برای عضویت فعال و پذیرفته‌شده، وضعیت درست و expiry را نشان می‌دهد.
- [ ] PUT true، expiry حدود سه ساعت پس از زمان server برمی‌گرداند.
- [ ] PUT true دوباره، expiry را تمدید می‌کند.
- [ ] PUT false، expiry را null و حضور را فوری خاموش می‌کند.
- [ ] بعد از گذشت expiry، GET `online=false` می‌دهد حتی اگر قبلاً switch روشن بوده است.
- [ ] مربی عضو دو مرکز فقط با `companionId` مرکز A آن مرکز را تغییر می‌دهد و مرکز B دست‌نخورده می‌ماند.
- [ ] هیچ PUT بدون body یا بدون `companionId` از UI ارسال نمی‌شود.
- [ ] عضو حذف‌شده، inactive یا `userAccept=false` نمی‌تواند وضعیت را بخواند/تغییر دهد.
- [ ] HTTP 200 با `isSuccess=false` به‌عنوان success نمایش داده نمی‌شود.

## 9. فایل‌های مرجع

- `Api/Areas/Companion/Controllers/ConsultationOnlineController.cs`
- `Application/Services/CompanionSrvs/ConsultationOnlineSrv/ConsultationOnlineService.cs`
- `Application/Services/CompanionSrvs/ConsultationOnlineSrv/ConsultationOnlineRules.cs`
- `Entities/Entities/CompanionField/CompanionUser.cs`
