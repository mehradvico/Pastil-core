# مستند رزرو خدمات کاربران برای پنل نماینده و اپراتور

**مخاطب:** فرانت‌اند پنل Companion/نماینده/کلینیک و پنل Operator/اپراتور مرکز  
**دامنه:** رزرو عادی یک خدمت مرکز (`CompanionReserve`)؛ شامل تاریخ، بازهٔ ساعت انتخاب‌شده، وضعیت رزرو، کاربر، پت، مسئول و وضعیت انجام خدمت.  
**خارج از دامنه:** رزرو پانسیون و ثبت‌نام مدرسه، مدل و endpoint مستقل دارند.

## پاسخ کوتاه به نیاز اصلی

برای نشان‌دادن «کاربر چه تاریخی و چه ساعتی خدمت را رزرو کرده» این سه منبع را با هم استفاده کنید:

```text
تاریخ نوبت     = doDate (فقط بخش تاریخ)
ساعت شروع      = companionTime.startTime
ساعت پایان     = companionTime.endTime

رزروهای قدیمی  = companionAssistanceTime.startTime / endTime
                 فقط وقتی companionTime وجود ندارد
```

رزروهای جدید از ساعت کاری مرکز (`companionTime`) استفاده می‌کنند؛ `companionAssistanceTime` مدل قدیمی زمان‌بندیِ هر خدمت است و فقط برای تاریخچه نگه داشته شده است. `doDate` به‌تنهایی ساعت نوبت نیست و نباید بدون slot زمانی به‌عنوان «زمان شروع» نمایش داده شود.

## دسترسی و endpointهای اصلی

همهٔ endpointهای این بخش نیازمند header زیر هستند:

```http
Authorization: Bearer <access-token>
```

| بازیگر | لیست رزروها | جزئیات یک رزرو | محدودهٔ داده‌ای که سرور اعمال می‌کند |
|---|---|---|---|
| مالک/نماینده/کلینیک | `GET /api/Companion/CompanionReserve` | `GET /api/Companion/CompanionReserve/{id}` | `companionId` از token گرفته می‌شود؛ query نمی‌تواند مرکز دیگری را انتخاب کند. |
| اپراتور/کارمند مرکز | `GET /api/Operator/CompanionReserve` | `GET /api/Operator/CompanionReserve/{id}` | فقط رزرو نهایی‌شده‌ای که به کاربر تخصیص دارد، یا اگر خود او مالک مرکز است، قابل دریافت است. عضویت فعال و پذیرفته‌شده بررسی می‌شود. |

در تمام مسیرها، `id` شناسهٔ داخلی رزرو است، نه `reserveCode`.

## ۱) دریافت لیست رزروها

### پنل نماینده / کلینیک

```http
GET /api/Companion/CompanionReserve?reserveState=CompanionReserveState_CurrentDays&pageIndex=1&pageSize=50&sortBy=Old
```

### پنل اپراتور

```http
GET /api/Operator/CompanionReserve?reserveState=CompanionReserveState_CurrentDays&pageIndex=1&pageSize=50&sortBy=Old
```

سرور در endpoint نماینده مقدار `companionId` و در endpoint اپراتور مقدار `companionAssistanceUserId` را از token بازنویسی می‌کند. این دو مقدار را از UI ارسال یا به آن‌ها برای کنترل دسترسی تکیه نکنید.

### Queryهای پشتیبانی‌شده

| پارامتر | نوع | کاربرد |
|---|---|---|
| `pageIndex` | number، از ۱ | شماره صفحه؛ پیش‌فرض ۱. |
| `pageSize` | number، ۱ تا ۱۰۰۰ | تعداد رکورد هر صفحه؛ پیش‌فرض ۲۰. |
| `sortBy` | `1` یا `2` | `1` = جدیدترین شناسهٔ ثبت‌شده، `2` = قدیمی‌ترین شناسهٔ ثبت‌شده. این sort بر مبنای زمان نوبت نیست. |
| `q` | string | جست‌وجوی دقیق `reserveCode` یا بخشی از نام، نام خانوادگی یا موبایل رزروکننده. |
| `bookerId` | number | فیلتر کاربر رزروکننده. |
| `userPetId` | number | فیلتر رزروهای دارای آن پت. |
| `companionAssistanceId` | number | فیلتر یک خدمت مشخص مرکز. |
| `companionTimeId` | number | فیلتر slot جدید ساعت کاری مرکز. |
| `companionAssistanceTimeId` | number | فیلتر slot قدیمیِ ساعت خدمت. |
| `isFemale` | boolean | فیلتر جنسیت انتخاب‌شده برای اپراتور، اگر داده ثبت شده باشد. |
| `reserveState` | enum | فیلتر وضعیت عملیاتی لیست؛ جدول پایین را ببینید. |

