# مستند کامل کارت بانکی کاربر برای فرانت اپ پاستیل

این سند قرارداد واقعی بک‌اند فعلی برای ثبت و مدیریت کارت بانکی در `app.pastil.pet` است و برای پیاده‌سازی توسط امیرمحسن نوشته شده است.

موضوع این بخش «کارت بانکی کاربر» است، نه درگاه پرداخت. کارت ثبت‌شده برای شناسایی حساب مقصد و فرایندهای مالی مانند تسویه استفاده می‌شود و اطلاعات CVV2، تاریخ انقضا یا رمز کارت در هیچ مرحله‌ای نباید دریافت شوند.

---

## ۱. خلاصه جریان صحیح

1. کاربر وارد صفحه کارت‌های بانکی می‌شود.
2. فرانت لیست کارت‌های متعلق به کاربر جاری را از بک می‌گیرد.
3. کاربر نام صاحب کارت، شماره کارت و در صورت نیاز شماره شبا را وارد می‌کند.
4. فرانت ارقام فارسی و عربی را به انگلیسی تبدیل و فاصله و خط تیره را حذف می‌کند.
5. بانک از روی ۶ رقم اول کارت تشخیص داده می‌شود.
6. اطلاعات برای بک ارسال می‌شود.
7. کارت با وضعیت «در انتظار تأیید» ثبت می‌شود.
8. ادمین کارت را تأیید یا همراه با توضیح رد می‌کند.
9. فقط کارت تأییدشده باید در فرم‌های تسویه قابل انتخاب باشد.
10. ویرایش اطلاعات کارت، تأیید قبلی را باطل می‌کند و کارت باید دوباره بررسی شود.

فرانت نباید بانک، وضعیت تأیید یا `userId` را به‌عنوان داده قابل اعتماد تعیین کند؛ تصمیم نهایی با بک است.

---

## ۲. آدرس‌ها و احراز هویت

```text
API_BASE_URL=https://api.pastil.pet
```

تمام Endpointهای `EndUser/UserBankCard` نیازمند لاگین هستند:

```http
Authorization: Bearer {accessToken}
Accept: application/json
Content-Type: application/json
```

Endpoint فهرست بانک‌ها در Area عمومی قرار دارد:

```http
GET /api/Common/BankCard
```

---

## ۳. ساختار پاسخ‌های بک

### پاسخ عملیات

ثبت، ویرایش، حذف و دریافت جزئیات معمولاً با `BaseResult` برمی‌گردند:

```json
{
  "isSuccess": true,
  "messages": [],
  "code": 0,
  "data": {}
}
```

خطای منطقی ممکن است با HTTP Status برابر `200` برگردد:

```json
{
  "isSuccess": false,
  "messages": [
    {
      "item1": "متن قابل نمایش به کاربر",
      "item2": ""
    }
  ],
  "code": 0,
  "data": null
}
```

پس موفقیت را فقط از Status Code تشخیص ندهید:

```ts
function assertSucceeded<T>(response: BaseResult<T>): T {
  if (response?.isSuccess !== true) {
    throw new Error(
      response?.messages?.[0]?.item1 || 'عملیات ناموفق بود',
    )
  }

  return response.data
}
```

### پاسخ لیست

Endpointهای جستجو مستقیماً پاسخ صفحه‌بندی‌شده می‌دهند:

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "q": null,
  "sortBy": 1,
  "available": null,
  "totalCount": 2,
  "list": []
}
```

---

## ۴. مدل‌های TypeScript پیشنهادی

```ts
export type BankCardCatalogItem = {
  id: number
  bankName: string
  cardPrefix: string
}

export type UserBankCard = {
  id: number
  userId: number
  cardNumber: string
  shebaNumber: string | null
  bankCardId: number
  cardHolderName: string
  createDate: string
  lastUpdateDate: string
  approved: boolean
  adminDetail: string | null
  bankCard: BankCardCatalogItem | null
  user?: unknown
}

export type UserBankCardPayload = {
  id?: number
  cardNumber: string
  shebaNumber: string | null
  bankCardId: number
  cardHolderName: string
}

export type BaseResult<T> = {
  isSuccess: boolean
  code: number
  messages: Array<{
    item1: string
    item2: string
  }>
  data: T
}

