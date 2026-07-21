# راهنمای فرانت سایت؛ ثبت لوکیشن کاربر و نمایش Companionهای اطراف

هدف این قابلیت این است که کاربر لوکیشن فعلی خود را از مرورگر ثبت کند و سپس Companionهای نزدیک را روی نقشه یا به شکل لیست ببیند. تشخیص شهر و محله کاملاً در بک‌اند انجام می‌شود.

## جریان پیشنهادی صفحه

1. کاربر روی دکمه «استفاده از موقعیت فعلی من» کلیک می‌کند.
2. فرانت از Browser Geolocation اجازه دسترسی می‌گیرد.
3. فقط نقطه جغرافیایی به بک‌اند ارسال می‌شود.
4. بک‌اند شهر و محله را با محدوده‌های ثبت‌شده پیدا و لوکیشن کاربر را ذخیره می‌کند.
5. فرانت endpoint مربوط به Companionهای اطراف را فراخوانی می‌کند.
6. نتیجه هم در لیست و هم با marker روی نقشه نمایش داده می‌شود.

تمام endpointهای این مستند نیازمند ورود کاربر و هدر زیر هستند:

```http
Authorization: Bearer {token}
```

## قرارداد مختصات

در تمام درخواست‌ها و پاسخ‌ها:

| فیلد | معنی |
|---|---|
| `x` | Longitude یا طول جغرافیایی |
| `y` | Latitude یا عرض جغرافیایی |

در Browser Geolocation تبدیل به شکل زیر انجام شود:

```ts
const point = {
  x: position.coords.longitude,
  y: position.coords.latitude
};
```

نمونه نقطه تست در تهران:

```json
{
  "x": 51.389,
  "y": 35.6892
}
```

مختصات را جابه‌جا نکنید؛ `latitude` باید در `y` و `longitude` باید در `x` قرار بگیرد.

## ثبت یا به‌روزرسانی لوکیشن کاربر

```http
POST /api/EndUser/UserCurrentLocation
Content-Type: application/json
```

Body:

```json
{
  "location": {
    "x": 51.389,
    "y": 35.6892
  }
}
```

فرانت نباید هیچ‌کدام از موارد زیر را ارسال کند:

- `userId`
- `cityId`
- `neighborhoodId`
- `lastUpdateDate`

بک‌اند کاربر را از توکن شناسایی می‌کند، نقطه را با SRID 4326 ذخیره می‌کند و City و Neighborhood را از روی مرزهای جغرافیایی پیدا می‌کند.

این عملیات Upsert است؛ یعنی هر کاربر فقط یک Current Location دارد و ثبت بعدی همان رکورد را به‌روزرسانی می‌کند.

### رفتار تشخیص محدوده

- ابتدا محله‌ای که نقطه داخل آن است پیدا می‌شود و City نیز از همان محله تعیین می‌شود.
- اگر محله پیدا نشود، محدوده City بررسی می‌شود.
- اگر Point داخل مرز شهر ثبت‌شده نباشد، نزدیک‌ترین محله بررسی می‌شود؛ فقط وقتی فاصله حداکثر ۲ کیلومتر باشد City ثبت می‌شود و Neighborhood خالی می‌ماند.
- اگر هیچ محدوده قابل قبولی پیدا نشود، عملیات ناموفق است و لوکیشن قبلی کاربر overwrite نمی‌شود.

### نمونه فراخوانی مرورگر

```ts
navigator.geolocation.getCurrentPosition(
  async (position) => {
    await api.post('/api/EndUser/UserCurrentLocation', {
      location: {
        x: position.coords.longitude,
        y: position.coords.latitude
      }
    });
  },
  (error) => {
    // نمایش پیام مناسب برای عدم دسترسی، timeout یا unavailable بودن GPS
  },
  {
    enableHighAccuracy: true,
    timeout: 15000,
    maximumAge: 60000
  }
);
```

بهتر است درخواست permission فقط بعد از کلیک مستقیم کاربر اجرا شود، نه هنگام load اولیه صفحه.

## دریافت Current User و لوکیشن ذخیره‌شده

```http
GET /api/EndUser/CurrentUser
```

در `data` فیلد زیر وجود دارد:

```json
{
  "userCurrentLocation": {
    "id": 18,
    "userId": 123,
    "location": {
      "x": 51.389,
      "y": 35.6892
    },
    "cityId": 87,
    "neighborhoodId": 14,
    "lastUpdateDate": "2026-07-21T18:28:07Z",
    "city": {
      "id": 87,
      "name": "تهران"
    },
    "neighborhood": {
      "id": 14,
      "name": "..."
    }
  }
}
```

اگر کاربر هنوز لوکیشن خود را ثبت نکرده باشد، `userCurrentLocation` برابر `null` است. در این حالت CTA ثبت لوکیشن نمایش داده شود و endpoint Nearby هنوز فراخوانی نشود.

## دریافت Companionهای اطراف

```http
GET /api/EndUser/Companion/Nearby
```

مثال:

```http
GET /api/EndUser/Companion/Nearby?radiusMeter=20000&pageIndex=1&pageSize=50&onlyInServiceArea=false
```

مرکز جستجو از لوکیشن ذخیره‌شده کاربر خوانده می‌شود؛ Point مرکز نباید در query ارسال شود.

### پارامترهای قابل استفاده

| پارامتر | نوع | پیش‌فرض | توضیح |
|---|---|---:|---|
| `radiusMeter` | number | `10000` | شعاع جستجو به متر؛ بیشتر از صفر و حداکثر ۵۰٬۰۰۰ |
| `pageIndex` | number | `1` | شماره صفحه |
| `pageSize` | number | `50` | تعداد هر صفحه؛ حداکثر ۱۰۰ |
| `onlyInServiceArea` | boolean | `false` | فقط Companionهایی که محدوده کاربر را پوشش می‌دهند |
| `q` | string | خالی | جستجو در نام و SearchKey |
| `typeId` | number | خالی | فیلتر نوع Companion |
| `petId` | number | خالی | فیلتر حیوان قابل پذیرش |
| `assistanceId` | number | خالی | فیلتر خدمت فعال و تأییدشده |

به دلیل ارث‌بری DTO ممکن است `sortBy` یا `available` نیز در Swagger دیده شوند، اما در endpoint Nearby فعلاً استفاده نمی‌شوند و فرانت نیازی به ارسال آن‌ها ندارد.

### شرایط ورود Companion به نتیجه

Companion باید:

- حذف نشده باشد؛
- فعال و تأییدشده باشد؛
- لوکیشن داشته باشد؛
- داخل شعاع انتخابی کاربر باشد.

### نمونه پاسخ

```json
{
  "data": {
    "radiusMeter": 20000,
    "centerLocation": {
      "x": 51.389,
      "y": 35.6892
    },
    "cityId": 87,
    "neighborhoodId": 14,
    "pageIndex": 1,
    "pageSize": 50,
    "totalCount": 2,
    "list": [
      {
        "id": 25,
        "name": "کلینیک نمونه",
        "addressValue": "تهران، ...",
        "phone": "...",
        "cityId": 87,
        "neighborhoodId": 14,
        "cityName": "تهران",
        "neighborhoodName": "...",
        "pictureId": 10,
        "location": {
          "x": 51.401,
          "y": 35.701
        },
        "picture": {},
        "rateAvg": 4.7,
        "rateCount": 32,
        "isGold": true,
        "isSilver": false,
        "hasPansion": false,
        "distanceMeter": 1850.4,
        "hasServiceZone": true,
        "isInServiceArea": true
      }
    ]
  },
  "code": 0,
  "isSuccess": true,
  "messages": []
}
```

## معنی وضعیت محدوده خدمات

| `hasServiceZone` | `isInServiceArea` | معنی در UI |
|---:|---:|---|
| `true` | `true` | Companion محدوده فعلی کاربر را پوشش می‌دهد |
| `true` | `false` | نزدیک است، ولی محدوده فعلی کاربر را پوشش نمی‌دهد |
| `false` | `null` | Companion هنوز محدوده خدمات تعریف نکرده است |

اگر Neighborhood کاربر `null` باشد، فقط Zone از نوع «کل شهر» match می‌شود و Zone محله‌ای قابل تطبیق نیست.

