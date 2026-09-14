# مستند کامل فلوی رزرو خدمات همراه (کلینیک / مربی / آرایشگاه) — از صفر تا پرداخت

**مخاطب:** تیم توسعه اپ فلاتر پاستیل
**دامنه:** رزرو خدمات کلینیک دامپزشکی، مربی (Coach)، آرایشگاه/گرومینگ و سایر انواع «همراه» (Companion) — از مرحله‌ی کشف و جست‌وجو تا ثبت رزرو، انتخاب زمان، پرداخت و مراحل پس از پرداخت.
**منبع حقیقت:** این سند مستقیماً از خواندن کد فعلی بک‌اند (`backend/`, .NET 9) و پیاده‌سازی مرجع و در حال کار Nuxt در `webapp/` استخراج شده؛ هرجا وب‌اپ فعلاً کار می‌کند، دقیقاً همان رفتار و همان ترتیب فراخوانی مستند شده است. مسیرهای وب‌اپ (`webapp/app/server/api/**`) صرفاً یک لایه BFF داخلی Nuxt هستند و اپ فلاتر **نباید** به آن‌ها متصل شود؛ در همه‌جای این سند، مسیر واقعی بک‌اند (کنترلر .NET) آورده شده است.

> ⚠️ اگر جایی بین این سند و کد زنده تناقض دیدید، کد بک‌اند (فایل و خط ذکرشده) مرجع نهایی است، نه این سند و نه هیچ سند قدیمی‌تر در `backend/Docs/`.

---

## فهرست

0. قرارداد عمومی پاسخ API و قواعد کلی
1. مفاهیم پایه و شناسه‌ها (Glossary)
2. گام ۱ — کشف/جست‌وجوی مراکز (Discovery)
3. گام ۲ — صفحه‌ی پروفایل مرکز: خدمات، پکیج‌ها، انتخاب حالت ارائه
4. گام ۳ — زمان‌بندی (ساعات کاری مرکز — CompanionTime)
5. گام ۴‑الف — ثبت رزرو تکی (Single Reserve)
6. گام ۴‑ب — ثبت سبد رزرو چندخدمتی (Batch Reserve)
7. پت‌رسان (Trip) — سرویس رفت‌وآمد پت به مرکز
8. کیف پول و کد تخفیف
9. انتخاب درگاه پرداخت (Merchant)
10. شروع پرداخت + قرارداد Idempotency-Key
11. Callback پرداخت و تشخیص اتمام در اپ موبایل (WebView)
12. بعد از پرداخت: وضعیت‌ها، تخصیص اپراتور، لغو، ثبت نظر
13. دیاگرام کامل توالی — رزرو تکی
14. دیاگرام کامل توالی — سبد رزرو چندخدمتی
15. پیوست الف — کاتالوگ کامل پیام‌های خطای فارسی
16. پیوست ب — نکات و دام‌های مهم (Gotchas)

---

## 0. قرارداد عمومی پاسخ API و قواعد کلی

### 0.1 Envelope پاسخ

تقریباً همه‌ی Endpointهای این پروژه، حتی در خطای اعتبارسنجی، **HTTP 200** برمی‌گردانند. موفقیت واقعی فقط با این مقدار مشخص می‌شود:

```ts
response.isSuccess === true
```

شکل کلی پاسخ:

```json
{
  "isSuccess": true,
  "code": 0,
  "messages": [
    { "item1": "متن قابل نمایش به کاربر", "item2": "جزئیات فنی/تشخیصی (معمولاً خالی)" }
  ],
  "data": { }
}
```

- در حالت خطا، متن `messages[0].item1` را مستقیماً (بدون تبدیل به پیام عمومی) به کاربر نمایش دهید.
- `item2` معمولاً خالی است؛ در برخی مسیرها (مثل خطای درج سفارش/رزرو) اخیراً برای عیب‌یابی داخلی پر می‌شود — روی آن به‌عنوان متن قابل‌نمایش به کاربر حساب نکنید.
- پیام «خطا در برقراری ارتباط با سرور» را فقط برای خطای واقعی شبکه یا پاسخ غیرقابل‌خواندن (JSON خراب، Timeout) استفاده کنید، نه برای هر `isSuccess:false`.
- هرگز بر اساس کد وضعیت HTTP نتیجه‌گیری نکنید؛ همیشه `isSuccess` را چک کنید.

### 0.2 احراز هویت

تمام Endpointهای ناحیه‌ی `EndUser` نیازمند هدر زیر هستند:

```http
Authorization: Bearer <token>
```

Endpointهای ریشه (بدون Area — مثل `/api/Companion`, `/api/CompanionAssistance`, `/api/CompanionAssistancePackage`, `/api/CompanionTime`, `/api/Assistance`, `/api/Merchant`) عمومی و بدون نیاز به توکن هستند، اما بعضی از آن‌ها رفتار متفاوتی برای کاربر لاگین‌کرده ندارند (فقط برای «کشف» استفاده می‌شوند).

### 0.3 نکته‌ی حیاتی درباره‌ی مسیرهای وب‌اپ

در کد Nuxt (`webapp/app/server/api/**`) مسیرهایی مثل `/api/companionReserve/BatchPayment` یا `/api/petresan/reservation` دیده می‌شوند — این‌ها مسیرهای **سرور Nuxt خودِ وب‌اپ** هستند (برای تزریق توکن از کوکی، Proxy کردن Idempotency-Key و غیره)، **نه** مسیر واقعی بک‌اند. در ادامه‌ی این سند همیشه مسیر واقعی و نهایی بک‌اند (.NET Controller) آمده؛ این همان مسیری‌ست که اپ فلاتر باید مستقیماً با Bearer Token خودش صدا بزند.

نمونه‌ی تناظر (برای رفع ابهام احتمالی):

| مسیر Proxy در وب‌اپ (فقط جهت اطلاع، استفاده نکنید) | مسیر واقعی بک‌اند (این را صدا بزنید) |
|---|---|
| `POST /api/cart/cart` | *(مربوط به فروشگاه، نه رزرو)* |
| `POST /api/companionReserve/enduserReserve` | `POST /api/EndUser/CompanionReserve` |
| `POST /api/companionReserve/enduserReserveBatch` | `POST /api/EndUser/CompanionReserveBatch` |
| `POST /api/companionReserve/ReservePayment` | `POST /api/EndUser/CompanionReservePayment` |
| `POST /api/companionReserve/BatchPayment` | `POST /api/EndUser/CompanionReserveBatchPayment` |
| `PUT /api/companionReserve/wallet` | `PUT /api/EndUser/CompanionReserveSetWallet` |
| `PUT /api/companionReserve/setRebate` | `PUT /api/EndUser/CompanionReserveSetRebate` |
| `PUT /api/companionReserve/removeRebate` | `PUT /api/EndUser/CompanionReserveRemoveRebate` |
| `GET /api/companionAssistancePackage/endUserPackages` | `GET /api/CompanionAssistancePackage` (ریشه، عمومی) |
| `GET /api/companionTime/endUserCompanionTime` | `GET /api/CompanionTime` (ریشه، عمومی) |

### 0.4 پایه‌ی آدرس‌ها

- API اصلی: سرویس `Api` (طبق `docs/ai`، پورت 5000 در محیط توسعه؛ آدرس Production را از تیم بک‌اند بگیرید — چیزی شبیه `https://api.pastil.pet`).
- Callback پرداخت روی یک **سرویس کاملاً جدا** اجرا می‌شود (`Payment`, پورت 5002) و آدرس پایه‌اش از تنظیمات `Urls:PaymentBaseUrl` می‌آید — این آدرس را هم از تیم بک‌اند بگیرید. اپ فلاتر مستقیماً این آدرس را صدا نمی‌زند؛ فقط لینکی که در پاسخ شروع پرداخت (`paymentUrl`) می‌آید را در WebView باز می‌کند (بخش ۱۱).

---

## 1. مفاهیم پایه و شناسه‌ها (Glossary)

### 1.1 Companion — «همراه» (مرکز/نماینده)

هر ارائه‌دهنده‌ی خدمت (کلینیک، مربی، آرایشگاه، آزمایشگاه، پرستار، داگ‌واکر) یک رکورد `Companion` است. فایل: `backend/Entities/Entities/CompanionField/Companion.cs`.

### 1.2 CompanionType — دسته‌بندی نوع همراه

هر Companion می‌تواند یک یا چند `CompanionType` (رکورد اتصال به جدول `Code`، گروه‌کد `CompanionType`) داشته باشد. این همان چیزی‌ست که «کلینیک / مربی / آرایشگاه» را از هم جدا می‌کند.

**⚠️ نکته‌ی بسیار مهم و گیج‌کننده:** نام‌های داخلی enum سمت بک‌اند (`Application/Common/Enumerable/Code/CompanionTypeEnum.cs`) با برچسب واقعی که وب‌اپ (و ظاهراً خودِ داده‌ی زنده‌ی سرور از طریق `/api/Companion/companionType`) استفاده می‌کند **یکی نیست**. نگاشت واقعی و قابل‌اعتماد (منبع: `webapp/app/composables/useCompanionType.ts`، که صراحتاً کامنت‌گذاری شده «مپ کامل... منبع: خروجی زنده‌ی API») این است:

| شناسه عددی (id) | نام نمایشی واقعی (فارسی) | نام Label واقعی | نام enum داخل کد C# بک‌اند (ممکن است گمراه‌کننده باشد) |
|---:|---|---|---|
| 40 | کلینیک | `CompanionType_Clinic` | `CompanionType_Clinic` ✅ یکی است |
| 41 | داگ واکری | `CompanionType_DogWalker` | `CompanionType_DogWalker` ✅ یکی است |
| 42 | آزمایشگاه | `CompanionType_Laboratory` | `CompanionType_Laboratory` ✅ یکی است |
| **43** | **مربی** | `CompanionType_Coach` | نام ثابت enum در کد: `CompanionType_Barber` ⚠️ **متفاوت است** |
| 44 | پرستار | `CompanionType_Nurse` | `CompanionType_Nurse` ✅ یکی است |
| **45** | **آرایشگاه (گرومینگ)** | `CompanionType_Grooming` | `CompanionType_Grooming` ✅ یکی است |
| 10023 | پانسیون (شناسه‌ی قدیمی، حفظ‌شده برای سازگاری) | `CompanionType_Pansion` | — |

**نتیجه برای درخواست مورد نظر شما (کلینیک/مربی/آرایشگاه):**
- کلینیک → `TypeId = 40`
- مربی → `TypeId = 43`
- آرایشگاه → `TypeId = 45`

توضیح ریشه‌ی گیج‌کنندگی: در سیستم قدیمی، شناسه‌ای که امروز «آرایشگاه/گرومینگ» (۴۵) نام دارد، قبلاً «Barber» نامیده می‌شد؛ وب‌اپ همین را به‌عنوان یک Alias نگه داشته (`'CompanionType_Barber' → 'CompanionType_Grooming'`، یعنی رشته‌ی Label قدیمی روی id=45 نگاشت می‌شود)، درحالی‌که به‌طور کاملاً مستقل، ثابت enum سمت بک‌اند برای id=43 هنوز اسمش `CompanionType_Barber` مانده ولی این عدد امروز واقعاً «مربی» است. **این یک ناسازگاری واقعی و اثبات‌شده در کد فعلی است، نه یک فرض.** تیم فلاتر باید همیشه بر اساس **عدد id** کار کند و نام‌های enum سمت بک‌اند را نادیده بگیرد؛ برای دریافت لیست زنده و قابل‌اعتماد این دسته‌ها:

```http
GET /api/CompanionType
GET /api/CompanionType/{id}
```

(کنترلر: `backend/Api/Controllers/CompanionTypeController.cs` — عمومی، بدون نیاز به توکن.)

### 1.3 CompanionAssistanceType — حالت ارائه‌ی خدمت (37 / 38 / 39)

این یک مفهوم کاملاً جدا از `CompanionType` است — مشخص می‌کند یک «خدمت» چطور ارائه می‌شود:

```csharp
// backend/Application/Common/Enumerable/Code/CompanionAssistanceTypeEnum.cs
CompanionAssistanceType_Online   = 37   // آنلاین (چت/تماس/ویدیوکال)
CompanionAssistanceType_InPerson = 38   // «در محل همراه» یعنی در مرکز/کلینیک خودشان (در UI: «در مرکز»)
CompanionAssistanceType_InPlace  = 39   // «در محل مشتری» یعنی نزد کاربر (در UI: «در محل»)
```

- برای `39` (در محل مشتری)، ارسال `addressId` **الزامی** است.
- هر `CompanionAssistance` (خدمتِ یک مرکز خاص) لیستی از این کدها را دارد (`companionAssistance.codes[]`) — فقط از میان همین لیست باید یکی را برای `companionAssistanceTypeId` انتخاب کرد؛ عدد ثابت یا حدس زده‌شده استفاده نکنید.

### 1.4 Assistance در برابر CompanionAssistance

- **`Assistance`** = تعریف عمومی یک خدمت (نام، توضیح، تصویر، گروه) — مستقل از هر مرکز خاص. کاتالوگ کلی: `GET /api/Assistance`.
- **`CompanionAssistance`** = رکورد اتصال «این مرکز خاص، این خدمت خاص را ارائه می‌دهد» — شامل `companionTypeId`، `isSinglePackage`، `commissionPercent`، `codes[]` (حالت‌های ارائه). همان چیزی‌ست که در فلوی رزرو با آن سروکار داریم و `companionAssistanceId` در همه‌جا به همین اشاره دارد.

### 1.5 CompanionAssistancePackage — پکیج قابل رزرو

هر خدمت یک یا چند «پکیج» قیمت‌گذاری‌شده دارد (مثلاً «اصلاح ساده» / «اصلاح + استحمام»). فیلدهای اصلی (نام‌های دقیق JSON، camelCase):

```json
{
  "id": 0,
  "name": "string",
  "price": 0,
  "prePaymentPrice": 0,
  "pictureId": 0,
  "active": true,
  "companionAssistanceId": 0,
  "discription": "string",
  "petSize": "Small | Medium | Large | null"
}
```

⚠️ نام فیلد توضیحات دقیقاً `discription` است (غلط املایی در خودِ کد بک‌اند حفظ شده — `description` ننویسید، جواب نمی‌دهد).

- `petSize`: اگر `null`/خالی باشد، پکیج برای همه‌ی سایزهای پت نمایش داده می‌شود. اگر مقدار داشته باشد، فقط برای پت‌هایی با همان `Size` نمایش داده می‌شود. مقادیر معتبر دقیقاً `Small` / `Medium` / `Large` (انگلیسی) هستند — همان مقادیری که هنگام ثبت پت در `UserPet.Size` ذخیره می‌شود.
- قیمت **به ازای هر پت ضرب می‌شود**، نه فرمول پیچیده‌تر: `PackagePrice = Σ(package.Price) × تعداد پت‌های انتخابی`. جدول قیمت جدای هر سایز وجود ندارد؛ فقط فیلتر نمایش بر اساس سایز است.
- `prePaymentPrice` مبلغی‌ست که واقعاً در لحظه‌ی رزرو باید پرداخت شود (پیش‌پرداخت)؛ ممکن است کمتر از `price` باشد.

### 1.6 CompanionTime در برابر CompanionAssistanceTime (بسیار مهم)

سیستم زمان‌بندی اخیراً از سطح «هر خدمت» به سطح «کل مرکز» تغییر کرده است:

- **`CompanionAssistanceTime`** (قدیمی): ساعات کاری به‌ازای هر خدمت جداگانه. **دیگر برای رزروهای جدید استفاده نمی‌شود**؛ فقط برای نمایش رزروهای تاریخی قدیمی نگه داشته شده.
- **`CompanionTime`** (جدید، فعلی): یک برنامه‌ی هفتگی مشترک برای کل مرکز (`CompanionId`)، مستقل از این‌که کدام خدمت رزرو می‌شود. **این چیزی است که باید استفاده کنید.**

Endpoint عمومی (بدون نیاز به توکن) برای گرفتن زمان‌های آزاد یک مرکز:

```http
GET /api/CompanionTime?CompanionId={companionId}&Active=true
```

پاسخ هر آیتم (`CompanionTimeVDto`) شامل `id`, `startTime` ("HH:mm")، `endTime`، `weekDay` (نام و شماره‌ی روز هفته)، `active` است.

فیلد نهایی که باید در بدنه‌ی ثبت رزرو ارسال شود: **`companionTimeId`** (نه `companionAssistanceTimeId`). هر دو فیلد در DTOهای رزرو هنوز وجود دارند (برای سازگاری با گذشته)، اما فقط `companionTimeId` روی رزروهای جدید عمل می‌کند.

### 1.7 CompanionReserve و CompanionReserveBatch

- **`CompanionReserve`**: یک رزرو برای یک خدمت مشخص.
- **`CompanionReserveBatch`**: «سبد» چند رزرو مستقل (هرکدام برای یک خدمت متفاوت) که همگی متعلق به **یک مرکز واحد** هستند و در یک تراکنش با هم ثبت می‌شوند تا با **یک پرداخت مشترک** تسویه شوند.

### 1.8 وضعیت‌های رزرو

```csharp
// CompanionReserveStateEnum — وضعیت مالی/کلی رزرو
CompanianReserveState_Registered = 30   // ثبت شده، هنوز پرداخت نشده
CompanianReserveState_PrePaid    = 31   // پیش‌پرداخت انجام شده (پرداخت موفق کالبک خورده)
CompanianReserveState_Paid       = 32   // تسویه‌ی نهایی انجام شده (توسط اپراتور/ادمین)
CompanianReserveState_Complete   = 33   // خدمت انجام و تکمیل شده

// CompanionReserveOperatorStateEnum — وضعیت عملیاتی از دید اپراتور/مرکز
OperatorState_InComplete = 34   // هنوز انجام نشده
OperatorState_Complete   = 35   // انجام شده
OperatorState_Cancelled  = 36   // لغو شده (توسط اپراتور)
```

فیلد `IsReserved` (بولین جدا) فقط بعد از پرداخت موفق (یا برای ادمین بلافاصله در لحظه‌ی ثبت) `true` می‌شود؛ تا قبل از آن رزرو «موقت/در انتظار پرداخت» است.

---

## 2. گام ۱ — کشف/جست‌وجوی مراکز (Discovery)

### 2.1 جست‌وجوی عمومی مراکز (فیلترپذیر، بدون نیاز به موقعیت مکانی)

```http
GET /api/Companion?TypeId={40|43|45}&Q={متن جستجو}&PageIndex=1&PageSize=20
```

