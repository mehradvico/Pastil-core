# مستند کامل پیاده‌سازی Pastil Match برای فرانت اپ پاستیل

این سند قرارداد واقعی بک‌اند فعلی Pastil Match را برای پیاده‌سازی در `app.pastil.pet` توضیح می‌دهد. تمام نام Endpointها، DTOها، شناسه وضعیت‌ها، محدودیت‌ها و ترتیب عملیات از روی کد بک‌اند نوشته شده‌اند.

---

## ۱. ماهیت Pastil Match

Pastil Match برای پیدا کردن یک پت‌فرند مناسب است. کاربر نباید لیستی از همه پروفایل‌ها ببیند. جریان صحیح این است:

1. کاربر برای یکی از پت‌های خودش پروفایل Match می‌سازد.
2. هدف یا هدف‌های Match را مشخص می‌کند.
3. فیلترهای دلخواه را وارد می‌کند.
4. بک‌اند فقط **یک پیشنهاد برتر** با درصد تطابق برمی‌گرداند.
5. کاربر می‌تواند پیشنهاد را رد کند و پیشنهاد بعدی بگیرد یا برای همان پروفایل درخواست بفرستد.
6. طرف مقابل درخواست را قبول یا رد می‌کند.
7. در صورت قبول، رکورد Match فعال ساخته می‌شود و چت در دسترس قرار می‌گیرد.
8. هر طرف می‌تواند ارتباط را ببندد، کاربر را بلاک کند یا گزارش ثبت کند.

نمایش Grid یا لیست قابل پیمایش از تمام پروفایل‌ها، جریان موردنظر محصول نیست. صفحه جستجو باید کارت یک‌به‌یک شبیه Tinder داشته باشد.

---

## ۲. تنظیمات عمومی API

```text
API_BASE_URL=https://api.pastil.pet
FILE_BASE_URL=https://file.pastil.pet
```

تمام Endpointهای Pastil Match نیازمند لاگین هستند:

```http
Authorization: Bearer {accessToken}
Content-Type: application/json
Accept: application/json
```

نام Area در URL دقیقاً `EndUser` است:

```text
/api/EndUser/...
```

### نکته بسیار مهم درباره پاسخ‌ها

بیشتر خطاهای منطقی بک‌اند با HTTP Status برابر `200` برمی‌گردند. موفقیت عملیات فقط با `isSuccess === true` تعیین شود.

نمونه موفق:

```json
{
  "isSuccess": true,
  "messages": [],
  "code": 0,
  "data": {}
}
```

نمونه ناموفق:

```json
{
  "isSuccess": false,
  "messages": [
    {
      "item1": "متن قابل نمایش به کاربر",
      "item2": "نام فیلد یا توضیح تکمیلی"
    }
  ],
  "code": 0,
  "data": null
}
```

قانون فرانت:

```ts
const message = response?.messages?.[0]?.item1 || 'عملیات ناموفق بود'

if (response?.isSuccess !== true) {
  throw new Error(message)
}
```