Endpoint فعلی **فیلتر تاریخ شروع/پایان ندارد** و sort زمانی هم ندارد. برای تقویم روزانه/هفتگی، فیلتر و مرتب‌سازی را پس از دریافت رکوردها و بر پایهٔ «زمان ترکیبی نوبت» انجام دهید. اگر حجم رزروها از یک صفحه بیشتر است، برای تقویم دقیق به endpoint با date-range سمت سرور نیاز است؛ فقط افزایش `pageSize` راه‌حل مقیاس‌پذیر نیست.

### پاسخ لیست

لیست، `BaseSearchDto` است و برخلاف endpoint جزئیات، wrapper `isSuccess` ندارد:

```json
{
  "pageIndex": 1,
  "pageSize": 50,
  "q": null,
  "sortBy": 2,
  "available": null,
  "totalCount": 1,
  "list": [
    {
      "id": 981,
      "reserveCode": "CR-...",
      "doDate": "2026-10-03T00:00:00",
      "isReserved": true,
      "isCancel": false,
      "companionTimeId": 71,
      "companionTime": {
        "id": 71,
        "startTime": "09:30",
        "endTime": "10:15",
        "weekDay": { "label": "Friday", "name": "جمعه" }
      },
      "companionAssistance": {
        "id": 18,
        "assistance": { "id": 5, "name": "ویزیت" }
      },
      "booker": {
        "id": 42,
        "fullName": "علی رضایی",
        "mobile": "09121234567"
      },
      "state": { "id": 31, "name": "...", "label": "...", "value": "..." },
      "operatorState": { "id": 34, "name": "...", "label": "...", "value": "..." }
    }
  ]
}
```

مقادیر متنی Code (`name`، `label` و `value`) از تنظیمات سرور می‌آیند. برای متن وضعیت، آن‌ها را بر عددهای hard-code شده ترجیح دهید.

## ۲) محاسبه و نمایش تاریخ/ساعت نوبت

### الگوریتم الزام‌آور نمایش

```ts
type Reserve = {
  doDate: string;
  companionTime?: { startTime?: string; endTime?: string } | null;
  companionAssistanceTime?: { startTime?: string; endTime?: string } | null;
  companionAssistancePackageOnlineSelection?: unknown | null;
};

function getReserveSlot(reserve: Reserve) {
  // ترتیب مهم است: CompanionTime منبع جدید و اصلی است.
  return reserve.companionTime ?? reserve.companionAssistanceTime ?? null;
}

function getAppointmentDisplay(reserve: Reserve) {
  const slot = getReserveSlot(reserve);
  const date = reserve.doDate.slice(0, 10);

  if (slot?.startTime) {
    return {
      date,
      startTime: slot.startTime, // مانند 09:30
      endTime: slot.endTime ?? null,
      scheduledStart: `${date}T${slot.startTime}:00`,
      source: reserve.companionTime ? "companionTime" : "legacyAssistanceTime"
    };
  }

  // رزرو آنلاین فوری ممکن است slot نداشته باشد؛ doDate همان زمان شروع آن است.
  if (reserve.companionAssistancePackageOnlineSelection) {
    return { date, startTime: null, endTime: null, scheduledStart: reserve.doDate, source: "instantOnline" };
  }

  return { date, startTime: null, endTime: null, scheduledStart: null, source: "missingSlot" };
}
```

