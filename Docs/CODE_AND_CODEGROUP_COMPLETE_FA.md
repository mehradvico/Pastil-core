# مستند کامل `Code` و `CodeGroup` در بک‌اند پاستیل

**مبنای بررسی:** سورس شاخهٔ `master` مخزن `mehradvico/Pastil-core` در commit `45d6720f6209508dcd0d4062ccb8329d6c34aca1` (۸ اکتبر ۲۰۲۶). ابزار خواندن checkout محلی هنگام تهیهٔ سند خطای راه‌اندازی داشت؛ بنابراین تفاوت احتمالی فایل‌های محلیِ commit‌نشده با این نسخه تأیید نشده است. این سند رفتار موجود را توضیح می‌دهد، نه پیشنهاد تغییر کد. اصطلاحی که گاهی «Group Code» گفته می‌شود در نام کلاس و route پروژه **`CodeGroup`** است.

## ۱. کاربرد و رابطهٔ داده‌ها

`CodeGroup` یک دسته از گزینه‌های ثابت/قابل مدیریت است؛ `Code` هر گزینهٔ داخل آن دسته است. این ساختار برای وضعیت‌ها، نوع خدمات، دلایل لغو، نوع پرداخت، اولویت‌ها و مانند آن استفاده می‌شود.

```text
CodeGroup (Id, Name, Label)
    1 ──────────── N
Code (Id, Name, Label, Value, CodeGroupId, Priority, Active)
```

`Code.CodeGroupId` کلید خارجی الزامی به `CodeGroup.Id` است. رابطه در EF با `DeleteBehavior.Restrict` تعریف شده است؛ حذف گروه دارای کد معمولاً به‌خاطر کلید خارجی شکست می‌خورد. `CodeGroup` فیلد `Active` ندارد. هیچ‌یک از دو موجودیت فیلد `Deleted` ندارند؛ `DELETE` این APIها حذف نرم نیست.

| مفهوم | معنی | نمونه |
|---|---|---|
| `CodeGroup.Name` | عنوان نمایشی گروه | «دلیل لغو سفر توسط راننده» |
| `CodeGroup.Label` | شناسهٔ متنی گروه برای کد/فیلتر | `TripCancelReason_Driver` |
| `Code.Name` | متن گزینه برای نمایش به کاربر | «مشکل فنی خودرو» |
| `Code.Label` | شناسهٔ متنی خود گزینه برای منطق سرور | `TripCancelReason_Driver_VehicleIssue` |
| `Code.Value` | مقدار کاری گزینه؛ **الزاماً برابر Label نیست** | در یک migration نوع پرداخت مدرسه `school-reserve` است |
| `Code.Priority` | ترتیب نمایش؛ عدد کوچک‌تر در جست‌وجوی Code زودتر می‌آید | `10` قبل از `60` |
| `Code.Active` | فعال بودن گزینه | `true`/`false` |

شناسهٔ عددی `CodeGroupEnum` در C# را به‌جای `CodeGroup.Id` نفرستید؛ enum فقط نام گروه‌های شناخته‌شدهٔ بخشی از کد را فهرست می‌کند. شناسهٔ واقعی گروه از دیتابیس/پاسخ API می‌آید. همچنین `BaseResultDto.code` یک کد نتیجهٔ درخواست است و با موجودیت `Code` یا `Code.Id` فرق دارد.

## ۲. مدل پایگاه‌داده و فیلدهای API

### ۲.۱. `CodeGroup`

| فیلد | نوع | DTO/رفتار |
|---|---|---|
| `id` | `long` | کلید اصلی identity؛ در `CodeGroupDto` از `Name_FieldDto` می‌آید |
| `name` | `string` | عنوان گروه؛ `[Required]` از `Name_FieldDto` |
| `label` | `string` | شناسهٔ متنی گروه؛ `[Required]` در `CodeGroupDto` |
| `codes` | collection در Entity | رابطهٔ یک‌به‌چند در مدل؛ **در `CodeGroupDto` و پاسخ فهرست/جزئیات برگردانده نمی‌شود** |

جدول `CodeGroups` برای `Name` و `Label` ستون `nvarchar(max)` دارد. در snapshot فعلی برای آن‌ها unique index یا سقف طول تعریف نشده است. DTO نیز قاعدهٔ یکتایی/حداکثر طول ندارد؛ پس تکراری‌بودن Label در این مسیر به‌صورت صریح رد نمی‌شود.

### ۲.۲. `Code`