کنترلر: `backend/Api/Controllers/CompanionController.cs` (عمومی). فیلترهای مهم (`CompanionInputDto`):

| فیلد | نوع | توضیح |
|---|---|---|
| `TypeId` | `long?` | همان دسته‌بندی ۱٫۲ (40 کلینیک / 43 مربی / 45 آرایشگاه) |
| `Q` | `string` | جست‌وجو در نام یا شماره تماس |
| `CityId` / `StateId` | `long?` | فیلتر جغرافیایی ساده (بدون فاصله) |
| `NeighborhoodIds` | `List<long>` | فیلتر محله |
| `AssistanceId` | `long?` | فقط مراکزی که این خدمت خاص را ارائه می‌دهند |
| `HasInsurance` | `bool?` | دارای بیمه فعال |
| `SortBy` | `SortEnum` (`New`/`Old`/`MorePriority`/`MoreSell`/`LessSell`) | ترتیب |
| `GoldAccount`/`SilverAccount` | `bool?` | فیلتر سطح اکانت (نتایج طلایی/نقره‌ای همیشه در اولویت نمایش‌اند) |

پاسخ: `{ totalCount, list: CompanionVDto[] }` به‌همراه اکوی همان فیلترها.

### 2.2 مراکز نزدیک (Nearby / رادار)

این Endpoint **نیازمند توکن** است و از موقعیت مکانیِ **ذخیره‌شده‌ی خودِ کاربر** استفاده می‌کند (نه هر مختصاتی که در همان لحظه پاس داده شود):

```http
GET /api/EndUser/Companion/Nearby?RadiusMeter=10000&TypeId=40&PageIndex=1&PageSize=50
Authorization: Bearer <token>
```

**پیش‌نیاز:** قبل از فراخوانی این Endpoint، باید موقعیت فعلی کاربر یک‌بار ثبت شده باشد:

```http
POST /api/EndUser/UserCurrentLocation
Authorization: Bearer <token>
Content-Type: application/json

{ "location": { "x": 51.4, "y": 35.7 } }
```

(دقت کنید: `x` = طول جغرافیایی/longitude، `y` = عرض جغرافیایی/latitude — برعکسِ ترتیب معمول lat/lng.)

اگر رکورد موقعیت برای کاربر وجود نداشته باشد، `GetNearby` با پیام «موردی یافت نشد» شکست می‌خورد — این خطا را به «لطفاً دسترسی موقعیت مکانی را فعال کنید» ترجمه کنید، چون واقعاً یعنی موقعیت ثبت نشده.

قواعد اعتبارسنجی ورودی: `RadiusMeter` باید بین ۱ تا ۵۰۰۰۰ باشد، `PageSize` حداکثر ۱۰۰ — در غیر این صورت خطای «دیتا استباه است» (بله، همین املا در متن اصلی رزورس فارسی موجود است).

پاسخ هر آیتم (`NearbyCompanionVDto`): `id, name, addressValue, phone, cityId, neighborhoodId, cityName, neighborhoodName, pictureId, picture, location, rateAvg, rateCount, isGold, isSilver, hasPansion, distanceMeter, hasServiceZone, isInServiceArea`.

نکته: `isInServiceArea` فقط وقتی مقدار غیرِ‌null دارد که مرکز اصلاً `CompanionZone` تعریف کرده باشد (`hasServiceZone=true`)؛ اگر `OnlyInServiceArea=true` در کوئری بدهید، مراکزی که خارج از محدوده‌ی فعالیتشان هستند از لیست کاملاً حذف می‌شوند.

### 2.3 کاتالوگ خدمات عمومی (برای صفحه‌ی «خدمات محبوب»)

```http
GET /api/Assistance?PageIndex=1&PageSize=20
```

فیلدهای هر آیتم: `id, name, summary, description, pictureId, picture, assistanceGroupId, isPersonal, showToSite, active`.

⚠️ این Endpoint فقط کاتالوگ عمومی خدمات را می‌دهد؛ برای این‌که مطمئن شوید یک خدمت واقعاً حداقل یک پکیج فعال دارد (و ارزش نمایش دارد)، باید جداگانه با `AssistanceId` روی `CompanionAssistance` و بعد `CompanionAssistancePackage` چک کنید (بخش ۳).

---

## 3. گام ۲ — صفحه‌ی پروفایل مرکز: خدمات، پکیج‌ها، انتخاب حالت ارائه

### 3.1 اطلاعات مرکز

```http
GET /api/Companion/{companionId}
```

### 3.2 لیست خدمات این مرکز

```http
GET /api/CompanionAssistance?CompanionId={companionId}
```

هر آیتم (`CompanionAssistanceVDto`):

```json
{
  "id": 0,
  "companionId": 0,
  "assistanceId": 0,
  "companionTypeId": 0,
  "isSinglePackage": true,
  "active": true,
  "approved": true,
  "commissionPercent": 0,
  "companion": { },
  "assistance": { "id": 0, "name": "...", "summary": "...", "pictureId": 0, "picture": {} },
  "codes": [ { "id": 38, "label": "CompanionAssistanceType_InPerson", "name": "..." } ]
}
```

- `codes[]` = لیست حالت‌های ارائه (37/38/39) که این خدمت پشتیبانی می‌کند. UI باید فقط از میان همین لیست، دکمه‌ی «در مرکز»/«در محل» بسازد.
- `isSinglePackage=true` یعنی کاربر فقط می‌تواند دقیقاً **یک** پکیج از این خدمت انتخاب کند (نه چندتایی).

### 3.3 انتخاب اجباری حالت ارائه (اگر از قبل مشخص نیست)

اگر کاربر مستقیم وارد صفحه‌ی مرکز شده (نه از یک لینک با `type` مشخص)، باید یک مودال اجباری نشان دهید: «در مرکز» یا «در محل» — و تا انتخاب نشود، لیست خدمات فیلترشده نمایش داده نشود. بعد از انتخاب، **این انتخاب برای کل جلسه‌ی رزرو ثابت می‌ماند** — دیگر دوباره نپرسید و خدماتی که این حالت را پشتیبانی نمی‌کنند اصلاً نمایش ندهید.

### 3.4 پکیج‌های یک خدمت

```http
GET /api/CompanionAssistancePackage?CompanionAssistanceId={id}&Available=true&PageSize=100
```

⚠️ **حتماً** `Available=true` را خودتان ارسال کنید — Endpoint ریشه (بدون Area) این را به‌صورت خودکار اعمال نمی‌کند و پکیج‌های غیرفعال را هم برمی‌گرداند. (اگر بعداً از توکن استفاده کردید و به `GET /api/EndUser/CompanionAssistancePackage` رفتید، آن‌جا `Available=true` به‌طور خودکار اعمال می‌شود، اما روی مسیر ریشه خیر.)

فیلتر اختیاری `PetSize=Small|Medium|Large` هم قابل ارسال است (پکیج‌های بدون سایز مشخص همیشه همراه نتیجه می‌آیند).

### 3.5 گزینه‌های آنلاین (فقط برای حالت 37)

اگر `companionAssistanceTypeId === 37` (آنلاین) انتخاب شده، ممکن است خدمت دارای «گزینه‌های آنلاین» (`CompanionAssistancePackageOnline`, مثل چت/تماس/ویدیوکال با قیمت‌های متفاوت) باشد که یکی از آن‌ها باید به‌عنوان `companionAssistancePackageOnlineSelectionId` در بدنه‌ی رزرو ارسال شود. اگر گزینه‌ی انتخابی «فوری» (`isInstant=true`) باشد، نیازی به انتخاب زمان (`companionTimeId`) نیست — سرور خودش `doDate` را روی همین لحظه تنظیم می‌کند.

---

## 4. گام ۳ — زمان‌بندی (ساعات کاری مرکز)

```http
GET /api/CompanionTime?CompanionId={companionId}&Active=true
```

- هر آیتم شامل `id, startTime, endTime, weekDay:{id,label,name}, active` است.
- **قانون هماهنگی روز:** روز هفته‌ی `doDate` انتخابی کاربر باید دقیقاً با `weekDay` همان اسلات زمانی یکی باشد؛ در غیر این صورت سرور با پیام «روز تاریخ رزرو با روز زمان انتخاب‌شده هماهنگ نیست.» رد می‌کند. با تغییر تاریخ در UI، اسلات زمانی قبلی را پاک و لیست را دوباره (بر اساس روز هفته‌ی جدید) فیلتر کنید.
- تاریخ/ساعت گذشته هرگز قابل انتخاب نباشد (هم در UI مسدود کنید، هم سرور دوباره چک می‌کند).
- **اگر مرکز اصلاً هیچ `CompanionTime` فعالی نداشته باشد**، انتخاب زمان اختیاری می‌شود (برخی مراکز هنوز ساعت کاری تعریف نکرده‌اند)؛ ولی اگر حداقل یک ردیف فعال وجود داشته باشد، انتخاب زمان **اجباری** می‌شود و نرسیدن `companionTimeId` با پیام «انتخاب زمان الزامی است.» رد می‌شود.

---

## 5. گام ۴‑الف — ثبت رزرو تکی (Single Reserve)

### 5.1 Endpoint

```http
POST /api/EndUser/CompanionReserve
Authorization: Bearer <token>
Content-Type: application/json
```

### 5.2 بدنه‌ی دقیق درخواست