پاسخ Endpointهای جستجو BaseResult نیست و مستقیماً این ساختار را دارد:

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "q": null,
  "sortBy": 1,
  "available": null,
  "totalCount": 12,
  "list": []
}
```

### Pagination و Sort مشترک

| فیلد | نوع | پیش‌فرض | توضیح |
|---|---:|---:|---|
| `pageIndex` | number | `1` | شماره صفحه از یک |
| `pageSize` | number | `20` | تعداد آیتم در صفحه |
| `q` | string/null | `null` | عبارت جستجو در Endpointهایی که پشتیبانی می‌کنند |
| `sortBy` | number | `1` | نوع مرتب‌سازی |

مقادیر پرکاربرد `sortBy`:

| مقدار | معنی |
|---:|---|
| `0` | پیش‌فرض |
| `1` | جدیدترین |
| `2` | قدیمی‌ترین |
| `3` | نام |
| `4` | بیشترین بازدید/لایک |
| `5` | کمترین بازدید/لایک |

### فهرست جامع Endpointهای EndUser

این جدول برای کنترل نهایی پیاده‌سازی است. تمام مسیرهای زیر در بک فعلی وجود دارند و همگی نیازمند Bearer Token هستند.

| بخش | Method | Endpoint | کاربرد |
|---|---|---|---|
| Profile | `GET` | `/api/EndUser/PastilMatchProfile/{id}` | جزئیات پروفایل |
| Profile | `GET` | `/api/EndUser/PastilMatchProfile` | جستجو و لیست پروفایل |
| Profile | `POST` | `/api/EndUser/PastilMatchProfile` | ساخت پروفایل |
| Profile | `PUT` | `/api/EndUser/PastilMatchProfile` | ویرایش پروفایل |
| Profile | `DELETE` | `/api/EndUser/PastilMatchProfile?id={id}` | حذف نرم پروفایل |
| Verification | `POST` | `/api/EndUser/PastilMatchProfileVerificationRequest` | درخواست بررسی و تأیید پروفایل |
| Profile Goal | `GET` | `/api/EndUser/PastilMatchProfileGoal/{id}` | جزئیات هدف ثبت‌شده |
| Profile Goal | `GET` | `/api/EndUser/PastilMatchProfileGoal` | لیست هدف‌های پروفایل |
| Profile Goal | `POST` | `/api/EndUser/PastilMatchProfileGoal` | افزودن هدف |
| Profile Goal | `PUT` | `/api/EndUser/PastilMatchProfileGoal` | ویرایش رکورد هدف |
| Profile Goal | `DELETE` | `/api/EndUser/PastilMatchProfileGoal?id={id}` | حذف هدف با ID رکورد واسط |
| Suggestion | `POST` | `/api/EndUser/PastilMatchSuggestion` | دریافت دقیقاً یک پیشنهاد برتر |
| Request | `GET` | `/api/EndUser/PastilMatchRequest/{id}` | جزئیات درخواست |
| Request | `GET` | `/api/EndUser/PastilMatchRequest` | لیست درخواست‌های مجاز کاربر |
| Request | `POST` | `/api/EndUser/PastilMatchRequest` | ارسال درخواست |
| Request | `DELETE` | `/api/EndUser/PastilMatchRequest?id={id}` | لغو درخواست Pending توسط فرستنده |
| Request Response | `PUT` | `/api/EndUser/PastilMatchRequestResponse` | قبول یا رد درخواست |
| Match | `GET` | `/api/EndUser/PastilMatch/{id}` | جزئیات Match |
| Match | `GET` | `/api/EndUser/PastilMatch` | لیست Matchها |
| Match | `DELETE` | `/api/EndUser/PastilMatch?id={id}` | بستن Match فعال |
| Message | `GET` | `/api/EndUser/PastilMatchMessage/{id}` | جزئیات پیام |
| Message | `GET` | `/api/EndUser/PastilMatchMessage` | پیام‌ها، Pagination و Sync |
| Message | `POST` | `/api/EndUser/PastilMatchMessage` | ارسال پیام |
| Message | `DELETE` | `/api/EndUser/PastilMatchMessage?id={id}` | حذف نرم پیام خود کاربر |
| Delivery | `PUT` | `/api/EndUser/PastilMatchMessageDelivered` | ثبت تحویل یک پیام |
| Read | `PUT` | `/api/EndUser/PastilMatchMessageRead` | ثبت خواندن پیام‌ها تا یک ID |
| Attachment | `GET` | `/api/EndUser/PastilMatchMessageAttachment/{id}` | جزئیات فایل پیوست |
| Attachment | `GET` | `/api/EndUser/PastilMatchMessageAttachment` | لیست فایل‌های پیوست پیام |
| Attachment | `POST` | `/api/EndUser/PastilMatchMessageAttachment` | اتصال فایل آپلودشده به پیام |
| Attachment | `DELETE` | `/api/EndUser/PastilMatchMessageAttachment?id={id}` | حذف پیوست |
| Reaction | `GET` | `/api/EndUser/PastilMatchMessageReaction/{id}` | جزئیات Reaction |
| Reaction | `GET` | `/api/EndUser/PastilMatchMessageReaction` | لیست Reactionهای پیام |
| Reaction | `POST` | `/api/EndUser/PastilMatchMessageReaction` | ثبت یا تغییر Reaction |
| Reaction | `DELETE` | `/api/EndUser/PastilMatchMessageReaction?id={id}` | حذف Reaction |
| Like | `POST` | `/api/EndUser/PastilMatchProfileLike` | لایک پروفایل |
| Like | `DELETE` | `/api/EndUser/PastilMatchProfileLike?id={id}` | حذف لایک |
| Block | `GET` | `/api/EndUser/PastilMatchBlock/{id}` | جزئیات Block |
| Block | `GET` | `/api/EndUser/PastilMatchBlock` | لیست کاربران بلاک‌شده |
| Block | `POST` | `/api/EndUser/PastilMatchBlock` | بلاک کاربر |
| Block | `DELETE` | `/api/EndUser/PastilMatchBlock?id={id}` | آنبلاک |
| Report Reason | `GET` | `/api/EndUser/PastilMatchReportReason/{id}` | جزئیات دلیل گزارش |
| Report Reason | `GET` | `/api/EndUser/PastilMatchReportReason` | لیست دلایل فعال |
| Report | `GET` | `/api/EndUser/PastilMatchReport/{id}` | جزئیات گزارش متعلق به کاربر |
| Report | `GET` | `/api/EndUser/PastilMatchReport` | لیست گزارش‌های کاربر |
| Report | `POST` | `/api/EndUser/PastilMatchReport` | ثبت گزارش |

Endpoint عمومی EndUser برای فعال/غیرفعال‌کردن یا تأیید نهایی پروفایل وجود ندارد؛ این دو عملیات مدیریتی هستند. همچنین برای Edit و Pin پیام Controller عمومی وجود ندارد و فرانت نباید برایشان URL حدس بزند.

---

## ۳. Codeها و شناسه‌های ثابت فعلی

فرانت برای نمایش متن باید از `name` و برای منطق پایدار ترجیحاً از `label` استفاده کند. IDهای زیر قرارداد فعلی بک‌اند هستند، ولی متن فارسی نباید در UI هاردکد شود اگر Codeها از API مرکزی پروژه دریافت می‌شوند.

### هدف Match ـ CodeGroup Label: `PastilMatchGoal`

| ID | Label | عنوان فعلی |
|---:|---|---|
| `106` | `PastilMatchGoal_Walking` | پیاده‌روی |
| `107` | `PastilMatchGoal_Playing` | بازی |
| `108` | `PastilMatchGoal_Friendship` | دوستی |
| `109` | `PastilMatchGoal_ParkMeetup` | قرار در پارک |

### وضعیت درخواست ـ CodeGroup Label: `PastilMatchRequestStatus`

| ID | Label | معنی |
|---:|---|---|
| `110` | `PastilMatchRequestStatus_Pending` | در انتظار پاسخ |
| `111` | `PastilMatchRequestStatus_Accepted` | قبول‌شده |
| `112` | `PastilMatchRequestStatus_Rejected` | ردشده |
| `113` | `PastilMatchRequestStatus_Cancelled` | لغوشده توسط فرستنده |

### وضعیت Match ـ CodeGroup Label: `PastilMatchStatus`

| ID | Label | معنی |
|---:|---|---|
| `114` | `PastilMatchStatus_Active` | فعال و قابل چت |
| `115` | `PastilMatchStatus_Closed` | بسته‌شده |
| `116` | `PastilMatchStatus_Blocked` | بسته‌شده به‌علت بلاک |

### نوع پیام ـ CodeGroup Label: `PastilMatchMessageType`

| ID | Label | معنی |
|---:|---|---|
| `117` | `PastilMatchMessageType_Text` | متن |
| `118` | `PastilMatchMessageType_Image` | تصویر |
| `119` | `PastilMatchMessageType_Voice` | صدای ضبط‌شده |
| `120` | `PastilMatchMessageType_System` | پیام سیستمی؛ فرانت اجازه ساخت ندارد |

### سطح انرژی ـ CodeGroup Label: `EnergyLevel`

| ID | Label | عنوان |
|---:|---|---|
| `121` | `EnergyLevel_VeryLow` | خیلی کم |
| `122` | `EnergyLevel_Low` | کم |
| `123` | `EnergyLevel_Medium` | متوسط |
| `124` | `EnergyLevel_High` | زیاد |
| `125` | `EnergyLevel_VeryHigh` | خیلی زیاد |

### سطح اجتماعی ـ CodeGroup Label: `SocialLevel`

| ID | Label | عنوان |
|---:|---|---|
| `126` | `SocialLevel_VeryLow` | خیلی کم |
| `127` | `SocialLevel_Low` | کم |
| `128` | `SocialLevel_Medium` | متوسط |
| `129` | `SocialLevel_High` | زیاد |
| `130` | `SocialLevel_VeryHigh` | خیلی زیاد |

---

## ۴. آماده‌سازی اطلاعات پایه فرم

### پت‌های کاربر

```http
GET /api/EndUser/UserPet?pageIndex=1&pageSize=100
Authorization: Bearer {token}
```

این Endpoint فقط پت‌های فعال کاربر جاری را برمی‌گرداند. `userPetId` پروفایل Match باید از همین لیست انتخاب شود.

### شهرها

```http
GET /api/Common/City?pageIndex=1&pageSize=50
```

### محله‌ها

```http
GET /api/Neighborhood?cityId={cityId}&pageIndex=1&pageSize=1000
```

### مختصات

در تمام DTOهای مکانی:

```json
{
  "x": 51.389,
  "y": 35.721
}
```

- `x` = Longitude یا طول جغرافیایی
- `y` = Latitude یا عرض جغرافیایی
- مختصات با SRID 4326 ذخیره می‌شوند.
- جای `x` و `y` عوض نشود.

---

## ۵. ساخت و مدیریت پروفایل Match

هر پت فعال فقط یک پروفایل حذف‌نشده Match می‌تواند داشته باشد. تصویر و مشخصات پایه پت از `UserPet` می‌آید؛ PastilMatchProfile تصویر جداگانه ندارد.

### ساخت پروفایل

```http
POST /api/EndUser/PastilMatchProfile
Authorization: Bearer {token}
Content-Type: application/json
```

```json
{
  "userPetId": 12,
  "energyLevelId": 123,
  "socialLevelId": 128,
  "liveLocation": {
    "x": 51.389,
    "y": 35.721
  },
  "cityId": 1,
  "neighborhoodId": 25,
  "description": "آرام، بازیگوش و دوست‌دار پیاده‌روی"
}
```

فیلدهای قابل ارسال:

| فیلد | نوع | اجباری | توضیح |
|---|---|---|---|
| `userPetId` | number | بله | پت فعال متعلق به کاربر جاری |
| `energyLevelId` | number | بله | یکی از `121..125` |
| `socialLevelId` | number | بله | یکی از `126..130` |
| `liveLocation` | object/null | پیشنهاد می‌شود | برای محاسبه فاصله |
| `cityId` | number/null | خیر | شهر پروفایل |
| `neighborhoodId` | number/null | خیر | محله پروفایل |
| `description` | string/null | خیر | معرفی پت |

بک‌اند هنگام ساخت این مقادیر را خودش تعیین می‌کند و فرانت نباید روی مقدار ارسالی آن‌ها حساب کند:

- `likeCount = 0`
- `isActive = true`
- `isVerified = false`
- `lastActiveDate = now`
- `createDate = now`

اگر پروفایل حذف شود، ساخت مجدد همان پت تا ۲۴ ساعت مجاز نیست.

### دریافت پروفایل‌های خود کاربر

راه پیشنهادی این است که ابتدا پت‌های کاربر دریافت شوند و برای هر پت پروفایل جستجو شود:

```http
GET /api/EndUser/PastilMatchProfile?userPetId=12&pageIndex=1&pageSize=20
```

Endpoint جستجو فقط پروفایل‌های فعال را برمی‌گرداند؛ Controller مقدار `available=true` را اجباری می‌کند.

### جزئیات پروفایل

```http
GET /api/EndUser/PastilMatchProfile/{profileId}
Authorization: Bearer {token}
```

نمونه ساختار اصلی پاسخ:

```json
{
  "isSuccess": true,
  "messages": [],
  "data": {
    "id": 41,
    "userPetId": 12,
    "energyLevelId": 123,
    "socialLevelId": 128,
    "likeCount": 4,
    "liveLocation": { "x": 51.389, "y": 35.721 },
    "cityId": 1,
    "neighborhoodId": 25,
    "description": "آرام و بازیگوش",
    "isActive": true,
    "isVerified": false,
    "adminDescription": null,
    "verificationDate": null,
    "lastActiveDate": "2026-08-16T12:30:00+03:30",
    "createDate": "2026-08-10T18:00:00+03:30",
    "userPet": {},
    "city": {},
    "neighborhood": {},
    "pastilMatchProfileGoals": []
  }
}
```

`userPet` شامل نام، جنسیت، تاریخ تولد، نوع پت، نژاد اول و دوم و `picture` است. URL تصویر از اطلاعات `picture` ساخته شود:

```ts
const imageUrl = picture?.baseUrl
  ? `${picture.baseUrl}${picture.url}/${picture.guidName}-md${picture.extension}`
  : `${FILE_BASE_URL}${picture.url}/${picture.guidName}-md${picture.extension}`
