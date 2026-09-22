# مستند اپ نوبس: مدرسه، ثبت‌نام، پرداخت و انجام دوره

این سند قرارداد فعلی API مدرسه پاستیل را برای اپ نوبس مشخص می‌کند: کشف مدرسه و دوره، ثبت‌نام یک یا چند پت، تخفیف و کیف پول، پرداخت، پیگیری وضعیت و شرکت در دوره.

> مسیرهای عمومی با `API_BASE_URL/api/...` و مسیرهای کاربر با `API_BASE_URL/api/EndUser/...` ساخته می‌شوند. تمام endpointهای EndUser به `Authorization: Bearer <access-token>` نیاز دارند. در پاسخ‌های `BaseResultDto` فقط HTTP status معیار موفقیت نیست؛ `isSuccess` را بررسی کنید.

---

## 1. مدل محصول و محدودیت اصلی

یک «دوره» (`SchoolCourse`) محصول قابل ثبت‌نام است و به یک مدرسه تعلق دارد. ثبت‌نام (`SchoolReserve`) برای **فقط یک پت** ساخته می‌شود. بنابراین انتخاب سه پت یعنی سه رزرو مستقل، سه ظرفیت مصرف‌شده و سه پرداخت مستقل.

| نوع دوره | `courseTypeId` | نمایش در اپ |
| --- | ---: | --- |
| ویدیویی | `1` | محتوای ویدئویی دوره |
| آنلاین زنده | `2` | جلسات آنلاین با تاریخ، ساعت و لینک جلسه |
| حضوری | `3` | جلسات در محل مدرسه |

رابط اپ نباید `courseTypeId` ناشناخته را ویدیویی یا حضوری فرض کند؛ برچسب «نوع دوره نامشخص» نشان دهید.

### اصل مهم ظرفیت

ظرفیت باقی‌مانده از این فرمول به دست می‌آید:

```text
capacity - تعداد تمام SchoolReserveهای غیرلغوشده
```

رزرو در وضعیت «ثبت‌شده ولی پرداخت‌نشده» هم ظرفیت را اشغال می‌کند. سرور در زمان ایجاد با تراکنش Serializable ظرفیت را دوباره کنترل می‌کند، پس عددی که قبلاً در صفحه نمایش داده شده فقط راهنماست و نتیجه‌ی نهایی هنگام POST تعیین می‌شود.

---

## 2. قرارداد داده‌های عمومی

### 2.1 فهرست مدرسه و جزئیات

```http
GET /api/School
GET /api/School/{schoolId}
```

فهرست فقط مدرسه‌های تأیید، فعال و قابل نمایش را برمی‌گرداند. queryهای مورد استفاده شامل pagination و فیلترهایی مانند `StateId` هستند.

داده‌ی مهم مدرسه:

```ts
type School = {
  id: number
  name: string
  discription?: string
  addressValue?: string
  stateId: number
  cityId: number
  rateAvg: number
  rateCount: number
  regulations?: string
  picture?: unknown
}
```

### 2.2 فهرست دوره و جزئیات دوره

```http
GET /api/SchoolCourse?SchoolId={schoolId}&PageIndex=1&PageSize=20
GET /api/SchoolCourse/{courseId}
GET /api/SchoolCourse/remainingCapacity/{courseId}
```

`SchoolCourse/{id}` منبع اصلی صفحه‌ی نهایی‌سازی است، چون `remainingCapacity`، جلسات و محتوای دوره را هم دارد.

```ts
type SchoolCourse = {
  id: number
  schoolId: number
  name: string
  discription?: string
  courseTypeId: 1 | 2 | 3
  price: number
  sessionCount: number
  sessionDurationMinutes: number
  capacity: number
  remainingCapacity: number
  petId?: number | null
  petBreedId?: number | null
  active: boolean
  schoolCourseSessions: Array<{
    id: number
    sessionDate: string
    startTime: string
    endTime: string
    meetingUrl?: string | null
    active: boolean
  }>
  schoolCourseVideos: Array<{
    id: number
    name: string
    fileId: number
    sortOrder: number
    durationSeconds?: number | null
    active: boolean
  }>
}
```

### 2.3 انتخاب پت واجد شرایط

پت‌ها را از endpoint استاندارد کاربر (`GET /api/EndUser/UserPet`) بگیرید. اگر دوره `petId` دارد، فقط پت‌هایی را نشان دهید که `userPet.petId === course.petId` هستند. اگر علاوه بر آن `petBreedId` دارد، نژاد هم باید برابر باشد. در هر حال اعتبار نهایی مالکیت پت در سرور انجام می‌شود.

قبل از ورود به پرداخت کنترل کنید:

- حداقل یک پت انتخاب شده باشد؛
- تعداد پت‌های انتخاب‌شده از `remainingCapacity` بیشتر نباشد؛
- دوره فعال و قابل دریافت باشد؛
- مبلغ دوره را فقط از `course.price`/رزرو برگشتی بخوانید، نه از محاسبه‌ی محلی.

---

## 3. جریان ثبت‌نام

### 3.1 یک پت = یک رزرو

```http
POST /api/EndUser/SchoolReserve
Content-Type: application/json
Authorization: Bearer <token>
```

```json
{
  "schoolCourseId": 301,
  "userPetId": 41
}
```

فیلدهای مالی، `bookerId`، `price`، `statusId`، `isReserved` و `reserveCode` را client مشخص نکند. سرور user جاری را از token، قیمت را از دوره و وضعیت اولیه را تعیین می‌کند.

پاسخ موفق یک رزرو با حالت زیر ایجاد می‌کند:

| فیلد | مقدار اولیه |
| --- | --- |
| `isReserved` | `false` |
| `isCancel` | `false` |
| `statusId` | `1` (Registered) |
| `price` و `paymentPrice` | قیمت فعلی دوره |
| `fromWallet` | `false` |

سرور این موارد را رد می‌کند:

- دوره‌ی حذف‌شده یا غیرفعال؛
- پتی که به user جاری تعلق ندارد؛
- ثبت‌نام دوباره‌ی همان پت در همان دوره، تا وقتی رزرو قبلی لغو نشده است؛
- پر بودن ظرفیت.

### 3.2 ثبت چند پت

برای `selectedPetIds` درخواست‌ها را **ترتیبی** بفرستید؛ نه موازی. اگر ظرفیت در میانه تمام شد، رزروهای موفق باقی می‌مانند و برای پت‌های ناموفق پیام دقیق نشان دهید.

```text
برای هر petId انتخاب‌شده:
    POST SchoolReserve
    اگر موفق بود → reserveId را ذخیره کن
    اگر ناموفق بود → نام پت + پیام API را نمایش بده
بعد از پایان → دوره و ظرفیت را refresh کن
```

هیچ endpoint پرداخت گروهی برای مدرسه وجود ندارد. هر `reserveId` checkout مخصوص خودش را دارد.

### 3.3 رفتار درست در timeout ثبت‌نام

ایجاد `SchoolReserve` هدر `Idempotency-Key` ندارد. به همین دلیل:

1. CTA ثبت‌نام را هنگام درخواست busy کنید تا double tap رخ ندهد.
2. اگر پاسخ نامعلوم شد، همان POST را کورکورانه تکرار نکنید.
3. با `GET /api/EndUser/SchoolReserve?SchoolCourseId={courseId}` رزروهای خود کاربر را بخوانید و رکورد همان `userPetId` را پیدا کنید.
4. اگر وجود داشت، همان `reserveId` را ادامه دهید؛ اگر وجود نداشت، کاربر می‌تواند دوباره ثبت کند.

این قاعده جلوی نمایش خطای گمراه‌کننده‌ی «قبلاً ثبت‌نام کرده‌اید» بعد از timeout را می‌گیرد.

---

## 4. تخفیف و کیف پول

تنظیم‌های مالی روی **هر رزرو** اعمال می‌شوند، نه دوره یا مجموعه‌ی پت‌ها.

```http
PUT /api/EndUser/SchoolReserveSetRebate
{ "id": 9001, "rebateCode": "SCHOOL10" }

PUT /api/EndUser/SchoolReserveRemoveRebate?id=9001

PUT /api/EndUser/SchoolReserveSetWallet
{ "id": 9001, "fromWallet": true }
```

بعد از هر تغییر، جزییات رزرو را دوباره بخوانید:

```http
GET /api/EndUser/SchoolReserve/{reserveId}
```

و فاکتور را فقط با ارقام پاسخ سرور بسازید:

```text
مبلغ اولیه  = price
تخفیف        = rebatePrice
سهم کیف پول = walletPrice
مانده قابل پرداخت = paymentPrice - سهم واقعی کیف پولی که در شروع پرداخت تعیین می‌شود
```

نکات UX:

- کد تخفیف خالی را ارسال نکنید.
- بعد از شروع پرداخت، سرور تغییر تخفیف و کیف پول را نمی‌پذیرد؛ کنترل‌های مالی را قفل کنید.
- اگر کاربر کیف پول را انتخاب کرد، مقدار واقعی قابل برداشت فقط در شروع پرداخت محاسبه می‌شود. موجودی را در UI قطعی فرض نکنید.
- اگر چند پت انتخاب شده‌اند، نتیجه‌ی تخفیف ممکن است برای همه‌ی رزروها یکسان نباشد؛ نتیجه را per-reserve نمایش/handle کنید.