```json
{
  "id": 0,
  "userPetIds": [2],
  "doDate": "2026-09-20T00:00:00+03:30",
  "companionAssistanceId": 7,
  "companionAssistancePackagesIds": [35],
  "companionAssistanceTypeId": 38,
  "companionTimeId": 30,
  "companionAssistanceUserId": null,
  "companionAssistancePackageOnlineSelectionId": null,
  "addressId": null,
  "isFemale": null,
  "bookerDetail": "",
  "assistanceDetail": ""
}
```

نکات فیلد به فیلد:

- **`bookerId` را ارسال نکنید** یا اگر ارسال کردید نادیده گرفته می‌شود — سرور همیشه از روی توکن کاربر جاری پر می‌کند (`dto.BookerId = currentUser.UserId`). هیچ‌وقت آن را مبنای امنیتی یا نمایشی قرار ندهید.
- `companionTimeId`: شناسه‌ی اسلات از بخش ۴. اگر مرکز ساعت کاری فعال ندارد، `null` بفرستید.
- `companionAssistanceTypeId`: باید دقیقاً یکی از `codes[]` همان خدمت باشد (بخش ۳.۲).
- `addressId`: فقط برای `companionAssistanceTypeId=39` الزامی است؛ در غیر این صورت `null`.
- `companionAssistanceUserId`: اختیاری — اگر کاربر یک متخصص/اپراتور خاص را از قبل انتخاب کرده (بخش «تخصیص رزرو»، `GET /api/CompanionAssistanceUser?CompanionAssistanceId={id}&Available=true`).
- `isFemale`: ترجیح جنسیتی مسئول انجام خدمت — اگر استفاده نمی‌کنید `null` بفرستید.
- **این فیلدها را هرگز خودتان محاسبه نکنید و اگر فرستادید معتبر نیست — قیمت نهایی همیشه در سرور محاسبه می‌شود:** `prePaymentPrice`, `paymentPrice`, `packagePrice`, `walletPrice`, `stateId`, `operatorStateId`, `isReserved`, `isCancel`.

### 5.3 ترتیب دقیق اعتبارسنجی سمت سرور (به همین ترتیب اجرا می‌شود)

منبع: `CompanionReserveService.InsertAsyncDto` (`backend/Application/Services/CompanionSrvs/CompanionReserveSrv/CompanionReserveService.cs:214-516`).

| # | شرط | پیام خطا (فارسی دقیق) |
|---|---|---|
| 1 | خطای اعتبارسنجی مدل (DataAnnotations) | متغیر بر اساس فیلد |
| 2 | `userPetIds` خالی | «حداقل یک نوع را انتخاب نمایید» |
| 3 | یکی از `userPetIds` متعلق به کاربر جاری نیست | «دیتا استباه است» |
| 4 | همین ترکیب (خدمت + کاربر + زمان) قبلاً رزرو فعال دارد | «قبلا رزرو شده است» |
| 5 | `companionAssistanceTypeId=39` ولی `addressId` خالی | «لطفا آدرس خود را وارد نمایید» |
| 6 | خدمت (`CompanionAssistance`) پیدا نشد/غیرفعال/تاییدنشده | «موردی یافت نشد» |
| 7 | `companionAssistanceTypeId` متعلق به این خدمت نیست | «نوع ارائه انتخاب‌شده متعلق به این خدمت نیست.» |
| 8 | گزینه‌ی آنلاین نامعتبر | «دیتا استباه است» |
| 9 | تاریخ رزرو گذشته است | «امکان رزرو خدمت در تاریخ گذشته وجود ندارد.» |
| 10‑الف | `companionTimeId` داده شده ولی متعلق به این مرکز نیست/غیرفعال | «زمان انتخاب‌شده متعلق به این مرکز نیست یا فعال نیست.» |
| 10‑ب | روز هفته با `doDate` هماهنگ نیست | «روز تاریخ رزرو با روز زمان انتخاب‌شده هماهنگ نیست.» |
| 10‑ج | ساعت شروع محاسبه‌شده نامعتبر | «ساعت شروع خدمت معتبر نیست.» |
| 10‑د | ساعت شروع در گذشته | «امکان رزرو خدمت در زمان گذشته وجود ندارد.» |
| 11 | `companionTimeId` خالی ولی مرکز حداقل یک زمان فعال دارد (و گزینه‌ی آنلاینِ فوری هم نیست) | «انتخاب زمان الزامی است.» |
| 12‑الف | اپراتور انتخابی متعلق به این خدمت نیست/غیرفعال | «اپراتور انتخاب‌شده متعلق به این خدمت نیست یا فعال نیست.» |
| 12‑ب | عضویت اپراتور در نمایندگی فعال/تاییدشده نیست | «عضویت اپراتور انتخاب‌شده در این نمایندگی فعال و تأییدشده نیست.» |
| 12‑ج | اپراتور در همین بازه‌ی زمانی رزرو دیگری دارد | «اپراتور انتخاب‌شده در این بازه زمانی رزرو فعال دیگری دارد.» |
| 13‑الف | آدرس متعلق به کاربر نیست | «دیتا استباه است» |
| 13‑ب | مرکز در محدوده‌ی شهر آدرس فعالیت ندارد | «این کلینیک در محدوده آدرس شما فعالیت ندارد» |
| 14 | پکیج‌ها خالی/نامعتبر/تعداد اشتباه (با توجه به `isSinglePackage`) | «حداقل یک نوع را انتخاب نمایید» یا «دیتا استباه است» |

### 5.4 پاسخ موفق

```json
{ "isSuccess": true, "code": 0, "messages": [], "data": { "id": 512, "prePaymentPrice": 250000, "paymentPrice": 250000, "stateId": 30, "isReserved": false, "...": "..." } }
```

- توجه: `isReserved` هنوز `false` است — پرداخت هنوز شروع نشده. **این متد به‌تنهایی پرداخت را شروع نمی‌کند.**
- `id` بازگشتی را برای مرحله‌ی پرداخت (بخش ۱۰) و پت‌رسان (بخش ۷) نگه دارید.

### 5.5 رفتار پس از ثبت (جلوگیری از ثبت تکراری)

ثبت دیتابیس **قبل از** ارسال SMS/Push نهایی می‌شود؛ شکست سرویس‌های اعلان باعث `isSuccess:false` نمی‌شود. پس اگر `isSuccess:true` گرفتید، رزرو **قطعاً ساخته شده** و نباید به‌خاطر تأخیر اعلان دوباره Submit کنید:

1. هنگام ارسال، دکمه‌ی ثبت را Disable کنید.
2. بعد از اولین پاسخ موفق، فرم را دیگر دوباره Submit نکنید.
3. کاربر را بدون Refresh به مرحله‌ی بعد (زمان‌بندی/پرداخت) هدایت کنید.

---

## 6. گام ۴‑ب — ثبت سبد رزرو چندخدمتی (Batch Reserve)

وقتی کاربر از چند خدمتِ **همان یک مرکز** به‌طور همزمان پکیج انتخاب می‌کند (فلوی جمع‌آوری در سبد + یک پرداخت واحد).

### 6.1 Endpoint

```http
POST /api/EndUser/CompanionReserveBatch
Authorization: Bearer <token>
Content-Type: application/json
```

### 6.2 بدنه

```json
{
  "items": [
    {
      "id": 0,
      "userPetIds": [2],
      "doDate": "2026-09-20T00:00:00+03:30",
      "companionAssistanceId": 7,
      "companionAssistancePackagesIds": [35],
      "companionAssistanceTypeId": 38,
      "companionTimeId": 30,
      "addressId": null,
      "isFemale": null,
      "bookerDetail": "",
      "assistanceDetail": ""
    },
    {
      "userPetIds": [2],
      "doDate": "2026-09-20T00:00:00+03:30",
      "companionAssistanceId": 9,
      "companionAssistancePackagesIds": [41],
      "companionAssistanceTypeId": 38,
      "companionTimeId": 30,
      "addressId": null
    }
  ]
}
```

هر آیتم داخل `items[]` دقیقاً همان ساختار و همان قوانین اعتبارسنجی بخش ۵.۳ را دارد (چون سرور واقعاً برای هر آیتم، همان تابع `InsertAsyncDto` را صدا می‌زند — منطق تکراری‌نویسی نشده).

### 6.3 قوانین اضافه‌ی مخصوص سبد

1. تمام آیتم‌های `items[]` باید `bookerId` یکسان داشته باشند (به‌طور خودکار همه از توکن پر می‌شوند، پس این همیشه برقرار است مگر باگ کلاینت).
2. تمام `companionAssistanceId`های داخل سبد باید به **یک `CompanionId` واحد** برسند — یعنی **نمی‌توانید در یک سبد، خدمات دو مرکز متفاوت را با هم رزرو کنید**. اگر تخطی شود: «دیتا استباه است».
3. همه‌ی آیتم‌ها در **یک تراکنش Serializable مشترک** ثبت می‌شوند: یا همه با هم موفق می‌شوند یا هیچ‌کدام (Rollback کامل روی اولین شکست). یعنی اگر آیتم سوم رد شود، آیتم‌های اول و دوم هم اصلاً در دیتابیس باقی نمی‌مانند — پیام خطای همان آیتمِ شکست‌خورده به کاربر برمی‌گردد.
4. اعلان‌ها (SMS/Push) فقط بعد از Commit موفق کل تراکنش ارسال می‌شوند.

### 6.4 پاسخ موفق