| فیلد | معنی دقیق | نمایش پیشنهادی |
|---|---|---|
| `doDate` | تاریخ انجام رزرو. برای رزرو slotدار، سرور در اعتبارسنجی از بخش تاریخ آن استفاده می‌کند. | تاریخ شمسی/میلادی مطابق استاندارد محصول؛ زمان داخل آن را ساعت نوبت فرض نکنید. |
| `companionTime.startTime` | ساعت شروعِ انتخاب‌شده از ساعت کاری مرکز؛ فرمت `HH:mm`. | ساعت شروع اصلی رزرو جدید. |
| `companionTime.endTime` | ساعت پایان بازهٔ کاری؛ فرمت `HH:mm`. | نمایش بازه مانند `۰۹:۳۰ تا ۱۰:۱۵`. |
| `companionAssistanceTime.startTime/endTime` | ساعت خدمتِ مدل قدیمی. | فقط fallback تاریخچه، اگر `companionTime` وجود ندارد. |
| `companionTime.weekDay` / `companionAssistanceTime.weekDay` | روز هفتهٔ تعریف‌شده برای slot. | برای اطلاعات کمکی یا اعتبارسنجی؛ خود تاریخ نوبت را از `doDate` بخوانید. |
| `callStartDate`, `callEndDate` | زمان واقعی شروع/پایان تماس درون‌برنامه‌ای؛ nullable. | فقط log/جزئیات تماس؛ جای ساعت برنامه‌ریزی‌شده نیست. |

### قاعدهٔ مهم زمان و timezone

تاریخ‌های این API به شکل `DateTime` و بدون قرارداد offset صریح برمی‌گردند. آن‌ها را زمان کسب‌وکار/سرور در نظر بگیرید و بی‌دلیل با تبدیل UTC ساعت نوبت را جابه‌جا نکنید. به‌خصوص در رزرو slotدار، زمان معتبر برای تقویم از ترکیب `doDate.Date + startTime` ساخته می‌شود.

### رزروهای آنلاین فوری

اگر `companionAssistancePackageOnlineSelection` وجود دارد و روش انتخاب‌شده فوری باشد، سرور `doDate` را روی زمان فعلی ثبت می‌کند و ممکن است `companionTime` نداشته باشد. در این حالت `doDate` همان زمان مناسب برای نمایش «شروع فوری» است. برای رزرو آنلاینِ غیر فوری که slot دارد، همچنان از الگوریتم ترکیبی بالا استفاده کنید.

### محدودیت فعلی فیلتر «رزروهای جاری» در همان روز

`reserveState=CurrentDays` در سرور با شرط خام `doDate >= DateTime.Now` اجرا می‌شود، نه با زمان ترکیبی `doDate + startTime`. اگر یک رزرو برای امروز با `doDate` ساعت `00:00` ذخیره شده باشد، ممکن است با وجود slot آینده در تب «منقضی» دیده شود. بنابراین برای تقویم امروز، طبقه‌بندی نهایی UI را با `scheduledStart` محاسبه کنید و این فیلتر را تنها یک فیلتر کمکی بدانید.

## ۳) دریافت جزئیات رزرو

### نماینده / مرکز

```http
GET /api/Companion/CompanionReserve/981
```

پاسخ از نوع `BaseResultDto<CompanionReserveAdminVDto>` است. سرور بعد از دریافت داده، تعلق رزرو به مرکزِ کاربر جاری را بررسی می‌کند.

### اپراتور

```http
GET /api/Operator/CompanionReserve/981
```

پاسخ از نوع `BaseResultDto<CompanionReserveVDto>` است. سرور بررسی می‌کند رزرو نهایی شده باشد و به همان اپراتور تخصیص داشته باشد؛ مالک مرکز نیز برای رزروهای فوری/بدون متخصص می‌تواند آن را ببیند.

نمونه پاسخ اپراتور:

```json
{
  "isSuccess": true,
  "code": 0,
  "messages": [],
  "data": {
    "id": 981,
    "reserveCode": "CR-...",
    "doDate": "2026-10-03T00:00:00",
    "createDate": "2026-09-26T14:20:00",
    "doneDate": null,
    "isReserved": true,
    "isCancel": false,
    "companionTimeId": 71,
    "companionTime": { "startTime": "09:30", "endTime": "10:15" },
    "companionAssistance": { "assistance": { "name": "ویزیت" } },
    "booker": { "fullName": "علی رضایی", "mobile": "09121234567" },
    "userPets": [],
    "companionAssistanceUser": { "user": { "fullName": "اپراتور مرکز" } },
    "stateId": 31,
    "operatorStateId": 34,
    "operatorState": { "name": "...", "label": "..." }
  }
}
```

### تفاوت مهم پاسخ جزئیات نماینده

`GET /api/Companion/CompanionReserve/{id}` در DTO فعلی خود `companionTime` و `companionTimeId` ندارد، با اینکه سرویس آن را از دیتابیس load می‌کند. به‌همین دلیل برای رزروهای جدید، ساعت انتخاب‌شده در جزئیات مخصوص نماینده برنمی‌گردد.

راه‌حل فعلی فرانت:

1. ردیف همین رزرو را از پاسخ لیست نگه دارید، زیرا لیست از `CompanionReserveVDto` و شامل `companionTime` است.
2. در صفحهٔ جزئیات، جزئیات نماینده را با slot ذخیره‌شده از همان لیست merge کنید.
3. اگر صفحهٔ جزئیات با refresh مستقیم باز می‌شود و ردیف لیست موجود نیست، ابتدا لیست رزروها را برای پیدا کردن رکورد بخوانید؛ یا از بک‌اند بخواهید `CompanionTime`/`CompanionTimeId` را به `CompanionReserveAdminVDto` اضافه کند.

این محدودیت برای endpoint جزئیات اپراتور وجود ندارد، چون `CompanionReserveVDto` شامل `companionTime` است.

## ۴) فیلدهای مهم هر رزرو

### شناسه، کاربر و خدمت

| فیلد | توضیح |
|---|---|
| `id` | شناسهٔ فنی رزرو؛ برای endpoint جزئیات و عملیات ارسال می‌شود. |
| `reserveCode` | کد قابل نمایش/پیگیری رزرو؛ برای جست‌وجوی دقیق نیز پشتیبانی می‌شود. |
| `bookerId` و `booker` | شناسه و حداقل اطلاعات کاربر رزروکننده: `fullName`، `mobile`، `email`، `pictureId` و… |
| `userPets` و `petCount` | پت/پت‌های انتخاب‌شده و شمار آن‌ها. |
| `companionAssistanceId` و `companionAssistance` | خدمت رزرو شده؛ نام قابل نمایش معمولاً `companionAssistance.assistance.name` است. |
| `companionAssistancePackages` | پکیج‌های انتخاب‌شده برای خدمت، اگر وجود داشته باشند. |
| `companionAssistanceType` | نوع ارائه خدمت: آنلاین `37`، حضوری در مرکز `38`، در محل کاربر `39`. متن را از Code برگشتی نمایش دهید. |
| `companionAssistanceUserId` و `companionAssistanceUser` | مسئول تخصیص‌داده‌شده برای انجام خدمت؛ ممکن است null باشد. |

### زمان، مکان و یادداشت

| فیلد | توضیح |
|---|---|
| `doDate` | تاریخ نوبت؛ با ساعت slot ترکیب شود. |
| `createDate` | زمان ایجاد رزرو؛ با زمان نوبت اشتباه گرفته نشود. |
| `doneDate` | زمان تکمیل نهایی، در صورت ثبت. nullable است. |
| `cancelDate` و `cancelDetail` | زمان و توضیح لغو، اگر `isCancel=true`. |
| `bookerDetail` | توضیح ثبت‌شده توسط کاربر برای رزرو. |
| `assistanceDetail` | توضیحات مربوط به خدمت. |
| `operatorDetail` | توضیح اپراتور هنگام ثبت انجام/لغو عملیات. |
| `addressId` و `address` | برای خدمت در محل کاربر؛ شامل نام، موبایل، متن آدرس، شهر، کدپستی و `location` است. فقط در صورت نیاز عملیاتی نمایش داده شود. |