| فیلد | نوع | `CodeDto` (ساخت/ویرایش/جزئیات) | `CodeVDto` (ردیف فهرست) |
|---|---|---:|---:|
| `id` | `long` | بله | بله |
| `name` | `string`، عنوان نمایشی، `[Required]` | بله | بله |
| `label` | `string`، کلید متنی، `[Required]` | بله | بله |
| `value` | `string`، مقدار، `[Required]` | بله | بله |
| `codeGroupId` | `long`، FK الزامی | بله | **خیر** |
| `priority` | `int` | بله | **خیر** |
| `active` | `bool` | بله | **خیر** |
| `codeGroup` | navigation در Entity | در DTO نیست | در DTO نیست |

جدول `Codes` دارای PK identity، FK و index معمولی روی `CodeGroupId` است. در snapshot برای `Label`, `Value`, `Name` unique index دیده نمی‌شود و ستون‌ها `nvarchar(max)` هستند. در service، بررسی جداگانهٔ وجود گروه، تکراری نبودن `label` در گروه، یا یکتایی سراسری Label دیده نمی‌شود؛ خطای FK/DB در عملیات عمومی به پیام کلی ناموفق تبدیل می‌شود.

## ۳. endpointها و دسترسی

تمام controllerهای زیر `[Authorize]` دارند. روی خود آن‌ها قید مستقیم `Roles=...` دیده نمی‌شود؛ اگر سیاست مجوز سراسری/permission دیگری در سامانه فعال است باید جداگانه بررسی شود. مسیر Area ادمین محل مدیریت است؛ مسیرهای EndUser/Driver فقط خواندنی هستند.

| نقش/Area | روش | مسیر | عملکرد |
|---|---|---|---|
| Admin | GET | `/api/Admin/CodeGroup` | فهرست صفحه‌بندی‌شدهٔ گروه‌ها |
| Admin | GET | `/api/Admin/CodeGroup/{id}` | جزئیات یک گروه |
| Admin | POST | `/api/Admin/CodeGroup` | ایجاد گروه |
| Admin | PUT | `/api/Admin/CodeGroup` | ویرایش کامل گروه |
| Admin | DELETE | `/api/Admin/CodeGroup?id={id}` | حذف فیزیکی گروه |
| Admin | GET | `/api/Admin/Code` | فهرست صفحه‌بندی‌شدهٔ کدها |
| Admin | GET | `/api/Admin/Code/{id}` | جزئیات یک کد |
| Admin | POST | `/api/Admin/Code` | ایجاد کد |
| Admin | PUT | `/api/Admin/Code` | ویرایش کامل کد |
| Admin | DELETE | `/api/Admin/Code?id={id}` | حذف فیزیکی کد |
| EndUser | GET | `/api/EndUser/Code` | فهرست کدها؛ فقط خواندنی |
| Driver | GET | `/api/Driver/Code` | فهرست کدها؛ فقط خواندنی |

در شاخهٔ بررسی‌شده، controller اختصاصی `Code` یا `CodeGroup` برای `Seller`, `Companion` و `Operator` پیدا نشد. برای EndUser/Driver نیز endpoint اختصاصی `CodeGroup` و `Code/{id}` وجود ندارد. این موضوع با امکان استفادهٔ داخلی سرویس Code در سایر بخش‌ها فرق دارد.

### ۳.۱. بدنهٔ عملیات مدیریت

ایجاد گروه:

```http
POST /api/Admin/CodeGroup
Authorization: Bearer <token>
Content-Type: application/json

{"name":"دلیل لغو سفر توسط راننده","label":"TripCancelReason_Driver"}
```

ویرایش گروه باید `id` و هر دو فیلد نام/Label را بفرستد:

```json
{"id":125,"name":"دلیل لغو سفر راننده","label":"TripCancelReason_Driver"}
```

ایجاد کد بعد از داشتن `CodeGroup.Id`:

```http
POST /api/Admin/Code
Authorization: Bearer <token>
Content-Type: application/json

{"name":"مشکل فنی خودرو","label":"TripCancelReason_Driver_VehicleIssue","value":"TripCancelReason_Driver_VehicleIssue","codeGroupId":125,"priority":60,"active":true}
```

ویرایش کد باید `id` و **همهٔ فیلدهای CodeDto** را بفرستد:

```json
{"id":501,"name":"مشکل فنی خودرو","label":"TripCancelReason_Driver_VehicleIssue","value":"TripCancelReason_Driver_VehicleIssue","codeGroupId":125,"priority":60,"active":true}
```