```

در صورت نبود Thumbnail از فایل اصلی استفاده شود:

```ts
`${FILE_BASE_URL}${picture.url}/${picture.guidName}${picture.extension}`
```

### ویرایش پروفایل

```http
PUT /api/EndUser/PastilMatchProfile
```

```json
{
  "id": 41,
  "userPetId": 12,
  "energyLevelId": 124,
  "socialLevelId": 128,
  "liveLocation": { "x": 51.401, "y": 35.735 },
  "cityId": 1,
  "neighborhoodId": 31,
  "description": "متن جدید"
}
```

بک‌اند در ویرایش اجازه تغییر این فیلدها را به EndUser نمی‌دهد و مقدار قبلی را نگه می‌دارد:

- `userPetId`
- `likeCount`
- `isActive`
- `isVerified`
- `adminDescription`
- `verificationDate`
- `createDate`

### حذف پروفایل

```http
DELETE /api/EndUser/PastilMatchProfile?id={profileId}
```

حذف Soft Delete است. بعد از حذف، ساخت مجدد برای همان پت ۲۴ ساعت Cooldown دارد.

### وضعیت تأیید پروفایل

| مقدار `isVerified` | معنی UI |
|---|---|
| `false` | هنوز درخواست تأیید ارسال نشده یا قبلاً رد شده |
| `null` | درخواست برای بررسی ادمین در انتظار است |
| `true` | تأییدشده |

ارسال درخواست تأیید:

```http
POST /api/EndUser/PastilMatchProfileVerificationRequest
```

```json
{
  "pastilMatchProfileId": 41
}
```

فیلد بالا قرارداد اصلی فرانت است. بک برای سازگاری با نسخه‌های قدیمی، بدنه
`{ "id": 41 }` را نیز می‌پذیرد. مقدار باید ID خود `PastilMatchProfile` باشد،
نه `userPetId`.

در حالت `null` دکمه درخواست مجدد غیرفعال شود. در حالت ردشده، `adminDescription` برای مالک پروفایل قابل نمایش است. توضیح ادمین برای سایر کاربران از پاسخ حذف می‌شود.

---

## ۶. اهداف پروفایل

بعد از ساخت پروفایل، حداقل یک هدف باید ثبت شود؛ در غیر این صورت پیشنهادگیری انجام نمی‌شود.

### دریافت هدف‌های پروفایل

```http
GET /api/EndUser/PastilMatchProfileGoal?pastilMatchProfileId=41&pageIndex=1&pageSize=20
```

### افزودن هدف

```http
POST /api/EndUser/PastilMatchProfileGoal
```

```json
{
  "pastilMatchProfileId": 41,
  "pastilMatchGoalId": 108
}
```

### حذف هدف

در پاسخ لیست، ID رکورد واسط را نگه دارید؛ Delete با ID هدف Code انجام نمی‌شود.

```http
DELETE /api/EndUser/PastilMatchProfileGoal?id={profileGoalRecordId}
```

یک هدف تکراری برای یک پروفایل پذیرفته نمی‌شود.

---

## ۷. موتور پیشنهاد یک‌به‌یک

Endpoint اصلی صفحه جستجوی پت‌فرند:

```http
POST /api/EndUser/PastilMatchSuggestion
Authorization: Bearer {token}
Content-Type: application/json
```

### Request کامل

```json
{
  "sourceProfileId": 41,
  "excludedProfileIds": [],
  "requiredGoalIds": [108],
  "petBreedIds": [],
  "energyLevelIds": [122, 123, 124],
  "socialLevelIds": [127, 128, 129],
  "maxDistanceInKilometers": 20,
  "minAgeInMonths": 6,
  "maxAgeInMonths": 96,
  "cityId": 1,
  "neighborhoodId": null,
  "isMale": null,
  "isSterile": null,
  "verifiedOnly": false,
  "samePetTypeOnly": true,
  "minimumCompatibilityPercent": 0
}
```

### معنی فیلترها

| فیلد | نوع | توضیح |
|---|---|---|
| `sourceProfileId` | number | اجباری؛ پروفایل متعلق به کاربر جاری |
| `excludedProfileIds` | number[] | پروفایل‌هایی که در همین Session قبلاً نمایش داده شده‌اند |
| `requiredGoalIds` | number[] | هدف‌های اجباری؛ باید قبلاً برای پروفایل مبدا ثبت شده باشند |
| `petBreedIds` | number[] | نژادهای مجاز؛ نژاد اول یا دوم کاندید بررسی می‌شود |
| `energyLevelIds` | number[] | سطوح انرژی مجاز |
| `socialLevelIds` | number[] | سطوح اجتماعی مجاز |
| `maxDistanceInKilometers` | number/null | حداکثر فاصله؛ اگر فعال باشد هر دو پروفایل باید مختصات داشته باشند |
| `minAgeInMonths` | number/null | حداقل سن کاندید به ماه |
| `maxAgeInMonths` | number/null | حداکثر سن کاندید به ماه |
| `cityId` | number/null | فیلتر شهر |
| `neighborhoodId` | number/null | فیلتر محله |
| `isMale` | boolean/null | جنسیت پت کاندید |
| `isSterile` | boolean/null | وضعیت عقیم‌بودن |
| `verifiedOnly` | boolean | فقط پروفایل‌های تأییدشده |
| `samePetTypeOnly` | boolean | پیش‌فرض بک‌اند `true`؛ مثلاً سگ فقط با سگ |
| `minimumCompatibilityPercent` | number/null | بازه مجاز `0..100` |

فیلترهای خالی باید `[]` یا `null` باشند، نه رشته خالی.

### پاسخ دارای پیشنهاد

```json
{
  "isSuccess": true,
  "messages": [],
  "data": {
    "found": true,
    "message": null,
    "sourceProfileId": 41,
    "candidateProfileId": 87,
    "compatibilityPercent": 83,
    "recommendedGoalId": 108,
    "distanceInKilometers": 3.42,
    "ageDifferenceInMonths": 5,
    "score": {
      "goalsPercent": 100,
      "distancePercent": 93,
      "agePercent": 95,
      "breedPercent": 100,
      "energyPercent": 75,
      "socialPercent": 100
    },
    "profile": {},
    "excludedProfileIds": [12, 18, 41, 87]
  }
}
```

### پاسخ بدون پیشنهاد

نبود پیشنهاد خطا نیست:

```json
{
  "isSuccess": true,
  "messages": [],
  "data": {
    "found": false,
    "message": "برای پت شما یا متناسب با درخواست شما دوستی پیدا نشد.",
    "sourceProfileId": 41,
    "candidateProfileId": null,
    "compatibilityPercent": null,
    "profile": null,
    "excludedProfileIds": [12, 18, 41]
  }
}
```

در `found=false` Empty State نمایش داده شود و UI نباید آن را Toast خطا تلقی کند.

### مدیریت ExcludedProfileIds

بعد از هر پاسخ، آرایه برگشتی `data.excludedProfileIds` را جایگزین آرایه قبلی کنید:

```ts
excludedProfileIds.value = response.data.excludedProfileIds ?? []
```

برای «بعدی» همان فیلترها و همین آرایه را دوباره ارسال کنید. با تغییر اساسی فیلتر یا انتخاب پت دیگر، آرایه را Reset کنید.

بک‌اند علاوه بر آرایه فرانت، موارد زیر را خودکار حذف می‌کند:

- تمام پروفایل‌های متعلق به همان کاربر
- کاربران بلاک‌شده در هر دو جهت
- پروفایل‌هایی که درخواست Pending با آن‌ها وجود دارد
- پروفایل‌هایی که قبلاً با آن‌ها Match ساخته شده است
- پروفایل غیرفعال یا حذف‌شده
- پت غیرفعال یا حذف‌شده
- پروفایلی که هیچ هدف مشترکی ندارد

### ترتیب انتخاب بهترین گزینه

1. بالاترین درصد تطابق
2. در درصد برابر، فاصله کمتر
3. سپس `lastActiveDate` جدیدتر
4. سپس ID کمتر

### فرمول درصد تطابق

| عامل | وزن |
|---|---:|
| هدف‌های مشترک | ۲۵٪ |
| فاصله | ۲۵٪ |
| نزدیکی سن | ۲۰٪ |
| نژاد | ۱۵٪ |
| سطح انرژی | ۷.۵٪ |
| سطح اجتماعی | ۷.۵٪ |

- شباهت هدف‌ها با Jaccard محاسبه می‌شود.
- امتیاز فاصله در محدوده مبنای ۵۰ کیلومتر از ۱۰۰ به صفر کاهش می‌یابد.
- اختلاف سن در بازه مبنای ۹۶ ماه از ۱۰۰ به صفر کاهش می‌یابد.
- نژاد مشترک امتیاز بالاتری دارد؛ اگر نژاد ثبت نشده باشد، نوع یکسان پت امتیاز جزئی می‌گیرد.
- اگر تاریخ تولد یا مختصات موجود نباشد، وزن همان عامل از مخرج حذف می‌شود؛ مقدار مربوط در `score` برابر `null` است.
- درصد می‌تواند کمتر از ۳۰ هم باشد، مگر فرانت `minimumCompatibilityPercent` بالاتری ارسال کند.

---

## ۸. ارسال و مدیریت درخواست Match

### ارسال درخواست

پس از نمایش پیشنهاد، از `candidateProfileId` و `recommendedGoalId` استفاده شود:

```http
POST /api/EndUser/PastilMatchRequest
```

```json
{
  "senderProfileId": 41,
  "receiverProfileId": 87,
  "pastilMatchGoalId": 108,
  "description": "اگر دوست داشتی آخر هفته برای پیاده‌روی هماهنگ کنیم."
}
```

فرانت این فیلدها را نفرستد یا نتیجه‌شان را تعیین‌شده فرض نکند:

- `statusId`: بک‌اند همیشه `110` می‌گذارد.
- `compatibilityPercent`: بک‌اند دوباره محاسبه می‌کند.
- `createDate`, `responseDate`, `cancelDate`: توسط بک‌اند تعیین می‌شوند.

قوانین بک‌اند:

- فرستنده باید مالک `senderProfileId` باشد.
- هر دو پروفایل و هر دو پت باید فعال و حذف‌نشده باشند.
- درخواست به پروفایل خود یا یکی دیگر از پت‌های همان کاربر ممنوع است.
- هدف باید یکی از هدف‌های هر دو پروفایل باشد.
- بین دو پروفایل درخواست Pending تکراری در هیچ جهتی مجاز نیست.
- Match فعال تکراری مجاز نیست.
- کاربران بلاک‌شده نمی‌توانند درخواست بفرستند.

### لیست درخواست‌های دریافتی

```http
GET /api/EndUser/PastilMatchRequest?receiverProfileId=87&statusId=110&pageIndex=1&pageSize=20&sortBy=1
```

### لیست درخواست‌های ارسالی

```http
GET /api/EndUser/PastilMatchRequest?senderProfileId=41&pageIndex=1&pageSize=20&sortBy=1
```

کاربر فقط درخواست‌هایی را می‌بیند که یکی از پروفایل‌های خودش فرستنده یا گیرنده آن است.

### جزئیات درخواست

```http
GET /api/EndUser/PastilMatchRequest/{requestId}
```

### قبول درخواست

فقط مالک پروفایل گیرنده اجازه پاسخ دارد:

```http
PUT /api/EndUser/PastilMatchRequestResponse
```

```json
{
  "id": 55,
  "statusId": 111
}
```

با قبول درخواست، بک‌اند هم‌زمان یک Match با وضعیت `114` می‌سازد.

### رد درخواست

```json
{
  "id": 55,
  "statusId": 112
}
```

فقط `111` و `112` برای پاسخ مجازند.

### لغو درخواست توسط فرستنده

```http
DELETE /api/EndUser/PastilMatchRequest?id={requestId}
```

فقط درخواست Pending و فقط توسط فرستنده قابل لغو است. وضعیت به `113` تغییر می‌کند.

---

## ۹. Matchهای فعال و بسته‌شده

### لیست Matchهای یک پروفایل

```http
GET /api/EndUser/PastilMatch?pastilMatchProfileId=41&pageIndex=1&pageSize=20&sortBy=1
```

برای لیست چت فعال:

```http
GET /api/EndUser/PastilMatch?pastilMatchProfileId=41&statusId=114&pageIndex=1&pageSize=50
```

پاسخ هر Match شامل این موارد است:

- `pastilMatchRequestId`
- `firstProfileId`
- `secondProfileId`
- `pastilMatchGoalId`
- `statusId`
- `compatibilityPercent`
- `createDate`
- `closeDate`
- `firstProfile`
- `secondProfile`
- `pastilMatchGoal`
- `status`

برای نمایش طرف مقابل:

```ts
const otherProfile = match.firstProfileId === myProfileId
  ? match.secondProfile
  : match.firstProfile