```json
{
  "isSuccess": true,
  "data": {
    "id": 88,
    "createDate": "...",
    "totalPrePaymentPrice": 480000,
    "totalPackagePrice": 480000,
    "items": [ { "id": 512, "...": "..." }, { "id": 513, "...": "..." } ]
  }
}
```

`data.id` همان `companionReserveBatchId` است — برای پرداخت (بخش ۱۰) لازم دارید. `data.items[*].id` هم برای پت‌رسان (اولین آیتم) و برای فراخوانی جداگانه‌ی کیف‌پول/تخفیف به‌ازای هر آیتم لازم است (بخش ۸).

### 6.5 خواندن یک سبد

```http
GET /api/EndUser/CompanionReserveBatch/{batchId}
```

---

## 7. پت‌رسان (Trip) — سرویس رفت‌وآمد پت به مرکز

### 7.1 نام واقعی در کد

⚠️ در کد بک‌اند و پنل، این قابلیت با نام انگلیسی **`Trip`** پیاده‌سازی شده، نه «PetResan». کنترلرها: `backend/Api/Areas/EndUser/Controllers/Trip*Controller.cs` و `backend/Api/Areas/Driver/Controllers/Trip*Controller.cs`. سند مرجع کامل: `backend/Docs/PETRASAN_APP_FRONTEND_FA.md`.

### 7.2 ⚠️ نکته‌ی بسیار ضدِ‌شهود — پت‌رسان برای کدام حالت ارائه است؟

برخلاف تصور اولیه (که شاید فکر کنید پت‌رسان برای «در محل» یعنی `39` باشد)، **پت‌رسان فقط برای حالت `38` (در مرکز) معنا دارد**، چون این سرویس، پت را **به** کلینیک می‌برد؛ وقتی خودِ خدمت `39` (در محل مشتری) است، اصلاً نیازی به جابه‌جایی پت نیست. این دقیقاً همان چیزی‌ست که پیاده‌سازی زنده‌ی وب‌اپ بررسی می‌کند:

```js
// finalize-reserve-batch.vue
const isInPersonBatch = computed(() => cartStore.entries.some((e) => Number(e.companionAssistanceTypeId) === 38));
```

پس گزینه‌ی «افزودن پت‌رسان» را فقط وقتی نمایش دهید که `companionAssistanceTypeId === 38` باشد.

### 7.3 توالی ایجاد

سفر پت‌رسان **بعد از** ایجاد رزرو (تکی یا سبد) ولی **قبل از** شروع پرداخت ساخته می‌شود — چون به `companionReserveId` نیاز دارد:

1. `POST /api/EndUser/CompanionReserve` یا `POST /api/EndUser/CompanionReserveBatch`
2. (اگر پت‌رسان فعال است) `POST /EndUser/TripReservation`
3. (تنظیم کیف‌پول/تخفیف در صورت نیاز)
4. `POST /api/EndUser/CompanionReservePayment` یا `CompanionReserveBatchPayment`

برای سبد رزرو، فقط **یک** سفر برای کل سبد ساخته می‌شود (نه یکی به‌ازای هر آیتم) و `companionReserveId` آن، شناسه‌ی **اولین** آیتم ساخته‌شده‌ی سبد است.

### 7.4 بدنه‌ی درخواست

```http
POST /EndUser/TripReservation
Authorization: Bearer <token>
```

```json
{
  "companionReserveId": 512,
  "origin": { "x": 51.38, "y": 35.70 },
  "destination": { "x": 51.42, "y": 35.75 },
  "fromAddress": "متن آدرس مبدا",
  "toAddress": "متن آدرس مقصد (آدرس مرکز)",
  "scheduledLeadMinutes": 60,
  "ownerRidesAlong": false,
  "roundTrip": false,
  "tripOptionIds": [],
  "userPetIds": [2]
}
```

قواعد:
- `scheduledLeadMinutes` فقط می‌تواند دقیقاً `60` یا `120` باشد؛ هر مقدار دیگر رد می‌شود با پیام: «فاصله‌ی زمانی حرکت راننده فقط می‌تواند ۶۰ یا ۱۲۰ دقیقه باشد.»
- هر `CompanionReserve` فقط می‌تواند یک `Trip` مرتبط داشته باشد — تلاش دوباره رد می‌شود.
- اگر برای زمان حرکت محاسبه‌شده، قیمت‌گذاری تعریف نشده باشد، قیمت صفر برمی‌گردد و کل درخواست رد می‌شود: «قیمت نهایی در دسترس نیست».
- اگر ایجاد سفر شکست بخورد، رزروی که قبلاً ساخته شده (هنوز پرداخت‌نشده) **Rollback نمی‌شود** — فقط خطا را به کاربر نشان دهید (مثلاً «ثبت پت‌رسان برای این رزرو انجام نشد.») و اجازه بدهید بدون پت‌رسان یا با تلاش مجدد ادامه دهد.

### 7.5 لغو سفر

```http
PUT /EndUser/TripUserCancel
```
```json
{ "id": 10, "cancelReasonCodeId": 3, "cancelDetail": "توضیح اختیاری" }
```

لیست دلایل لغو:
```http
GET /EndUser/Code?codeGroupLabel=TripCancelReason_User
```

بعد از تحویل‌گرفتن پت توسط راننده (`progressStageId >= 3`) دیگر قابل لغو نیست: «پت تحویل گرفته شده و سفر دیگر از این مسیر قابل لغو نیست.»

### 7.6 پیگیری وضعیت سفر

بدون SignalR (به‌خاطر مشکل ۵۰۴ قبلی) — فقط Polling، با بازه‌ی پیشنهادی ۳ تا ۵ ثانیه.

---

## 8. کیف پول و کد تخفیف

هر دو، **فیلدهایی روی خودِ رزرو** هستند که باید **قبل از فراخوانی پرداخت** با Endpointهای جدا تنظیم شوند (نه این‌که در بدنه‌ی پرداخت پاس داده شوند).

### 8.1 استفاده از کیف پول

```http
PUT /api/EndUser/CompanionReserveSetWallet
Authorization: Bearer <token>
```
```json
{ "id": 512, "fromWallet": true }
```

- برای سبد رزرو، این را **جداگانه برای هر `id` داخل `items[]`** صدا بزنید.
- قوانین رد شدن:
  - رزرو متعلق به کاربر جاری نیست → «موردی یافت نشد»
  - پرداختی برای این رزرو (یا اگر عضو یک سبد است، برای کل سبد) از قبل شروع شده → «پرداخت این رزرو شروع شده و اطلاعات مالی آن قابل تغییر نیست.»
  - رزرو در وضعیت «تکمیل‌شده» → «این رزرو تکمیل شده است»
  - رزرو در وضعیت «پرداخت‌شده» → «این رزرو پرداخت شده است»
- سقف واقعی مبلغ قابل‌کسر از کیف‌پول در لحظه‌ی شروعِ پرداخت محاسبه می‌شود (بخش ۱۰)، نه اینجا — اینجا صرفاً یک «درخواست استفاده از کیف پول» ثبت می‌کنید.

### 8.2 اعمال کد تخفیف

```http
PUT /api/EndUser/CompanionReserveSetRebate
```
```json
{ "id": 512, "rebateCode": "SUMMER20" }
```

قوانین رد شدن (پیام‌های دقیق):
- کد پیدا نشد/غیرفعال/متعلق به کاربر دیگر → «موردی یافت نشد»
- منقضی/شروع‌نشده → «مدت زمان استفاده از این کد تخفیف به اتمام رسیده است» یا «زمان استفاده از این کد تخفیف فرا نرسیده است»
- سقف استفاده‌ی کلی یا شخصی پر شده → «محدودیت تعداد استفاده از این کد تخفیف به اتمام رسیده است»
- حداقل مبلغ سبد رعایت نشده → «حداقل مبلغ سفارش باید {مبلغ} باشد»
- خارج از دامنه‌ی هدف باشگاه مشتریان مربوط به این کد → «امکان استفاده از این کد تخفیف نیست»
- رزرو قفل مالی دارد (پرداخت شروع شده) → همان پیام بخش ۸.۱

### 8.3 حذف کد تخفیف

```http
PUT /api/EndUser/CompanionReserveRemoveRebate?id=512
```

⚠️ توجه: این Endpoint پارامتر را به‌صورت **Query String ساده** می‌گیرد (`?id=512`)، **نه** یک بدنه‌ی JSON — چون امضای اکشن سمت سرور `Put(long id)` بدون DTO است.

### 8.4 محدودیت سبد رزرو نسبت به کد تخفیف

در پرداختِ **سبد**، کد تخفیف مشترکِ کل سبد پشتیبانی نمی‌شود (نسخه‌ی فعلی) — کد تخفیف فقط باید روی هر آیتم به‌صورت جداگانه (بخش ۸.۲، قبل از پرداخت) اعمال شود؛ جمع تخفیف‌های هر آیتم در محاسبه‌ی نهایی سبد لحاظ می‌شود.

---

## 9. انتخاب درگاه پرداخت (Merchant)

```http
GET /api/Merchant?PageIndex=1&PageSize=50
```

عمومی، بدون توکن، همیشه فقط درگاه‌های فعال برمی‌گرداند (سرور خودش `Available=true` را اجباری می‌کند). پاسخ هر آیتم:

```json
{ "id": 2, "bankId": 5, "active": true, "bank": { "name": "بانک ملت", "pictureId": 12, "picture": { } } }
```

`id` همین‌جا همان چیزی‌ست که در مرحله‌ی پرداخت به‌عنوان `merchantId` ارسال می‌کنید.