---

## 5. پرداخت

### 5.1 شروع checkout برای یک رزرو

```http
POST /api/EndUser/SchoolReservePayment
Authorization: Bearer <token>
Idempotency-Key: 5c8c7667-89a5-4b53-a28b-a9bb3f9ea404
Content-Type: application/json
```

```json
{
  "schoolReserveId": 9001,
  "merchantId": 3
}
```

- `Idempotency-Key` را برای هر checkout با UUID canonical بسازید و بعد از retry همان checkout همان کلید را نگه دارید.
- یک کلید را برای `reserveId` یا payload متفاوت reuse نکنید.
- `amount`، `grossAmount`، `walletAmount`، `rebateAmount`، نوع callback و قیمت را از client قابل‌اعتماد ندانید و برای تصمیم‌گیری از آن‌ها استفاده نکنید؛ backend آن‌ها را محاسبه می‌کند.
- اگر کیف پول کل مبلغ را پوشش می‌دهد، `merchantId` لازم نیست؛ در غیر این صورت باید merchant معتبر انتخاب شود.

پاسخ پرداخت دارای `data` از نوع `PaymentStartDto` است. فیلدهای مهم:

```ts
type PaymentStartResult = {
  paymentCode: string
  paymentIsLink: boolean
  paymentUrl?: string | null
  isTestMode: boolean
  amount: number
  grossAmount: number
  rebateAmount: number
  walletAmount: number
}
```

رفتار اپ:

```text
isSuccess=false → پیام خطای API، CTA را دوباره فعال کن
paymentIsLink=false → انتقال به درگاه نده؛ رزرو را refresh کن
paymentIsLink=true و paymentUrl موجود → همان URL پاسخ را external-open کن
```

URL callback یا token callback را خود اپ نسازد، تغییر ندهد یا ذخیره‌ی دائمی نکند. پس از بازگشت از درگاه، تنها منبع حقیقت `GET /SchoolReserve/{id}` و فهرست رزروهاست.

### 5.2 پرداخت چند پت

```text
رزرو ۱ → checkout ۱
  ├─ پرداخت داخلی/کیف پول → رزرو ۲
  └─ انتقال به درگاه → توقف flow
بازگشت از درگاه → «ثبت‌نام‌های من» را نمایش بده
رزروهای Registered باقیمانده → هرکدام checkout مستقل
```

برای هر رزرو UUID payment جدا بسازید. اپ نباید جمع چند رزرو را در یک `SchoolReservePayment` بفرستد.

---

## 6. وضعیت رزرو، لغو و اتمام

### 6.1 دریافت رزروها

```http
GET /api/EndUser/SchoolReserve?PageIndex=1&PageSize=20
GET /api/EndUser/SchoolReserve/{reserveId}
```

GET لیست فقط رزروهای کاربر جاری را برمی‌گرداند. جزئیات رزرو شامل `schoolCourse`، `userPet` و `booker` است. اگر user مالک شناسه نباشد، endpoint جزئیات 404 برمی‌گرداند؛ آن را «این ثبت‌نام یافت نشد» نمایش دهید، نه خطای فنی.

| `statusId` | وضعیت نمایشی | اقدام کاربر |
| ---: | --- | --- |
| 1 | ثبت شده / در انتظار پرداخت | پرداخت، یا بازگشت به فهرست |
| 2 | پرداخت شده | مشاهده‌ی اطلاعات و محتوای دوره |
| 3 | کامل شده | نمایش پایان موفق دوره |
| 4 | لغو شده | پایان‌یافته؛ ثبت‌نام جدید فقط با توجه به ظرفیت |

`isReserved` منبع تکمیلی مهم است: فقط زمانی که پرداخت با موفقیت روی سرور اعمال شده true می‌شود. پرداخت آغازشده یا redirect به درگاه به معنی پرداخت‌شده نیست.

### 6.2 لغو

```http
PUT /api/EndUser/SchoolReserveCancel
Content-Type: application/json

{
  "id": 9001,
  "isCancel": true,
  "cancelDetail": "درخواست کاربر"
}
```

در وضعیت فعلی backend، لغو user فقط برای رزروی پذیرفته می‌شود که `isReserved === true` باشد و `cancelDetail` خالی نباشد. برای رزرو Registered و پرداخت‌نشده endpoint لغو موفق نمی‌شود؛ پس در UI آن را قابل لغو نشان ندهید و پیش از ایجاد رزرو، انتخاب پت را قطعی بگیرید.

این endpoint فقط وضعیت را Cancelled می‌کند؛ در کد فعلی بازگشت وجه خودکار تعریف نشده است. هیچ متن یا وعده‌ی refund در اپ نمایش ندهید مگر یک flow مالی جداگانه اضافه شود.

