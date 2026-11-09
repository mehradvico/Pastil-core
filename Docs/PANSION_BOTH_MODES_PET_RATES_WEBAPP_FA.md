# وب‌اپ: پانسیون و مهد در یک مرکز + قیمت بر اساس پت و سایز

**تاریخ:** ۱۴۰۵/۰۸/۱۸ (2026-11-09)
**مخاطب:** توسعه‌دهنده‌ی وب‌اپ (`webapp/`)
**وضعیت:**

| بخش | وضعیت |
|---|---|
| دیتابیس (`PansionPetRates.sql`) | اجرا شده (2026-11-09) |
| بک‌اند Api و Payment | کد آماده و تست‌شده، **هنوز دیپلوی نشده**؛ قبل از تست از تیم بک‌اند بپرسید |
| پنل ادمین و اپ نماینده | کد آماده، منتشر نشده |
| وب‌اپ | **همین سند** |

مسیر فایل‌ها و شماره خط‌ها از مخزن در تاریخ بالا گرفته شده است. اگر فرق داشت، با نام تابع پیدا کنید.

---

## چرا این تغییر لازم شد

یک مرکز فقط می‌توانست پانسیون یا مهد باشد و یک قیمت داشت. ارائه‌دهنده‌ای که در یک مکان هم پانسیون و هم مهد داشت و برای سگ کوچک، سگ بزرگ و گربه قیمت جدا می‌گرفت، مجبور می‌شد برای هر حالت مرکز جدا بسازد. حالا همه‌ی این‌ها یک مرکز است.

---

## خلاصه‌ی کارها

| # | کار | فایل | اولویت |
|---|---|---|---|
| ۱ | باز کردن رزرو مرکز «هر دو» | `utils/pansionServices.ts` | **اجباری** |
| ۲ | قیمت درست پت قبل از ثبت رزرو | `finalize-reserve-[id].vue` | **اجباری** |
| ۳ | چک سایز پت و گرفتن سایز از کاربر | `ChooseTime.vue` | **اجباری** |
| ۴ | نوع خدمت از خود رزرو، نه از مرکز | `ReservationReviewPrompt.vue` | **اجباری** |
| ۵ | نمایش جدول قیمت‌ها و حذف نام تکراری پت | `pansion-[id].vue`، `ChooseTime.vue` | **اجباری** |
| ۶ | پروفایل نماینده: انتخاب هر دو خدمت | `companionProfile/pansion/insert[id].vue` | **اجباری** |
| ۷ | پروفایل نماینده: قیمت پت‌ها | `companionProfile/pansion/index.vue` + پراکسی PUT | پیشنهادی، ولی بخش خاموش‌کردن پت اجباری است |

---

## قرارداد API

### ۱) `isDaycare` روی پانسیون

| مقدار | معنی | قیمت‌های لازم |
|---|---|---|
| `false` | فقط پانسیون شبانه | `pansionPrice > 0` |
| `true` | فقط مهد ساعتی | `schoolPrice > 0` |
| `null` | **هر دو خدمت** | هر دو بیشتر از صفر |

- `null` قبلاً یعنی «رکورد قدیمی بدون نوع». ممکن است چنین رکوردی هنوز با یک قیمت صفر وجود داشته باشد. **هر خدمت فقط وقتی قابل رزرو است که قیمتش بیشتر از صفر باشد.**
- مرکز عفونی (`isInfectious = true`) نمی‌تواند `null` باشد و مهد ندارد.

### ۲) `pansionPets[]`

`GET /api/Pansion/{id}` این فهرست را برمی‌گرداند:

```json
{
  "isDaycare": null,
  "pansionPrice": 400000,
  "schoolPrice": 60000,
  "pansionPets": [
    { "id": 11, "petId": 1, "petSize": "Small", "pansionPrice": 300000, "schoolPrice": 50000, "pet": { "id": 1, "name": "سگ" } },
    { "id": 12, "petId": 1, "petSize": "Large", "pansionPrice": 500000, "schoolPrice": null, "pet": { "id": 1, "name": "سگ" } },
    { "id": 13, "petId": 2, "petSize": null, "pansionPrice": 250000, "schoolPrice": null, "pet": { "id": 2, "name": "گربه" } }
  ]
}
```

| فیلد | مقدار |
|---|---|
| `petSize` | `Small`، `Medium`، `Large`؛ `null` = همه‌ی سایزها |
| `pansionPrice` | قیمت هر شب؛ `null` = قیمت پایه‌ی مرکز |
| `schoolPrice` | قیمت هر ساعت مهد؛ `null` = قیمت پایه‌ی مرکز |