```

### جزئیات Match

```http
GET /api/EndUser/PastilMatch/{matchId}
```

### بستن Match

```http
DELETE /api/EndUser/PastilMatch?id={matchId}
```

هرکدام از دو طرف می‌تواند Match فعال را ببندد. وضعیت به `115` تغییر می‌کند و دیگر ارسال پیام مجاز نیست.

---

## ۱۰. چت Pastil Match

در پیاده‌سازی فعلی Endpoint اختصاصی SignalR برای Pastil Match وجود ندارد. چت REST است؛ فرانت باید:

- در ورود صفحه پیام‌ها را Fetch کند.
- هنگام بازبودن صفحه با Polling کنترل‌شده، مثلاً هر ۵ تا ۱۰ ثانیه، پیام جدید بگیرد.
- در Resume شدن PWA یا برگشت Tab دوباره Sync کند.
- Push Notification را برای ورود کاربر به لیست چت استفاده کند.
- از `afterMessageId` برای دریافت پیام‌های جدید و از `beforeMessageId` برای صفحه‌بندی عقب استفاده کند.

### دریافت پیام‌های اولیه

```http
GET /api/EndUser/PastilMatchMessage?pastilMatchId=73&pageIndex=1&pageSize=30&sortBy=2
```

`sortBy=2` پیام‌ها را قدیمی به جدید می‌دهد و برای Render اولیه مناسب است.

### دریافت پیام‌های جدید بعد از آخرین پیام

```http
GET /api/EndUser/PastilMatchMessage?pastilMatchId=73&afterMessageId=940&pageIndex=1&pageSize=100&sortBy=2
```

### دریافت صفحه قبلی

```http
GET /api/EndUser/PastilMatchMessage?pastilMatchId=73&beforeMessageId=820&pageIndex=1&pageSize=30&sortBy=1
```

در حالت `sortBy=1` نتیجه جدید به قدیم است؛ برای prepend کردن، ترتیب آرایه را در فرانت کنترل کنید.

### مدل پیام دریافتی

```json
{
  "id": 940,
  "pastilMatchId": 73,
  "senderProfileId": 41,
  "pastilMatchMessageTypeId": 117,
  "replyToMessageId": null,
  "content": "سلام، برای فردا وقت دارید؟",
  "isEdited": false,
  "editDate": null,
  "isPinned": false,
  "pinDate": null,
  "deliveredDate": null,
  "readDate": null,
  "createDate": "2026-08-16T15:30:00+03:30",
  "pastilMatchMessageType": {},
  "replyToMessage": null,
  "attachments": [],
  "reactions": []
}
```

پیام سیستمی `senderProfileId=null` دارد.

### ارسال پیام متنی

```http
POST /api/EndUser/PastilMatchMessage
```

```json
{
  "pastilMatchId": 73,
  "senderProfileId": 41,
  "pastilMatchMessageTypeId": 117,
  "replyToMessageId": null,
  "content": "سلام، برای فردا وقت دارید؟"
}
```

برای Text، `content` اجباری است. `senderProfileId` باید پروفایل متعلق به کاربر جاری و یکی از دو پروفایل همان Match باشد.

### Reply

```json
{
  "pastilMatchId": 73,
  "senderProfileId": 41,
  "pastilMatchMessageTypeId": 117,
  "replyToMessageId": 939,
  "content": "بله، ساعت ۶ مناسب است."
}
```

پیام Reply باید حذف‌نشده و متعلق به همان Match باشد.

### ارسال تصویر

ترتیب فعلی:

1. تصویر روی File Service آپلود شود.
2. یک پیام نوع Image ساخته شود.
3. با ID پیام، Attachment ثبت شود.
4. پیام از سرور دوباره Fetch شود تا Attachment کامل در مدل پیام قرار گیرد.

آپلود:

```http
POST https://file.pastil.pet/api/PictureUpload
Content-Type: multipart/form-data
```

نام فیلد فایل:

```text
PictureFile
```

محدودیت تصویر:

- حداکثر ۵ مگابایت
- `jpg`, `jpeg`, `png`, `webp`

ساخت پیام:

```json
{
  "pastilMatchId": 73,
  "senderProfileId": 41,
  "pastilMatchMessageTypeId": 118,
  "replyToMessageId": null,
  "content": ""
}
```

ثبت Attachment:

```http
POST /api/EndUser/PastilMatchMessageAttachment
```

```json
{
  "pastilMatchMessageId": 941,
  "url": "https://file.pastil.pet/Media/2026/8/16/example.webp",
  "thumbnailUrl": "https://file.pastil.pet/Media/2026/8/16/example-sm.webp",
  "fileName": "photo.webp",
  "contentType": "image/webp",
  "fileSize": 245810,
  "duration": null,
  "width": 1280,
  "height": 720,
  "order": 0
}
```

`url` در Attachment رشته URL است و `pictureId` نیست.

### ارسال Voice

آپلود فایل صوتی:

```http
POST https://file.pastil.pet/api/FileUpload
Content-Type: multipart/form-data
```

نام فیلد:

```text
file
```

حداکثر حجم FileUpload برابر ۲۰ مگابایت است.

ساخت پیام Voice:

```json
{
  "pastilMatchId": 73,
  "senderProfileId": 41,
  "pastilMatchMessageTypeId": 119,
  "replyToMessageId": null,
  "content": ""
}
```

ثبت Attachment صوتی:

```json
{
  "pastilMatchMessageId": 942,
  "url": "https://file.pastil.pet/StaticFile/2026/8/16/voice.webm",
  "thumbnailUrl": null,
  "fileName": "voice.webm",
  "contentType": "audio/webm",
  "fileSize": 180430,
  "duration": 14,
  "width": null,
  "height": null,
  "order": 0
}
```

قوانین Attachment:

- فقط پیام Image یا Voice Attachment می‌پذیرد.
- Content-Type تصویر باید با `image/` شروع شود.
- Content-Type صدا باید با `audio/` شروع شود.
- برای Voice مقدار `duration` مثبت اجباری است.
- هر پیام Voice فقط یک Attachment فعال دارد.
- `fileSize` باید مثبت و `order` صفر یا بیشتر باشد.
- URL تکراری در یک پیام پذیرفته نمی‌شود.
- اگر آخرین Attachment یک پیام حذف شود، خود پیام هم حذف‌شده محسوب می‌شود.

### تحویل پیام

فقط گیرنده پیام می‌تواند Delivered را ثبت کند:

```http
PUT /api/EndUser/PastilMatchMessageDelivered
```

```json
{
  "id": 940
}
```

این عملیات Idempotent است؛ اجرای دوباره تاریخ قبلی را تغییر نمی‌دهد.

### خوانده‌شدن پیام‌ها

وقتی صفحه چت دیده شد، تمام پیام‌های طرف مقابل تا آخرین ID دیده‌شده را Read کنید:

```http
PUT /api/EndUser/PastilMatchMessageRead
```

```json
{
  "pastilMatchId": 73,
  "lastMessageId": 940
}
```

بک‌اند هم‌زمان `deliveredDate` خالی را هم مقداردهی می‌کند.

### ری‌اکشن پیام

```http
POST /api/EndUser/PastilMatchMessageReaction
```

```json
{
  "pastilMatchMessageId": 940,
  "reaction": "❤️"
}
```

- هر پروفایل روی هر پیام فقط یک Reaction فعال دارد.
- POST مجدد با مقدار جدید همان Reaction را تغییر می‌دهد.
- طول مقدار حداکثر ۳۲ کاراکتر است.
- `reactorProfileId` را فرانت ارسال نمی‌کند؛ بک‌اند از توکن و Match تشخیص می‌دهد.

حذف Reaction با ID رکورد Reaction:

```http
DELETE /api/EndUser/PastilMatchMessageReaction?id={reactionRecordId}
```

### حذف پیام

```http
DELETE /api/EndUser/PastilMatchMessage?id={messageId}
```

EndUser فقط پیام خودش را می‌تواند حذف کند. حذف Soft Delete است.

### Edit و Pin

سرویس بک‌اند متدهای داخلی Edit و Pin دارد، اما در حال حاضر Controller عمومی EndUser برای آن‌ها وجود ندارد. تا قبل از اضافه‌شدن Endpoint رسمی، دکمه Edit و Pin در اپ نمایش داده نشود و Endpoint فرضی ساخته نشود.

---

## ۱۱. لایک پروفایل

### ثبت لایک

```http
POST /api/EndUser/PastilMatchProfileLike
```

```json
{
  "likerProfileId": 41,
  "likedProfileId": 87
}
```

- لایک به پروفایل خود ممنوع است.
- `likerProfileId` باید متعلق به کاربر جاری باشد.
- لایک تکراری موفق و Idempotent است.
- تعداد `likeCount` پروفایل مقصد به‌روزرسانی می‌شود.

برای حذف Like باید ID رکورد Like برگشتی از POST نگهداری شود:

```http
DELETE /api/EndUser/PastilMatchProfileLike?id={likeRecordId}
```

---

## ۱۲. بلاک کاربر

Block بر اساس User انجام می‌شود، نه Profile. `blockedUserId` را از `candidateProfile.userPet.userId` یا پروفایل طرف Match بردارید.

```http
POST /api/EndUser/PastilMatchBlock
```

```json
{
  "blockedUserId": 91,
  "pastilMatchId": 73
}
```

`pastilMatchId` اختیاری است، ولی اگر از صفحه چت بلاک انجام می‌شود ارسال آن توصیه می‌شود.

اثر بلاک:

- تمام Matchهای فعال بین دو کاربر به وضعیت `116` می‌روند.
- دو کاربر دیگر به یکدیگر پیشنهاد نمی‌شوند.
- ارسال درخواست بین آن‌ها ممنوع می‌شود.
- بلاک خود کاربر ممکن نیست.

لیست کاربران بلاک‌شده:

```http
GET /api/EndUser/PastilMatchBlock?pageIndex=1&pageSize=50
```

آنبلاک با ID رکورد Block:

```http
DELETE /api/EndUser/PastilMatchBlock?id={blockRecordId}
```

آنبلاک، Match قبلی را دوباره فعال نمی‌کند.

---

## ۱۳. گزارش تخلف

### دریافت دلایل فعال گزارش

```http
GET /api/EndUser/PastilMatchReportReason?pageIndex=1&pageSize=100&sortBy=0
```

هر دلیل شامل:

- `id`
- `title`
- `description`
- `priority`
- `isDescriptionRequired`
- `active`

اگر `isDescriptionRequired=true` بود، Textarea توضیح در UI اجباری شود.

### گزارش پروفایل یا کاربر

```http
POST /api/EndUser/PastilMatchReport
```

```json
{
  "reportedUserId": 91,
  "reportedProfileId": 87,
  "pastilMatchId": null,
  "pastilMatchMessageId": null,
  "pastilMatchReportReasonId": 3,
  "description": "توضیح کاربر"
}
```

### گزارش از داخل Match

```json
{
  "reportedUserId": 91,
  "reportedProfileId": 87,
  "pastilMatchId": 73,
  "pastilMatchMessageId": null,
  "pastilMatchReportReasonId": 3,
  "description": "توضیح کاربر"
}
```

### گزارش یک پیام

```json
{
  "reportedUserId": 91,
  "reportedProfileId": 87,
  "pastilMatchId": 73,
  "pastilMatchMessageId": 940,
  "pastilMatchReportReasonId": 4,
  "description": "این پیام نامناسب بود."
}
```

در گزارش پیام:

- پیام باید متعلق به همان Match باشد.
- گزارش‌دهنده باید عضو Match باشد.
- `reportedUserId` باید مالک واقعی پیام باشد.
- پیام سیستمی گزارش نشود چون فرستنده کاربری ندارد.

گزارش تخلف خود کاربر ممنوع است.

---

## ۱۴. Push Notification و Deep Link

Pushهای فعلی Pastil Match:

| رویداد | مقصد | Route پیشنهادی/ثبت‌شده |
|---|---|---|
| دریافت درخواست | گیرنده درخواست | `/pastil-match/requests` |
| قبول درخواست | فرستنده درخواست | `/pastil-match/chats` |
| رد درخواست | فرستنده درخواست | `/pastil-match/requests` |
| لغو درخواست | گیرنده درخواست | `/pastil-match/requests` |
| پیام جدید | طرف مقابل | `/pastil-match/chats` |
| Reaction جدید | فرستنده پیام | `/pastil-match/chats` |
| لایک پروفایل | مالک پروفایل مقصد | `/pastil-match/profile` |
| بسته‌شدن Match | طرف مقابل | `/pastil-match` |
| تأیید پروفایل | مالک پروفایل | `/pastil-match/profile` |
| رد تأیید پروفایل | مالک پروفایل | `/pastil-match/profile` |

پس از کلیک Push، فرانت باید ابتدا Session را Restore کند و سپس Route را باز کند. اگر شناسه دقیق Match در Payload موجود نبود، لیست مربوط Fetch شود و جدیدترین رکورد نمایش داده شود.

هنگام دریافت Push پیام، اگر همان چت باز است:

1. با `afterMessageId` Sync شود.
2. آخرین پیام دریافتی Delivered شود.
3. اگر صفحه Visible است، پیام‌ها تا آخرین ID Read شوند.

---

## ۱۵. State Management پیشنهادی فرانت

```ts
type PastilMatchSearchSession = {
  sourceProfileId: number
  filters: {
    requiredGoalIds: number[]
    petBreedIds: number[]
    energyLevelIds: number[]
    socialLevelIds: number[]
    maxDistanceInKilometers: number | null
    minAgeInMonths: number | null
    maxAgeInMonths: number | null
    cityId: number | null
    neighborhoodId: number | null
    isMale: boolean | null
    isSterile: boolean | null
    verifiedOnly: boolean
    samePetTypeOnly: boolean
    minimumCompatibilityPercent: number
  }
  excludedProfileIds: number[]
  currentSuggestion: PastilMatchSuggestion | null
}
```

قواعد State:

- `excludedProfileIds` فقط مربوط به Session همان پروفایل و همان فیلتر است.
- با Logout، تغییر پت یا Reset فیلتر پاک شود.
- بعد از ارسال درخواست موفق، پیشنهاد فعلی پاک و پیشنهاد بعدی دریافت شود.
- درخواست POST با Double Click دوباره ارسال نشود؛ دکمه هنگام Pending غیرفعال باشد.
- پاسخ `found=false` در State نگه داشته شود تا Loop بی‌نهایت ساخته نشود.

---

## ۱۶. صفحات لازم در اپ

### صفحه شروع Pastil Match

- انتخاب پت
- نمایش وضعیت وجود یا نبود پروفایل Match
- ورود به ساخت/ویرایش پروفایل
- ورود به جستجوی دوست
- دسترسی به درخواست‌ها و چت‌ها

### فرم پروفایل Match

- انتخاب پت از UserPetهای فعال
- سطح انرژی Dynamic
- سطح اجتماعی Dynamic
- اهداف چندانتخابی
- شهر و محله Dynamic
- انتخاب Location روی نقشه با رضایت کاربر
- توضیحات
- وضعیت تأیید و توضیح رد ادمین

### صفحه جستجوی یک‌به‌یک

- یک کارت پیشنهاد
- تصویر پت، نام، سن، جنسیت، نژاد، توضیح
- درصد کل تطابق
- Breakdown امتیازها
- فاصله در صورت وجود
- هدف پیشنهادی
- دکمه «بعدی»
- دکمه «ارسال درخواست»
- Like، Report و Block
- Empty State برای `found=false`

### صفحه درخواست‌ها

- Tab درخواست‌های دریافتی
- Tab درخواست‌های ارسالی
- Badge وضعیت بر اساس `status.label`
- قبول و رد فقط برای Pending دریافتی
- لغو فقط برای Pending ارسالی

### صفحه چت‌ها

- فقط Matchهای `statusId=114` در بخش فعال
- Matchهای Closed/Blocked در آرشیو Read-only
- نمایش Profile طرف مقابل
- درصد تطابق و هدف Match
- آخرین پیام با Polling

### صفحه چت

- متن، تصویر، Voice، Reply و Reaction
- Delivered و Read
- Infinite Scroll به عقب
- Polling پیام‌های جدید با `afterMessageId`
- بستن Match، Block و Report

---

## ۱۷. خطاها و رفتار UI

| وضعیت | رفتار فرانت |
|---|---|
| HTTP `401` | Refresh Token؛ در صورت شکست انتقال به Login |
| HTTP `403` یا پیام AccessDenied | نمایش عدم دسترسی و برگشت از صفحه |
| HTTP `200` و `isSuccess=false` | نمایش `messages[0].item1`؛ State موفق نشود |
| `data=null` در جزئیات | صفحه Not Found یا برگشت امن |
| `found=false` در Suggestion | Empty State؛ نه Toast خطا |
| Timeout/Network Error | Retry کنترل‌شده بدون ثبت دوباره عملیات POST |
| Match غیر فعال | Composer چت غیرفعال و گفتگو Read-only شود |
| درخواست دیگر Pending نیست | لیست درخواست Refresh شود |

هر POST/PUT/DELETE تا پایان پاسخ باید Loading مستقل داشته باشد و Double Submit بسته شود.

---

## ۱۸. نکات امنیتی الزامی فرانت

- هیچ UserId، ProfileId یا درصد تطابق از LocalStorage به‌عنوان حقیقت معتبر فرض نشود.
- UserId از پاسخ CurrentUser و ProfileId از API گرفته شود.
- فرانت نباید `statusId` درخواست را هنگام ساخت تعیین کند.
- فرانت نباید `compatibilityPercent` را هنگام ارسال درخواست معتبر بداند؛ بک‌اند آن را دوباره محاسبه می‌کند.
- HTML خام در Description و Message Render نشود؛ خروجی Escape شود.
- URL Attachment قبل از نمایش فقط به پروتکل‌های مجاز `https:` محدود شود.
- برای Voice و Image نوع MIME و حجم قبل از Upload در UI بررسی شود.
- مختصات دقیق کاربر فقط با رضایت او گرفته شود؛ در صورت نبود Location، فیلتر فاصله فعال نشود.
- Access Token در Query String قرار نگیرد.

---

## ۱۹. سناریوی End-to-End پیشنهادی برای تست فرانت

1. کاربر A و B هرکدام یک پت فعال داشته باشند.
2. برای هر پت PastilMatchProfile ساخته شود.
3. برای هر دو پروفایل هدف `108` ثبت شود.
4. A با `PastilMatchSuggestion` جستجو کند و B را ببیند.
5. A یک Like ثبت کند.
6. A برای B درخواست بفرستد.
7. B درخواست Pending را در لیست دریافتی ببیند.
8. B درخواست را با Status `111` قبول کند.
9. A در لیست Matchها رکورد Active با Status `114` ببیند.
10. A پیام Text ارسال کند.
11. B پیام را با `afterMessageId` دریافت و Delivered کند.
12. B صفحه را باز کند و Read تا آخرین Message ثبت کند.
13. B روی پیام Reaction بگذارد.
14. A یک تصویر Upload، Message Image و Attachment ثبت کند.
15. B پیام تصویر را Fetch و نمایش دهد.
16. یکی از کاربران Match را ببندد و Composer هر دو طرف Read-only شود.
17. تست جداگانه Block انجام شود و Match به Status `116` برود.
18. تست Report با دلیل دارای توضیح اجباری انجام شود.

---

## ۲۰. چک‌لیست تحویل امیرمحسن

- [ ] تمام درخواست‌ها Bearer Token دارند.
- [ ] موفقیت فقط از `isSuccess` بررسی می‌شود.
- [ ] متن خطا از `messages[0].item1` نمایش داده می‌شود.
- [ ] فقط یک پیشنهاد در هر لحظه نمایش داده می‌شود.
- [ ] `excludedProfileIds` پاسخ در درخواست بعدی ارسال می‌شود.
- [ ] `found=false` به‌عنوان Empty State مدیریت شده است.
- [ ] `x=longitude` و `y=latitude` رعایت شده است.
- [ ] اهداف پروفایل قبل از جستجو ثبت شده‌اند.
- [ ] درخواست فقط با هدف مشترک ارسال می‌شود.
- [ ] قبول/رد فقط روی درخواست Pending انجام می‌شود.
- [ ] بعد از Accept، لیست Matchها Refresh می‌شود.
- [ ] چت فقط برای Match فعال Composer دارد.
- [ ] Pagination پیام با `beforeMessageId` و Sync با `afterMessageId` انجام شده است.
- [ ] Delivered و Read ثبت می‌شوند.
- [ ] Upload تصویر/صدا قبل از ثبت Attachment انجام می‌شود.
- [ ] Delete Like/Reaction/Block/Goal با ID رکورد واسط انجام می‌شود.
- [ ] Block بر اساس UserId است، نه ProfileId.
- [ ] Deep Linkهای Push پیاده‌سازی شده‌اند.
- [ ] Polling هنگام Hidden شدن صفحه متوقف یا کند می‌شود.
- [ ] Double Submit روی تمام عملیات بسته شده است.
- [ ] Edit و Pin پیام تا زمان ارائه Endpoint عمومی نمایش داده نمی‌شوند.