ارقام `125` و `501` صرفاً نمونهٔ ساختار درخواست‌اند و شناسهٔ واقعی دیتابیس نیستند. `PUT` در `CommonSrv` یک Entity تازه از DTO می‌سازد و کل ردیف را `Modified` می‌کند؛ آن را PATCH جزئی تصور نکنید. حذف فیزیکی گروه/کدِ استفاده‌شده در رکوردهای دیگر ممکن است به‌خاطر FK شکست بخورد. برای خارج کردن یک گزینه از فهرست فعال، تغییر `active=false` روی Code معمولاً مناسب‌تر از حذف آن است؛ برای خود گروه سوییچ فعال وجود ندارد.

## ۴. جست‌وجو و صفحه‌بندی

### ۴.۱. گروه‌ها: `GET /api/Admin/CodeGroup`

پارامترها از `BaseInputDto` هستند: `pageIndex`, `pageSize`, `q`, `sortBy`, `available`.

| پارامتر | رفتار واقعی service |
|---|---|
| `q` | اگر غیرخالی باشد، `Name.Contains(q)`؛ جست‌وجوی Label نیست. در همین حالت sort با `Id` نزولی است. Trim صریح در service دیده نمی‌شود. |
| `pageIndex` | پیش‌فرض ۱، به بازهٔ ۱ تا ۱٬۰۰۰٬۰۰۰ clamp می‌شود |
| `pageSize` | پیش‌فرض ۲۰؛ ۰/منفی به ۲۰ و بزرگ‌تر از ۱۰۰۰ به ۱۰۰۰ تبدیل می‌شود |
| `sortBy` | در `CodeGroupService.Search` اعمال نمی‌شود |
| `available` | اعمال نمی‌شود؛ گروه فیلد Active ندارد |

وقتی `q` خالی باشد، ترتیب صریحی برای query گروه‌ها تعیین نشده؛ ترتیب صفحات را پایدار فرض نکنید. خروجی **`BaseSearchDto<CodeGroupDto>`** با `list`, `totalCount`, اطلاعات صفحه و `q` است. annotation متد در controller نوع `BaseResultDto<CodeGroupDto>` نوشته، ولی خروجی واقعی service نوع جست‌وجوی صفحه‌بندی‌شده است.

```http
GET /api/Admin/CodeGroup?q=سفر&pageIndex=1&pageSize=20
Authorization: Bearer <token>
```

### ۴.۲. کدها: `GET /api/Admin/Code`، `/api/EndUser/Code` و `/api/Driver/Code`

هر سه از `CodeService.Search(CodeInputDto)` استفاده می‌کنند؛ فیلترها و ترتیب در هر سه یکسان‌اند.

| پارامتر | رفتار واقعی service |
|---|---|
| `codeGroupLabel` | برابری با `CodeGroup.Label`؛ برای گرفتن گزینه‌های یک گروه. این **ID گروه نیست**. |
| `q` | اگر غیرخالی باشد، برابری با `Code.Name` (`Name.Equals`)، نه contains و نه جست‌وجوی `Label`/`Value` |
| `available=true/false` | برابری با `Code.Active`؛ اگر ارسال نشود، فعال و غیرفعال هر دو می‌آیند |
| `pageIndex`, `pageSize` | قواعد پایهٔ بالا؛ ۱/۲۰ پیش‌فرض و سقف ۱۰۰۰ |
| `sortBy` | ورودی پذیرفته می‌شود ولی نادیده گرفته می‌شود؛ query همیشه `Priority` **صعودی** است |

برابری متن `q`/`codeGroupLabel` مطابق collation دیتابیس انجام می‌شود؛ service خودش lowercase/trim نمی‌کند. برای گزینه‌های قابل انتخاب کاربر یا راننده، `available=true` را صریح بفرستید. در اولویت‌های برابر، tie-breaker مشخصی در service نیست.

```http
GET /api/Driver/Code?codeGroupLabel=TripCancelReason_Driver&available=true&pageIndex=1&pageSize=100
Authorization: Bearer <token>
```

خروجی `CodeSearchDto` است: `list` از **`CodeVDto`**، `totalCount`, `codeGroupLabel` و فیلدهای پایهٔ صفحه‌بندی. هر ردیف لیست فقط `id`, `name`, `label`, `value` دارد؛ `active`, `priority`, `codeGroupId` را از ردیف فهرست نخوانید. اگر پنل Admin برای ویرایش این فیلدها نیاز دارد، `GET /api/Admin/Code/{id}` را بزند یا قرارداد list backend توسعه یابد.