**یک نوع پت می‌تواند چند ردیف داشته باشد.** هر کدی که فرض می‌کند هر `petId` یک ردیف دارد باید عوض شود.

> **توجه:** فهرست `GET /api/Pansion` مقدار `pansionPets` را **برنمی‌گرداند** و فقط جزئیات آن را دارد. پس روی کارت فهرست همان قیمت پایه نمایش داده می‌شود. اگر «از ... تومان» لازم شد، از بک‌اند فیلد جدا بخواهید.

### ۳) منطق انتخاب قیمت

سرور مبلغ نهایی را می‌سازد؛ این منطق فقط برای نمایش قبل از ثبت و پیام‌های زودهنگام است.

1. `pansionPets` خالی است: همه‌ی پت‌ها با قیمت پایه پذیرفته می‌شوند.
2. ردیفی با `petId` پت نیست: پت پذیرفته نمی‌شود.
3. ردیف هم‌سایز با پت.
4. وگرنه ردیف `petSize = null`.
5. وگرنه سایز پذیرفته نمی‌شود. **پت بدون سایز** هم فقط با ردیف `petSize = null` می‌خواند.

بعد از انتخاب ردیف، اگر قیمت ردیف برای آن خدمت `null` یا صفر بود، قیمت پایه‌ی مرکز حساب می‌شود.

### ۴) ثبت رزرو

`POST /api/EndUser/PansionReserve` عوض نشده است. روی مرکز `null` باید **دقیقاً یکی** از این دو شکل را بفرستید:

```json
{ "pansionId": 1, "userPetId": 5, "fromDate": "2026-11-20T00:00:00", "toDate": "2026-11-22T00:00:00" }
```

```json
{ "pansionId": 1, "userPetId": 5, "schoolCreateDate": "2026-11-20T00:00:00", "startTime": "09:00", "endTime": "13:00" }
```

پاسخ موفق `price`، `paymentPrice`، `dayCount` و `hourCount` دارد. مبلغ هر پت می‌تواند متفاوت باشد.

### ۵) پیام‌های خطای رزرو

مثل همیشه HTTP 200 با `isSuccess: false` و متن در `messages[0].item1`.

| پیام | حالت |
|---|---|
| این مرکز نوع حیوان انتخاب‌شده را پذیرش نمی‌کند. | نوع پت مجاز نیست |
| این مرکز سایز پت شما را پذیرش نمی‌کند. اگر سایز پت در پروفایلش ثبت نشده، اول آن را تکمیل کنید. | **جدید** |
| این مرکز خدمت انتخاب‌شده را ارائه نمی‌دهد. | **جدید**؛ قیمت آن خدمت صفر است |

---

## ۱) `app/utils/pansionServices.ts`

الان مرکز `null` قابل رزرو نیست و `offersDaycare` رکورد قدیمی بدون قیمت مهد را هم مهد نشان می‌دهد. این بخش‌ها را جایگزین کنید:

```ts
/** مهد دارد؟ null فقط با قیمت مهد */
export const offersDaycare = (pansion: PansionLike): boolean =>
  pansion?.isDaycare === true || (pansion?.isDaycare == null && asNumber(pansion?.schoolPrice) > 0)

/** پانسیون شبانه دارد؟ رکورد قدیمی null در فهرست شبانه می‌ماند ولی بدون قیمت رزرو نمی‌شود */
export const offersOvernight = (pansion: PansionLike): boolean => pansion?.isDaycare !== true

export const canReservePansionMode = (pansion: PansionLike, mode: PansionBookingMode): boolean => {
  if (!isPansionPublished(pansion)) return false
  return mode === 'daycare'
    ? pansion?.isDaycare !== false && asNumber(pansion?.schoolPrice) > 0
    : pansion?.isDaycare !== true && asNumber(pansion?.pansionPrice) > 0
}
```

`pansionModeOf` بعد از این تغییر در این فایل استفاده نمی‌شود. اگر جای دیگری هم استفاده نشده، حذفش کنید. توضیح خط ۱۹ را هم اصلاح کنید.

تابع کمکی قیمت را همین‌جا اضافه کنید تا همه‌جا یکسان باشد:

```ts
export type PetLike = { petId?: number; pet?: { id?: number }; size?: string | null }

export const matchPansionPetRate = (pansion: PansionLike, pet: PetLike) => {
  const rows = (pansion?.pansionPets as any[]) || []
  if (!rows.length) return { status: 'accepted' as const, rate: null }

  const petId = Number(pet?.petId ?? pet?.pet?.id)
  const ofPet = rows.filter((row) => Number(row?.petId ?? row?.pet?.id) === petId)
  if (!ofPet.length) return { status: 'pet' as const, rate: null }

  const size = String(pet?.size || '').toLowerCase()
  const rate = (size && ofPet.find((row) => String(row?.petSize || '').toLowerCase() === size))
    || ofPet.find((row) => !row?.petSize)
  return rate ? { status: 'accepted' as const, rate } : { status: 'size' as const, rate: null }
}

export const pansionUnitPrice = (pansion: PansionLike, mode: PansionBookingMode, rate: any): number =>
  mode === 'daycare'
    ? asNumber(rate?.schoolPrice) || asNumber(pansion?.schoolPrice)
    : asNumber(rate?.pansionPrice) || asNumber(pansion?.pansionPrice)
```

## ۲) `app/pages/pansion/finalize-reserve-[id].vue`

`unitPrice` (خط ۱۶۲) یک عدد مشترک برای همه‌ی پت‌هاست و `estimatedTotalPrice` (خط ۲۰۷) آن را در تعداد پت ضرب می‌کند. بعد از ثبت رزرو، `displayTotalPrice` درست است چون از `price` سرور جمع می‌زند، ولی برآورد قبل از ثبت اشتباه می‌شود.

- `estimatedTotalPrice` را به جمع قیمت هر پت تغییر دهید: برای هر پت انتخاب‌شده `pansionUnitPrice(...) × duration`.
- `unitPrice` فعلاً به قیمت خدمت دیگر هم fallback می‌کند (`sPrice > 0 ? sPrice : pPrice`). این fallback را حذف کنید؛ قیمت مهد و پانسیون قابل جایگزینی نیستند.
- اگر ردیف قیمت‌ها را جدا برای هر پت نشان می‌دهید، نام پت و قیمت واحدش را هم بنویسید.

## ۳) `app/components/reserve/ChooseTime.vue`

- `acceptedPetTypes` (خط ۴۳۴) نام‌ها را از همه‌ی ردیف‌ها می‌سازد و با چند ردیف نام تکراری می‌شود (`سگ، سگ، گربه`). نام‌ها را با `new Set` یکتا کنید.
- `petNotAccepted` و `togglePet` فقط نوع را چک می‌کنند. با `matchPansionPetRate` حالت `size` را هم بگیرید:
  - پت سایز دارد ولی پذیرفته نمی‌شود: پیام خطا و انتخاب نشود.
  - پت سایز ندارد: همان باکس انتخاب سایز صفحه‌ی رزرو کلینیک را نشان دهید (`clinic-[id]/reserve-[assistanceId].vue`، تابع `setPetSize` و `petSizeChoices`) و با `PUT /api/pet/userPet` روی پت ذخیره کنید.
- انتخابگر خدمت برای `isDaycare === null` از قبل هست (خط ۱۱۳). اگر فقط یکی از قیمت‌ها وجود دارد، با `canReservePansionMode` همان خدمت را نشان دهید و حالت پیش‌فرض را هم از خدمت قابل رزرو بگیرید (الان در `openModal` خط ۷۰۱ پیش‌فرض `pansion` است).

## ۴) نوع خدمت رزرو

برای مرکز `null` مقدار `reserve.pansion.isDaycare` دیگر نوع خدمت رزرو را نمی‌گوید. نوع را از خود رزرو بخوانید: اگر `schoolCreateDate` یا `startTime` داشت مهد است.

- `components/reserve/ReservationReviewPrompt.vue` خط ۲۲۲ را اصلاح کنید؛ الان مهدِ مرکز «هر دو» را «پانسیون» می‌نویسد.
- `pages/pansion/reserve-[id].vue` خط ۱۲۹ با `null` درست کار می‌کند، ولی بهتر است با قاعده‌ی بالا یکسان شود.

## ۵) `app/pages/pansion/pansion-[id].vue`

- `admissionFacts` (خط ۴۰۵): نام پت‌ها را یکتا کنید.
- یک بخش «قیمت‌ها» زیر حیوانات مجاز اضافه کنید. فقط وقتی ردیفی قیمت اختصاصی دارد نمایش داده شود:

| پت | سایز | هر شب | هر ساعت مهد |
|---|---|---|---|
| سگ | کوچک | ۳۰۰٬۰۰۰ | ۵۰٬۰۰۰ |
| گربه | همه | ۲۵۰٬۰۰۰ | قیمت پایه |

ستون هر خدمت را فقط وقتی نشان دهید که مرکز آن خدمت را دارد. برچسب‌ها: `Small` کوچک، `Medium` متوسط، `Large` بزرگ.