---

## 10. شروع پرداخت + قرارداد Idempotency-Key

### 10.1 دو Endpoint مجزا

| نوع رزرو | Endpoint |
|---|---|
| تکی | `POST /api/EndUser/CompanionReservePayment` |
| سبد | `POST /api/EndUser/CompanionReserveBatchPayment` |

### 10.2 بدنه‌ی درخواست

```http
Authorization: Bearer <token>
Idempotency-Key: <UUID v4>
Content-Type: application/json
```

برای تکی:
```json
{ "companionReserveId": 512, "merchantId": 2 }
```

برای سبد:
```json
{ "companionReserveBatchId": 88, "merchantId": 2 }
```

هیچ فیلد مبلغی (Amount, GrossAmount, ...) را خودتان پر نکنید — همه از روی رزرو/سبدِ ذخیره‌شده در دیتابیس (شامل تخفیف و کیف‌پولِ از قبل ثبت‌شده در بخش ۸) توسط سرور محاسبه می‌شود.

### 10.3 هدر Idempotency-Key — الزامی برای اپ فلاتر (حتی اگر سمت سرور اختیاری باشد)

- نام هدر دقیقاً: `Idempotency-Key`
- مقدار: یک UUID نسخه‌ی ۴ استاندارد (`Guid.TryParseExact(..., "D", ...)`, یعنی شکل `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`).
- **یک UUID تازه فقط زمانی بسازید که «هویت» درخواست تغییر کند** (رزرو/سبد متفاوت، مبلغ متفاوت، درگاه متفاوت). برای تلاش مجدد همان درخواست (مثلاً بعد از قطعی شبکه)، **همان UUID قبلی را دوباره بفرستید** — سرور اگر پرداختی با همین کلید و همین مشخصات پیدا کند، همان نتیجه‌ی قبلی را بدون ایجاد پرداخت تکراری برمی‌گرداند (Replay).
- کلید را فقط بعد از دریافت یک نتیجه‌ی **قطعی** (`isSuccess === true`) از حافظه/Storage پاک کنید؛ در صورت خطای شبکه یا timeout، کلید را نگه دارید و در تلاش بعدی همان را دوباره بفرستید.
- اگر همان کلید را با مشخصات متفاوت (کاربر/درگاه/مبلغ دیگر) دوباره بفرستید: «این Idempotency-Key قبلاً برای Checkout دیگری استفاده شده است.» — یعنی باگ کلاینت (کلید باید عوض می‌شد و نشده).
- اگر فرمت هدر معتبر UUID نباشد: «هدر Idempotency-Key باید یک UUID معتبر باشد.»

### 10.4 پاسخ

```json
{
  "isSuccess": true,
  "data": {
    "paymentId": 900,
    "paymentCode": "RSV-...",
    "isOnline": true,
    "paymentIsLink": true,
    "paymentUrl": "https://gateway.../start/...",
    "isTestMode": false
  }
}
```

سه حالت ممکن برای شاخه‌بندی نتیجه (دقیقاً همین ترتیب را چک کنید):

1. **`data.isTestMode === true`** → پرداخت آزمایشی، از قبل روی سرور تایید شده؛ نیازی به باز کردن هیچ صفحه‌ای نیست — فقط پیام موفقیت نشان دهید و کاربر را به لیست رزروها ببرید.
2. **`data.paymentIsLink === true` و `paymentUrl` موجود** → این حالت واقعی درگاه بانکی است؛ `paymentUrl` را در یک **WebView** باز کنید (بخش ۱۱).
3. در غیر این دو حالت (مثلاً مبلغ صفر چون کیف‌پول/تخفیف کل مبلغ را پوشش داده) → پرداخت داخلی/رایگان از قبل تسویه شده؛ مستقیماً پیام موفقیت نشان دهید.

---

## 11. Callback پرداخت و تشخیص اتمام در اپ موبایل (WebView)

### 11.1 مسیر Callback

```
GET {PaymentBaseUrl}/callback/{paymentId}?callbackToken={token}
```

این مسیر روی **سرویس جدای `Payment`** میزبانی می‌شود (نه سرویس اصلی `Api`) و توسط خودِ درگاه بانکی، بعد از تکمیل/لغو پرداخت توسط کاربر، صدا زده می‌شود. آدرس کامل آن از قبل داخل `paymentUrl` که در بخش ۱۰ گرفتید تعبیه شده — شما مستقیماً این URL را نمی‌سازید.

### 11.2 این یک صفحه‌ی HTML است، نه JSON و نه Deep Link

⚠️ نکته‌ی بسیار مهم برای معماری اپ موبایل: خروجی این Endpoint یک **صفحه‌ی HTML رندرشده در سرور** (فارسی، «پرداخت با موفقیت انجام شد» / «پرداخت شما ناموفق بود.») است، **نه** یک پاسخ JSON و **نه** یک ریدایرکت به یک Custom URL Scheme (مثل `pastil://...`). این صفحه یک دکمه‌ی ساده‌ی `<a href="...">بازگشت به اپ پاستیل</a>` دارد که به آدرس عمومی سایت (`Urls:BaseUrl`, یک لینک معمولی `https://`) اشاره می‌کند — **هیچ مکانیزم Deep Link یا `postMessage` رسمی/مستندشده‌ای در بک‌اند برای این مرحله وجود ندارد.**

### 11.3 استراتژی پیشنهادی برای اپ فلاتر

چون هیچ قرارداد رسمی Deep Link برای این صفحه وجود ندارد، روش عملی زیر را پیاده کنید:

1. `paymentUrl` را در یک WebView داخل اپ باز کنید.
2. روی `onNavigationRequest`/`onUrlChange` WebView گوش دهید و به‌دنبال یکی از این دو الگو باشید:
   - رسیدن به مسیر `**/callback/{paymentId}**` (یعنی پرداخت به مرحله‌ی نهایی رسیده و صفحه‌ی نتیجه رندر شده) — این را می‌توانید علامت «پایان یافتن فرایند پرداخت» در نظر بگیرید، حتی قبل از کلیک کاربر روی دکمه‌ی بازگشت.
   - یا رسیدن به آدرس پایه‌ی سایت (`Urls:BaseUrl`، همان که دکمه‌ی «بازگشت به اپ پاستیل» به آن می‌رود) — این قطعاً یعنی کاربر تعامل با صفحه‌ی نتیجه را تمام کرده.
3. به‌محض تشخیص یکی از این دو، WebView را ببندید.
4. **هرگز به محتوای HTML صفحه (متن موفقیت/شکست) اعتماد نکنید** — بلافاصله بعد از بستن WebView، وضعیت واقعی را از API خودتان بخوانید:
   ```http
   GET /api/EndUser/CompanionReserve/{reserveId}
   ```
   یا برای سبد:
   ```http
   GET /api/EndUser/CompanionReserveBatch/{batchId}
   ```
   و فقط بر اساس `isReserved` / `stateId` واقعیِ برگشتی از این Endpoint، نتیجه‌ی نهایی را به کاربر نشان دهید (همان قانون طلایی بخش ۰.۱: فقط `isSuccess`/فیلدهای واقعی داده، نه متن نمایشی).

این دقیقاً همان روشی است که باید در نبود یک قرارداد رسمی Deep Link به کار گرفته شود؛ اگر تیم بک‌اند بعداً یک Custom Scheme یا Universal Link برای این صفحه اضافه کرد، این سند باید به‌روزرسانی شود.

---

## 12. بعد از پرداخت: وضعیت‌ها، تخصیص اپراتور، لغو، ثبت نظر

### 12.1 اتفاقاتی که بلافاصله بعد از پرداخت موفق رخ می‌دهد (خودکار، سمت سرور)

منبع: `CompanionReservePaymentCallback` (`CompanionReserveService.cs:1345` به بعد). به‌ترتیب:

1. اگر از کیف‌پول استفاده شده بود، مبلغ واقعاً از کیف‌پول کاربر کسر می‌شود.
2. اگر کد تخفیف اعمال شده بود، شمارنده‌ی استفاده از آن کد افزایش می‌یابد.
3. `isReserved = true` و `stateId = CompanianReserveState_PrePaid (31)`.
4. کمیسیون مرکز/سایت محاسبه می‌شود.
5. امتیاز باشگاه مشتریان (اگر مبلغ کافی باشد) اضافه می‌شود.
6. اگر یک اپراتور/متخصص خاص از قبل انتخاب شده بود، برای او Push «رزرو جدید برای شما» ارسال می‌شود.

### 12.2 لیست/جزئیات رزروهای من

```http
GET /api/EndUser/CompanionReserve?PageIndex=1&PageSize=20&ReserveState={CurrentDays|Done|Expired}
GET /api/EndUser/CompanionReserve/{id}
```

فیلترهای مفید دیگر روی همین Endpoint: `CompanionAssistanceId`, `CompanionId`, `UserPetId`, `IsFemale`. `bookerId` را خودتان ست نکنید — سرور همیشه فقط رزروهای کاربر جاری را برمی‌گرداند.

مقادیر معتبر `ReserveState` (فیلتر منطقی، نه وضعیت خام رزرو): `CompanionReserveState_CurrentDays` (رزروهای پیشِ‌رو)، `CompanionReserveState_Done` (انجام‌شده)، `CompanionReserveState_Expired` (منقضی).

### 12.3 ویرایش محدود یک رزروِ از‌قبل‌ثبت‌شده