### 6.3 شرکت در دوره و پایان دوره

- برای دوره‌ی Live و InPerson، جلسات فعال `schoolCourseSessions` شامل تاریخ، بازه ساعت و در صورت وجود `meetingUrl` هستند.
- job سرور هر ۵ دقیقه، برای جلسات غیر ویدیوییِ شروع‌شونده در ۱۰ دقیقه‌ی آینده، به ثبت‌نام‌کنندگان غیرلغوشده push «شروع کلاس» می‌فرستد. push مکمل است؛ جدول جلسات باید همیشه در صفحه‌ی دوره در دسترس باشد.
- کاربر endpoint ثبت حضور یا کامل‌کردن دوره ندارد.
- تغییر به `statusId = 3` (Complete) توسط مدیریت/ارائه‌دهنده انجام می‌شود، نه اپ نوبس. اپ فقط آن را هنگام refresh نشان می‌دهد.

### محدودیت فعلی دسترسی محتوا

جزئیات عمومی دوره در API فعلی شامل لیست sessionها و videoها است. بنابراین backend فعلی دسترسی محتوای دوره را بر اساس پرداخت end-user محدود نمی‌کند. اپ نوبس نباید ادعا کند «محتوا فقط پس از پرداخت باز می‌شود» مگر کنترل دسترسی server-side جداگانه اضافه شود.

---

## 7. state پیشنهادی اپ

```ts
type SchoolCheckoutState = {
  courseId: number
  selectedPetIds: number[]
  reserves: Array<{
    petId: number
    reserveId: number
    statusId: 1 | 2 | 3 | 4
    isReserved: boolean
    paymentIdempotencyKey?: string
  }>
  phase: 'browse' | 'select-pets' | 'creating' | 'financials' | 'paying' | 'result'
}
```

- پس از POST موفق، `reserveId` را پیش از ورود به financial/payment state ذخیره کنید.
- در resume اپ یا برگشت از درگاه، جزئیات همه‌ی `reserveId`ها را refresh کنید.
- هر CTA فقط یک درخواست در حال اجرا داشته باشد.
- در خطای شبکه‌ی پرداخت، همان `Idempotency-Key` را برای همان reserve retry کنید.
- در خطای شبکه‌ی ایجاد رزرو، ابتدا لیست را بازیابی کنید؛ POST را بی‌محابا retry نکنید.

---

## 8. چک‌لیست QA نوبس

- [ ] فیلتر پت با `petId` و `petBreedId` درست اعمال می‌شود.
- [ ] انتخاب بیش از ظرفیت در UI مسدود است و خطای نهایی سرور هم نمایش داده می‌شود.
- [ ] ثبت چند پت ترتیبی است؛ نتیجه‌ی partial failure از بین نمی‌رود.
- [ ] timeout ثبت‌نام با بازیابی فهرست حل می‌شود، نه duplicate POST.
- [ ] تخفیف/کیف پول برای هر reserve اعمال و بعد از آن جزئیات refresh می‌شود.
- [ ] پرداخت هر reserve UUID idempotency جدا دارد؛ retry همان UUID را دارد.
- [ ] `paymentIsLink=false` redirect نمی‌شود.
- [ ] بعد از callback، `isReserved` و `statusId` از API خوانده می‌شود.
- [ ] لغو بدون علت یا برای رزرو پرداخت‌نشده به کاربر پیشنهاد نمی‌شود.
- [ ] status Complete فقط نمایش داده می‌شود؛ اپ endpoint تغییر وضعیت را فراخوانی نمی‌کند.

---

## 9. فهرست endpointها

| جریان | Method | Endpoint |
| --- | --- | --- |
| مدارس عمومی | GET | `/api/School`، `/api/School/{id}` |
| دوره‌ها و ظرفیت | GET | `/api/SchoolCourse`، `/api/SchoolCourse/{id}`، `/api/SchoolCourse/remainingCapacity/{id}` |
| ثبت‌نام/فهرست/جزئیات من | POST/GET/GET | `/api/EndUser/SchoolReserve`، `/api/EndUser/SchoolReserve/{id}` |
| تخفیف | PUT | `/api/EndUser/SchoolReserveSetRebate`، `/api/EndUser/SchoolReserveRemoveRebate?id={id}` |
| کیف پول | PUT | `/api/EndUser/SchoolReserveSetWallet` |
| پرداخت | POST | `/api/EndUser/SchoolReservePayment` |
| لغو | PUT | `/api/EndUser/SchoolReserveCancel` |

**وضعیت سند:** منطبق با backend فعلی پاستیل در ۲۱ سپتامبر ۲۰۲۶.