- عنوان‌های «معرفی» و «قوانین» (خط ۷۰ و ۸۵ و ۱۷۳) با `null` کلمه‌ی «پانسیون» را نشان می‌دهند. برای مرکز «هر دو» متن عمومی‌تر مثل «مرکز» بگذارید.

## ۶) پروفایل نماینده: `companionProfile/pansion/insert[id].vue`

- دو دکمه‌ی پانسیون و مهد (خط ۱۰۶ و ۱۱۴) را چندانتخابی کنید: هر دو روشن = `null`، فقط مهد = `true`، فقط پانسیون = `false`.
- خط ۳۳۱ (`typeof isDaycare !== 'boolean'`) مقدار `null` را رد می‌کند و باید برداشته شود؛ به‌جایش حداقل یک خدمت را اجبار کنید.
- خط ۳۳۵: برای هر خدمت روشن، قیمتش را بسنجید.
- خط ۳۴۵ تا ۳۴۹: فقط قیمت خدمت خاموش صفر شود.
- وقتی مرکز عفونی است، مهد را خاموش و غیرقابل انتخاب کنید.
- هنگام بارگذاری مرکز `null`: هر خدمتی که قیمتش بیشتر از صفر است روشن شود.

## ۷) پروفایل نماینده: `companionProfile/pansion/index.vue`

**اجباری:** `togglePet` (خط ۳۲۴) با `findIndex` فقط اولین ردیف را حذف می‌کند. برای خاموش‌کردن نوع پت، **همه‌ی ردیف‌های آن `petId`** را حذف کنید.

**پیشنهادی:** برای هر پت روشن، ردیف‌های قیمت و دکمه‌ی افزودن و ویرایش بگذارید.

| کار | درخواست به بک‌اند |
|---|---|
| فهرست | `GET /api/Companion/PansionPet?PansionId=1` |
| افزودن | `POST /api/Companion/PansionPet` |
| ویرایش سایز و قیمت | `PUT /api/Companion/PansionPet` |
| حذف | `DELETE /api/Companion/PansionPet?id=11` |

```json
{ "id": 11, "pansionId": 1, "petId": 1, "petSize": "Large", "pansionPrice": 500000, "schoolPrice": null }
```

- پراکسی `server/api/pansion/pansionPet.put.js` وجود ندارد و باید ساخته شود.
- بررسی کنید `pansionPet.get.js` مقدار `PansionId` و `pansionPet.post.js` فیلدهای جدید را به بک‌اند می‌رسانند.
- ترکیب تکراری پت و سایز روی یک مرکز رد می‌شود (`مقدار تکراری است`).
- تغییر قیمت پت‌ها مرکز را به بررسی ادمین برنمی‌گرداند.

`PartnerApplicationForm.vue` فعلاً تک‌انتخابی می‌ماند؛ درخواست همکاری با یک خدمت ثبت می‌شود و خدمت دوم بعداً اضافه می‌شود.

---

## چک‌لیست تست

- [ ] مرکز `false` و `true` مثل قبل رزرو می‌شوند.
- [ ] مرکز `null` با هر دو قیمت: هر دو خدمت دیده و رزرو می‌شوند.
- [ ] رکورد قدیمی `null` با قیمت مهد صفر: مهد نشان داده نمی‌شود و رزرو نمی‌شود.
- [ ] سگ کوچک و سگ بزرگ هر کدام قیمت خودشان را می‌گیرند.
- [ ] رزرو هم‌زمان دو پت با قیمت متفاوت: برآورد قبل از ثبت با `price` سرور یکی است.
- [ ] پت بدون سایز روی مرکز سایزدار: سایز پرسیده می‌شود و بعد رزرو انجام می‌شود.
- [ ] سایز متوسط روی مرکزی که فقط کوچک و بزرگ دارد: پیام رد نمایش داده می‌شود.
- [ ] مرکزی که ردیف «همه‌ی سایزها» دارد: پت بدون سایز رزرو می‌شود.
- [ ] نظرسنجی بعد از مهدِ مرکز «هر دو»، عنوان «مهد پت» دارد.
- [ ] نام پت‌ها در صفحه‌ی مرکز تکراری نیست.
- [ ] خاموش‌کردن یک نوع پت در پروفایل نماینده همه‌ی ردیف‌هایش را حذف می‌کند.

## ترتیب انتشار

وب‌اپ را هم‌زمان یا بلافاصله بعد از Api منتشر کنید. **تا انتشار وب‌اپ، مرکز «هر دو» را فعال نکنید و قیمت اختصاصی پت نگذارید**؛ وگرنه مرکز `null` رزرو نمی‌شود و برآورد قبل از ثبت با مبلغ نهایی فرق می‌کند.