```http
PUT /api/EndUser/CompanionReserve
```
```json
{ "id": 512, "companionAssistanceTimeId": null, "isFemale": true, "userPetIds": [2, 5] }
```

⚠️ **محدودیت مهم:** این Endpoint فقط سه فیلد را واقعاً به‌روزرسانی می‌کند — `isFemale`, `userPetIds`, و فیلد **قدیمیِ** `companionAssistanceTimeId` (نه `companionTimeId` جدید!). یعنی امکان «تغییر زمان رزروِ جدید» از طریق این Endpoint وجود ندارد — اگر کاربر می‌خواهد زمان را عوض کند، باید رزرو را لغو/دوباره ثبت کند (و لغو، طبق ۱۲.۴، فقط از سمت ادمین ممکن است — این یک محدودیت واقعی محصول است، نه سهل‌انگاری در این سند).

### 12.4 ⚠️ لغو رزرو — فقط از سمت ادمین، نه کاربر نهایی

بر خلاف انتظار، **هیچ Endpoint خودکاری برای لغو رزرو توسط خودِ مشتری در اپ وجود ندارد.** تنها مسیر لغو (`UpdateCancelDto` در `CompanionReserveService.cs:1464`) صراحتاً پشت یک چک نقش ادمین قفل است:

```csharp
if (!isAdmin) return ... Resource.Notification.AccessDenied; // "شما به این بخش دسترسی ندارید"
```

اگر محصول نیاز به «لغو توسط کاربر» دارد، این باید به‌عنوان یک قابلیت جدید از تیم بک‌اند درخواست شود؛ در حال حاضر در اپ فلاتر دکمه‌ی «لغو رزرو» برای کاربر عادی نمایش ندهید مگر این‌که این Endpoint تغییر کند.

### 12.5 ثبت نظر بعد از تکمیل خدمت

بعد از تغییر وضعیت رزرو به «تکمیل‌شده»، سرور ۱۲ ساعت بعد یک Push فارسی زمان‌بندی می‌کند که کاربر را به مسیر `/reserve?reviewType=companion&reserveId={id}` می‌برد (این را به یک صفحه‌ی معادل در اپ فلاتر نگاشت کنید). بعد از کلیک، جزئیات رزرو را دوباره از API بگیرید (روی وضعیت داخل payload نوتیفیکیشن حساب نکنید) و فرم نظر را باز کنید:

```http
POST /api/EndUser/CompanionReserveComment
```
```json
{
  "companionReserveId": 512,
  "text": "توضیحات کاربر",
  "companionReserveCommentRates": [ { "assistanceQuestionnaireId": 10, "rate": 5 } ]
}
```

قوانین: فقط صاحبِ رزروِ رزرو‌شده (`isReserved=true`)، لغونشده و **تکمیل‌شده** مجاز به ثبت نظر است. همه‌ی پرسش‌های فعال همان خدمت باید دقیقاً یک‌بار با امتیاز ۱ تا ۵ ارسال شوند. خطای رایج: «ثبت نظر فقط برای رزرو تکمیل‌شده‌ی خودتان امکان‌پذیر است.»

### 12.6 تخصیص/تغییرِ اپراتور توسط صاحب مرکز (اطلاعات تکمیلی، خارج از اپ مشتری)

این بخش برای اپ فلاتر **مشتری نهایی** معمولاً کاربردی نیست (مربوط به پنل صاحب کسب‌وکار/اپ نمایندگی است)، اما برای درک کامل چرخه‌ی عمر رزرو آورده می‌شود: بعد از پرداخت موفق، صاحب مرکز از طریق `PUT /api/Companion/CompanionReserveAssign` یک کاربر نمایندگی را به رزرو تخصیص می‌دهد؛ آن کاربر سپس در مسیر `/operator` (اگر اپ فلاتر نقش «اپراتور/متخصص» هم دارد) رزروهای تخصیص‌یافته را می‌بیند: `GET /api/Operator/CompanionReserve`. جزئیات کامل در `backend/Docs/APP_COMPANION_RESERVE_ASSIGNMENT_FA.md`.

---

## 13. دیاگرام کامل توالی — رزرو تکی (بدون پت‌رسان)

```
کاربر                          اپ فلاتر                                    بک‌اند
  |                               |                                          |
  |  انتخاب دسته (کلینیک/مربی/آرایشگاه) |                                     |
  |------------------------------>| GET /api/Companion?TypeId=40|43|45 ------>|
  |                               |<----------------- لیست مراکز -------------|
  |  انتخاب مرکز                  |                                          |
  |------------------------------>| GET /api/Companion/{id} ----------------->|
  |                               | GET /api/CompanionAssistance?CompanionId=id ->|
  |  انتخاب «در مرکز»/«در محل»    |                                          |
  |------------------------------>| (فیلتر لیست خدمات بر اساس codes[])       |
  |  انتخاب خدمت                  | GET /api/CompanionAssistancePackage?CompanionAssistanceId=..&Available=true ->|
  |  انتخاب پکیج(ها) + پت         |                                          |
  |------------------------------>| GET /api/CompanionTime?CompanionId=id&Active=true ->|
  |  انتخاب تاریخ/ساعت            |                                          |
  |------------------------------>| POST /api/EndUser/CompanionReserve ------>|
  |                               |<---------- {id, prePaymentPrice, ...} ----|
  |  [اختیاری] فعال‌سازی پت‌رسان    | POST /EndUser/TripReservation ----------->|
  |  [اختیاری] فعال‌سازی کیف‌پول   | PUT /api/EndUser/CompanionReserveSetWallet ->|
  |  [اختیاری] کد تخفیف           | PUT /api/EndUser/CompanionReserveSetRebate ->|
  |  انتخاب درگاه بانکی           | GET /api/Merchant ----------------------->|
  |  زدن «پرداخت»                 | POST /api/EndUser/CompanionReservePayment |
  |                               | Header: Idempotency-Key: <uuid>          |
  |                               |<---------- {paymentUrl, ...} -------------|
  |                               | [باز کردن WebView با paymentUrl]         |
  |  پرداخت در درگاه بانک          |                                          |
  |                               | [WebView به /callback/{id} می‌رسد] ------>| (سرویس Payment)
  |                               | [بستن WebView]                           |
  |                               | GET /api/EndUser/CompanionReserve/{id} -->|
  |                               |<------------- isReserved:true ------------|
  |  نمایش نتیجه‌ی نهایی            |                                          |
```

---

## 14. دیاگرام کامل توالی — سبد رزرو چندخدمتی

```
کاربر                          اپ فلاتر                                    بک‌اند
  |                               |                                          |
  |  ورود به صفحه‌ی مرکز           | GET /api/CompanionAssistance?CompanionId=id ->|
  |  انتخاب «در مرکز»/«در محل»    |                                          |
  |  باز کردن خدمت ۱ (آکاردئون)   | GET /api/CompanionAssistancePackage?...=1 ->|
  |  انتخاب پکیج از خدمت ۱        |                                          |
  |  باز کردن خدمت ۲ (آکاردئون)   | GET /api/CompanionAssistancePackage?...=2 ->|
  |  انتخاب پکیج از خدمت ۲        |                                          |
  |  زدن «مشاهده‌ی سبد»            | (نمایش سبد محلی، جمع قیمت)                |
  |  انتخاب پت مشترک برای کل سبد   |                                          |
  |------------------------------>| GET /api/CompanionTime?CompanionId=id ->|
  |  انتخاب تاریخ/ساعت مشترک      |                                          |
  |------------------------------>| POST /api/EndUser/CompanionReserveBatch ->|
  |                               |<---- {id: batchId, items:[...], ...} ----|
  |  [اختیاری] پت‌رسان (فقط اگر یکی از آیتم‌ها type=38 باشد) | POST /EndUser/TripReservation (با companionReserveId اولین آیتم) ->|
  |  [اختیاری] کیف‌پول/تخفیف به‌ازای هر آیتم | PUT SetWallet / SetRebate (تکرار برای هر id در items) ->|
  |  انتخاب درگاه بانکی           | GET /api/Merchant ----------------------->|
  |  زدن «پرداخت»                 | POST /api/EndUser/CompanionReserveBatchPayment |
  |                               | Header: Idempotency-Key: <uuid>          |
  |                               |<---------- {paymentUrl, ...} -------------|
  |                               | [WebView → همان بخش ۱۱]                  |
  |                               | GET /api/EndUser/CompanionReserveBatch/{batchId} ->|
  |  نمایش نتیجه‌ی نهایی            |                                          |
```

---

## 15. پیوست الف — کاتالوگ کامل پیام‌های خطای فارسی

جدول زیر همه‌ی پیام‌های خطای فارسیِ واقعی (نه کلید Resource) که در این فلو ممکن است دریافت کنید را جمع می‌کند، تا در اپ فلاتر بتوانید (در صورت نیاز) روی متن دقیق آن‌ها منطق خاصی پیاده کنید (مثل هایلایت‌کردن فیلد مربوطه) — هرچند طبق قانون ۰.۱، نمایش پیش‌فرض همیشه باید همان `messages[0].item1` خام باشد.