export type PagedResult<T> = {
  pageIndex: number
  pageSize: number
  q: string | null
  sortBy: number
  available: boolean | null
  totalCount: number
  list: T[]
}
```

---

## ۵. فهرست کامل Endpointها

| Method | Endpoint | کاربرد |
|---|---|---|
| `GET` | `/api/Common/BankCard` | فهرست بانک‌ها و پیش‌شماره کارت‌ها |
| `GET` | `/api/EndUser/UserBankCard` | فهرست کارت‌های کاربر لاگین‌شده |
| `GET` | `/api/EndUser/UserBankCard/{id}` | جزئیات یک کارت |
| `POST` | `/api/EndUser/UserBankCard` | ثبت کارت جدید |
| `PUT` | `/api/EndUser/UserBankCard` | ویرایش کارت |
| `DELETE` | `/api/EndUser/UserBankCard?id={id}` | حذف نرم کارت |

مسیر مدیریتی تأیید کارت که اپ EndUser نباید آن را صدا بزند:

```http
PUT /api/Admin/UserBankCardApprove
```

---

## ۶. دریافت بانک‌ها و تشخیص بانک کارت

```http
GET /api/Common/BankCard?pageIndex=1&pageSize=500&sortBy=3
```

جستجو با نام بانک:

```http
GET /api/Common/BankCard?q=ملت&pageIndex=1&pageSize=100
```

نمونه پاسخ:

```json
{
  "pageIndex": 1,
  "pageSize": 500,
  "q": null,
  "sortBy": 3,
  "totalCount": 36,
  "list": [
    {
      "id": 1,
      "bankName": "بانک ملت",
      "cardPrefix": "610433"
    }
  ]
}
```

هر رکورد یک BIN یا پیش‌شماره ۶ رقمی است. ممکن است یک بانک چند پیش‌شماره و در نتیجه چند رکورد داشته باشد.

تشخیص بانک در فرانت:

```ts
function detectBank(
  cardNumber: string,
  catalog: BankCardCatalogItem[],
) {
  const normalized = normalizeCardNumber(cardNumber)

  if (normalized.length < 6) return null

  return catalog.find(
    item => item.cardPrefix === normalized.slice(0, 6),
  ) ?? null
}
```

نکات UI:

- انتخاب دستی بانک لازم نیست؛ بانک از شماره کارت تشخیص داده شود.
- تا قبل از ورود ۶ رقم، Placeholder بانک نمایش داده شود.
- اگر بعد از ۶ رقم بانک پیدا نشد، پیام «پیش‌شماره کارت در سیستم پشتیبانی نمی‌شود» نمایش داده شود.
- DTO بانک لوگو ندارد. لوگو باید از Assetهای فرانت و با Map روی نام یا پیش‌شماره بانک نمایش داده شود.
- وجود Bank در فرانت به معنی ثبت موفق نیست؛ بک دوباره آن را کنترل می‌کند.

---

## ۷. دریافت کارت‌های کاربر جاری

```http
GET /api/EndUser/UserBankCard?pageIndex=1&pageSize=20&sortBy=1
Authorization: Bearer {token}
```

Controller مقدار `userId` را از توکن می‌گیرد و هر `userId` ارسالی در Query را بازنویسی می‌کند. بنابراین فرانت نباید User ID را برای این لیست ارسال کند.

فیلتر وضعیت:

```http
GET /api/EndUser/UserBankCard?approved=true&pageIndex=1&pageSize=50
```

مرتب‌سازی:

| `sortBy` | معنی |
|---:|---|
| `1` | جدیدترین |
| `2` | قدیمی‌ترین |

نمونه آیتم پاسخ:

```json
{
  "id": 18,
  "userId": 42,
  "cardNumber": "6104331234567890",
  "shebaNumber": "IR050170000000123456789012",
  "bankCardId": 1,
  "cardHolderName": "مهراد حسینی",
  "createDate": "2026-08-16T14:30:00+03:30",
  "lastUpdateDate": "2026-08-16T14:30:00+03:30",
  "approved": false,
  "adminDetail": null,
  "bankCard": {
    "id": 1,
    "bankName": "بانک ملت",
    "cardPrefix": "610433"
  }
}
```

صفحه خالی باید Empty State داشته باشد:

```text
هنوز کارت بانکی ثبت نکرده‌اید.
برای دریافت تسویه، یک کارت به نام خودتان ثبت کنید.
```

---

## ۸. ثبت کارت بانکی

```http
POST /api/EndUser/UserBankCard
Authorization: Bearer {token}
Content-Type: application/json
```

Payload پیشنهادی:

```json
{
  "cardNumber": "6104331234567890",
  "shebaNumber": "IR050170000000123456789012",
  "bankCardId": 1,
  "cardHolderName": "مهراد حسینی"
}
```

نکات مهم Payload:

- `userId` ارسال نشود؛ بک آن را از Token تعیین می‌کند.
- `approved` ارسال نشود؛ بک همیشه کارت جدید را تأییدنشده ثبت می‌کند.
- `id` در POST ارسال نشود یا صفر باشد.
- `bankCardId` را از بانک تشخیص‌داده‌شده بفرستید؛ بااین‌حال بک در POST بانک را دوباره از ۶ رقم اول پیدا و مقدار را جایگزین می‌کند.
- `shebaNumber` اختیاری است و در صورت خالی‌بودن `null` ارسال شود، نه رشته شامل فاصله.
- نام صاحب کارت قبل از ارسال Trim شود.

بک هنگام ثبت:

1. خالی‌نبودن نام صاحب کارت را بررسی می‌کند.
2. خالی‌نبودن شماره کارت را بررسی می‌کند.
3. تمام کاراکترهای غیردیجیت شماره کارت را حذف می‌کند.
4. طول شماره کارت را دقیقاً ۱۶ رقم بررسی می‌کند.
5. بانک را از ۶ رقم اول در جدول `BankCards` پیدا می‌کند.
6. شبا را به فرمت `IR` به‌علاوه ۲۴ رقم تبدیل می‌کند.
7. تکراری‌نبودن شماره کارت را بررسی می‌کند.
8. `approved=false` ثبت می‌کند.
9. Notice ثبت کارت جدید را برای ادمین ایجاد می‌کند.

نمونه پاسخ موفق:

```json
{
  "isSuccess": true,
  "messages": [],
  "code": 0,
  "data": {
    "id": 18,
    "userId": 42,
    "cardNumber": "6104331234567890",
    "shebaNumber": "IR050170000000123456789012",
    "bankCardId": 1,
    "cardHolderName": "مهراد حسینی",
    "approved": false
  }
}
```

بعد از موفقیت:

- فرم بسته شود.
- Toast موفقیت نمایش داده شود.
- لیست کارت‌ها از سرور Refresh شود.
- وضعیت کارت «در انتظار تأیید» نمایش داده شود.
- از Push دستی آیتم در آرایه، بدون Refresh، پرهیز شود چون پاسخ POST مدل کامل `bankCard` را ندارد.

---

## ۹. نرمال‌سازی ورودی‌ها

کاربر ممکن است اعداد فارسی، عربی، فاصله یا خط تیره وارد کند. قبل از اعتبارسنجی و ارسال، همه‌چیز Normalize شود.

```ts
export function toEnglishDigits(value = '') {
  return value
    .replace(/[۰-۹]/g, digit =>
      String('۰۱۲۳۴۵۶۷۸۹'.indexOf(digit)),
    )
    .replace(/[٠-٩]/g, digit =>
      String('٠١٢٣٤٥٦٧٨٩'.indexOf(digit)),
    )
}