وقتی `onlyInServiceArea=true` باشد، فقط آیتم‌هایی با `isInServiceArea=true` برمی‌گردند؛ Companionهای بدون Zone نیز از نتیجه حذف می‌شوند.

### ترتیب نتایج

بک‌اند نتایج را به ترتیب زیر مرتب می‌کند:

1. Companionهایی که محدوده کاربر را پوشش می‌دهند
2. Companionهای نزدیک که هنوز Zone ندارند
3. Companionهای نزدیک که Zone دارند ولی محدوده کاربر را پوشش نمی‌دهند
4. داخل هر گروه، فاصله کمتر در اولویت است

فرانت لازم نیست این مرتب‌سازی را دوباره انجام دهد.

## نمایش روی نقشه و لیست

برای marker هر Companion:

```ts
const longitude = companion.location.x;
const latitude = companion.location.y;
```

پیشنهاد نمایش هر card:

- تصویر و نام
- فاصله تبدیل‌شده به متر یا کیلومتر
- امتیاز و تعداد رأی
- شهر، محله و آدرس
- badge «در محدوده خدمات شما» برای `isInServiceArea === true`
- badge خنثی «محدوده خدمات اعلام نشده» برای `isInServiceArea === null`
- پیام هشدار ملایم برای `isInServiceArea === false`

نمونه فرمت فاصله:

```ts
function formatDistance(meter: number) {
  return meter < 1000
    ? `${Math.round(meter)} متر`
    : `${(meter / 1000).toFixed(1)} کیلومتر`;
}
```

در نسخه فعلی، جستجو همیشه حول لوکیشن ذخیره‌شده کاربر انجام می‌شود. جابه‌جایی دستی نقشه، مرکز جستجوی API را تغییر نمی‌دهد؛ برای تغییر مرکز باید لوکیشن جدید کاربر ثبت شود.

## مدیریت وضعیت‌ها و خطاها

- اگر کاربر login نیست، ابتدا مسیر ورود اجرا شود.
- اگر permission لوکیشن رد شد، روش فعال‌سازی دسترسی در مرورگر و امکان تلاش مجدد نمایش داده شود.
- اگر GPS timeout شد یا unavailable بود، پیام مناسب و دکمه تلاش مجدد نمایش داده شود.
- اگر ثبت لوکیشن `isSuccess: false` بود، Nearby فراخوانی نشود و پیام `messages` نمایش داده شود.
- اگر Current Location وجود ندارد، CTA ثبت لوکیشن نمایش داده شود.
- اگر `totalCount` صفر است، empty state متناسب با شعاع و فیلترها نمایش داده شود.
- هنگام تغییر شعاع یا فیلتر، `pageIndex` به ۱ برگردد.
- هنگام درخواست جدید، درخواست قبلی لغو یا نتیجه قدیمی نادیده گرفته شود تا race condition ایجاد نشود.

نکته: پاسخ‌های business ممکن است HTTP 200 داشته باشند، بنابراین همیشه `isSuccess` را بررسی کنید.

## ترتیب پیشنهادی پیاده‌سازی فرانت

1. دکمه دریافت GPS و مدیریت permission/error
2. ثبت نقطه با `POST UserCurrentLocation`
3. refresh کردن `CurrentUser`
4. فراخوانی `Companion/Nearby`
5. نمایش markerها و cardهای همگام
6. فیلتر شعاع، نوع، حیوان، خدمت و محدوده خدمات
7. pagination و empty/loading/error state

## چک‌لیست تحویل سایت

- [ ] ارسال `longitude` در `x` و `latitude` در `y`
- [ ] عدم ارسال City، Neighborhood و UserId از فرانت
- [ ] بررسی `isSuccess` بعد از ثبت لوکیشن
- [ ] نمایش CTA برای کاربر بدون Current Location
- [ ] نمایش مرکز کاربر و Companionها روی نقشه
- [ ] نمایش و فرمت فاصله
- [ ] نمایش سه حالت محدوده خدمات
- [ ] پشتیبانی از شعاع حداکثر ۵۰ کیلومتر
- [ ] اعمال فیلترها و reset کردن صفحه
- [ ] مدیریت permission denied، timeout، loading و empty state