### ۴.۳. ارتباط دو لیست در پنل Admin

1. `GET /api/Admin/CodeGroup` را با صفحه‌بندی بگیرید و گروه را با `id/name/label` نشان دهید.
2. برای کدهای داخل گروه، **`label` گروه** را در `codeGroupLabel` فهرست Code بفرستید؛ `CodeGroupDto` شامل آرایهٔ `codes` نیست.
3. برای ایجاد کد، **`id` گروه** را در `codeGroupId` body بفرستید. پس query با Label فیلتر می‌شود ولی mutation با ID رابطه را ذخیره می‌کند.
4. برای ویرایش کد، ابتدا detail Admin را بخوانید چون list فیلدهای `codeGroupId`, `priority`, `active` را ندارد.

## ۵. خروجی‌های عملیاتی و خطاها

`GET /api/Admin/Code/{id}` و `GET /api/Admin/CodeGroup/{id}` از `FindAsyncDto` مشترک استفاده می‌کنند و `BaseResultDto<T>` با `isSuccess`, `messages`, `code`, `data` برمی‌گردانند. مورد پیدا نشده در همین ساختار `isSuccess=false` می‌شود؛ controller آن را با `Ok(...)` ارسال می‌کند، پس HTTP 200 به‌تنهایی نشانهٔ موفقیت نیست.

`POST` در `CommonSrv` annotationهای `[Required]` را بررسی می‌کند، Entity را ذخیره می‌کند و `BaseResultDto<TDto>` با دادهٔ ساخته‌شده برمی‌گرداند. `PUT` و `DELETE` خروجی واقعی `BaseResultDto` بدون DTO هستند. شکست validation دارای `messages` با نام فیلد است؛ exception دیتابیس/عمومی اغلب فقط پیام کلی ناموفق می‌دهد. با `[ApiController]` ممکن است خطای ModelState پیش از رسیدن به service نیز پاسخ HTTP 400 تولید کند.

`DELETE` از `CommonSrv.DeleteDto(id)` استفاده می‌کند. چون Code/CodeGroup فیلد `Deleted` ندارند، `entity.Remove` و حذف فیزیکی اجرا می‌شود. وجود FK از گروه به کد یا از رکوردهای کسب‌وکار به یک Code می‌تواند حذف را متوقف کند و `isSuccess=false` بدهد. API endpoint «حذف گروه همراه همهٔ کدها» یا «جابه‌جایی خودکار کدها به گروه دیگر» ندارد.

## ۶. lookupهای داخلی و نکات یکپارچگی

`ICodeService` علاوه بر جست‌وجو و CRUD عمومی این متدها را دارد؛ **هیچ‌کدام endpoint عمومی جداگانه در controllerهای بررسی‌شده ندارند**:

| متد | رفتار |
|---|---|
| `GetByLabelAsync(label)` | اولین `Code` با Label برابر را به `CodeDto` نگاشت می‌کند |
| `GetByIdAsync(id)` | Code با ID را به `CodeDto` نگاشت می‌کند |
| `GetIdByLabelAsync(label)` | ID اولین Code با Label برابر را برمی‌گرداند |

این lookupها در کد موجود نه `Active` را فیلتر می‌کنند و نه `CodeGroupId` را. چون unique index سراسری روی `Code.Label` در snapshot دیده نمی‌شود، Label تکراری می‌تواند انتخاب «اولین» را مبهم کند. اگر `GetIdByLabelAsync` هیچ سطری پیدا نکند، دسترسی به `item.Id` خطا می‌دهد. تغییر Label/ID کدهای مبنای وضعیت و پرداخت می‌تواند منطق سرویس‌های دیگر را بشکند؛ مدیریت این داده‌ها باید با احتیاط و migration هماهنگ باشد.

`CodeVDto.value` را جای `id` برای فیلدهای FK مانند `statusId` نفرستید. در مدل‌های وابسته، `...Id` معمولاً به **`Code.Id`** اشاره دارد؛ `label` کلید منطقی مورد استفادهٔ lookupها و `value` مقدار رشته‌ای کسب‌وکار است. نمونهٔ واقعی اختلاف `label/value`: migration نوع پرداخت مدرسه `Label='PaymentType_SchoolReserve'` و `Value='school-reserve'` ذخیره می‌کند.