export function normalizeCardNumber(value = '') {
  return toEnglishDigits(value).replace(/\D/g, '').slice(0, 16)
}

export function normalizeSheba(value = '') {
  const normalized = toEnglishDigits(value)
    .toUpperCase()
    .replace(/\s|-/g, '')
    .replace(/^IR/, '')
    .replace(/\D/g, '')
    .slice(0, 24)

  return normalized ? `IR${normalized}` : null
}

export function formatCardNumber(value = '') {
  return normalizeCardNumber(value)
    .replace(/(.{4})/g, '$1-')
    .replace(/-$/, '')
}

export function maskCardNumber(value = '') {
  const card = normalizeCardNumber(value)

  if (card.length !== 16) return card

  return `${card.slice(0, 4)}-${card.slice(4, 6)}**-****-${card.slice(12)}`
}

export function maskSheba(value = '') {
  const sheba = normalizeSheba(value)

  if (!sheba) return ''

  return `${sheba.slice(0, 6)}••••••••••••••${sheba.slice(-4)}`
}
```

در Input شماره کارت بهتر است مقدار خام فقط Digit باشد و ظاهر چهارتایی با CSS یا Computed نمایش داده شود. `type="number"` استفاده نشود؛ چون کنترل طول، صفرهای احتمالی و رفتار کیبورد را خراب می‌کند.

پیشنهاد Input:

```html
<input
  type="text"
  inputmode="numeric"
  autocomplete="cc-number"
  maxlength="19"
  dir="ltr"