| پیام دقیق فارسی | زمینه/معنا |
|---|---|
| «حداقل یک نوع را انتخاب نمایید» | پت، پکیج یا نوع ارائه انتخاب نشده |
| «دیتا استباه است» | ورودی نامعتبر عمومی (این املا — «استباه» به‌جای «اشتباه» — دقیقاً همان چیزی‌ست که در فایل منبع resx وجود دارد) |
| «قبلا رزرو شده است» | تلاش برای رزرو تکراری همان خدمت/زمان |
| «لطفا آدرس خود را وارد نمایید» | حالت ارائه ۳۹ بدون آدرس |
| «موردی یافت نشد» | خدمت/رزرو/کد تخفیف/آدرس پیدا نشد یا به کاربر تعلق ندارد |
| «نوع ارائه انتخاب‌شده متعلق به این خدمت نیست.» | `companionAssistanceTypeId` نامعتبر برای این خدمت |
| «امکان رزرو خدمت در تاریخ گذشته وجود ندارد.» | `doDate` گذشته |
| «زمان انتخاب‌شده متعلق به این مرکز نیست یا فعال نیست.» | `companionTimeId` نامعتبر |
| «روز تاریخ رزرو با روز زمان انتخاب‌شده هماهنگ نیست.» | ناهماهنگی روز هفته |
| «ساعت شروع خدمت معتبر نیست.» | خطای محاسبه‌ی زمان شروع |
| «امکان رزرو خدمت در زمان گذشته وجود ندارد.» | ساعت شروع محاسبه‌شده در گذشته |
| «انتخاب زمان الزامی است.» | مرکز ساعت کاری فعال دارد ولی زمانی انتخاب نشده |
| «اپراتور انتخاب‌شده متعلق به این خدمت نیست یا فعال نیست.» | `companionAssistanceUserId` نامعتبر |
| «عضویت اپراتور انتخاب‌شده در این نمایندگی فعال و تأییدشده نیست.» | عضویت اپراتور تایید نشده |
| «اپراتور انتخاب‌شده در این بازه زمانی رزرو فعال دیگری دارد.» | تداخل زمانی اپراتور |
| «این کلینیک در محدوده آدرس شما فعالیت ندارد» | خارج از `CompanionZone` |
| «پرداخت این رزرو شروع شده و اطلاعات مالی آن قابل تغییر نیست.» | تلاش برای تغییر کیف‌پول/تخفیف بعد از شروع پرداخت |
| «این رزرو تکمیل شده است» / «این رزرو پرداخت شده است» | تلاش برای تغییر رزروی که از این مراحل عبور کرده |
| «مدت زمان استفاده از این کد تخفیف به اتمام رسیده است» | کد تخفیف منقضی |
| «زمان استفاده از این کد تخفیف فرا نرسیده است» | کد تخفیف هنوز فعال نشده |
| «محدودیت تعداد استفاده از این کد تخفیف به اتمام رسیده است» | سقف مصرف کد تخفیف |
| «حداقل مبلغ سفارش باید {مبلغ} باشد» | شرط حداقل مبلغ سبد برای کد تخفیف |
| «امکان استفاده از این کد تخفیف نیست» | خارج از دامنه‌ی هدف باشگاه مشتریان |
| «مبلغ کمتر از {عدد} تومان قابل پرداخت نیست» | مبلغ کمتر از حداقل مجاز پرداخت آنلاین |
| «کاربر یافت نشد» | خطای داخلی (نباید در عمل رخ دهد اگر توکن معتبر است) |
| «لطفا درگاه بانکی را انتخاب نمایید» | `merchantId` خالی با مبلغ بیش از صفر و بدون کیف‌پول |
| «این Idempotency-Key قبلاً برای Checkout دیگری استفاده شده است.» | استفاده‌ی نادرست/تکراری از یک UUID با مشخصات متفاوت |
| «هدر Idempotency-Key باید یک UUID معتبر باشد.» | فرمت هدر اشتباه |
| «برای این مورد یک پرداخت فعال یا موفق وجود دارد.» | تلاش دوباره برای پرداخت روی هدفی که همین الان پرداخت فعال/موفق دارد |
| «فاصله‌ی زمانی حرکت راننده فقط می‌تواند ۶۰ یا ۱۲۰ دقیقه باشد.» | مقدار نامعتبر `scheduledLeadMinutes` در پت‌رسان |
| «قیمت نهایی در دسترس نیست» | نبود قیمت‌گذاری برای زمان محاسبه‌شده‌ی پت‌رسان |
| «پت تحویل گرفته شده و سفر دیگر از این مسیر قابل لغو نیست.» | تلاش دیرهنگام برای لغو سفر پت‌رسان |
| «ثبت نظر فقط برای رزرو تکمیل‌شده‌ی خودتان امکان‌پذیر است.» | ثبت نظر روی رزروی که هنوز تکمیل نشده |
| «شما به این بخش دسترسی ندارید» | تلاش کاربر عادی برای لغو رزرو (فقط ادمین مجاز است) |
| «عملیات ناموفق بود» | خطای عمومیِ Fallback — هر استثنای پیش‌بینی‌نشده در سرور دقیقاً همین متن را برمی‌گرداند، معمولاً بدون جزئیات بیشتر در `item2`. اگر این پیام را زیاد دیدید، مشکل را با تیم بک‌اند/لاگ سرور پیگیری کنید، چون خودِ متن هیچ سرنخی نمی‌دهد. |

---

## 16. پیوست ب — نکات و دام‌های مهم (Gotchas)

این‌ها نکاتی هستند که با خواندن سطحی کد یا مستندات قدیمی‌تر ممکن است اشتباه برداشت شوند — همه‌شان با خواندن مستقیم کد فعلی تایید شده‌اند:

1. **شناسه‌ی دسته‌بندی ۴۳ در نام enum بک‌اند «Barber» است ولی امروز واقعاً «مربی» است** (بخش ۱.۲). همیشه بر اساس عدد و لیست زنده‌ی `/api/CompanionType` کار کنید، نه نام‌های ثابت داخل کد C#.
2. **پت‌رسان برای حالت ۳۸ (در مرکز) است، نه ۳۹ (در محل)** — برعکسِ حدسِ اول (بخش ۷.۲).
3. **زمان‌بندی جدید (`CompanionTime`) در سطح مرکز است، نه در سطح خدمت** — فیلد `companionTimeId` را پر کنید، نه `companionAssistanceTimeId` (که فقط برای رزروهای بسیار قدیمی هنوز در DTO مانده).
4. **Endpoint ریشه‌ی `/api/CompanionAssistancePackage` خودش `Available=true` را اعمال نمی‌کند** — اگر این پارامتر را نفرستید، پکیج‌های غیرفعال هم برمی‌گردند.
5. **لغو رزرو توسط مشتری نهایی از طریق API فعلی ممکن نیست** — فقط ادمین می‌تواند لغو کند (بخش ۱۲.۴). این را در طراحی UI اپ لحاظ کنید (دکمه‌ی لغو برای کاربر عادی نمایش ندهید مگر این محدودیت برداشته شود).
6. **Endpoint ویرایش رزرو (`PUT /api/EndUser/CompanionReserve`) فقط سه فیلد را واقعاً تغییر می‌دهد**: `isFemale`, `userPetIds`, و فیلد قدیمی `companionAssistanceTimeId` — تغییر زمان رزروِ جدید از این مسیر ممکن نیست.
7. **کد تخفیف مشترک روی کل سبد رزرو پشتیبانی نمی‌شود** — فقط به‌ازای هر آیتم جداگانه، پیش از پرداخت.
8. **پیام «عملیات ناموفق بود» یک Fallback عمومی برای هر استثنای پیش‌بینی‌نشده است** و می‌تواند از چند نقطه‌ی کاملاً متفاوت در کد (نه فقط رزرو) بیاید؛ اگر زیاد دیدید، لزوماً به‌معنای یک باگ خاص و مشخص نیست — نیاز به لاگ سمت سرور برای تشخیص علت واقعی دارد.
9. **صفحه‌ی Callback پرداخت یک صفحه‌ی HTML سمت سرور است، نه JSON یا Deep Link رسمی** — استراتژی تشخیص در WebView طبق بخش ۱۱ پیاده‌سازی شود، و همیشه وضعیت نهایی از API خودتان (نه از متن HTML) خوانده شود.
10. **`x`/`y` در `PointDto` به‌ترتیب longitude/latitude هستند** (نه lat/lng) — در تمام Endpointهای مرتبط با موقعیت مکانی (`UserCurrentLocation`, `Trip` origin/destination) همین ترتیب رعایت شود.
11. **نام فیلد توضیحات پکیج دقیقاً `discription` است** (غلط املایی عمدی/میراثی در کد بک‌اند) — `description` جواب نمی‌دهد.
12. **مقادیر `PetSize`/`UserPet.Size` دقیقاً `Small`/`Medium`/`Large` (انگلیسی) هستند.**
13. **سبد رزرو فقط می‌تواند شامل خدمات یک مرکز واحد باشد** — تلاش برای ترکیب دو مرکز مختلف در یک سبد رد می‌شود.
14. **بعد از ثبت موفق رزرو (`isSuccess:true`)، هرگز به‌خاطر تاخیر SMS/Push دوباره Submit نکنید** — ثبت پایگاه‌داده مستقل و قبل از اعلان‌هاست.

---

*این سند در تاریخ ۲۰۲۶/۰۹/۱۴ با خواندن مستقیم کد فعلی `backend/` و `webapp/` تهیه شده است. با هر تغییر معماری در فلوی رزرو (خصوصاً تغییرات آینده در زمان‌بندی، پرداخت یا پت‌رسان)، این فایل باید به‌روزرسانی شود.*