## ۵) معنای وضعیت‌ها

### وضعیت نهایی‌سازی و لغو

| فیلد | معنی | رفتار UI |
|---|---|---|
| `isReserved` | پرداخت/نهایی‌سازی رزرو اعمال شده است. | فقط وقتی true است رزرو را قطعی و قابل انجام بدانید. |
| `isCancel` | رزرو لغو شده است. | کارت را در وضعیت لغوشده نشان دهید؛ زمان/کاربر را نگه دارید، عملیات انجام را disable کنید. |
| `doneDate` | زمان تکمیل نهاییِ ثبت‌شده. | فقط وجود آن معنای «انجام‌شده» برای فیلتر Done سرور دارد. |

### وضعیت مالی/اصلی (`state`)

`stateId` و آبجکت `state` وضعیت اصلی را می‌دهند. مقادیر پیش‌فرض فعلی عبارت‌اند از:

| شناسه | enum داخلی | برداشت UI |
|---:|---|---|
| 30 | `CompanianReserveState_Registered` | رزرو ثبت شده، هنوز وضعیت پرداخت نهایی نشده است. |
| 31 | `CompanianReserveState_PrePaid` | پرداخت پیش‌پرداخت اعمال شده و `isReserved` معمولاً true است. |
| 32 | `CompanianReserveState_Paid` | پرداخت/مبلغ نهایی ثبت شده است. |
| 33 | `CompanianReserveState_Complete` | وضعیت اصلی کامل‌شده، اگر در جریان مدیریتی ثبت شود. |

در UI متن `state.name` یا `state.label` را اولویت دهید؛ شناسه‌ها fallback هستند.

### وضعیت اجرای اپراتور (`operatorState`)

| شناسه | enum داخلی | معنی |
|---:|---|---|
| 34 | `OperatorState_InComplete` | هنوز نتیجهٔ انجام خدمت ثبت نشده است. |
| 35 | `OperatorState_Complete` | اپراتور/مالک مرکز انجام را ثبت کرده است. |
| 36 | `OperatorState_Cancelled` | اپراتور/مالک مرکز لغو عملیات را ثبت کرده است. |

`operatorState` با `state` یکی نیست. برای badge اجرا، `operatorState` را نمایش دهید؛ برای پرداخت/تسویه، `state` و قیمت‌ها را نمایش دهید؛ برای لغو نهایی، ابتدا `isCancel` را بررسی کنید.

### پاسخ کاربر به انجام خدمت

`userResponse` سه‌حالته است:

| مقدار | معنی |
|---|---|
| `null` | هنوز پاسخ کاربر ثبت نشده یا نتیجه در انتظار تأیید است. |
| `true` | کاربر تأیید کرده است؛ در لغو عملیاتی نیز سرور آن را true می‌گذارد چون تأیید کاربر لازم نیست. |
| `false` | کاربر نتیجه را تأیید نکرده است؛ باید برای پیگیری نمایش داده شود. |

نکتهٔ مهم: ثبت `operatorStateId=35` به‌تنهایی `doneDate` را پر نمی‌کند. پس «اپراتور تکمیل را زده» و «رزرو در تب Done سرور است» دو وضعیت معادل نیستند. همچنین endpoint پاسخ کاربر نیز در پیاده‌سازی فعلی `doneDate` را پر نمی‌کند. برای وضعیت ظاهری، از `operatorState` و `userResponse` استفاده کنید؛ برای فیلتر Done دقیقاً از `doneDate` پیروی کنید.

## ۶) فیلترهای وضعیت لیست

| مقدار `reserveState` | نام enum | شرط واقعی سرور |
|---:|---|---|
| 0 | `CompanionReserveState_CurrentDays` | `isReserved=true`، `isCancel=false`، `doneDate=null` و `doDate >= اکنون` |
| 1 | `CompanionReserveState_Done` | `isReserved=true`، `isCancel=false` و `doneDate` مقدار دارد |
| 2 | `CompanionReserveState_Expired` | `isReserved=true`، `isCancel=false`، `doneDate=null` و `doDate <= اکنون` |