/>
```

برای شبا:

```html
<input
  type="text"
  inputmode="text"
  autocomplete="off"
  maxlength="26"
  dir="ltr"
/>
```

---

## ۱۰. اعتبارسنجی فرانت

### شماره کارت

- اجباری
- بعد از Normalize دقیقاً ۱۶ رقم
- بانک از ۶ رقم اول قابل تشخیص باشد
- الگوریتم Checksum کارت نیز در فرانت بررسی شود

```ts
export function isValidIranianCardNumber(value: string) {
  const card = normalizeCardNumber(value)

  if (card.length !== 16 || /^(\d)\1{15}$/.test(card)) {
    return false
  }

  const sum = [...card].reduce((total, digit, index) => {
    let result = Number(digit) * (index % 2 === 0 ? 2 : 1)
    if (result > 9) result -= 9
    return total + result
  }, 0)

  return sum % 10 === 0
}
```

### شماره شبا

- اختیاری
- اگر وارد شد، دقیقاً `IR` به‌علاوه ۲۴ رقم
- Checksum استاندارد IBAN در فرانت بررسی شود

```ts
export function isValidIranianSheba(value: string) {
  const sheba = normalizeSheba(value)

  if (!sheba || !/^IR\d{24}$/.test(sheba)) return false

  const rearranged = `${sheba.slice(4)}1827${sheba.slice(2, 4)}`
  let remainder = 0

  for (const digit of rearranged) {
    remainder = (remainder * 10 + Number(digit)) % 97
  }

  return remainder === 1
}
```

### نام صاحب کارت

- اجباری
- Trim شده
- پیشنهاد UI: حداقل ۳ و حداکثر ۱۰۰ کاراکتر
- فقط نام درج‌شده روی حساب بانکی وارد شود

اعتبارسنجی فرانت فقط برای UX است. بک باید مرجع امنیتی و اعتبارسنجی نهایی باقی بماند.

---

## ۱۱. وضعیت کارت در UI

مدل فعلی فقط `approved` و `adminDetail` دارد. نگاشت وضعیت پیشنهادی:

| شرط | وضعیت UI | رنگ |
|---|---|---|
| `approved === true` | تأییدشده | سبز |
| `approved === false && !adminDetail` | در انتظار تأیید | زرد/نارنجی |
| `approved === false && adminDetail` | ردشده | قرمز |

```ts
export function getBankCardStatus(card: UserBankCard) {
  if (card.approved) {
    return { key: 'approved', title: 'تأییدشده' }
  }

  if (card.adminDetail?.trim()) {
    return { key: 'rejected', title: 'ردشده' }
  }

  return { key: 'pending', title: 'در انتظار تأیید' }
}
```

قواعد نمایش:

- توضیح رد ادمین فقط برای کارت ردشده نمایش داده شود.
- کارت Pending یا Rejected برای تسویه قابل انتخاب نباشد.
- کارت Approved با Badge سبز و دکمه انتخاب نمایش داده شود.
- پس از ویرایش، کارت دوباره Pending می‌شود.
- وضعیت را در LocalStorage نگه ندارید؛ هر بار از سرور دریافت شود.

چرخه مدیریتی:

```text
ثبت کاربر → approved=false → بررسی ادمین
                            ├─ تأیید → approved=true و adminDetail=null
                            └─ رد → approved=false و adminDetail=توضیح ادمین