## ۷. فهرست گروه‌های نام‌گذاری‌شده در سورس

`CodeGroupEnum` فعلی این ۱۹ نام را دارد. این‌ها **نام enum** هستند، نه تضمین وجود سطر در دیتابیس، نه `CodeGroup.Id` و نه فهرست کامل گروه‌های قابل ایجاد:

| ردیف | نام enum | حوزهٔ معمول |
|---:|---|---|
| ۱ | `Comment` | وضعیت نظر |
| ۲ | `Detail_Type` | نوع جزئیات |
| ۳ | `ProductType` | نوع محصول |
| ۴ | `ProductStatus` | وضعیت محصول |
| ۵ | `Order_PaymentType` | شیوهٔ پرداخت سفارش |
| ۶ | `Order_Status` | وضعیت سفارش |
| ۷ | `Order_State` | حالت سفارش |
| ۸ | `Feature_Type` | نوع ویژگی |
| ۹ | `Ticket_Status` | وضعیت تیکت |
| ۱۰ | `Ticket_Importance` | اهمیت تیکت |
| ۱۱ | `Discount_Type` | نوع تخفیف |
| ۱۲ | `Store_Type` | نوع فروشگاه |
| ۱۳ | `Driver_Status` | وضعیت راننده |
| ۱۴ | `Trip_Status` | وضعیت سفر |
| ۱۵ | `Map_type` | نوع نقشه |
| ۱۶ | `CompanionType` | نوع نماینده |
| ۱۷ | `CompanionReserveState` | وضعیت رزرو خدمت نماینده |
| ۱۸ | `CompanionAssistance_Type` | نوع خدمت نماینده |
| ۱۹ | `NotificationType` | نوع اعلان |

نمونهٔ **گروه‌های خارج از این enum** که در سورس/seed تعریف شده‌اند: `TripCancelReason_Driver` و `TripCancelReason_User`. اسکریپت `SeedTripCancelReasons.sql` برای هر گروه پنج گزینه می‌سازد؛ گروه راننده شامل `VehicleIssue`, `UserUnreachable`, `TrafficDelay`, `PetMismatch`, `Other` و گروه کاربر شامل `ChangedMind`, `FoundAlternative`, `DriverTooLate`, `WrongDetails`, `Other` است. پس `CodeGroupEnum` کاتالوگ کاملِ داده‌های runtime نیست.

Enumهای C# دیگری نیز برای برخی کدها/وضعیت‌ها وجود دارند؛ برای نمونه `Comment_NotChecked=1`, `Comment_Accept=2`, `Comment_Reject=3` و `PansionReserveState_Registered=46`, `...Paid=47`, `...Complete=48`. این اعداد را فقط در حوزه و منبع مربوط به خودشان استفاده کنید. همهٔ enumهای پروژه لزوماً سطر `Codes` نیستند؛ بعضی وضعیت‌ها مانند `PansionReserveOwnerDecisionEnum` مستقل از `Code` تعریف شده‌اند. چند enum نیز مقدار عددی هم‌پوشان دارند؛ بنابراین از جمع enumها یک جدول جهانی `Code.Id` نسازید.

## ۸. گرفتن «همهٔ گروه‌ها و کدهای واقعی» از محیط اجرا

سورس و migrationها ساختار و نمونه‌های seed را نشان می‌دهند، اما فهرست **همهٔ رکوردهای فعلی هر محیط** (توسعه/آزمایش/تولید) فقط با خواندن همان دیتابیس قطعی است؛ ادمین می‌تواند Code و CodeGroup جدید بسازد یا تغییر دهد. برای خروجی کامل:

1. `GET /api/Admin/CodeGroup?pageIndex=1&pageSize=1000` را صفحه‌به‌صفحه تا `totalCount` بخوانید.
2. `GET /api/Admin/Code?pageIndex=1&pageSize=1000` را بدون `available` بخوانید تا فعال و غیرفعال هر دو بیایند. این لیست `codeGroupId` ندارد؛ برای رابطهٔ دقیق هر ردیف از `GET /api/Admin/Code/{id}` استفاده کنید، یا در گزارش ادمین یک query/join اختصاصی بسازید.
3. برای نمایش گزینه‌های یک گروه، `codeGroupLabel` را با `label` گروه فیلتر و برای انتخاب‌های فعال `available=true` اضافه کنید.

نمونهٔ SQL فقط‌خواندنی برای مدیر دیتابیس:

```sql
SELECT g.Id AS CodeGroupId, g.Name AS CodeGroupName, g.Label AS CodeGroupLabel,
       c.Id AS CodeId, c.Name AS CodeName, c.Label AS CodeLabel,
       c.Value, c.Priority, c.Active
FROM dbo.CodeGroups AS g
LEFT JOIN dbo.Codes AS c ON c.CodeGroupId = g.Id
ORDER BY g.Id, c.Priority, c.Id;
```

این query فهرست واقعی همان پایگاه‌داده را می‌دهد؛ سند حاضر ادعای فهرست کامل رکوردهای محیط تولید را ندارد.

## ۹. چک‌لیست پیاده‌سازی پنل

- [ ] route گروه را `/api/Admin/CodeGroup` بزنید؛ نام route `GroupCode` نیست.
- [ ] برای فهرست کدهای یک گروه `codeGroupLabel=CodeGroup.label` و برای ثبت کد `codeGroupId=CodeGroup.id` بفرستید.
- [ ] برای انتخاب‌های کاربر/راننده `available=true` بگذارید؛ سرویس به‌صورت پیش‌فرض کد غیرفعال را نیز برمی‌گرداند.
- [ ] جست‌وجوی گروه `Name.Contains(q)` است، اما جست‌وجوی کد `Name.Equals(q)`؛ جست‌وجوی Label/Value با `q` موجود نیست.
- [ ] ترتیب کدها Priority صعودی است، `sortBy` را برای تغییر آن مبنا نگیرید.
- [ ] برای فرم ویرایش کد از detail Admin استفاده کنید؛ list فیلدهای فعال/اولویت/گروه را ندارد.
- [ ] روی `isSuccess` و خطاهای ModelState حساب کنید، نه فقط HTTP 200.
- [ ] `DELETE` را حذف فیزیکی بدانید؛ برای Code استفاده‌شده ابتدا غیرفعال‌کردن را بررسی کنید.
- [ ] `CodeGroupEnum` و شمارهٔ اعضای آن را به‌جای رکوردهای زندهٔ دیتابیس نفرستید.

## ۱۰. منابع کد بررسی‌شده

- [کنترلر Admin کد](https://github.com/mehradvico/Pastil-core/blob/45d6720f6209508dcd0d4062ccb8329d6c34aca1/Api/Areas/Admin/Controllers/CodeController.cs)، [کنترلر Admin گروه](https://github.com/mehradvico/Pastil-core/blob/45d6720f6209508dcd0d4062ccb8329d6c34aca1/Api/Areas/Admin/Controllers/CodeGroupController.cs)، [EndUser](https://github.com/mehradvico/Pastil-core/blob/45d6720f6209508dcd0d4062ccb8329d6c34aca1/Api/Areas/EndUser/Controllers/CodeController.cs)، [Driver](https://github.com/mehradvico/Pastil-core/blob/45d6720f6209508dcd0d4062ccb8329d6c34aca1/Api/Areas/Driver/Controllers/CodeController.cs)
- [CodeService](https://github.com/mehradvico/Pastil-core/blob/45d6720f6209508dcd0d4062ccb8329d6c34aca1/Application/Services/Setting/CodeSrv/CodeService.cs)، [CodeGroupService](https://github.com/mehradvico/Pastil-core/blob/45d6720f6209508dcd0d4062ccb8329d6c34aca1/Application/Services/Setting/CodeGroupSrv/CodeGroupService.cs)، [CommonSrv](https://github.com/mehradvico/Pastil-core/blob/45d6720f6209508dcd0d4062ccb8329d6c34aca1/Application/Common/Service/CommonSrv.cs)
- [Entity کد](https://github.com/mehradvico/Pastil-core/blob/45d6720f6209508dcd0d4062ccb8329d6c34aca1/Entities/Entities/Code.cs)، [Entity گروه](https://github.com/mehradvico/Pastil-core/blob/45d6720f6209508dcd0d4062ccb8329d6c34aca1/Entities/Entities/CodeGroup.cs)، [CodeGroupEnum](https://github.com/mehradvico/Pastil-core/blob/45d6720f6209508dcd0d4062ccb8329d6c34aca1/Application/Common/Enumerable/CodeGroupEnum.cs)، [seed دلایل لغو](https://github.com/mehradvico/Pastil-core/blob/45d6720f6209508dcd0d4062ccb8329d6c34aca1/scripts/database/SeedTripCancelReasons.sql)