در query می‌توانید نام enum یا مقدار عددی آن را بفرستید. نمونهٔ عددی:

```http
GET /api/Operator/CompanionReserve?reserveState=0&pageIndex=1&pageSize=50&sortBy=2
```

رزرو لغوشده در هیچ‌یک از این سه تب نیست. برای گزارش لغوشده، endpoint فعلی فیلتر اختصاصی ندارد؛ لیست بدون `reserveState` را بخوانید و در کلاینت با `isCancel` جدا کنید، با درنظرگرفتن pagination.

## ۷) عملیات تغییر وضعیت

### ثبت نتیجه توسط اپراتور

```http
PUT /api/Operator/CompanionReserveOperatorUpdate
```

```json
{
  "id": 981,
  "operatorStateId": 35,
  "operatorDetail": "ویزیت انجام شد",
  "operatorStuffPrice": 0,
  "operatorWagesPrice": 350000,
  "operatorUnpaid": false
}
```

اپراتور فقط رزروی را می‌تواند تغییر دهد که نهایی شده، لغو نشده، به خود او تخصیص یافته و عضویت فعال/پذیرفته‌شده در مرکز داشته باشد. مالک مرکز نیز برای رزروهای بدون متخصص مشخص اجازه دارد.

### ثبت نتیجه توسط مالک مرکز

```http
PUT /api/Companion/CompanionReserveOperatorUpdate
```

بدنه همان `CompanionReserveOperatorDto` بالاست. مالک مرکز می‌تواند رزروهای مرکز خود را مدیریت کند.

قواعد مهم:

- `operatorStateId=34` به‌عنوان نتیجهٔ جدید پذیرفته نمی‌شود؛ وضعیت باید Complete یا Cancelled باشد.
- برای `operatorStateId=36`، `operatorDetail` اجباری است.
- قیمت دستمزد و لوازم نمی‌تواند منفی باشد.
- پس از `operatorStateId=35`، تغییر به وضعیت دیگر مجاز نیست؛ ارسال دوبارهٔ همان Complete idempotent است.
- مبلغ نهایی برابر `operatorStuffPrice + operatorWagesPrice` می‌شود و در `operatorFinalPrice` و `paymentPrice` ثبت می‌شود.
- `operatorUnpaid=true` فقط برای Complete با مبلغ نهایی بزرگ‌تر از صفر معنی دارد.

پس از موفقیت، ردیف/جزئیات را refresh کنید؛ پاسخ عملیات فقط DTO تغییر وضعیت را برمی‌گرداند، نه مدل کامل رزرو.

### تغییر وضعیت اصلی توسط نماینده

```http
PUT /api/Companion/CompanionReserveChangeState
```

```json
{ "id": 981, "stateId": 33 }
```

این endpoint فقط وقتی `paymentPrice` صفر نباشد وضعیت اصلی را تغییر می‌دهد و به مرکز token محدود است. این عملیات هم `doneDate` را تنظیم نمی‌کند.

### تخصیص مسئول به رزرو (نماینده)

```http
GET /api/Companion/CompanionReserveAssignee/981?includeAll=true
PUT /api/Companion/CompanionReserveAssign
```

نمونه تخصیص همکارِ متصل به خدمت:

```json
{ "id": 981, "companionAssistanceUserId": 44 }
```

نمونه تخصیص همکار مرکز که هنوز به خدمت متصل نشده است:

```json
{ "id": 981, "companionAssistanceUserId": 0, "companionUserId": 16 }
```

سرور برای مورد دوم اتصال فعال به خدمت را می‌سازد. تخصیص جدید، وضعیت اپراتور و هزینه‌های قبلی را reset می‌کند و با زمان انتخاب‌شده (`companionTime` یا fallback قدیمی) تداخل برنامهٔ مسئول را کنترل می‌کند.

### لغو رزرو

```http
PUT /api/Companion/CompanionReserveCancel
```

این endpoint با وجود route Companion، در سرویس فعلی فقط برای Admin/دارندهٔ permission ناحیهٔ Admin مجاز است و برای مالک عادی مرکز مبنای لغو نیست. در صورت مجازبودن:

```json
{ "id": 981, "isCancel": true, "cancelDetail": "توضیح اجباری لغو" }
```

## ۸) مدل نمایشی پیشنهادی برای جدول/تقویم

```ts
type ReserveRow = {
  id: number;
  reserveCode: string;
  customerName: string;
  customerMobile: string;
  serviceName: string;
  appointmentDate: string;
  startTime: string | null;
  endTime: string | null;
  scheduleSource: "companionTime" | "legacyAssistanceTime" | "instantOnline" | "missingSlot";
  mainStatus: string;
  executionStatus: string;
  isReserved: boolean;
  isCancelled: boolean;
  assignedOperatorName: string | null;
};
```

ترتیب badge پیشنهادی:

1. اگر `isCancel=true`: «لغو نهایی».
2. اگر `isReserved=false`: «در انتظار نهایی‌سازی/پرداخت».
3. اگر `operatorStateId=36`: «لغو توسط مرکز/اپراتور».
4. اگر `operatorStateId=35` و `userResponse=null`: «انجام شد؛ در انتظار پاسخ کاربر».
5. اگر `operatorStateId=35` و `userResponse=true`: «انجام و تأیید شد».
6. اگر `operatorStateId=35` و `userResponse=false`: «نیازمند پیگیری کاربر».
7. در غیر این صورت: «رزرو قطعی؛ در انتظار انجام».

`state` را به‌عنوان badge مالی دوم نگه دارید؛ آن را جای وضعیت اجرای اپراتور قرار ندهید.

## ۹) خطاها و نکات پیاده‌سازی

- عملیات‌های تغییر وضعیت و جزئیات با wrapper `isSuccess/messages/data` برمی‌گردند. حتی با HTTP `200`، پیش از به‌روزرسانی UI باید `isSuccess` را بررسی کنید.
- `GET` لیست wrapper موفقیت ندارد؛ errorهای HTTP و شکل پاسخ را جداگانه مدیریت کنید.
- اگر ساعت slot وجود ندارد و رزرو فوری آنلاین نیست، «ساعت تعیین نشده» نشان دهید؛ از حدس‌زدن ساعت با `doDate` خودداری کنید.
- برای رزروهای در محل کاربر، دادهٔ `address` و `location` شخصی است. فقط به اپراتور/نمایندهٔ مجاز و فقط در صفحه‌ای که برای انجام خدمت لازم است نمایش داده شود.
- بعد از تغییر وضعیت، لغو، تخصیص مسئول یا بازگشت از جزئیات، لیست را دوباره دریافت یا همان رزرو را refresh کنید؛ از mutate کردن حدسی چند فیلد خودداری کنید.
- از `reserveCode` برای نمایش و جست‌وجو استفاده کنید، اما برای همهٔ عملیات API مقدار `id` ارسال شود.
- ساعت‌های `startTime/endTime` رشته‌های `HH:mm` هستند؛ آن‌ها را با formatter تاریخ که انتظار ISO datetime دارد parse نکنید.

## چک‌لیست فرانت‌اند

- [ ] لیست نماینده از `/api/Companion/CompanionReserve` و لیست اپراتور از `/api/Operator/CompanionReserve` خوانده شود.
- [ ] زمان هر ردیف از `doDate + companionTime` و سپس fallback `companionAssistanceTime` ساخته شود.
- [ ] رزرو فوری آنلاین بدون slot از `doDate` به‌عنوان زمان شروع استفاده کند.
- [ ] `state`، `operatorState`، `isReserved`، `isCancel` و `userResponse` هم‌زمان در UI نمایش/مدیریت شوند.
- [ ] جدول امروز با زمان ترکیبی سمت کلاینت مرتب شود؛ بر `sortBy` یا `CurrentDays` برای ساعت امروز تکیه نشود.
- [ ] صفحهٔ جزئیات نماینده slot زمانی را از ردیف لیست نگه دارد، چون DTO جزئیات فعلی `companionTime` ندارد.
- [ ] در همهٔ عملیات `isSuccess` بررسی و پیام‌های سرور نمایش داده شود.