ویرایش کاربر → approved=false → بررسی مجدد ادمین
```

Payload ادمین برای اطلاع از قرارداد، نه استفاده در اپ:

```json
{
  "id": 18,
  "approved": false,
  "adminDetail": "شماره شبا با صاحب کارت مطابقت ندارد."
}
```

در رد کارت، `adminDetail` برای ادمین اجباری است.

---

## ۱۲. دریافت جزئیات کارت

```http
GET /api/EndUser/UserBankCard/18
Authorization: Bearer {token}
```

پاسخ شامل `bankCard` و اطلاعات وضعیت است. برای ورود به صفحه جزئیات، ID فقط باید از لیست کارت‌های همان کاربر گرفته شود.

در UI جزئیات:

- نام بانک
- لوگوی بانک از Assetهای فرانت
- شماره کارت Mask شده
- شبا Mask شده
- نام صاحب کارت
- وضعیت
- توضیح رد ادمین
- تاریخ ثبت و آخرین تغییر

شماره کامل فقط هنگام ویرایش و بعد از اقدام صریح کاربر نمایش داده شود. در حالت عادی Mask شود.

---

## ۱۳. ویرایش کارت

```http
PUT /api/EndUser/UserBankCard
Authorization: Bearer {token}
Content-Type: application/json
```

این Endpoint از نوع PUT کامل است، نه PATCH. همه فیلدهای اصلی را ارسال کنید:

```json
{
  "id": 18,
  "cardNumber": "6104331234567890",
  "shebaNumber": "IR050170000000123456789012",
  "bankCardId": 1,
  "cardHolderName": "مهراد حسینی"
}
```

نکات:

- `id` اجباری است.
- `userId` را ارسال نکنید؛ Controller آن را از توکن تعیین می‌کند.
- `approved` را ارسال نکنید؛ تغییر کارت تأیید را باطل می‌کند.
- مقدار فعلی `bankCardId` باید همراه Payload باقی بماند.
- بعد از موفقیت، لیست از سرور Refresh شود.
- وضعیت جدید باید Pending نمایش داده شود.
- خطای `امکان ویرایش وجود ندارد` به‌صورت Inline یا Toast قابل فهم نمایش داده شود.

بک فعلی قصد دارد EndUser فقط یک‌بار کارت را ویرایش کند؛ ادمین از این محدودیت مستثنا است. محدودیت واقعی فعلی در بخش «ایرادهای بک» توضیح داده شده است.

---

## ۱۴. حذف کارت

```http
DELETE /api/EndUser/UserBankCard?id=18
Authorization: Bearer {token}
```

حذف Soft Delete است. پس از موفقیت:

- Modal تأیید حذف بسته شود.
- کارت از لیست Refresh شود.
- اگر کارت انتخاب‌شده فرم دیگری بوده، انتخاب پاک شود.

متن Modal:

```text
حذف کارت بانکی
آیا از حذف کارت ****-7890 مطمئن هستید؟
```

اگر کارت در رکورد تسویه استفاده شده باشد، حذف ممکن است به‌علت وابستگی دیتابیس ناموفق شود. در این حالت متن `messages[0].item1` نمایش داده شود و کارت بدون پاسخ موفق از UI حذف نشود.

---

## ۱۵. طراحی پیشنهادی صفحه

### Header

- عنوان: «کارت‌های بانکی»
- توضیح: «مدیریت کارت‌های مقصد برای دریافت تسویه»
- دکمه اصلی: «افزودن کارت بانکی»

### کارت لیست

- لوگو و نام بانک
- شماره Mask شده با `dir=ltr`
- نام صاحب کارت
- شبا Mask شده
- Badge وضعیت
- توضیح رد در صورت وجود
- دکمه ویرایش
- دکمه حذف

### فرم افزودن و ویرایش

ترتیب فیلدها:

1. شماره کارت
2. بانک تشخیص‌داده‌شده، فقط Read-only
3. نام و نام خانوادگی صاحب کارت
4. شماره شبا، اختیاری
5. توضیح حریم خصوصی
6. دکمه ذخیره

متن حریم خصوصی:

```text
اطلاعات کارت فقط برای شناسایی حساب مقصد و انجام تسویه استفاده می‌شود.
رمز، CVV2 و تاریخ انقضای کارت را در این فرم وارد نکنید.
```

رفتار فرم:

- Validation هر فیلد زیر همان Input باشد.
- دکمه ذخیره تا معتبرشدن فرم غیرفعال باشد.
- هنگام Submit دکمه Loading داشته و Double Submit بسته شود.
- بعد از خطای سرور، مقادیر فرم پاک نشوند.
- پیام دقیق بک نمایش داده شود.

---

## ۱۶. Composable پیشنهادی Nuxt

```ts
export function useUserBankCards() {
  const api = useApi()

  const getBanks = (params = {}) =>
    api.get<PagedResult<BankCardCatalogItem>>(
      '/api/Common/BankCard',
      { params },
    )

  const getCards = (params = {}) =>
    api.get<PagedResult<UserBankCard>>(
      '/api/EndUser/UserBankCard',
      { params },
    )

  const getCard = (id: number) =>
    api.get<BaseResult<UserBankCard>>(
      `/api/EndUser/UserBankCard/${id}`,
    )

  const createCard = (payload: UserBankCardPayload) =>
    api.post<BaseResult<UserBankCard>>(
      '/api/EndUser/UserBankCard',
      payload,
    )

  const updateCard = (
    payload: Required<Pick<UserBankCardPayload, 'id'>> & UserBankCardPayload,
  ) => api.put<BaseResult<unknown>>(
    '/api/EndUser/UserBankCard',
    payload,
  )

  const deleteCard = (id: number) =>
    api.delete<BaseResult<unknown>>(
      '/api/EndUser/UserBankCard',
      { params: { id } },
    )

  return {
    getBanks,
    getCards,
    getCard,
    createCard,
    updateCard,
    deleteCard,
  }
}
```

اگر Wrapper پروژه امضای متفاوتی برای `delete` یا `params` دارد، با ساختار موجود پروژه هماهنگ شود؛ URL و Method بالا قرارداد بک هستند.

---

## ۱۷. نگهداری State و Cache

- فهرست بانک‌ها داده عمومی و کم‌تغییر است و می‌تواند برای چند ساعت Cache شود.
- لیست کارت‌های کاربر بعد از POST، PUT، DELETE یا بازگشت از Background Refresh شود.
- اطلاعات کامل کارت در LocalStorage، IndexedDB، Pinia Persist یا Log ذخیره نشود.
- فقط ID کارت انتخاب‌شده را در State موقت فرم نگه دارید.
- هنگام Logout تمام State کارت‌ها پاک شود.
- در SSR پاسخ کارت کاربر بین Requestهای مختلف Cache مشترک نشود.

کلیدهای پیشنهادی Query:

```ts
['bank-card-catalog']
['user-bank-cards', filters]
['user-bank-card', id]
```

---

## ۱۸. امنیت و حریم خصوصی

- CVV2، رمز اول، رمز پویا و تاریخ انقضا هیچ‌وقت دریافت نشوند.
- شماره کامل کارت یا شبا در Analytics، Sentry breadcrumb، Console یا Network logger اختصاصی ثبت نشود.
- در UI عمومی شماره کارت و شبا Mask شوند.
- Clipboard برای کپی شماره کامل فقط با اقدام صریح کاربر فعال شود.
- Screenshot protection در وب قابل تضمین نیست؛ اطلاعات غیرضروری نمایش داده نشود.
- ID کارت از Route قابل اعتماد نیست و بک باید مالکیت را بررسی کند.
- `approved` فقط از پاسخ سرور خوانده شود.
- کارت تأییدنشده فقط در صفحه مدیریت کارت دیده شود و در انتخاب مقصد تسویه قرار نگیرد.
- اعتبارسنجی فرانت جای اعتبارسنجی و Authorization بک را نمی‌گیرد.
- پاسخ `401` با Refresh Token مدیریت و در صورت شکست کاربر به Login منتقل شود.
- روی `403` پیام عدم دسترسی نمایش داده شود.

---

## ۱۹. پیام‌های خطای قابل انتظار

| پیام بک | معنی و رفتار UI |
|---|---|
| `لطفا مشخصات کامل دارنده کارت را وارد نمایید` | خطای زیر فیلد نام صاحب کارت |
| `لطفا شماره کارت را وارد نمایید` | خطای زیر فیلد شماره کارت |
| `شماره کارت وارد حتما باید 16 رقم باشد` | شماره Normalize شده ۱۶ رقم نیست |
| `متاسفانه کارت وارد شده معتبر نیست` | پیش‌شماره بانک در دیتابیس وجود ندارد |
| `شماره شبا باید 24 رقم باشد` | بخش عددی شبا ۲۴ رقم نیست |
| `مقدار تکراری است` | این شماره کارت قبلاً ثبت شده است |
| `امکان ویرایش وجود ندارد` | محدودیت ویرایش کارت |
| `موردی یافت نشد` | کارت حذف شده یا ID معتبر نیست |
| `شما به این بخش دسترسی ندارید` | کارت متعلق به کاربر جاری نیست |

Fallback:

```ts
const errorMessage =
  response?.messages?.[0]?.item1 ||
  error?.data?.message ||
  'در ارتباط با سرور خطایی رخ داد.'
```

---

## ۲۰. نکات مهم و ایرادهای فعلی بک‌اند

این موارد از روی کد فعلی استخراج شده‌اند. فرانت نباید برای آن‌ها Workaround ناامن بسازد؛ این‌ها باید در بک اصلاح شوند:

1. `GET /api/EndUser/UserBankCard/{id}` در سرویس فعلی مالکیت کارت را با کاربر لاگین‌شده محدود نمی‌کند. IDOR و نشت اطلاعات کارت ممکن است رخ دهد.
2. `DELETE /api/EndUser/UserBankCard?id=` نیز قبل از حذف مالکیت کارت را بررسی نمی‌کند.
3. شرط ویرایش EndUser بر اساس برابر بودن دقیق `LastUpdateDate` و `CreateDate` است؛ این دو هنگام ثبت با دو `DateTime.Now` جدا مقدار می‌گیرند و ممکن است از ابتدا برابر نباشند. بنابراین ویرایش می‌تواند بلافاصله با «امکان ویرایش وجود ندارد» شکست بخورد.
4. مسیر Update برخلاف Insert، شماره کارت را Normalize و ۱۶ رقمی‌بودن، BIN بانک، تکراری‌بودن و نام صاحب کارت را به‌طور کامل دوباره اعتبارسنجی نمی‌کند.
5. بعد از ویرایش، `approved=false` می‌شود اما `adminDetail` قبلی پاک نمی‌شود؛ در نتیجه کارت ارسال‌شده برای بررسی مجدد ممکن است همچنان در UI ردشده دیده شود.
6. فیلد `bankCardId` در DTO جستجوی کارت‌های کاربر وجود دارد اما سرویس Search فعلی آن را اعمال نمی‌کند.
7. بک فعلی فقط طول و BIN کارت را بررسی می‌کند و Checksum شماره کارت را کنترل نمی‌کند.
8. بک فعلی برای شبا فقط طول را بررسی می‌کند و Checksum استاندارد IBAN را کنترل نمی‌کند.
9. پاسخ API شماره کامل کارت و شبا را برمی‌گرداند؛ بهتر است DTO عمومی نسخه Mask شده داشته باشد و اطلاعات کامل فقط در مسیر امن و ضروری ارائه شود.
10. مدل فعلی فیلد «کارت پیش‌فرض» ندارد. فرانت نباید Default Card ساختگی و دائمی در LocalStorage ایجاد کند.
11. هنگام ساخت تسویه باید در بک نیز مالکیت و `Approved=true` بودن کارت کنترل شود؛ مخفی‌کردن کارت تأییدنشده در فرانت به‌تنهایی کافی نیست.

تا قبل از اصلاح موارد ۱ و ۲، فقط IDهای دریافت‌شده از لیست خود کاربر استفاده شوند؛ بااین‌حال این اقدام فرانت، جای اصلاح Authorization بک را نمی‌گیرد.

---

## ۲۱. سناریوهای تست End-to-End

### ثبت موفق

1. کاربر لاگین کند.
2. فهرست بانک‌ها دریافت شود.
3. شماره کارت معتبر وارد شود.
4. بانک بعد از رقم ششم نمایش داده شود.
5. نام صاحب کارت و شبا وارد شوند.
6. POST فقط یک بار ارسال شود.
7. `isSuccess=true` دریافت شود.
8. لیست Refresh و کارت Pending نمایش داده شود.

### ورودی فارسی

1. شماره کارت با ارقام فارسی وارد شود.
2. فرانت آن را به ارقام انگلیسی تبدیل کند.
3. Payload فقط شامل ارقام انگلیسی باشد.

### شماره کارت نامعتبر

1. شماره کمتر از ۱۶ رقم باشد؛ Request ارسال نشود.
2. Checksum نامعتبر باشد؛ خطای Inline نمایش داده شود.
3. BIN ناشناخته باشد؛ بانک پیدا نشود و Submit غیرفعال بماند.

### شبا

1. شبا خالی باشد؛ `null` ارسال و ثبت مجاز باشد.
2. شبا با `IR` وارد شود؛ فقط یک `IR` در Payload وجود داشته باشد.
3. شبا با اعداد فارسی وارد شود؛ به انگلیسی تبدیل شود.
4. Checksum نامعتبر باشد؛ Request ارسال نشود.

### کارت تکراری

1. همان کارت دوباره ثبت شود.
2. HTTP 200 ولی `isSuccess=false` دریافت شود.
3. پیام «مقدار تکراری است» نمایش داده شود.
4. کارت تکراری به لیست اضافه نشود.

### تأیید و رد

1. کارت جدید Pending باشد.
2. ادمین آن را تأیید کند؛ پس از Refresh وضعیت Approved شود.
3. کارت دیگری رد شود؛ توضیح ادمین نمایش داده شود.
4. کارت ردشده در تسویه قابل انتخاب نباشد.

### حذف

1. Modal تأیید نمایش داده شود.
2. در Cancel هیچ Request ارسال نشود.
3. در Confirm فقط یک DELETE ارسال شود.
4. آیتم فقط بعد از `isSuccess=true` از لیست حذف شود.

### امنیت

1. Token کاربر A برای جزئیات و حذف کارت کاربر B آزمایش شود و باید `403` یا پاسخ ناموفق برگردد؛ در بک فعلی این تست احتمالاً شکست می‌خورد و باید قبل از Production اصلاح شود.
2. شماره کارت در Log و Analytics ظاهر نشود.
3. بعد از Logout، State کارت‌ها خالی شود.

---

## ۲۲. چک‌لیست تحویل امیرمحسن

- [ ] صفحه لیست کارت‌های بانکی ساخته شده است.
- [ ] Empty State و Loading Skeleton وجود دارد.
- [ ] فهرست بانک‌ها Dynamic از `/api/Common/BankCard` دریافت می‌شود.
- [ ] بانک از ۶ رقم اول به‌صورت Read-only تشخیص داده می‌شود.
- [ ] ارقام فارسی و عربی به انگلیسی تبدیل می‌شوند.
- [ ] شماره کارت چهارتایی نمایش و بدون خط تیره ارسال می‌شود.
- [ ] Checksum شماره کارت در فرانت کنترل می‌شود.
- [ ] شبا اختیاری و در صورت وجود با `IR` ارسال می‌شود.
- [ ] Checksum شبا در فرانت کنترل می‌شود.
- [ ] `userId` و `approved` از فرانت ارسال نمی‌شوند.
- [ ] موفقیت با `isSuccess` بررسی می‌شود.
- [ ] خطا از `messages[0].item1` نمایش داده می‌شود.
- [ ] Double Submit بسته شده است.
- [ ] بعد از هر Mutation لیست از سرور Refresh می‌شود.
- [ ] وضعیت Pending، Approved و Rejected درست نمایش داده می‌شود.
- [ ] توضیح رد ادمین نمایش داده می‌شود.
- [ ] فقط کارت Approved در انتخاب تسویه دیده می‌شود.
- [ ] شماره کارت و شبا در لیست Mask شده‌اند.
- [ ] اطلاعات کامل بانکی در Storage، Console و Analytics ذخیره نمی‌شوند.
- [ ] حذف دارای Modal تأیید است.
- [ ] خطای 401 و 403 مدیریت شده است.
- [ ] همه سناریوهای بخش تست اجرا شده‌اند.
- [ ] ایرادهای امنیتی بخش ۲۰ برای اصلاح بک پیگیری شده‌اند.
